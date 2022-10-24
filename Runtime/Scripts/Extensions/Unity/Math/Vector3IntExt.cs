// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector3IntExt.cs" company="VRMADA">
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
    ///     <see cref="Vector3Int" /> extensions.
    /// </summary>
    public static class Vector3IntExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Representation of the minimum int values per component.
        /// </summary>
        public static ref readonly Vector3Int MinValue => ref s_minValue;

        /// <summary>
        ///     Representation of the maximum int values per component.
        /// </summary>
        public static ref readonly Vector3Int MaxValue => ref s_maxValue;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether any vector component stores an infinity value.
        /// </summary>
        /// <param name="self">Vector to check</param>
        /// <returns>Whether any component has an infinity value</returns>
        public static bool IsInfinity(this in Vector3Int self)
        {
            return self.x == int.MinValue || self.x == int.MaxValue ||
                   self.y == int.MinValue || self.y == int.MaxValue ||
                   self.z == int.MinValue || self.z == int.MaxValue;
        }

        /// <summary>
        ///     Computes the absolute values of each vector component.
        /// </summary>
        /// <param name="self">Input vector</param>
        /// <returns>Result vector where each component is the absolute value of the input value component</returns>
        public static Vector3Int Abs(this in Vector3Int self)
        {
            return new Vector3Int(Mathf.Abs(self.x), Mathf.Abs(self.y), Mathf.Abs(self.z));
        }

        /// <summary>
        ///     Clamps the vector components between min and max values.
        /// </summary>
        /// <param name="self">Input vector whose values to clamp</param>
        /// <param name="min">Minimum component values</param>
        /// <param name="max">Maximum component values</param>
        /// <returns>Clamped vector</returns>
        public static Vector3Int Clamp(this in Vector3Int self, in Vector3Int min, in Vector3Int max)
        {
            int[] result = new int[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToVector3Int();
        }

        /// <summary>
        ///     Replaces NaN component values with <paramref name="other" /> valid values.
        /// </summary>
        /// <param name="self">Vector whose NaN values to replace</param>
        /// <param name="other">Vector with valid values</param>
        /// <returns>Result vector</returns>
        public static Vector3Int FillNaNWith(this in Vector3Int self, in Vector3Int other)
        {
            int[] result = new int[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = self.x == int.MinValue || self.x == int.MaxValue ? other[i] : self[i];
            }

            return result.ToVector3Int();
        }

        /// <summary>
        ///     Transforms an array of ints to a <see cref="Vector3Int" /> component by component.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result vector</returns>
        public static Vector3Int ToVector3Int(this int[] data)
        {
            Array.Resize(ref data, VectorLength);
            return new Vector3Int(data[0], data[1], data[2]);
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector3Int" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="result">Parsed vector or <see cref="MaxValue" /> if there was an error</param>
        /// <returns>Whether the vector was parsed successfully</returns>
        public static bool TryParse(string s, out Vector3Int result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = MaxValue;
                return false;
            }
        }

        /// <summary>
        ///     Parses a <see cref="Vector3Int" /> from a string.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <returns>Parsed vector</returns>
        public static Vector3Int Parse(string s)
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

            return result.ToVector3Int();
        }

        /// <summary>
        ///     Tries to parse a <see cref="Vector3Int" /> from a string, asynchronously.
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task returning the parsed vector or null if there was an error</returns>
        public static Task<Vector3Int?> ParseAsync(string s, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(s, out Vector3Int result) ? result : (Vector3Int?)null, ct);
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength      = 3;
        private const string CardinalSeparator = ",";

        private static readonly char[]     s_cardinalSeparator = CardinalSeparator.ToCharArray();
        private static readonly Vector3Int s_minValue          = int.MinValue * Vector3Int.one;
        private static readonly Vector3Int s_maxValue          = int.MaxValue * Vector3Int.one;

        #endregion
    }
}