// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpriteExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Sprite" /> extensions.
    /// </summary>
    public static class SpriteExt
    {
        #region Public Methods

        /// <summary>
        ///     Loads asynchronously a sprite from a given file <paramref name="uri" />. See <see cref="FileExt.Read" /> for
        ///     information on the file location.
        /// </summary>
        /// <param name="targetImage">Image component the sprite will be used for</param>
        /// <param name="uri">File location. <see cref="FileExt.Read" /> for more information</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation.</param>
        /// <returns>An awaitable <seealso cref="Task" /> that returns the loaded sprite</returns>
        /// <exception cref="ArgumentNullException"><paramref name="uri" /> is null or empty</exception>
        /// <exception cref="OperationCanceledException">Task canceled using <paramref name="ct" /></exception>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="uri" /> was not found.</exception>
        /// <exception cref="NotSupportedException"><paramref name="uri" /> is in an invalid format.</exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
        public static async Task<Sprite> ReadSpriteFileAsync(Image targetImage, string uri, CancellationToken ct = default)
        {
            Texture2D texture2D = await Texture2DExt.FromFile(uri, ct);

            RectTransform t    = targetImage.rectTransform;
            Vector2       size = t.sizeDelta;
            Rect          rect = new Rect(0.0f, 0.0f, size.x, size.y);

            return Sprite.Create(texture2D, rect, t.pivot);
        }

        /// <summary>
        ///     Loads asynchronously a sprite encoded in a base64 <see cref="string" />.
        /// </summary>
        /// <param name="targetImage">Image component the sprite will be used for</param>
        /// <param name="base64">String encoding the file in base64</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>An awaitable <seealso cref="Task" /> that returns the loaded sprite</returns>
        /// <exception cref="ArgumentNullException"><paramref name="base64" /> is null or empty</exception>
        /// <exception cref="OperationCanceledException">Task canceled using <paramref name="ct" /></exception>
        /// <exception cref="FormatException">
        ///     The length of <paramref name="base64" />, ignoring white-space characters, is not
        ///     zero or a multiple of 4.
        /// </exception>
        public static async Task<Sprite> ReadSpriteBase64Async(Image targetImage, string base64, CancellationToken ct = default)
        {
            Texture2D texture2D = await Texture2DExt.FromBase64(base64, ct);

            RectTransform t    = targetImage.rectTransform;
            Vector2       size = t.sizeDelta;
            Rect          rect = new Rect(0.0f, 0.0f, size.x, size.y);

            return Sprite.Create(texture2D, rect, t.pivot);
        }

        #endregion
    }
}