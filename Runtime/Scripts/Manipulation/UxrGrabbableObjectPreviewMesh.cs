// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectPreviewMesh.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component used internally by the editor. They are added to keep track of grab pose preview meshes and delete them
    ///     when the preview is no longer needed.
    /// </summary>
    public class UxrGrabbableObjectPreviewMesh : UxrComponent
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the preview mesh object used by the editor (editor type UxrPreviewHandGripMesh).
        /// </summary>
        public object PreviewMesh { get; set; }

        #endregion
    }
}