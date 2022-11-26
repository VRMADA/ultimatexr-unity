// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateXR.Extensions.System.Collections
{
    /// <summary>
    ///     <see cref="List{T}" /> extensions.
    /// </summary>
    public static class ListExt
    {
        #region Public Methods

        /// <summary>
        ///     Gets the index of a given item in a list.
        /// </summary>
        /// <param name="self">List where to look for the item</param>
        /// <param name="item">Item to look for</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>Element index or -1 if not found</returns>
        /// <remarks>Equals() is used for comparison</remarks>
        public static int IndexOf<T>(this IReadOnlyList<T> self, T item)
        {
            for (int i = 0; i < self.Count; ++i)
            {
                if (Equals(self[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Returns a random element from the list.
        /// </summary>
        /// <param name="self">List to get the random element from</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>Random element from the list</returns>
        /// <remarks>
        ///     Uses Unity's random number generator (<see cref="Random.Range(int,int)" />).
        /// </remarks>
        public static T RandomElement<T>(this IReadOnlyList<T> self)
        {
            return self.Count > 0 ? self[Random.Range(0, self.Count)] : default;
        }

        /// <summary>
        ///     Returns a list with n random elements from a list without repetition.
        /// </summary>
        /// <param name="self">List to get the random elements from</param>
        /// <param name="count">Number of random elements to get</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>
        ///     List with random elements. If <paramref name="count" /> is larger than the list, the result list will be as
        ///     long as the input list.
        /// </returns>
        /// <remarks>
        ///     Uses Unity's random number generator (<see cref="Random.Range(int,int)" />).
        /// </remarks>
        public static List<T> RandomElementsWithoutRepetition<T>(this IReadOnlyList<T> self, int count)
        {
            List<T> candidates     = new List<T>(self);
            List<T> randomElements = new List<T>();

            for (int i = 0; i < count && candidates.Count > 0; ++i)
            {
                int randomIndex = Random.Range(0, candidates.Count);
                randomElements.Add(candidates[randomIndex]);
                candidates.RemoveAt(randomIndex);
            }

            return randomElements;
        }

        /// <summary>
        ///     Returns a list with n random elements from a list without repetition. An additional list can be provided to exclude
        ///     elements from appearing in the results.
        /// </summary>
        /// <param name="self">List to get the random elements from</param>
        /// <param name="listToExclude">List of elements to exclude from the results</param>
        /// <param name="count">Number of random elements to get</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>
        ///     List with random elements. If <paramref name="count" /> is larger than the list, the result list will be as
        ///     long as the input list minus the excluded elements.
        /// </returns>
        /// <remarks>
        ///     Uses Unity's random number generator (<see cref="Random.Range(int,int)" />).
        /// </remarks>
        public static List<T> RandomElementsWithoutRepetitionExcept<T>(this IReadOnlyList<T> self, IReadOnlyList<T> listToExclude, int count)
        {
            List<T> candidates     = new List<T>(self.Where(p => !listToExclude.Any(p2 => Equals(p2, p))));
            List<T> randomElements = new List<T>();

            for (int i = 0; i < count && candidates.Count > 0; ++i)
            {
                int randomIndex = Random.Range(0, candidates.Count);
                randomElements.Add(candidates[randomIndex]);
                candidates.RemoveAt(randomIndex);
            }

            return randomElements;
        }

        /// <summary>
        ///     Returns a list with the input list elements shuffled.
        /// </summary>
        /// <param name="self">List to get the random elements from</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>List with shuffled elements.</returns>
        /// <remarks>
        ///     Uses Unity's random number generator (<see cref="Random.Range(int,int)" />).
        /// </remarks>
        public static List<T> Shuffled<T>(this IReadOnlyList<T> self)
        {
            return self.RandomElementsWithoutRepetition(self.Count);
        }

        #endregion
    }
}