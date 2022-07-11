// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTranslationType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Enumerates the different types an avatar can teleport from one place to another.
    /// </summary>
    public enum UxrTranslationType
    {
        /// <summary>
        ///     Immediate teleportation.
        /// </summary>
        Immediate,

        /// <summary>
        ///     Fadeout -> teleportation -> Fadein.
        /// </summary>
        Fade,

        /// <summary>
        ///     Position interpolation.
        /// </summary>
        Smooth
    }
}