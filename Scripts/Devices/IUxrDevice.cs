// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrDevice.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Interface for VR devices, mainly designed to abstract tracking/input devices.
    /// </summary>
    public interface IUxrDevice
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called whenever the device is connected or disconnected
        /// </summary>
        event EventHandler<UxrDeviceConnectEventArgs> DeviceConnected;

        /// <summary>
        ///     Gets the SDK the implemented device needs in order to be available.
        ///     It should be null or empty if there is no SDK dependency. Otherwise is should use any of the SDK names in
        ///     <see cref="UxrManager" />. For example if requires the Oculus SDK, it should return
        ///     <see cref="UxrManager.SdkOculus" />.
        /// </summary>
        string SDKDependency { get; }

        #endregion
    }
}