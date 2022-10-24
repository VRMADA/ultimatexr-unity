// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Lamp.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Binds the light intensity to the state of the currently placed light bulbs.
    /// </summary>
    public class Lamp : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrGrabbableObjectAnchor[] _sockets;
        [SerializeField] private Light                      _light;

        #endregion

        #region Unity

        /// <summary>
        ///     Updates the light intensity based on the currently placed light bulbs.
        /// </summary>
        private void Update()
        {
            float lightBulbIntensity = 0.0f;

            foreach (UxrGrabbableObjectAnchor socket in _sockets)
            {
                if (socket.CurrentPlacedObject != null)
                {
                    LightBulb lightBulb = socket.CurrentPlacedObject.GetComponentInChildren<LightBulb>();

                    if (lightBulb != null)
                    {
                        lightBulbIntensity += lightBulb.Intensity;
                    }
                }
            }

            _light.intensity = lightBulbIntensity;
        }

        #endregion
    }
}