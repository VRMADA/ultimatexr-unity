// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrLocomotionUpdater.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Internal interface for locomotion components to make updating publicly available only from within the framework.
    ///     Child classes from <see cref="UxrLocomotion" /> will implement these through the protected methods.
    /// </summary>
    internal interface IUxrLocomotionUpdater
    {
        #region Public Methods

        /// <summary>
        ///     Updates the locomotion and the avatar's position/orientation the component belongs to.
        /// </summary>
        void UpdateLocomotion();

        #endregion
    }
}