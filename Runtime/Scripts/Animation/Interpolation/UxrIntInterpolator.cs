// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrIntInterpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for int values.
    /// </summary>
    public class UxrIntInterpolator : UxrVarInterpolator<int>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrIntInterpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<int>

        /// <inheritdoc />
        protected override int GetInterpolatedValue(int a, int b, float t)
        {
            return Mathf.RoundToInt(Mathf.Lerp(a, b, t));
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrIntInterpolator DefaultInterpolator = new UxrIntInterpolator();

        #endregion
    }
}