// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Exceptions;
using UltimateXR.Extensions.Unity.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UltimateXR.Extensions.System.IO
{
    /// <summary>
    ///     File extensions.
    /// </summary>
    public static class FileExt
    {
        #region Public Methods

        /// <summary>
        ///     Reads bytes from a file asynchronously.
        ///     Multiple file locations are supported:
        ///     <list type="bullet">
        ///         <item>Files in <see cref="Application.streamingAssetsPath" /></item>
        ///         <item>Files in an http:// location</item>
        ///         <item>Files in a file:// location</item>
        ///     </list>
        ///     All other Uris will be considered file paths and the file:// location will be added.
        /// </summary>
        /// <param name="uri">File full path to be opened for reading</param>
        /// <param name="ct">
        ///     Optional cancellation token, to be able to cancel the asynchronous operation
        /// </param>
        /// <returns>
        ///     Bytes read
        /// </returns>
        /// <remarks>
        ///     <see cref="UnityWebRequest.Get(string)">UnityWebRequest.Get()</see> is used internally to perform the actual
        ///     reading
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     Task canceled using <paramref name="ct" />
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file specified in <paramref name="uri" /> was not found.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     <paramref name="uri" /> is in an invalid format.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The stream is currently in use by a previous read operation.
        /// </exception>
        public static async Task<byte[]> Read(string uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            uri.ThrowIfNullOrWhitespace(nameof(uri));

            if (UnityWebRequestExt.IsUwrUri(uri))
            {
                try
                {
                    return await UnityWebRequestExt.Read(uri, ct);
                }
                catch (UwrException e)
                {
                    throw new FileNotFoundException(e.Message, uri, e);
                }
            }

            List<byte> bytes  = new List<byte>();
            byte[]     buffer = new byte[0x1000];
            using (var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, true))
            {
                while (await fs.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false) != 0)
                {
                    bytes.AddRange(buffer);
                }
            }
            return bytes.ToArray();
        }

        /// <summary>
        ///     Reads text from a file asynchronously.
        ///     Multiple file locations are supported:
        ///     <list type="bullet">
        ///         <item>Files in <see cref="Application.streamingAssetsPath" /></item>
        ///         <item>Files in an http:// location</item>
        ///         <item>Files in a file:// location</item>
        ///     </list>
        ///     All other Uris will be considered file paths and the file:// location will be added.
        /// </summary>
        /// <param name="uri">File location</param>
        /// <param name="encoding">Optional file encoding</param>
        /// <param name="ct">Optional cancellation token, to cancel the asynchronous operation</param>
        /// <returns>A pair describing a boolean success value and the text read</returns>
        /// <remarks>
        ///     <see cref="UnityWebRequest.Get(string)">UnityWebRequest.Get()</see> is used internally to perform the actual
        ///     reading
        /// </remarks>
        public static async Task<(bool success, string text)> TryReadText(string uri, Encoding encoding = default, CancellationToken ct = default)
        {
            (bool success, string text) result;
            try
            {
                result.text    = await ReadText(uri, encoding, ct).ConfigureAwait(false);
                result.success = true;
            }
            catch
            {
                result.text    = null;
                result.success = false;
            }
            return result;
        }

        /// <summary>
        ///     Reads text from a file asynchronously.
        ///     Multiple file locations are supported:
        ///     <list type="bullet">
        ///         <item>Files in <see cref="Application.streamingAssetsPath" /></item>
        ///         <item>Files in an http:// location</item>
        ///         <item>Files in a file:// location</item>
        ///     </list>
        ///     All other Uris will be considered file paths and the file:// location will be added.
        /// </summary>
        /// <param name="uri">File full path to be opened for reading</param>
        /// <param name="encoding">Optional file encoding</param>
        /// <param name="ct">Optional cancellation token, to cancel the asynchronous operation</param>
        /// <returns>Text content of the file or <see langword="null" /> if not found.</returns>
        /// <remarks>
        ///     <see cref="UnityWebRequest.Get(string)">UnityWebRequest.Get()</see> is used internally to perform the actual
        ///     reading
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     Task canceled using <paramref name="ct" />
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     <paramref name="uri" /> is in an invalid format.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The stream is currently in use by a previous read operation.
        /// </exception>
        public static async Task<string> ReadText(string uri, Encoding encoding = default, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            uri.ThrowIfNullOrWhitespace(nameof(uri));
            encoding ??= DefaultEncoding;

            if (UnityWebRequestExt.IsUwrUri(uri))
            {
                try
                {
                    return await UnityWebRequestExt.ReadText(uri, ct);
                }
                catch (UwrException e)
                {
                    throw new FileNotFoundException(e.Message, uri, e);
                }
            }

            if (!File.Exists(uri))
            {
                throw new FileNotFoundException("File does not exist", uri);
            }

            using StreamReader sr = new StreamReader(uri, encoding, true);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously writes an <see cref="Array" /> of <see cref="byte" /> to a file at <paramref name="path" />.
        /// </summary>
        /// <param name="bytes">File content as <see cref="Array" /> of <see cref="byte" /></param>
        /// <param name="path">File full path to be opened for writing</param>
        /// <param name="ct">Optional cancellation token, to cancel the asynchronous operation</param>
        /// <returns>An awaitable writing <see cref="Task" /></returns>
        /// <exception cref="IOException">
        ///     An I/O error occurred while creating the file.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is a zero-length string, contains only white space, or
        ///     contains one or more invalid characters. You can query for invalid characters by using the
        ///     <see cref="Path.GetInvalidPathChars" /> method.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The <paramref name="path" /> parameter is longer than the system-defined maximum length.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required permission.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Task canceled using <paramref name="ct" />
        /// </exception>
        public static async Task Write(byte[] bytes, string path, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            path.ThrowIfNullOrWhitespace(nameof(path));

            using var stream = new MemoryStream(bytes);
            await Write(path, stream, ct);
        }

        /// <summary>
        ///     Asynchronously writes the content of an <paramref name="sourceStream" /> to a file at <paramref name="path" />.
        /// </summary>
        /// <param name="path">File full path to be opened for writing</param>
        /// <param name="sourceStream"><see cref="Stream" /> to be written into a file.</param>
        /// <param name="ct">Optional cancellation token, to cancel the asynchronous operation</param>
        /// <returns>An awaitable writing <see cref="Task" /></returns>
        /// <exception cref="IOException">
        ///     An I/O error occurred while creating the file.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is a zero-length string, contains only white space, or
        ///     contains one or more invalid characters. You can query for invalid characters by using the
        ///     <see cref="Path.GetInvalidPathChars" /> method.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The <paramref name="path" /> parameter is longer than the system-defined maximum length.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     <paramref name="sourceStream" /> does not support reading, or the destination stream does not support writing.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required permission.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Task canceled using <paramref name="ct" />
        /// </exception>
        public static Task Write(string path, Stream sourceStream, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            path.ThrowIfNullOrWhitespace(nameof(path));
            sourceStream.ThrowIfNull(nameof(sourceStream));

            string    dirName    = Path.GetDirectoryName(path);
            const int bufferSize = 81920;

            async Task WriteInternal()
            {
                if (dirName != null)
                {
                    Directory.CreateDirectory(dirName);
                }

                using FileStream outputStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, FileOptions.Asynchronous);
                await sourceStream.CopyToAsync(outputStream, bufferSize, ct).ConfigureAwait(false);
            }

            // Creating the new file has a performance impact that requires a new thread.
            return Task.Run(WriteInternal, ct);
        }

        /// <summary>
        ///     Asynchronously writes text to a file location.
        /// </summary>
        /// <param name="path">File full path to be opened for writing</param>
        /// <param name="text">Text to write</param>
        /// <param name="encoding">Optional file encoding</param>
        /// <param name="append">Optional boolean telling whether to append or override. Default behaviour is to override.</param>
        /// <param name="ct">Optional cancellation token, to cancel the asynchronous operation</param>
        /// <returns>Awaitable task</returns>
        /// <exception cref="OperationCanceledException">
        ///     Task canceled using <paramref name="ct" />
        /// </exception>
        public static Task WriteText(string path, string text, Encoding encoding = default, bool append = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            path.ThrowIfNullOrWhitespace(nameof(path));

            encoding ??= DefaultEncoding;
            string dirName = Path.GetDirectoryName(path);

            async Task WriteInternal()
            {
                if (dirName != null)
                {
                    Directory.CreateDirectory(dirName);
                }

                using StreamWriter sw = new StreamWriter(path, append, encoding);
                await sw.WriteAsync(text).ConfigureAwait(false);
            }

            // Creating the new file has a performance impact that requires a new thread.
            return Task.Run(WriteInternal, ct);
        }

        #endregion

        #region Private Types & Data

        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

        #endregion
    }
}