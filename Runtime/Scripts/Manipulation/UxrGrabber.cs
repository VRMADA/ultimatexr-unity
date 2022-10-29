// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabber.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     <para>
    ///         Component that added to an <see cref="UxrAvatar" /> allows to interact with <see cref="UxrGrabbableObject" />
    ///         entities. Normally there are two per avatar, one on each hand. They are usually added to the hand object since
    ///         it is the <see cref="UxrGrabber" /> transform where grabbable objects will be snapped to when snapping is used.
    ///     </para>
    ///     <para>
    ///         By default, the grabber transform is also used to compute distances to grabbable objects. Additional proximity
    ///         transforms can be specified on the grabber so that grabbable objects can choose which one is used. This can be
    ///         useful in some scenarios: In an aircraft cockpit most knobs and buttons will prefer the distance from the tip
    ///         of the index finger, while bigger objects will prefer from the palm of the hand.
    ///     </para>
    /// </summary>
    public partial class UxrGrabber : UxrAvatarComponent<UxrGrabber>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Renderer        _handRenderer;
        [SerializeField] private GameObject[]    _objectsToDisableOnGrab;
        [SerializeField] private List<Transform> _optionalProximityTransforms;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in local-space that is pointing
        ///     to the fingers, excluding the thumb.
        /// </summary>
        public Vector3 LocalFingerDirection
        {
            get
            {
                if (Avatar == null || Avatar.AvatarRigInfo == null)
                {
                    return transform.forward;
                }

                return transform.GetClosestLocalAxis(Avatar.AvatarRigInfo.GetArmInfo(Side).HandUniversalLocalAxes.WorldForward);
            }
        }

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in world-space that is pointing
        ///     to the fingers, excluding the thumb.
        /// </summary>
        public Vector3 FingerDirection => transform.TransformDirection(LocalFingerDirection);

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in local-space that is pointing
        ///     outwards from the palm.
        /// </summary>
        public Vector3 LocalPalmOutDirection
        {
            get
            {
                if (Avatar == null || Avatar.AvatarRigInfo == null)
                {
                    return -transform.up;
                }

                return transform.GetClosestLocalAxis(-Avatar.AvatarRigInfo.GetArmInfo(Side).HandUniversalLocalAxes.WorldUp);
            }
        }

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in world-space that is pointing
        ///     outwards from the palm..
        /// </summary>
        public Vector3 PalmOutDirection => transform.TransformDirection(LocalPalmOutDirection);

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in local-space that is pointing
        ///     towards the thumb.
        /// </summary>
        public Vector3 LocalPalmThumbDirection
        {
            get
            {
                Vector3 direction = transform.right;
                
                if (Avatar != null && Avatar.AvatarRigInfo != null)
                {
                    direction = transform.GetClosestLocalAxis(Avatar.AvatarRigInfo.GetArmInfo(Side).HandUniversalLocalAxes.WorldRight);
                }

                return Side == UxrHandSide.Left ? direction : -direction;
            }
        }

        /// <summary>
        ///     Gets from all the positive and negative axes in the grabber's transform, the axis in world-space that is pointing
        ///     towards the thumb.
        /// </summary>
        public Vector3 PalmThumbDirection => transform.TransformDirection(LocalPalmThumbDirection);

        /// <summary>
        ///     <para>
        ///         Gets, based on <see cref="FingerDirection" /> and <see cref="PalmOutDirection" />, which mirroring snap
        ///         transforms
        ///         should use with the grabber if they want to be mirrored.
        ///     </para>
        ///     Snap transforms are GameObjects in <see cref="UxrGrabbableObject" /> that determine where the hand should be placed
        ///     during grabs by making the <see cref="UxrGrabber" />'s transform align with the snap <see cref="Transform" />.
        ///     Mirroring snap transforms is used to quickly create/modify grab positions/orientations.
        /// </summary>
        /// <returns>Which mirroring TransformExt.ApplyMirroring() should use</returns>
        public TransformExt.MirrorType RequiredMirrorType
        {
            get
            {
                Vector3 other = Vector3.Cross(LocalPalmOutDirection, LocalFingerDirection);

                if (Mathf.Abs(other.z) > 0.5)
                {
                    return TransformExt.MirrorType.MirrorXY;
                }
                if (Mathf.Abs(other.y) > 0.5)
                {
                    return TransformExt.MirrorType.MirrorXZ;
                }

                return TransformExt.MirrorType.MirrorYZ;
            }
        }

        /// <summary>
        ///     Gets whether the grabber component is on the left or right hand.
        /// </summary>
        public UxrHandSide OppositeSide => Side == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;

        /// <summary>
        ///     Gets whether the grabber component is on the left or right hand.
        /// </summary>
        public UxrHandSide Side
        {
            get
            {
                if (!_sideInitialized || (Application.isEditor && !Application.isPlaying))
                {
                    InitializeSide();
                }

                return _side;
            }
            private set
            {
                _side            = value;
                _sideInitialized = true;
            }
        }

        /// <summary>
        ///     Gets the avatar hand bone that corresponds to the grabber.
        /// </summary>
        public Transform HandBone { get; private set; }

        /// <summary>
        ///     Gets the relative position of the hand bone to the grabber.
        /// </summary>
        public Vector3 HandBoneRelativePos { get; private set; }

        /// <summary>
        ///     Gets the relative rotation of the hand bone to the grabber.
        /// </summary>
        public Quaternion HandBoneRelativeRot { get; private set; }

        /// <summary>
        ///     Gets or sets the hand renderer.
        /// </summary>
        public Renderer HandRenderer
        {
            get
            {
                // Try to get it automatically if it is unassigned or disabled.

                if ((_handRenderer == null || !_handRenderer.gameObject.activeInHierarchy) && Avatar != null)
                {
                    SkinnedMeshRenderer handRenderer = UxrAvatarRig.TryToGetHandRenderer(Avatar, Side);

                    if (handRenderer != null)
                    {
                        _handRenderer = handRenderer;
                    }
                }

                return _handRenderer;
            }
            set => _handRenderer = value;
        }

        /// <summary>
        ///     Gets the opposite hand grabber in the same avatar.
        /// </summary>
        public UxrGrabber OppositeHandGrabber { get; private set; }

        /// <summary>
        ///     The unprocessed grabber position. This is the position the grabber has taking only the hand controller tracking
        ///     sensor into account.
        ///     The hand position is updated by the <see cref="UxrGrabManager" /> and may be forced into a certain position if the
        ///     object being grabbed has constraints, altering also the <see cref="UxrGrabber" /> position. Sometimes it is
        ///     preferred to use the unprocessed grabber position.
        /// </summary>
        public Vector3 UnprocessedGrabberPosition { get; internal set; }

        /// <summary>
        ///     Gets the unprocessed grabber rotation. See <see cref="UnprocessedGrabberPosition" />.
        /// </summary>
        public Quaternion UnprocessedGrabberRotation { get; internal set; }

        /// <summary>
        ///     Gets the currently grabbed object if there is one. null if no object is being grabbed.
        /// </summary>
        public UxrGrabbableObject GrabbedObject
        {
            get => _grabbedObject;
            set
            {
                _grabbedObject = value;

                if (_objectsToDisableOnGrab != null)
                {
                    foreach (GameObject go in _objectsToDisableOnGrab)
                    {
                        go.SetActive(value == null);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets <see cref="UxrGrabber" />'s current frame velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets <see cref="UxrGrabber" />'s current frame angular velocity.
        /// </summary>
        public Vector3 AngularVelocity { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets <see cref="UxrGrabber" />'s velocity smoothed using averaged previous frame data.
        /// </summary>
        public Vector3 SmoothVelocity { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets <see cref="UxrGrabber" />'s angular velocity smoothed using averaged previous frame data.
        /// </summary>
        public Vector3 SmoothAngularVelocity { get; private set; } = Vector3.zero;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the given proximity transform, used to compute distances to<see cref="UxrGrabbableObject" /> entities
        /// </summary>
        /// <param name="proximityIndex">
        ///     Proximity transform index. -1 for the default (the grabber's transform) and 0 to n for any
        ///     optional proximity transform.
        /// </param>
        /// <returns>Proximity transform. If the index is out of range it will return the default transform</returns>
        public Transform GetProximityTransform(int proximityIndex = -1)
        {
            if (proximityIndex >= 0 && proximityIndex < _optionalProximityTransforms.Count)
            {
                return _optionalProximityTransforms[proximityIndex];
            }

            return transform;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Updates the hand renderer enabled state.
        /// </summary>
        internal void UpdateHandGrabberRenderer()
        {
            if (_handRenderer != null && Avatar && (Avatar.RenderMode == UxrAvatarRenderModes.Avatar || Avatar.RenderMode == UxrAvatarRenderModes.AllControllersAndAvatar))
            {
                if (GrabbedObject == null)
                {
                    _handRenderer.enabled = true;
                }
                else
                {
                    _handRenderer.enabled = !UxrGrabManager.Instance.ShouldHideHandRenderer(this, GrabbedObject);
                }
            }
        }

        /// <summary>
        ///     Updates the throw physics information.
        /// </summary>
        internal void UpdateThrowPhysicsInfo()
        {
            Transform     sampledTransform     = GrabbedObject != null ? GrabbedObject.transform : this.transform;
            Vector3       centerOfMassPosition = transform.TransformPoint(ThrowCenterOfMassLocalPosition);
            Vector3       throwTipPosition     = transform.TransformPoint(ThrowTipLocalPosition);
            PhysicsSample newSample            = new PhysicsSample(_physicsSampleWindow.LastOrDefault(), sampledTransform, centerOfMassPosition, throwTipPosition, Time.deltaTime);

            // Update timers
            _physicsSampleWindow.ForEach(s => s.Age += Time.deltaTime);

            // Remove samples out of the time window
            _physicsSampleWindow.RemoveAll(s => s.Age > SampleWindowSeconds);

            // Add new sample
            _physicsSampleWindow.Add(newSample);

            // Compute instant and smoothed values:
            Velocity              = newSample.Velocity;
            AngularVelocity       = newSample.EulerSpeed;
            SmoothVelocity        = Vector3Ext.Average(_physicsSampleWindow.Select(s => s.TotalVelocity));

            Quaternion relative = Quaternion.Inverse(_physicsSampleWindow.First().Rotation) * _physicsSampleWindow.Last().Rotation;
            relative.ToAngleAxis(out float angle, out Vector3 axis);
            
            SmoothAngularVelocity = (angle * sampledTransform.TransformDirection(axis)) / _physicsSampleWindow.First().Age;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrAvatarRig.GetHandSide(transform, out UxrHandSide handSide);
            Side = handSide;

            if (Avatar != null)
            {
                // Compute hand bone info

                HandBone            = Avatar.GetHandBone(Side);
                HandBoneRelativePos = HandBone != null ? transform.InverseTransformPoint(HandBone.position) : Vector3.zero;
                HandBoneRelativeRot = HandBone != null ? Quaternion.Inverse(transform.rotation) * HandBone.rotation : Quaternion.identity;

                // Compute grabber info

                UxrGrabber[] avatarGrabbers = Avatar.GetComponentsInChildren<UxrGrabber>();

                OppositeHandGrabber = avatarGrabbers.FirstOrDefault(g => g != null && Side != g.Side);
                GrabbedObject       = null;

                // Compute throw physics info

                if (Avatar.GetHand(handSide).GetPalmCenter(out Vector3 palmCenter) && Avatar.GetHand(handSide).GetPalmToFingerDirection(out Vector3 palmToFinger))
                {
                    ThrowCenterOfMassLocalPosition = transform.InverseTransformPoint(palmCenter);
                    ThrowTipLocalPosition          = transform.InverseTransformPoint(palmCenter + palmToFinger * ThrowAxisLength);
                }
            }
        }

        /// <summary>
        ///     Called when the object is destroyed. Releases any grabbed objects.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (GrabbedObject != null)
            {
                UxrGrabManager.Instance.ReleaseObject(this, GrabbedObject, true);
            }
        }

        /// <summary>
        ///     Called when the object is disabled. Releases any grabbed objects.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (GrabbedObject != null)
            {
                UxrGrabManager.Instance.ReleaseObject(this, GrabbedObject, true);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Assigns the hand the grabber belongs to.
        /// </summary>
        private void InitializeSide()
        {
            if (UxrAvatarRig.GetHandSide(transform, out UxrHandSide handSide))
            {
                Side = handSide;
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the throw center of mass (palm center) in the grabber's local coordinate system.
        /// </summary>
        private Vector3 ThrowCenterOfMassLocalPosition { get; set; } = Vector3.zero;

        /// <summary>
        ///     Gets the throw center of mass (palm center) in the grabber local coordinate system.
        /// </summary>
        private Vector3 ThrowTipLocalPosition { get; set; } = Vector3.zero;

        /// <summary>
        ///     Distance from the center of mass (palm) to the fingers to compute throw angular speed.
        /// </summary>
        private const float ThrowAxisLength = 0.1f;

        /// <summary>
        ///     History physics sample window in seconds.
        /// </summary>
        private const float SampleWindowSeconds = 0.15f;

        private readonly List<PhysicsSample> _physicsSampleWindow = new List<PhysicsSample>();

        private bool               _sideInitialized;
        private UxrHandSide        _side;
        private UxrGrabbableObject _grabbedObject;

        #endregion
    }
}