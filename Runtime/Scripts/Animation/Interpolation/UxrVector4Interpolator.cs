// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrVector4Interpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for <see cref="Vector4" />.
    /// </summary>
    public class UxrVector4Interpolator : UxrVarInterpolator<Vector4>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrVector4Interpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<Vector4>

        /// <inheritdoc />
        protected override Vector4 GetInterpolatedValue(Vector4 a, Vector4 b, float t)
        {
            return Vector4.Lerp(a, b, t);
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrVector4Interpolator DefaultInterpolator = new UxrVector4Interpolator(0.0f, false);

        #endregion
    }
}