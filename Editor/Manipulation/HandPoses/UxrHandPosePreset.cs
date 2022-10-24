// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPosePreset.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    /// <summary>
    ///     Stores a <see cref="UxrHandPoseAsset" /> that can be used as a preset to modify other hands.
    ///     It also allows to show a thumbnail in the editor by looking for an image with the same pose name in the folder.
    /// </summary>
    public class UxrHandPosePreset
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the hand pose asset.
        /// </summary>
        public UxrHandPoseAsset Pose { get; }

        /// <summary>
        ///     Gets the thumbnail, or null if it wasn't found.
        /// </summary>
        public Texture2D Thumbnail { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor that initializes the preset.
        /// </summary>
        /// <param name="file">File that contains a <see cref="UxrHandPoseAsset" /> scriptable object</param>
        /// <param name="sameFolderFiles">
        ///     List of other files in the folder which will be used to look for a thumbnail with the same name as the pose
        /// </param>
        public UxrHandPosePreset(string file, string[] sameFolderFiles)
        {
            Pose = AssetDatabase.LoadAssetAtPath<UxrHandPoseAsset>(file);

            foreach (string otherFile in sameFolderFiles)
            {
                if (otherFile != file && Path.GetFileNameWithoutExtension(otherFile) == Path.GetFileNameWithoutExtension(file) && AssetDatabase.GetMainAssetTypeAtPath(otherFile) == typeof(Texture2D))
                {
                    Thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(otherFile);
                }
            }
        }

        #endregion
    }
}