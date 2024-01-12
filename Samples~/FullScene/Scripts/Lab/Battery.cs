// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Battery.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.Helpers;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Component that adds the smooth slide-in/slide-out effects to the grabbable battery.
    /// </summary>
    public class Battery : UxrAutoSlideInObject
    {
        #region Unity

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
            if (GrabbableObject.CurrentAnchor != null)
            {
                // Constrain transform when the battery is inside a door

                GeneratorDoor generatorDoor = GrabbableObject.CurrentAnchor.GetComponentInParent<GeneratorDoor>();

                if (generatorDoor != null && !generatorDoor.IsLockOpen)
                {
                    // Lock is closed? Battery can't move
                    GrabbableObject.IsLockedInPlace = true;
                }
                else
                {
                    // Battery can move
                    GrabbableObject.IsLockedInPlace = false;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        protected override void OnPlacedAfterSlidingIn()
        {
            base.OnPlacedAfterSlidingIn();

            if (GrabbableObject.CurrentAnchor != null)
            {
                GeneratorDoor generatorDoor = GrabbableObject.CurrentAnchor.GetComponentInParent<GeneratorDoor>();

                if (generatorDoor != null)
                {
                    // Turn lights on when the battery finished sliding in
                    generatorDoor.IsBatteryInContact = true;
                }
            }
        }

        /// <summary>
        ///     Called right after the battery was grabbed.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            if (!GrabbableObject.IsLockedInPlace && e.GrabbableAnchor != null)
            {
                GeneratorDoor generatorDoor = e.GrabbableAnchor.GetComponentInParent<GeneratorDoor>();

                if (generatorDoor != null)
                {
                    // Turn lights off when the battery is moved away
                    generatorDoor.IsBatteryInContact = true;
                }
            }
        }

        #endregion
    }
}