// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDebugControllerPanel.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace UltimateXR.Devices.DebugPanels
{
    /// <summary>
    ///     UI panel showing all information related to the current main VR input device.
    /// </summary>
    public class UxrDebugControllerPanel : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool       _blinkButtonsOnInput;
        [SerializeField] private Text       _textDeviceName;
        [SerializeField] private Text       _textControllerNames;
        [SerializeField] private GameObject _containerControllerNames;
        [SerializeField] private GameObject _panelInput1D;
        [SerializeField] private GameObject _panelInput2D;
        [SerializeField] private GameObject _panelInputButtons;
        [SerializeField] private GameObject _prefabWidget1D;
        [SerializeField] private GameObject _prefabWidget2D;
        [SerializeField] private GameObject _prefabWidgetButton;

        #endregion

        #region Unity

        /// <summary>
        ///     Checks if the current input device changed in order to regenerate the panel.
        /// </summary>
        private void Update()
        {
            UxrAvatar          avatar          = UxrAvatar.LocalAvatar;
            UxrControllerInput controllerInput = avatar != null ? avatar.ControllerInput : null;

            if (avatar != _avatar || controllerInput != _avatarControllerInput)
            {
                // Unsubscribe from the current avatar controller events.

                if (_avatarControllerInput != null)
                {
                    _avatarControllerInput.ButtonStateChanged -= ControllerInput_ButtonStateChanged;
                    _avatarControllerInput.Input1DChanged     -= ControllerInput_Input1DChanged;
                    _avatarControllerInput.Input2DChanged     -= ControllerInput_Input2DChanged;
                }

                // Cache the new avatar and regenerate the panel.

                _avatar                = avatar;
                _avatarControllerInput = controllerInput;

                RegeneratePanel();

                // Subscribe to the input events to update the input UI widgets.

                if (_avatarControllerInput != null)
                {
                    _avatarControllerInput.ButtonStateChanged += ControllerInput_ButtonStateChanged;
                    _avatarControllerInput.Input1DChanged     += ControllerInput_Input1DChanged;
                    _avatarControllerInput.Input2DChanged     += ControllerInput_Input2DChanged;
                }
            }

            // This one can change each frame, depending on controllers getting connected/disconnected
            UpdateControllerStrings();
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever a VR controller button state changed.
        /// </summary>
        /// <param name="sender">The controller that generated the event</param>
        /// <param name="e">Event arguments</param>
        private void ControllerInput_ButtonStateChanged(object sender, UxrInputButtonEventArgs e)
        {
            UxrControllerInput    controllerInput   = (UxrControllerInput)sender;
            UxrControllerElements controllerElement = UxrControllerInput.ButtonToControllerElement(e.Button);

            bool allControllerElementsBlinking = controllerInput.AreAllControllerElementsBlinking(e.HandSide, controllerElement);

            if (_blinkButtonsOnInput && controllerElement != UxrControllerElements.None && allControllerElementsBlinking == false)
            {
                _avatarControllerInput.StartControllerElementsBlinking(e.HandSide, controllerElement, Color.white, 5, 2.0f);
            }
        }

        /// <summary>
        ///     Called whenever a VR controller single-axis value changed.
        /// </summary>
        /// <param name="sender">The controller that generated the event</param>
        /// <param name="e">Event arguments</param>
        private void ControllerInput_Input1DChanged(object sender, UxrInput1DEventArgs e)
        {
            UxrControllerInput    controllerInput   = (UxrControllerInput)sender;
            UxrControllerElements controllerElement = UxrControllerInput.Input1DToControllerElement(e.Target);

            bool allControllerElementsBlinking = controllerInput.AreAllControllerElementsBlinking(e.HandSide, controllerElement);

            if (_blinkButtonsOnInput && controllerElement != UxrControllerElements.None && allControllerElementsBlinking == false)
            {
                _avatarControllerInput.StartControllerElementsBlinking(e.HandSide, controllerElement, Color.white, 5, 2.0f);
            }
        }

        /// <summary>
        ///     Called whenever a VR controller two-axis value changed.
        /// </summary>
        /// <param name="sender">The controller that generated the event</param>
        /// <param name="e">Event arguments</param>
        private void ControllerInput_Input2DChanged(object sender, UxrInput2DEventArgs e)
        {
            UxrControllerInput    controllerInput   = (UxrControllerInput)sender;
            UxrControllerElements controllerElement = UxrControllerInput.Input2DToControllerElement(e.Target);

            bool allControllerElementsBlinking = controllerInput.AreAllControllerElementsBlinking(e.HandSide, controllerElement);

            if (_blinkButtonsOnInput && controllerElement != UxrControllerElements.None && allControllerElementsBlinking == false)
            {
                _avatarControllerInput.StartControllerElementsBlinking(e.HandSide, controllerElement, Color.white, 5, 2.0f);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the controller names shown.
        /// </summary>
        private void UpdateControllerStrings()
        {
            if (_avatar && _avatarControllerInput)
            {
                string leftName  = _avatarControllerInput.LeftControllerName ?? NoController;
                string rightName = _avatarControllerInput.RightControllerName ?? NoController;

                _containerControllerNames.SetActive(!string.IsNullOrEmpty(leftName) || !string.IsNullOrEmpty(rightName));

                if (_avatarControllerInput.SetupType == UxrControllerSetupType.Single)
                {
                    // Single controller setup. Both sides will return the same name. 
                    _textControllerNames.text = $"Controller: {leftName}";
                }
                else if (_avatarControllerInput.SetupType == UxrControllerSetupType.Dual)
                {
                    // Dual controller setup.
                    _textControllerNames.text = $"Left controller: {leftName}, right controller: {rightName}";
                }
            }
        }

        /// <summary>
        ///     Re-generates the panel adding widgets for all input elements present in the current controller(s).
        /// </summary>
        private void RegeneratePanel()
        {
            _panelInput1D.transform.DestroyAllChildren();
            _panelInput2D.transform.DestroyAllChildren();
            _panelInputButtons.transform.DestroyAllChildren();

            if (_avatar == null || _avatarControllerInput == null)
            {
                _textDeviceName.text = $"No {nameof(UxrAvatar)} with an active {nameof(UxrControllerInput)} component found";
                return;
            }

            // Device strings

            _textDeviceName.text = $"Device: {UxrTrackingDevice.HeadsetDeviceName}, Loaded: {XRSettings.loadedDeviceName}";

            UpdateControllerStrings();

            // Dynamically add all current devices' supported Controllers1D elements to the UI
            foreach (UxrInput1D input1D in Enum.GetValues(typeof(UxrInput1D)))
            {
                UxrControllerElements controllerElement = UxrControllerInput.Input1DToControllerElement(input1D);

                foreach (UxrHandSide handSide in Enum.GetValues(typeof(UxrHandSide)))
                {
                    if (controllerElement != UxrControllerElements.None && _avatarControllerInput.HasControllerElements(handSide, controllerElement))
                    {
                        GameObject        newWidget = Instantiate(_prefabWidget1D, _panelInput1D.transform);
                        UxrDebugInput1dUI uiInput1d = newWidget.GetComponent<UxrDebugInput1dUI>();

                        uiInput1d.TargetController = _avatarControllerInput;
                        uiInput1d.TargetHand       = handSide;
                        uiInput1d.Target           = input1D;
                    }
                }
            }

            // Dynamically add all current devices' supported Controllers2D elements to the UI
            foreach (UxrInput2D input2D in Enum.GetValues(typeof(UxrInput2D)))
            {
                UxrControllerElements controllerElement = UxrControllerInput.Input2DToControllerElement(input2D);

                foreach (UxrHandSide handSide in Enum.GetValues(typeof(UxrHandSide)))
                {
                    if (controllerElement != UxrControllerElements.None && _avatarControllerInput.HasControllerElements(handSide, controllerElement))
                    {
                        GameObject        newWidget = Instantiate(_prefabWidget2D, _panelInput2D.transform);
                        UxrDebugInput2dUI uiInput2d = newWidget.GetComponent<UxrDebugInput2dUI>();

                        uiInput2d.TargetController = _avatarControllerInput;
                        uiInput2d.TargetHand       = handSide;
                        uiInput2d.Target           = input2D;
                    }
                }
            }

            // Dynamically add all current devices' supported button elements to the UI
            foreach (UxrInputButtons button in Enum.GetValues(typeof(UxrInputButtons)))
            {
                UxrControllerElements controllerElement = UxrControllerInput.ButtonToControllerElement(button);

                foreach (UxrHandSide handSide in Enum.GetValues(typeof(UxrHandSide)))
                {
                    if (controllerElement != UxrControllerElements.None && _avatarControllerInput.HasControllerElements(handSide, controllerElement))
                    {
                        GameObject            newWidget = Instantiate(_prefabWidgetButton, _panelInputButtons.transform);
                        UxrDebugInputButtonUI uiButton  = newWidget.GetComponent<UxrDebugInputButtonUI>();

                        uiButton.TargetController = _avatarControllerInput;
                        uiButton.TargetHand       = handSide;
                        uiButton.Target           = button;
                    }
                }
            }
        }

        #endregion

        #region Private Types & Data

        private const string NoController = "None";

        private UxrAvatar          _avatar;
        private UxrControllerInput _avatarControllerInput;

        #endregion
    }
}