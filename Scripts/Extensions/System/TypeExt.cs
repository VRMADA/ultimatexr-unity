// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     <see cref="Type" /> extensions.
    /// </summary>
    public static class TypeExt
    {
        #region Public Methods

        /// <summary>
        ///     Throws an <see cref="InvalidCastException" /> if the type defined by <paramref name="from" /> can't be casted to
        ///     the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="from">Source type</param>
        /// <param name="to">Destination type</param>
        /// <exception cref="InvalidCastException">Thrown when the source type can't be casted to the destination type</exception>
        public static void ThrowIfInvalidCast(Type from, Type to)
        {
            if (!IsValidCast(from, to))
            {
                throw new InvalidCastException($"{from.Name} is not assignable to {to.Name}");
            }
        }

        /// <summary>
        ///     Checks whether the type defined by <paramref name="from" /> can be casted to the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="from">Source type</param>
        /// <param name="to">Destination type</param>
        /// <returns>Whether it can be casted</returns>
        public static bool IsValidCast(Type from, Type to)
        {
            return to.IsAssignableFrom(from);
        }

        /// <summary>
        ///     Throws an <see cref="InvalidCastException" /> if the type defined by <paramref name="self" /> can't be casted to
        ///     the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="self">Source type</param>
        /// <param name="to">Destination type</param>
        /// <exception cref="InvalidCastException">Thrown when the source type can't be casted to the destination type</exception>
        public static void ThrowIfCannotCastTo(this Type self, Type to)
        {
            ThrowIfInvalidCast(self, to);
        }

        /// <summary>
        ///     Checks whether the type defined by <paramref name="self" /> can be casted to the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="self">Source type</param>
        /// <param name="to">Destination type</param>
        /// <returns>Whether it can be casted</returns>
        public static bool CanCastTo(this Type self, Type to)
        {
            return IsValidCast(self, to);
        }

        #endregion
    }
}