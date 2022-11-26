// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrObjectFade.ObjectEntry.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateXR.Animation.GameObjects
{
    public partial class UxrObjectFade
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information about an object in a fade animation.
        /// </summary>
        private partial class ObjectEntry
        {
            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="renderer">Renderer component</param>
            public ObjectEntry(Renderer renderer)
            {
                Renderer        = renderer;
                SharedMaterials = renderer.sharedMaterials;
                Materials       = renderer.materials;
                MaterialEntries = new MaterialEntry[Materials.Length];

                for (int i = 0; i < Materials.Length; ++i)
                {
                    MaterialEntries[i].StartColor    = Materials[i].color;
                    MaterialEntries[i].ShaderChanged = false;
                }
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Changes the material transparency.
            /// </summary>
            /// <param name="startQuantity">Start alpha</param>
            /// <param name="endQuantity">End alpha</param>
            /// <param name="fadeT">Interpolation factor [0.0, 1.0]</param>
            public void Fade(float startQuantity, float endQuantity, float fadeT)
            {
                for (int i = 0; i < Materials.Length; ++i)
                {
                    if (!MaterialEntries[i].ShaderChanged)
                    {
                        ChangeStandardMaterialRenderMode(Materials[i]);
                        MaterialEntries[i].ShaderChanged = true;
                    }

                    Color color = MaterialEntries[i].StartColor;
                    color.a            *= Mathf.Lerp(startQuantity, endQuantity, fadeT);
                    Materials[i].color =  color;
                }

                Renderer.materials = Materials;
            }

            /// <summary>
            ///     Restores the original material(s).
            /// </summary>
            public void Restore()
            {
                Renderer.sharedMaterials = SharedMaterials;
                MaterialEntries.ForEach(m => m.ShaderChanged = false);
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///     Enables transparency on a material.
            /// </summary>
            /// <param name="material">Material to enable transparency on</param>
            private void ChangeStandardMaterialRenderMode(Material material)
            {
                if (material.HasProperty(UxrConstants.Shaders.SurfaceModeVarName))
                {
                    // Universal render pipeline
                    material.SetInt(UxrConstants.Shaders.SurfaceModeVarName, UxrConstants.Shaders.SurfaceModeTransparent);
                    material.SetInt(UxrConstants.Shaders.BlendModeVarName,   UxrConstants.Shaders.BlendModeAlpha);
                    material.renderQueue = (int)RenderQueue.Transparent;
                }
                else if (material.IsKeywordEnabled(UxrConstants.Shaders.AlphaBlendOnKeyword) == false)
                {
                    // Built-in render pipeline
                    material.SetInt(UxrConstants.Shaders.SrcBlendVarName, (int)BlendMode.SrcAlpha);
                    material.SetInt(UxrConstants.Shaders.DstBlendVarName, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(UxrConstants.Shaders.ZWriteVarName,   0);
                    material.DisableKeyword(UxrConstants.Shaders.AlphaTestOnKeyword);
                    material.EnableKeyword(UxrConstants.Shaders.AlphaBlendOnKeyword);
                    material.DisableKeyword(UxrConstants.Shaders.AlphaPremultiplyOnKeyword);
                    material.renderQueue = (int)RenderQueue.Transparent;
                }
            }

            #endregion

            #region Private Types & Data

            private MaterialEntry[] MaterialEntries { get; }
            private Renderer        Renderer        { get; }
            private Material[]      Materials       { get; }
            private Material[]      SharedMaterials { get; }

            #endregion
        }

        #endregion
    }
}