// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableResizable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

#pragma warning disable 67 // Disable warnings due to unused events

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     <para>
    ///         Component that allows an object to be scaled by grabbing it by both sides and moving them closer or apart.
    ///         The hierarchy should be as follows:
    ///     </para>
    ///     <code>
    ///    -Root GameObject: With UxrGrabbableResizable and UxrGrabbableObject component.
    ///    |                 The UxrGrabbableObject is a dummy grabbable parent that enables moving
    ///    |                 this root by grabbing the child extensions. It can also have its own
    ///    |                 grab points but they are not required.
    ///    |---Root resizable:  Object that will be scaled when the two extensions are moved.
    ///    |---Grabbable left:  Left grabbable extension with locked rotation and translation
    ///    |                    constrained to sliding it left-right.
    ///    |---Grabbable right: Right grabbable extension with locked rotation and translation
    ///                         constrained to sliding it left-right.
    /// </code>
    ///     All objects should use an axis system with x right, y up and z forward.
    /// </summary>
    public sealed partial class UxrGrabbableResizable : UxrComponent, IUxrGrabbable
    {
        #region Inspector Properties/Serialized Fields

        [Header("General")] [SerializeField] private Transform _resizableRoot;
        [SerializeField]                     private float     _startScale = 1.0f;

        [Header("Grabbing")] [SerializeField] private UxrGrabbableObject _grabbableRoot;
        [SerializeField]                      private UxrGrabbableObject _grabbableExtendLeft;
        [SerializeField]                      private UxrGrabbableObject _grabbableExtendRight;

        [Header("Haptics")] [SerializeField] [Range(0.0f, 1.0f)] private float _hapticsIntensity = 0.1f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is going to be scaled when the two grabbable objects are moved apart.
        /// </summary>
        public Transform ResizableRoot => _resizableRoot;

        /// <summary>
        ///     Gets the root grabbable object.
        /// </summary>
        public UxrGrabbableObject GrabbableRoot => _grabbableRoot;

        /// <summary>
        ///     Gets the left grabbable extension.
        /// </summary>
        public UxrGrabbableObject GrabbableExtendLeft => _grabbableExtendLeft;

        /// <summary>
        ///     Gets the right grabbable extension.
        /// </summary>
        public UxrGrabbableObject GrabbableExtendRight => _grabbableExtendRight;

        #endregion

        #region Implicit IUxrGrabbable

        /// <inheritdoc />
        public bool IsBeingGrabbed => GrabbableRoot.IsBeingGrabbed || GrabbableExtendLeft.IsBeingGrabbed || GrabbableExtendRight.IsBeingGrabbed;

        /// <inheritdoc />
        public bool IsGrabbable
        {
            get => GrabbableRoot.IsGrabbable || GrabbableExtendLeft.IsGrabbable || GrabbableExtendRight.IsGrabbable;
            set
            {
                BeginSync();

                GrabbableRoot.IsGrabbable        = value;
                GrabbableExtendLeft.IsGrabbable  = value;
                GrabbableExtendRight.IsGrabbable = value;

                EndSyncProperty(value);
            }
        }

        /// <inheritdoc />
        public bool IsKinematic
        {
            get => GrabbableRoot.IsKinematic || GrabbableExtendLeft.IsKinematic || GrabbableExtendRight.IsKinematic;
            set
            {
                BeginSync();

                GrabbableRoot.IsKinematic        = value;
                GrabbableExtendLeft.IsKinematic  = value;
                GrabbableExtendRight.IsKinematic = value;

                EndSyncProperty(value);
            }
        }

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Grabbing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Grabbed;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Releasing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Released;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Placing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Placed;

        /// <inheritdoc />
        public void ResetPositionAndState(bool propagateEvents)
        {
            // This method will be synchronized through network
            BeginSync();

            ReleaseGrabs(true);
            GrabbableRoot.ResetPositionAndState(propagateEvents);
            GrabbableExtendLeft.ResetPositionAndState(propagateEvents);
            GrabbableExtendRight.ResetPositionAndState(propagateEvents);
            UpdateResizableScale();

            EndSyncMethod(new object[] { propagateEvents });
        }

        /// <inheritdoc />
        public void ReleaseGrabs(bool propagateEvents)
        {
            // This method will be synchronized through network
            BeginSync();

            GrabbableRoot.ReleaseGrabs(propagateEvents);
            GrabbableExtendLeft.ReleaseGrabs(propagateEvents);
            GrabbableExtendRight.ReleaseGrabs(propagateEvents);
            _grabbingCount = 0;
            _grabbedCount  = 0;

            EndSyncMethod(new object[] { propagateEvents });
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _initialGrabsSeparation     = Vector3.Distance(GrabbableExtendLeft.transform.position, GrabbableExtendRight.transform.position);
            _initialResizableLocalScale = _resizableRoot.transform.localScale;
            _separationToBoundsFactor   = _initialGrabsSeparation / _resizableRoot.gameObject.GetLocalBounds(true).size.x;
        }

        /// <summary>
        ///     Subscribes to relevant events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;

            GrabbableRoot.Grabbing         += Grabbable_Grabbing;
            GrabbableRoot.Grabbed          += Grabbable_Grabbed;
            GrabbableRoot.Releasing        += Grabbable_Releasing;
            GrabbableRoot.Released         += Grabbable_Released;
            GrabbableExtendLeft.Grabbing   += Grabbable_Grabbing;
            GrabbableExtendLeft.Grabbed    += Grabbable_Grabbed;
            GrabbableExtendLeft.Releasing  += Grabbable_Releasing;
            GrabbableExtendLeft.Released   += Grabbable_Released;
            GrabbableExtendRight.Grabbed   += Grabbable_Grabbed;
            GrabbableExtendRight.Grabbing  += Grabbable_Grabbing;
            GrabbableExtendRight.Releasing += Grabbable_Releasing;
            GrabbableExtendRight.Released  += Grabbable_Released;

            _hapticsCoroutine = StartCoroutine(HapticsCoroutine());
        }

        /// <summary>
        ///     Unsubscribes from relevant events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;

            GrabbableRoot.Grabbing         -= Grabbable_Grabbing;
            GrabbableRoot.Grabbed          -= Grabbable_Grabbed;
            GrabbableRoot.Releasing        -= Grabbable_Releasing;
            GrabbableRoot.Released         -= Grabbable_Released;
            GrabbableExtendLeft.Grabbing   -= Grabbable_Grabbing;
            GrabbableExtendLeft.Grabbed    -= Grabbable_Grabbed;
            GrabbableExtendLeft.Releasing  -= Grabbable_Releasing;
            GrabbableExtendLeft.Released   -= Grabbable_Released;
            GrabbableExtendRight.Grabbed   -= Grabbable_Grabbed;
            GrabbableExtendRight.Grabbing  -= Grabbable_Grabbing;
            GrabbableExtendRight.Releasing -= Grabbable_Releasing;
            GrabbableExtendRight.Released  -= Grabbable_Released;

            StopCoroutine(_hapticsCoroutine);
        }

        /// <summary>
        ///     Scales the resizable using the initial scale if it's different than 1.0
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (!Mathf.Approximately(1.0f, _startScale))
            {
                float halfOffset = (_startScale * _initialGrabsSeparation - _initialGrabsSeparation) * 0.5f;
                GrabbableExtendLeft.transform.localPosition  -= Vector3.right * halfOffset;
                GrabbableExtendRight.transform.localPosition += Vector3.right * halfOffset;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that sends haptic feedback in case of scaling.
        /// </summary>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator HapticsCoroutine()
        {
            void SendHapticClip(UxrGrabbableObject grabbableObject, UxrHandSide handSide, float speed)
            {
                if (_hapticsIntensity < 0.001f || !UxrGrabManager.Instance.GetGrabbingHand(grabbableObject, 0, out UxrGrabber grabber) || grabber.Avatar.AvatarMode != UxrAvatarMode.Local)
                {
                    return;
                }

                float quantityPos = HapticsManipulationMaxSpeed - HapticsManipulationMinSpeed <= 0.0f ? 0.0f : (speed - HapticsManipulationMinSpeed) / (HapticsManipulationMaxSpeed - HapticsManipulationMinSpeed);

                if (quantityPos > 0.0f)
                {
                    float frequencyPos = Mathf.Lerp(HapticsManipulationMinFrequency, HapticsManipulationMaxFrequency, Mathf.Clamp01(quantityPos));
                    float amplitudePos = Mathf.Lerp(0.1f, 1.0f, Mathf.Clamp01(quantityPos)) * _hapticsIntensity;

                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, frequencyPos, amplitudePos, UxrConstants.InputControllers.HapticSampleDurationSeconds);
                }
            }

            float lastDistance = Vector3.Distance(GrabbableExtendLeft.transform.position, GrabbableExtendRight.transform.position);

            while (true)
            {
                if (_grabbableExtendLeft != null && _grabbableExtendRight != null && _grabbableExtendLeft.IsBeingGrabbed && _grabbableExtendRight.IsBeingGrabbed)
                {
                    float currentDistance = Vector3.Distance(GrabbableExtendLeft.transform.position, GrabbableExtendRight.transform.position);
                    float speed           = Mathf.Abs(lastDistance - currentDistance) / UxrConstants.InputControllers.HapticSampleDurationSeconds;
                    SendHapticClip(_grabbableExtendLeft,  UxrHandSide.Left,  speed);
                    SendHapticClip(_grabbableExtendRight, UxrHandSide.Right, speed);
                    lastDistance = Vector3.Distance(GrabbableExtendLeft.transform.position, GrabbableExtendRight.transform.position);
                }

                yield return new WaitForSeconds(UxrConstants.InputControllers.HapticSampleDurationSeconds);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called right after the avatars and manipulation update. Scale the object at this point.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            UpdateResizableScale();
        }

        /// <summary>
        ///     Called when any grabbable is about to be grabbed. It is responsible for sending the appropriate
        ///     <see cref="UxrGrabbableResizable" /> manipulation events if necessary.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Grabbable_Grabbing(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsGrabbedStateChanged)
            {
                _grabbingCount++;

                if (_grabbingCount == 1)
                {
                    Grabbing?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        ///     Called right after any grabbable was grabbed. It is responsible for sending the appropriate
        ///     <see cref="UxrGrabbableResizable" /> manipulation events if necessary.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Grabbable_Grabbed(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsGrabbedStateChanged)
            {
                _grabbedCount++;

                if (_grabbedCount == 1)
                {
                    Grabbed?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        ///     Called when any grabbable is about to be released. It is responsible for sending the appropriate
        ///     <see cref="UxrGrabbableResizable" /> manipulation events if necessary.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Grabbable_Releasing(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsGrabbedStateChanged)
            {
                _grabbingCount--;

                if (_grabbingCount == 0)
                {
                    Releasing?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        ///     Called right after any grabbable was released. It is responsible for sending the appropriate
        ///     <see cref="UxrGrabbableResizable" /> manipulation events if necessary.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Grabbable_Released(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsGrabbedStateChanged)
            {
                _grabbedCount--;

                if (_grabbedCount == 0)
                {
                    Released?.Invoke(this, e);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the resizable scale based on the current separation between the left and right extensions.
        /// </summary>
        private void UpdateResizableScale()
        {
            float currentGrabSeparation = Vector3.Distance(GrabbableExtendLeft.transform.position, GrabbableExtendRight.transform.position);

            // Move the center in between the two extensions

            Vector3 localCenter = transform.InverseTransformPoint((GrabbableExtendLeft.transform.position + GrabbableExtendRight.transform.position) * 0.5f);

            Vector3 resizableLocalPos = _resizableRoot.transform.localPosition;
            resizableLocalPos.x                    = localCenter.x;
            _resizableRoot.transform.localPosition = resizableLocalPos;

            // Scale the object

            float   localScaleZ         = _resizableRoot.transform.localScale.z;
            Vector3 resizableLocalScale = _initialResizableLocalScale * (currentGrabSeparation / _initialGrabsSeparation * _separationToBoundsFactor);
            resizableLocalScale.z               = localScaleZ;
            _resizableRoot.transform.localScale = resizableLocalScale;
        }

        #endregion

        #region Private Types & Data

        private const float HapticsManipulationMinSpeed     = 0.03f;
        private const float HapticsManipulationMaxSpeed     = 1.0f;
        private const float HapticsManipulationMinFrequency = 10;
        private const float HapticsManipulationMaxFrequency = 100;

        private Vector3   _initialResizableLocalScale;
        private float     _initialGrabsSeparation;
        private float     _separationToBoundsFactor;
        private int       _grabbingCount;
        private int       _grabbedCount;
        private Coroutine _hapticsCoroutine;

        #endregion
    }
}

#pragma warning restore 67