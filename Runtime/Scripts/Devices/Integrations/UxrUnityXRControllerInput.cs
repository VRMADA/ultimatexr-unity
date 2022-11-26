// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityXRControllerInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Haptics;
using UnityEngine;
using UnityEngine.XR;

namespace UltimateXR.Devices.Integrations
{
    /// <summary>
    ///     Generic base class for left-right input devices that can be handled through the new
    ///     generic Unity XR input interface. Before, we had to manually support each SDK individually.
    /// </summary>
    public abstract partial class UxrUnityXRControllerInput : UxrControllerInput
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets list of controller names that the component can handle
        /// </summary>
        public abstract IEnumerable<string> ControllerNames { get; }

        /// <summary>
        ///     We use this when we are implementing new controllers that we don't know the name of, in order to
        ///     show the controller names in the UxrDebugControllerPanel.
        ///     Returning true will register the controllers in <see cref="InputDevices_DeviceConnected" /> no
        ///     matter which input device gets connected. Then using the UxrDebugControllerPanel we can see which
        ///     devices got connected.
        ///     This is mostly useful for untethered devices that cannot be tested directly in Unity.
        /// </summary>
        public virtual bool ForceUseAlways => false;

        #endregion

        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override string LeftControllerName => _deviceLeft.isValid ? _deviceLeft.name : string.Empty;

        /// <inheritdoc />
        public override string RightControllerName => _deviceRight.isValid ? _deviceRight.name : string.Empty;

