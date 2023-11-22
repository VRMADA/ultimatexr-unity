// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMetaTouchQuest2Input.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.Meta
{
    /// <summary>
    ///     Oculus Touch controller input using Oculus SDK.
    /// </summary>
    public class UxrMetaTouchQuest2Input : UxrUnityXRControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <summary>
        ///     Gets the SDK dependency: Oculus SDK.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkOculus;

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
                                        UxrControllerElements.ThumbCapSense |
                                        UxrControllerElements.Button1 |
                                        UxrControllerElements.Button2 |
                                        UxrControllerElements.Menu |
                                        UxrControllerElements.DPad);

            if (handSide == UxrHandSide.Right)
            {
                // Remove menu button from right controller, which is reserved.
                validElements = validElements & ~(uint)UxrControllerElements.Menu;
            }

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        #endregion

        #region Public Overrides UxrUnityXRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get
            {
                if (UxrTrackingDevice.HeadsetDeviceName is "Oculus Quest2" || 
                    UxrTrackingDevice.HeadsetDeviceName is "Meta Quest 3")
                {
                    yield return "Oculus Touch Controller - Left";
                    yield return "Oculus Touch Controller - Right";
                }
            }
        }

        #endregion
    }
}