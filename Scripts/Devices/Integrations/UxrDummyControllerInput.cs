// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDummyControllerInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Devices.Integrations
{
    /// <summary>
    ///     Dummy input class that is used when there is no active input component in the avatar.
    ///     It has the advantage of avoiding to check for null input component and it doesn't generate any type of input
    ///     events.
    /// </summary>
    public class UxrDummyControllerInput : UxrControllerInput
    {
        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override UxrControllerSetupType SetupType => UxrControllerSetupType.Dual;

        /// <inheritdoc />
        public override string LeftControllerName => "Dummy Left";

        /// <inheritdoc />
        public override string RightControllerName => "Dummy Right";

        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <inheritdoc />
        public override bool IsControllerEnabled(UxrHandSide handSide)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElement)
        {
            return false;
        }

        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            return 0.0f;
        }

        /// <inheritdoc />
        public override Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false)
        {
            return Vector2.zero;
        }

        /// <inheritdoc />
        public override UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide)
        {
            return 0;
        }

        #endregion
    }
}