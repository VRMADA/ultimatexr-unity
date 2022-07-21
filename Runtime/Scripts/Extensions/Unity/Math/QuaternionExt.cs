// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuaternionExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Math
{
    /// <summary>
    ///     <see cref="Quaternion" /> extensions.
    /// </summary>
    public static class QuaternionExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Represents a NaN Quaternion.
        /// </summary>
        public static ref readonly Quaternion NaN => ref s_nan;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Transforms a <see cref="Quaternion" /> to a <see cref="Vector4" /> component by component.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns><see cref="Vector4" /> with the components of the <see cref="Quaternion" /></returns>
        public static Vector4 ToVector4(this in Quaternion self)
        {
            return new Vector4(self.x, self.y, self.z, self.w);
        }

        /// <summary>
        ///     Checks whether the given <see cref="Quaternion" /> has any NaN value.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns>Whether the quaternion has any NaN value</returns>
        public static bool IsNaN(this in Quaternion self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsNaN(self.z) || float.IsNaN(self.w);
        }

        /// <summary>
        ///     Checks whether the given <see cref="Quaternion" /> has any infinity value.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns>Whether the quaternion has any infinity value</returns>
        public static bool IsInfinity(this in Quaternion self)
        {
            return float.IsInfinity(self.x) || float.IsInfinity(self.y) || float.IsInfinity(self.z) || float.IsInfinity(self.w);
        }

        /// <summary>
        ///     Checks whether the given <see cref="Quaternion" /> has any 0 value.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns>Whether the quaternion has any 0 value</returns>
        public static bool IsZero(this in Quaternion self)
        {
            return self.x == 0.0f && self.y == 0.0f && self.z == 0.0f && self.w == 0.0f;
        }

        /// <summary>
        ///     Checks whether the given <see cref="Quaternion" /> contains valid data.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns>Whether the quaternion contains valid data</returns>
        public static bool IsValid(this in Quaternion self)
        {
            return !self.IsNaN() && !self.IsInfinity() && !self.IsZero();
        }

        /// <summary>
        ///     Multiplies two quaternions component by component.
        /// </summary>
        /// <param name="self">Operand A</param>
        /// <param name="other">Operand B</param>
        /// <returns>Result quaternion</returns>
        public static Quaternion Multiply(this in Quaternion self, in Quaternion other)
        {
            return new Quaternion(self.x * other.x,
                                  self.y * other.y,
                                  self.z * other.z,
                                  self.w * other.w);
        }

        /// <summary>
        ///     Computes the inverse of a quaternion component by component (1 / value), checking for divisions by 0. Divisions by
        ///     0 have a result of 0.
        /// </summary>
        /// <param name="self">Source quaternion</param>
        /// <returns>Result quaternion</returns>
        public static Quaternion Inverse(this in Quaternion self)
        {
            return new Quaternion(Mathf.Approximately(self.x, 0f) ? 0f : 1f / self.x,
                                  Mathf.Approximately(self.y, 0f) ? 0f : 1f / self.y,
                                  Mathf.Approximately(self.z, 0f) ? 0f : 1f / self.z,
                                  Mathf.Approximately(self.w, 0f) ? 0f : 1f / self.w);
        }

        /// <summary>
        ///     Divides two quaternions component by component, checking for divisions by 0. Divisions by 0 have a result of 0.
        /// </summary>
        /// <param name="self">Dividend</param>
        /// <param name="divisor">Divisor</param>
        /// <returns>Result quaternion</returns>
        public static Quaternion Divide(this in Quaternion self, in Quaternion divisor)
        {
            return self.Multiply(divisor.Inverse());
        }

        /// <summary>
        ///     Computes the average quaternion from a list.
        /// </summary>
        /// <param name="quaternions">List of quaternions</param>
        /// <returns>Average quaternion</returns>
        /// <remarks>
        ///     From
        ///     https://gamedev.stackexchange.com/questions/119688/calculate-average-of-arbitrary-amount-of-quaternions-recursion
        /// </remarks>
        public static Quaternion Average(IEnumerable<Quaternion> quaternions)
        {
            if (quaternions == null || quaternions.Count() == 0)
            {
                return Quaternion.identity;
            }

            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;
            float w = 0.0f;

            foreach (Quaternion q in quaternions)
            {
                x += q.x;
                y += q.y;
                z += q.z;
                w += q.w;
            }

            float k = 1.0f / Mathf.Sqrt(x * x + y * y + z * z + w * w);
            return new Quaternion(x * k, y * k, z * k, w * k);
        }

        /// <summary>
        ///     Applies the transformation to make a rotation defined by <paramref name="sourceRotation" /> rotate towards
        ///     <paramref name="targetRotation" />.
        /// </summary>
        /// <param name="self">Quaternion to apply the rotation to</param>
        /// <param name="sourceRotation">Source rotation that will try to match <paramref name="targetRotation" /></param>
        /// <param name="targetRotation">Target rotation to match</param>
        /// <param name="t">Optional interpolation value [0.0, 1.0]</param>
        public static void ApplyAlignment(this Quaternion self, Quaternion sourceRotation, Quaternion targetRotation, float t = 1.0f)
        {
            Quaternion rotationTowards = Quaternion.RotateTowards(sourceRotation, targetRotation, 180.0f);
            Quaternion relative        = Quaternion.Inverse(sourceRotation) * self;
            Quaternion result          = Quaternion.Slerp(self, rotationTowards * relative, t);
            self.Set(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        ///     Parses a <see cref="Quaternion" />.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <returns>Result quaternion</returns>
        public static Quaternion Parse(string s)
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

            return result.ToQuaternion();
        }

        /// <summary>
        ///     Tries to parse a <see cref="Quaternion" />.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="result">Parsed quaternion</param>
        /// <returns>Whether the quaternion was successfully parsed</returns>
        public static bool TryParse(string s, out Quaternion result)
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
        ///     Creates a <see cref="Quaternion" /> from a float array. If the array does not contain enough elements, the missing
        ///     components will contain NaN.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result quaternion</returns>
        public static Quaternion ToQuaternion(this float[] data)
        {
            return data.Length switch
                   {
                               0 => NaN,
                               1 => new Quaternion(data[0], float.NaN, float.NaN, float.NaN),
                               2 => new Quaternion(data[0], data[1],   float.NaN, float.NaN),
                               3 => new Quaternion(data[0], data[1],   data[2],   float.NaN),
                               _ => new Quaternion(data[0], data[1],   data[2],   data[3])
                   };
        }

        /// <summary>
        ///     Parses a <see cref="Quaternion" /> asynchronously.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable <see cref="Task" /> that returns the parsed <see cref="Quaternion" /> or null if there was an error</returns>
        public static Task<Quaternion?> ParseAsync(string s, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(s, out Quaternion result) ? result : (Quaternion?)null, ct);
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength      = 4;
        private const string CardinalSeparator = ",";

        private static readonly char[]     s_cardinalSeparator = CardinalSeparator.ToCharArray();
        private static readonly Quaternion s_nan               = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

        #endregion
    }
}