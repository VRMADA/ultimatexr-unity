// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnityWebRequestExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Exceptions;
using UnityEngine;
using UnityEngine.Networking;

namespace UltimateXR.Extensions.Unity.IO
{
    /// <summary>
    ///     <see cref="UnityWebRequest" /> extensions.
    /// </summary>
    public static class UnityWebRequestExt
    {
        #region Public Methods

        /// <summary>
        ///     Checks whether a given URI can be read using a <see cref="UnityWebRequest" />.
        /// </summary>
        /// <param name="uri">URI to check</param>
        /// <returns>Whether the URI is compatible with <see cref="UnityWebRequest" /></returns>
        public static bool IsUwrUri(string uri)
        {
            return uri.Contains(Application.streamingAssetsPath)
                   || uri.StartsWith(FilePrefix)
                   || uri.StartsWith(HttpPrefix)
                   || uri.StartsWith(HttpsPrefix);
        }

        /// <summary>
        ///     Sends a <see cref="UnityWebRequest" /> asynchronously.
        /// </summary>
        /// <param name="self">Request to send</param>
        /// <param name="ct">Cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task that will finish when the request was sent</returns>
        /// <exception cref="HttpUwrException">HttpError flag is on</exception>
        /// <exception cref="NetUwrException">NetworkError flag is on</exception>
        public static async Task Send(this UnityWebRequest self, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            await self.SendWebRequest().Wait(ct);
            if (ct.IsCancellationRequested)
            {
                self.Abort();
            }

            if (self.result == UnityWebRequest.Result.ConnectionError)
            {
                throw new NetUwrException(self.error);
            }

            if (self.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new HttpUwrException(self.error, self.responseCode);
            }
        }

        /// <summary>
        ///     Sends a <see cref="UnityWebRequest" /> asynchronously.
        /// </summary>
        /// <param name="self">Request to send</param>
        /// <param name="ct">Cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task that will finish when the request was sent</returns>
        /// <exception cref="OperationCanceledException">The task was canceled using <paramref name="ct" /></exception>
        /// <exception cref="HttpUwrException">HttpError flag is on</exception>
        /// <exception cref="NetUwrException">NetworkError flag is on</exception>
        public static async Task Fetch(this UnityWebRequest self, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await self.Send(ct);
        }

        /// <summary>
        ///     Loads an <see cref="AudioClip" /> asynchronously from an URI.
        /// </summary>
        /// <param name="uri">Location of the audio clip</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable <see cref="Task" /> that returns the loaded audio clip</returns>
        /// <exception cref="HttpUwrException">HttpError flag is on</exception>
        /// <exception cref="NetUwrException">NetworkError flag is on</exception>
        /// <exception cref="OperationCanceledException">The task was canceled using <paramref name="ct" /></exception>
        public static async Task<AudioClip> LoadAudioClip(string uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            using UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(FixUri(uri), AudioType.UNKNOWN);
            await req.Fetch(ct);

            ct.ThrowIfCancellationRequested();
            AudioClip result = DownloadHandlerAudioClip.GetContent(req);
            result.name = Path.GetFileNameWithoutExtension(uri);
            return result;
        }

        /// <summary>
        ///     Reads bytes asynchronously from an URI.
        /// </summary>
        /// <param name="uri">Location of the data</param>
        /// <param name="ct">Optional cancellation token, to cancel the operation</param>
        /// <returns>Awaitable task that returns the bytes read</returns>
        /// <exception cref="OperationCanceledException">The task was canceled using <paramref name="ct" /></exception>
        /// <exception cref="HttpUwrException">
        ///     HttpError flag is on
        /// </exception>
        /// <exception cref="NetUwrException">
        ///     NetworkError flag is on
        /// </exception>
        public static async Task<byte[]> Read(string uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            using UnityWebRequest req = UnityWebRequest.Get(FixUri(uri));
            await req.Fetch(ct);

            ct.ThrowIfCancellationRequested();
            return req.downloadHandler.data;
        }

        /// <summary>
        ///     Reads a string asynchronously from an URI.
        /// </summary>
        /// <param name="uri">Text location</param>
        /// <param name="ct">Optional cancellation, to cancel the operation</param>
        /// <returns>Awaitable task that returns the string read</returns>
        /// <exception cref="OperationCanceledException">The task was canceled using <paramref name="ct" /></exception>
        /// <exception cref="HttpUwrException">
        ///     HttpError flag is on
        /// </exception>
        /// <exception cref="NetUwrException">
        ///     NetworkError flag is on
        /// </exception>
        public static async Task<string> ReadText(string uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            using UnityWebRequest req = UnityWebRequest.Get(FixUri(uri));
            await req.Fetch(ct);

            ct.ThrowIfCancellationRequested();
            return req.downloadHandler.text;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Fixes an URI string.
        /// </summary>
        /// <param name="uri">String to fix</param>
        /// <returns>Fixed URI string</returns>
        private static string FixUri(string uri)
        {
            string result = uri.Trim('\\', '/', ' ');
            if (!IsUwrUri(uri))
            {
                result = FilePrefix + uri;
            }

            return result;
        }

        #endregion

        #region Private Types & Data

        private const string FilePrefix  = "file://";
        private const string HttpPrefix  = "http://";
        private const string HttpsPrefix = "https://";

        #endregion
    }
}