// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMuzzleFlash.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Muzzle flash component for weapons that are firing shots.
    /// </summary>
    public class UxrMuzzleFlash : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Material _material;
        [SerializeField] private int      _textureColumns     = 1;
        [SerializeField] private int      _textureRows        = 1;
        [SerializeField] private bool     _randomizeAngle     = true;
        [SerializeField] private float    _minRandomizeScale  = 1.0f;
        [SerializeField] private float    _maxRandomizeScale  = 1.0f;
        [SerializeField] private string   _scaleOffsetVarName = UxrConstants.Shaders.StandardMainTextureScaleOffsetVarName;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _meshRenderers = GetComponentsInChildren<MeshRenderer>();

            if (_randomizeAngle)
            {
                transform.Rotate(Vector3.forward, Random.value * 360.0f, Space.Self);
            }

            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                int randomColumn = Random.Range(0, _textureColumns);
                int randomRow    = Random.Range(0, _textureRows);

                if (meshRenderer.sharedMaterial == _material)
                {
                    Vector4 vecScaleOffset = meshRenderer.material.GetVector(_scaleOffsetVarName);

                    if (_textureColumns > 0)
                    {
                        vecScaleOffset.x = 1.0f / _textureColumns;
                        vecScaleOffset.z = randomColumn * vecScaleOffset.x;
                    }

                    if (_textureRows > 0)
                    {
                        vecScaleOffset.y = 1.0f / _textureRows;
                        vecScaleOffset.w = randomRow * vecScaleOffset.y;
                    }

                    meshRenderer.material.SetVector(_scaleOffsetVarName, vecScaleOffset);
                }
            }

            float randomScale = Random.Range(_minRandomizeScale, _maxRandomizeScale);
            transform.localScale *= randomScale;
        }

        #endregion

        #region Private Types & Data

        private MeshRenderer[] _meshRenderers;

        #endregion
    }
}