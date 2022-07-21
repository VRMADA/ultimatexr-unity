// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneManagerExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.Unity.IO;
using UnityEngine.SceneManagement;

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     <see cref="SceneManager" /> extensions.
    /// </summary>
    public sealed class SceneManagerExt : SceneManager
    {
        #region Public Methods

        /// <summary>
        ///     Creates an awaitable task that asynchronously loads a new scene.
        /// </summary>
        /// <param name="sceneName">Scene to load</param>
        /// <param name="mode">Mode in which the scene will be loaded</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static Task Load(string sceneName, LoadSceneMode mode, CancellationToken ct = default)
        {
            return LoadSceneAsync(sceneName, mode).Wait(ct);
        }

        /// <summary>
        ///     Creates an awaitable task that asynchronously unloads a scene.
        /// </summary>
        /// <param name="sceneName">Scene to unload</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static Task Unload(string sceneName, CancellationToken ct = default)
        {
            return UnloadSceneAsync(sceneName).Wait(ct);
        }

        #endregion
    }
}