// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Battery.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Haptics.Helpers;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Component that adds the smooth slide-in/slide-out effects to the grabbable battery.
    /// </summary>
    public class Battery : UxrGrabbableObjectComponent<Battery>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _batteryDiameter;
        [SerializeField] private float _batteryInsertOffset;

        #endregion

        #region Unity

        /// <summary>
        ///     Initialize component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _placed                     = GrabbableObject.CurrentAnchor != null;
            _batteryOffsetConstraint    = GrabbableObject.TranslationLimitsMax;
            _manipulationHapticFeedback = GetComponent<UxrManipulationHapticFeedback>();
        }


        /// <summary>
        ///     Subscribes to the avatars updated event so that the manipulation logic is done after UltimateXR's manipulation
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

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after UltimateXR has done all the frame updating. Does the manipulation logic.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (GrabbableObject.CurrentAnchor == null && UxrGrabManager.Instance.IsBeingGrabbed(GrabbableObject))
            {
                // The battery is being grabbed and is detached. Check if we need to place it an anchor again by proximity.

                foreach (BatteryAnchor batteryAnchor in BatteryAnchor.EnabledComponents)
                {
                    // If it is inside our valid volume, place it in the door again

                    if (batteryAnchor.Anchor.CurrentPlacedObject == null && IsBatteryNearPlacement(batteryAnchor.Anchor))
                    {
                        // Add constraints and place

                        GrabbableObject.TranslationConstraint = UxrTranslationConstraintMode.RestrictLocalOffset;
                        GrabbableObject.RotationConstraint    = UxrRotationConstraintMode.Locked;

                        UxrGrabManager.Instance.PlaceObject(GrabbableObject, batteryAnchor.Anchor, UxrPlacementOptions.Smooth | UxrPlacementOptions.DontRelease, true);

                        if (_manipulationHapticFeedback)
                        {
                            _manipulationHapticFeedback.MinAmplitude = _minHapticAmplitude;
                            _manipulationHapticFeedback.MaxAmplitude = _maxHapticAmplitude;
                        }

                        break;
                    }
                }
            }

            if (GrabbableObject.CurrentAnchor != null)
            {
                // Constrain transform when the battery is inside a door

                GeneratorDoor generatorDoor = GrabbableObject.CurrentAnchor.GetComponentInParent<GeneratorDoor>();

                if (generatorDoor != null && !generatorDoor.IsLockOpen)
                {
                    // Lock is closed? Battery can't move
                    GrabbableObject.TranslationLimitsMax = Vector3.zero;
                }
                else
                {
                    // Battery can only move in a specific axis but if it is grabbed past this distance it becomes free
                    GrabbableObject.TranslationLimitsMax = _batteryOffsetConstraint;

                    if (transform.parent != null && UxrGrabManager.Instance.IsBeingGrabbed(GrabbableObject) && transform.localPosition.z > _batteryInsertOffset)
                    {
                        // Free battery

                        if (_manipulationHapticFeedback)
                        {
                            _minHapticAmplitude                      = _manipulationHapticFeedback.MinAmplitude;
                            _maxHapticAmplitude                      = _manipulationHapticFeedback.MaxAmplitude;
                            _manipulationHapticFeedback.MinAmplitude = 0.0f;
                            _manipulationHapticFeedback.MaxAmplitude = 0.0f;
                        }

                        UxrGrabManager.Instance.RemoveObjectFromAnchor(GrabbableObject, false);
                        GrabbableObject.TranslationConstraint = UxrTranslationConstraintMode.Free;
                        GrabbableObject.RotationConstraint    = UxrRotationConstraintMode.Free;
                    }

                    // Also, if it is not being grabbed it will slide in

                    if (UxrGrabManager.Instance.IsBeingGrabbed(GrabbableObject) == false)
                    {
                        // Use simple gravity to slide in. Gravity will be mapped to z axis to slide in our local coordinate system.

                        Vector3 speed = Physics.gravity * _slideInTimer;
                        Vector3 pos   = GrabbableObject.transform.localPosition;
                        pos.z += Time.deltaTime * speed.y;

                        if (pos.z < 0.0f)
                        {
                            pos.z = 0.0f;

                            if (_placed == false)
                            {
                                _placed = true;

                                if (generatorDoor != null)
                                {
                                    // Turn lights on when the battery finished sliding in
                                    generatorDoor.IsBatteryInContact = true;
                                }
                            }
                        }

                        GrabbableObject.transform.localPosition = pos;

                        _slideInTimer += Time.deltaTime;
                    }
                    else
                    {
                        _slideInTimer = 0.0f;
                    }
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called right after the battery was grabbed.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            if (e.GrabbableObject.TranslationLimitsMax != Vector3.zero)
            {
                _placed = false;
            }
        }

        /// <summary>
        ///     Called right after the battery was released.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            if (e.GrabbableObject.CurrentAnchor != null && e.GrabbableObject.RigidBodySource)
            {
                e.GrabbableObject.RigidBodySource.isKinematic = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the battery is close enough to the given anchor to be placed.
        /// </summary>
        /// <param name="anchor">Battery anchor</param>
        /// <returns>Whether the battery is close enough</returns>
        private bool IsBatteryNearPlacement(UxrGrabbableObjectAnchor anchor)
        {
            float bias = -0.05f;

            Vector3 localPos = anchor.AlignTransform.InverseTransformPoint(GrabbableObject.DropAlignTransform.position);
            return localPos.z < _batteryInsertOffset + bias && localPos.z > 0.0f && Mathf.Abs(localPos.x) < _batteryDiameter && Mathf.Abs(localPos.y) < _batteryDiameter;
        }

        #endregion

        #region Private Types & Data

        private Vector3                       _batteryOffsetConstraint;
        private float                         _slideInTimer;
        private bool                          _placed;
        private UxrManipulationHapticFeedback _manipulationHapticFeedback;
        private float                         _minHapticAmplitude;
        private float                         _maxHapticAmplitude;

        #endregion
    }
}