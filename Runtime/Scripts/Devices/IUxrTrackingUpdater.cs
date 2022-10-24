// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrTrackingUpdater.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Internal interface to be able to update the tracking information only from the UltimateXR assembly.
    /// </summary>
    internal interface IUxrTrackingUpdater
    {
        #region Public Methods

        /// <summary>
        ///     Updates the sensor information
        /// </summary>
        void UpdateSensors();

        /// <summary>
        ///     Updates the avatar using the current sensor information
        /// </summary>
        void UpdateAvatar();

        #endregion
    }
}