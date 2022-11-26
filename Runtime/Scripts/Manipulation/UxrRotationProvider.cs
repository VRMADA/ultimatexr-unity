// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRotationProvider.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different ways a <see cref="UxrGrabbableObject" /> with rotation constraints can be
    ///     rotated while being manipulated.
    /// </summary>
    public enum UxrRotationProvider
    {
        /// <summary>
        ///     Grabbed object will rotate as the hand rotates.
        /// </summary>
        HandOrientation,

        /// <summary>
        ///     Grabbed object rotate based on the hand position around the object's pivot. Useful for levers, steering wheels...
        /// </summary>
        HandPositionAroundPivot,
    }
}