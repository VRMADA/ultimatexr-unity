// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrObjectBlink.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.GameObjects
{
    /// <summary>
    ///     Component that allows to make objects blink using their material's emission channel.
    /// </summary>
    public class UxrObjectBlink : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Color        _colorNormal     = Color.black;
        [SerializeField] private Color        _colorHighlight  = Color.white;
        [SerializeField] private float        _blinksPerSec    = 4.0f;
        [SerializeField] private float        _durationSeconds = -1.0f;
        [SerializeField] private bool         _useUnscaledTime;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the object is currently blinking.
        /// </summary>
        public bool IsBlinking { get; private set; } = true;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts a blinking animation using the emission material of an object.
        /// </summary>
        /// <param name="gameObject">The GameObject to blink</param>
        /// <param name="emissionColor">The emission color</param>
        /// <param name="blinksPerSec">The blink frequency</param>
        /// <param name="durationSeconds">Total duration of the blinking animation</param>
        /// <param name="useUnscaledTime">
        ///     Whether to use unscaled time (<see cref="Time.unscaledTime" />) or not (
        ///     <see cref="Time.time" />)
        /// </param>
        /// <returns>Animation component</returns>
        public static UxrObjectBlink StartBlinking(GameObject gameObject, Color emissionColor, float blinksPerSec, float durationSeconds, bool useUnscaledTime = false)
        {
            if (gameObject == null)
            {
                return null;
            }

            UxrObjectBlink blinkComponent = gameObject.GetOrAddComponent<UxrObjectBlink>();

            blinkComponent.CheckInitialize();
            blinkComponent.StartBlinkingInternal(emissionColor, blinksPerSec, durationSeconds, useUnscaledTime);

            return blinkComponent;
        }

        /// <summary>
        ///     Stops a blinking animation on an object if it has any.
        /// </summary>
        /// <param name="gameObject">GameObject to stop the animation from</param>
        public static void StopBlinking(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }
            
            if (gameObject.TryGetComponent<UxrObjectBlink>(out var blinkComponent))
            {
                blinkComponent.StopBlinkingInternal();
            }
        }

        /// <summary>
        ///     Checks whether the given GameObject has any blinking animation running.
        /// </summary>
        /// <param name="gameObject">GameObject to check</param>
        /// <returns>Whether the given GameObject has any blinking animation running</returns>
        public static bool CheckBlinking(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            UxrObjectBlink blinkComponent = gameObject.GetComponent<UxrObjectBlink>();

            return blinkComponent != null && blinkComponent.IsBlinking;
        }

        /// <summary>
        ///     Sets up the blinking animation parameters.
        /// </summary>
        /// <param name="renderer">Renderer whose material will be animated</param>
        /// <param name="colorNormal">The emission color when it is not blinking</param>
        /// <param name="colorHighlight">The fully blinking color</param>
        /// <param name="blinksPerSec">The blinking frequency</param>
        /// <param name="durationSeconds">The total duration of the animation in seconds</param>
        public void Setup(MeshRenderer renderer, Color colorNormal, Color colorHighlight, float blinksPerSec = 4.0f, float durationSeconds = -1.0f)
        {
            _renderer        = renderer;
            _colorNormal     = colorNormal;
            _colorHighlight  = colorHighlight;
            _blinksPerSec    = blinksPerSec;
            _durationSeconds = durationSeconds;
            IsBlinking       = false;
            IsInitialized    = false;
            CheckInitialize();
        }

        /// <summary>
        ///     Starts or restarts the blinking animation using the current parameters.
        /// </summary>
        public void StartBlinkingWithCurrentParameters()
        {
            CheckInitialize();
            StartBlinkingInternal(_colorHighlight, _blinksPerSec, _durationSeconds, _useUnscaledTime);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CheckInitialize();
        }

        /// <summary>
        ///     When re-enabled, starts blinking again with the current parameters.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            StartBlinkingWithCurrentParameters();
        }

        /// <summary>
        ///     Stops blinking.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            StopBlinkingInternal();
        }

        /// <summary>
        ///     Updates the blinking animation if active.
        /// </summary>
        private void Update()
        {
            if (IsBlinking && _renderer != null)
            {
                float timer = CurrentTime - _blinkStartTime;

                if (_durationSeconds >= 0.0f && timer >= _durationSeconds)
                {
                    StopBlinkingInternal();
                }
                else
                {
                    float blend = (Mathf.Sin(timer * Mathf.PI * _blinksPerSec * 2.0f) + 1.0f) * 0.5f;
                    _renderer.material.SetColor(UxrConstants.Shaders.EmissionColorVarName, Color.Lerp(_colorNormal, _colorHighlight, blend));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the component if necessary.
        /// </summary>
        private void CheckInitialize()
        {
            if (!IsInitialized)
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<MeshRenderer>();
                }

                if (_renderer != null)
                {
                    _originalMaterial = _renderer.sharedMaterial;
                }

                IsInitialized = true;
            }
        }

        /// <summary>
        ///     Starts blinking.
        /// </summary>
        /// <param name="emissionColor">Emission color</param>
        /// <param name="blinksPerSec">Blinking frequency</param>
        /// <param name="durationSeconds">Total duration in seconds</param>
        /// <param name="useUnscaledTime">Whether to use unscaled time or not</param>
        private void StartBlinkingInternal(Color emissionColor, float blinksPerSec, float durationSeconds, bool useUnscaledTime)
        {
            if (_renderer == null)
            {
                return;
            }

            _useUnscaledTime = useUnscaledTime;
            _blinkStartTime  = CurrentTime;
            _colorHighlight  = emissionColor;
            _blinksPerSec    = blinksPerSec;
            _durationSeconds = durationSeconds;

            _renderer.material.EnableKeyword(UxrConstants.Shaders.EmissionKeyword);

            IsBlinking = true;
        }

        /// <summary>
        ///     Stops blinking.
        /// </summary>
        private void StopBlinkingInternal()
        {
            if (_renderer == null)
            {
                return;
            }

            IsBlinking = false;
            _renderer.material.SetColor(UxrConstants.Shaders.EmissionColorVarName, _colorNormal);
            _renderer.material.DisableKeyword(UxrConstants.Shaders.EmissionKeyword);
            _renderer.sharedMaterial = _originalMaterial;
        }

        #endregion

        #region Private Types & Data

        private float CurrentTime => _useUnscaledTime ? Time.unscaledTime : Time.time;

        private bool IsInitialized { get; set; }

        private float    _blinkStartTime;
        private Material _originalMaterial;

        #endregion
    }
}