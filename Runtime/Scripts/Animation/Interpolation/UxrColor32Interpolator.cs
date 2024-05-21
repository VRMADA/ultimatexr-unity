// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrColor32Interpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for <see cref="Color32" />.
    /// </summary>
    public class UxrColor32Interpolator : UxrVarInterpolator<Color32>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrColor32Interpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<Color32>

        /// <inheritdoc />
        protected override Color32 GetInterpolatedValue(Color32 a, Color32 b, float t)
        {
            return Color32.Lerp(a, b, t);
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrColor32Interpolator DefaultInterpolator = new UxrColor32Interpolator(0.0f, false);

        #endregion
    }
}