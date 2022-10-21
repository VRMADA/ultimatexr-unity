// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraPointerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.UI
{
    /// <summary>
    ///     Serialized property for <see cref="UxrCameraPointer" />.
    /// </summary>
    [CustomEditor(typeof(UxrCameraPointer))]
    public class UxrCameraPointerEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyRayLength = serializedObject.FindProperty("_rayLength");
            _propertyCrosshair = serializedObject.FindProperty("_crosshair");
        }

        /// <summary>
        ///     Draws the custom inspector and handles user input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propertyRayLength, ContentRayLength);
            EditorGUILayout.PropertyField(_propertyCrosshair, ContentCrosshair);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentRayLength { get; } = new GUIContent("Ray Length", "Length of the raycast in units");
        private GUIContent ContentCrosshair { get; } = new GUIContent("Crosshair",  "Optional crosshair object. This will allow the component to disable its colliders if there are any.");

        private SerializedProperty _propertyRayLength;
        private SerializedProperty _propertyCrosshair;

        #endregion
    }
}