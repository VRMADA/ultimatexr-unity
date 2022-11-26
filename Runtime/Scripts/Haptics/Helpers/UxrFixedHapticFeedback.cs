// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFixedHapticFeedback.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Haptics.Helpers
{
    /// <summary>
    ///     Component that will send haptic feedback while enabled.
    /// </summary>
    public class UxrFixedHapticFeedback : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]               private UxrHandSide   _handSide      = UxrHandSide.Left;
        [SerializeField]               private UxrHapticMode _hapticMixMode = UxrHapticMode.Mix;
        [SerializeField] [Range(0, 1)] private float         _amplitude     = 0.5f;
        [SerializeField]               private float         _frequency     = 100.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the target hand.
        /// </summary>
        public UxrHandSide HandSide
        {
            get => _handSide;
            set => _handSide = value;
        }

        /// <summary>
        ///     Gets or sets the haptic playback mix mode.
        /// </summary>
        public UxrHapticMode HapticMixMode
        {
            get => _hapticMixMode;
            set => _hapticMixMode = value;
        }

        /// <summary>
        ///     Gets or sets the haptic signal amplitude.
        /// </summary>
        public float Amplitude
        {
            get => _amplitude;
            set => _amplitude = value;
        }

        /// <summary>
        ///     Gets or sets the haptic signal frequency.
        /// </summary>
        public float Frequency
        {
            get => _frequency;
            set => _frequency = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Starts the haptic coroutine.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _hapticsCoroutine = StartCoroutine(HapticsCoroutine());
        }

        /// <summary>
        ///     Stops the haptic coroutine.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            StopCoroutine(_hapticsCoroutine);
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that sends continuous, fixed, haptic feedback to the target controller.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator HapticsCoroutine()
        {
            yield return null;

            while (true)
            {
                if (isActiveAndEnabled && UxrAvatar.LocalAvatar)
                {
                    SendHapticClip(_handSide);
                }

                yield return new WaitForSeconds(SampleDurationSeconds);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sends the haptic feedback.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        private void SendHapticClip(UxrHandSide handSide)
        {
            UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _frequency, _amplitude, SampleDurationSeconds);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Controls the duration of each individual sample sent, and thus the iteration of each loop.
        /// </summary>
        private const float SampleDurationSeconds = 0.1f;

        private Coroutine _hapticsCoroutine;

        #endregion
    }
}