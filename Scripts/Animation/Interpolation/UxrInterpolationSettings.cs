// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInterpolationSettings.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Describes the different parameters of an interpolation.
    /// </summary>
    [Serializable]
    public class UxrInterpolationSettings
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float       _durationSeconds;
        [SerializeField] private float       _delaySeconds;
        [SerializeField] private UxrEasing   _easing;
        [SerializeField] private UxrLoopMode _loopMode;
        [SerializeField] private float       _loopedDurationSeconds;
        [SerializeField] private bool        _useUnscaledTime;
        [SerializeField] private bool        _delayUsingEndValue;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the interpolation duration in seconds. In looped interpolations it tells the duration of a single
        ///     loop.
        /// </summary>
        public float DurationSeconds
        {
            get => _durationSeconds;
            set => _durationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets the interpolation delay in seconds. The delay is usually relative to the time the object that it uses
        ///     was enabled and specifies an initial waiting time before the actual interpolation will start.
        /// </summary>
        public float DelaySeconds
        {
            get => _delaySeconds;
            set => _delaySeconds = value;
        }

        /// <summary>
        ///     Gets or sets the easing function to use by the interpolation.
        /// </summary>
        public UxrEasing Easing
        {
            get => _easing;
            set => _easing = value;
        }

        /// <summary>
        ///     Gets or sets if and how to loop the interpolation.
        /// </summary>
        public UxrLoopMode LoopMode
        {
            get => _loopMode;
            set => _loopMode = value;
        }

        /// <summary>
        ///     Gets or sets the total animation duration in interpolations that use looping. The duration of a single loop is
        ///     described by <see cref="DurationSeconds" />. A negative value tells to loop indefinitely.
        /// </summary>
        public float LoopedDurationSeconds
        {
            get => _loopedDurationSeconds;
            set => _loopedDurationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets whether to use unscaled time (<see cref="Time.unscaledTime" />) or regular time
        ///     <see cref="Time.time" /> when interpolating.
        ///     Regular time is affected by <see cref="Time.timeScale" />, which is normally used to pause the application or
        ///     simulate slow motion effects.
        /// </summary>
        public bool UseUnscaledTime
        {
            get => _useUnscaledTime;
            set => _useUnscaledTime = value;
        }

        /// <summary>
        ///     Gets or sets whether to use the interpolation end value during the initial delay, if there is a
        ///     <see cref="DelaySeconds" /> value specified.
        ///     By default the interpolation uses the start value during the initial delay.
        /// </summary>
        public bool DelayUsingEndValue
        {
            get => _delayUsingEndValue;
            set => _delayUsingEndValue = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor
        /// </summary>
        public UxrInterpolationSettings()
        {
            _durationSeconds       = 0.0f;
            _delaySeconds          = 0.0f;
            _easing                = UxrEasing.Linear;
            _loopMode              = UxrLoopMode.None;
            _loopedDurationSeconds = -1.0f;
            _useUnscaledTime       = false;
            _delayUsingEndValue    = false;
        }

        /// <summary>
        ///     UxrInterpolationSettings constructor.
        /// </summary>
        /// <param name="durationSeconds">
        ///     The duration in seconds the interpolation will be applied. If a loopMode was specified, it tells the duration of a
        ///     single loop.
        /// </param>
        /// <param name="delaySeconds">The delay in seconds before the interpolation</param>
        /// <param name="easing">The type of interpolation used.</param>
        /// <param name="loopMode">The type of looping used.</param>
        /// <param name="loopedDurationSeconds">
        ///     If loopMode is not LoopMode.None this parameter will tell how many seconds the total duration of
        ///     the interpolation will last and durationSeconds will tell the duration of each loop. A negative value means it will
        ///     loop forever.
        /// </param>
        /// <param name="useUnscaledTime">
        ///     Tells whether to use the real timer value <see cref="Time.unscaledTime" /> (true) or the scaled
        ///     <see cref="Time.time" /> value (false) which is affected by <see cref="Time.timeScale" />.
        /// </param>
        /// <param name="delayUsingEndValue">
        ///     Tells whether to use the interpolation end value during the delay, if there is a
        ///     <paramref name="delaySeconds" /> specified. By default it's false, which means the interpolation start value is
        ///     used during the delay.
        /// </param>
        public UxrInterpolationSettings(float       durationSeconds,
                                        float       delaySeconds          = 0.0f,
                                        UxrEasing   easing                = UxrEasing.Linear,
                                        UxrLoopMode loopMode              = UxrLoopMode.None,
                                        float       loopedDurationSeconds = -1.0f,
                                        bool        useUnscaledTime       = false,
                                        bool        delayUsingEndValue    = false)
        {
            _durationSeconds       = durationSeconds;
            _delaySeconds          = delaySeconds;
            _easing                = easing;
            _loopMode              = loopMode;
            _loopedDurationSeconds = loopedDurationSeconds;
            _useUnscaledTime       = useUnscaledTime;
            _delayUsingEndValue    = delayUsingEndValue;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the T value used for linear interpolations given a time value.
        /// </summary>
        /// <param name="time">Time value</param>
        /// <returns>The interpolation t value required to linearly interpolate using the current parameters</returns>
        public float GetInterpolationFactor(float time)
        {
            return UxrInterpolator.Interpolate(0.0f, 1.0f, _durationSeconds, _delaySeconds, time, _easing, _loopMode, _loopedDurationSeconds, _delayUsingEndValue);
        }

        /// <summary>
        ///     Checks if the given time has surpassed the interpolation duration.
        /// </summary>
        /// <param name="time">Time value</param>
        /// <returns>Boolean telling whether the interpolation has finished</returns>
        public bool CheckInterpolationHasFinished(float time)
        {
            if (LoopMode == UxrLoopMode.None && time > DelaySeconds + DurationSeconds)
            {
                return true;
            }

            if (LoopMode != UxrLoopMode.None && time > DelaySeconds + LoopedDurationSeconds && LoopedDurationSeconds >= 0.0f)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}