// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector3Ext.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Math;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Math
{
    /// <summary>
    ///     <see cref="Vector3" /> extensions.
    /// </summary>
    public static class Vector3Ext
    {
        #region Public Types & Data

        /// <summary>
        ///     Represents the NaN vector, an invalid value.
        /// </summary>
        public static ref readonly Vector3 NaN => ref s_nan;

        /// <summary>
        ///     Represents the Vector3 with minimum float values per component.
        /// </summary>
        public static ref readonly Vector3 MinValue => ref s_minValue;

        /// <summary>
        ///     Represents the Vector3 with maximum float values per component.
        /// </summary>
        public static ref readonly Vector3 MaxValue => ref s_maxValue;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the given vector has any NaN component.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether any of the vector components has a NaN value</returns>
        public static bool IsNaN(this in Vector3 self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsNaN(self.z);
        }

        /// <summary>
        ///     Checks whether the given vector has any infinity component.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether any of the vector components has an infinity value</returns>
        public static bool IsInfinity(this in Vector3 self)
        {
            return float.IsInfinity(self.x) || float.IsInfinity(self.y) || float.IsInfinity(self.z);
        }

        /// <summary>
        ///     Checks whether the given vector contains valid data.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether the vector contains all valid values</returns>
        public static bool IsValid(this in Vector3 self)
        {
            return !self.IsNaN() && !self.IsInfinity();
        }

        /// <summary>
        ///     Replaces NaN component values with <paramref name="other" /> valid values.
        /// </summary>
        /// <param name="self">Vector whose NaN values to replace</param>
        /// <param name="other">Vector with valid values</param>
        /// <returns>Result vector</returns>
        public static Vector3 FillNanWith(this in Vector3 self, in Vector3 other)
        {
            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = float.IsNaN(self[i]) ? other[i] : self[i];
            }

            return result.ToVector3();
        }

        /// <summary>
        ///     Computes the absolute value of each component in a vector.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Vector whose components are the absolute values</returns>
        public static Vector3 Abs(this in Vector3 self)
        {
            return new Vector3(Mathf.Abs(self.x), Mathf.Abs(self.y), Mathf.Abs(self.z));
        }

        /// <summary>
        ///     Clamps <see cref="Vector3" /> values component by component.
        /// </summary>
        /// <param name="self">Vector whose components to clamp</param>
        /// <param name="min">Minimum values</param>
        /// <param name="max">Maximum values</param>
        /// <returns>Clamped vector</returns>
        public static Vector3 Clamp(this in Vector3 self, in Vector3 min, in Vector3 max)
        {
            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToVector3();
        }

        /// <summary>
        ///     Fixes Euler angles so that they are always in the -180, 180 degrees range.
        /// </summary>
        /// <param name="self">Euler angles to fix</param>
        /// <returns>Euler angles in the -180, 180 degrees range</returns>
        public static Vector3 ToEuler180(this in Vector3 self)
        {
            float[] result = new float[VectorLength];

            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = self[i].ToEuler180();
            }

            return result.ToVector3();
        }

        /// <summary>
        ///     Computes the average of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with components averaged</returns>
        public static Vector3 Average(params Vector3[] vectors)
        {
            return new Vector3(vectors.Average(v => v.x),
                               vectors.Average(v => v.y),
                               vectors.Average(v => v.z));
        }

        /// <summary>
        ///     Computes the average of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with components averaged</returns>
        public static Vector3 Average(IEnumerable<Vector3> vectors)
        {
            return new Vector3(vectors.Average(v => v.x),
                               vectors.Average(v => v.y),
                               vectors.Average(v => v.z));
        }

        /// <summary>
        ///     Computes the maximum values of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with maximum component values</returns>
        public static Vector3 Max(params Vector3[] vectors)
        {
            return new Vector3(vectors.Max(v => v.x),
                               vectors.Max(v => v.y),
                               vectors.Max(v => v.z));
        }

        /// <summary>
        ///     Computes the maximum values of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with maximum component values</returns>
        public static Vector3 Max(IEnumerable<Vector3> vectors)
        {
            return new Vector3(vectors.Max(v => v.x),
                               vectors.Max(v => v.y),
                               vectors.Max(v => v.z));
        }

        /// <summary>
        ///     Computes the minimum values of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with minimum component values</returns>
        public static Vector3 Min(params Vector3[] vectors)
        {
            return new Vector3(vectors.Min(v => v.x),
                               vectors.Min(v => v.y),
                               vectors.Min(v => v.z));
        }

        /// <summary>
        ///     Computes the minimum values of a set of vectors.
        /// </summary>
        /// <param name="vectors">Input vectors</param>
        /// <returns>Vector with minimum component values</returns>
        public static Vector3 Min(IEnumerable<Vector3> vectors)
        {
            return new Vector3(vectors.Min(v => v.x),
                               vectors.Min(v => v.y),
                               vectors.Min(v => v.z));
        }

        /// <summary>
        ///     returns a vector with all components containing 1/component, checking for divisions by 0. Divisions by 0 have a
        ///     result of 0.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Result vector</returns>
        public static Vector3 Inverse(this in Vector3 self)
        {
            return new Vector3(Mathf.Approximately(self.x, 0f) ? 0f : 1f / self.x,
                               Mathf.Approximately(self.y, 0f) ? 0f : 1f / self.y,
                               Mathf.Approximately(self.z, 0f) ? 0f : 1f / self.z);
        }

        /// <summary>
        ///     Gets the number of components that are different between two vectors.
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <returns>The number of components [0, 3] that are different</returns>
        public static int DifferentComponentCount(Vector3 a, Vector3 b)
        {
            int count = 0;

            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                if (!Mathf.Approximately(a[axisIndex], b[axisIndex]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Multiplies two <see cref="Vector3" /> component by component.
        /// </summary>
        /// <param name="self">Operand A</param>
        /// <param name="other">Operand B</param>
        /// <returns>Result of multiplying both vectors component by component</returns>
        public static Vector3 Multiply(this in Vector3 self, in Vector3 other)
        {
            return new Vector3(self.x * other.x,
                               self.y * other.y,
                               self.z * other.z);
        }

        /// <summary>
        ///     Divides a <see cref="Vector3" /> by another, checking for divisions by 0. Divisions by 0 have a result of 0.
        /// </summary>
        /// <param name="self">Dividend</param>
        /// <param name="divisor">Divisor</param>
        /// <returns>Result vector</returns>
        public static Vector3 Divide(this in Vector3 self, in Vector3 divisor)
        {
            return self.Multiply(divisor.Inverse());
        }

        /// <summary>
        ///     Transforms an array of floats to a <see cref="Vector3" /> component by component. If there are not enough values to
        ///     read, the remaining values are set to NaN.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result vector</returns>
        public static Vector3 ToVector3(this float[] data)
        {
            return data.Length switch
                   {
                               0 => NaN,
                               1 => new Vector3(data[0], float.NaN, float.NaN),
                               2 => new Vector3(data[0], data[1],   float.NaN),
                               _ => new Vector3(data[0], data[1],   data[2])
                   };
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector3" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="result">Parsed vector or <see cref="NaN" /> if there was an error</param>
        /// <returns>Whether the vector was parsed successfully</returns>
        public static bool TryParse(string s, out Vector3 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = NaN;
                return false;
            }
        }

        /// <summary>
        ///     Parses a <see cref="Vector3" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <returns>Parsed vector</returns>
        public static Vector3 Parse(string s)
        {
            s.ThrowIfNullOrWhitespace(nameof(s));

            // Remove the parentheses
            s = s.TrimStart(' ', '(', '[');
            s = s.TrimEnd(' ', ')', ']');

            // split the items
            string[] sArray = s.Split(s_cardinalSeparator, VectorLength);

            // store as an array
            float[] result = new float[VectorLength];
            for (int i = 0; i < sArray.Length; ++i)
            {
                result[i] = float.TryParse(sArray[i],
                                           NumberStyles.Float,
                                           CultureInfo.InvariantCulture.NumberFormat,
                                           out float f)
                                        ? f
                                        : float.NaN;
            }

            return result.ToVector3();
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector3" /> from a string, asynchronously.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task returning the parsed vector or null if there was an error</returns>
        public static Task<Vector3?> ParseAsync(string s, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(s, out Vector3 result) ? result : (Vector3?)null, ct);
        }

        /// <summary>
        ///     Gets the vector which is the dominant negative or positive axis it is mostly pointing towards.
        /// </summary>
        /// <param name="vector">Vector to process</param>
        /// <returns>
        ///     Can return <see cref="Vector3.right" />, <see cref="Vector3.up" />, <see cref="Vector3.forward" />, -
        ///     <see cref="Vector3.right" />, -<see cref="Vector3.up" /> or -<see cref="Vector3.forward" />.
        /// </returns>
        public static Vector3 GetClosestAxis(this Vector3 vector)
        {
            float absX = Mathf.Abs(vector.x);
            float absY = Mathf.Abs(vector.y);
            float absZ = Mathf.Abs(vector.z);

            if (absX > absY)
            {
                return absX > absZ ? Mathf.Sign(vector.x) * Vector3.right : Mathf.Sign(vector.z) * Vector3.forward;
            }

            return absY > absZ ? Mathf.Sign(vector.y) * Vector3.up : Mathf.Sign(vector.z) * Vector3.forward;
        }

        /// <summary>
        ///     Computes a perpendicular vector.
        /// </summary>
        /// <param name="vector">Vector to compute another perpendicular to</param>
        /// <returns>Perpendicular vector in 3D space</returns>
        public static Vector3 GetPerpendicularVector(this Vector3 vector)
        {
            if (Mathf.Approximately(vector.x, 0.0f) == false)
            {
                return new Vector3(-vector.y, vector.x, 0.0f);
            }

            if (Mathf.Approximately(vector.y, 0.0f) == false)
            {
                return new Vector3(0.0f, -vector.z, vector.y);
            }

            return new Vector3(vector.z, 0.0f, -vector.y);
        }

        /// <summary>
        ///     Computes the signed distance from a point to a plane.
        /// </summary>
        /// <param name="point">The point to compute the distance from</param>
        /// <param name="planePoint">Point in a plane</param>
        /// <param name="planeNormal">Plane normal</param>
        /// <returns>Signed distance from a point to a plane</returns>
        public static float DistanceToPlane(this Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return new Plane(planeNormal, planePoint).GetDistanceToPoint(point);
        }

        /// <summary>
        ///     Computes the distance from a point to a line.
        /// </summary>
        /// <param name="point">The point to compute the distance from</param>
        /// <param name="lineA">Point A in the line</param>
        /// <param name="lineB">Point B in the line</param>
        /// <returns>Distance from point to the line</returns>
        public static float DistanceToLine(this Vector3 point, Vector3 lineA, Vector3 lineB)
        {
            return Vector3.Cross(lineB - lineA, point - lineA).magnitude;
        }

        /// <summary>
        ///     Computes the distance from a point to a segment.
        /// </summary>
        /// <param name="point">The point to compute the distance from</param>
        /// <param name="segmentA">Segment start point</param>
        /// <param name="segmentB">Segment end point</param>
        /// <returns>Distance from point to the segment</returns>
        public static float DistanceToSegment(this Vector3 point, Vector3 segmentA, Vector3 segmentB)
        {
            Vector3 ab = segmentB - segmentA;
            Vector3 av = point - segmentA;

            if (Vector3.Dot(av, ab) <= 0.0f)
            {
                return av.magnitude;
            }

            Vector3 bv = point - segmentB;

            if (Vector3.Dot(bv, ab) >= 0.0)
            {
                return bv.magnitude;
            }

            return Vector3.Cross(ab, av).magnitude / ab.magnitude;
        }

        /// <summary>
        ///     Computes the closest point in a segment to another point.
        /// </summary>
        /// <param name="point">The point to compute the distance to</param>
        /// <param name="segmentA">Segment start point</param>
        /// <param name="segmentB">Segment end point</param>
        /// <returns>Closest point in the segment</returns>
        public static Vector3 GetClosestPointFromSegment(this Vector3 point, Vector3 segmentA, Vector3 segmentB)
        {
            Vector3 ab = segmentB - segmentA;
            Vector3 av = point - segmentA;

            if (Vector3.Dot(av, ab) <= 0.0f)
            {
                return segmentA;
            }

            Vector3 bv = point - segmentB;

            if (Vector3.Dot(bv, ab) >= 0.0)
            {
                return segmentB;
            }

            return segmentA + Vector3.Project(av, ab.normalized);
        }

        /// <summary>
        ///     Checks if a point is inside a sphere. Supports spheres without uniform scaling.
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="sphere">Sphere collider to test against</param>
        /// <returns>Boolean telling whether the point is inside</returns>
        public static bool IsInsideSphere(this Vector3 point, SphereCollider sphere)
        {
            Vector3 localPos = sphere.transform.InverseTransformPoint(point);
            return localPos.magnitude <= sphere.radius;
        }

        /// <summary>
        ///     Checks if a point is inside of a BoxCollider.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="box">Box collider to test against</param>
        /// <param name="margin">Optional margin to be added to the each of the box sides</param>
        /// <param name="marginIsWorld">Whether the margin is specified in world coordinates or local</param>
        /// <returns>Whether point is inside</returns>
        public static bool IsInsideBox(this Vector3 point, BoxCollider box, Vector3 margin = default, bool marginIsWorld = true)
        {
            if (box == null)
            {
                return false;
            }

            Vector3 localPos = box.transform.InverseTransformPoint(point);

            if (marginIsWorld && box.transform.lossyScale != Vector3.one)
            {
                Vector3 pointPlusX = box.transform.InverseTransformPoint(point + box.transform.right);
                Vector3 pointPlusY = box.transform.InverseTransformPoint(point + box.transform.up);
                Vector3 pointPlusZ = box.transform.InverseTransformPoint(point + box.transform.forward);

                margin.x *= Vector3.Distance(localPos, pointPlusX);
                margin.y *= Vector3.Distance(localPos, pointPlusY);
                margin.z *= Vector3.Distance(localPos, pointPlusZ);
            }

            if (localPos.x - box.center.x >= -box.size.x * 0.5f - margin.x && localPos.x - box.center.x <= box.size.x * 0.5f + margin.x)
            {
                if (localPos.y - box.center.y >= -box.size.y * 0.5f - margin.y && localPos.y - box.center.y <= box.size.y * 0.5f + margin.y)
                {
                    if (localPos.z - box.center.z >= -box.size.z * 0.5f - margin.z && localPos.z - box.center.z <= box.size.z * 0.5f + margin.z)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a point is inside of a box.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="boxPosition">The box position in world space</param>
        /// <param name="boxRotation">The box rotation in world space</param>
        /// <param name="boxScale">The box scale</param>
        /// <param name="boxCenter">The box center in local box coordinates</param>
        /// <param name="boxSize">The box size in local box coordinates</param>
        /// <param name="margin">Optional margin to be added to the each of the box sides</param>
        /// <returns>True if it is inside, false if not</returns>
        public static bool IsInsideBox(this Vector3 point,
                                       Vector3      boxPosition,
                                       Quaternion   boxRotation,
                                       Vector3      boxScale,
                                       Vector3      boxCenter,
                                       Vector3      boxSize,
                                       Vector3      margin = default)
        {
            Matrix4x4 boxMatrix        = Matrix4x4.TRS(boxPosition, boxRotation, boxScale);
            Matrix4x4 inverseBoxMatrix = boxMatrix.inverse;
            Vector3   localPos         = inverseBoxMatrix.MultiplyPoint(point);

            if (localPos.x - boxCenter.x >= -boxSize.x * 0.5f - margin.x && localPos.x - boxCenter.x <= boxSize.x * 0.5f + margin.x)
            {
                if (localPos.y - boxCenter.y >= -boxSize.y * 0.5f - margin.y && localPos.y - boxCenter.y <= boxSize.y * 0.5f + margin.y)
                {
                    if (localPos.z - boxCenter.z >= -boxSize.z * 0.5f - margin.z && localPos.z - boxCenter.z <= boxSize.z * 0.5f + margin.z)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a point is inside of a BoxCollider. If it is outside, it is clamped to remain inside.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="box">Box collider to test against</param>
        /// <returns>Point clamped inside given box volume</returns>
        public static Vector3 ClampToBox(this Vector3 point, BoxCollider box)
        {
            if (box == null)
            {
                return point;
            }

            Vector3 pos         = box.transform.InverseTransformPoint(point);
            Vector3 center      = box.center;
            Vector3 halfBoxSize = box.size * 0.5f;

            if (pos.x < center.x - halfBoxSize.x)
            {
                pos.x = center.x - halfBoxSize.x;
            }

            if (pos.x > center.x + halfBoxSize.x)
            {
                pos.x = center.x + halfBoxSize.x;
            }

            if (pos.y < center.y - halfBoxSize.y)
            {
                pos.y = center.y - halfBoxSize.y;
            }

            if (pos.y > center.y + halfBoxSize.y)
            {
                pos.y = center.y + halfBoxSize.y;
            }

            if (pos.z < center.z - halfBoxSize.z)
            {
                pos.z = center.z - halfBoxSize.z;
            }

            if (pos.z > center.z + halfBoxSize.z)
            {
                pos.z = center.z + halfBoxSize.z;
            }

            return box.transform.TransformPoint(pos);
        }

        /// <summary>
        ///     Checks if a point is inside of a SphereCollider. If it is outside, it is clamped to remain inside.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="sphere">Sphere collider to test against</param>
        /// <returns>Point restricted to the given sphere volume</returns>
        public static Vector3 ClampToSphere(this Vector3 point, SphereCollider sphere)
        {
            if (sphere == null)
            {
                return point;
            }

            Vector3 pos    = sphere.transform.InverseTransformPoint(point);
            Vector3 center = sphere.center;

            float distance = Vector3.Distance(center, pos);

            if (distance > sphere.radius)
            {
                pos = center + (pos - center).normalized * sphere.radius;
                return sphere.transform.TransformPoint(pos);
            }

            return point;
        }

        /// <summary>
        ///     Computes the rotation of a direction around an axis.
        /// </summary>
        /// <param name="direction">Direction to rotate</param>
        /// <param name="axis">The rotation axis to use for the rotation</param>
        /// <param name="degrees">Rotation angle</param>
        /// <returns>Rotated direction</returns>
        public static Vector3 GetRotationAround(this Vector3 direction, Vector3 axis, float degrees)
        {
            return Quaternion.AngleAxis(degrees, axis) * direction;
        }

        /// <summary>
        ///     Computes the rotation of a point around a pivot and an axis.
        /// </summary>
        /// <param name="point">Point to rotate</param>
        /// <param name="pivot">Pivot to rotate it around to</param>
        /// <param name="axis">The rotation axis to use for the rotation</param>
        /// <param name="degrees">Rotation angle</param>
        /// <returns>Rotated point</returns>
        public static Vector3 GetRotationAround(this Vector3 point, Vector3 pivot, Vector3 axis, float degrees)
        {
            Vector3 dir = point - pivot;
            dir = Quaternion.AngleAxis(degrees, axis) * dir;
            return dir + pivot;
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength      = 3;
        private const string CardinalSeparator = ",";

        private static readonly char[]  s_cardinalSeparator = CardinalSeparator.ToCharArray();
        private static readonly Vector3 s_nan               = float.NaN * Vector3.one;
        private static readonly Vector3 s_minValue          = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        private static readonly Vector3 s_maxValue          = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        #endregion
    }
}