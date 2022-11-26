// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Extensions.System.Math
{
    /// <summary>
    ///     <see cref="int" /> and <see cref="uint" /> extensions.
    /// </summary>
    public static class IntExt
    {
        #region Public Methods

        /// <summary>
        ///     Checks if the given int value has one or more flags set.
        /// </summary>
        /// <param name="self">int value</param>
        /// <param name="flags">Flag(s) to check for</param>
        /// <returns>Whether the int value has the given flag(s) set</returns>
        public static bool HasFlags(this int self, int flags)
        {
            return flags == (flags & self);
        }

        /// <summary>
        ///     Checks if the given uint value has one or more flags set.
        /// </summary>
        /// <param name="self">uint value</param>
        /// <param name="flags">Flag(s) to check for</param>
        /// <returns>Whether the uint value has the given flag(s) set</returns>
        public static bool HasFlags(this uint self, uint flags)
        {
            return flags == (flags & self);
        }

        /// <summary>
        ///     Returns an int value with one or more flags set if they weren't set already.
        /// </summary>
        /// <param name="self">int value</param>
        /// <param name="flags">Flag(s) to set when returned</param>
        /// <returns>int value with the given flag(s) set</returns>
        public static int WithFlags(this int self, int flags)
        {
            return self | flags;
        }

        /// <summary>
        ///     Returns an uint value with one or more flags set if they weren't set already.
        /// </summary>
        /// <param name="self">uint value</param>
        /// <param name="flags">Flag(s) to set when returned</param>
        /// <returns>uint value with the given flag(s) set</returns>
        public static uint WithFlags(this uint self, uint flags)
        {
            return self | flags;
        }

        /// <summary>
        ///     Returns an int value with one or more flags cleared if they were set.
        /// </summary>
        /// <param name="self">int value</param>
        /// <param name="flags">Flag(s) to clear when returned</param>
        /// <returns>int value with the given flag(s) cleared</returns>
        public static int WithoutFlags(this int self, int flags)
        {
            return self & ~flags;
        }

        /// <summary>
        ///     Returns an uint value with one or more flags cleared if they were set.
        /// </summary>
        /// <param name="self">uint value</param>
        /// <param name="flags">Flag(s) to clear when returned</param>
        /// <returns>uint value with the given flag(s) cleared</returns>
        public static uint WithoutFlags(this uint self, uint flags)
        {
            return self & ~flags;
        }

        /// <summary>
        ///     Clamps a value so that it doesn't go beyond a given range.
        /// </summary>
        /// <param name="self">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped value between [min, max]</returns>
        public static int Clamp(this ref int self, int min, int max)
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
        public static int Clamped(this int self, int min, int max)
        {
            return Mathf.Clamp(self, min, max);
        }

        #endregion
    }
}