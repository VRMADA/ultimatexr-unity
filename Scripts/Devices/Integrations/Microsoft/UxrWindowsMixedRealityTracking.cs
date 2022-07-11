// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWindowsMixedRealityTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.Microsoft
{
    /// <summary>
    ///     Tracking component for devices based on Windows Mixed Reality.
    /// </summary>
    public class UxrWindowsMixedRealityTracking : UxrUnityXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrWindowsMixedRealityInput);

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkWindowsMixedReality;

        #endregion
    }
}