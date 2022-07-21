// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMaterialRenderQueue.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     Component that changes the RenderQueue of a material. Changes will be applied at runtime.
    /// </summary>
    public class UxrMaterialRenderQueue : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _instanceOnly;
        [SerializeField] private bool _everyFrame = true;
        [SerializeField] private int  _slot;
        [SerializeField] private int  _value;

        #endregion

        #region Unity

        /// <summary>
        ///     Gets the component and applies the RenderQueue value.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _renderer = GetComponent<Renderer>();
            Apply();
        }

        /// <summary>
        ///     Applies the RenderQueue each frame if required.
        /// </summary>
        private void LateUpdate()
        {
            if (_everyFrame)
            {
                Apply();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Internal method that applies the RenderQueue value.
        /// </summary>
        private void Apply()
        {
            if (_renderer != null)
            {
                if (_instanceOnly)
                {
                    Material[] materials = _renderer.materials;
                    if (_slot >= 0 && _slot < materials.Length)
                    {
                        materials[_slot].renderQueue = _value;
                    }
                    _renderer.materials = materials;
                }
                else
                {
                    Material[] materials = _renderer.sharedMaterials;
                    if (_slot >= 0 && _slot < materials.Length)
                    {
                        materials[_slot].renderQueue = _value;
                    }
                    _renderer.sharedMaterials = materials;
                }
            }
        }

        #endregion

        #region Private Types & Data

        private Renderer _renderer;

        #endregion
    }
}