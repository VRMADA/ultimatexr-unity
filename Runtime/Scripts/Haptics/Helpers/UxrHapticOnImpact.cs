// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticOnImpact.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UnityEngine;

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Haptics.Helpers
{
    /// <summary>
    ///     Component that plays haptic clips on the VR controllers whenever certain points hit colliders.
    ///     This enables to model haptic functionality like hitting walls with a hammer and similar.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public partial class UxrHapticOnImpact : UxrGrabbableObjectComponent<UxrHapticOnImpact>
    {
        #region Inspector Properties/Serialized Fields

        [Header("General")] [SerializeField] private List<Transform> _hitPoints;
        [SerializeField]                     private LayerMask       _collisionLayers       = ~0;
        [SerializeField] [Range(0, 180)]     private float           _forwardAngleThreshold = 30.0f;
        [SerializeField] [Range(0, 180)]     private float           _surfaceAngleThreshold = 30.0f;
        [SerializeField]                     private float           _minSpeed              = 0.01f;
        [SerializeField]                     private float           _maxSpeed              = 1.0f;

        [Header("Haptics")] [SerializeField] private UxrHapticClipType _hapticClip              = UxrHapticClipType.Shot;
        [SerializeField]                     private UxrHapticMode     _hapticMode              = UxrHapticMode.Mix;
        [SerializeField]                     private float             _hapticPulseDurationMin  = 0.05f;
        [SerializeField]                     private float             _hapticPulseDurationMax  = 0.05f;
        [SerializeField] [Range(0, 1)]       private float             _hapticPulseAmplitudeMin = 0.2f;
        [SerializeField] [Range(0, 1)]       private float             _hapticPulseAmplitudeMax = 1.0f;

        [Header("Physics")] [SerializeField] private float _minHitForce = 1.0f;
        [SerializeField]                     private float _maxHitForce = 100.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event triggered when the component detects a collision between any hit point and a collider.
        /// </summary>
        public event EventHandler<UxrHapticImpactEventArgs> Hit;

        /// <summary>
        ///     Gets the hit point transforms.
        /// </summary>
        public IEnumerable<Transform> HitPoints => _hitPoints;

        /// <summary>
        ///     Gets the total number of times something was hit.
        /// </summary>
        public int TotalHitCount { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Applies an explosive force to a rigidbody as a result of a hit.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to apply a force to</param>
        /// <param name="eventArgs">Event parameters</param>
        /// <param name="force">Explosive force applied to the rigidbody</param>
        public static void ApplyBreakExplosionForce(Rigidbody rigidbody, UxrHapticImpactEventArgs eventArgs, float force)
        {
            if (rigidbody != null)
            {
                rigidbody.AddForceAtPosition(eventArgs.Velocity.normalized * force, eventArgs.HitInfo.point);
                rigidbody.AddTorque(eventArgs.Velocity.normalized * force, ForceMode.Impulse);
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes internal data.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CreateHitPointInfo();
        }

        /// <summary>
        ///     Subscribes to events and re-initializes data.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;

            _hitPointInfos.ForEach(p =>
                                   {
                                       p.LastPos = p.HitPoint.position;

                                       for (int i = 0; i < VelocityAverageSamples; ++i)
                                       {
                                           p.VelocitySamples[i] = Vector3.zero;
                                       }
                                   });
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after avatars are updated. Tries to find objects that were hit.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (isActiveAndEnabled == false)
            {
                return;
            }

            if (Time.deltaTime > 0.0f)
            {
                foreach (HitPointInfo hitPointInfo in _hitPointInfos)
                {
                    // Update velocity frame history

                    Vector3 currentFrameVelocity = (hitPointInfo.HitPoint.position - hitPointInfo.LastPos) / Time.deltaTime;

                    for (int i = 0; i < hitPointInfo.VelocitySamples.Count - 1; ++i)
                    {
                        hitPointInfo.VelocitySamples[i] = hitPointInfo.VelocitySamples[i + 1];
                    }

                    hitPointInfo.VelocitySamples[hitPointInfo.VelocitySamples.Count - 1] = currentFrameVelocity;

                    // Average history to compute current velocity

                    hitPointInfo.Velocity = Vector3.zero;

                    for (int i = 0; i < hitPointInfo.VelocitySamples.Count - 1; ++i)
                    {
                        hitPointInfo.Velocity += hitPointInfo.VelocitySamples[i];
                    }

                    hitPointInfo.Velocity = hitPointInfo.Velocity / hitPointInfo.VelocitySamples.Count;
                }
            }

            foreach (HitPointInfo hitPointInfo in _hitPointInfos)
            {
                float speed = hitPointInfo.Velocity.magnitude;

                // Check if we are grabbing the object and moving it with enough speed
                if (UxrGrabManager.Instance && UxrGrabManager.Instance.GetGrabbingHand(GrabbableObject, out bool isLeft, out bool isRight) && speed > _minSpeed && speed > 0.0f)
                {
                    Vector3 direction = hitPointInfo.HitPoint.position - hitPointInfo.LastPos;

                    // Raycast between the previous and current frame positions

                    RaycastHit[] hits = Physics.RaycastAll(hitPointInfo.LastPos, direction, direction.magnitude, _collisionLayers, QueryTriggerInteraction.Ignore);

                    foreach (RaycastHit hitInfo in hits)
                    {
                        // Avoid self collision first
                        if (hitInfo.collider.transform.HasParent(transform))
                        {
                            continue;
                        }

                        // We hit something! get the normalized force 0 = min, 1 = max
                        float forceT = Mathf.Clamp01((speed - _minSpeed) / (_maxSpeed - _minSpeed));

                        // Compute angles (forward motion angle and surface angle)
                        float forwardVelocityAngle = Vector3.Angle(hitPointInfo.Velocity, hitPointInfo.HitPoint.forward);
                        float surfaceAngle         = Vector3.Angle(direction,             -hitInfo.normal);

                        // Below thresholds to trigger event?
                        if (forwardVelocityAngle <= _forwardAngleThreshold && surfaceAngle <= _surfaceAngleThreshold)
                        {
                            // Yes
                            UxrHapticImpactEventArgs eventArgs = new UxrHapticImpactEventArgs(hitInfo,
                                                                                              forceT,
                                                                                              hitPointInfo.Velocity,
                                                                                              forwardVelocityAngle,
                                                                                              Vector3.Angle(hitPointInfo.HitPoint.forward, -hitInfo.normal));

                            // Apply physics to the other object if it is dynamic
                            Rigidbody otherRigidbody = hitInfo.collider.GetComponent<Rigidbody>();

                            if (otherRigidbody && !otherRigidbody.isKinematic)
                            {
                                ApplyBreakExplosionForce(otherRigidbody, eventArgs, Mathf.Lerp(_minHitForce, _maxHitForce, eventArgs.ForceT));
                            }

                            // Send haptic feedback
                            if (UxrAvatar.LocalAvatarInput)
                            {
                                float amplitude = Mathf.Lerp(_hapticPulseAmplitudeMin, _hapticPulseAmplitudeMax, forceT);
                                float duration  = Mathf.Lerp(_hapticPulseDurationMin,  _hapticPulseDurationMax,  forceT);

                                if (isLeft)
                                {
                                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(UxrHandSide.Left, _hapticClip, amplitude, duration, _hapticMode);
                                }

                                if (isRight)
                                {
                                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(UxrHandSide.Right, _hapticClip, amplitude, duration, _hapticMode);
                                }

                                OnHit(eventArgs);
                            }

                            // Check if there is a receiver component to send the event
                            UxrHapticImpactReceiver receiver = hitInfo.collider.GetComponentInParent<UxrHapticImpactReceiver>();

                            if (receiver)
                            {
                                receiver.OnHit(this, eventArgs);
                            }
                        }
                    }
                }
            }

            foreach (HitPointInfo hitPointInfo in _hitPointInfos)
            {
                hitPointInfo.LastPos = hitPointInfo.HitPoint.position;
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="Hit" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnHit(UxrHapticImpactEventArgs e)
        {
            TotalHitCount++;
            Hit?.Invoke(this, e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates the hit point information list.
        /// </summary>
        private void CreateHitPointInfo()
        {
            foreach (Transform hitPoint in _hitPoints)
            {
                _hitPointInfos.Add(new HitPointInfo(hitPoint));
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     The number of frames to sample velocity to average.
        /// </summary>
        private const int VelocityAverageSamples = 3;

        private readonly List<HitPointInfo> _hitPointInfos = new List<HitPointInfo>();

        #endregion
    }
}

#pragma warning restore 414