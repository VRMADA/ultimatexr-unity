// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrQuaternionInterpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Interpolator for <see cref="Quaternion" />.
    /// </summary>
    public class UxrQuaternionInterpolator : UxrVarInterpolator<Quaternion>
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp for interpolation [0.0, 1.0] where 0.0 means no smoothing</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        public UxrQuaternionInterpolator(float smoothDamp = 0.0f, bool useStep = false) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Protected Overrides UxrVarInterpolator<Quaternion>

        /// <inheritdoc />
        protected override Quaternion GetInterpolatedValue(Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Slerp(a, b, t);
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default interpolator with smoothing.
        /// </summary>
        public static readonly UxrQuaternionInterpolator DefaultInterpolator = new UxrQuaternionInterpolator(0.0f, false);

        #endregion
    }
}