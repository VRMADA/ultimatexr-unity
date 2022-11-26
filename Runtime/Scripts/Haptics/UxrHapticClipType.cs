// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticClipType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Haptics
{
    /// <summary>
    ///     Enumerates the supported pre-defined set of haptic feedbacks that can be generated procedurally and played using
    ///     raw haptic mode.
    /// </summary>
    public enum UxrHapticClipType
    {
        None,
        RumbleFreqVeryLow,
        RumbleFreqLow,
        RumbleFreqNormal,
        RumbleFreqHigh,
        RumbleFreqVeryHigh,
        Click,
        Shot,
        ShotBig,
        ShotBigger,
        Slide,
        Explosion
    }
}