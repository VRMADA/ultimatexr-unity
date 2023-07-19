using System;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Haptics;
using UnityEngine;
#if ULTIMATEXR_USE_WEBXR_SDK
using WebXR;
#endif
namespace UltimateXR.Devices.Integrations.WebXR
{
    public abstract class UxrWebXRControllerInput : UxrControllerInput
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets list of controller names that the component can handle.
        /// </summary>
        public abstract IEnumerable<string> ControllerNames { get; }

        /// <summary>
        ///     Gets if the class will use hand skeletons.
        /// </summary>
        public virtual bool UsesHandSkeletons => false;

        #endregion
        #region Inspector Properties/Serialized Fields
#if ULTIMATEXR_USE_WEBXR_SDK
        /// <summary>
        /// The controller left, have the control behavior like buttons pressed, vibration, touch, etc.
        /// </summary>
        private WebXRController leftWebXRController;

        /// <summary>
        /// The controller right, have the control behavior like buttons pressed, vibration, touch, etc.
        /// </summary>
        private WebXRController rightWebXRController;
#endif
        #endregion
        #region Unity
        protected override void Awake()
        {
            base.Awake();
#if ULTIMATEXR_USE_WEBXR_SDK
            // Create the necessary WebXRManager and WebXRControllers objects
            if (WebXRManager.Instance == null)
            {
                new GameObject("WebXRManager", typeof(WebXRManager));
            }

            InstantiateWebXRController(WebXRControllerHand.LEFT, ref leftWebXRController);
            InstantiateWebXRController(WebXRControllerHand.RIGHT, ref rightWebXRController);
#endif
        }
        #endregion
        #region Public Overrides UxrControllerInput
        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <summary>
        /// Require WebXR SDK to access functionality.
        /// </summary>
        public override string SDKDependency => "WebXR";

        /// <inheritdoc />
        public override bool IsControllerEnabled(UxrHandSide handSide)
        {
#if ULTIMATEXR_USE_WEBXR_SDK
            bool isControllerActive = handSide == UxrHandSide.Left ? leftWebXRController.isControllerActive : rightWebXRController.isControllerActive;
            return isControllerActive;
#else
            return false;
#endif
        }
        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0.0f;
            }

#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRController source = handSide == UxrHandSide.Left ? leftWebXRController : rightWebXRController;
            if (input1D == UxrInput1D.Grip)
            {
                return source.GetAxis(WebXRController.AxisTypes.Grip);
            }
            else if (input1D == UxrInput1D.Trigger)
            {
                return source.GetAxis(WebXRController.AxisTypes.Trigger);
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

#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRController source = handSide == UxrHandSide.Left ? leftWebXRController : rightWebXRController;
            if (input2D == UxrInput2D.Joystick)
            {
                return source.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            }
            else if (input2D == UxrInput2D.Joystick2)
            {
                return source.GetAxis2D(WebXRController.Axis2DTypes.Touchpad);
            }
#endif
            return Vector2.zero;
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide handSide, UxrHapticClip hapticClip)
        {
            if (hapticClip.FallbackClipType != UxrHapticClipType.None)
            {
                SendHapticFeedback(handSide, hapticClip.FallbackClipType, hapticClip.FallbackAmplitude, hapticClip.FallbackDurationSeconds, hapticClip.HapticMode);
            }
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide handSide,
                                                float frequency,
                                                float amplitude,
                                                float durationSeconds,
                                                UxrHapticMode hapticMode = UxrHapticMode.Mix)
        {
#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRController source = handSide == UxrHandSide.Left ? leftWebXRController : rightWebXRController;
            source.Pulse(frequency, durationSeconds);
#endif
        }

        /// <inheritdoc />
        public override void StopHapticFeedback(UxrHandSide handSide)
        {
            // TODO. Doesn't seem to be supported.
        }
#if ULTIMATEXR_USE_WEBXR_SDK
        /// <summary>
        /// Instantiates a WebXRController object based on the provided hand type.
        /// Assigns the instantiated object to the ref parameter 'webXRController'.
        /// </summary>
        /// <param name="webXRControllerHand">The hand side configuration</param>
        /// <param name="webXRController">The instantiated WebXRController</param>
        private void InstantiateWebXRController(WebXRControllerHand webXRControllerHand, ref WebXRController webXRController)
        {
            // Create a new game object with a name based on the hand type (left or right).
            webXRController = new GameObject($"WebXRController_{(webXRControllerHand == WebXRControllerHand.LEFT ? "Left" : "Right")}")
                .AddComponent<WebXRController>();

            // Set the parent of the instantiated object to the current object's transform.
            webXRController.transform.parent = transform;

            // Deactivate the new WebXRController game object to configure it before activation.
            webXRController.gameObject.SetActive(false);

            // Set the hand property of the WebXRController to the provided hand type.
            webXRController.hand = webXRControllerHand;

            // Activate the WebXRController game object.
            webXRController.gameObject.SetActive(true);
        }
