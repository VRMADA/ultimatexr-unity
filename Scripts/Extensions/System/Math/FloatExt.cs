// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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

        #endregion
    }
}