// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourcesExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.IO
{
    /// <summary>
    ///     <see cref="Resources" /> extensions.
    /// </summary>
    public class ResourcesExt
    {
        #region Public Methods

        /// <summary>
        ///     Loads a resource asynchronously.
        /// </summary>
        /// <param name="filePath">The path relative to a Resources folder</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <typeparam name="T">Resource type to load</typeparam>
        /// <returns>Awaitable <see cref="Task" /> that returns the loaded resource</returns>
        public static async Task<T> Load<T>(string filePath, CancellationToken ct = default)
                    where T : Object
        {
            ResourceRequest op = Resources.LoadAsync<T>(filePath);
            await op.Wait(ct);
            return (T)op.asset;
        }

        #endregion
    }
}