// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTransformSpace.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    /// <summary>
    ///     Enumerates the different transformation spaces.
    /// </summary>
    public enum UxrTransformSpace
    {
        /// <summary>
        ///     Transformation in world space.
        /// </summary>
        World = 0,

        /// <summary>
        ///     Transformation in local space, relative to the parent.
        /// </summary>
        Local = 1,

        /// <summary>
        ///     Transformation in avatar space.
        /// </summary>
        Avatar = 2
    }
}