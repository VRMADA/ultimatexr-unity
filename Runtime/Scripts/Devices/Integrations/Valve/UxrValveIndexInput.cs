// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrValveIndexInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.Valve
{
    /// <summary>
    ///     Valve index controllers. Also known as Knuckles.
    /// </summary>
    public class UxrValveIndexInput : UxrSteamVRControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool MainJoystickIsTouchpad => false;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick |  // Joystick
                                        UxrControllerElements.Joystick2 | // Trackpad
                                        UxrControllerElements.Grip |      // Grip
                                        UxrControllerElements.Trigger |   // Trigger
                                        UxrControllerElements.Button1 |   // Button A
                                        UxrControllerElements.Button2 |   // Button B
                                        UxrControllerElements.DPad);      // Joystick

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            return UxrControllerInputCapabilities.HapticImpulses | UxrControllerInputCapabilities.TrackedHandPose;
        }

        #endregion

        #region Public Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return "indexcontroller";
                yield return "Knuckles EV3.0 Left";
                yield return "Knuckles EV3.0 Right";
                yield return "Knuckles EV2.0 Left";
                yield return "Knuckles EV2.0 Right";
                yield return "Knuckles Left";
                yield return "Knuckles Right";
            }
        }

        /// <inheritdoc />
        public override bool UsesHandSkeletons => true;

        #endregion

        #region Protected Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        protected override void UpdateInput()
        {
            base.UpdateInput();

            // Propagate touchpad touch to press, since only touch is signaled by the API
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Joystick2, GetButtonsTouch(UxrHandSide.Left,  UxrInputButtons.Joystick2));
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Joystick2, GetButtonsTouch(UxrHandSide.Right, UxrInputButtons.Joystick2));

            // Propagate grip touch to press, since only touch is signaled by the API
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Grip, GetButtonsTouch(UxrHandSide.Left,  UxrInputButtons.Grip));
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Grip, GetButtonsTouch(UxrHandSide.Right, UxrInputButtons.Grip));
        }

        #endregion
    }
}