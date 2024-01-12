// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkCapabilities.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Enumerates the different capabilities of a Networking SDK.
    /// </summary>
    [Flags]
    public enum UxrNetworkCapabilities
    {
        /// <summary>
        ///     The SDK has support for components that add network synchronization of <see cref="Transform" /> components.
        /// </summary>
        NetworkTransform = 1 << 0,

        /// <summary>
        ///     The SDK has support for components that add network synchronization of <see cref="Rigidbody" /> components.
        /// </summary>
        NetworkRigidbody = 1 << 1,

        /// <summary>
        ///     The SDK has support for voice transmission.
        /// </summary>
        Voice = 1 << 16
    }
}