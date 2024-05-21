// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGlobalSettingsEditor.cs" company="VRMADA">
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
            _propLogLevelAnimation               = serializedObject.FindProperty(PropertyLogLevelAnimation);
            _propLogLevelAvatar                  = serializedObject.FindProperty(PropertyLogLevelAvatar);
            _propLogLevelCore                    = serializedObject.FindProperty(PropertyLogLevelCore);
            _propLogLevelDevices                 = serializedObject.FindProperty(PropertyLogLevelDevices);
            _propLogLevelLocomotion              = serializedObject.FindProperty(PropertyLogLevelLocomotion);
            _propLogLevelManipulation            = serializedObject.FindProperty(PropertyLogLevelManipulation);
            _propLogLevelNetworking              = serializedObject.FindProperty(PropertyLogLevelNetworking);
            _propLogLevelRendering               = serializedObject.FindProperty(PropertyLogLevelRendering);
            _propLogLevelUI                      = serializedObject.FindProperty(PropertyLogLevelUI);
            _propLogLevelWeapons                 = serializedObject.FindProperty(PropertyLogLevelWeapons);
            _propNetFormatInitialState           = serializedObject.FindProperty(PropertyNetFormatInitialState);
            _propNetFormatStateSync              = serializedObject.FindProperty(PropertyNetFormatStateSync);
            _propNetSyncGrabbablePhysics         = serializedObject.FindProperty(PropertyNetSyncGrabbablePhysics);
            _propNetGrabbableSyncIntervalSeconds = serializedObject.FindProperty(PropertyNetGrabbableSyncIntervalSeconds);
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
                EditorGUILayout.PropertyField(_propLogLevelRendering,    ContentLogLevelRendering);
                EditorGUILayout.PropertyField(_propLogLevelUI,           ContentLogLevelUI);
                EditorGUILayout.PropertyField(_propLogLevelWeapons,      ContentLogLevelWeapons);
            }

            // Networking config

            _showNetworking = UxrEditorUtils.FoldoutStylish("Networking", _showNetworking);

            if (_showNetworking)
            {
                EditorGUILayout.PropertyField(_propNetFormatInitialState,   ContentNetFormatInitialState);
                EditorGUILayout.PropertyField(_propNetFormatStateSync,      ContentNetFormatStateSync);
                EditorGUILayout.PropertyField(_propNetSyncGrabbablePhysics, ContentNetSyncGrabbablePhysics);

                if (_propNetSyncGrabbablePhysics.boolValue)
                {
                    EditorGUILayout.Slider(_propNetGrabbableSyncIntervalSeconds, 0.1f, 10.0f, ContentNetGrabbableSyncIntervalSeconds);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentLogLevelAnimation               { get; } = new GUIContent("Animation",                         "Selects the console log level for animation events");
        private GUIContent ContentLogLevelAvatar                  { get; } = new GUIContent("Avatar",                            "Selects the console log level for avatar events");
        private GUIContent ContentLogLevelCore                    { get; } = new GUIContent("Core",                              "Selects the console log level for core events");
        private GUIContent ContentLogLevelDevices                 { get; } = new GUIContent("Devices",                           "Selects the console log level for device events");
        private GUIContent ContentLogLevelLocomotion              { get; } = new GUIContent("Locomotion",                        "Selects the console log level for locomotion events");
        private GUIContent ContentLogLevelManipulation            { get; } = new GUIContent("Manipulation",                      "Selects the console log level for manipulation events");
        private GUIContent ContentLogLevelNetworking              { get; } = new GUIContent("Networking",                        "Selects the console log level for networking events");
        private GUIContent ContentLogLevelRendering               { get; } = new GUIContent("Rendering",                         "Selects the console log level for rendering events");
        private GUIContent ContentLogLevelUI                      { get; } = new GUIContent("UI",                                "Selects the console log level for UI events");
        private GUIContent ContentLogLevelWeapons                 { get; } = new GUIContent("Weapons",                           "Selects the console log level for weapon events");
        private GUIContent ContentNetFormatInitialState           { get; } = new GUIContent("Initial State Msg Format",          "Selects the message format to use when the host sends the initial state of the session upon joining. Compression has a little CPU overhead but will use less bandwidth.");
        private GUIContent ContentNetFormatStateSync              { get; } = new GUIContent("State Sync Msg Format",             "Selects the message format to use when exchanging state synchronization updates. Compression has a little CPU overhead but will use less bandwidth.");
        private GUIContent ContentNetSyncGrabbablePhysics         { get; } = new GUIContent("Sync Grabbable Physics",            "Selects whether to sync grabbable objects with rigidbodies that have no NetworkTransform/NetworkRidibody set up. This keeps position/rotation and speeds manually in sync by sending periodic messages.");
        private GUIContent ContentNetGrabbableSyncIntervalSeconds { get; } = new GUIContent("Grabbable Sync Interval (Seconds)", "Selects the interval in seconds grabbable objects with rigidbodies are kept in sync when there are no NetworkTransform/NetworkRidibody set up. Lower values will send RPC messages more frequently but will increase bandwidth.");

        private const string PropertyLogLevelAnimation               = "_logLevelAnimation";
        private const string PropertyLogLevelAvatar                  = "_logLevelAvatar";
        private const string PropertyLogLevelCore                    = "_logLevelCore";
        private const string PropertyLogLevelDevices                 = "_logLevelDevices";
        private const string PropertyLogLevelLocomotion              = "_logLevelLocomotion";
        private const string PropertyLogLevelManipulation            = "_logLevelManipulation";
        private const string PropertyLogLevelNetworking              = "_logLevelNetworking";
        private const string PropertyLogLevelRendering               = "_logLevelRendering";
        private const string PropertyLogLevelUI                      = "_logLevelUI";
        private const string PropertyLogLevelWeapons                 = "_logLevelWeapons";
        private const string PropertyNetFormatInitialState           = "_netFormatInitialState";
        private const string PropertyNetFormatStateSync              = "_netFormatStateSync";
        private const string PropertyNetSyncGrabbablePhysics         = "_syncGrabbablePhysics";
        private const string PropertyNetGrabbableSyncIntervalSeconds = "_grabbableSyncIntervalSeconds";

        private SerializedProperty _propLogLevelAnimation;
        private SerializedProperty _propLogLevelAvatar;
        private SerializedProperty _propLogLevelCore;
        private SerializedProperty _propLogLevelDevices;
        private SerializedProperty _propLogLevelLocomotion;
        private SerializedProperty _propLogLevelManipulation;
        private SerializedProperty _propLogLevelNetworking;
        private SerializedProperty _propLogLevelRendering;
        private SerializedProperty _propLogLevelUI;
        private SerializedProperty _propLogLevelWeapons;
        private SerializedProperty _propNetFormatInitialState;
        private SerializedProperty _propNetFormatStateSync;
        private SerializedProperty _propNetSyncGrabbablePhysics;
        private SerializedProperty _propNetGrabbableSyncIntervalSeconds;

        private bool _showLogLevels  = true;
        private bool _showNetworking = true;

        #endregion
    }
}