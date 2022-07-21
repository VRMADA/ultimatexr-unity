// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPicoNeo3Tracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.Pico
{
    /// <summary>
    ///     Tracking for Pico Neo 3 devices using the PicoXR SDK.
    /// </summary>
    public class UxrPicoNeo3Tracking : UxrUnityXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrPicoNeo3Input);

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkPicoXR + "";

        #endregion
    }
}