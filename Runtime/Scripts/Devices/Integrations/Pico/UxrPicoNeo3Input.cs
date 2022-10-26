// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPicoNeo3Input.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.Pico
{
    /// <summary>
    ///     Pico Neo 3 controller input using PicoXR SDK.
    /// </summary>
    public class UxrPicoNeo3Input : UxrUnityXRControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <summary>
        ///     Gets the SDK dependency: PicoXR.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkPicoXR;

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <inheritdoc />
        public override bool MainJoystickIsTouchpad => false;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick |
                                        UxrControllerElements.Grip |
                                        UxrControllerElements.Trigger |
                                        UxrControllerElements.Button1 |
                                        UxrControllerElements.Button2 |
                                        UxrControllerElements.Menu |
                                        UxrControllerElements.Back |
                                        UxrControllerElements.DPad);

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        #endregion

        #region Public Overrides UxrUnityXRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return "PicoXR Controller-Left";
                yield return "PicoXR Controller-Right";
                yield return "PICO Controller-Left";
                yield return "PICO Controller-Right";
            }
        }

        #endregion
    }
}