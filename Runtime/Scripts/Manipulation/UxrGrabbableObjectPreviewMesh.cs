// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectPreviewMesh.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component used internally by the editor. They are added to keep track of grab pose preview meshes and delete them
    ///     when the preview is no longer needed.
    ///     These work together with <see cref="UxrGrabbableObjectPreviewMeshProxy" /> to avoid non-uniform scaling problems
    ///     when previewing grab poses in UxrGrabbableObject hierarchies.
    /// </summary>
    public class UxrGrabbableObjectPreviewMesh : UxrComponent
    {
        #region Public Types & Data

        public MeshFilter MeshFilterComponent => PreviewMeshProxy.MeshFilterComponent;

        /// <summary>
        ///     Gets or sets the preview mesh object used by the editor (editor type UxrPreviewHandGripMesh).
        /// </summary>
        public object PreviewMesh
        {
            get => PreviewMeshProxy.PreviewMesh;
            set => PreviewMeshProxy.PreviewMesh = value;
        }

        public UxrGrabbableObjectPreviewMeshProxy PreviewMeshProxy { get; set; }

        #endregion
    }
}