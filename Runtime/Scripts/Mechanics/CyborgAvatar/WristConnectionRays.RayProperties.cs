// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WristConnectionRays.RayProperties.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Mechanics.CyborgAvatar
{
    public partial class WristConnectionRays
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores the properties of a connection ray.
        /// </summary>
        [Serializable]
        private class RayProperties
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] [ColorUsage(true, true)] private Color _color;
            [SerializeField]                          private float _thickness;
            [SerializeField]                          private float _offset;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the ray thickness.
            /// </summary>
            public float Thickness => _thickness;

            /// <summary>
            ///     Gets the ray offset.
            /// </summary>
            public float Offset => _offset;

            /// <summary>
            ///     Gets the ray color.
            /// </summary>
            public Color Color
            {
                get => _color;
                set => _color = value;
            }

            /// <summary>
            ///     Gets or sets the GameObject created at runtime for the ray.
            /// </summary>
            public GameObject GameObject { get; set; }

            /// <summary>
            ///     Gets or sets the line renderer component created at runtime for the ray.
            /// </summary>
            public LineRenderer LineRenderer { get; set; }

            /// <summary>
            ///     Gets or sets the offset in the 2d section of the ray direction.
            /// </summary>
            public Vector2 OffsetXY { get; set; }

            /// <summary>
            ///     Gets or sets the start color.
            /// </summary>
            public Color StartColor { get; set; }

            #endregion
        }

        #endregion
    }
}