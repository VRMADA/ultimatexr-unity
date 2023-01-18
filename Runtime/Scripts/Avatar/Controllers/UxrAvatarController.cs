// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarController.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Devices;
using UltimateXR.Locomotion;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Base class for <see cref="UxrAvatar" /> controllers. <see cref="UxrAvatarController" /> components are responsible
    ///     for updating the avatar. UltimateXR provides the <see cref="UxrStandardAvatarController" /> which has great
    ///     functionality. For flexibility and scalability, different avatar controllers can be created if required.
    /// </summary>
    /// <remarks>
    ///     <see cref="UxrAvatarController" /> components require the <see cref="UxrAvatar" /> component in the same
    ///     <see cref="GameObject" /> or any of its parents. Only one <see cref="UxrAvatarController" /> component type can
    ///     be active at the same time.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UxrAvatar))]
    public abstract class UxrAvatarController : UxrAvatarComponent<UxrAvatarController>, IUxrAvatarControllerUpdater
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _allowHandTracking = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets whether hand tracking is used when available.
        /// </summary>
        public bool AllowHandTracking
        {
            get => _allowHandTracking;
            set => _allowHandTracking = value;
        }

        #endregion

        #region Explicit IUxrAvatarControllerUpdater

        /// <inheritdoc />
        void IUxrAvatarControllerUpdater.UpdateAvatar()
        {
            // Will call the protected method, which is allowed to be overriden by child classes while
            // hiding the functionality so that it is handled by the framework in the correct order.
            UpdateAvatar();
        }

        /// <inheritdoc />
        void IUxrAvatarControllerUpdater.UpdateAvatarUsingTrackingDevices()
        {
            UpdateAvatarUsingTrackingDevices();
        }

        /// <inheritdoc />
        void IUxrAvatarControllerUpdater.UpdateAvatarManipulation()
        {
            // Will call the protected method, which is allowed to be overriden by child classes while
            // hiding the functionality so that it is handled by the framework in the correct order.
            UpdateAvatarManipulation();
        }

        /// <inheritdoc />
        void IUxrAvatarControllerUpdater.UpdateAvatarAnimation()
        {
            // Will call the protected method, which is allowed to be overriden by child classes while
            // hiding the functionality so that it is handled by the framework in the correct order.
            UpdateAvatarAnimation();
        }
        
        /// <inheritdoc />
        void IUxrAvatarControllerUpdater.UpdateAvatarPostProcess()
        {
            // Will call the protected method, which is allowed to be overriden by child classes while
            // hiding the functionality so that it is handled by the framework in the correct order.
            UpdateAvatarPostProcess();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets if the hand is available to interact with UI elements, such as pressing buttons. This is used by the UI
        ///     interaction system to ignore the hand for these events.
        ///     For example, when the hand is holding an object it could be desirable to not let it interact inadvertently with any
        ///     user interface.
        /// </summary>
        /// <param name="handSide">Which hand to check</param>
        /// <returns>Whether the given handed can interact with user interfaces</returns>
        public virtual bool CanHandInteractWithUI(UxrHandSide handSide)
        {
            return true;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Updates the avatar for the given frame. This is normally in charge of updating input devices, tracking devices and
        ///     locomotion.
        ///     Animation is left for a later stage (<see cref="UpdateAvatarAnimation" />), to make sure it is performed in the
        ///     right order right after Unity has updated the built-in animation components such as <see cref="Animator" />.
        /// </summary>
        protected virtual void UpdateAvatar()
        {
        }

        /// <summary>
        ///     Executes the avatar manipulation actions based on user input.
        /// </summary>
        protected virtual void UpdateAvatarManipulation()
        {
        }

        /// <summary>
        ///     Updates the animation and rig transforms for the given frame. It is performed in a later stage than
        ///     <see cref="UpdateAvatar" /> to make sure the transforms override the transforms that Unity may have updated using
        ///     built-in components such as <see cref="Animator" />.
        /// </summary>
        protected virtual void UpdateAvatarAnimation()
        {
        }

        /// <summary>
        ///     Updates the avatar for a given frame, at the end of all stages and UltimateXR manager updates such as the
        ///     <see cref="UxrGrabManager" />. It can be used to perform operations that require to be executed at the end of all
        ///     stages, such as Inverse Kinematics.
        /// </summary>
        protected virtual void UpdateAvatarPostProcess()
        {
        }

        /// <summary>
        ///     Updates the currently enabled input devices.
        /// </summary>
        protected void UpdateInputDevice()
        {
            foreach (UxrControllerInput controllerInput in Avatar.EnabledControllerInputs)
            {
                // Call method using internal interface
                ((IUxrControllerInputUpdater)controllerInput).UpdateInput();
            }

            // Refresh render mode
            Avatar.RenderMode = Avatar.RenderMode;
        }

        /// <summary>
        ///     Updates the tracking devices.
        /// </summary>
        protected void UpdateTrackingDevices()
        {
            foreach (UxrTrackingDevice trackingDevice in Avatar.TrackingDevices)
            {
                if (trackingDevice && trackingDevice.enabled)
                {
                    // Update tracking by calling internal interface
                    ((IUxrTrackingUpdater)trackingDevice).UpdateSensors();
                }
            }
        }

        /// <summary>
        ///     Updates the avatar using the current tracking data.
        /// </summary>
        protected void UpdateAvatarUsingTrackingDevices()
        {
            foreach (UxrTrackingDevice trackingDevice in Avatar.TrackingDevices)
            {
                if (trackingDevice && trackingDevice.enabled)
                {
                    // Update avatar by calling internal interface
                    ((IUxrTrackingUpdater)trackingDevice).UpdateAvatar();
                }
            }
        }

        /// <summary>
        ///     Updates the enabled locomotion components in the avatar.
        /// </summary>
        protected void UpdateLocomotion()
        {
            foreach (UxrLocomotion locomotion in UxrLocomotion.GetComponents<UxrLocomotion>(Avatar))
            {
                if (locomotion.gameObject.activeInHierarchy && locomotion.enabled)
                {
                    ((IUxrLocomotionUpdater)locomotion).UpdateLocomotion();
                }
            }
        }

        #endregion
    }
}