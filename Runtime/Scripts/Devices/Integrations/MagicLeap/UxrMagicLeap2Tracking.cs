// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMagicLeap2Tracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices.Integrations.MagicLeap
{
    /// <summary>
    ///     Tracking for Magic Leap 2 devices using the Magic Leap SDK.
    /// </summary>
    public class UxrMagicLeap2Tracking : UxrUnityXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrMagicLeap2Input);

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override bool IsMixedRealityDevice => true;

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkMagicLeap;

        #endregion
    }
}