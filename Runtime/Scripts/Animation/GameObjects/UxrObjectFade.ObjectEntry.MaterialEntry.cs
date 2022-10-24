// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrObjectFade.ObjectEntry.MaterialEntry.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.GameObjects
{
    public partial class UxrObjectFade
    {
        #region Private Types & Data

        private partial class ObjectEntry
        {
            #region Private Types & Data

            /// <summary>
            ///     Stores information of a material in a fade animation.
            /// </summary>
            private struct MaterialEntry
            {
                #region Public Types & Data

                /// <summary>
                ///     Gets or sets the initial material color.
                /// </summary>
                public Color StartColor { get; set; }

                /// <summary>
                ///     Gets or sets whether the shader transparency was enabled.
                /// </summary>
                public bool ShaderChanged { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}