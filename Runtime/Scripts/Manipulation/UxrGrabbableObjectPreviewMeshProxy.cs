// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectPreviewMeshProxy.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component used internally by the editor. They are added to keep track of grab pose preview meshes and
    ///     delete them when the preview is no longer needed.
    ///     <see cref="UxrGrabbableObjectPreviewMesh" /> components will be hidden hanging from each
    ///     <see cref="UxrGrabbableObjectSnapTransform" />. They could have the mesh themselves but when dealing with
    ///     non-uniform scaling in grabbable hierarchies, the preview mesh would be distorted when hanging directly.
    ///     Instead, each <see cref="UxrGrabbableObjectPreviewMesh" /> component will additionally point to a hidden
    ///     root GameObject with a <see cref="UxrGrabbableObjectPreviewMeshProxy" /> that will have the mesh data.
    ///     Being a root GameObject will avoid non-uniform scaling problems.
    /// </summary>
    [ExecuteInEditMode]
    public class UxrGrabbableObjectPreviewMeshProxy : UxrComponent
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the mesh filter.
        /// </summary>
        public MeshFilter MeshFilterComponent => GetCachedComponent<MeshFilter>();

        /// <summary>
        ///     Gets or sets the preview mesh component that the proxy is following.
        /// </summary>
        public UxrGrabbableObjectPreviewMesh PreviewMeshComponent { get; set; }

        /// <summary>
        ///     Gets or sets the preview mesh object used by the editor (editor type UxrPreviewHandGripMesh).
        /// </summary>
        public object PreviewMesh { get; set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Makes sure to hide the GameObject initially during play mode when working from the editor.
        /// </summary>
        protected override void Awake()
        {
            if (Application.isPlaying)
            {
                base.Awake();

                if (Application.isEditor && Application.isPlaying)
                {
                    gameObject.SetActive(false);
                }
            }
        }
        
#if UNITY_EDITOR

        /// <summary>
        ///     Follow the source object and monitors deletion.
        /// </summary>
        private void Update()
        {
            if (PreviewMeshComponent == null)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);                    
                }
            }
            else
            {
                transform.SetPositionAndRotation(PreviewMeshComponent.transform.position, PreviewMeshComponent.transform.rotation);
            }
        }

#endif

        #endregion

        #region Private Types & Data

        private MeshFilter _meshFilterComponent;

        #endregion
    }
}