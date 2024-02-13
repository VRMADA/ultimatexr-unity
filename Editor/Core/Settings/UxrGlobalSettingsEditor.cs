// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGlobalSettingsInspector.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core.Settings
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrGlobalSettings" />.
    /// </summary>
    [CustomEditor(typeof(UxrGlobalSettings))]
    public class UxrGlobalSettingsEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propLogLevelAnimation    = serializedObject.FindProperty(PropertyLogLevelAnimation);
            _propLogLevelAvatar       = serializedObject.FindProperty(PropertyLogLevelAvatar);
            _propLogLevelCore         = serializedObject.FindProperty(PropertyLogLevelCore);
            _propLogLevelDevices      = serializedObject.FindProperty(PropertyLogLevelDevices);
            _propLogLevelLocomotion   = serializedObject.FindProperty(PropertyLogLevelLocomotion);
            _propLogLevelManipulation = serializedObject.FindProperty(PropertyLogLevelManipulation);
            _propLogLevelNetworking   = serializedObject.FindProperty(PropertyLogLevelNetworking);
            _propLogLevelUI           = serializedObject.FindProperty(PropertyLogLevelUI);
            _propLogLevelWeapons      = serializedObject.FindProperty(PropertyLogLevelWeapons);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.TextArea($"{UxrConstants.UltimateXR} version: {UxrConstants.Version}");

            // Log level configuration

            _showLogLevels = UxrEditorUtils.FoldoutStylish("Log Levels", _showLogLevels);

            if (_showLogLevels)
            {
                EditorGUILayout.PropertyField(_propLogLevelAnimation,    ContentLogLevelAnimation);
                EditorGUILayout.PropertyField(_propLogLevelAvatar,       ContentLogLevelAvatar);
                EditorGUILayout.PropertyField(_propLogLevelCore,         ContentLogLevelCore);
                EditorGUILayout.PropertyField(_propLogLevelDevices,      ContentLogLevelDevices);
                EditorGUILayout.PropertyField(_propLogLevelLocomotion,   ContentLogLevelLocomotion);
                EditorGUILayout.PropertyField(_propLogLevelManipulation, ContentLogLevelManipulation);
                EditorGUILayout.PropertyField(_propLogLevelNetworking,   ContentLogLevelNetworking);
                EditorGUILayout.PropertyField(_propLogLevelUI,           ContentLogLevelUI);
                EditorGUILayout.PropertyField(_propLogLevelWeapons,      ContentLogLevelWeapons);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentLogLevelAnimation    { get; } = new GUIContent("Animation",    "Selects the console log level for animation events");
        private GUIContent ContentLogLevelAvatar       { get; } = new GUIContent("Avatar",       "Selects the console log level for avatar events");
        private GUIContent ContentLogLevelCore         { get; } = new GUIContent("Core",         "Selects the console log level for core events");
        private GUIContent ContentLogLevelDevices      { get; } = new GUIContent("Devices",      "Selects the console log level for device events");
        private GUIContent ContentLogLevelLocomotion   { get; } = new GUIContent("Locomotion",   "Selects the console log level for locomotion events");
        private GUIContent ContentLogLevelManipulation { get; } = new GUIContent("Manipulation", "Selects the console log level for manipulation events");
        private GUIContent ContentLogLevelNetworking   { get; } = new GUIContent("Networking",   "Selects the console log level for networking events");
        private GUIContent ContentLogLevelUI           { get; } = new GUIContent("UI",           "Selects the console log level for UI events");
        private GUIContent ContentLogLevelWeapons      { get; } = new GUIContent("Weapons",      "Selects the console log level for weapon events");

        private const string PropertyLogLevelAnimation    = "_logLevelAnimation";
        private const string PropertyLogLevelAvatar       = "_logLevelAvatar";
        private const string PropertyLogLevelCore         = "_logLevelCore";
        private const string PropertyLogLevelDevices      = "_logLevelDevices";
        private const string PropertyLogLevelLocomotion   = "_logLevelLocomotion";
        private const string PropertyLogLevelManipulation = "_logLevelManipulation";
        private const string PropertyLogLevelNetworking   = "_logLevelNetworking";
        private const string PropertyLogLevelUI           = "_logLevelUI";
        private const string PropertyLogLevelWeapons      = "_logLevelWeapons";

        private SerializedProperty _propLogLevelAnimation;
        private SerializedProperty _propLogLevelAvatar;
        private SerializedProperty _propLogLevelCore;
        private SerializedProperty _propLogLevelDevices;
        private SerializedProperty _propLogLevelLocomotion;
        private SerializedProperty _propLogLevelManipulation;
        private SerializedProperty _propLogLevelNetworking;
        private SerializedProperty _propLogLevelUI;
        private SerializedProperty _propLogLevelWeapons;

        private bool _showLogLevels = true;

        #endregion
    }
}