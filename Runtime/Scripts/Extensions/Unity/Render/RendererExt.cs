// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RendererExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Renderer" /> extensions.
    /// </summary>
    public static class RendererExt
    {
        #region Public Methods
        
        /// <summary>
        ///     Calculates the <see cref="Bounds" /> encapsulating a set of renderers.
        /// </summary>
        /// <param name="renderers">Renderers to compute the bounds for</param>
        /// <returns><see cref="Bounds" /> encapsulating all renderers</returns>
        public static Bounds CalculateBounds(this IEnumerable<Renderer> renderers)
        {
            renderers.ThrowIfNull(nameof(renderers));

            Bounds bounds  = default;
            bool   isFirst = true;

            foreach (Renderer r in renderers)
            {
                Bounds b = r.bounds;
                if (isFirst)
                {
                    bounds  = r.bounds;
                    isFirst = false;
                }
                else
                {
                    bounds.Encapsulate(b);
                }
            }

            return bounds;
        }

        #endregion
    }
}