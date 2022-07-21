// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector2Ext.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Math
{
    /// <summary>
    ///     <see cref="Vector2" /> extensions.
    /// </summary>
    public static class Vector2Ext
    {
        #region Public Types & Data

        /// <summary>
        ///     Represents a NaN vector.
        /// </summary>
        public static ref readonly Vector2 NaN => ref s_nan;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the given vector has any NaN component.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether any of the vector components has a NaN value</returns>
        public static bool IsNaN(this in Vector2 self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y);
        }

        /// <summary>
        ///     Checks whether the given vector has any infinity component.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether any of the vector components has an infinity value</returns>
        public static bool IsInfinity(this in Vector2 self)
        {
            return float.IsInfinity(self.x) || float.IsInfinity(self.y);
        }

        /// <summary>
        ///     Checks whether the given vector contains valid data.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Whether the vector contains all valid values</returns>
        public static bool IsValid(this in Vector2 self)
        {
            return !self.IsNaN() && !self.IsInfinity();
        }

        /// <summary>
        ///     Replaces NaN component values with <paramref name="other" /> valid values.
        /// </summary>
        /// <param name="self">Vector whose NaN values to replace</param>
        /// <param name="other">Vector with valid values</param>
        /// <returns>Result vector</returns>
        public static Vector2 FillNanWith(this in Vector2 self, in Vector2 other)
        {
            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = float.IsNaN(self[i]) ? other[i] : self[i];
            }

            return result.ToVector2();
        }

        /// <summary>
        ///     Computes the absolute value of each component in a vector.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Vector whose components are the absolute values</returns>
        public static Vector2 Abs(this in Vector2 self)
        {
            return new Vector2(Mathf.Abs(self.x), Mathf.Abs(self.y));
        }

        /// <summary>
        ///     Clamps <see cref="Vector2" /> values component by component.
        /// </summary>
        /// <param name="self">Vector whose components to clamp</param>
        /// <param name="min">Minimum values</param>
        /// <param name="max">Maximum values</param>
        /// <returns>Clamped vector</returns>
        public static Vector2 Clamp(this in Vector2 self, in Vector2 min, in Vector2 max)
        {
            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToVector2();
        }

        /// <summary>
        ///     returns a vector with all components containing 1/component, checking for divisions by 0. Divisions by 0 have a
        ///     result of 0.
        /// </summary>
        /// <param name="self">Source vector</param>
        /// <returns>Result vector</returns>
        public static Vector2 Inverse(this in Vector2 self)
        {
            return new Vector2(Mathf.Approximately(self.x, 0f) ? 0f : 1f / self.x,
                               Mathf.Approximately(self.y, 0f) ? 0f : 1f / self.y);
        }

        /// <summary>
        ///     Multiplies two <see cref="Vector2" /> component by component.
        /// </summary>
        /// <param name="self">Operand A</param>
        /// <param name="other">Operand B</param>
        /// <returns>Result of multiplying both vectors component by component</returns>
        public static Vector2 Multiply(this in Vector2 self, in Vector2 other)
        {
            return new Vector2(self.x * other.x,
                               self.y * other.y);
        }

        /// <summary>
        ///     Divides a <see cref="Vector2" /> by another, checking for divisions by 0. Divisions by 0 have a result of 0.
        /// </summary>
        /// <param name="self">Dividend</param>
        /// <param name="divisor">Divisor</param>
        /// <returns>Result vector</returns>
        public static Vector2 Divide(this in Vector2 self, in Vector2 divisor)
        {
            return self.Multiply(divisor.Inverse());
        }

        /// <summary>
        ///     Transforms an array of floats to a <see cref="Vector2" /> component by component. If there are not enough values to
        ///     read, the remaining values are set to NaN.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result vector</returns>
        public static Vector2 ToVector2(this float[] data)
        {
            return data.Length switch
                   {
                               0 => NaN,
                               1 => new Vector2(data[0], float.NaN),
                               _ => new Vector2(data[0], data[1])
                   };
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector2" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="result">Parsed vector or NaN if there was an error</param>
        /// <returns>Whether the vector was parsed successfully</returns>
        public static bool TryParse(string s, out Vector2 result)
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
        ///     Parses a <see cref="Vector2" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <returns>Parsed vector</returns>
        public static Vector2 Parse(string s)
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

            return result.ToVector2();
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector2" /> from a string, asynchronously.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task returning the parsed vector or null if there was an error</returns>
        public static Task<Vector2?> ParseAsync(string s, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(s, out Vector2 result) ? result : (Vector2?)null, ct);
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength      = 2;
        private const string CardinalSeparator = ",";

        private static readonly char[]  s_cardinalSeparator = CardinalSeparator.ToCharArray();
        private static readonly Vector2 s_nan               = float.NaN * Vector2.one;

        #endregion
    }
}