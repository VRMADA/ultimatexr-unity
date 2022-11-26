// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPlacementType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different ways a <see cref="UxrGrabbableObject" /> can transition when being placed on an
    ///     <see cref="UxrGrabbableObjectAnchor" />.
    /// </summary>
    public enum UxrPlacementType
    {
        /// <summary>
        ///     Place immediately.
        /// </summary>
        Immediate,

        /// <summary>
        ///     Place using smooth transition (interpolation).
        /// </summary>
        Smooth
    }
}