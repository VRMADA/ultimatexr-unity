// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHpReverbG2Tracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.Microsoft;

namespace UltimateXR.Devices.Integrations.HP
{
    /// <summary>
    ///     Tracking component for HP Reverb G2.
    /// </summary>
    public class UxrHpReverbG2Tracking : UxrWindowsMixedRealityTracking
    {
        #region Public Overrides UxrWindowsMixedRealityTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrHpReverbG2Input);

        #endregion
    }
}