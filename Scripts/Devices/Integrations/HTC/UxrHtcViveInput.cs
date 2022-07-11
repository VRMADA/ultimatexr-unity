// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     HTC Vive controllers input using SteamVR.
    /// </summary>
    public class UxrHtcViveInput : UxrSteamVRControllerInput
    {
        #region Public Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return "controller_vive";
                yield return "Vive. Controller MV";
                yield return "Vive. Controller Pro MV";
                yield return "VIVE Controller MV";
                yield return "VIVE Controller Pro MV";

            }
        }

        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            // Since Vive controllers don't have an analog grip button, make sure the analog
            // grip functionality is supported.
            if (input1D == UxrInput1D.Grip)
            {
                return GetButtonsPress(handSide, UxrInputButtons.Grip, getIgnoredInput) ? 1.0f : 0.0f;
            }

            return 0.0f;
        }

        #endregion

        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool MainJoystickIsTouchpad => true;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick | // Joystick
                                        UxrControllerElements.Grip |     // Grip
                                        UxrControllerElements.Trigger |  // Trigger
                                        UxrControllerElements.Button1 |  // Button A
                                        UxrControllerElements.DPad);     // Joystick

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            return UxrControllerInputCapabilities.HapticImpulses;
        }

        #endregion
    }
}