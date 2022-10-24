// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDebugInputButtonUI.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Devices.DebugPanels
{
    /// <summary>
    ///     UI Widget for a button in a VR input controller.
    /// </summary>
    public class UxrDebugInputButtonUI : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrControllerInput _controllerInput;
        [SerializeField] private UxrHandSide        _hand;
        [SerializeField] private UxrInputButtons    _button;
        [SerializeField] private Text               _name;
        [SerializeField] private Image              _imagePressing;
        [SerializeField] private Image              _imagePressDown;
        [SerializeField] private Image              _imagePressUp;
        [SerializeField] private Image              _imageTouching;
        [SerializeField] private Image              _imageTouchDown;
        [SerializeField] private Image              _imageTouchUp;
        [SerializeField] private Color              _colorEnabled;
        [SerializeField] private Color              _colorDisabled;
        [SerializeField] private float              _secondsUpAndDownEnabled = 0.1f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the controller to monitor for input.
        /// </summary>
        public UxrControllerInput TargetController
        {
            get => _controllerInput;
            set => _controllerInput = value;
        }

        /// <summary>
        ///     Gets the hand to monitor for input.
        /// </summary>
        public UxrHandSide TargetHand
        {
            get => _hand;
            set => _hand = value;
        }

        /// <summary>
        ///     Gets the button to monitor for input.
        /// </summary>
        public UxrInputButtons Target
        {
            get => _button;
            set => _button = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Updates the widget information.
        /// </summary>
        private void Update()
        {
            _name.text = $"{_hand} {_button}";

            if (_controllerInput != null)
            {
                _pressDownTimer -= Time.deltaTime;
                _pressUpTimer   -= Time.deltaTime;
                _touchDownTimer -= Time.deltaTime;
                _touchUpTimer   -= Time.deltaTime;

                if (_controllerInput.GetButtonsPressDown(_hand, _button, true))
                {
                    _pressDownTimer = _secondsUpAndDownEnabled;
                }

                if (_controllerInput.GetButtonsPressUp(_hand, _button, true))
                {
                    _pressUpTimer = _secondsUpAndDownEnabled;
                }

                if (_controllerInput.GetButtonsTouchDown(_hand, _button, true))
                {
                    _touchDownTimer = _secondsUpAndDownEnabled;
                }

                if (_controllerInput.GetButtonsTouchUp(_hand, _button, true))
                {
                    _touchUpTimer = _secondsUpAndDownEnabled;
                }

                _imagePressing.color = _controllerInput.GetButtonsPress(_hand, _button, true) ? _colorEnabled : _colorDisabled;
                _imageTouching.color = _controllerInput.GetButtonsTouch(_hand, _button, true) ? _colorEnabled : _colorDisabled;

                _imagePressDown.color = _pressDownTimer > 0.0f ? _colorEnabled : _colorDisabled;
                _imagePressUp.color   = _pressUpTimer > 0.0f ? _colorEnabled : _colorDisabled;
                _imageTouchDown.color = _touchDownTimer > 0.0f ? _colorEnabled : _colorDisabled;
                _imageTouchUp.color   = _touchUpTimer > 0.0f ? _colorEnabled : _colorDisabled;
            }
        }

        #endregion

        #region Private Types & Data

        private float _pressDownTimer = -1.0f;
        private float _pressUpTimer   = -1.0f;
        private float _touchDownTimer = -1.0f;
        private float _touchUpTimer   = -1.0f;

        #endregion
    }
}