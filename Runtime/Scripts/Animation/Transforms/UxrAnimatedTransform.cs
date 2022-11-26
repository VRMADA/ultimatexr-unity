// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedTransform.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     Component that allows to animate transforms on objects or even camera properties. Both at runtime through scripting
    ///     or at edit time through the inspector properties.
    /// </summary>
    public sealed class UxrAnimatedTransform : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAnimationMode             _translationMode;
        [SerializeField] private UxrTransformTranslationSpace _translationSpace;
        [SerializeField] private Vector3                      _translationSpeed;
        [SerializeField] private Vector3                      _translationStart;
        [SerializeField] private Vector3                      _translationEnd;
        [SerializeField] private bool                         _translationUseUnscaledTime;
        [SerializeField] private UxrInterpolationSettings     _translationInterpolationSettings = new UxrInterpolationSettings();
        [SerializeField] private UxrAnimationMode             _rotationMode;
        [SerializeField] private UxrTransformRotationSpace    _rotationSpace;
        [SerializeField] private Vector3                      _eulerSpeed;
        [SerializeField] private Vector3                      _eulerStart;
        [SerializeField] private Vector3                      _eulerEnd;
        [SerializeField] private bool                         _rotationUseUnscaledTime;
        [SerializeField] private UxrInterpolationSettings     _rotationInterpolationSettings = new UxrInterpolationSettings();
        [SerializeField] private UxrAnimationMode             _scalingMode;
        [SerializeField] private Vector3                      _scalingSpeed;
        [SerializeField] private Vector3                      _scalingStart;
        [SerializeField] private Vector3                      _scalingEnd;
        [SerializeField] private bool                         _scalingUseUnscaledTime;
        [SerializeField] private UxrInterpolationSettings     _scalingInterpolationSettings = new UxrInterpolationSettings();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called when the translation animation finished. This only applies to translation animations that end.
        /// </summary>
        public event Action TranslationFinished;

        /// <summary>
        ///     Event called when the rotation animation finished. This only applies to rotation animations that end.
        /// </summary>
        public event Action RotationFinished;

        /// <summary>
        ///     Event called when the scaling animation finished. This only applies to scaling animations that end.
        /// </summary>
        public event Action ScalingFinished;

        /// <summary>
        ///     Gets whether the translation interpolation curve finished.
        ///     If no translation interpolation curve was started it will return false.
        /// </summary>
        public bool HasTranslationFinished { get; private set; }

        /// <summary>
        ///     Gets whether the rotation interpolation curve finished.
        ///     If no rotation interpolation curve was started it will return false.
        /// </summary>
        public bool HasRotationFinished { get; private set; }

        /// <summary>
        ///     Gets whether the scaling interpolation curve finished.
        ///     If no scaling interpolation curve was started it will return false.
        /// </summary>
        public bool HasScalingFinished { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts a translation at a constant speed
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the translation to</param>
        /// <param name="space">The space where the translation takes place</param>
        /// <param name="speed">The translation speed (units per second in X/Y/Z axes)</param>
        /// <param name="useUnscaledTime">
        ///     If it is true then <see cref="Time.unscaledTime" /> will be used to count seconds. By default it is false meaning
        ///     <see cref="Time.time" /> will be used instead.
        ///     <see cref="Time.time" /> is affected by <see cref="Time.timeScale" /> which in many cases is used for application
        ///     pauses or bullet-time effects, while <see cref="Time.unscaledTime" /> is not.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform Translate(GameObject gameObject, UxrTransformTranslationSpace space, Vector3 speed, bool useUnscaledTime = false)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._translationMode                                  = UxrAnimationMode.Speed;
                component._translationSpace                                 = space;
                component._translationSpeed                                 = speed;
                component._translationInterpolationSettings.UseUnscaledTime = useUnscaledTime;
                component.HasTranslationFinished                            = false;
            }

            return component;
        }

        /// <summary>
        ///     Starts a rotation at a constant speed
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the rotation to</param>
        /// <param name="space">The space where the rotation takes place</param>
        /// <param name="speed">The rotation speed (degrees per second, per component X/Y/Z)</param>
        /// <param name="useUnscaledTime">
        ///     If it is true then Time.unscaledTime will be used
        ///     to count seconds. By default it is false meaning Time.time will be used instead.
        ///     Time.time is affected by Time.timeScale which in many cases is used for application pauses
        ///     or bullet-time effects, while Time.unscaledTime is not.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform Rotate(GameObject gameObject, UxrTransformRotationSpace space, Vector3 speed, bool useUnscaledTime = false)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._rotationMode                                  = UxrAnimationMode.Speed;
                component._rotationSpace                                 = space;
                component._useEuler                                      = true;
                component._eulerSpeed                                    = speed;
                component._rotationInterpolationSettings.UseUnscaledTime = useUnscaledTime;
                component.HasRotationFinished                            = false;
            }

            return component;
        }

        /// <summary>
        ///     Starts scaling at a constant speed
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the scaling to</param>
        /// <param name="speed">The scaling speed (units per second in X/Y/Z axes)</param>
        /// <param name="useUnscaledTime">
        ///     If it is true then Time.unscaledTime will be used
        ///     to count seconds. By default it is false meaning Time.time will be used instead.
        ///     Time.time is affected by Time.timeScale which in many cases is used for application pauses
        ///     or bullet-time effects, while Time.unscaledTime is not.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform Scale(GameObject gameObject, Vector3 speed, bool useUnscaledTime = false)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._scalingMode                                  = UxrAnimationMode.Speed;
                component._scalingSpeed                                 = speed;
                component._scalingInterpolationSettings.UseUnscaledTime = useUnscaledTime;
                component.HasScalingFinished                            = false;
            }

            return component;
        }

        /// <summary>
        ///     Starts a translation using an interpolation curve
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the translation to</param>
        /// <param name="space">The space where the translation takes place</param>
        /// <param name="startPos">The start position</param>
        /// <param name="endPos">The end position</param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">
        ///     Optional callback called when the animation finished. Only applies to non-looping
        ///     animations.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform PositionInterpolation(GameObject gameObject, UxrTransformTranslationSpace space, Vector3 startPos, Vector3 endPos, UxrInterpolationSettings settings, Action finishedCallback = null)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._translationMode                  = UxrAnimationMode.Interpolate;
                component._translationSpace                 = space;
                component._translationStart                 = startPos;
                component._translationEnd                   = endPos;
                component._translationInterpolationSettings = settings;
                component._translationFinishedCallback      = finishedCallback;
            }

            return component;
        }

        /// <summary>
        ///     Starts a rotation using an interpolation curve
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the rotation to</param>
        /// <param name="space">The space where the rotation takes place</param>
        /// <param name="startEuler">The start Euler angles</param>
        /// <param name="endEuler">The end Euler angles</param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">
        ///     Optional callback called when the animation finished. Only applies to non-looping
        ///     animations.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform RotationInterpolation(GameObject gameObject, UxrTransformRotationSpace space, Vector3 startEuler, Vector3 endEuler, UxrInterpolationSettings settings, Action finishedCallback = null)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._rotationMode                  = UxrAnimationMode.Interpolate;
                component._useEuler                      = true;
                component._rotationSpace                 = space;
                component._eulerStart                    = startEuler;
                component._eulerEnd                      = endEuler;
                component._rotationInterpolationSettings = settings;
                component._rotationFinishedCallback      = finishedCallback;
            }

            return component;
        }

        /// <summary>
        ///     Starts a rotation using an interpolation curve
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the rotation to</param>
        /// <param name="space">The space where the rotation takes place</param>
        /// <param name="startRot">The start Quaternion orientation</param>
        /// <param name="endRot">The end Quaternion orientation</param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">
        ///     Optional callback called when the animation finished. Only applies to non-looping
        ///     animations.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform RotationInterpolation(GameObject gameObject, UxrTransformRotationSpace space, Quaternion startRot, Quaternion endRot, UxrInterpolationSettings settings, Action finishedCallback = null)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._rotationMode                  = UxrAnimationMode.Interpolate;
                component._useEuler                      = false;
                component._rotationSpace                 = space;
                component._quaternionStart               = startRot;
                component._quaternionEnd                 = endRot;
                component._rotationInterpolationSettings = settings;
                component._rotationFinishedCallback      = finishedCallback;
            }

            return component;
        }

        /// <summary>
        ///     Starts scaling using an interpolation curve
        /// </summary>
        /// <param name="gameObject">The GameObject to apply the scaling to</param>
        /// <param name="startScale">The start scale</param>
        /// <param name="endScale">The end scale</param>
        /// <param name="settings">The interpolation settings with the curve parameters</param>
        /// <param name="finishedCallback">
        ///     Optional callback called when the animation finished. Only applies to non-looping
        ///     animations.
        /// </param>
        /// <returns>The animation component</returns>
        public static UxrAnimatedTransform ScalingInterpolation(GameObject gameObject, Vector3 startScale, Vector3 endScale, UxrInterpolationSettings settings, Action finishedCallback = null)
        {
            UxrAnimatedTransform component = gameObject.GetOrAddComponent<UxrAnimatedTransform>();

            if (component)
            {
                component._scalingMode                  = UxrAnimationMode.Interpolate;
                component._scalingStart                 = startScale;
                component._scalingEnd                   = endScale;
                component._scalingInterpolationSettings = settings;
                component._scalingFinishedCallback      = finishedCallback;
            }

            return component;
        }

        /// <summary>
        ///     Stops the position/rotation/scaling animations on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <param name="restoreOriginal">
        ///     Whether to reset the position/rotation/scale values to the state before the animation
        ///     started
        /// </param>
        public static void StopAll(GameObject gameObject, bool restoreOriginal = true)
        {
            UxrAnimatedTransform anim = gameObject.GetComponent<UxrAnimatedTransform>();

            if (anim)
            {
                anim.StopAll(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the translation animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <param name="restoreOriginal">Whether to reset the position to the state before the animation started.</param>
        public static void StopTranslation(GameObject gameObject, bool restoreOriginal = true)
        {
            UxrAnimatedTransform anim = gameObject.GetComponent<UxrAnimatedTransform>();

            if (anim)
            {
                anim.StopTranslation(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the rotation animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <param name="restoreOriginal">Whether to reset the rotation to the state before the animation started.</param>
        public static void StopRotation(GameObject gameObject, bool restoreOriginal = true)
        {
            UxrAnimatedTransform anim = gameObject.GetComponent<UxrAnimatedTransform>();

            if (anim)
            {
                anim.StopRotation(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the scaling animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <param name="restoreOriginal">Whether to reset the scale to the state before the animation started.</param>
        public static void StopScaling(GameObject gameObject, bool restoreOriginal = true)
        {
            UxrAnimatedTransform anim = gameObject.GetComponent<UxrAnimatedTransform>();

            if (anim)
            {
                anim.StopScaling(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the position/rotation/scaling animations on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="restoreOriginal">
        ///     Whether to reset the position/rotation/scale values to the state before the animation
        ///     started
        /// </param>
        public void StopAll(bool restoreOriginal = true)
        {
            HasTranslationFinished = true;
            HasRotationFinished    = true;
            HasScalingFinished     = true;

            if (restoreOriginal)
            {
                transform.localPosition = _initialLocalPosition;
                transform.localRotation = _initialLocalRotation;
                transform.localScale    = _initialLocalScale;
            }
        }

        /// <summary>
        ///     Stops the translation animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset the position to the state before the animation started.</param>
        public void StopTranslation(bool restoreOriginal = true)
        {
            HasTranslationFinished = true;

            if (restoreOriginal)
            {
                transform.localPosition = _initialLocalPosition;
            }
        }

        /// <summary>
        ///     Stops the rotation animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset the rotation to the state before the animation started.</param>
        public void StopRotation(bool restoreOriginal = true)
        {
            HasRotationFinished = true;

            if (restoreOriginal)
            {
                transform.localRotation = _initialLocalRotation;
            }
        }

        /// <summary>
        ///     Stops the scaling animation on an object if it has an <see cref="UxrAnimatedTransform" />
        ///     component currently attached.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset the scale to the state before the animation started.</param>
        public void StopScaling(bool restoreOriginal = true)
        {
            HasScalingFinished = true;

            if (restoreOriginal)
            {
                transform.localScale = _initialLocalScale;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stores some initial values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _scaleTimer = 0.0f;
        }

        /// <summary>
        ///     Called each time the object is enabled. Reset timer and set the curve state to unfinished.
        ///     The first time it's called it stores the original transform values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _startTimeTranslation = GetCurrentTime(_translationUseUnscaledTime, _translationMode, _translationInterpolationSettings);
            _startTimeRotation    = GetCurrentTime(_rotationUseUnscaledTime,    _rotationMode,    _rotationInterpolationSettings);
            _startTimeScaling     = GetCurrentTime(_scalingUseUnscaledTime,     _scalingMode,     _scalingInterpolationSettings);

            HasTranslationFinished = false;
            HasRotationFinished    = false;
            HasScalingFinished     = false;

            if (!_originalValuesStored)
            {
                _originalValuesStored = true;
                _initialLocalPosition = transform.localPosition;
                _initialLocalRotation = transform.localRotation;
                _initialLocalScale    = transform.localScale;
            }
        }

        /// <summary>
        ///     Performs transform updates
        /// </summary>
        private void Update()
        {
            // Translation ////////////////////////////////////////////////////////////////////////////////

            if (!HasTranslationFinished)
            {
                switch (_translationMode)
                {
                    case UxrAnimationMode.None: break;

                    case UxrAnimationMode.Speed:
                    {
                        Vector3 xAxis = Vector3.right;
                        Vector3 yAxis = Vector3.up;
                        Vector3 zAxis = Vector3.forward;

                        if (_translationSpace == UxrTransformTranslationSpace.Local)
                        {
                            xAxis = transform.right;
                            yAxis = transform.up;
                            zAxis = transform.forward;
                        }
                        else if (_translationSpace == UxrTransformTranslationSpace.Parent)
                        {
                            if (transform.parent != null)
                            {
                                xAxis = transform.parent.right;
                                yAxis = transform.parent.up;
                                zAxis = transform.parent.forward;
                            }
                        }

                        float deltaTime = GetDeltaTime(_translationUseUnscaledTime);
                        transform.Translate(_translationSpeed.x * deltaTime * xAxis + _translationSpeed.y * deltaTime * yAxis + _translationSpeed.z * deltaTime * zAxis, Space.World);
                        break;
                    }

                    case UxrAnimationMode.Interpolate:
                    {
                        float   time     = GetCurrentTime(_translationUseUnscaledTime, _translationMode, _translationInterpolationSettings) - _startTimeTranslation;
                        Vector3 position = UxrInterpolator.Interpolate(_translationStart, _translationEnd, time, _translationInterpolationSettings);

                        switch (_translationSpace)
                        {
                            case UxrTransformTranslationSpace.World:
                                transform.position = position;
                                break;

                            case UxrTransformTranslationSpace.Local:
                                transform.localPosition = position;
                                break;

                            case UxrTransformTranslationSpace.Parent:

                                if (transform.parent == null)
                                {
                                    transform.position = position;
                                }
                                else
                                {
                                    transform.position = transform.parent.position + transform.parent.GetScaledVector(position);
                                }

                                break;

                            default: throw new ArgumentOutOfRangeException();
                        }

                        if (_translationInterpolationSettings.CheckInterpolationHasFinished(time))
                        {
                            HasTranslationFinished = true;
                            OnTranslationFinished();
                        }
                        break;
                    }

                    case UxrAnimationMode.Noise: // TODO
                        break;
                }
            }

            // Rotation ////////////////////////////////////////////////////////////////////////////////

            if (!HasRotationFinished)
            {
                switch (_rotationMode)
                {
                    case UxrAnimationMode.None: break;

                    case UxrAnimationMode.Speed:
                    {
                        float deltaTime = GetDeltaTime(_rotationUseUnscaledTime);
                        transform.Rotate(_eulerSpeed * deltaTime, _rotationSpace == UxrTransformRotationSpace.Local ? Space.Self : Space.World);
                        break;
                    }

                    case UxrAnimationMode.Interpolate:
                    {
                        float      time     = GetCurrentTime(_rotationUseUnscaledTime, _rotationMode, _rotationInterpolationSettings) - _startTimeRotation;
                        Quaternion rotation = Quaternion.identity;

                        if (_useEuler)
                        {
                            Vector3 euler = UxrInterpolator.Interpolate(_eulerStart, _eulerEnd, time, _rotationInterpolationSettings);
                            rotation = Quaternion.Euler(euler);
                        }
                        else
                        {
                            rotation = UxrInterpolator.Interpolate(_quaternionStart, _quaternionEnd, time, _rotationInterpolationSettings);
                        }

                        switch (_rotationSpace)
                        {
                            case UxrTransformRotationSpace.World:
                                transform.rotation = rotation;
                                break;

                            case UxrTransformRotationSpace.Local:
                                transform.localRotation = rotation;
                                break;

                            default: throw new ArgumentOutOfRangeException();
                        }

                        if (_rotationInterpolationSettings.CheckInterpolationHasFinished(time))
                        {
                            HasRotationFinished = true;
                            OnRotationFinished();
                        }
                        break;
                    }

                    case UxrAnimationMode.Noise:
                        // TODO
                        break;
                }
            }

            // Scaling /////////////////////////////////////////////////////////////////////////////////

            if (!HasScalingFinished)
            {
                switch (_scalingMode)
                {
                    case UxrAnimationMode.None: break;

                    case UxrAnimationMode.Speed:
                        _scaleTimer          += GetDeltaTime(_scalingUseUnscaledTime);
                        transform.localScale =  _initialLocalScale + Vector3.Scale(_initialLocalScale, _scalingSpeed * _scaleTimer);
                        break;

                    case UxrAnimationMode.Interpolate:
                    {
                        float time = GetCurrentTime(_scalingUseUnscaledTime, _scalingMode, _scalingInterpolationSettings) - _startTimeScaling;
                        transform.localScale = UxrInterpolator.Interpolate(_scalingStart, _scalingEnd, time, _scalingInterpolationSettings);

                        if (_scalingInterpolationSettings.CheckInterpolationHasFinished(time))
                        {
                            HasScalingFinished = true;
                            OnScalingFinished();
                        }
                        break;
                    }

                    case UxrAnimationMode.Noise:
                        // TODO
                        break;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        private void OnTranslationFinished()
        {
            TranslationFinished?.Invoke();
            _translationFinishedCallback?.Invoke();
        }

        private void OnRotationFinished()
        {
            RotationFinished?.Invoke();
            _rotationFinishedCallback?.Invoke();
        }

        private void OnScalingFinished()
        {
            ScalingFinished?.Invoke();
            _scalingFinishedCallback?.Invoke();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the current delta time depending on the timing used.
        /// </summary>
        /// <param name="useUnscaledTime">Whether to use the unscaled delta time or not</param>
        /// <returns>Correct delta time value to use</returns>
        private float GetDeltaTime(bool useUnscaledTime)
        {
            return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        /// <summary>
        ///     Gets the current time in seconds. It computes the correct time, either <see cref="Time.unscaledTime" /> or
        ///     <see cref="Time.time" />, depending on the animation configuration.
        /// </summary>
        /// <param name="useUnscaledTime">The default value if no interpolation is set up</param>
        /// <param name="mode">Animation mode</param>
        /// <param name="settings">
        ///     The interpolation settings to use if animation is set to
        ///     <see cref="UxrAnimationMode.Interpolate" />.
        /// </param>
        /// <returns>Correct time value to use</returns>
        private float GetCurrentTime(bool useUnscaledTime, UxrAnimationMode mode, UxrInterpolationSettings settings)
        {
            if (settings != null && mode == UxrAnimationMode.Interpolate)
            {
                return settings.UseUnscaledTime ? Time.unscaledTime : Time.time;
            }

            return useUnscaledTime ? Time.unscaledTime : Time.time;
        }

        #endregion

        #region Private Types & Data

        private bool       _useEuler;
        private Quaternion _quaternionStart;
        private Quaternion _quaternionEnd;
        private float      _scaleTimer;

        private Action _translationFinishedCallback;
        private Action _rotationFinishedCallback;
        private Action _scalingFinishedCallback;

        private bool       _originalValuesStored;
        private Vector3    _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private Vector3    _initialLocalScale;

        private float _startTimeTranslation;
        private float _startTimeRotation;
        private float _startTimeScaling;

        #endregion
    }
}