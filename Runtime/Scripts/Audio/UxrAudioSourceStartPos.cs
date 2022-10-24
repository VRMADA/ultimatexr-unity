// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAudioSourceStartPos.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Audio
{
    /// <summary>
    ///     Component that allows to make an <see cref="AudioSource" /> start playing at a fixed time or a random time. It
    ///     can be used for example to scatter multiple environment audio sources around that share the same loop and make them
    ///     start playing at different times.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UxrAudioSourceStartPos : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool  _random;
        [SerializeField] private float _startTime;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the target <see cref="AudioSource" />.
        /// </summary>
        public AudioSource AudioSourceTarget => GetCachedComponent<AudioSource>();

        /// <summary>
        ///     Gets or sets whether to start playing from a random position in the audio interval.
        /// </summary>
        public bool UseRandomStartTime
        {
            get => _random;
            set => _random = value;
        }

        /// <summary>
        ///     Gets or sets the start time position to start playing from. <see cref="UseRandomStartTime" /> needs to be false to
        ///     use it.
        /// </summary>
        public float StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Sets the <see cref="AudioSource" /> play time using the current parameters.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (UseRandomStartTime)
            {
                AudioSourceTarget.time = Random.Range(0.0f, AudioSourceTarget.clip.length);
            }
            else
            {
                AudioSourceTarget.time = StartTime;
            }
        }

        #endregion
    }
}