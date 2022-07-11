// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphicExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Extensions.Unity.UI
{
    /// <summary>
    ///     <see cref="Graphic" /> extensions.
    /// </summary>
    public static class GraphicExt
    {
        #region Public Methods

        /// <summary>
        ///     Sets the alpha value of a <see cref="Graphic" /> component.
        /// </summary>
        /// <param name="graphic">Target <see cref="Graphic" /> component to set the alpha value of</param>
        /// <param name="alpha">New alpha value</param>
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a       = alpha;
            graphic.color = color;
        }

        #endregion
    }
}