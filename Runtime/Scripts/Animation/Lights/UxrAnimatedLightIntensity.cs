// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedLightIntensity.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.Lights
{
    /// <summary>
    ///     Component that allows to animate a light's intensity.
    /// </summary>
    public class UxrAnimatedLightIntensity : UxrAnimatedComponent<UxrAnimatedLightIntensity>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Light _light;

        #endregion

        #region Public Types & Data

        /// <inheritdoc cref="UxrAnimatedComponent{T}.Speed" />
        public new float Speed
        {
            get => base.Speed.x;
            set => base.Speed = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.InterpolatedValueStart" />
        public new float InterpolatedValueStart
        {
            get => base.InterpolatedValueStart.x;
            set => base.InterpolatedValueStart = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.InterpolatedValueEnd" />
        public new float InterpolatedValueEnd
        {
            get => base.InterpolatedValueEnd.x;
            set => base.InterpolatedValueEnd = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.InterpolatedValueWhenDisabled" />
        public new float InterpolatedValueWhenDisabled
        {
            get => base.InterpolatedValueWhenDisabled.x;
            set => base.InterpolatedValueWhenDisabled = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseValueMin" />
        public new float NoiseValueMin
        {
            get => base.NoiseValueMin.x;
            set => base.NoiseValueMin = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseValueMax" />
        public new float NoiseValueMax
        {
            get => base.NoiseValueMax.x;
            set => base.NoiseValueMax = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseValueStart" />
        public new float NoiseValueStart
        {
            get => base.NoiseValueStart.x;
            set => base.NoiseValueStart = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseValueEnd" />
        public new float NoiseValueEnd
        {
            get => base.NoiseValueEnd.x;
            set => base.NoiseValueEnd = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseFrequency" />
        public new float NoiseFrequency
        {
            get => base.NoiseFrequency.x;
            set => base.NoiseFrequency = ToVector4(value);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.NoiseOffset" />
        public new float NoiseOffset
        {
            get => base.NoiseOffset.x;
            set => base.NoiseOffset = ToVector4(value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts an animation at a constant speed
        /// </summary>
        /// <param name="light">The light component to apply the animation to</param>
        /// <param name="speed">
        ///     The animation speed. For int/float values use .x, for Vector2 use x and y. For
        ///     Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="useUnscaledTime">
        ///     If it is true then Time.unscaledTime will be used
        ///     to count seconds. By default it is false meaning Time.time will be used instead.
        ///     Time.time is affected by Time.timeScale which in many cases is used for application pauses
        ///     or bullet-time effects, while Time.unscaledTime is not.
        /// </param>
        /// <returns>Animation component</returns>
        public static UxrAnimatedLightIntensity Animate(Light light, float speed, bool useUnscaledTime = false)
        {
            UxrAnimatedLightIntensity component = light.gameObject.GetOrAddComponent<UxrAnimatedLightIntensity>();

            if (component)
            {
                component._light          = light;
                component.AnimationMode   = UxrAnimationMode.Speed;
                component.Speed           = speed;
                component.UseUnscaledTime = useUnscaledTime;
                component.StartTimer();
            }

            return component;
        }

        /// <summary>
        ///     Starts an animation using an interpolation curve
        /// </summary>
        /// <param name="light">The light component to apply the animation to</param>
        /// <param name="startValue">The start intensity value</param>
        /// <param name="endValue">The end intensity value</param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>Animation component</returns>
        public static UxrAnimatedLightIntensity AnimateInterpolation(Light light, float startValue, float endValue, UxrInterpolationSettings settings, Action finishedCallback = null)
        {
            UxrAnimatedLightIntensity component = light.gameObject.GetOrAddComponent<UxrAnimatedLightIntensity>();

            if (component)
            {
                component._light                 = light;
                component.AnimationMode          = UxrAnimationMode.Interpolate;
                component.InterpolatedValueStart = startValue;
                component.InterpolatedValueEnd   = endValue;
                component.InterpolationSettings  = settings;
                component._finishedCallback      = finishedCallback;
                component.StartTimer();
            }

            return component;
        }

        /// <summary>
        ///     Starts an animation using noise.
        /// </summary>
        /// <param name="light">The light component to apply the animation to</param>
        /// <param name="noiseTimeStart">The time in seconds the noise will start (Time.time or Time.unscaledTime value)</param>
        /// <param name="noiseTimeDuration">The duration in seconds of the noise animation</param>
        /// <param name="noiseValueStart">The start intensity value</param>
        /// <param name="noiseValueEnd">The end intensity value</param>
        /// <param name="noiseValueMin">The minimum intensity value for the noise</param>
        /// <param name="noiseValueMax">The maximum intensity value for the noise</param>
        /// <param name="noiseValueFrequency">The noise frequency</param>
        /// <param name="useUnscaledTime">If true it will use Time.unscaledTime, if false it will use Time.time</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>Animation component</returns>
        public static UxrAnimatedLightIntensity AnimateNoise(Light  light,
                                                             float  noiseTimeStart,
                                                             float  noiseTimeDuration,
                                                             float  noiseValueStart,
                                                             float  noiseValueEnd,
                                                             float  noiseValueMin,
                                                             float  noiseValueMax,
                                                             float  noiseValueFrequency,
                                                             bool   useUnscaledTime  = false,
                                                             Action finishedCallback = null)
        {
            UxrAnimatedLightIntensity component = light.gameObject.GetOrAddComponent<UxrAnimatedLightIntensity>();

            if (component)
            {
                component._light               = light;
                component.AnimationMode        = UxrAnimationMode.Noise;
                component.NoiseTimeStart       = noiseTimeStart;
                component.NoiseDurationSeconds = noiseTimeDuration;
                component.NoiseValueStart      = noiseValueStart;
                component.NoiseValueEnd        = noiseValueEnd;
                component.NoiseValueMin        = noiseValueMin;
                component.NoiseValueMax        = noiseValueMax;
                component.NoiseFrequency       = noiseValueFrequency;
                component.UseUnscaledTime      = useUnscaledTime;
                component._finishedCallback    = finishedCallback;
                component.StartTimer();
            }

            return component;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stores the initial light intensity
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _initialIntensity = _light != null ? _light.intensity : 0.0f;
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc cref="UxrAnimatedComponent{T}.OnFinished" />
        protected override void OnFinished(UxrAnimatedLightIntensity anim)
        {
            base.OnFinished(anim);
            _finishedCallback?.Invoke();
        }

        #endregion

        #region Protected Overrides UxrAnimatedComponent<UxrAnimatedLightIntensity>

        /// <inheritdoc cref="UxrAnimatedComponent{T}.RestoreOriginalValue" />
        protected override void RestoreOriginalValue()
        {
            if (_light != null)
            {
                _light.intensity = _initialIntensity;
            }
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.GetParameterValue" />
        protected override Vector4 GetParameterValue()
        {
            return ToVector4(_light != null ? _light.intensity : 0.0f);
        }

        /// <inheritdoc cref="UxrAnimatedComponent{T}.SetParameterValue" />
        protected override void SetParameterValue(Vector4 value)
        {
            if (_light != null)
            {
                _light.intensity = value.x;
            }
        }

        #endregion

        #region Private Types & Data

        private float  _initialIntensity;
        private Action _finishedCallback;

        #endregion
    }
}