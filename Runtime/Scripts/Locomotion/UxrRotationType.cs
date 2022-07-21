// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRotationType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Enumerates the supported types of rotation around the avatar's axis.
    /// </summary>
    public enum UxrRotationType
    {
        /// <summary>
        ///     Rotating around is not allowed.
        /// </summary>
        NotAllowed,

        /// <summary>
        ///     Avatar will rotate immediately.
        /// </summary>
        Immediate,

        /// <summary>
        ///     Fade-out followed by the rotation and fade-in.
        /// </summary>
        Fade,

        /// <summary>
        ///     Interpolated rotation.
        /// </summary>
        Smooth
    }
}