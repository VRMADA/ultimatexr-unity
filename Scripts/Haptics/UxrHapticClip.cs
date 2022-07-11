// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticClip.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Haptics
{
    /// <summary>
    ///     Describes a haptic clip. It is possible to specify an audio clip whose wave will be used as a primary source for
    ///     the vibration, but also a secondary clip type that will be used if the device doesn't support audio clips as haptic
    ///     feedback.
    ///     If no audio clip is specified, the fallback clip type will always be used.
    /// </summary>
    [Serializable]
    public class UxrHapticClip
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]               private AudioClip         _clip;
        [SerializeField] [Range(0, 1)] private float             _clipAmplitude           = 1.0f;
        [SerializeField]               private UxrHapticMode     _hapticMode              = UxrHapticMode.Mix;
        [SerializeField]               private UxrHapticClipType _fallbackClipType        = UxrHapticClipType.None;
        [SerializeField] [Range(0, 1)] private float             _fallbackAmplitude       = 1.0f;
        [SerializeField]               private float             _fallbackDurationSeconds = -1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the primary <see cref="AudioClip" /> to use as source for vibration. If the device does not support
        ///     audio
        ///     clips as sources or this value is null, <see cref="FallbackClipType" /> will be used.
        /// </summary>
        public AudioClip Clip
        {
            get => _clip;
            set => _clip = value;
        }

        /// <summary>
        ///     Gets or sets the amplitude to play <see cref="Clip" />. Valid range is [0.0, 1.0].
        /// </summary>
        public float ClipAmplitude
        {
            get => _clipAmplitude;
            set => _clipAmplitude = value;
        }

        /// <summary>
        ///     Gets or sets whether to replace or mix the clip with any current haptic feedback being played.
        /// </summary>
        public UxrHapticMode HapticMode
        {
            get => _hapticMode;
            set => _hapticMode = value;
        }

        /// <summary>
        ///     Gets or sets the fallback clip: A value from a pre-defined set of procedurally generated haptic feedback clips. It
        ///     will be
        ///     used if the current device can't play <see cref="AudioClip" /> as haptics or <see cref="Clip" /> is not assigned.
        /// </summary>
        public UxrHapticClipType FallbackClipType
        {
            get => _fallbackClipType;
            set => _fallbackClipType = value;
        }

        /// <summary>
        ///     Gets or sets the amplitude to play the fallback clip (1.0f = use default).
        /// </summary>
        public float FallbackAmplitude
        {
            get => _fallbackAmplitude;
            set => _fallbackAmplitude = value;
        }

        /// <summary>
        ///     Gets or sets the duration in seconds of the fallback clip (negative = use predefined).
        /// </summary>
        public float FallbackDurationSeconds
        {
            get => _fallbackDurationSeconds;
            set => _fallbackDurationSeconds = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Public constructor.
        /// </summary>
        /// <param name="clip">The audio clip</param>
        /// <param name="fallbackClipType">The fallback clip if the primary audio clip is null</param>
        /// <param name="hapticMode">The haptic mixing mode</param>
        /// <param name="clipAmplitude">The amplitude of the audio clip</param>
        /// <param name="fallbackAmplitude">The amplitude of the fallback clip</param>
        /// <param name="fallbackDurationSeconds">The duration in seconds of the fallback clip (negative = use predefined)</param>
        public UxrHapticClip(AudioClip         clip                    = null,
                             UxrHapticClipType fallbackClipType        = UxrHapticClipType.None,
                             UxrHapticMode     hapticMode              = UxrHapticMode.Mix,
                             float             clipAmplitude           = 1.0f,
                             float             fallbackAmplitude       = 1.0f,
                             float             fallbackDurationSeconds = -1.0f)
        {
            Clip                    = clip;
            FallbackClipType        = fallbackClipType;
            HapticMode              = hapticMode;
            ClipAmplitude           = clipAmplitude;
            FallbackAmplitude       = fallbackAmplitude;
            FallbackDurationSeconds = fallbackDurationSeconds;
        }

        #endregion
    }
}