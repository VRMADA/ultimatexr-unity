// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrVector2Interpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for <see cref="Vector2" />.
    /// </summary>
    public class UxrVector2Interpolator : UxrVarInterpolator<Vector2>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrVector2Interpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<Vector2>

        /// <inheritdoc />
        protected override Vector2 GetInterpolatedValue(Vector2 a, Vector2 b, float t)
        {
            return Vector2.Lerp(a, b, t);
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrVector2Interpolator DefaultInterpolator = new UxrVector2Interpolator(0.0f, false);

        #endregion
    }
}