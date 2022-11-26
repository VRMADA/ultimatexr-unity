// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerInputCapabilities.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates the possible capabilities of a VR controller.
    /// </summary>
    [Flags]
    public enum UxrControllerInputCapabilities
    {
        /// <summary>
        ///     It supports raw haptic impulses.
        /// </summary>
        HapticImpulses = 1,

        /// <summary>
        ///     It supports haptic feedback using pre-recorded clips.
        /// </summary>
        HapticClips = 1 << 1,

        /// <summary>
        ///     It supports finger tracking.
        /// </summary>
        TrackedHandPose = 1 << 2
    }
}