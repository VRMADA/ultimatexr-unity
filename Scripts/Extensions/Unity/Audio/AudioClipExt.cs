// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioClipExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Exceptions;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.IO;
using UltimateXR.Extensions.Unity.IO;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Audio
{
    /// <summary>
    ///     Audio extensions.
    /// </summary>
    public static partial class AudioClipExt
    {
        #region Public Methods

        /// <summary>
        ///     Ubiquitously plays an <see cref="AudioClip" />.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="self">Reference to the sound clip file that will be played.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback speed.
        /// </param>
        /// <returns>The just created temporal <see cref="AudioSource" />.</returns>
        /// <seealso cref="AudioSourceExt.PlayClip" />
        public static AudioSource PlayClip(AudioClip self,
                                           float     volume = 1.0f,
                                           float     delay  = 0.0f,
                                           float     pitch  = 1.0f)
        {
            return AudioSourceExt.PlayClip(self, volume, delay, pitch);
        }

        /// <summary>
        ///     Plays an AudioClip at a given position in world space.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="self">Reference to the sound clip file that will be played.</param>
        /// <param name="point">Position in world space from which sound originates.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <param name="spatialBlend">Sets how much the 3D engine has an effect on the audio source [0.0, 1.0].</param>
        /// <returns>The just created temporal <see cref="AudioSource" />.</returns>
        /// <seealso cref="AudioSourceExt.PlayClipAtPoint" />
        public static AudioSource PlayClipAtPoint(AudioClip self,
                                                  Vector3   point,
                                                  float     volume       = 1.0f,
                                                  float     delay        = 0.0f,
                                                  float     pitch        = 1.0f,
                                                  float     spatialBlend = AudioSourceExt.SpatialBlend3D)
        {
            return AudioSourceExt.PlayClipAtPoint(self, point, volume, delay, pitch, spatialBlend);
        }

        /// <summary>
        ///     Asynchronous and ubiquitously plays the <see cref="AudioClip" />.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="self">Reference to the sound clip file that will be played.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <param name="ct"><see cref="CancellationToken" /> to stop playing.</param>
        /// <returns>An awaitable <see cref="Task" />.</returns>
        /// <seealso cref="AudioSourceExt.PlayClipAsync" />
        public static Task PlayAsync(this AudioClip    self,
                                     float             volume = 1.0f,
                                     float             delay  = 0.0f,
                                     float             pitch  = 1.0f,
                                     CancellationToken ct     = default)
        {
            return AudioSourceExt.PlayClipAsync(self, volume, delay, pitch, ct);
        }


        /// <summary>
        ///     Asynchronously plays the <see cref="AudioClip" /> at a given position in world space.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="self">Reference to the sound clip file that will be played.</param>
        /// <param name="point">Position in world space from which sound originates.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <param name="spatialBlend">Sets how much the 3D engine has an effect on the audio source [0.0, 1.0].</param>
        /// <param name="ct"><see cref="CancellationToken" /> to stop playing.</param>
        /// <returns>An awaitable <see cref="Task" />.</returns>
        /// <seealso cref="AudioSourceExt.PlayClipAtPointAsync" />
        public static Task PlayAtPointAsync(this AudioClip    self,
                                            Vector3           point,
                                            float             volume       = 1.0f,
                                            float             delay        = 0.0f,
                                            float             pitch        = 1.0f,
                                            float             spatialBlend = AudioSourceExt.SpatialBlend3D,
                                            CancellationToken ct           = default)
        {
            return AudioSourceExt.PlayClipAtPointAsync(self, point, volume, delay, pitch, spatialBlend, ct);
        }

        /// <summary>
        ///     Creates an <see cref="AudioClip" /> from a PCM stream.
        /// </summary>
        /// <param name="sourceStream">The source stream</param>
        /// <param name="clipName">The name assigned to the clip</param>
        /// <returns>The <see cref="AudioClip" /> object</returns>
        public static AudioClip FromPcmStream(Stream sourceStream, string clipName = "pcm")
        {
            clipName.ThrowIfNullOrWhitespace(nameof(clipName));
            byte[] bytes = new byte[sourceStream.Length];
            sourceStream.Read(bytes, 0, bytes.Length);
            return FromPcmBytes(bytes, clipName);
        }

        /// <summary>
        ///     Creates an <see cref="AudioClip" /> from a PCM stream asynchronously.
        /// </summary>
        /// <param name="sourceStream">The source stream</param>
        /// <param name="clipName">The name assigned to the clip</param>
        /// <param name="ct">The optional cancellation token, to cancel the task</param>
        /// <returns>An awaitable task that returns the <see cref="AudioClip" /> object</returns>
        public static async Task<AudioClip> FromPcmStreamAsync(Stream sourceStream, string clipName = "pcm", CancellationToken ct = default)
        {
            clipName.ThrowIfNullOrWhitespace(nameof(clipName));
            byte[] bytes = new byte[sourceStream.Length];
            await sourceStream.ReadAsync(bytes, 0, bytes.Length, ct);
            return await FromPcmBytesAsync(bytes, clipName, ct);
        }

        /// <summary>
        ///     Creates an <see cref="AudioClip" /> from a PCM byte array.
        /// </summary>
        /// <param name="bytes">The source data</param>
        /// <param name="clipName">The name assigned to the clip</param>
        /// <returns>The <see cref="AudioClip" /> object</returns>
        public static AudioClip FromPcmBytes(byte[] bytes, string clipName = "pcm")
        {
            clipName.ThrowIfNullOrWhitespace(nameof(clipName));
            var pcmData   = PcmData.FromBytes(bytes);
            var audioClip = AudioClip.Create(clipName, pcmData.Length, pcmData.Channels, pcmData.SampleRate, false);
            audioClip.SetData(pcmData.Value, 0);
            return audioClip;
        }

        /// <summary>
        ///     Creates an <see cref="AudioClip" /> from a PCM byte array asynchronously.
        /// </summary>
        /// <param name="bytes">The source data</param>
        /// <param name="clipName">The name assigned to the clip</param>
        /// <param name="ct">The optional cancellation token, to cancel the task</param>
        /// <returns>An awaitable task that returns the <see cref="AudioClip" /> object</returns>
        public static async Task<AudioClip> FromPcmBytesAsync(byte[] bytes, string clipName = "pcm", CancellationToken ct = default)
        {
            clipName.ThrowIfNullOrWhitespace(nameof(clipName));
            var pcmData   = await Task.Run(() => PcmData.FromBytes(bytes), ct);
            var audioClip = AudioClip.Create(clipName, pcmData.Length, pcmData.Channels, pcmData.SampleRate, false);
            audioClip.SetData(pcmData.Value, 0);
            return audioClip;
        }


        /// <summary>
        ///     Asynchronously reads and loads an <see cref="AudioClip" /> into memory from a given <paramref name="uri" />
        /// </summary>
        /// <param name="uri">Full path for <see cref="AudioClip" /> file</param>
        /// <param name="ct">The optional cancellation token, to cancel the task</param>
        /// <returns>Loaded <see cref="AudioClip" /></returns>
        /// <exception cref="HttpUwrException">
        ///     HttpError flag is on
        /// </exception>
        /// <exception cref="NetUwrException">
        ///     NetworkError flag is on
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The task was canceled using <paramref name="ct" />
        /// </exception>
        public static Task<AudioClip> FromFile(string uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            uri.ThrowIfNullOrWhitespace(nameof(uri));

            try
            {
                return UnityWebRequestExt.LoadAudioClip(uri, ct);
            }
            catch (UwrException e)
            {
                throw new FileNotFoundException(e.Message, uri, e);
            }
        }


        /// <summary>
        ///     Asynchronously reads and loads an <see cref="AudioClip" /> into memory from a given <paramref name="uri" />
        ///     pointing to a file with PCM bytes.
        /// </summary>
        /// <param name="uri">Full path with the PCM bytes</param>
        /// <param name="ct">Optional cancellation token to cancel the task</param>
        /// <returns>Loaded <see cref="AudioClip" /></returns>
        /// <exception cref="HttpUwrException">
        ///     HttpError flag is on
        /// </exception>
        /// <exception cref="NetUwrException">
        ///     NetworkError flag is on
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The task was canceled using <paramref name="ct" />
        /// </exception>
        public static async Task<AudioClip> FromPcmFile(string uri, CancellationToken ct = default)
        {
            string fileName = Path.GetFileNameWithoutExtension(uri);
            byte[] bytes    = await FileExt.Read(uri, ct);
            return await FromPcmBytesAsync(bytes, fileName, ct);
        }

        /// <summary>
        ///     Creates a <see cref="StreamedPcmClip" /> object from a stream containing PCM data.
        /// </summary>
        /// <param name="pcmStream">PCM data</param>
        /// <param name="clipName">The name that will be assigned to the clip</param>
        /// <returns><see cref="StreamedPcmClip" /> object</returns>
        public static StreamedPcmClip CreatePcmStreamed(Stream pcmStream, string clipName = "pcm")
        {
            return StreamedPcmClip.Create(pcmStream, clipName);
        }

        #endregion
    }
}