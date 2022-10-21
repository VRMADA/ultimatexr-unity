// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAudioManipulationEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Audio;
using UnityEditor;
using UnityEngine;

#pragma warning disable 0414

namespace UltimateXR.Editor.Audio
{
    /// <summary>
    ///     Custom editor used by the <see cref="UxrAudioManipulation" /> component.
    /// </summary>
    [CustomEditor(typeof(UxrAudioManipulation))]
    [CanEditMultipleObjects]
    public class UxrAudioManipulationEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties
        /// </summary>
        private void OnEnable()
        {
            _propContinuousManipulationAudio = serializedObject.FindProperty("_continuousManipulationAudio");
            _propAudioLoopClip               = serializedObject.FindProperty("_audioLoopClip");
            _propMinVolume                   = serializedObject.FindProperty("_minVolume");
            _propMaxVolume                   = serializedObject.FindProperty("_maxVolume");
            _propMinFrequency                = serializedObject.FindProperty("_minFrequency");
            _propMaxFrequency                = serializedObject.FindProperty("_maxFrequency");
            _propMinSpeed                    = serializedObject.FindProperty("_minSpeed");
            _propMaxSpeed                    = serializedObject.FindProperty("_maxSpeed");
            _propMinAngularSpeed             = serializedObject.FindProperty("_minAngularSpeed");
            _propMaxAngularSpeed             = serializedObject.FindProperty("_maxAngularSpeed");
            _propUseExternalRigidbody        = serializedObject.FindProperty("_useExternalRigidbody");
            _propExternalRigidbody           = serializedObject.FindProperty("_externalRigidbody");

            _propAudioOnGrab    = serializedObject.FindProperty("_audioOnGrab");
            _propAudioOnPlace   = serializedObject.FindProperty("_audioOnPlace");
            _propAudioOnRelease = serializedObject.FindProperty("_audioOnRelease");
        }

        /// <summary>
        ///     Draws the custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propContinuousManipulationAudio, ContentContinuousManipulationAudio);

            if (_propContinuousManipulationAudio.boolValue)
            {
                EditorGUILayout.PropertyField(_propAudioLoopClip, ContentAudioLoopClip);
                EditorGUILayout.Slider(_propMinVolume, 0.0f, 1.0f, ContentMinVolume);
                EditorGUILayout.Slider(_propMaxVolume, 0.0f, 1.0f, ContentMaxVolume);
                EditorGUILayout.PropertyField(_propMinFrequency,         ContentMinFrequency);
                EditorGUILayout.PropertyField(_propMaxFrequency,         ContentMaxFrequency);
                EditorGUILayout.PropertyField(_propMinSpeed,             ContentMinSpeed);
                EditorGUILayout.PropertyField(_propMaxSpeed,             ContentMaxSpeed);
                EditorGUILayout.PropertyField(_propMinAngularSpeed,      ContentMinAngularSpeed);
                EditorGUILayout.PropertyField(_propMaxAngularSpeed,      ContentMaxAngularSpeed);
                EditorGUILayout.PropertyField(_propUseExternalRigidbody, ContentUseExternalRigidbody);

                if (_propUseExternalRigidbody.boolValue)
                {
                    EditorGUILayout.PropertyField(_propExternalRigidbody, ContentExternalRigidbody);
                }
            }

            EditorGUILayout.PropertyField(_propAudioOnGrab,    true);
            EditorGUILayout.PropertyField(_propAudioOnPlace,   true);
            EditorGUILayout.PropertyField(_propAudioOnRelease, true);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentContinuousManipulationAudio { get; } = new GUIContent("Continuous Manipulation", "");
        private GUIContent ContentAudioLoopClip               { get; } = new GUIContent("Audio Loop Clip",         "");
        private GUIContent ContentMinVolume                   { get; } = new GUIContent("Min Volume",              "");
        private GUIContent ContentMaxVolume                   { get; } = new GUIContent("Max Volume",              "");
        private GUIContent ContentMinFrequency                { get; } = new GUIContent("Min Frequency",           "");
        private GUIContent ContentMaxFrequency                { get; } = new GUIContent("Max Frequency",           "");
        private GUIContent ContentMinSpeed                    { get; } = new GUIContent("Min Speed",               "");
        private GUIContent ContentMaxSpeed                    { get; } = new GUIContent("Max Speed",               "");
        private GUIContent ContentMinAngularSpeed             { get; } = new GUIContent("Min Angular Speed",       "");
        private GUIContent ContentMaxAngularSpeed             { get; } = new GUIContent("Max Angular Speed",       "");
        private GUIContent ContentUseExternalRigidbody        { get; } = new GUIContent("Use External Rigidbody",  "");
        private GUIContent ContentExternalRigidbody           { get; } = new GUIContent("External Rigidbody",      "");

        private SerializedProperty _propContinuousManipulationAudio;
        private SerializedProperty _propAudioLoopClip;
        private SerializedProperty _propMinVolume;
        private SerializedProperty _propMaxVolume;
        private SerializedProperty _propMinFrequency;
        private SerializedProperty _propMaxFrequency;
        private SerializedProperty _propMinSpeed;
        private SerializedProperty _propMaxSpeed;
        private SerializedProperty _propMinAngularSpeed;
        private SerializedProperty _propMaxAngularSpeed;
        private SerializedProperty _propUseExternalRigidbody;
        private SerializedProperty _propExternalRigidbody;

        private SerializedProperty _propAudioOnGrab;
        private SerializedProperty _propAudioOnPlace;
        private SerializedProperty _propAudioOnRelease;

        #endregion
    }
}

#pragma warning restore 0414