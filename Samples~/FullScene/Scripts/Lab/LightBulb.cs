// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LightBulb.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Allows to model light bulbs that will affect the light attached to the <see cref="Lamp" /> they are placed on.
    /// </summary>
    public class LightBulb : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrGrabbableObject _grabbableObject;
        [SerializeField] private float              _lightIntensity;
        [SerializeField] private bool               _isFaulty;
        [SerializeField] private Color              _emissiveDisabled = Color.black;
        [SerializeField] private Color              _emissiveEnabled  = Color.white;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the light intensity contributed by the light bulb, which may flicker if it's faulty or be zero if it's not
        ///     connected to the lamp.
        /// </summary>
        public float Intensity
        {
            get
            {
                if (_grabbableObject.CurrentAnchor == null)
                {
                    // Not attached to anything
                    return 0.0f;
                }
                if (_grabbableObject.CurrentAnchor.GetComponentInParent<Lamp>())
                {
                    // Attached to a lamp. See if it is faulty or works correctly.

                    if (_isFaulty)
                    {
                        float noise = Mathf.PerlinNoise(_randX + Time.time * 20.0f, _randY * 10.0f);

                        if (noise > 0.66f)
                        {
                            return _lightIntensity;
                        }
                        if (noise > 0.16f)
                        {
                            return 0.0f;
                        }
                        return _lightIntensity * 0.5f;
                    }
                    return _lightIntensity;
                }
                // Not attached to a lamp
                return 0.0f;
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

            Renderer renderer = GetComponent<Renderer>();

            if (renderer)
            {
                _material = renderer.material;
            }

            _randX = Random.value;
            _randY = Random.value;
        }

        /// <summary>
        ///     Updates the emissive based on the light intensity.
        /// </summary>
        private void Update()
        {
            if (_material && _grabbableObject && _lightIntensity > 0.0f)
            {
                Color emissiveColor = Color.Lerp(_emissiveDisabled, _emissiveEnabled, Intensity / _lightIntensity);
                _material.SetColor(UxrConstants.Shaders.EmissionColorVarName, emissiveColor);
            }
        }

        #endregion

        #region Private Types & Data

        private Material _material;
        private float    _randX;
        private float    _randY;

        #endregion
    }
}