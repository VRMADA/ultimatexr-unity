// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMaterialParameterType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     Material parameter types that can be animated by <see cref="UxrAnimatedMaterial" />.
    /// </summary>
    public enum UxrMaterialParameterType
    {
        /// <summary>
        ///     Integer value.
        /// </summary>
        Int,

        /// <summary>
        ///     Single floating point value.
        /// </summary>
        Float,

        /// <summary>
        ///     Vector2 value representing two floating points.
        /// </summary>
        Vector2,

        /// <summary>
        ///     Vector3 value representing three floating points.
        /// </summary>
        Vector3,

        /// <summary>
        ///     Vector4 value representing four floating points.
        /// </summary>
        Vector4,

        /// <summary>
        ///     Color represented by 4 values RGBA.
        /// </summary>
        Color
    }
}