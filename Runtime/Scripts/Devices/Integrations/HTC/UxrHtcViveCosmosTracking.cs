// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveCosmosTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     Tracking for HTC Vive Cosmos controllers using SteamVR SDK.
    /// </summary>
    public class UxrHtcViveCosmosTracking : UxrSteamVRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrHtcViveCosmosInput);

        #endregion
    }
}