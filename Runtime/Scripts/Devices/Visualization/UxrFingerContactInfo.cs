// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerContactInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     Finger contact information. Stores information about where a finger touched a controller element.
    /// </summary>
    public class UxrFingerContactInfo
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the transform the finger is currently touching.
        /// </summary>
        public Transform Transform { get; set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="transform">The transform of where it made contact</param>
        public UxrFingerContactInfo(Transform transform)
        {
            Transform = transform;
        }

        #endregion
    }
}