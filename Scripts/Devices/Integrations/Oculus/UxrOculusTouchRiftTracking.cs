// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrOculusTouchRiftTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.Oculus
{
    /// <summary>
    ///     Tracking for Oculus Touch devices using the Oculus SDK.
    /// </summary>
    public class UxrOculusTouchRiftTracking : UxrUnityXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrOculusTouchRiftInput);

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkOculus;

        #endregion
    }
}