// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMagicLeap2Input.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.MagicLeap
{
    /// <summary>
    ///     Magic Leap 2 controller input using Magic Leap SDK.
    /// </summary>
    public class UxrMagicLeap2Input : UxrUnityXRControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <summary>
        ///     Gets the SDK dependency: Magic Leap 2.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkMagicLeap;

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <inheritdoc />
        public override bool MainJoystickIsTouchpad => true;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick |
                                        UxrControllerElements.Trigger |
                                        UxrControllerElements.Grip |
                                        UxrControllerElements.Bumper |
                                        UxrControllerElements.Menu |
                                        UxrControllerElements.DPad);

            return (validElements & (uint)controllerElements) == (uint)controllerElements;
        }

        #endregion

        #region Public Overrides UxrUnityXRControllerInput

        /// <inheritdoc />
        public override IEnumerable<string> ControllerNames
        {
            get { yield return "MagicLeap Controller"; }
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        protected override void OnDeviceConnected(UxrDeviceConnectEventArgs e)
        {
            base.OnDeviceConnected(e);

#if ULTIMATEXR_USE_MAGICLEAP_SDK            
            if (e.IsConnected)
            {
                _mlInputs = new MagicLeapInputs();
                _mlInputs.Enable();
            }
            else
            {
                if (_mlInputs != null)
                {
                    _mlInputs.Dispose();
                    _mlInputs = null;
                }
            }
#endif
        }

        #endregion

        #region Protected Overrides UxrUnityXRControllerInput

        /// <inheritdoc />
        protected override void UpdateInput()
        {
            base.UpdateInput();

            // Propagate touchpad touch to press, since only touch is signaled by the API
            SetButtonFlags(ButtonFlags.PressFlagsLeft,  UxrInputButtons.Joystick, GetButtonsTouch(UxrHandSide.Left,  UxrInputButtons.Joystick));
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.Joystick, GetButtonsTouch(UxrHandSide.Right, UxrInputButtons.Joystick));
        }

        /// <inheritdoc />
        protected override bool HasButtonContactOther(UxrHandSide handSide, UxrInputButtons button, ButtonContact buttonContact)
        {
#if ULTIMATEXR_USE_MAGICLEAP_SDK
            if (button == UxrInputButtons.Bumper)
            {
                return _mlInputs.Controller.Bumper.IsPressed();   
            }
            
            // To allow quick integration with UltimateXR manipulation, and since the ML2 doesn't have a grip button, we map
            // the bumper to the grip too.

            if (button == UxrInputButtons.Grip)
            {
                return _mlInputs.Controller.Bumper.IsPressed();
            }
#endif
            
            return false;
        }

        #endregion

        #region Private Types & Data

#if ULTIMATEXR_USE_MAGICLEAP_SDK
        private MagicLeapInputs _mlInputs;
#endif

        #endregion
    }
}