// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNonDrawingGraphic.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Graphic component that can be used together with <see cref="UxrControlInput" /> on a UI element that has no
    ///     <see cref="Graphic" /> attached. It is useful to handle input on controls that need to graphic rendering, in order
    ///     to save performance.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UxrNonDrawingGraphic : Graphic
    {
        #region Public Overrides Graphic

        /// <inheritdoc />
        public override void SetMaterialDirty()
        {
        }

        /// <inheritdoc />
        public override void SetVerticesDirty()
        {
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        #endregion
    }
}