// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     Tracking for HTC Vive controllers using SteamVR SDK.
    /// </summary>
    public class UxrHtcViveTracking : UxrSteamVRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrHtcViveInput);

        #endregion
    }
}