        /// <inheritdoc />
        public override bool IsControllerEnabled(UxrHandSide handSide)
        {
            return GetInputDevice(handSide).isValid;
        }

        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0.0f;
            }

            InputDevice inputDevice = GetInputDevice(handSide);

            if (inputDevice.isValid)
            {
                float value;

                switch (input1D)
                {
                    case UxrInput1D.Grip:

                        if (inputDevice.TryGetFeatureValue(CommonUsages.grip, out value))
                        {
                            return value;
                        }

                        break;

                    case UxrInput1D.Trigger:

                        if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out value))
                        {
                            return value;
                        }

                        break;

                    case UxrInput1D.Trigger2: break;
                }
            }

            return 0.0f;
        }

        /// <inheritdoc />
        public override Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return Vector2.zero;
            }

            InputDevice inputDevice = GetInputDevice(handSide);

            if (inputDevice.isValid)
            {
                Vector2 value;

                switch (input2D)
                {
                    case UxrInput2D.Joystick:

                        if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out value))
                        {
                            return FilterTwoAxesDeadZone(value, JoystickDeadZone);
                        }

                        break;

                    case UxrInput2D.Joystick2:

                        if (inputDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out value))
                        {
                            return FilterTwoAxesDeadZone(value, JoystickDeadZone);
                        }

                        break;
                }
            }

            return Vector2.zero;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            UxrControllerInputCapabilities capabilities = 0;

            InputDevice inputDevice = GetInputDevice(handSide);

            if (!inputDevice.isValid)
            {
                return capabilities;
            }

            if (!inputDevice.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities))
            {
                if (hapticCapabilities.supportsBuffer)
                {
                    capabilities |= UxrControllerInputCapabilities.HapticClips;
                }

                if (hapticCapabilities.supportsImpulse)
                {
                    capabilities |= UxrControllerInputCapabilities.HapticImpulses;
                }
            }

            return capabilities;
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide handSide, UxrHapticClip hapticClip)
        {
            InputDevice inputDevice = GetInputDevice(handSide);

            if (!inputDevice.isValid)
            {
                return;
            }

            if (hapticClip == null)
            {
                return;
            }

            if (hapticClip.Clip == null)
            {
                SendHapticFeedback(handSide,
                                   hapticClip.FallbackClipType,
                                   hapticClip.FallbackAmplitude,
                                   hapticClip.FallbackDurationSeconds,
                                   hapticClip.HapticMode);
                return;
            }

            if (!inputDevice.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities))
            {
                return;
            }

            // Create haptics clip from audio
            byte[] hapticBuffer = CreateHapticBufferFromAudioClip(inputDevice, hapticClip.Clip);

            if (hapticBuffer == null)
            {
                return;
            }

            // Readjust amplitude?
            if (Mathf.Approximately(hapticClip.ClipAmplitude, 1.0f) == false)
            {
                for (int i = 0; i < hapticBuffer.Length; ++i)
                {
                    hapticBuffer[i] = (byte)Mathf.Clamp(Mathf.RoundToInt(hapticBuffer[i] * hapticClip.ClipAmplitude), 0, 255);
                }
            }

            // Send using replace or mix
            uint channel = 0;

            if (hapticClip.HapticMode == UxrHapticMode.Mix)
            {
                if (handSide == UxrHandSide.Left)
                {
                    _leftHapticChannel = (_leftHapticChannel + 1) % hapticCapabilities.numChannels;
                    channel            = _leftHapticChannel;
                }
                else
                {
                    _rightHapticChannel = (_rightHapticChannel + 1) % hapticCapabilities.numChannels;
                    channel             = _rightHapticChannel;
                }
            }
            else
            {
                inputDevice.StopHaptics();

                _leftHapticChannel  = 0;
                _rightHapticChannel = 0;
            }

            inputDevice.SendHapticBuffer(channel, hapticBuffer);
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide   handSide,
                                                float         frequency,
                                                float         amplitude,
                                                float         durationSeconds,
                                                UxrHapticMode hapticMode = UxrHapticMode.Mix)
        {
            InputDevice inputDevice = GetInputDevice(handSide);

            if (!inputDevice.isValid)
            {
                return;
            }

            if (!inputDevice.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities))
            {
                return;
            }

            // Setup using replace or mix
            uint channel = 0;

            if (hapticMode == UxrHapticMode.Mix)
            {
                if (handSide == UxrHandSide.Left)
                {
                    _leftHapticChannel = (_leftHapticChannel + 1) % hapticCapabilities.numChannels;
                    channel            = _leftHapticChannel;
                }
                else
                {
                    _rightHapticChannel = (_rightHapticChannel + 1) % hapticCapabilities.numChannels;
                    channel             = _rightHapticChannel;
                }
            }
            else
            {
                inputDevice.StopHaptics();

                _leftHapticChannel  = 0;
                _rightHapticChannel = 0;
            }

            // Send
            if (hapticCapabilities.supportsBuffer)
            {
                byte[] samples = new byte[(int)(hapticCapabilities.bufferFrequencyHz * durationSeconds)];
                int    steps   = frequency > 0.0f ? Mathf.RoundToInt(hapticCapabilities.bufferFrequencyHz / frequency) : -1;
                byte   sample  = (byte)Mathf.Clamp(amplitude * 255.0f, 0, 255.0f);

                for (int i = 0; i < samples.Length; ++i)
                {
                    if (steps < 2)
                    {
                        samples[i] = sample;
                    }
                    else
                    {
                        samples[i] = i % steps == 0 ? sample : (byte)0;
                    }
                }

                inputDevice.SendHapticBuffer(channel, samples);
            }
            else if (hapticCapabilities.supportsImpulse)
            {
                inputDevice.SendHapticImpulse(channel, amplitude, durationSeconds);
            }
        }

        /// <inheritdoc />
        public override void StopHapticFeedback(UxrHandSide handSide)
        {
            InputDevice inputDevice = GetInputDevice(handSide);

            if (!inputDevice.isValid)
            {
                return;
            }

            inputDevice.StopHaptics();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes variables and subscribes to events.
        ///     If the controllers were already initialized, enables the component. Otherwise it begins disabled until devices are
        ///     connected.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _leftHapticChannel  = 0;
            _rightHapticChannel = 0;

            if (enabled)
            {
                InputDevices.deviceConnected    += InputDevices_DeviceConnected;
                InputDevices.deviceDisconnected += InputDevices_DeviceDisconnected;

                // Check if the device is already connected. This may happen if a new scene was loaded, because
                // the connection events were already triggered and processed. We should have them registered in
                // our static fields.
                _deviceLeft  = s_activeInputDevices.FirstOrDefault(d => ControllerNames.Any(n => string.Equals(d.name, n)) && IsLeftController(d));
                _deviceRight = s_activeInputDevices.FirstOrDefault(d => ControllerNames.Any(n => string.Equals(d.name, n)) && IsRightController(d));

                List<InputDevice> devices = new List<InputDevice>();
                InputDevices.GetDevices(devices);

                if (!_deviceLeft.isValid)
                {
                    _deviceLeft = devices.FirstOrDefault(d => ControllerNames.Any(n => string.Equals(d.name, n)) && IsLeftController(d));
                }

                if (!_deviceRight.isValid)
                {
                    _deviceRight = devices.FirstOrDefault(d => ControllerNames.Any(n => string.Equals(d.name, n)) && IsRightController(d));
                }

                enabled             = _deviceLeft.isValid || _deviceRight.isValid;
                RaiseConnectOnStart = enabled;
            }
        }

        /// <summary>
        ///     Unsubscribes from device events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            InputDevices.deviceConnected    -= InputDevices_DeviceConnected;
            InputDevices.deviceDisconnected -= InputDevices_DeviceDisconnected;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Event called when a device is connected. Check for compatible devices.
        /// </summary>
        /// <param name="inputDevice">The device that was connected</param>
        private void InputDevices_DeviceConnected(InputDevice inputDevice)
        {
            // Check if device is compatible with component
            if (ForceUseAlways || ControllerNames.Any(n => string.Equals(n, inputDevice.name)))
            {
                // Found compatible device. Look for features.
                List<InputFeatureUsage> listFeatures = new List<InputFeatureUsage>();

                bool isController = false;

                // Check for controllers and side
                if (IsLeftController(inputDevice))
                {
                    // Left controller

                    if (LogLevel >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{InputClassName}::{nameof(InputDevices_DeviceConnected)}: Device name {inputDevice.name} was registered by {InputClassName} and is being processed as left controller. InputDevice.isValid={inputDevice.isValid}");
                    }

                    _deviceLeft  = inputDevice;
                    isController = true;
                }
                else if (IsRightController(inputDevice))
                {
                    // Right controller

                    if (LogLevel >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{InputClassName}::{nameof(InputDevices_DeviceConnected)}: Device name {inputDevice.name} was registered by {InputClassName} and is being processed as right controller. InputDevice.isValid={inputDevice.isValid}");
                    }

                    _deviceRight  = inputDevice;
                    isController  = true;
                }

                if (isController)
                {
                    // Register active device
                    s_activeInputDevices.Add(inputDevice);

                    if (!enabled)
                    {
                        // Component is disabled. Enable it and send Connected event.
                        enabled = true;
                        OnDeviceConnected(new UxrDeviceConnectEventArgs(true));
                    }
                }
            }
            else
            {
                // Check for controllers and side
                if (IsLeftController(inputDevice))
                {
                    // Left controller
                    
                    if (LogLevel >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{InputClassName}::{nameof(InputDevices_DeviceConnected)}: Left device connected unknown: {inputDevice.name}. InputDevice.isValid={inputDevice.isValid}");
                    }
                }
                else if (IsRightController(inputDevice))
                {
                    // Right controller
                    
                    if (LogLevel >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{InputClassName}::{nameof(InputDevices_DeviceConnected)}: Right device connected unknown: {inputDevice.name}. InputDevice.isValid={inputDevice.isValid}");
                    }
                }
            }
        }

        /// <summary>
        ///     Event called when a device is disconnected. We use it to update our internal lists.
        /// </summary>
        /// <param name="inputDevice">The device that was disconnected</param>
        private void InputDevices_DeviceDisconnected(InputDevice inputDevice)
        {
            // Check if device is compatible with component
            if (ForceUseAlways || ControllerNames.Any(n => string.Equals(n, inputDevice.name)))
            {
                if (string.Equals(inputDevice.serialNumber, _deviceLeft.serialNumber) || string.Equals(inputDevice.serialNumber, _deviceRight.serialNumber))
                {
                    if (LogLevel >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{InputClassName}::{nameof(InputDevices_DeviceDisconnected)}: Device name {inputDevice.name} was registered by {InputClassName} and is being disconnected. InputDevice.isValid={inputDevice.isValid}");
                    }
                }

                // Unregister device
                s_activeInputDevices.RemoveAll(i => string.Equals(i.name, inputDevice.name));

                // If last device was disconnected, disable component. Component will be re-enabled using connection event.
                if (enabled && !_deviceLeft.isValid && !_deviceRight.isValid)
                {
                    enabled = false;
                    OnDeviceConnected(new UxrDeviceConnectEventArgs(false));
                }
            }
        }

        #endregion

        #region Protected Overrides UxrControllerInput

        /// <summary>
        ///     Updates the input state. This should not be called by the user since it is called by the framework already.
        /// </summary>
        protected override void UpdateInput()
        {
            base.UpdateInput();

            bool buttonPressTriggerLeft        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Trigger,       ButtonContact.Press);
            bool buttonPressTriggerRight       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Trigger,       ButtonContact.Press);
            bool buttonPressJoystickLeft       = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Joystick,      ButtonContact.Press);
            bool buttonPressJoystickRight      = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Joystick,      ButtonContact.Press);
            bool buttonPressButton1Left        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Button1,       ButtonContact.Press);
            bool buttonPressButton1Right       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Button1,       ButtonContact.Press);
            bool buttonPressButton2Left        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Button2,       ButtonContact.Press);
            bool buttonPressButton2Right       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Button2,       ButtonContact.Press);
            bool buttonPressMenuLeft           = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Menu,          ButtonContact.Press);
            bool buttonPressMenuRight          = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Menu,          ButtonContact.Press);
            bool buttonPressGripLeft           = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Grip,          ButtonContact.Press);
            bool buttonPressGripRight          = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Grip,          ButtonContact.Press);
            bool buttonPressThumbCapSenseLeft  = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.ThumbCapSense, ButtonContact.Press);
            bool buttonPressThumbCapSenseRight = HasButtonContact(UxrHandSide.Right, UxrInputButtons.ThumbCapSense, ButtonContact.Press);

            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Trigger,       buttonPressTriggerLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Trigger,       buttonPressTriggerRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Joystick,      buttonPressJoystickLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Joystick,      buttonPressJoystickRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Button1,       buttonPressButton1Left);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Button1,       buttonPressButton1Right);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Button2,       buttonPressButton2Left);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Button2,       buttonPressButton2Right);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Menu,          buttonPressMenuLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Menu,          buttonPressMenuRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Grip,          buttonPressGripLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Grip,          buttonPressGripRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.ThumbCapSense, buttonPressThumbCapSenseLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.ThumbCapSense, buttonPressThumbCapSenseRight);

            bool buttonTouchTriggerLeft        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Trigger,       ButtonContact.Touch);
            bool buttonTouchTriggerRight       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Trigger,       ButtonContact.Touch);
            bool buttonTouchJoystickLeft       = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Joystick,      ButtonContact.Touch);
            bool buttonTouchJoystickRight      = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Joystick,      ButtonContact.Touch);
            bool buttonTouchButton1Left        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Button1,       ButtonContact.Touch);
            bool buttonTouchButton1Right       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Button1,       ButtonContact.Touch);
            bool buttonTouchButton2Left        = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Button2,       ButtonContact.Touch);
            bool buttonTouchButton2Right       = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Button2,       ButtonContact.Touch);
            bool buttonTouchMenuLeft           = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Menu,          ButtonContact.Touch);
            bool buttonTouchMenuRight          = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Menu,          ButtonContact.Touch);
            bool buttonTouchGripLeft           = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.Grip,          ButtonContact.Touch);
            bool buttonTouchGripRight          = HasButtonContact(UxrHandSide.Right, UxrInputButtons.Grip,          ButtonContact.Touch);
            bool buttonTouchThumbCapSenseLeft  = HasButtonContact(UxrHandSide.Left,  UxrInputButtons.ThumbCapSense, ButtonContact.Touch);
            bool buttonTouchThumbCapSenseRight = HasButtonContact(UxrHandSide.Right, UxrInputButtons.ThumbCapSense, ButtonContact.Touch);

            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Trigger,       buttonTouchTriggerLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Trigger,       buttonTouchTriggerRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Joystick,      buttonTouchJoystickLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Joystick,      buttonTouchJoystickRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Button1,       buttonTouchButton1Left);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Button1,       buttonTouchButton1Right);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Button2,       buttonTouchButton2Left);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Button2,       buttonTouchButton2Right);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Menu,          buttonTouchMenuLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Menu,          buttonTouchMenuRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.Grip,          buttonTouchGripLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Grip,          buttonTouchGripRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft,  UxrInputButtons.ThumbCapSense, buttonTouchThumbCapSenseLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.ThumbCapSense, buttonTouchThumbCapSenseRight);

            Vector2 leftJoystick = GetInput2D(UxrHandSide.Left, UxrInput2D.Joystick, true);
            Vector2 leftDPad     = leftJoystick; // Mapped to joystick by default

            if (leftJoystick != Vector2.zero && leftJoystick.magnitude > AnalogAsDPadThreshold)
            {
                float joystickAngle = Input2DToAngle(leftJoystick);

                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,  IsInput2dDPadLeft(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, IsInput2dDPadRight(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,    IsInput2dDPadUp(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,  IsInput2dDPadDown(joystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,  false);
            }

            if (leftDPad != Vector2.zero && leftDPad.magnitude > AnalogAsDPadThreshold)
            {
                float dPadAngle = Input2DToAngle(leftDPad);

                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  IsInput2dDPadLeft(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, IsInput2dDPadRight(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    IsInput2dDPadUp(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  IsInput2dDPadDown(dPadAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  false);
            }

            Vector2 rightJoystick = GetInput2D(UxrHandSide.Right, UxrInput2D.Joystick, true);
            Vector2 rightDPad     = rightJoystick; // Mapped to joystick by default

            if (rightJoystick != Vector2.zero && rightJoystick.magnitude > AnalogAsDPadThreshold)
            {
                float joystickAngle = Input2DToAngle(rightJoystick);

                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickLeft,  IsInput2dDPadLeft(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickRight, IsInput2dDPadRight(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickUp,    IsInput2dDPadUp(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickDown,  IsInput2dDPadDown(joystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickDown,  false);
            }

            if (rightDPad != Vector2.zero && rightDPad.magnitude > AnalogAsDPadThreshold)
            {
                float dPadAngle = Input2DToAngle(rightDPad);

                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft,  IsInput2dDPadLeft(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, IsInput2dDPadRight(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp,    IsInput2dDPadUp(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown,  IsInput2dDPadDown(dPadAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown,  false);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Gets the input device interface in Unity's input system for a given hand.
        ///     Usually if it is a left+right setup it will give a list with a single entry but the system is very generic
        ///     so it is prepared to handle different setups.
        ///     Normally we get the list and just use the first entry if available.
        /// </summary>
        /// <param name="handSide">Hand to get the input devices for</param>
        /// <returns><see cref="InputDevice" /> representing the input device</returns>
        protected InputDevice GetInputDevice(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? _deviceLeft : _deviceRight;
        }

        /// <summary>
        ///     Using an audio file, creates a haptic samples buffer that can be sent for feedback.
        ///     This code is based on the Oculus SDK (OVRHaptics.cs).
        /// </summary>
        /// <param name="inputDevice">Unity input device that will be the feedback target</param>
        /// <param name="audioClip">Audio clip whose audio sample will be used to create haptics</param>
        /// <returns>Buffer that can be sent to the device as haptic feedback</returns>
        protected byte[] CreateHapticBufferFromAudioClip(InputDevice inputDevice, AudioClip audioClip)
        {
            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);

            if (!inputDevice.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities))
            {
                return null;
            }

            double stepSizePrecise = (audioClip.frequency + 1e-6) / hapticCapabilities.bufferFrequencyHz;

            if (stepSizePrecise < 1.0)
            {
                return null;
            }

            int    stepSize      = (int)stepSizePrecise;
            double stepSizeError = stepSizePrecise - stepSize;
            int    length        = audioData.Length;

            double accumStepSizeError = 0.0f;
            byte[] samples            = new byte[length];
            int    i                  = 0;
            int    s                  = 0;

            while (i < length)
            {
                byte sample = (byte)(Mathf.Clamp01(Mathf.Abs(audioData[i])) * byte.MaxValue);

                if (s < samples.Length)
                {
                    samples[s] = sample;
                    s++;
                }
                else
                {
                    break;
                }

                i                  += stepSize * audioClip.channels;
                accumStepSizeError += stepSizeError;
                if ((int)accumStepSizeError > 0)
                {
                    i                  += (int)accumStepSizeError * audioClip.channels;
                    accumStepSizeError =  accumStepSizeError - (int)accumStepSizeError;
                }
            }

            return samples;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets if the given input device is a left side VR controller
        /// </summary>
        /// <param name="inputDevice">Device to check</param>
        /// <returns>Whether the given input device is a left side VR controller</returns>
        private static bool IsLeftController(InputDevice inputDevice)
        {
            return inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left);
        }

        /// <summary>
        ///     Gets if the given input device is a right side VR controller
        /// </summary>
        /// <param name="inputDevice">Device to check</param>
        /// <returns>Whether the given input device is a right side VR controller</returns>
        private static bool IsRightController(InputDevice inputDevice)
        {
            return inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right);
        }

        /// <summary>
        ///     Checks whether the given button in a controller is currently being touched or pressed.
        /// </summary>
        /// <param name="handSide">Which controller side to check</param>
        /// <param name="button">Button to check</param>
        /// <param name="buttonContact">Type of contact to check for (touch or press)</param>
        /// <returns>Boolean telling whether the specified button has contact</returns>
        private bool HasButtonContact(UxrHandSide handSide, UxrInputButtons button, ButtonContact buttonContact)
        {
            InputDevice inputDevice = GetInputDevice(handSide);
            if (!inputDevice.isValid)
            {
                return false;
            }

            if (button == UxrInputButtons.Joystick)
            {
                var featureUsage = buttonContact == ButtonContact.Press ? CommonUsages.primary2DAxisClick : CommonUsages.primary2DAxisTouch;
                if (inputDevice.TryGetFeatureValue(featureUsage, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.Joystick2)
            {
                return false;
            }
            else if (button == UxrInputButtons.Trigger)
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float valueFloat))
                {
                    // We try getting the float value first because in analog buttons like the oculus it will trigger too early with the bool version.
                    return valueFloat > AnalogAsDPadThreshold;
                }

                if (inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.Trigger2)
            {
                return false;
            }
            else if (button == UxrInputButtons.Grip)
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.Button1)
            {
                var featureUsage = buttonContact == ButtonContact.Press ? CommonUsages.primaryButton : CommonUsages.primaryTouch;
                if (inputDevice.TryGetFeatureValue(featureUsage, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.Button2)
            {
                var featureUsage = buttonContact == ButtonContact.Press ? CommonUsages.secondaryButton : CommonUsages.secondaryTouch;
                if (inputDevice.TryGetFeatureValue(featureUsage, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.Menu)
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.menuButton, out bool value))
                {
                    return value;
                }
            }
            else if (button == UxrInputButtons.ThumbCapSense)
            {
                if (buttonContact == ButtonContact.Press)
                {
                    if (inputDevice.TryGetFeatureValue(CommonUsages.thumbrest, out bool value))
                    {
                        return value;
                    }
                }
                else if (buttonContact == ButtonContact.Touch)
                {
                    if (inputDevice.TryGetFeatureValue(CommonUsages.thumbTouch, out float floatValue))
                    {
                        return floatValue > 0.0f;
                    }
                }
            }
            else if (button == UxrInputButtons.IndexCapSense)
            {
            }
            else if (button == UxrInputButtons.MiddleCapSense)
            {
            }
            else if (button == UxrInputButtons.RingCapSense)
            {
            }
            else if (button == UxrInputButtons.LittleCapSense)
            {
            }

            return false;
        }

        #endregion

        #region Private Types & Data

        private string InputClassName => GetType().Name;

        private static readonly List<InputDevice> s_activeInputDevices = new List<InputDevice>();

        private InputDevice _deviceLeft;
        private InputDevice _deviceRight;
        private uint        _leftHapticChannel;
        private uint        _rightHapticChannel;

        #endregion
    }
}