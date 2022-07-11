// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseAssetEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Manipulation.HandPoses.Editor
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrHandPoseAsset" />.
    /// </summary>
    [CustomEditor(typeof(UxrHandPoseAsset))]
    public class UxrHandPoseAssetEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Draw the inspector but editing will be disabled because data should be only modified by the hand pose editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            DrawPropertiesExcluding(serializedObject, "m_Script");
            GUI.enabled = true;
        }

        #endregion
    }
}