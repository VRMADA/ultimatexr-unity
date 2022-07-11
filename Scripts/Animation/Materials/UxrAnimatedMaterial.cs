// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedMaterial.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     Component that allows to animate material properties.
    /// </summary>
    public class UxrAnimatedMaterial : UxrAnimatedComponent<UxrAnimatedMaterial>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                     _animateSelf = true;
        [SerializeField] private GameObject               _targetGameObject;
        [SerializeField] private int                      _materialSlot;
        [SerializeField] private UxrMaterialMode          _materialMode        = UxrMaterialMode.InstanceOnly;
        [SerializeField] private bool                     _restoreWhenFinished = true;
        [SerializeField] private UxrMaterialParameterType _parameterType       = UxrMaterialParameterType.Vector4;
        [SerializeField] private string                   _parameterName       = "";

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     The default blink frequency
        /// </summary>
        public const float DefaultBlinkFrequency = 3.0f;

        /// <summary>
        ///     Gets or sets if the original material value should be restored when finished.
        /// </summary>
        public bool RestoreWhenFinished
        {
            get => _restoreWhenFinished;
            set => _restoreWhenFinished = value;
        }

        /// <summary>
        ///     Gets or sets whether the animation will be applied to the GameObject where the component is, or an external one.
        /// </summary>
        public bool AnimateSelf
        {
            get => _animateSelf;
            set => _animateSelf = value;
        }

        /// <summary>
        ///     Gets or sets the target GameObject when <see cref="AnimateSelf" /> is true.
        /// </summary>
        public GameObject TargetGameObject
        {
            get => _targetGameObject;
            set => _targetGameObject = value;
        }

        /// <summary>
        ///     Gets or sets the material slot to apply the material animation to.
        /// </summary>
        public int MaterialSlot
        {
            get => _materialSlot;
            set => _materialSlot = value;
        }

        /// <summary>
        ///     Gets or sets the material mode, whether to use the instanced material or the shared material.
        /// </summary>
        public UxrMaterialMode MaterialMode
        {
            get => _materialMode;
            set => _materialMode = value;
        }

        /// <summary>
        ///     Gets or sets the material's parameter type.
        /// </summary>
        public UxrMaterialParameterType ParameterType
        {
            get => _parameterType;
            set => _parameterType = value;
        }

        /// <summary>
        ///     Gets or sets the material's parameter name.
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts an animation at a constant speed
        /// </summary>
        /// <param name="gameObject">The GameObject with the material to apply the animation to</param>
        /// <param name="materialSlot">The renderer material slot where the material is</param>
        /// <param name="materialMode">
        ///     The material mode. Use instance to animate the material of a single object,
        ///     use shared to also affect all other objects that share the same material
        /// </param>
        /// <param name="parameterType">Selects the type of the parameter to animate</param>
        /// <param name="parameterName">
        ///     Selects the name of the parameter to animate.
        ///     This name is the name in the shader, not in the inspector!
        /// </param>
        /// <param name="speed">
        ///     The animation speed. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="durationSeconds">
        ///     Duration in seconds of the animation. Use a negative value to keep updating until stopped
        ///     manually.
        /// </param>
        /// <param name="useUnscaledTime">
        ///     If it is true then Time.unscaledTime will be used
        ///     to count seconds. By default it is false meaning Time.time will be used instead.
        ///     Time.time is affected by Time.timeScale which in many cases is used for application pauses
        ///     or bullet-time effects, while Time.unscaledTime is not.
        /// </param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedMaterial Animate(GameObject               gameObject,
                                                  int                      materialSlot,
                                                  UxrMaterialMode          materialMode,
                                                  UxrMaterialParameterType parameterType,
                                                  string                   parameterName,
                                                  Vector4                  speed,
                                                  float                    durationSeconds  = -1.0f,
                                                  bool                     useUnscaledTime  = false,
                                                  Action                   finishedCallback = null)
        {
            UxrAnimatedMaterial component = gameObject.GetOrAddComponent<UxrAnimatedMaterial>();

            if (component)
            {
                component._restoreWhenFinished = false;
                component._animateSelf         = true;
                component._materialSlot        = materialSlot;
                component._materialMode        = materialMode;
                component._parameterType       = parameterType;
                component._parameterName       = parameterName;
                component.AnimationMode        = UxrAnimationMode.Speed;
                component.Speed                = speed;
                component.UseUnscaledTime      = useUnscaledTime;
                component.SpeedDurationSeconds = durationSeconds;
                component._finishedCallback    = finishedCallback;
                component.Initialize();
                component.StartTimer();
                
            }

            return component;
        }

        /// <summary>
        ///     Starts a material parameter animation using an interpolation curve
        /// </summary>
        /// <param name="gameObject">The GameObject with the material to apply the animation to</param>
        /// <param name="materialSlot">The renderer material slot where the material is</param>
        /// <param name="materialMode">
        ///     The material mode. Use instance to animate the material of a single object,
        ///     use shared to also affect all other objects that share the same material
        /// </param>
        /// <param name="parameterType">Selects the type of the parameter to animate</param>
        /// <param name="parameterName">
        ///     Selects the name of the parameter to animate.
        ///     This name is the name in the shader, not in the inspector!
        /// </param>
        /// <param name="startValue">
        ///     The start value. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="endValue">
        ///     The end value. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedMaterial AnimateInterpolation(GameObject               gameObject,
                                                               int                      materialSlot,
                                                               UxrMaterialMode          materialMode,
                                                               UxrMaterialParameterType parameterType,
                                                               string                   parameterName,
                                                               Vector4                  startValue,
                                                               Vector4                  endValue,
                                                               UxrInterpolationSettings settings,
                                                               Action                   finishedCallback = null)
        {
            UxrAnimatedMaterial component = gameObject.GetOrAddComponent<UxrAnimatedMaterial>();

            if (component)
            {
                component._restoreWhenFinished   = false;
                component._animateSelf           = true;
                component._materialSlot          = materialSlot;
                component._materialMode          = materialMode;
                component._parameterType         = parameterType;
                component._parameterName         = parameterName;
                component.AnimationMode          = UxrAnimationMode.Interpolate;
                component.InterpolatedValueStart = startValue;
                component.InterpolatedValueEnd   = endValue;
                component.InterpolationSettings  = settings;
                component._finishedCallback      = finishedCallback;
                component.Initialize();
                component.StartTimer();
                component.InterpolatedValueWhenDisabled = component._valueBeforeAnimation;
            }

            return component;
        }

        /// <summary>
        ///     Starts a material parameter animation using noise
        /// </summary>
        /// <param name="gameObject">The GameObject with the material to apply the animation to</param>
        /// <param name="materialSlot">The renderer material slot where the material is</param>
        /// <param name="materialMode">
        ///     The material mode. Use instance to animate the material of a single object,
        ///     use shared to also affect all other objects that share the same material
        /// </param>
        /// <param name="parameterType">Selects the type of the parameter to animate</param>
        /// <param name="parameterName">
        ///     Selects the name of the parameter to animate.
        ///     This name is the name in the shader, not in the inspector!
        /// </param>
        /// <param name="noiseTimeStart">The time in seconds the noise will start (Time.time or Time.unscaledTime value)</param>
        /// <param name="noiseTimeDuration">The duration in seconds of the noise animation</param>
        /// <param name="noiseValueStart">
        ///     The start value. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="noiseValueEnd">
        ///     The end value. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="noiseValueMin">
        ///     The minimum intensity value for the noise. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="noiseValueMax">
        ///     The maximum intensity value for the noise. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="noiseValueFrequency">
        ///     The noise frequency. For int/float values use .x, for Vector2 use x and y.
        ///     For Vector3 use x, y, z. etc.
        /// </param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <param name="useUnscaledTime">If true it will use Time.unscaledTime, if false it will use Time.time</param>
        public static void AnimateNoise(GameObject               gameObject,
                                        int                      materialSlot,
                                        UxrMaterialMode          materialMode,
                                        UxrMaterialParameterType parameterType,
                                        string                   parameterName,
                                        float                    noiseTimeStart,
                                        float                    noiseTimeDuration,
                                        Vector4                  noiseValueStart,
                                        Vector4                  noiseValueEnd,
                                        Vector4                  noiseValueMin,
                                        Vector4                  noiseValueMax,
                                        Vector4                  noiseValueFrequency,
                                        bool                     useUnscaledTime  = false,
                                        Action                   finishedCallback = null)
        {
            UxrAnimatedMaterial component = gameObject.GetOrAddComponent<UxrAnimatedMaterial>();

            if (component)
            {
                component._restoreWhenFinished = false;
                component._animateSelf         = true;
                component._materialSlot        = materialSlot;
                component._materialMode        = materialMode;
                component._parameterType       = parameterType;
                component._parameterName       = parameterName;
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
                component.Initialize();
                component.StartTimer();
                component.InterpolatedValueWhenDisabled = component._valueBeforeAnimation;
            }
        }

        /// <summary>
        ///     Starts animating a GameObject's material making one if its float parameters blink.
        /// </summary>
        /// <param name="gameObject">GameObject whose material to animate</param>
        /// <param name="varNameFloat">The float var name</param>
        /// <param name="valueMin">The minimum float value in the blink</param>
        /// <param name="valueMax">The maximum float value in the blink</param>
        /// <param name="blinkFrequency">The blinking frequency</param>
        /// <param name="durationSeconds">
        ///     The duration in seconds. Use a negative value to keep keep blinking until stopping
        ///     manually.
        /// </param>
        /// <param name="materialMode">The material mode</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>Material animation component</returns>
        public static UxrAnimatedMaterial AnimateFloatBlink(GameObject      gameObject,
                                                            string          varNameFloat,
                                                            float           valueMin         = 0.0f,
                                                            float           valueMax         = 1.0f,
                                                            float           blinkFrequency   = DefaultBlinkFrequency,
                                                            float           durationSeconds  = -1.0f,
                                                            UxrMaterialMode materialMode     = UxrMaterialMode.InstanceOnly,
                                                            Action          finishedCallback = null)
        {
            return AnimateInterpolation(gameObject,
                                        0,
                                        materialMode,
                                        UxrMaterialParameterType.Float,
                                        varNameFloat,
                                        Vector4.one * valueMin,
                                        Vector4.one * valueMax,
                                        new UxrInterpolationSettings(1.0f / blinkFrequency * 0.5f, 0.0f, UxrEasing.EaseInOutSine, UxrLoopMode.PingPong, durationSeconds),
                                        finishedCallback);
        }

        /// <summary>
        ///     Starts animating a GameObject's material making one if its color parameters blink.
        /// </summary>
        /// <param name="gameObject">GameObject whose material to animate</param>
        /// <param name="varNameColor">The float var name</param>
        /// <param name="colorOff">The minimum color value in the blink</param>
        /// <param name="colorOn">The maximum color value in the blink</param>
        /// <param name="blinkFrequency">The blinking frequency</param>
        /// <param name="durationSeconds">
        ///     The duration in seconds. Use a negative value to keep keep blinking until stopping
        ///     manually.
        /// </param>
        /// <param name="materialMode">The material mode</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>Material animation component</returns>
        public static UxrAnimatedMaterial AnimateBlinkColor(GameObject      gameObject,
                                                            string          varNameColor,
                                                            Color           colorOff,
                                                            Color           colorOn,
                                                            float           blinkFrequency   = DefaultBlinkFrequency,
                                                            float           durationSeconds  = -1.0f,
                                                            UxrMaterialMode materialMode     = UxrMaterialMode.InstanceOnly,
                                                            Action          finishedCallback = null)
        {
            return AnimateInterpolation(gameObject,
                                        0,
                                        materialMode,
                                        UxrMaterialParameterType.Color,
                                        varNameColor,
                                        colorOff,
                                        colorOn,
                                        new UxrInterpolationSettings(1.0f / blinkFrequency * 0.5f, 0.0f, UxrEasing.EaseInOutSine, UxrLoopMode.PingPong, durationSeconds),
                                        finishedCallback);
        }

        /// <summary>
        ///     Restores the original (shared) material. This may have some performance advantages.
        /// </summary>
        public void RestoreOriginalSharedMaterial()
        {
            if (_renderer)
            {
                if (_materialSlot == 0)
                {
                    _renderer.sharedMaterial = _originalMaterial;
                }
                else
                {
                    _renderer.sharedMaterials = _originalMaterials;
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes internal variables
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc cref="UxrAnimatedComponent{T}.OnFinished" />
        protected override void OnFinished(UxrAnimatedMaterial anim)
        {
            base.OnFinished(anim);

            if (RestoreWhenFinished && MaterialMode == UxrMaterialMode.InstanceOnly)
            {
                RestoreOriginalSharedMaterial();
            }

            _finishedCallback?.Invoke();
        }

        #endregion

        #region Protected Overrides UxrAnimatedComponent<UxrAnimatedMaterial>

        /// <summary>
        ///     Restores the original value before the animation started.
        /// </summary>
        protected override void RestoreOriginalValue()
        {
            if (_valueBeforeAnimationInitialized)
            {
                SetParameterValue(_valueBeforeAnimation);
            }
        }

        /// <summary>
        ///     Gets the parameter value from the material
        /// </summary>
        /// <returns>
        ///     Vector4 containing the value. This value may not use all components depending on the parameter type.
        /// </returns>
        protected override Vector4 GetParameterValue()
        {
            if (_renderer && _materialSlot < _renderer.sharedMaterials.Length)
            {
                switch (_parameterType)
                {
                    case UxrMaterialParameterType.Int:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            return _materialSlot == 0 ? new Vector4(_renderer.material.GetInt(_parameterName), 0, 0, 0) : new Vector4(_renderer.materials[_materialSlot].GetInt(_parameterName), 0, 0, 0);
                        }
                        else
                        {
                            return _materialSlot == 0 ? new Vector4(_renderer.sharedMaterial.GetInt(_parameterName), 0, 0, 0) : new Vector4(_renderer.sharedMaterials[_materialSlot].GetInt(_parameterName), 0, 0, 0);
                        }

                    case UxrMaterialParameterType.Float:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            return _materialSlot == 0 ? new Vector4(_renderer.material.GetFloat(_parameterName), 0, 0, 0) : new Vector4(_renderer.materials[_materialSlot].GetFloat(_parameterName), 0, 0, 0);
                        }
                        else
                        {
                            return _materialSlot == 0 ? new Vector4(_renderer.sharedMaterial.GetFloat(_parameterName), 0, 0, 0) : new Vector4(_renderer.sharedMaterials[_materialSlot].GetFloat(_parameterName), 0, 0, 0);
                        }

                    case UxrMaterialParameterType.Vector2:
                    case UxrMaterialParameterType.Vector3:
                    case UxrMaterialParameterType.Vector4:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            return _materialSlot == 0 ? _renderer.material.GetVector(_parameterName) : _renderer.materials[_materialSlot].GetVector(_parameterName);
                        }
                        else
                        {
                            return _materialSlot == 0 ? _renderer.sharedMaterial.GetVector(_parameterName) : _renderer.sharedMaterials[_materialSlot].GetVector(_parameterName);
                        }

                    case UxrMaterialParameterType.Color:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            return _materialSlot == 0 ? _renderer.material.GetColor(_parameterName) : _renderer.materials[_materialSlot].GetColor(_parameterName);
                        }
                        else
                        {
                            return _materialSlot == 0 ? _renderer.sharedMaterial.GetColor(_parameterName) : _renderer.sharedMaterials[_materialSlot].GetColor(_parameterName);
                        }
                }
            }

            Debug.LogWarning($"Material slot {_materialSlot} for {this.GetPathUnderScene()} is not valid");
            return Vector4.zero;
        }

        /// <summary>
        ///     Sets the material parameter value
        /// </summary>
        /// <param name="value">
        ///     Vector4 containing the value. This value may not use all components depending on the parameter type
        /// </param>
        protected override void SetParameterValue(Vector4 value)
        {
            Material[] materials = null;

            if (_renderer && _materialSlot < _renderer.sharedMaterials.Length)
            {
                switch (_parameterType)
                {
                    case UxrMaterialParameterType.Int:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.material.SetInt(_parameterName, Mathf.RoundToInt(value.x));
                            }
                            else
                            {
                                materials = _renderer.materials;
                                materials[_materialSlot].SetInt(_parameterName, Mathf.RoundToInt(value.x));
                                _renderer.materials = materials;
                            }
                        }
                        else
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.sharedMaterial.SetInt(_parameterName, Mathf.RoundToInt(value.x));
                            }
                            else
                            {
                                materials = _renderer.sharedMaterials;
                                materials[_materialSlot].SetInt(_parameterName, Mathf.RoundToInt(value.x));
                                _renderer.sharedMaterials = materials;
                            }
                        }

                        return;

                    case UxrMaterialParameterType.Float:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.material.SetFloat(_parameterName, value.x);
                            }
                            else
                            {
                                materials = _renderer.materials;
                                materials[_materialSlot].SetFloat(_parameterName, value.x);
                                _renderer.materials = materials;
                            }
                        }
                        else
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.sharedMaterial.SetFloat(_parameterName, value.x);
                            }
                            else
                            {
                                materials = _renderer.sharedMaterials;
                                materials[_materialSlot].SetFloat(_parameterName, value.x);
                                _renderer.sharedMaterials = materials;
                            }
                        }

                        return;

                    case UxrMaterialParameterType.Vector2:
                    case UxrMaterialParameterType.Vector3:
                    case UxrMaterialParameterType.Vector4:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.material.SetVector(_parameterName, value);
                            }
                            else
                            {
                                materials = _renderer.materials;
                                materials[_materialSlot].SetVector(_parameterName, value);
                                _renderer.materials = materials;
                            }
                        }
                        else
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.sharedMaterial.SetVector(_parameterName, value);
                            }
                            else
                            {
                                materials = _renderer.sharedMaterials;
                                materials[_materialSlot].SetVector(_parameterName, value);
                                _renderer.sharedMaterials = materials;
                            }
                        }

                        return;

                    case UxrMaterialParameterType.Color:

                        if (_materialMode == UxrMaterialMode.InstanceOnly)
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.material.SetColor(_parameterName, value);
                            }
                            else
                            {
                                materials = _renderer.materials;
                                materials[_materialSlot].SetColor(_parameterName, value);
                                _renderer.materials = materials;
                            }
                        }
                        else
                        {
                            if (_materialSlot == 0)
                            {
                                _renderer.sharedMaterial.SetColor(_parameterName, value);
                            }
                            else
                            {
                                materials = _renderer.sharedMaterials;
                                materials[_materialSlot].SetColor(_parameterName, value);
                                _renderer.sharedMaterials = materials;
                            }
                        }

                        return;
                }
            }

            Debug.LogWarning("Material slot " + _materialSlot + " for object " + name + " is not valid");
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes internal data
        /// </summary>
        private void Initialize()
        {
            if (_renderer == null)
            {
                _renderer = _animateSelf || !_targetGameObject ? GetComponent<Renderer>() : _targetGameObject.GetComponent<Renderer>();

                if (_renderer)
                {
                    if (_materialSlot == 0)
                    {
                        _originalMaterial = _renderer.sharedMaterial;
                    }
                    else
                    {
                        _originalMaterials = _renderer.sharedMaterials;
                    }
                }
            }

            if (_renderer && !string.IsNullOrEmpty(_parameterName) && !_valueBeforeAnimationInitialized)
            {
                _valueBeforeAnimation            = GetParameterValue();
                _valueBeforeAnimationInitialized = true;
            }
        }

        #endregion

        #region Private Types & Data

        private Renderer   _renderer;
        private Material   _originalMaterial;
        private Material[] _originalMaterials;
        private bool       _valueBeforeAnimationInitialized;
        private Vector4    _valueBeforeAnimation;
        private Action     _finishedCallback;

        #endregion
    }
}