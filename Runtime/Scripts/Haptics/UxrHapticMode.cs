// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Haptics
{
    /// <summary>
    ///     Enumerates the different types of supported haptic playback.
    /// </summary>
    public enum UxrHapticMode
    {
        /// <summary>
        ///     Replaces the current haptics on the device.
        /// </summary>
        Replace,

        /// <summary>
        ///     Mixes the new haptics with the current haptics on the device.
        /// </summary>
        Mix
    }
}