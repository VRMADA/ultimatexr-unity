// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Texture2DExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.IO;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Texture2D" /> extensions.
    /// </summary>
    public static class Texture2DExt
    {
        #region Public Methods

        /// <summary>
        ///     Creates a texture with a flat color.
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">Color to fill the texture with</param>
        /// <returns>The created <see cref="Texture2D" /> object</returns>
        public static Texture2D Create(int width, int height, Color32 color)
        {
            Texture2D tex = new Texture2D(width, height);

            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }

        /// <summary>
        ///     Loads asynchronously a texture from a given file <paramref name="uri" />. See <see cref="FileExt.Read" /> for
        ///     information on the file location.
        /// </summary>
        /// <param name="uri">Location of the texture file. See <see cref="FileExt.Read" /></param>
        /// <param name="ct">Optional cancellation token, to cancel the operation.</param>
        /// <returns>An awaitable <seealso cref="Task" /> that returns the loaded texture</returns>
        /// <exception cref="ArgumentNullException"><paramref name="uri" /> is null or empty</exception>
        /// <exception cref="OperationCanceledException">Task canceled using <paramref name="ct" /></exception>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="uri" /> was not found.</exception>
        /// <exception cref="NotSupportedException"><paramref name="uri" /> is in an invalid format.</exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
        public static async Task<Texture2D> FromFile(string uri, CancellationToken ct = default)
        {
            byte[] bytes = await FileExt.Read(uri, ct);
            return FromBytes(bytes);
        }

        /// <summary>
        ///     Loads asynchronously a texture from a file encoded in a base64 <seealso cref="string" />.
        /// </summary>
        /// <param name="base64">The base 64 image string</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <exception cref="ArgumentNullException"><paramref name="base64" /> is null or empty</exception>
        /// <exception cref="OperationCanceledException">Task canceled using <paramref name="ct" /></exception>
        /// <exception cref="FormatException">
        ///     The length of <paramref name="base64" />, ignoring white-space characters, is not
        ///     zero or a multiple of 4.
        /// </exception>
        /// <returns>An awaitable <see cref="Task" /> that returns the loaded texture, or null if it could not be loaded</returns>
        public static async Task<Texture2D> FromBase64(string base64, CancellationToken ct = default)
        {
            base64.ThrowIfNullOrWhitespace(nameof(base64));

            // Screenshot is from a file embedded in a string in base64 format
            byte[] bytes = await Task.Run(() => Convert.FromBase64String(base64), ct);
            return FromBytes(bytes);
        }


        /// <summary>
        ///     Loads a texture from a file loaded in a byte array.
        /// </summary>
        /// <param name="bytes">Image file byte array</param>
        /// <returns>The loaded <see cref="Texture2D" /></returns>
        public static Texture2D FromBytes(byte[] bytes)
        {
            bytes.ThrowIfNull(nameof(bytes));
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }

        #endregion
    }
}