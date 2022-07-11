// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimationMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation
{
    /// <summary>
    ///     The supported animation modes used in different animation components.
    /// </summary>
    public enum UxrAnimationMode
    {
        /// <summary>
        ///     No animation.
        /// </summary>
        None,

        /// <summary>
        ///     Animate using a constant increase/decrease speed.
        /// </summary>
        Speed,

        /// <summary>
        ///     Animate using interpolation with different easing types. Can optionally be looped.
        /// </summary>
        Interpolate,

        /// <summary>
        ///     Animate using noise input.
        /// </summary>
        Noise
    }
}