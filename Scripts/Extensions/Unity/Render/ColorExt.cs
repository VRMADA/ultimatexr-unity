// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorExt.cs" company="VRMADA">
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
    ///     <see cref="Color" /> extensions.
    /// </summary>
    public static class ColorExt
    {
        #region Public Methods

        /// <summary>
        ///     Transforms an array of floats to a <see cref="Color" /> component by component. If there are not enough values to
        ///     read, the remaining values are set to 0.0 for RGB and 1.0 for A.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <returns>Result color</returns>
        public static Color ToColor(this float[] data)
        {
            return data.Length switch
                   {
                               0 => default,
                               1 => new Color(data[0], 0f,      0f,      1f),
                               2 => new Color(data[0], data[1], 0f,      1f),
                               3 => new Color(data[0], data[1], data[2], 1f),
                               _ => new Color(data[0], data[1], data[2], data[3])
                   };
        }

        /// <summary>
        ///     Clamps <see cref="Color" /> values component by component.
        /// </summary>
        /// <param name="self">Color whose components to clamp</param>
        /// <param name="min">Minimum RGB values</param>
        /// <param name="max">Maximum RGB values</param>
        /// <returns>Clamped color</returns>
        public static Color Clamp(this in Color self, in Color min, in Color max)
        {
            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength; ++i)
            {
                result[i] = Mathf.Clamp(self[i], min[i], max[i]);
            }

            return result.ToColor();
        }

        /// <summary>
        ///     Multiplies two colors by multiplying each component.
        /// </summary>
        /// <param name="self">Operand A</param>
        /// <param name="other">Operand B</param>
        /// <returns>Result color</returns>
        public static Color Multiply(this in Color self, in Color other)
        {
            return new Color(self.r * other.r,
                             self.g * other.g,
                             self.b * other.b,
                             self.a * other.a);
        }

        /// <summary>
        ///     Creates a color based on an already existing color and an alpha value.
        /// </summary>
        /// <param name="color">Color value</param>
        /// <param name="alpha">Alpha value</param>
        /// <returns>
        ///     Result of combining the RGB of the <paramref name="color" /> value and alpha of <paramref name="alpha" />
        /// </returns>
        public static Color ColorAlpha(in Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        ///     Creates a color based on an already existing color and an alpha value.
        /// </summary>
        /// <param name="self">Color value</param>
        /// <param name="alpha">Alpha value</param>
        /// <returns>
        ///     Result of combining the RGBA of the color value and <paramref name="alpha" />
        /// </returns>
        public static Color WithAlpha(this in Color self, float alpha)
        {
            return ColorAlpha(self, alpha);
        }

        /// <summary>
        ///     Creates a color based on an already existing color and a brightness scale value.
        /// </summary>
        /// <param name="color">Color value</param>
        /// <param name="brightnessScale">The brightness scale factor</param>
        /// <returns>Color with adjusted brightness</returns>
        public static Color ScaleColorBrightness(this in Color color, float brightnessScale)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= brightnessScale;
            return Color.HSVToRGB(h, s, v);
        }

        /// <summary>
        ///     Converts the color to a HTML color value (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="self">Color to convert</param>
        /// <returns>HTML color string</returns>
        public static string ToHtml(this in Color self)
        {
            return Mathf.Approximately(self.a, 1f)
                               ? string.Format(StringFormatRGB,
                                               (byte)Mathf.Round(Mathf.Clamp01(self.r) * byte.MaxValue),
                                               (byte)Mathf.Round(Mathf.Clamp01(self.g) * byte.MaxValue),
                                               (byte)Mathf.Round(Mathf.Clamp01(self.b) * byte.MaxValue))
                               : string.Format(StringFormatRGBA,
                                               (byte)Mathf.Round(Mathf.Clamp01(self.r) * byte.MaxValue),
                                               (byte)Mathf.Round(Mathf.Clamp01(self.g) * byte.MaxValue),
                                               (byte)Mathf.Round(Mathf.Clamp01(self.b) * byte.MaxValue),
                                               (byte)Mathf.Round(Mathf.Clamp01(self.a) * byte.MaxValue));
        }

        /// <summary>
        ///     Tries to parse a <see cref="Color" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <param name="result">Parsed color or the default color value if there was an error</param>
        /// <returns>Whether the color was parsed successfully</returns>
        public static bool TryParse(string html, out Color result)
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
        ///     Parses a <see cref="Color" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <returns>The parsed color</returns>
        /// <exception cref="FormatException">The string had an incorrect format</exception>
        public static Color Parse(string html)
        {
            html.ThrowIfNull(nameof(html));

            Match match = _regex.Match(html);
            if (!match.Success)
            {
                throw new FormatException($"Input string [{html}] does not have the right format: #RRGGBB or #RRGGBBAA");
            }

            float[] result = new float[VectorLength];
            for (int i = 0; i < VectorLength - 1; ++i)
            {
                string hex = match.Groups[i + 1].Value;
                result[i] = byte.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat) / (float)byte.MaxValue;
            }

            string aa = match.Groups[VectorLength].Value;
            result[VectorLength - 1] = aa != string.Empty ? byte.Parse(aa, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat) / (float)byte.MaxValue : 1f;

            return result.ToColor();
        }

        /// <summary>
        ///     Parses asynchronously a <see cref="Color" /> from an HTML string (#RRGGBB or #RRGGBBAA).
        /// </summary>
        /// <param name="html">Source HTML string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>An awaitable <see cref="Task" /> that returns the parsed color</returns>
        /// <exception cref="FormatException">The string had an incorrect format</exception>
        public static Task<Color?> ParseAsync(string html, CancellationToken ct = default)
        {
            return Task.Run(() => TryParse(html, out Color result) ? result : (Color?)null, ct);
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