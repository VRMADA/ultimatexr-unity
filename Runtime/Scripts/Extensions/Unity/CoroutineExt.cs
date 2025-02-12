// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoroutineExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     Coroutine extensions.
    /// </summary>
    public static class CoroutineExt
    {
        #region Public Methods

        /// <summary>
        ///     Wraps a coroutine into a <see cref="Task" /> so that it can be used with async/await. The coroutine doesn't return
        ///     any value. For coroutines that can return a value use
        ///     <see cref="AsTaskWithResult{T}(System.Collections.IEnumerator,UnityEngine.MonoBehaviour)" />.
        /// </summary>
        /// <param name="coroutine">Coroutine to wrap</param>
        /// <param name="monoBehaviour">The MonoBehaviour that will run the coroutine</param>
        /// <returns>The task</returns>
        public static Task AsTask(this IEnumerator coroutine, MonoBehaviour monoBehaviour)
        {
            var tcs = new TaskCompletionSource<bool>();
            monoBehaviour.StartCoroutine(WaitForCoroutine(coroutine, tcs));
            return tcs.Task;
        }

        /// <summary>
        ///     Wraps a coroutine into a <see cref="Task" /> so that it can be used with async/await. The coroutine returns a value
        ///     of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="coroutine">Coroutine to wrap</param>
        /// <param name="monoBehaviour">The MonoBehaviour that will run the coroutine</param>
        /// <typeparam name="T">The task result type</typeparam>
        /// <returns>The task</returns>
        public static Task<T> AsTaskWithResult<T>(this IEnumerator coroutine, MonoBehaviour monoBehaviour)
        {
            var tcs = new TaskCompletionSource<T>();
            monoBehaviour.StartCoroutine(WaitForCoroutineWithResult(coroutine, tcs));
            return tcs.Task;
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Waits for a coroutine to complete.
        /// </summary>
        /// <param name="coroutine">Coroutine to run</param>
        /// <param name="tcs">A <see cref="TaskCompletionSource{TResult}" /></param>
        /// <returns>Coroutine IEnumerator</returns>
        private static IEnumerator WaitForCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
        {
            // Wait for the coroutine to complete
            yield return coroutine;

            // Mark the Task as completed
            tcs.SetResult(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Waits for a coroutine to complete. The coroutine returns a result.
        /// </summary>
        /// <param name="coroutine">Coroutine to run</param>
        /// <param name="tcs">A <see cref="TaskCompletionSource{TResult}" /></param>
        /// <typeparam name="T">The result type</typeparam>
        /// <returns>Coroutine IEnumerator</returns>
        private static IEnumerator WaitForCoroutineWithResult<T>(IEnumerator coroutine, TaskCompletionSource<T> tcs)
        {
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }

            // Extract the result if the coroutine returns one
            if (coroutine.Current is T result)
            {
                tcs.SetResult(result);
            }
            else
            {
                // No result, return default
                tcs.SetResult(default);
            }
        }

        #endregion
    }
}