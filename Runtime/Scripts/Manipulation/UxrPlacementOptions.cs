// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPlacementOptions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different ways a <see cref="UxrGrabbableObject" /> can transition when being placed on an
    ///     <see cref="UxrGrabbableObjectAnchor" />.
    /// </summary>
    [Flags]
    public enum UxrPlacementOptions
    {
        /// <summary>
        ///     Place immediately. If the object is being grabbed, release it.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Place using smooth transition (interpolation).
        /// </summary>
        Smooth = 1,

        /// <summary>
        ///     Do not release the object when placing.
        /// </summary>
        DontRelease = 1 << 1
    }
}