// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Color32Ext.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Color32" /> extensions.
    /// </summary>
    public static class Color32Ext
    {
        #region Public Methods

        /// <summary>
        ///     Transforms an array of bytes to a <see cref="Color32" /> component by component. If there are not enough values to
        ///     read, the remaining values are set to <see cref="byte.MinValue" /> (0) for RGB and <see cref="byte.MaxValue" />
        ///     (255) for A.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result color</returns>
        public static Color32 ToColor32(this byte[] data)
        {
            switch (data.Length)
            {
                case 0:  return default;
                case 1:  return new Color32(data[0], byte.MinValue, byte.MinValue, byte.MaxValue);
                case 2:  return new Color32(data[0], data[1],       byte.MinValue, byte.MaxValue);
                case 3:  return new Color32(data[0], data[1],       data[2],       byte.MaxValue);
                default: return new Color32(data[0], data[1],       data[2],       data[3]);
            }
        }

        /// <summary>
        ///     Transforms a <see cref="Color32" /> value into the int value it encodes the color in.
        /// </summary>
        /// <param name="self">Color</param>
        /// <returns>Int value</returns>
        public static int ToInt(this in Color32 self)
        {
            return self.r << 24 | self.g << 16 | self.b << 8 | self.a;
        }

        /// <summary>
        ///     Clamps <see cref="Color32" /> values component by component.
        /// </summary>
        /// <param name="self">Color whose components to clamp</param>
        /// <param name="min">Minimum RGB values</param>
        /// <param name="max">Maximum RGB values</param>
        /// <returns>Clamped color</returns>
        public static Color32 Clamp(this in Color32 self, in Color32 min, in Color32 max)
        {
            byte[] result = new byte[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = (byte)Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToColor32();
        }

        /// <summary>
        ///     Multiplies two colors by multiplying each component.
        /// </summary>
        /// <param name="self">Operand A</param>
        /// <param name="other">Operand B</param>
        /// <returns>Result color</returns>
        public static Color32 Multiply(this in Color32 self, in Color32 other)
        {
            return new Color32((byte)(self.r * other.r),
                               (byte)(self.g * other.g),
                               (byte)(self.b * other.b),
                               (byte)(self.a * other.a));
        }

        /// <summary>
        ///     Compares two colors.
        /// </summary>
        /// <param name="self">First color to compare</param>
        /// <param name="other">Second color to compare</param>
        /// <returns>Whether the two colors are the same</returns>
        public static bool IsSameColor(this in Color32 self, in Color32 other)
        {
            return self.r == other.r && self.g == other.g && self.b == other.b && self.a == other.a;
        }

        /// <summary>
        ///     Converts the color to a HTML color value (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="self">Color to convert</param>
        /// <returns>HTML color string</returns>
        public static string ToHtml(this in Color32 self)
        {
            return self.a == 255 ? string.Format(StringFormatRGB, self.r, self.g, self.b) : string.Format(StringFormatRGBA, self.r, self.g, self.b, self.a);
        }

        /// <summary>
        ///     Tries to parse a <see cref="Color32" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <param name="result">Parsed color or the default color value if there was an error</param>
        /// <returns>Whether the color was parsed successfully</returns>
        public static bool TryParse(string html, out Color32 result)
        {
            try
            {
                result = Parse(html);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        ///     Parses a <see cref="Color32" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <returns>The parsed color</returns>
        /// <exception cref="FormatException">The string had an incorrect format</exception>
        public static Color32 Parse(string html)
        {
            html.ThrowIfNull(nameof(html));

            Match match = _regex.Match(html);
            if (!match.Success)
            {
                throw new FormatException($"Input string [{html}] does not have the right format: #RRGGBB or #RRGGBBAA");
            }

            byte[] colorBytes = new byte[VectorLength];

            for (int i = 0; i < VectorLength - 1; ++i)
            {
                string hex = match.Groups[i + 1].Value;
                colorBytes[i] = byte.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);
            }

            string aa = match.Groups[VectorLength].Value;
            colorBytes[VectorLength - 1] = aa == string.Empty
                                                       ? byte.MaxValue
                                                       : byte.Parse(aa, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);

            return colorBytes.ToColor32();
        }

        /// <summary>
        ///     Parses asynchronously a <see cref="Color32" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>An awaitable <see cref="Task" /> that returns the parsed color</returns>
        /// <exception cref="FormatException">The string had an incorrect format</exception>
        public static Task<Color32?> ParseAsync(string html, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(html, out Color32 result) ? result : (Color32?)null, ct);
        }

        #endregion

        #region Private Types & Data

        private const int    VectorLength     = 4;
        private const string StringFormatRGBA = "#{0:X2}{1:X2}{2:X2}{3:X2}";
        private const string StringFormatRGB  = "#{0:X2}{1:X2}{2:X2}";
        private const string RegexPattern     = "^#?([0-9A-F]{2})([0-9A-F]{2})([0-9A-F]{2})([0-9A-F]{2})?$";

        private static readonly Regex _regex = new Regex(RegexPattern);

        #endregion
    }
}