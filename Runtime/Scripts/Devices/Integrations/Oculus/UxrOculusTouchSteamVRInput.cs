// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrOculusTouchSteamVRInput.cs" company="VRMADA">
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
    public class UxrOculusTouchSteamVRInput : UxrSteamVRControllerInput
    {
        #region Public Overrides UxrSteamVRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                yield return ControllerOculusTouchNameLeft;
                yield return ControllerOculusTouchNameRight;
                yield return ControllerOculusTouchQuestNameLeft;
                yield return ControllerOculusTouchQuestNameRight;
                yield return ControllerOculusTouchQuest2NameLeft;
                yield return ControllerOculusTouchQuest2NameRight;
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

        #region Private Types & Data

        private const string ControllerOculusTouchNameLeft        = "oculus_touch_controller_left";
        private const string ControllerOculusTouchNameRight       = "oculus_touch_controller_right";
        private const string ControllerOculusTouchQuestNameLeft   = "oculus_quest_controller_left";
        private const string ControllerOculusTouchQuestNameRight  = "oculus_quest_controller_right";
        private const string ControllerOculusTouchQuest2NameLeft  = "oculus_quest2_controller_left";
        private const string ControllerOculusTouchQuest2NameRight = "oculus_quest2_controller_right";

        #endregion
    }
}