#endif

        #endregion
        #region Protected Overrides UxrControllerInput
        /// <inheritdoc />
        protected override void UpdateInput()
        {
#if ULTIMATEXR_USE_WEBXR_SDK
            bool buttonPressTriggerLeft = leftWebXRController.GetButton(WebXRController.ButtonTypes.Trigger);
            bool buttonPressTriggerRight = rightWebXRController.GetButton(WebXRController.ButtonTypes.Trigger);
            bool buttonPressJoystickLeft = leftWebXRController.GetButton(WebXRController.ButtonTypes.Thumbstick);
            bool buttonPressJoystickRight = rightWebXRController.GetButton(WebXRController.ButtonTypes.Thumbstick);
            bool buttonPressButton1Left = leftWebXRController.GetButton(WebXRController.ButtonTypes.ButtonA);
            bool buttonPressButton1Right = rightWebXRController.GetButton(WebXRController.ButtonTypes.ButtonA);
            bool buttonPressButton2Left = leftWebXRController.GetButton(WebXRController.ButtonTypes.ButtonB);
            bool buttonPressButton2Right = rightWebXRController.GetButton(WebXRController.ButtonTypes.ButtonB);
            bool buttonPressGripLeft = leftWebXRController.GetButton(WebXRController.ButtonTypes.Grip);
            bool buttonPressGripRight = rightWebXRController.GetButton(WebXRController.ButtonTypes.Grip);

            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Trigger, buttonPressTriggerLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Trigger, buttonPressTriggerRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Joystick, buttonPressJoystickLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Joystick, buttonPressJoystickRight);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button1, buttonPressButton1Left);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Button1, buttonPressButton1Right);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Button2, buttonPressButton2Left);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Button2, buttonPressButton2Right);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.Grip, buttonPressGripLeft);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Grip, buttonPressGripRight);

            bool buttonTouchTriggerLeft = leftWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Trigger);
            bool buttonTouchTriggerRight = rightWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Trigger);
            bool buttonTouchJoystickLeft = leftWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Thumbstick);
            bool buttonTouchJoystickRight = rightWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Thumbstick);
            bool buttonTouchButton1Left = leftWebXRController.GetButtonTouched(WebXRController.ButtonTypes.ButtonA);
            bool buttonTouchButton1Right = rightWebXRController.GetButtonTouched(WebXRController.ButtonTypes.ButtonA);
            bool buttonTouchButton2Left = leftWebXRController.GetButtonTouched(WebXRController.ButtonTypes.ButtonB);
            bool buttonTouchButton2Right = rightWebXRController.GetButtonTouched(WebXRController.ButtonTypes.ButtonB);
            bool buttonTouchGripLeft = leftWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Grip);
            bool buttonTouchGripRight = rightWebXRController.GetButtonTouched(WebXRController.ButtonTypes.Grip);

            SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Trigger, buttonTouchTriggerLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Trigger, buttonTouchTriggerRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Joystick, buttonTouchJoystickLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Joystick, buttonTouchJoystickRight);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Button1, buttonTouchButton1Left);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Button1, buttonTouchButton1Right);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Button2, buttonTouchButton2Left);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Button2, buttonTouchButton2Right);
            SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Grip, buttonTouchGripLeft);
            SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Grip, buttonTouchGripRight);

            Vector2 leftJoystick = GetInput2D(UxrHandSide.Left, UxrInput2D.Joystick, true);
            Vector2 leftDPad = leftJoystick; // Mapped to joystick by default

            if (leftJoystick != Vector2.zero && leftJoystick.magnitude > AnalogAsDPadThreshold)
            {
                float joystickAngle = Input2DToAngle(leftJoystick);

                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft, IsInput2dDPadLeft(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, IsInput2dDPadRight(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp, IsInput2dDPadUp(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown, IsInput2dDPadDown(joystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown, false);
            }

            if (leftDPad != Vector2.zero && leftDPad.magnitude > AnalogAsDPadThreshold)
            {
                float dPadAngle = Input2DToAngle(leftDPad);

                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft, IsInput2dDPadLeft(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, IsInput2dDPadRight(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp, IsInput2dDPadUp(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown, IsInput2dDPadDown(dPadAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown, false);
            }

            Vector2 rightJoystick = GetInput2D(UxrHandSide.Right, UxrInput2D.Joystick, true);
            Vector2 rightDPad = rightJoystick; // Mapped to joystick by default

            if (rightJoystick != Vector2.zero && rightJoystick.magnitude > AnalogAsDPadThreshold)
            {
                float joystickAngle = Input2DToAngle(rightJoystick);

                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickLeft, IsInput2dDPadLeft(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickRight, IsInput2dDPadRight(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickUp, IsInput2dDPadUp(joystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickDown, IsInput2dDPadDown(joystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickLeft, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickUp, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickDown, false);
            }

            if (rightDPad != Vector2.zero && rightDPad.magnitude > AnalogAsDPadThreshold)
            {
                float dPadAngle = Input2DToAngle(rightDPad);

                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft, IsInput2dDPadLeft(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, IsInput2dDPadRight(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp, IsInput2dDPadUp(dPadAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown, IsInput2dDPadDown(dPadAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown, false);
            }
#endif
        }
        #endregion
    }
}
