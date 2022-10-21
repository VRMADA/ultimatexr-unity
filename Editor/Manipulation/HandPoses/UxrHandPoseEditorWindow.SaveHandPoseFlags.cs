// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseEditorWindow.SaveHandPoseFlags.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Manipulation.HandPoses;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    public partial class UxrHandPoseEditorWindow
    {
        #region Private Types & Data

        /// <summary>
        ///     Flags enumerating the different hand pose elements that can be saved.
        /// </summary>
        [Flags]
        private enum SaveHandPoseFlags
        {
            /// <summary>
            ///     <see cref="UxrHandPoseAsset" /> asset file.
            /// </summary>
            Assets = 1 << 0,

            /// <summary>
            ///     The <see cref="HandDescriptors" /> containing the finger bone transforms.
            /// </summary>
            HandDescriptors = 1 << 1,

            /// <summary>
            ///     All data and asset files.
            /// </summary>
            All = 0x7FFFFFF
        }

        #endregion
    }
}