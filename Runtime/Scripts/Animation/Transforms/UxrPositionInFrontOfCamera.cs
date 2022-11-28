// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPositionInFrontOfCamera.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     Positions an object in front of the VR camera with a height offset compared to it
    /// </summary>
    public sealed class UxrPositionInFrontOfCamera : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float   _distance     = 0.2f;
        [SerializeField] private float   _heightOffset = -0.2f;
        [SerializeField] private Vector3 _eulerAngles  = Vector3.zero;

        #endregion

        #region Unity

        /// <summary>
        ///     Performs transform
        /// </summary>
        private void LateUpdate()
        {
            if (UxrAvatar.LocalAvatarCamera)
            {
                Transform cameraTransform = UxrAvatar.LocalAvatar.CameraTransform;
                Vector3   forward         = UxrAvatar.LocalAvatar.ProjectedCameraForward;

                if (forward != Vector3.zero)
                {
                    forward.Normalize();

                    if ((cameraTransform.forward.y < 0.0f && Vector3.Dot(cameraTransform.forward, -Vector3.up) < 0.9f) || (cameraTransform.forward.y >= 0.0f && Vector3.Dot(cameraTransform.forward, Vector3.up) < 0.9f))
                    {
                        _forward = forward;
                    }
                }

                transform.SetPositionAndRotation(UxrAvatar.LocalAvatar.CameraPosition + _forward * _distance + Vector3.up * _heightOffset, Quaternion.LookRotation(_forward, Vector3.up) * Quaternion.Euler(_eulerAngles));
            }
        }

        #endregion

        #region Private Types & Data

        private Vector3 _forward;

        #endregion
    }
}