// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWindowsMixedRealityInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Devices.Integrations.Microsoft
{
    /// <summary>
    ///     Windows Mixed Reality input using Unity XR.
    /// </summary>
    public class UxrWindowsMixedRealityInput : UxrUnityXRControllerInput
    {
        #region Inspector Properties/Serialized Fields

        [Header("Windows Mixed Reality:")] [SerializeField] private float _joystickDeadZone = 0.15f;

        #endregion

        #region Public Overrides UxrControllerInput

        /// <summary>
        ///     Requires WMR SDK using OpenXR.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkWindowsMixedReality;

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <inheritdoc />
        public override float JoystickDeadZone => _joystickDeadZone;

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            uint validElements = (uint)(UxrControllerElements.Joystick |
                                        UxrControllerElements.Joystick2 |
                                        UxrControllerElements.Grip |
                                        UxrControllerElements.Trigger |
                                        UxrControllerElements.Menu |
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
                yield return "Windows MR Controller";
                yield return "Windows MR Controller OpenXR";
            }
        }

        #endregion
    }
}