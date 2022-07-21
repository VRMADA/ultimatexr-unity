// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrValveIndexTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.Valve
{
    /// <summary>
    ///     Tracking component for Valve Index controllers, also known as Knuckles, using SteamVR.
    /// </summary>
    public class UxrValveIndexTracking : UxrSteamVRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrValveIndexInput);

        #endregion
    }
}