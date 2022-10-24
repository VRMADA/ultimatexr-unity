// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrImpactDecal.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Audio;
using UltimateXR.Core;
using UltimateXR.Core.Caching;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that defines a decal generated as a result of the impact of a projectile.
    /// </summary>
    public class UxrImpactDecal : UxrComponent<UxrImpactDecal>, IUxrPrecacheable
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool           _ignoreDynamicObjects;
        [SerializeField] private float          _decalOffset = 0.005f;
        [SerializeField] private Renderer[]     _decalRenderers;
        [SerializeField] private UxrAudioSample _audioImpact            = new UxrAudioSample();
        [SerializeField] private bool           _audioOnDynamicObjects  = true;
        [SerializeField] private string         _decalRendererColorName = UxrConstants.Shaders.StandardColorVarName;

        #endregion

        #region Implicit IUxrPrecacheable

        /// <inheritdoc />
        public IEnumerable<GameObject> PrecachedInstances
        {
            get { yield return gameObject; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if a given impact should generate a decal, and creates it if necessary.
        /// </summary>
        /// <param name="raycastHit">Impact raycast hit</param>
        /// <param name="checkLayerMask">Layer mask that should generate a decal</param>
        /// <param name="prefabDecal">The decal prefab to use when if the decal should be generated</param>
        /// <param name="lifeTime">New decal life time, after which it will fade out and be destroyed</param>
        /// <param name="fadeOutDurationSeconds">Decal fade out duration in seconds</param>
        /// <param name="createDoubleSidedDecal">Whether to also generate a secondary decal for the other side of the impact</param>
        /// <param name="doubleSidedDecalThickness">
        ///     Surface thickness to consider when generating the secondary decal for the other
        ///     side
        /// </param>
        /// <returns>New decal or null if no decal was generated</returns>
        public static UxrImpactDecal CheckCreateDecal(RaycastHit     raycastHit,
                                                      LayerMask      checkLayerMask,
                                                      UxrImpactDecal prefabDecal,
                                                      float          lifeTime,
                                                      float          fadeOutDurationSeconds,
                                                      bool           createDoubleSidedDecal    = false,
                                                      float          doubleSidedDecalThickness = 0.001f)
        {
            if (prefabDecal != null && raycastHit.collider != null && prefabDecal._ignoreDynamicObjects && raycastHit.collider.gameObject.IsDynamic())
            {
                // Dynamic objects have been set up to not generate decals. Play impact audio only.

                if (prefabDecal._audioImpact != null && prefabDecal._audioOnDynamicObjects)
                {
                    prefabDecal._audioImpact.Play(raycastHit.point);
                }

                return null;
            }

            if (prefabDecal != null && (checkLayerMask & 1 << raycastHit.collider.gameObject.layer) != 0)
            {
                UxrImpactDecal decal = Instantiate(prefabDecal, raycastHit.point + raycastHit.normal * prefabDecal._decalOffset, Quaternion.LookRotation(raycastHit.normal));
                decal.transform.parent = raycastHit.collider.transform;

                if (lifeTime > 0.0f)
                {
                    Destroy(decal.gameObject, lifeTime);
                }

                decal._fadeOutDuration = fadeOutDurationSeconds;
                decal._fadeOutTimer    = lifeTime;

                if (createDoubleSidedDecal)
                {
                    UxrImpactDecal decalDoubleSided = Instantiate(prefabDecal, raycastHit.point - raycastHit.normal * (prefabDecal._decalOffset + doubleSidedDecalThickness), Quaternion.LookRotation(-raycastHit.normal));
                    decalDoubleSided.transform.parent = raycastHit.collider.transform;

                    if (lifeTime > 0.0f)
                    {
                        Destroy(decalDoubleSided.gameObject, lifeTime);
                    }

                    decalDoubleSided._fadeOutDuration = fadeOutDurationSeconds;
                    decalDoubleSided._fadeOutTimer    = lifeTime;
                }

                if (prefabDecal._audioImpact != null)
                {
                    prefabDecal._audioImpact.Play(raycastHit.point);
                }

                return decal;
            }

            return null;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_decalRenderers != null)
            {
                foreach (Renderer decalRenderer in _decalRenderers)
                {
                    _startColors.Add(decalRenderer, decalRenderer.sharedMaterial.HasProperty(_decalRendererColorName) ? decalRenderer.sharedMaterial.GetColor(_decalRendererColorName) : Color.white);
                }
            }
        }

        /// <summary>
        ///     Updates the component.
        /// </summary>
        private void Update()
        {
            _fadeOutTimer -= Time.deltaTime;

            if (_fadeOutTimer < _fadeOutDuration)
            {
                foreach (Renderer decalRenderer in _decalRenderers)
                {
                    Material material = decalRenderer.material;
                    Color    color    = _startColors[decalRenderer];
                    color.a = _startColors[decalRenderer].a * (_fadeOutTimer / _fadeOutDuration);
                    material.SetColor(_decalRendererColorName, color);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private readonly Dictionary<Renderer, Color> _startColors = new Dictionary<Renderer, Color>();
        private          float                       _fadeOutTimer;
        private          float                       _fadeOutDuration;

        #endregion
    }
}