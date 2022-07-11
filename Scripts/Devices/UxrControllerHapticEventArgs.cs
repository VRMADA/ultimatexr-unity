// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHapticEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Haptics;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Wraps information about a haptic request event.
    /// </summary>
    public class UxrControllerHapticEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the haptic clip if the event is <see cref="UxrHapticEventType.Clip" />.
        /// </summary>
        public UxrHapticClip HapticClip { get; }

        /// <summary>
        ///     Gets the haptic feedback raw frequency if the event is <see cref="UxrHapticEventType.Raw" />.
        /// </summary>
        public float Frequency { get; }

        /// <summary>
        ///     Gets the haptic feedback raw amplitude if the event is <see cref="UxrHapticEventType.Raw" />.
        /// </summary>
        public float Amplitude { get; }

        /// <summary>
        ///     Gets the haptic feedback duration in seconds if the event is <see cref="UxrHapticEventType.Raw" />.
        /// </summary>
        public float DurationSeconds { get; }

        /// <summary>
        ///     Gets the haptic feedback playback mode.
        /// </summary>
        public UxrHapticMode HapticMode { get; }

        /// <summary>
        ///     Gets the haptic feedback target hand.
        /// </summary>
        public UxrHandSide HandSide { get; private set; }

        /// <summary>
        ///     Gets the haptic event type.
        /// </summary>
        public UxrHapticEventType HapticEventType { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor that registers a <see cref="UxrHapticEventType.Clip" /> event.
        /// </summary>
        /// <param name="handSide">Haptic feedback target hand</param>
        /// <param name="clip">Clip that was requested</param>
        public UxrControllerHapticEventArgs(UxrHandSide handSide, UxrHapticClip clip)
        {
            HandSide        = handSide;
            HapticEventType = UxrHapticEventType.Clip;
            HapticClip      = clip;
        }

        /// <summary>
        ///     Constructor that registers a <see cref="UxrHapticEventType.Raw" /> event.
        /// </summary>
        /// <param name="handSide">Haptic feedback target hand</param>
        /// <param name="frequency">Vibration frequency</param>
        /// <param name="amplitude">Vibration amplitude</param>
        /// <param name="durationSeconds">Feedback duration in seconds</param>
        /// <param name="hapticMode">Haptic mode: mix or replace</param>
        public UxrControllerHapticEventArgs(UxrHandSide handSide, float frequency, float amplitude, float durationSeconds, UxrHapticMode hapticMode)
        {
            HapticEventType = UxrHapticEventType.Raw;
            HandSide        = handSide;
            Frequency       = frequency;
            Amplitude       = amplitude;
            DurationSeconds = durationSeconds;
            HapticMode      = hapticMode;
        }

        /// <summary>
        ///     Default constructor is private
        /// </summary>
        private UxrControllerHapticEventArgs()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Generates a <see cref="UxrHapticEventType.Stop" /> event for the given hand
        /// </summary>
        /// <param name="handSide">Haptic feedback target hand</param>
        public static UxrControllerHapticEventArgs GetHapticStopEvent(UxrHandSide handSide)
        {
            return new UxrControllerHapticEventArgs
                   {
                               HapticEventType = UxrHapticEventType.Stop,
                               HandSide        = handSide
                   };
        }

        #endregion
    }
}