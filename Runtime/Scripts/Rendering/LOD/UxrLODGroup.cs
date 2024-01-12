// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLODGroup.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Locomotion;
using UnityEngine;

namespace UltimateXR.Rendering.LOD
{
    /// <summary>
    ///     <para>
    ///         Component that, added to a GameObject with a Unity LODGroup component, will take over the LOD switching.
    ///     </para>
    ///     When using a locomotion based on teleportation, it will only switch LOD levels when the teleportation happens to
    ///     avoid popping due to head movement. When using a smooth locomotion system it will use regular LOD switching.
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
                UxrTeleportLocomotion.GlobalEnabled += UxrLocomotion_Enabled;
                
                UnityEngine.LOD[] lods = UnityLODGroup.GetLODs();

                foreach (UnityEngine.LOD lod in lods)
                {
                    foreach (Renderer r in lod.renderers)
                    {
                        if (r != null && !_lodRenderers.ContainsKey(r))
                        {
                            _lodRenderers.Add(r, r.enabled);
                        }
                    }
                }
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
                UxrTeleportLocomotion.GlobalEnabled -= UxrLocomotion_Enabled;
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
            }
            
            StartCoroutine(FixLodBiasCoroutine());

            if (UxrAvatar.LocalAvatar && !_onlyFixLodBias)
            {
                if (UxrAvatar.LocalAvatar.CameraComponent)
                {
                    UnityLODGroup.EnableLevelRenderers(UnityLODGroup.GetVisibleLevel(UxrAvatar.LocalAvatar.CameraComponent));
                }

                UxrLocomotion locomotion = UxrLocomotion.EnabledComponentsInLocalAvatar.FirstOrDefault();

                if (locomotion != null)
                {
                    _isSmoothLocomotionEnabled = locomotion.IsSmoothLocomotion;
                    UnityLODGroup.enabled      = locomotion.IsSmoothLocomotion;
                }
            }

            // Re-enable renderers that were initially enabled because when disabling and enabling this component again,
            // the renderer states get saved incorrectly.

            if (_reEnableRenderers && !_onlyFixLodBias)
            {
                foreach (var rendererState in _lodRenderers)
                {
                    if (rendererState.Key != null && rendererState.Value)
                    {
                        rendererState.Key.enabled = true;
                    }
                }

                _reEnableRenderers = false;
            }
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
                UnityLODGroup.enabled       =  true;
                _reEnableRenderers          =  true;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Fixes the LOD bias so that the LOD switching in VR behaves like in the editor.
        ///     From: From: https://forum.unity.com/threads/lodgroup-in-vr.455394/
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
                yield return null;
            }

            // Fix?

            if (!s_lodGroupChanged)
            {
                float editorCameraRadians = Mathf.PI / 3.0f;
                QualitySettings.lodBias *= Mathf.Tan(UxrAvatar.LocalAvatarCamera.fieldOfView * Mathf.Deg2Rad / 2) / Mathf.Tan(editorCameraRadians / 2);
                s_lodGroupChanged       =  true;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever a <see cref="UxrLocomotion" /> component was enabled. If it is from the local avatar it will
        ///     determine, depending on <see cref="UxrLocomotion.IsSmoothLocomotion" />, whether to use LOD switching each frame or
        ///     only when the avatar position changed.
        /// </summary>
        /// <param name="locomotion">Locomotion component that was enabled</param>
        private void UxrLocomotion_Enabled(UxrLocomotion locomotion)
        {
            if (locomotion.Avatar.AvatarMode == UxrAvatarMode.Local && !_onlyFixLodBias)
            {
                _isSmoothLocomotionEnabled = locomotion.IsSmoothLocomotion;
                UnityLODGroup.enabled      = locomotion.IsSmoothLocomotion;

                if (!locomotion.IsSmoothLocomotion)
                {
                    UnityLODGroup.EnableLevelRenderers(UnityLODGroup.GetVisibleLevel(locomotion.Avatar.CameraComponent));
                }
            }
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
                UnityLODGroup.EnableLevelRenderers(UnityLODGroup.GetVisibleLevel(avatar.CameraComponent));
            }
        }

        #endregion

        #region Private Types & Data

        private static   bool                       s_lodGroupChanged;
        private readonly Dictionary<Renderer, bool> _lodRenderers              = new Dictionary<Renderer, bool>();
        private          bool                       _isSmoothLocomotionEnabled = true;
        private          bool                       _reEnableRenderers;

        #endregion
    }
}