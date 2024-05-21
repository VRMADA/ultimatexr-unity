// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFloatInterpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for float values.
    /// </summary>
    public class UxrFloatInterpolator : UxrVarInterpolator<float>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrFloatInterpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<float>

        /// <inheritdoc />
        protected override float GetInterpolatedValue(float a, float b, float t)
        {
            return Mathf.Lerp(a, b, t);
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrFloatInterpolator DefaultInterpolator = new UxrFloatInterpolator(0.0f, false);

        #endregion
    }
}