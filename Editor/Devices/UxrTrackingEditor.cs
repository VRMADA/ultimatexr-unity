// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTrackingEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices;
using UltimateXR.Editor.Core;
using UltimateXR.Editor.Sdks;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Devices
{
    /// <summary>
    ///     Custom Unity editor for <see cref="UxrTrackingDevice" /> components. Checks for SDK availability.
    /// </summary>
    [CustomEditor(typeof(UxrTrackingDevice), true)]
    public class UxrTrackingEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Checks if the given tracking component needs an SDK installed and available. Then draws the component itself.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrTrackingDevice tracking = serializedObject.targetObject as UxrTrackingDevice;

            if (string.IsNullOrEmpty(tracking.SDKDependency) == false)
            {
                if (UxrSdkManager.IsAvailable(tracking.SDKDependency) == false)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox($"In order to work properly this component needs the following SDK installed and active: {tracking.SDKDependency}", MessageType.Warning);

                    if (UxrEditorUtils.CenteredButton(new GUIContent("Check", "Go to the SDK Manager to check the SDK")))
                    {
                        UxrSdkManagerWindow.ShowWindow();
                    }

                    EditorGUILayout.Space();
                }
            }

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}