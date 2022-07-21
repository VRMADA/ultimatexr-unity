// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WristConnectionRays.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Mechanics.CyborgAvatar
{
    /// <summary>
    ///     Component that drives the two devices that connect the Cyborg wrist to the arm.
    /// </summary>
    public partial class WristConnectionRays : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float               _gradientPosStart1 = 0.15f;
        [SerializeField] private float               _gradientPosStart2 = 0.2f;
        [SerializeField] private float               _gradientPosEnd1   = 0.8f;
        [SerializeField] private float               _gradientPosEnd2   = 0.85f;
        [SerializeField] private Material            _rayMaterial;
        [SerializeField] private bool                _useMaterialNoiseParameters;
        [SerializeField] private Transform           _src;
        [SerializeField] private Transform           _dst;
        [SerializeField] private List<RayProperties> _rays;

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to avatar update event.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from avatar update events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            Create(_src.position, _dst.position);
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Updates the component.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (_src != null && _dst != null)
            {
                UpdateRays(_src.position, _dst.position);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates the connections.
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dst">Destination position</param>
        private void Create(Vector3 src, Vector3 dst)
        {
            foreach (RayProperties ray in _rays)
            {
                ray.GameObject = new GameObject("Ray");
                ray.GameObject.transform.SetParent(transform, true);
                ray.GameObject.transform.localPosition = Vector3.zero;
                ray.GameObject.transform.localRotation = Quaternion.identity;

                ray.LineRenderer          = ray.GameObject.AddComponent<LineRenderer>();
                ray.LineRenderer.material = _rayMaterial;

                if (_useMaterialNoiseParameters)
                {
                    ray.LineRenderer.material.SetFloat(DistortTimeStartVarName, Random.value * 10000.0f);
                }

                ray.LineRenderer.textureMode = LineTextureMode.Stretch;
                ray.OffsetXY                 = Random.insideUnitCircle;
            }

            UpdateRays(src, dst);
        }

        /// <summary>
        ///     Updates the connection rays.
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dst">End position</param>
        private void UpdateRays(Vector3 src, Vector3 dst)
        {
            foreach (RayProperties ray in _rays)
            {
                if (ray.GameObject == null)
                {
                    continue;
                }

                ray.GameObject.transform.position = src;
                ray.GameObject.transform.LookAt(dst);

                float rayLength = Vector3.Distance(src, dst) / ray.LineRenderer.transform.lossyScale.z;

                Vector3[] positions =
                {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.0f, 0.0f, rayLength * _gradientPosStart1),
                            new Vector3(0.0f, 0.0f, rayLength * _gradientPosStart2),
                            new Vector3(0.0f, 0.0f, rayLength * _gradientPosEnd1),
                            new Vector3(0.0f, 0.0f, rayLength * _gradientPosEnd2),
                            new Vector3(0.0f, 0.0f, rayLength)
                };

                Vector3 offset = (ray.GameObject.transform.right * ray.OffsetXY.x + ray.GameObject.transform.up * ray.OffsetXY.y).normalized * ray.Offset;

                for (int pos = 0; pos < positions.Length; ++pos)
                {
                    positions[pos] = ray.LineRenderer.transform.InverseTransformPoint(ray.GameObject.transform.TransformPoint(positions[pos]) + offset);
                }

                ray.LineRenderer.useWorldSpace = false;
                ray.LineRenderer.positionCount = 6;
                ray.LineRenderer.SetPositions(positions);
                ray.LineRenderer.startWidth     = ray.Thickness;
                ray.LineRenderer.endWidth       = ray.Thickness;
                ray.LineRenderer.material.color = ray.Color;

                if (ray.LineRenderer.material.mainTexture != null)
                {
                    ray.LineRenderer.material.mainTextureScale = new Vector2(rayLength / ray.Thickness / (ray.LineRenderer.material.mainTexture.width / (float)ray.LineRenderer.material.mainTexture.height), 1.0f);
                }

                Gradient colorGradient = new Gradient();

                colorGradient.colorKeys = new[]
                                          {
                                                      new GradientColorKey(Color.white, 0.0f),
                                                      new GradientColorKey(Color.white, _gradientPosStart1),
                                                      new GradientColorKey(Color.white, _gradientPosStart2),
                                                      new GradientColorKey(Color.white, _gradientPosEnd1),
                                                      new GradientColorKey(Color.white, _gradientPosEnd2),
                                                      new GradientColorKey(Color.white, 1.0f)
                                          };

                colorGradient.alphaKeys = new[]
                                          {
                                                      new GradientAlphaKey(0.0f, 0.0f),
                                                      new GradientAlphaKey(0.0f, _gradientPosStart1),
                                                      new GradientAlphaKey(1.0f, _gradientPosStart2),
                                                      new GradientAlphaKey(1.0f, _gradientPosEnd1),
                                                      new GradientAlphaKey(0.0f, _gradientPosEnd2),
                                                      new GradientAlphaKey(0.0f, 1.0f)
                                          };

                ray.LineRenderer.colorGradient = colorGradient;
            }
        }

        #endregion

        #region Private Types & Data

        private readonly string DistortTimeStartVarName = "_DistortTimeStart";

        #endregion
    }
}