// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrAvatarControllerUpdater.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Internal interface for avatar controllers to make updating publicly available only from within the framework.
    ///     Child classes from <see cref="UxrAvatarController" /> will implement these through the protected methods.
    /// </summary>
    internal interface IUxrAvatarControllerUpdater
    {
        #region Public Methods

        /// <summary>
        ///     Updates the avatar for the given frame. This is normally in charge of updating input devices, tracking devices and
        ///     locomotion.
        ///     Animation is left for a later stage (<see cref="UpdateAvatarAnimation" />), to make sure it is performed in the
        ///     right order right after Unity has updated the built-in animation components such as <see cref="Animator" />.
        /// </summary>
        void UpdateAvatar();

        /// <summary>
        ///     Updates the avatar using the current tracking data.
        /// </summary>
        void UpdateAvatarUsingTrackingDevices();

        /// <summary>
        ///     Updates the avatar manipulation actions based on user input.
        /// </summary>
        void UpdateAvatarManipulation();

        /// <summary>
        ///     Updates the animation and rig transforms for the given frame. It is performed in a later stage than
        ///     <see cref="UpdateAvatar" /> to make sure the transforms override the transforms that Unity may have updated using
        ///     built-in components such as <see cref="Animator" />.
        /// </summary>
        void UpdateAvatarAnimation();

        /// <summary>
        ///     Updates the avatar for a given frame, at the end of all stages and UltimateXR manager updates such as the
        ///     <see cref="UxrGrabManager" />. It can be used to perform operations that require to be executed at the end of all
        ///     stages, such as Inverse Kinematics.
        /// </summary>
        void UpdateAvatarPostProcess();

        #endregion
    }
}