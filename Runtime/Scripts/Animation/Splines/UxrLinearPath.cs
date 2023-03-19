// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLinearPath.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Animation.Splines
{
    /// <summary>
    ///     Linear interpolation point sequence. It is used to interpolate linearly between a set of points.
    /// </summary>
    public class UxrLinearPath : UxrSpline
    {
        #region Public Overrides UxrSpline

        /// <summary>
        ///     Does the object contain valid data in order to evaluate the path?
        /// </summary>
        public override bool HasValidData => _points != null && _points.Count > 1;

        /// <summary>
        ///     Evaluates the path.
        /// </summary>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <param name="position">Interpolated point</param>
        /// <returns>Success or failure</returns>
        public override bool Evaluate(float t, out Vector3 position)
        {
            return Evaluate(t, out position, out float _);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a path. If <see cref="UxrSpline.UsePrecomputedSampleCount" /> > 0 it will also precompute samples in order
        ///     to evaluate the path using arc-length parameter.
        /// </summary>
        /// <param name="points">Set of points defining the curve</param>
        /// <returns>Success or failure</returns>
        public bool Create(params Vector3[] points)
        {
            _points = new List<Vector3>(points);

            if (points.Length < 2)
            {
                return false;
            }

            ComputeArcLengthSamples();
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Interpolates the path using linear interpolation.
        /// </summary>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <param name="position">Interpolated position</param>
        /// <param name="segmentLength">Length of the segment that this point belongs to</param>
        /// <returns>Success or failure</returns>
        private bool Evaluate(float t, out Vector3 position, out float segmentLength)
        {
            position      = Vector3.zero;
            segmentLength = 0.0f;

            t = Mathf.Clamp01(t);

            // Compute the index of p1
            int   indexA   = Mathf.FloorToInt(t * (_points.Count - 1));
            float segmentT = t * (_points.Count - 1) - indexA;

            if (indexA >= _points.Count - 1)
            {
                indexA   = _points.Count - 2;
                segmentT = 1.0f;
            }

            Vector3 p1 = _points[indexA];
            Vector3 p2 = _points[indexA + 1];

            segmentLength = Vector3.Distance(p1, p2);

            // Interpolate
            position = Vector3.Lerp(p1, p2, segmentT);
            return true;
        }

        #endregion

        #region Private Types & Data

        private List<Vector3> _points;

        #endregion
    }
}