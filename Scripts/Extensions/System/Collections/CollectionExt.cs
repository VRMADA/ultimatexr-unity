// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace UltimateXR.Extensions.System.Collections
{
    /// <summary>
    ///     <see cref="IReadOnlyCollection{T}" /> and <see cref="ICollection{T}" /> extensions.
    /// </summary>
    public static class CollectionExt
    {
        #region Public Methods

        /// <summary>
        ///     Throws an exception if a given index is out of a <see cref="IReadOnlyCollection{T}" /> bounds.
        /// </summary>
        /// <param name="self">Collection</param>
        /// <param name="index">Index to check if it is out of bounds</param>
        /// <param name="paramName">Optional argument name</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <exception cref="IndexOutOfRangeException">When index is out of range and no parameter name was specified</exception>
        /// <exception cref="ArgumentOutOfRangeException">When index is out of range and a parameter name was specified</exception>
        public static void ThrowIfInvalidIndex<T>(this IReadOnlyCollection<T> self, int index, string paramName = null)
        {
            if (index >= 0 && index < self.Count)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new IndexOutOfRangeException($"Index[{index}] out of range for collection of {typeof(T).Name}");
            }
            throw new ArgumentOutOfRangeException(paramName, index, $"Index[{index}] out of range for collection of {typeof(T).Name}");
        }

        /// <summary>
        ///     Throws an exception if any of the given indexes is out of a <see cref="IReadOnlyCollection{T}" /> bounds.
        /// </summary>
        /// <param name="self">Collection</param>
        /// <param name="index1">Index 1 to check if it is out of bounds</param>
        /// <param name="index2">Index 2 to check if it is out of bounds</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <seealso cref="ThrowIfInvalidIndex{T}" />
        public static void ThrowIfInvalidIndexes<T>(this IReadOnlyCollection<T> self, int index1, int index2)
        {
            self.ThrowIfInvalidIndex(index1);
            self.ThrowIfInvalidIndex(index2);
        }

        /// <summary>
        ///     Throws an exception if any of the given indexes is out of a <see cref="IReadOnlyCollection{T}" /> bounds.
        /// </summary>
        /// <param name="self">Collection</param>
        /// <param name="index1">Index 1 to check if it is out of bounds</param>
        /// <param name="index2">Index 2 to check if it is out of bounds</param>
        /// <param name="index3">Index 3 to check if it is out of bounds</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <seealso cref="ThrowIfInvalidIndex{T}" />
        public static void ThrowIfInvalidIndexes<T>(this IReadOnlyCollection<T> self, int index1, int index2, int index3)
        {
            self.ThrowIfInvalidIndex(index1);
            self.ThrowIfInvalidIndex(index2);
            self.ThrowIfInvalidIndex(index3);
        }

        /// <summary>
        ///     Throws an exception if any of the given indexes is out of a <see cref="IReadOnlyCollection{T}" /> bounds.
        /// </summary>
        /// <param name="self">Collection</param>
        /// <param name="indexes">Indexes to check</param>
        /// <typeparam name="T">Element type</typeparam>
        public static void ThrowIfInvalidIndexes<T>(this IReadOnlyCollection<T> self, params int[] indexes)
        {
            foreach (int index in indexes)
            {
                self.ThrowIfInvalidIndex(index);
            }
        }

        /// <summary>
        ///     Splits a string using <see cref="string.Split(char[])" /> and adds the result to the collection.
        /// </summary>
        /// <param name="self">Collection to add the split result to</param>
        /// <param name="toSplit">String to split</param>
        /// <param name="separator">
        ///     Separator to use for splitting. This will be used to call <see cref="string.Split(char[])" />
        ///     on <paramref name="toSplit" />
        /// </param>
        /// <returns>The result collection</returns>
        public static ICollection<string> SplitAddRange(this ICollection<string> self, string toSplit, char separator)
        {
            self.ThrowIfNull(nameof(self));

            if (string.IsNullOrWhiteSpace(toSplit))
            {
                return self;
            }

            foreach (string s in toSplit.Split(separator))
            {
                self.Add(s.Trim());
            }

            return self;
        }

        /// <summary>
        ///     Splits a string using <see cref="string.Split(char[])" /> and sets the result in the collection.
        /// </summary>
        /// <param name="self">Collection to set the split result in</param>
        /// <param name="toSplit">String to split</param>
        /// <param name="separator">
        ///     Separator to use for splitting. This will be used to call <see cref="string.Split(char[])" />
        ///     on <paramref name="toSplit" />
        /// </param>
        /// <returns>The result collection</returns>
        public static ICollection<string> SplitSetRange(this ICollection<string> self, string toSplit, char separator)
        {
            self.ThrowIfNull(nameof(self));
            self.Clear();
            return self.SplitAddRange(toSplit, separator);
        }

        #endregion
    }
}