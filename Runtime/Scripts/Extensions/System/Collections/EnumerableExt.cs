// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateXR.Extensions.System.Collections
{
    /// <summary>
    ///     <see cref="IEnumerable{T}" /> extensions.
    /// </summary>
    public static class EnumerableExt
    {
        #region Public Methods

        /// <summary>
        ///     Returns a random element from the collection.
        /// </summary>
        /// <param name="list">Collection to get the random element from</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <returns>Random element from the collection</returns>
        /// <remarks>
        ///     Uses Unity's random number generator (<see cref="UnityEngine.Random.Range(int,int)" />).
        /// </remarks>
        public static TIn RandomElement<TIn>(this IEnumerable<TIn> list)
        {
            return list.Any() ? list.ElementAt(Random.Range(0, list.Count())) : default(TIn);
        }

        /// <summary>
        ///     Applies an <see cref="Action" /> on all elements in a collection.
        /// </summary>
        /// <param name="list">Elements to apply the action on</param>
        /// <param name="action">Action to apply</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <exception cref="ArgumentException">Any of the parameters was null</exception>
        public static void ForEach<TIn>(this IEnumerable<TIn> list, Action<TIn> action)
        {
            if (list == null)
            {
                throw new ArgumentException("Argument cannot be null.", nameof(list));
            }

            if (action == null)
            {
                throw new ArgumentException("Argument cannot be null.", nameof(action));
            }

            foreach (TIn value in list)
            {
                action(value);
            }
        }

        /// <summary>
        ///     Asynchronously applies a function on all elements in a collection.
        /// </summary>
        /// <param name="list">Elements to apply the function on</param>
        /// <param name="function">Function to apply</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <returns>An awaitable task wrapping the Task.WhenAll applying the function on all elements in a collection</returns>
        public static Task ForEachAsync<TIn>(this IEnumerable<TIn> list, Func<TIn, Task> function)
        {
            return Task.WhenAll(list.Select(function));
        }

        /// <summary>
        ///     Asynchronously applies a function to all elements in a collection.
        /// </summary>
        /// <param name="list">Elements to apply the function on</param>
        /// <param name="function">Function to apply</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <typeparam name="TOut">Function return type</typeparam>
        /// <returns>An awaitable task wrapping the Task.WhenAll applying the function on all elements in a collection</returns>
        public static Task<TOut[]> ForEachAsync<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, Task<TOut>> function)
        {
            return Task.WhenAll(list.Select(function));
        }

        /// <summary>
        ///     Asynchronously applies an action on all elements in a collection.
        /// </summary>
        /// <param name="list">Elements to apply the action on</param>
        /// <param name="action">Action to apply</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <returns>Task wrapping the Task.WhenAll applying the action on all elements in a collection</returns>
        public static Task ForEachThreaded<TIn>(this IEnumerable<TIn> list, Action<TIn> action)
        {
            void OnFaulted(Task runTask, int itemIndex)
            {
                Debug.LogWarning($"ForEachThreaded::Item[{itemIndex}] FAULTED (see reason below):");
                Debug.LogException(runTask.Exception);
            }

            return Task.WhenAll(list.Select((item, index) => Task.Run(() => action(item)).ContinueWith(runTask => OnFaulted(runTask, index), TaskContinuationOptions.OnlyOnFaulted)));
        }

        /// <summary>
        ///     Asynchronously applies a function on all elements in a collection.
        /// </summary>
        /// <param name="list">Elements to apply the function on</param>
        /// <param name="function">Function to apply</param>
        /// <typeparam name="TIn">Element type</typeparam>
        /// <returns>Task wrapping the Task.WhenAll applying the function on all elements in a collection</returns>
        public static Task<TOut[]> ForEachThreaded<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> function)
        {
            TOut OnFaulted(Task<TOut> t, int itemIndex)
            {
                Debug.LogWarning($"ForEachThreaded::Item[{itemIndex}] FAULTED (see reason below):");
                Debug.LogException(t.Exception);
                return default;
            }

            return Task.WhenAll(list.Select((item, index) => Task.Run(() => function(item)).ContinueWith(t => OnFaulted(t, index), TaskContinuationOptions.OnlyOnFaulted)));
        }

        /// <summary>
        ///     Returns the maximal element of the given sequence, based on the given projection.
        /// </summary>
        /// <remarks>
        ///     This overload uses the default comparer for the projected type. This operator uses deferred execution. The results
        ///     are evaluated and cached on first use to returned sequence.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null</exception>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null!);
        }

        /// <summary>
        ///     Returns the maximal element of the given sequence, based on the given projection and the specified comparer for
        ///     projected values.
        /// </summary>
        /// <remarks>
        ///     This operator uses deferred execution. The results are evaluated and cached on first use to returned sequence.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />, <paramref name="selector" /> or <paramref name="comparer" /> is null
        /// </exception>
        /// <exception cref="T:System.Exception">A delegate callback throws an exception.</exception>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            source.ThrowIfNull(nameof(source));
            selector.ThrowIfNull(nameof(selector));

            TSource result = default;
            TKey    keyMax = default;
            comparer ??= Comparer<TKey>.Default;
            bool isFirst = true;

            foreach (TSource s in source)
            {
                TKey key = selector(s);

                if (isFirst)
                {
                    result  = s;
                    keyMax  = key;
                    isFirst = false;
                }
                else if (comparer.Compare(key, keyMax) > 0)
                {
                    result = s;
                    keyMax = key;
                }
            }

            return result;
        }

        /// <summary>
        ///     Splits a list of strings using CamelCase.
        /// </summary>
        /// <param name="strings">List of strings</param>
        /// <returns>List of strings with added spacing</returns>
        public static IEnumerable<string> SplitCamelCase(this IEnumerable<string> strings)
        {
            foreach (string element in strings)
            {
                yield return element.SplitCamelCase();
            }
        }

        #endregion
    }
}