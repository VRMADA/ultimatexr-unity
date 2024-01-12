// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentProcessorWindow.TargetObjects.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor.Utilities
{
    public abstract partial class ComponentProcessorWindow<T>
    {
        #region Private Types & Data

        /// <summary>
        ///     Enumerates the different potential target object(s) for the component processor.
        /// </summary>
        private enum TargetObjects
        {
            /// <summary>
            ///     Processes a single component.
            /// </summary>
            SingleComponent,

            /// <summary>
            ///     Processes the current selection, and optionally the hierarchy below or inside the prefab.
            /// </summary>
            CurrentSelection,

            /// <summary>
            ///     Processes the current scene.
            /// </summary>
            CurrentScene,

            /// <summary>
            ///     Processes a whole folder and all sub-folders recursively.
            /// </summary>
            ProjectFolder,
        }

        #endregion
    }
}