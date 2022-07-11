// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControlFeedback.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Haptics;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;

namespace UltimateXR.UI
{
    /// <summary>
    ///     Defines sound and haptic feedback for pressing events. Each <see cref="UxrControlInput" />, for instance, has a
    ///     <see cref="UxrControlFeedback" /> for each of its click/down/up events.
    /// </summary>
    [Serializable]
    public class UxrControlFeedback
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]               private UxrHapticClip _hapticClip = new UxrHapticClip();
        [SerializeField]               private AudioClip     _audioClip;
        [SerializeField] [Range(0, 1)] private float         _audioVolume = 1.0f;
        [SerializeField]               private bool          _useAudio3D  = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the haptic clip.
        /// </summary>
        public UxrHapticClip HapticClip
        {
            get => _hapticClip;
            set => _hapticClip = value;
        }

        /// <summary>
        ///     Gets or sets the audio clip.
        /// </summary>
        public AudioClip AudioClip
        {
            get => _audioClip;
            set => _audioClip = value;
        }

        /// <summary>
        ///     Gets or sets the audio volume.
        /// </summary>
        public float AudioVolume
        {
            get => _audioVolume;
            set => _audioVolume = value;
        }

        /// <summary>
        ///     Gets or sets whether to use 3D audio.
        /// </summary>
        public bool UseAudio3D
        {
            get => _useAudio3D;
            set => _useAudio3D = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrControlFeedback()
        {
        }

        /// <summary>
        ///     Constructor allowing to define the haptic clip.
        /// </summary>
        /// <param name="hapticClip">Haptic clip to play on the event</param>
        public UxrControlFeedback(UxrHapticClip hapticClip)
        {
            HapticClip = hapticClip;
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Subtle click
        /// </summary>
        public static UxrControlFeedback FeedbackDown = new UxrControlFeedback(new UxrHapticClip(null, UxrHapticClipType.Click, UxrHapticMode.Mix, 1.0f, 0.2f));

        /// <summary>
        ///     No feedback
        /// </summary>
        public static UxrControlFeedback FeedbackUp = new UxrControlFeedback(new UxrHapticClip());

        /// <summary>
        ///     Regular click
        /// </summary>
        public static UxrControlFeedback FeedbackClick = new UxrControlFeedback(new UxrHapticClip(null, UxrHapticClipType.Click, UxrHapticMode.Mix, 1.0f, 0.6f));

        #endregion
    }
}