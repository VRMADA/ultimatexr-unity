// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHtcViveFocus3Tracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.HTC
{
    /// <summary>
    ///     Tracking for HTC Vive Focus 3 controllers using WaveXR SDK's UnityXR support.
    /// </summary>
    public class UxrHtcViveFocus3Tracking : UxrUnityXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrHtcViveFocus3Input);

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkWaveXR;

        #endregion
    }
}