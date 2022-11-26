// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioSourceExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateXR.Extensions.Unity.Audio
{
    /// <summary>
    ///     <see cref="AudioSource" /> extensions.
    /// </summary>
    public static class AudioSourceExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Default spatial blend for 3D positioned audio.
        /// </summary>
        public const float SpatialBlend3D = 0.9f;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Ubiquitously plays an <see cref="AudioClip" />.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="clip">Reference to the sound clip file that will be played.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <returns>The just created temporal <see cref="AudioSource" />.</returns>
        public static AudioSource PlayClip(AudioClip clip,
                                           float     volume = 1.0f,
                                           float     delay  = 0.0f,
                                           float     pitch  = 1.0f)
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Playback is only allowed while playing.");
            }
            clip.ThrowIfNull(nameof(clip));
            volume = Mathf.Clamp01(volume);
            pitch  = Mathf.Clamp01(pitch);

            var gameObject  = new GameObject($"{nameof(AudioSourceExt)}_{nameof(PlayClip)}_{clip.name}");
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip         = clip;
            audioSource.volume       = volume;
            audioSource.pitch        = pitch;
            audioSource.spatialBlend = SpatialBlendUbiquitous;

            if (delay > 0.0f)
            {
                audioSource.PlayDelayed(delay);
            }
            else
            {
                audioSource.Play();
            }

            float duration = (delay + clip.length) * (Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale);
            Object.Destroy(gameObject, duration);
            return audioSource;
        }

        /// <summary>
        ///     Plays an AudioClip at a given position in world space.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="clip">Reference to the sound clip file that will be played.</param>
        /// <param name="point">Position in world space from which sound originates.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <param name="spatialBlend">Sets how much the 3D engine has an effect on the audio source [0.0, 1.0].</param>
        /// <returns>The just created temporal <see cref="AudioSource" />.</returns>
        /// <seealso cref="AudioSource.PlayClipAtPoint(AudioClip, Vector3, float)" />
        public static AudioSource PlayClipAtPoint(AudioClip clip,
                                                  Vector3   point,
                                                  float     volume       = 1.0f,
                                                  float     delay        = 0.0f,
                                                  float     pitch        = 1.0f,
                                                  float     spatialBlend = SpatialBlend3D)
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Playback is only allowed while playing.");
            }

            clip.ThrowIfNull(nameof(clip));
            volume       = Mathf.Clamp01(volume);
            spatialBlend = Mathf.Clamp01(spatialBlend);

            var gameObject  = new GameObject($"{nameof(AudioSourceExt)}_{nameof(PlayClipAtPoint)}_{clip.name}") { transform = { position = point } };
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip         = clip;
            audioSource.volume       = volume;
            audioSource.pitch        = pitch;
            audioSource.spatialBlend = spatialBlend;
            audioSource.Play();

            if (delay > 0.0f)
            {
                audioSource.PlayDelayed(delay);
            }
            else
            {
                audioSource.Play();
            }

            float duration = (delay + clip.length) * (Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale);
            Object.Destroy(gameObject, duration);
            return audioSource;
        }


        /// <summary>
        ///     Asynchronous and ubiquitously plays an <see cref="AudioClip" />.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="clip">Reference to the sound clip file that will be played.</param>
        /// <param name="volume">How loud the sound is at a distance of one world unit (one meter) [0.0, 1.0].</param>
        /// <param name="delay">Delay time specified in seconds.</param>
        /// <param name="pitch">
        ///     Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback
        ///     speed.
        /// </param>
        /// <param name="ct"><see cref="CancellationToken" /> to stop playing.</param>
        /// <returns>An awaitable <see cref="Task" />.</returns>
        public static async Task PlayClipAsync(AudioClip         clip,
                                               float             volume = 1.0f,
                                               float             delay  = 0.0f,
                                               float             pitch  = 1.0f,
                                               CancellationToken ct     = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Playback is only allowed while playing.");
            }

            float       duration    = (delay + clip.length) * (Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale);
            AudioSource audioSource = PlayClip(clip, volume, delay, pitch);
            await TaskExt.Delay(duration, ct);

            if (ct.IsCancellationRequested && audioSource != null)
            {
                audioSource.Stop();
                Object.Destroy(audioSource.gameObject);
            }
        }

        /// <summary>
        ///     Asynchronously plays an <see cref="AudioClip" /> at a given position in world space.
        /// </summary>
        /// <remarks>
        ///     This function creates an <see cref="AudioSource" /> but automatically disposes of it once the clip has finished
        ///     playing.
        /// </remarks>
        /// <param name="clip">Reference to the sound clip file that will be played.</param>
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
        public static async Task PlayClipAtPointAsync(AudioClip         clip,
                                                      Vector3           point,
                                                      float             volume       = 1.0f,
                                                      float             delay        = 0.0f,
                                                      float             pitch        = 1.0f,
                                                      float             spatialBlend = SpatialBlend3D,
                                                      CancellationToken ct           = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Playback is only allowed while playing.");
            }

            float       duration    = (delay + clip.length) * (Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale);
            AudioSource audioSource = PlayClipAtPoint(clip, point, volume, delay, pitch, spatialBlend);
            await TaskExt.Delay(duration, ct);

            if (ct.IsCancellationRequested && audioSource != null)
            {
                audioSource.Stop();
                Object.Destroy(audioSource.gameObject);
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Spatial blend for ubiquitous playback.
        /// </summary>
        private const float SpatialBlendUbiquitous = 0f;

        #endregion
    }
}