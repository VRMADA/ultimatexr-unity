// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncOperationExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System.Threading;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.IO
{
    /// <summary>
    ///     <see cref="AsyncOperation" /> extensions.
    /// </summary>
    public static class AsyncOperationExt
    {
        #region Public Methods

        /// <summary>
        ///     Creates an awaitable <see cref="Task" /> that finishes when the given <see cref="AsyncOperation" /> finished.
        /// </summary>
        /// <param name="self">Unity asynchronous operation object</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable <see cref="Task" /> returning the caller <see cref="AsyncOperation" /> object</returns>
        public static async Task<AsyncOperation> Wait(this AsyncOperation self, CancellationToken ct = default)
        {
            await TaskExt.WaitUntil(() => self.isDone, ct).ConfigureAwait(false);
            return self;
        }

        #endregion
    }
}