// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrVarInterpolator_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Generic base for interpolator classes.
    /// </summary>
    /// <typeparam name="T">The type the class will interpolate</typeparam>
    public abstract class UxrVarInterpolator<T> : UxrVarInterpolator
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="smoothDamp">Smooth damp value [0.0, 1.0]</param>
        /// <param name="useStep">Whether to use step interpolation, where the interpolation will always return the start value</param>
        protected UxrVarInterpolator(float smoothDamp, bool useStep) : base(smoothDamp, useStep)
        {
        }

        #endregion

        #region Public Overrides UxrVarInterpolator

        /// <inheritdoc />
        public override object Interpolate(object a, object b, float t)
        {
            if (UseStep)
            {
                return a;
            }
            
            if (a is not T ta)
            {
                return default(T);
            }

            if (b is not T tb)
            {
                return default(T);
            }

            return Interpolate(ta, tb, t);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Interpolates between 2 values.
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor [0.0, 1.0]</param>
        /// <returns>Interpolated value</returns>
        /// <remarks>
        ///     The interpolated value will be affected by smoothing if the object was initialized with a smoothDamp value
        ///     greater than 0
        /// </remarks>
        public T Interpolate(T a, T b, float t)
        {
            T result = GetInterpolatedValue(a, b, t);

            if (!RequiresSmoothDampRestart && SmoothDamp > 0.0f)
            {
                result = GetInterpolatedValue(_lastValue, result, UxrInterpolator.GetSmoothInterpolationValue(SmoothDamp, Time.deltaTime));
            }

            ClearSmoothDampRestart();

            _lastValue = result;
            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Interpolates between 2 values. To be interpolated in child classes.
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor [0.0, 1.0]</param>
        /// <returns>Interpolated value</returns>
        protected abstract T GetInterpolatedValue(T a, T b, float t);

        #endregion

        #region Private Types & Data

        private T _lastValue;

        #endregion
    }
}