// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Extensions.System.Math
{
    /// <summary>
    ///     <see cref="float" /> extensions.
    /// </summary>
    public static class FloatExt
    {
        #region Public Methods

        /// <summary>
        ///     Compares two <c>float</c> values for equality with a specified precision threshold.
        /// </summary>
        /// <param name="a">The first <c>float</c> to compare</param>
        /// <param name="b">The second <c>float</c> to compare</param>
        /// <param name="precisionThreshold">
        ///     The precision threshold for <c>float</c> comparisons. Defaults to
        ///     <see cref="UxrConstants.Math.DefaultPrecisionThreshold" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <c>float</c> are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool EqualsUsingPrecision(this float a, float b, float precisionThreshold = UxrConstants.Math.DefaultPrecisionThreshold)
        {
            return Mathf.Abs(a - b) <= precisionThreshold;
        }

        /// <summary>
        ///     Converts a float value representing time in seconds to a formatted string value.
        /// </summary>
        /// <param name="self">Seconds to convert</param>
        /// <returns>Formatted time hh:mm::ss</returns>
        public static string SecondsToTimeString(this float self)
        {
            int hours   = Mathf.FloorToInt(self / 3600.0f);
            int minutes = Mathf.FloorToInt((self - hours * 3600.0f) / 60.0f);
            int seconds = Mathf.FloorToInt(self - hours * 3600.0f - minutes * 60.0f);

            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        /// <summary>
        ///     Checks if a float value is or is very close to zero.
        /// </summary>
        /// <param name="self">Value to check</param>
        /// <returns>Boolean telling whether the float value is or is very close to zero</returns>
        public static bool IsAlmostZero(this float self)
        {
            return Mathf.Approximately(self, 0.0f);
        }

        /// <summary>
        ///     Given a value in degrees, returns the same angle making sure it's in range [-180, 180]. For example, an
        ///     input of -380.3 would return -20.3.
        /// </summary>
        /// <param name="self">Value to process</param>
        /// <returns>Degrees in range between [-180, 180]</returns>
        public static float ToEuler180(this float self)
        {
            float angle = self % 360.0f;

            if (angle > 180.0f)
            {
                angle -= 360.0f;
            }
            else if (angle < -180.0f)
            {
                angle += 360.0f;
            }

            return angle;
        }

        /// <summary>
        ///     Clamps a value so that it doesn't go beyond a given range.
        /// </summary>
        /// <param name="self">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped value between [min, max]</returns>
        public static float Clamp(this ref float self, float min, float max)
        {
            self = Mathf.Clamp(self, min, max);
            return self;
        }

        /// <summary>
        ///     Returns a clamped value.
        /// </summary>
        /// <param name="self">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped value between [min, max]</returns>
        public static float Clamped(this float self, float min, float max)
        {
            return Mathf.Clamp(self, min, max);
        }

        /// <summary>
        ///     Clamps a value to [0.0, 1.0].
        /// </summary>
        /// <param name="self">Value to clamp</param>
        /// <returns>Clamped value between [0.0, 1.0]</returns>
        public static float Clamp(this ref float self)
        {
            self = Mathf.Clamp01(self);
            return self;
        }

        /// <summary>
        ///     Returns a clamped value in range [0.0, 1.0].
        /// </summary>
        /// <param name="self">Value to clamp</param>
        /// <returns>Clamped value between [0.0, 1.0]</returns>
        public static float Clamped(this float self)
        {
            return Mathf.Clamp01(self);
        }

        /// <summary>
        ///     Returns the value from the set with the maximum absolute value, but keeping the sign.
        /// </summary>
        /// <param name="values">Set of values</param>
        /// <returns>Value with the maximum absolute value keeping the sign</returns>
        public static float SignedAbsMax(params float[] values)
        {
            float signedAbsoluteMax = 0.0f;
            bool  initialized       = false;

            foreach (float value in values)
            {
                if (!initialized || Mathf.Abs(value) > Mathf.Abs(signedAbsoluteMax))
                {
                    initialized       = true;
                    signedAbsoluteMax = value;
                }
            }

            return signedAbsoluteMax;
        }

        #endregion
    }
}