// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAudioSample.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Audio
{
    /// <summary>
    ///     Describes an audio clip that can be played and its parameters.
    /// </summary>
    [Serializable]
    public class UxrAudioSample
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]               private AudioClip _clip;
        [SerializeField] [Range(0, 1)] private float     _volume = 1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the audio clip.
        /// </summary>
        public AudioClip Clip
        {
            get => _clip;
            set => _clip = value;
        }

        /// <summary>
        ///     Gets or sets the volume used when playing the clip.
        /// </summary>
        public float Volume
        {
            get => _volume;
            set => _volume = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrAudioSample()
        {
            Clip   = null;
            Volume = 1.0f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Plays the audio in a given position in space.
        /// </summary>
        /// <param name="position">World space coordinates where the audio will be played.</param>
        public void Play(Vector3 position)
        {
            if (Clip != null)
            {
                AudioSource.PlayClipAtPoint(Clip, position, Volume);
            }
        }

        #endregion
    }
}