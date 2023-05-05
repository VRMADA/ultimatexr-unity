// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGamepadInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEngine;
#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
using UnityEngine.InputSystem;
#endif

namespace UltimateXR.Devices.Integrations
{
    /// <summary>
    ///     Standard X-box like gamepad input.
    /// </summary>
    public class UxrGamepadInput : UxrControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkUnityInputSystem;

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Single;

        /// <inheritdoc />
        public override bool IsHandednessSupported => false;

        /// <inheritdoc />
        public override bool IsControllerEnabled(UxrHandSide handSide)
        {
#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            return Gamepad.current != null;
#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick |  // Left thumbstick
                                        UxrControllerElements.Joystick2 | // Right thumbstick
                                        UxrControllerElements.Trigger |   // Left index trigger
                                        UxrControllerElements.Trigger2 |  // Right index trigger
                                        UxrControllerElements.Button1 |   // Button 1
                                        UxrControllerElements.Button2 |   // Button 2
                                        UxrControllerElements.Button3 |   // Button 3
                                        UxrControllerElements.Button4 |   // Button 4
                                        UxrControllerElements.Bumper |    // Left shoulder button
                                        UxrControllerElements.Bumper2 |   // Right shoulder button
                                        UxrControllerElements.Back |      // Left system button
                                        UxrControllerElements.Menu |      // Right system button
                                        UxrControllerElements.DPad);      // Digital pad

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            return UxrControllerInputCapabilities.HapticImpulses;
        }

        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0.0f;
            }

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            switch (input1D)
            {
                case UxrInput1D.Trigger: return Gamepad.current.leftTrigger.ReadValue();

                case UxrInput1D.Trigger2: return Gamepad.current.rightTrigger.ReadValue();
            }
#endif
            return 0.0f;
        }

        /// <inheritdoc />
        public override Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return Vector2.zero;
            }

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            switch (input2D)
            {
                case UxrInput2D.Joystick: return Gamepad.current.leftStick.ReadValue();

                case UxrInput2D.Joystick2: return Gamepad.current.rightStick.ReadValue();
            }
#else
            switch (input2D)
            {
                case UxrInput2D.Joystick: return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                case UxrInput2D.Joystick2: return Vector2.zero;
            }
#endif
            return Vector2.zero;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to device events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (enabled)
            {
#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
                InputSystem.onDeviceChange += InputSystem_DeviceChanged;
                enabled                    =  _gamepad != null;
#else
                enabled = false;
#endif
                RaiseConnectOnStart = enabled;
            }
        }

        /// <summary>
        ///     Unsubscribes from device events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            InputSystem.onDeviceChange -= InputSystem_DeviceChanged;
#endif
        }

        #endregion

        #region Event Handling Methods

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK

        /// <summary>
        ///     Handles the <see cref="InputSystem.onDeviceChange" /> event.
        /// </summary>
        /// <param name="device">Device changed</param>
        /// <param name="deviceChange">The change</param>
        private void InputSystem_DeviceChanged(InputDevice device, InputDeviceChange deviceChange)
        {
            if (enabled == false && Gamepad.current != null)
            {
                // If component is disabled and gamepad is available, act as connect
                enabled  = true;
                _gamepad = Gamepad.current;
                OnDeviceConnected(new UxrDeviceConnectEventArgs(true));
            }
            else if (enabled && Gamepad.current == null)
            {
                // If component is enabled and gamepad is unavailable, act as disconnect
                enabled  = false;
                _gamepad = null;
                OnDeviceConnected(new UxrDeviceConnectEventArgs(false));
            }
        }
