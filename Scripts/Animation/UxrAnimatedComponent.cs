// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation
{
    /// <summary>
    ///     Base class to create components that animate properties.
    ///     Animation components should support two main ways of usage:
    ///     <list type="bullet">
    ///         <item>Adding and setting up component using Unity's editor.</item>
    ///         <item>Adding and setting up component through scripting at runtime.</item>
    ///     </list>
    /// </summary>
    /// <typeparam name="T">Type of animated component</typeparam>
    public abstract class UxrAnimatedComponent<T> : UxrComponent where T : UxrAnimatedComponent<T>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAnimationMode         _animationMode = UxrAnimationMode.None;
        [SerializeField] private float                    _valueSpeedDurationSeconds;
        [SerializeField] private Vector4                  _valueSpeed;
        [SerializeField] private Vector4                  _valueStart;
        [SerializeField] private Vector4                  _valueEnd;
        [SerializeField] private Vector4                  _valueDisabled;
        [SerializeField] private UxrInterpolationSettings _interpolationSettings;
        [SerializeField] private float                    _valueNoiseTimeStart;
        [SerializeField] private float                    _valueNoiseDuration;
        [SerializeField] private Vector4                  _valueNoiseValueStart;
        [SerializeField] private Vector4                  _valueNoiseValueEnd;
        [SerializeField] private Vector4                  _valueNoiseValueMin;
        [SerializeField] private Vector4                  _valueNoiseValueMax;
        [SerializeField] private Vector4                  _valueNoiseFrequency;
        [SerializeField] private Vector4                  _valueNoiseOffset;
        [SerializeField] private bool                     _useUnscaledTime;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when the animation finished.
        /// </summary>
        public event Action<T> Finished;

        /// <summary>
        ///     Gets the current animation time in seconds. The animation time is the scaled or unscaled time relative to the time
        ///     the component was enabled.
        /// </summary>
        public float AnimationTime => CurrentTime - _startTime;

        /// <summary>
        ///     Gets the animation mode.
        /// </summary>
        public UxrAnimationMode AnimationMode
        {
            get => _animationMode;
            protected set => _animationMode = value;
        }

        /// <summary>
        ///     Gets whether the animation finished.
        /// </summary>
        public bool HasFinished { get; private set; }

        /// <summary>
        ///     Gets or sets the increment per second when the animation mode is set to <see cref="UxrAnimationMode.Speed" />.
        /// </summary>
        public Vector4 Speed
        {
            get => _valueSpeed;
            set => _valueSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the animation duration in seconds when the animation mode is set to
        ///     <see cref="UxrAnimationMode.Speed" />.
        ///     Durations of 0 or less than 0 will be considered as infinite duration.
        /// </summary>
        public float SpeedDurationSeconds
        {
            get => _valueSpeedDurationSeconds;
            set => _valueSpeedDurationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets the start value when the animation mode is set to <see cref="UxrAnimationMode.Interpolate" />.
        /// </summary>
        public Vector4 InterpolatedValueStart
        {
            get => _valueStart;
            set => _valueStart = value;
        }

        /// <summary>
        ///     Gets or sets the end value when the animation mode is set to <see cref="UxrAnimationMode.Interpolate" />.
        /// </summary>
        public Vector4 InterpolatedValueEnd
        {
            get => _valueEnd;
            set => _valueEnd = value;
        }

        /// <summary>
        ///     Gets or sets the value to set when the component is disabled, when the animation mode is set to
        ///     <see cref="UxrAnimationMode.Interpolate" />.
        /// </summary>
        public Vector4 InterpolatedValueWhenDisabled
        {
            get => _valueDisabled;
            set => _valueDisabled = value;
        }

        /// <summary>
        ///     Gets or sets the interpolation settings when the animation mode is set to
        ///     <see cref="UxrAnimationMode.Interpolate" />.
        /// </summary>
        public UxrInterpolationSettings InterpolationSettings
        {
            get => _interpolationSettings;
            set => _interpolationSettings = value;
        }

        /// <summary>
        ///     Gets or sets the noise min value when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseValueMin
        {
            get => _valueNoiseValueMin;
            set => _valueNoiseValueMin = value;
        }

        /// <summary>
        ///     Gets or sets the noise max value when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseValueMax
        {
            get => _valueNoiseValueMax;
            set => _valueNoiseValueMax = value;
        }

        /// <summary>
        ///     Gets or sets the start time when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public float NoiseTimeStart
        {
            get => _valueNoiseTimeStart;
            set => _valueNoiseTimeStart = value;
        }

        /// <summary>
        ///     Gets or sets the animation duration in seconds when the animation mode is set to
        ///     <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public float NoiseDurationSeconds
        {
            get => _valueNoiseDuration;
            set => _valueNoiseDuration = value;
        }

        /// <summary>
        ///     Gets or sets the start value when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseValueStart
        {
            get => _valueNoiseValueStart;
            set => _valueNoiseValueStart = value;
        }

        /// <summary>
        ///     Gets or sets the end value when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseValueEnd
        {
            get => _valueNoiseValueEnd;
            set => _valueNoiseValueEnd = value;
        }

        /// <summary>
        ///     Gets or sets the noise frequency when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseFrequency
        {
            get => _valueNoiseFrequency;
            set => _valueNoiseFrequency = value;
        }

        /// <summary>
        ///     Gets or sets the noise offset when the animation mode is set to <see cref="UxrAnimationMode.Noise" />.
        /// </summary>
        public Vector4 NoiseOffset
        {
            get => _valueNoiseOffset;
            set => _valueNoiseOffset = value;
        }

        /// <summary>
        ///     Gets or sets whether to use the unscaled time (<see cref="Time.unscaledTime" /> instead of <see cref="Time.time" />
        ///     .
        /// </summary>
        public bool UseUnscaledTime
        {
            get => _useUnscaledTime;
            set => _useUnscaledTime = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Stops the animation on an object if it has an <see cref="UxrAnimatedComponent{T}" /> component currently attached.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <param name="restoreOriginal">Whether to reset the animated component to the state before the animation started</param>
        public static void Stop(GameObject gameObject, bool restoreOriginal = true)
        {
            T anim = gameObject.GetComponent<T>();

            if (anim)
            {
                anim.Stop(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the animation on an object if it has an <see cref="UxrAnimatedComponent{T}" /> component currently attached.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset the animated component to the state before the animation started</param>
        public void Stop(bool restoreOriginal = true)
        {
            HasFinished = true;

            if (restoreOriginal)
            {
                RestoreOriginalValue();
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called each time the object is enabled. Reset timer and set the curve state to unfinished.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            StartTimer();
        }

        /// <summary>
        ///     Called each time the object is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (AnimationMode == UxrAnimationMode.Interpolate)
            {
                SetParameterValue(InterpolatedValueWhenDisabled);
            }
            else if (AnimationMode == UxrAnimationMode.Noise)
            {
                SetParameterValue(NoiseValueEnd);
            }
        }

        /// <summary>
        ///     Updates the animation.
        /// </summary>
        private void Update()
        {
            if (HasFinished)
            {
                return;
            }

            if (AnimationMode == UxrAnimationMode.Speed)
            {
                Vector4 value = GetParameterValue();
                value += Speed * (_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                SetParameterValue(value);

                if (_valueSpeedDurationSeconds > 0.0f)
                {
                    if (AnimationTime >= _valueSpeedDurationSeconds)
                    {
                        HasFinished = true;
                        OnFinished(this as T);
                    }
                }
            }
            else if (AnimationMode == UxrAnimationMode.Interpolate)
            {
                Vector4 value = UxrInterpolator.Interpolate(_valueStart, _valueEnd, AnimationTime, InterpolationSettings);
                SetParameterValue(value);

                if (InterpolationSettings.CheckInterpolationHasFinished(AnimationTime))
                {
                    SetParameterValue(_valueEnd);
                    HasFinished = true;
                    OnFinished(this as T);
                }
            }
            else if (AnimationMode == UxrAnimationMode.Noise)
            {
                if (AnimationTime < NoiseTimeStart)
                {
                    SetParameterValue(NoiseValueStart);
                }
                else if (AnimationTime > NoiseTimeStart + NoiseDurationSeconds)
                {
                    SetParameterValue(NoiseValueEnd);
                    HasFinished = true;
                    OnFinished(this as T);
                }
                else
                {
                    float tX = Mathf.PerlinNoise(_valueNoiseOffset[0] + AnimationTime * _valueNoiseFrequency[0], _valueNoiseOffset[0]);
                    float tY = Mathf.PerlinNoise(_valueNoiseOffset[1] + AnimationTime * _valueNoiseFrequency[1], _valueNoiseOffset[1]);
                    float tZ = Mathf.PerlinNoise(_valueNoiseOffset[2] + AnimationTime * _valueNoiseFrequency[2], _valueNoiseOffset[2]);
                    float tW = Mathf.PerlinNoise(_valueNoiseOffset[3] + AnimationTime * _valueNoiseFrequency[3], _valueNoiseOffset[3]);

                    SetParameterValue(new Vector4(Mathf.Lerp(_valueNoiseValueMin[0], _valueNoiseValueMax[0], tX),
                                                  Mathf.Lerp(_valueNoiseValueMin[1], _valueNoiseValueMax[1], tY),
                                                  Mathf.Lerp(_valueNoiseValueMin[2], _valueNoiseValueMax[2], tZ),
                                                  Mathf.Lerp(_valueNoiseValueMin[3], _valueNoiseValueMax[3], tW)));
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the animation finished.
        /// </summary>
        /// <param name="anim">Animation that finished</param>
        protected virtual void OnFinished(T anim)
        {
            Finished?.Invoke(anim);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Converts a float value to a Vector4. Internally Vector4 values are used for everything but some animations only
        ///     require to store a float value. The x component will be used to store the value.
        /// </summary>
        /// <param name="v">Float value to store</param>
        /// <returns>Vector4 storing the float value in the x component.</returns>
        protected static Vector4 ToVector4(float v)
        {
            return new Vector4(v, 0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        ///     Restores the animated component to the state before the animation started.
        /// </summary>
        protected abstract void RestoreOriginalValue();

        /// <summary>
        ///     Gets the current parameter value
        /// </summary>
        /// <returns>
        ///     Vector4 containing the value. This value may not use all components depending on which parameter it is
        ///     animating.
        /// </returns>
        protected abstract Vector4 GetParameterValue();

        /// <summary>
        ///     Sets the parameter value
        /// </summary>
        /// <param name="value">
        ///     Vector4 containing the value. This value may not use all components depending on which parameter it is animating.
        /// </param>
        protected abstract void SetParameterValue(Vector4 value);

        /// <summary>
        ///     (Re)Starts the animation timer.
        /// </summary>
        protected void StartTimer()
        {
            HasFinished = false;
            _startTime  = CurrentTime;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the current time in seconds. It computes the correct time, either <see cref="Time.unscaledTime" />
        ///     or <see cref="Time.time" />, depending on the animation configuration.
        /// </summary>
        private float CurrentTime
        {
            get
            {
                if (InterpolationSettings != null && AnimationMode == UxrAnimationMode.Interpolate)
                {
                    return InterpolationSettings.UseUnscaledTime ? Time.unscaledTime : Time.time;
                }
                return _useUnscaledTime ? Time.unscaledTime : Time.time;
            }
        }

        private float _startTime;

        #endregion
    }
}