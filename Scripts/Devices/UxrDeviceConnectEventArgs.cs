// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDeviceConnectEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Device connection/disconnection event arguments.
    /// </summary>
    public class UxrDeviceConnectEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the device was connected (true) or disconnected (false).
        /// </summary>
        public bool IsConnected { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="isConnected">Whether the device was connected (true) or disconnected (false)</param>
        public UxrDeviceConnectEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }

        #endregion
    }
}