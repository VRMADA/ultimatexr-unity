// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveCosmosInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     HTC Vive Cosmos controllers input using SteamVR.
    /// </summary>
    public class UxrHtcViveCosmosInput : UxrSteamVRControllerInput
    {
        #region Public Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get { yield return ControllerNameHtcViveCosmos; }
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
                                        UxrControllerElements.Bumper |   // Bumper
                                        UxrControllerElements.Trigger |  // Trigger
                                        UxrControllerElements.Button1 |  // Button A/X
                                        UxrControllerElements.Button2 |  // Button B/Y
                                        UxrControllerElements.DPad);     // Joystick

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            return UxrControllerInputCapabilities.HapticImpulses;
        }

        #endregion

        #region Private Types & Data

        private const string ControllerNameHtcViveCosmos = "vive_cosmos_controller";

        #endregion
    }
}