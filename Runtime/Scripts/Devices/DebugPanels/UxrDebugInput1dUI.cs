// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDebugInput1dUI.cs" company="VRMADA">
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
    ///     UI Widget for a single-axis input element in a VR input controller. Examples are trigger buttons, grip buttons...
    /// </summary>
    public class UxrDebugInput1dUI : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrControllerInput _controllerInput;
        [SerializeField] private UxrHandSide        _hand;
        [SerializeField] private UxrInput1D         _target;
        [SerializeField] private Text               _name;
        [SerializeField] private RectTransform      _cursor;
        [SerializeField] private float              _coordAmplitude;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the controller(s) to monitor for input.
        /// </summary>
        public UxrControllerInput TargetController
        {
            get => _controllerInput;
            set => _controllerInput = value;
        }

        /// <summary>
        ///     Gets or sets the hand to monitor for input.
        /// </summary>
        public UxrHandSide TargetHand
        {
            get => _hand;
            set => _hand = value;
        }

        /// <summary>
        ///     Gets or sets the single-axis element to monitor.
        /// </summary>
        public UxrInput1D Target
        {
            get => _target;
            set => _target = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Updates the widget information.
        /// </summary>
        private void Update()
        {
            _name.text = $"{_hand} {_target}";

            if (_controllerInput != null)
            {
                _cursor.anchoredPosition = new Vector2(0.0f, 1.0f) * (_coordAmplitude * _controllerInput.GetInput1D(_hand, _target, true));
            }
        }

        #endregion
    }
}