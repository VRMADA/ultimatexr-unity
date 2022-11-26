// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationHapticFeedbackEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Haptics.Helpers;
using UnityEditor;

#pragma warning disable 0414

namespace UltimateXR.Editor.Haptics
{
    /// <summary>
    ///     Custom inspector editor for <see cref="UxrManipulationHapticFeedback" />
    /// </summary>
    [CustomEditor(typeof(UxrManipulationHapticFeedback))]
    [CanEditMultipleObjects]
    public class UxrManipulationHapticFeedbackEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propContinuousManipulationHaptics = serializedObject.FindProperty("_continuousManipulationHaptics");
            _propHapticMixMode                 = serializedObject.FindProperty("_hapticMixMode");
            _propMinAmplitude                  = serializedObject.FindProperty("_minAmplitude");
            _propMaxAmplitude                  = serializedObject.FindProperty("_maxAmplitude");
            _propMinFrequency                  = serializedObject.FindProperty("_minFrequency");
            _propMaxFrequency                  = serializedObject.FindProperty("_maxFrequency");
            _propMinSpeed                      = serializedObject.FindProperty("_minSpeed");
            _propMaxSpeed                      = serializedObject.FindProperty("_maxSpeed");
            _propMinAngularSpeed               = serializedObject.FindProperty("_minAngularSpeed");
            _propMaxAngularSpeed               = serializedObject.FindProperty("_maxAngularSpeed");
            _propUseExternalRigidbody          = serializedObject.FindProperty("_useExternalRigidbody");
            _propExternalRigidbody             = serializedObject.FindProperty("_externalRigidbody");

            _propHapticClipOnGrab    = serializedObject.FindProperty("_hapticClipOnGrab");
            _propHapticClipOnPlace   = serializedObject.FindProperty("_hapticClipOnPlace");
            _propHapticClipOnRelease = serializedObject.FindProperty("_hapticClipOnRelease");
        }

        /// <summary>
        ///     Draws the custom inspector and gathers user input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propContinuousManipulationHaptics);

            if (_propContinuousManipulationHaptics.boolValue)
            {
                EditorGUILayout.PropertyField(_propHapticMixMode);
                EditorGUILayout.Slider(_propMinAmplitude, 0.0f, 1.0f);
                EditorGUILayout.Slider(_propMaxAmplitude, 0.0f, 1.0f);
                EditorGUILayout.PropertyField(_propMinFrequency);
                EditorGUILayout.PropertyField(_propMaxFrequency);
                EditorGUILayout.PropertyField(_propMinSpeed);
                EditorGUILayout.PropertyField(_propMaxSpeed);
                EditorGUILayout.PropertyField(_propMinAngularSpeed);
                EditorGUILayout.PropertyField(_propMaxAngularSpeed);
                EditorGUILayout.PropertyField(_propUseExternalRigidbody);

                if (_propUseExternalRigidbody.boolValue)
                {
                    EditorGUILayout.PropertyField(_propExternalRigidbody);
                }
            }

            EditorGUILayout.PropertyField(_propHapticClipOnGrab,    true);
            EditorGUILayout.PropertyField(_propHapticClipOnPlace,   true);
            EditorGUILayout.PropertyField(_propHapticClipOnRelease, true);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propContinuousManipulationHaptics;
        private SerializedProperty _propHapticMixMode;
        private SerializedProperty _propMinAmplitude;
        private SerializedProperty _propMaxAmplitude;
        private SerializedProperty _propMinFrequency;
        private SerializedProperty _propMaxFrequency;
        private SerializedProperty _propMinSpeed;
        private SerializedProperty _propMaxSpeed;
        private SerializedProperty _propMinAngularSpeed;
        private SerializedProperty _propMaxAngularSpeed;
        private SerializedProperty _propUseExternalRigidbody;
        private SerializedProperty _propExternalRigidbody;

        private SerializedProperty _propHapticClipOnGrab;
        private SerializedProperty _propHapticClipOnPlace;
        private SerializedProperty _propHapticClipOnRelease;

        #endregion
    }
}

#pragma warning restore 0414