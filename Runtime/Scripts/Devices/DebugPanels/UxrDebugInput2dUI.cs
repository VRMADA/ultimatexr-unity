// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDebugInput2dUI.cs" company="VRMADA">
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
    ///     UI Widget for a two-axis input element in a VR input controller. Examples are joysticks, trackpads...
    /// </summary>
    public class UxrDebugInput2dUI : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrControllerInput _controllerInput;
        [SerializeField] private UxrHandSide        _hand;
        [SerializeField] private UxrInput2D         _target;
        [SerializeField] private Text               _name;
        [SerializeField] private RectTransform      _cursor;
        [SerializeField] private float              _coordAmplitude;

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
        ///     Gets the two-axis element to monitor for input.
        /// </summary>
        public UxrInput2D Target
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
                _cursor.anchoredPosition = Vector2.Scale(Vector2.one * _coordAmplitude, _controllerInput.GetInput2D(_hand, _target, true));
            }
        }

        #endregion
    }
}