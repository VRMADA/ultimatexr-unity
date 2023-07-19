using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltimateXR.Core;
using UltimateXR.Devices;

namespace UltimateXR.Devices.Integrations.WebXR
{
    public class UxrWebXRInput : UxrWebXRControllerInput
    {
        #region Public Overrides UxrControllerInput
        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return "Oculus Touch Controller - Left";
                yield return "Oculus Touch Controller - Right";

            }
        }
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
