// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraDepthEnable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.CameraUtils
{
    /// <summary>
    ///     Component added to a camera, enabling camera depth texture mode <see cref="Camera" />. Depth texture mode is
    ///     required for soft particles.
    /// </summary>
    public class UxrCameraDepthEnable : UxrAvatarComponent<UxrCameraDepthEnable>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private DepthTextureMode _depthTextureMode = DepthTextureMode.Depth;

        #endregion

        #region Unity

        /// <summary>
        ///     Called at startup. Sets up the camera with the given parameter.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Avatar.CameraComponent)
            {
                Avatar.CameraComponent.depthTextureMode = _depthTextureMode;
            }
        }

        #endregion
    }
}