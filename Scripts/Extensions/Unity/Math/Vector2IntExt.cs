// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector2IntExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Math
{
    /// <summary>
    ///     <see cref="Vector2Int" /> extensions.
    /// </summary>
    public static class Vector2IntExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Representation of the negative infinity vector.
        /// </summary>
        public static ref readonly Vector2Int NegativeInfinity => ref s_negativeInfinity;

        /// <summary>
        ///     Representation of the positive infinity vector.
        /// </summary>
        public static ref readonly Vector2Int PositiveInfinity => ref s_positiveInfinity;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether any vector component stores an infinity value.
        /// </summary>
        /// <param name="self">Vector to check</param>
        /// <returns>Whether any component has an infinity value</returns>
        public static bool IsInfinity(this in Vector2Int self)
        {
            return self.x == int.MinValue || self.x == int.MaxValue || self.y == int.MinValue || self.y == int.MaxValue;
        }

        /// <summary>
        ///     Computes the absolute values of each vector component.
        /// </summary>
        /// <param name="self">Input vector</param>
        /// <returns>Result vector where each component is the absolute value of the input value component</returns>
        public static Vector2Int Abs(this in Vector2Int self)
        {
            return new Vector2Int(Mathf.Abs(self.x), Mathf.Abs(self.y));
        }

        /// <summary>
        ///     Clamps the vector components between min and max values.
        /// </summary>
        /// <param name="self">Input vector whose values to clamp</param>
        /// <param name="min">Minimum component values</param>
        /// <param name="max">Maximum component values</param>
        /// <returns>Clamped vector</returns>
        public static Vector2Int Clamp(this in Vector2Int self, in Vector2Int min, in Vector2Int max)
        {
            int[] result = new int[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToVector2Int();
        }

        /// <summary>
        ///     Replaces NaN component values with <paramref name="other" /> valid values.
        /// </summary>
        /// <param name="self">Vector whose NaN values to replace</param>
        /// <param name="other">Vector with valid values</param>
        /// <returns>Result vector</returns>
        public static Vector2Int FillNanWith(this in Vector2Int self, in Vector2Int other)
        {
            int[] result = new int[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = self.x == int.MinValue || self.x == int.MaxValue ? other[i] : self[i];
            }

            return result.ToVector2Int();
        }

        /// <summary>
        ///     Transforms an array of ints to a <see cref="Vector2Int" /> component by component.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result vector</returns>
        public static Vector2Int ToVector2Int(this int[] data)
        {
            Array.Resize(ref data, VectorLength);
            return new Vector2Int(data[0], data[1]);
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector2Int" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="result">Parsed vector or <see cref="PositiveInfinity" /> if there was an error</param>
        /// <returns>Whether the vector was parsed successfully</returns>
        public static bool TryParse(string s, out Vector2Int result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = PositiveInfinity;
                return false;
            }
        }

        /// <summary>
        ///     Parses a <see cref="Vector2Int" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <returns>Parsed vector</returns>
        public static Vector2Int Parse(string s)
        {
            s.ThrowIfNullOrWhitespace(nameof(s));

            // Remove the parentheses
            s = s.TrimStart(' ', '(', '[');
            s = s.TrimEnd(' ', ')', ']');

            // split the items
            string[] sArray = s.Split(s_cardinalSeparator, VectorLength);

            // store as an array
            int[] result = new int[VectorLength];
            for (int i = 0; i < sArray.Length; ++i)
            {
                result[i] = int.Parse(sArray[i], CultureInfo.InvariantCulture.NumberFormat);
            }

            return result.ToVector2Int();
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector2Int" /> from a string, asynchronously.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task returning the parsed vector or null if there was an error</returns>
        public static Task<Vector2Int?> ParseAsync(string s, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(s, out Vector2Int result) ? result : (Vector2Int?)null, ct);
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength      = 2;
        private const string CardinalSeparator = ",";

        private static readonly char[]     s_cardinalSeparator = CardinalSeparator.ToCharArray();
        private static readonly Vector2Int s_negativeInfinity  = int.MinValue * Vector2Int.one;
        private static readonly Vector2Int s_positiveInfinity  = int.MaxValue * Vector2Int.one;

        #endregion
    }
}