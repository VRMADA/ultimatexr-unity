// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCatmullRomSpline.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Animation.Splines
{
    /// <summary>
    ///     Catmull-Rom spline. It is used to interpolate smoothly between a set of points.
    /// </summary>
    public class UxrCatmullRomSpline : UxrSpline
    {
        #region Public Overrides UxrSpline

        /// <summary>
        ///     Does the object contain valid data in order to evaluate the curve?
        /// </summary>
        public override bool HasValidData => _points != null && _points.Count > 1;

        /// <summary>
        ///     Evaluates the curve
        /// </summary>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <param name="position">Interpolated point</param>
        /// <returns>Success or failure</returns>
        public override bool Evaluate(float t, out Vector3 position)
        {
            return Evaluate(t, out position, out float segmentLength);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Smoothly interpolates, using Catmull-Rom equations, from p1 to p2 using additional p0 and p3 points.
        /// </summary>
        /// <param name="p0">Point 0</param>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="p3">Point 3</param>
        /// <param name="t">Interpolation parameter [0.0f, 1.0f]</param>
        /// <returns>Interpolated point</returns>
        public static Vector3 Evaluate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 ret = new Vector3();
            float   t2  = t * t;
            float   t3  = t2 * t;

            ret.x = 0.5f * (2.0f * p1.x + (-p0.x + p2.x) * t + (2.0f * p0.x - 5.0f * p1.x + 4 * p2.x - p3.x) * t2 + (-p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x) * t3);
            ret.y = 0.5f * (2.0f * p1.y + (-p0.y + p2.y) * t + (2.0f * p0.y - 5.0f * p1.y + 4 * p2.y - p3.y) * t2 + (-p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y) * t3);
            ret.z = 0.5f * (2.0f * p1.z + (-p0.z + p2.z) * t + (2.0f * p0.z - 5.0f * p1.z + 4 * p2.z - p3.z) * t2 + (-p0.z + 3.0f * p1.z - 3.0f * p2.z + p3.z) * t3);

            return ret;
        }

        /// <summary>
        ///     Creates a spline. If UsePrecomputedSampleCount > 0 it will also precompute samples in order to evaluate
        ///     the spline using arc-length parameter.
        /// </summary>
        /// <param name="inOutMultiplier">
        ///     Magnitude of spline start and end dummy tangent vectors
        ///     compared to their respective control points. A value of 1 (default) will create dummies
        ///     mirroring p1 and p(n-1) vectors. A different value will multiply these vectors by it.
        ///     It can be used to change the spline start/end curvature.
        /// </param>
        /// <param name="points">Set of points defining the curve</param>
        /// <returns>Success or failure</returns>
        public bool Create(float inOutMultiplier = 1.0f, params Vector3[] points)
        {
            _points = new List<Vector3>(points);

            if (points.Length < 2)
            {
                return false;
            }

            _dummyStart = points[0] + (points[0] - points[1]) * inOutMultiplier;
            _dummyEnd   = points[points.Length - 1] + (points[points.Length - 1] - points[points.Length - 2]) * inOutMultiplier;

            ComputeArcLengthSamples();

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Interpolates the curve using Catmull-Rom equations.
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

            // Compute the index of p1 and build a Catmull segment with 4 points from there
            int   indexA   = Mathf.FloorToInt(t * (_points.Count - 1));
            float segmentT = t * (_points.Count - 1) - indexA;

            if (indexA >= _points.Count - 1)
            {
                indexA   = _points.Count - 2;
                segmentT = 1.0f;
            }

            Vector3 p0 = indexA == 0 ? _dummyStart : _points[indexA - 1];
            Vector3 p1 = _points[indexA];
            Vector3 p2 = _points[indexA + 1];
            Vector3 p3 = indexA >= _points.Count - 2 ? _dummyEnd : _points[indexA + 2];

            segmentLength = Vector3.Distance(p1, p2);

            // Interpolate
            position = Evaluate(p0, p1, p2, p3, segmentT);
            return true;
        }

        #endregion

        #region Private Types & Data

        private List<Vector3> _points;
        private Vector3       _dummyStart;
        private Vector3       _dummyEnd;

        #endregion
    }
}