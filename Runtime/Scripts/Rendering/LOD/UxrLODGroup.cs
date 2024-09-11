// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLODGroup.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Locomotion;
using UnityEngine;

namespace UltimateXR.Rendering.LOD
{
    /// <summary>
    ///     Fixes LOD levels in VR, which by default do not work correctly. See
    ///     https://forum.unity.com/threads/lodgroup-in-vr.455394/.<br />
    ///     In addition, when using non-smooth locomotion, it can switch LOD levels only when the avatar moved.
    ///     This avoids LOD switching caused by head movement, where the camera is. When using a smooth locomotion
    ///     system it will use regular LOD switching.<br />
    ///     Using LOD switching on teleports only can be disabled using the "Only Fix LOD Bias" parameter.
    /// </summary>
    [RequireComponent(typeof(LODGroup))]
    public class UxrLODGroup : UxrComponent<UxrLODGroup>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _onlyFixLodBias;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the Unity LODGroup component.
        /// </summary>
        public LODGroup UnityLODGroup => GetCachedComponent<LODGroup>();

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to the global event called whenever any <see cref="UxrLocomotion" /> component is enabled.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (!_onlyFixLodBias)
            {
                UxrAvatar.LocalAvatarChanged += UxrAvatar_LocalAvatarChanged;
            }
        }

        /// <summary>
        ///     Unsubscribes from the global event called whenever any <see cref="UxrLocomotion" /> component is enabled.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_onlyFixLodBias)
            {
                UxrAvatar.LocalAvatarChanged -= UxrAvatar_LocalAvatarChanged;
            }
        }

        /// <summary>
        ///     Subscribes to the event called whenever an avatar was moved.
        ///     Also starts the LOD Bias fix coroutine and sets up the correct LOD level if there is a local avatar.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (!_onlyFixLodBias)
            {
                UxrAvatar.GlobalAvatarMoved += UxrAvatar_GlobalAvatarMoved;
                UpdateMode(UxrAvatar.LocalAvatar);
            }

            StartCoroutine(FixLodBiasCoroutine());
        }

        /// <summary>
        ///     Unsubscribes from the event called whenever an avatar was moved.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (!_onlyFixLodBias)
            {
                UxrAvatar.GlobalAvatarMoved -= UxrAvatar_GlobalAvatarMoved;

                if (UnityLODGroup.enabled && UnityLODGroup.gameObject.activeInHierarchy)
                {
                    UnityLODGroup.ForceLOD(-1);
                }
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Fixes the LOD bias so that the LOD switching in VR behaves like in the editor.
        ///     From: https://forum.unity.com/threads/lodgroup-in-vr.455394/
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator FixLodBiasCoroutine()
        {
            // Wait until the camera gets the correct VR FOV

            yield return null;
            yield return null;
            yield return null;

            while (UxrAvatar.LocalAvatarCamera == null)
            {
                if (UxrAvatar.LocalAvatar != null && UxrAvatar.LocalStandardAvatarController == null)
                {
                    // Replay or other.
                    yield break;
                }

                yield return null;
            }

            // Fix?

            if (!s_lodGroupFixed)
            {
                float oldLodBias          = QualitySettings.lodBias; 
                float editorCameraRadians = Mathf.PI / 3.0f;
                QualitySettings.lodBias  *= Mathf.Tan(UxrAvatar.LocalAvatarCamera.fieldOfView * Mathf.Deg2Rad / 2) / Mathf.Tan(editorCameraRadians / 2);
                s_lodGroupFixed           = true;

                if (UxrGlobalSettings.Instance.LogLevelRendering >= UxrLogLevel.Relevant)
                {
                    Debug.Log($"{UxrConstants.RenderingModule}: Fixing LOD Bias for VR cameras. Old value = {oldLodBias}, new value = {QualitySettings.lodBias}.");
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever the local avatar changed. We use it to store whether the current local avatar
        ///     uses smooth locomotion.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_LocalAvatarChanged(object sender, UxrAvatarEventArgs e)
        {
            UpdateMode(e.Avatar);
        }

        /// <summary>
        ///     Called whenever an avatar moved. In non-smooth locomotion mode it will be used to switch LOD levels manually.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_GlobalAvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            UxrAvatar avatar = sender as UxrAvatar;

            if (avatar == UxrAvatar.LocalAvatar && !_isSmoothLocomotionEnabled)
            {
                EnableLevelRenderers(avatar.CameraComponent);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Manually enables the LOD levels for a given camera.
        /// </summary>
        /// <param name="cam"></param>
        private void EnableLevelRenderers(Camera cam)
        {
            if (UnityLODGroup.enabled && UnityLODGroup.gameObject.activeInHierarchy)
            {
                UnityLODGroup.ForceLOD(UnityLODGroup.GetVisibleLevel(cam));
            }
        }

        /// <summary>
        ///     Updates the continuous/discrete operation mode of the component.
        /// </summary>
        /// <param name="avatar">Avatar to update the current mode for</param>
        private void UpdateMode(UxrAvatar avatar)
        {
            _isSmoothLocomotionEnabled = avatar != null && avatar.AvatarController != null && avatar.AvatarController.UsesSmoothLocomotion;

            bool autoLod = _isSmoothLocomotionEnabled || avatar == null || avatar.CameraComponent == null;

            if (!autoLod)
            {
                if (avatar != null && avatar.CameraComponent != null)
                {
                    EnableLevelRenderers(avatar.CameraComponent);
                }
            }
            else
            {
                if (UnityLODGroup.enabled && UnityLODGroup.gameObject.activeInHierarchy)
                {
                    UnityLODGroup.EnableAllLevelRenderers();
                    UnityLODGroup.ForceLOD(-1);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private static bool s_lodGroupFixed;
        private        bool _isSmoothLocomotionEnabled = true;

        #endregion
    }
}