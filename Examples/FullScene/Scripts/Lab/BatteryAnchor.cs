// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatteryAnchor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Component that helps enumerate the potential battery anchors in a scene using LabBatteryAnchor.EnabledComponents.
    ///     Battery placing has some additional special mechanics for the sliding-in/sliding-out behavior. This component
    ///     helps identifying where those places are.
    /// </summary>
    public class BatteryAnchor : UxrComponent<BatteryAnchor>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrGrabbableObjectAnchor _batteryAnchor;

        /// <summary>
        ///     Actual anchor component where a battery can be placed.
        /// </summary>
        public UxrGrabbableObjectAnchor Anchor => _batteryAnchor;

        #endregion
    }
}