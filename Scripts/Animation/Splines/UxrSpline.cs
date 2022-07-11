// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSpline.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Animation.Splines
{
    /// <summary>
    ///     Spline base class. We use splines to interpolate smoothly between a set of points.
    ///     Interpolation can be done using the traditional t [0.0f, 1.0f] parameter and also distances to allow
    ///     arc-length evaluation.
    /// </summary>
    public abstract partial class UxrSpline
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the spline contains valid data in order to evaluate the curve.
        /// </summary>
        public abstract bool HasValidData { get; }

        /// <summary>
        ///     Gets the actual length of the curve.
        /// </summary>
        public float ArcLength => _arcLength;

        /// <summary>
        ///     Gets whether the spline contains valid data in order to evaluate the curve using arc length parametrization.
        /// </summary>
        public bool HasValidArcLengthData => HasValidData && _precomputedSamples != null && _precomputedSamples.Count > 0;

        /// <summary>
        ///     Number of curve samples that are going to be pre-computed in order to enable arc length parametrization.
        ///     This method must be called before creating the spline and will enable EvaluateUsingArcLength() calls.
        ///     For short splines the default value is enough. For very long splines it may be required to increase the
        ///     sample count.
        /// </summary>
        public int UsePrecomputedSampleCount { get; set; } = DefaultPrecomputedSampleCount;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Evaluates the curve
        /// </summary>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <param name="position">Interpolated point</param>
        /// <returns>Success or failure</returns>
        public abstract bool Evaluate(float t, out Vector3 position);

        /// <summary>
        ///     Evaluates the curve
        /// </summary>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <param name="position">Interpolated point</param>
        /// <param name="direction">Interpolated point direction vector</param>
        /// <returns>Success or failure</returns>
        public bool Evaluate(float t, out Vector3 position, out Vector3 direction)
        {
            position  = Vector3.zero;
            direction = Vector3.zero;

            if (!HasValidData)
            {
                return false;
            }

            // First the out of range cases. Needed because direction needs distance between the evaluated points.

            // If we have ArcLength information it's helpful to map distance to interpolation value.
            // Otherwise we risk guessing an interpolation value which may or may not be precise enough.
            float distanceT = HasValidArcLengthData ? EvalDirectionDistanceArcLength * ArcLength : EvalDirectionDistanceT;

            if (t < 0.0f)
            {
                Evaluate(0.0f,      out position);
                Evaluate(distanceT, out Vector3 positionTo);
                direction = (positionTo - position).normalized;
                return true;
            }

            if (t > 1.0f)
            {
                Evaluate(1.0f,             out position);
                Evaluate(1.0f - distanceT, out Vector3 positionFrom);
                direction = (position - positionFrom).normalized;
                return true;
            }

            // Evaluate position
            if (!Evaluate(t, out position))
            {
                return false;
            }

            // Evaluate a position a little bit further, to get the direction (see EvalDirectionDistance constant).
            if (!Evaluate(t + EvalDirectionDistanceT, out Vector3 position2))
            {
                return false;
            }

            // Compute direction vector and normalize
            direction = (position2 - position).normalized;
            return true;
        }

        /// <summary>
        ///     Evaluates the curve using arc-length parametrization
        /// </summary>
        /// <param name="distance">Distance parameter [0.0f, ArcLength]</param>
        /// <param name="position">Interpolated point</param>
        /// <returns>Success or failure</returns>
        public bool EvaluateUsingArcLength(float distance, out Vector3 position)
        {
            position = Vector3.zero;

            if (!HasValidArcLengthData)
            {
                return false;
            }

            // Search using the cache
            int foundPos;

            for (foundPos = _cachedIndexA; foundPos >= 0 && foundPos < _precomputedSamples.Count - 1;)
            {
                if (distance < _precomputedSamples[foundPos].Distance)
                {
                    --foundPos;
                }
                else if (distance > _precomputedSamples[foundPos + 1].Distance)
                {
                    ++foundPos;
                }
                else
                {
                    break;
                }
            }

            foundPos = Mathf.Clamp(foundPos, 0, _precomputedSamples.Count - 2);

            // 0.0f <= segmentT <= 1.0f. It will tell us where in between the two pre-computed points our point lies.
            float segmentT = (distance - _precomputedSamples[foundPos].Distance)
                             / (_precomputedSamples[foundPos + 1].Distance - _precomputedSamples[foundPos].Distance);

            // 0.0f <= t <= 1.0f. It will tell us which "t" to use to evaluate our curve.
            float t = Mathf.Lerp(_precomputedSamples[foundPos].LerpT, _precomputedSamples[foundPos + 1].LerpT, segmentT);

            // Update cache
            _cachedIndexA    = foundPos;
            _cachedArcLength = _precomputedSamples[foundPos].Distance;

            // Evaluate our curve!
            return Evaluate(t, out position);
        }

        /// <summary>
        ///     Evaluates the curve using arc-length parametrization
        /// </summary>
        /// <param name="distance">Distance parameter [0.0f, ArcLength]</param>
        /// <param name="position">Interpolated point</param>
        /// <param name="direction">Interpolated point direction vector</param>
        /// <returns>Success or failure</returns>
        public bool EvaluateUsingArcLength(float distance, out Vector3 position, out Vector3 direction)
        {
            position  = Vector3.zero;
            direction = Vector3.zero;

            if (!HasValidArcLengthData)
            {
                return false;
            }

            // Early tests. Needed because we need two points with distance between them to compute the direction vector.
            if (distance <= 0.0f)
            {
                Evaluate(0.0f, out position);
                EvaluateUsingArcLength(EvalDirectionDistanceArcLength, out Vector3 positionTo);
                direction = (positionTo - position).normalized;
                return true;
            }

            if (distance >= _arcLength)
            {
                Evaluate(1.0f, out position);
                EvaluateUsingArcLength(1.0f - EvalDirectionDistanceArcLength, out Vector3 positionFrom);
                direction = (position - positionFrom).normalized;
                return true;
            }

            // Evaluate position
            if (!EvaluateUsingArcLength(distance, out position))
            {
                return false;
            }

            // Evaluate a position a little bit further, to get the direction (see EvalDirectionDistance constant)
            if (!EvaluateUsingArcLength(distance + EvalDirectionDistanceArcLength, out Vector3 position2))
            {
                return false;
            }

            // Compute direction vector and normalize
            direction = (position2 - position).normalized;
            return true;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Pre-computes a set of samples that will enable to evaluate the curve using arc-length parametrization.
        /// </summary>
        protected void ComputeArcLengthSamples()
        {
            _precomputedSamples = new List<Sample>();

            _arcLength = 0.0f;
            Vector3 lastPos = Vector3.zero;

            for (int i = 0; i < UsePrecomputedSampleCount; ++i)
            {
                float t = i / (UsePrecomputedSampleCount - 1.0f);
                Evaluate(t, out Vector3 position);

                if (i > 0)
                {
                    _arcLength += Vector3.Distance(lastPos, position);
                }

                _precomputedSamples.Add(new Sample(t, _arcLength, position));
                lastPos = position;
            }

            _cachedIndexA    = 0;
            _cachedArcLength = 0.0f;
        }

        #endregion

        #region Private Types & Data

        private const int   DefaultPrecomputedSampleCount  = 100;
        private const float EvalDirectionDistanceT         = 0.005f;
        private const float EvalDirectionDistanceArcLength = 0.005f;

        private float        _arcLength;
        private List<Sample> _precomputedSamples;
        private int          _cachedIndexA;
        private float        _cachedArcLength;

        #endregion
    }
}