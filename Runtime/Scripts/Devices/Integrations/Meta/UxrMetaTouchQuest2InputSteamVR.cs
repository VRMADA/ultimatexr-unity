// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMetaTouchQuest2InputSteamVR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.Oculus
{
    /// <summary>
    ///     Oculus Touch controllers input using SteamVR.
    /// </summary>
    public class UxrMetaTouchQuest2InputSteamVR : UxrSteamVRControllerInput
    {
        #region Public Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return "Oculus Quest2 (Left Controller)";
                yield return "Oculus Quest2 (Right Controller)";
            }
        }

        #endregion

        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool MainJoystickIsTouchpad => false;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick | // Joystick
                                        UxrControllerElements.Grip |     // Grip
                                        UxrControllerElements.Trigger |  // Trigger
                                        UxrControllerElements.Button1 |  // Button A
                                        UxrControllerElements.Button2 |  // Button B
                                        UxrControllerElements.DPad);

            if (handSide == UxrHandSide.Left)
            {
                validElements |= (uint)UxrControllerElements.Menu;
            }

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