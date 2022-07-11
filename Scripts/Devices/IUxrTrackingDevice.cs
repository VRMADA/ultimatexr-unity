// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrTrackingDevice.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Interface for tracking devices.
    /// </summary>
    public interface IUxrTrackingDevice : IUxrDevice
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called right before updating sensor data.
        /// </summary>
        event EventHandler SensorsUpdating;

        /// <summary>
        ///     Event called right after updating sensor data.
        /// </summary>
        event EventHandler SensorsUpdated;

        /// <summary>
        ///     Event called right before updating an avatar with the current sensor data.
        /// </summary>
        event EventHandler AvatarUpdating;

        /// <summary>
        ///     Event called right after updating an avatar with the current sensor data.
        /// </summary>
        event EventHandler AvatarUpdated;

        #endregion
    }
}