#endif

        #endregion

        #region Protected Overrides UxrControllerInput

        /// <inheritdoc />
        protected override void UpdateInput()
        {
            base.UpdateInput();

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            if (Gamepad.current == null)
            {
                return;
            }

            float leftJoystickAngle  = Input2DToAngle(Gamepad.current.leftStick.ReadValue());
            float rightJoystickAngle = Input2DToAngle(Gamepad.current.rightStick.ReadValue());

            // For single devices where there is no handedness, IUxrControllerInputUpdater.UpdateInput() expects the left button flags to be updated
            // and it will take care of copying them to the right so that both hands return the same input.

            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,   IsInput2dDPadLeft(leftJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight,  IsInput2dDPadRight(leftJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,     IsInput2dDPadUp(leftJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,   IsInput2dDPadDown(leftJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Left,  IsInput2dDPadLeft(rightJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Right, IsInput2dDPadRight(rightJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Up,    IsInput2dDPadUp(rightJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Down,  IsInput2dDPadDown(rightJoystickAngle));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Trigger,        Gamepad.current.leftTrigger.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Trigger2,       Gamepad.current.rightTrigger.ReadValue() > AnalogAsDPadThreshold);
#else
            Vector2 leftJoystick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (leftJoystick != Vector2.zero && leftJoystick.magnitude > AnalogAsDPadThreshold)
            {
                float joystickAngle = Input2DToAngle(leftJoystick);

                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,  IsInput2dDPadLeft(joystickAngle));
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, IsInput2dDPadRight(joystickAngle));
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,    IsInput2dDPadUp(joystickAngle));
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,  IsInput2dDPadDown(joystickAngle));
            }
            else
            {
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,  false);
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, false);
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,    false);
                this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,  false);
            }

            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Left,  false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Right, false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Up,    false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2Down,  false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Trigger,        false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Trigger2,       false);
#endif

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick,  Gamepad.current.leftStickButton.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2, Gamepad.current.rightStickButton.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper,    Gamepad.current.leftShoulder.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper2,   Gamepad.current.rightShoulder.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button1,   Gamepad.current.buttonSouth.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button2,   Gamepad.current.buttonEast.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button3,   Gamepad.current.buttonWest.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button4,   Gamepad.current.buttonNorth.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Back,      Gamepad.current.selectButton.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Menu,      Gamepad.current.startButton.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  Gamepad.current.dpad.left.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, Gamepad.current.dpad.right.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    Gamepad.current.dpad.up.ReadValue() > AnalogAsDPadThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  Gamepad.current.dpad.down.ReadValue() > AnalogAsDPadThreshold);

#elif UNITY_STANDALONE_OSX
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick,  Input.GetKey(KeyCode.JoystickButton11));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2, Input.GetKey(KeyCode.JoystickButton12));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper,    Input.GetKey(KeyCode.JoystickButton13));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper2,   Input.GetKey(KeyCode.JoystickButton14));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button1,   Input.GetKey(KeyCode.JoystickButton16));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button2,   Input.GetKey(KeyCode.JoystickButton17));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button3,   Input.GetKey(KeyCode.JoystickButton18));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button4,   Input.GetKey(KeyCode.JoystickButton19));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Menu,      Input.GetKey(KeyCode.JoystickButton9));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  Input.GetKey(KeyCode.JoystickButton7));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, Input.GetKey(KeyCode.JoystickButton8));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    Input.GetKey(KeyCode.JoystickButton5));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  Input.GetKey(KeyCode.JoystickButton6));

#elif UNITY_STANDALONE_LINUX
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick,  Input.GetKey(KeyCode.JoystickButton9));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2, Input.GetKey(KeyCode.JoystickButton10));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper,    Input.GetKey(KeyCode.JoystickButton4));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper2,   Input.GetKey(KeyCode.JoystickButton5));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button1,   Input.GetKey(KeyCode.JoystickButton0));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button2,   Input.GetKey(KeyCode.JoystickButton1));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button3,   Input.GetKey(KeyCode.JoystickButton2));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button4,   Input.GetKey(KeyCode.JoystickButton3));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Menu,      Input.GetKey(KeyCode.JoystickButton7));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  Input.GetKey(KeyCode.JoystickButton11));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, Input.GetKey(KeyCode.JoystickButton12));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    Input.GetKey(KeyCode.JoystickButton13));
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  Input.GetKey(KeyCode.JoystickButton14));

#else
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick,  Input.GetKey(KeyCode.JoystickButton8));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick2, Input.GetKey(KeyCode.JoystickButton9));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper,    Input.GetKey(KeyCode.JoystickButton4));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Bumper2,   Input.GetKey(KeyCode.JoystickButton5));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button1,   Input.GetKey(KeyCode.JoystickButton0));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button2,   Input.GetKey(KeyCode.JoystickButton1));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button3,   Input.GetKey(KeyCode.JoystickButton2));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button4,   Input.GetKey(KeyCode.JoystickButton3));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Back,      Input.GetKey(KeyCode.JoystickButton6));
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Menu,      Input.GetKey(KeyCode.JoystickButton7));

            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    false);
            this.SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  false);
#endif
        }

        #endregion

        #region Private Types & Data

#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
        private Gamepad _gamepad;
#endif

        #endregion
    }
}