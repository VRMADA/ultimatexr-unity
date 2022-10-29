// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     Enum extensions.
    /// </summary>
    public static class EnumExt
    {
        #region Public Methods

        /// <summary>
        ///     Enumerates all flags that are set in the enum value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="includeZero">Whether to include the 0 in the list</param>
        /// <returns>List of flags set in the enum value</returns>
        public static IEnumerable<T> GetFlags<T>(this T self, bool includeZero = false) where T : Enum
        {
            foreach (T value in Enum.GetValues(self.GetType()))
            {
                if (self.HasFlag(value) && !(!includeZero && Convert.ToInt32(value) == 0))
                {
                    yield return value;
                }
            }
        }

        #endregion
    }
}