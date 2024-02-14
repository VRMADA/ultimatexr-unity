// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAutoSlideInObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Core.Math;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
using UltimateXR.Haptics.Helpers;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Component that, together with <see cref="UxrAutoSlideInAnchor" /> will add the following behaviour to a
    ///     <see cref="UxrGrabbableObject" />:
    ///     <list type="bullet">
    ///         <item>
    ///             It will slide along the axis given by the grabbable object translation constraint. The constraint should
    ///             be pre-configured along a single axis.
    ///         </item>
    ///         <item>
    ///             It will be smoothly removed from the anchor and made free if dragged beyond the upper translation
    ///             constraint.
    ///         </item>
    ///         <item>It will be smoothly placed automatically on the anchor when moved back close enough.</item>
    ///         <item>It will fall back by itself when released while sliding along the axis.</item>
    ///     </list>
    /// </summary>
    public partial class UxrAutoSlideInObject : UxrGrabbableObjectComponent<UxrAutoSlideInObject>
    {
        [SerializeField] private Vector3 _translationConstraintMin = Vector3.zero;
        [SerializeField] private Vector3 _translationConstraintMax = Vector3.forward * 0.1f;
                    
        #region Public Types & Data

        /// <summary>
        ///     Event called right after the object hit the end after sliding in after it was released.
        /// </summary>
        public event Action PlacedAfterSlidingIn;

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to the avatars updated event so that the manipulation logic is done after all manipulation
        ///     logic has been updated.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from the avatars updated event.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Initialize component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _placedAfterSlidingIn = GrabbableObject.CurrentAnchor != null;

            // Get slide axis
            int insertAxis = GrabbableObject.SingleTranslationAxisIndex;

            if (insertAxis == -1 || GrabbableObject.TranslationConstraint != UxrTranslationConstraintMode.RestrictLocalOffset)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.ManipulationModule} {this}: {nameof(UxrGrabbableObject)} component needs to have a local offset translation constraint along a single axis to work properly");
                }

                _insertAxis = UxrAxis.Z;
            }
            else
            {
                _insertAxis = insertAxis;
            }

            // Store haptic feedback component in case it exists, to disable it while the object is out of the sliding zone
            _manipulationHapticFeedback = GetComponent<UxrManipulationHapticFeedback>();

            // Compute the slide length
            _insertOffset = _translationConstraintMax[_insertAxis] > -_translationConstraintMin[_insertAxis] ? _translationConstraintMax[_insertAxis] : _translationConstraintMin[_insertAxis];

            _insertOffsetSign = Mathf.Sign(_insertOffset);
            _insertOffset     = Mathf.Abs(_insertOffset);

            // Fix some object parameters if we need to

            if (GrabbableObject.DropSnapMode != UxrSnapToAnchorMode.DontSnap)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {this.GetPathUnderScene()}: GrabbableObject needs DropSnapMode to be DontSnap in order to work properly. Overriding.");
                }

                GrabbableObject.DropSnapMode = UxrSnapToAnchorMode.DontSnap;
            }

            GrabbableObject.IsPlaceable = false; // We will handle placement ourselves

            // Compute the object size in local coordinates
            _objectLocalSize = gameObject.GetLocalBounds(true).size;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after UltimateXR has done all the frame updating. Does the manipulation logic.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            bool grabbedByLocalAvatar = UxrGrabManager.Instance.IsBeingGrabbed(GrabbableObject) && UxrGrabManager.Instance.GetGrabbingHands(GrabbableObject).First().Avatar.AvatarMode == UxrAvatarMode.Local;

            if (GrabbableObject.CurrentAnchor == null && grabbedByLocalAvatar)
            {
                // The object is being grabbed and is detached. Check if we need to place it on an anchor again by proximity.

                foreach (UxrAutoSlideInAnchor anchor in UxrAutoSlideInAnchor.EnabledComponents.Where(a => a.Anchor.enabled))
                {
                    // If it is inside the valid release "volume", place it in the anchor again and let it slide by re-assigning the constraints

                    if (anchor.Anchor.CurrentPlacedObject == null && anchor.Anchor.IsCompatibleObject(GrabbableObject) && IsObjectNearPlacement(anchor.Anchor))
                    {
                        AttachObject(anchor);
                        return;
                    }
                }
            }

            if (GrabbableObject.CurrentAnchor != null && _insertAxis != null)
            {
                // Object can only move in a specific axis but if it is grabbed past this distance it becomes free

                if (transform.parent != null && grabbedByLocalAvatar && Mathf.Abs(GrabbableObject.InitialLocalPosition[_insertAxis] - transform.localPosition[_insertAxis]) > _insertOffset * 0.99f)
                {
                    DetachObject();
                    return;
                }

                // If it is not being grabbed it will slide in

                if (!GrabbableObject.IsBeingGrabbed)
                {
                    // Use simple gravity to slide in. Gravity will be mapped to z axis to slide in our local coordinate system.

                    Vector3 speed = Physics.gravity * _slideInTimer;
                    Vector3 pos   = GrabbableObject.transform.localPosition;
                    pos[_insertAxis] += Time.deltaTime * speed.y * _insertOffsetSign;

                    if ((_insertOffsetSign > 0.0f && pos[_insertAxis] < GrabbableObject.InitialLocalPosition[_insertAxis]) ||
                        (_insertOffsetSign < 0.0f && pos[_insertAxis] > GrabbableObject.InitialLocalPosition[_insertAxis]))
                    {
                        pos[_insertAxis] = GrabbableObject.InitialLocalPosition[_insertAxis];

                        if (_placedAfterSlidingIn == false)
                        {
                            _placedAfterSlidingIn = true;
                            OnPlacedAfterSlidingIn();
                        }
                    }

                    // Interpolate other rotation/translation in case the object was released before the transition

                    float smooth = 0.1f;

                    pos[_insertAxis.Perpendicular]          = UxrInterpolator.SmoothDamp(pos[_insertAxis.Perpendicular],      GrabbableObject.InitialLocalPosition[_insertAxis.Perpendicular],      smooth);
                    pos[_insertAxis.OtherPerpendicular]     = UxrInterpolator.SmoothDamp(pos[_insertAxis.OtherPerpendicular], GrabbableObject.InitialLocalPosition[_insertAxis.OtherPerpendicular], smooth);
                    GrabbableObject.transform.localRotation = UxrInterpolator.SmoothDampRotation(GrabbableObject.transform.localRotation, GrabbableObject.InitialLocalRotation, smooth);

                    // Update

                    GrabbableObject.transform.localPosition = pos;

                    _slideInTimer += Time.deltaTime;
                }
                else
                {
                    _slideInTimer = 0.0f;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the object was placed at the end sliding in after it was released.
        ///     Use in child classes to
        /// </summary>
        protected virtual void OnPlacedAfterSlidingIn()
        {
            PlacedAfterSlidingIn?.Invoke();
        }

        /// <summary>
        ///     Called right after the object was grabbed.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            if (!GrabbableObject.IsLockedInPlace)
            {
                _placedAfterSlidingIn = false;
            }
        }

        /// <summary>
        ///     Called right after the object was released.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            if (e.GrabbableObject.CurrentAnchor != null && e.GrabbableObject.RigidBodySource)
            {
                // Force kinematic while released, so that we update the position/rotation.
                e.GrabbableObject.RigidBodySource.isKinematic = true;
            }
        }

        #endregion

        #region Protected Overrides UxrGrabbableObjectComponent<UxrAutoSlideInObject>

        /// <inheritdoc />
        protected override bool IsGrabbableObjectRequired => true;

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Attaches the object to the anchor and assigns constraints to let it slide.
        /// </summary>
        protected void AttachObject(UxrAutoSlideInAnchor anchor)
        {
            // This method will be synchronized through network
            BeginSync();

            // Set up constraints and place

            GrabbableObject.TranslationConstraint = UxrTranslationConstraintMode.RestrictLocalOffset;
            GrabbableObject.RotationConstraint    = UxrRotationConstraintMode.Locked;

            GrabbableObject.TranslationLimitsMin = _translationConstraintMin;
            GrabbableObject.TranslationLimitsMax = _translationConstraintMax;

            UxrGrabManager.Instance.PlaceObject(GrabbableObject, anchor.Anchor, UxrPlacementOptions.Smooth | UxrPlacementOptions.DontRelease, true);

            if (_manipulationHapticFeedback)
            {
                _manipulationHapticFeedback.MinAmplitude = _minHapticAmplitude;
                _manipulationHapticFeedback.MaxAmplitude = _maxHapticAmplitude;
            }

            EndSyncMethod(new object[] { anchor });
        }

        /// <summary>
        ///     Detaches the object from the anchor so that it becomes free.
        /// </summary>
        protected void DetachObject()
        {
            // This method will be synchronized through network
            BeginSync();

            if (_manipulationHapticFeedback)
            {
                _minHapticAmplitude                      = _manipulationHapticFeedback.MinAmplitude;
                _maxHapticAmplitude                      = _manipulationHapticFeedback.MaxAmplitude;
                _manipulationHapticFeedback.MinAmplitude = 0.0f;
                _manipulationHapticFeedback.MaxAmplitude = 0.0f;
            }

            UxrGrabManager.Instance.RemoveObjectFromAnchor(GrabbableObject, true);
            GrabbableObject.TranslationConstraint = UxrTranslationConstraintMode.Free;
            GrabbableObject.RotationConstraint    = UxrRotationConstraintMode.Free;

            EndSyncMethod();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the object is close enough to the given anchor to be placed.
        /// </summary>
        /// <param name="anchor">Object anchor</param>
        /// <returns>Whether the object is close enough</returns>
        private bool IsObjectNearPlacement(UxrGrabbableObjectAnchor anchor)
        {
            if (anchor.enabled == false)
            {
                return false;
            }

            // Is it near enough in the longitudinal axis?

            float threshold = Mathf.Min(0.03f, Mathf.Abs(_insertOffset * 0.1f));

            Vector3 localOffset = anchor.AlignTransform.InverseTransformPoint(GrabbableObject.DropAlignTransform.position) - GrabbableObject.InitialLocalPosition;
            bool isInLongitudinalAxisRange = (_insertOffsetSign > 0.0f && localOffset[_insertAxis] < +_insertOffset - threshold && localOffset[_insertAxis] > 0.0f) ||
                                             (_insertOffsetSign < 0.0f && localOffset[_insertAxis] > -_insertOffset + threshold && localOffset[_insertAxis] < 0.0f);

            // Is it near enough in both other axes?

            float minGrabDistance = float.MaxValue;

            foreach (UxrGrabber grabber in UxrGrabManager.Instance.GetGrabbingHands(GrabbableObject))
            {
                UxrGrabPointInfo grabPointInfo = GrabbableObject.GetGrabPoint(UxrGrabManager.Instance.GetGrabbedPoint(grabber));

                if (grabPointInfo.MaxDistanceGrab < minGrabDistance)
                {
                    minGrabDistance = grabPointInfo.MaxDistanceGrab;
                }
            }

            // We use some calculations for the other axes so that it feels good.

            float sizeOneAxis   = Mathf.Min(Mathf.Max(_objectLocalSize[_insertAxis.Perpendicular],      0.1f), minGrabDistance);
            float sizeOtherAxis = Mathf.Min(Mathf.Max(_objectLocalSize[_insertAxis.OtherPerpendicular], 0.1f), minGrabDistance);

            // Return conditions

            return isInLongitudinalAxisRange && Mathf.Abs(localOffset[_insertAxis.Perpendicular]) < sizeOneAxis && Mathf.Abs(localOffset[_insertAxis.OtherPerpendicular]) < sizeOtherAxis;
        }

        #endregion

        #region Private Types & Data

        private UxrAxis                       _insertAxis;
        private float                         _insertOffset;
        private float                         _insertOffsetSign;
        private Vector3                       _objectLocalSize;
        private float                         _slideInTimer;
        private bool                          _placedAfterSlidingIn;
        private UxrManipulationHapticFeedback _manipulationHapticFeedback;
        private float                         _minHapticAmplitude;
        private float                         _maxHapticAmplitude;

        #endregion
    }
}