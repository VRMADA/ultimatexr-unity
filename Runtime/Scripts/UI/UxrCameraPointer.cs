// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraPointer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;

namespace UltimateXR.UI
{
    /// <summary>
    ///     Gaze pointer that can be added to a camera to enable gaze interaction with user interfaces.
    /// </summary>
    public class UxrCameraPointer : UxrLaserPointer
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private GameObject _crosshair;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            if (_crosshair != null)
            {
                // Disable crosshair colliders to avoid ray-casting

                _crosshair.GetComponentsInChildren<Collider>().ForEach(c => c.enabled = false);
            }

            ClickInput     = UxrInputButtons.None;
            ShowLaserInput = UxrInputButtons.None;
            IsInvisible    = true;

            // At the end so that the overriden parameters initialize the UxrLaserPointer component correctly.
            base.Awake();
        }

        #endregion
    }
}