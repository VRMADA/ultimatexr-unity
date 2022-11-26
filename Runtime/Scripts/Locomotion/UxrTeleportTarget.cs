// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportTarget.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Component describing the visual representation of a teleport destination.
    /// </summary>
    public class UxrTeleportTarget : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private GameObject       _reorientArrowRoot;
        [SerializeField] private List<GameObject> _validColorStateIgnoreList;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the component has a separate object that is used to point to where the avatar will be facing towards
        ///     after the teleportation.
        /// </summary>
        public bool HasReorientArrow => _reorientArrowRoot != null;

        /// <summary>
        ///     Gets the forward vector of the reorient arrow.
        /// </summary>
        public Vector3 ReorientArrowForward => _reorientArrowRoot != null ? _reorientArrowRoot.transform.forward : Vector3.zero;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Enables the reorient arrow.
        /// </summary>
        /// <param name="enable">Whether to enable it</param>
        public void EnableReorientArrow(bool enable)
        {
            if (_reorientArrowRoot != null)
            {
                _reorientArrowRoot.SetActive(enable);
            }
        }

        /// <summary>
        ///     Orients the direction arrow.
        /// </summary>
        /// <param name="rotation">New rotation</param>
        public void OrientArrow(Quaternion rotation)
        {
            if (_reorientArrowRoot != null)
            {
                _reorientArrowRoot.transform.rotation = rotation;
            }
        }

        /// <summary>
        ///     Sets the material color for objects whose color need to be changed in order to indicate a valid or invalid
        ///     teleport.
        /// </summary>
        /// <param name="color">New color</param>
        public void SetMaterialColor(Color color)
        {
            foreach (Renderer targetRenderer in _targetRenderers)
            {
                if (_object2MaterialID.TryGetValue(targetRenderer.gameObject, out UxrTeleportTargetMaterialID teleportMaterialID))
                {
                    Material[] materials = targetRenderer.materials;

                    if (teleportMaterialID.MaterialID >= 0 && teleportMaterialID.MaterialID < materials.Length)
                    {
                        materials[teleportMaterialID.MaterialID].color = color;
                        targetRenderer.materials                       = materials;
                    }
                }
                else
                {
                    targetRenderer.material.color = color;
                }
            }

            foreach (ParticleSystem pSystem in _particleSystems)
            {
                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = pSystem.colorOverLifetime;

                if (colorOverLifetime.enabled && colorOverLifetime.color.gradient != null)
                {
                    GradientColorKey[] keys = colorOverLifetime.color.gradient.colorKeys;
                    keys.ForEach(k => k.color = color);
                    colorOverLifetime.color.gradient.colorKeys = keys;
                }
            }

            foreach (ParticleSystemRenderer pSystemRenderer in _particleSystemRenderers)
            {
                if (_object2MaterialID.TryGetValue(pSystemRenderer.gameObject, out UxrTeleportTargetMaterialID teleportMaterialID))
                {
                    Material[] materials = pSystemRenderer.materials;

                    if (teleportMaterialID.MaterialID >= 0 && teleportMaterialID.MaterialID < materials.Length)
                    {
                        materials[teleportMaterialID.MaterialID].color = color;
                        materials[teleportMaterialID.MaterialID].SetColor(UxrConstants.Shaders.TintColorVarName, color);
                        pSystemRenderer.materials = materials;
                    }
                }
                else
                {
                    pSystemRenderer.material.color = color;
                    pSystemRenderer.material.SetColor(UxrConstants.Shaders.TintColorVarName, color);
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Register renderers

            _targetRenderers         = new List<Renderer>(GetComponentsInChildren<Renderer>(false));
            _particleSystems         = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>(false));
            _particleSystemRenderers = new List<ParticleSystemRenderer>(GetComponentsInChildren<ParticleSystemRenderer>(false));
            _object2MaterialID       = new Dictionary<GameObject, UxrTeleportTargetMaterialID>();

            _targetRenderers.RemoveAll(r => _validColorStateIgnoreList.Contains(r.gameObject));
            _particleSystems.RemoveAll(p => _validColorStateIgnoreList.Contains(p.gameObject));
            _particleSystemRenderers.RemoveAll(r => _validColorStateIgnoreList.Contains(r.gameObject));

            // Register objects that have a UxrTeleportTargetMaterialID component that will tell us which material's color
            // should be changed if the teleport component uses colors to differentiate between valid and invalid targets.

            foreach (Renderer targetRenderer in _targetRenderers)
            {
                if (targetRenderer.gameObject.TryGetComponent<UxrTeleportTargetMaterialID>(out var teleportMaterialID))
                {
                    _object2MaterialID.Add(teleportMaterialID.gameObject, teleportMaterialID);
                }
            }

            foreach (ParticleSystemRenderer pSystemRenderer in _particleSystemRenderers)
            {
                if (pSystemRenderer.gameObject.TryGetComponent<UxrTeleportTargetMaterialID>(out var teleportMaterialID))
                {
                    _object2MaterialID.Add(teleportMaterialID.gameObject, teleportMaterialID);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private List<Renderer>                                      _targetRenderers;
        private List<ParticleSystem>                                _particleSystems;
        private List<ParticleSystemRenderer>                        _particleSystemRenderers;
        private Dictionary<GameObject, UxrTeleportTargetMaterialID> _object2MaterialID;

        #endregion
    }
}