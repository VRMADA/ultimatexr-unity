// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveFocus3Input.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     HTC Vive Focus 3 controller input using WaveXR SDK's UnityXR support.
    /// </summary>
    public class UxrHtcViveFocus3Input : UxrUnityXRControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <summary>
        ///     Gets the SDK dependency: Wave XR.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkWaveXR;

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
                yield return "WVR_CR_Left_001";
                yield return "WVR_CR_Right_001";
            }
        }

        #endregion

        #region Protected Overrides UxrUnityXRControllerInput

        /// <inheritdoc />
        protected override void UpdateInput()
        {
            base.UpdateInput();

            // To avoid grip requiring to press the whole button, we use the analog value and a threshold

            float gripThreshold = 0.7f;

            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Grip, GetInput1D(UxrHandSide.Left,  UxrInput1D.Grip) > gripThreshold);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Grip, GetInput1D(UxrHandSide.Right, UxrInput1D.Grip) > gripThreshold);
        }

        #endregion
    }
}