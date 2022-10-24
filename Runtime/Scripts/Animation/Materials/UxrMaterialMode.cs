// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMaterialMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     The material modes supported by <see cref="UxrAnimatedMaterial" />. It can animate the object's instanced material
    ///     or all the objects that share the same material.
    /// </summary>
    public enum UxrMaterialMode
    {
        /// <summary>
        ///     Animate this instance of the material only.
        /// </summary>
        InstanceOnly,

        /// <summary>
        ///     Animate the material, so that all renderers that share the same material are affected too.
        /// </summary>
        Shared
    }
}