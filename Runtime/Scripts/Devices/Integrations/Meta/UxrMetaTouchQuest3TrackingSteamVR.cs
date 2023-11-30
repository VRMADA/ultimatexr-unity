// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMetaTouchQuest3TrackingSteamVR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.Meta
{
    /// <summary>
    ///     Tracking for Oculus Touch controllers using SteamVR SDK.
    /// </summary>
    public class UxrMetaTouchQuest3TrackingSteamVR : UxrSteamVRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrMetaTouchQuest3InputSteamVR);

        #endregion
    }
}