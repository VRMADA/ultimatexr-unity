// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimateLightIntensityEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation;
using UltimateXR.Animation.Lights;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Lights
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrAnimatedLightIntensity" />.
    /// </summary>
    [CustomEditor(typeof(UxrAnimatedLightIntensity))]
    [CanEditMultipleObjects]
    public class UxrAnimateLightIntensityEditor : UnityEditor.Editor
    {
        #region Unity

        private void OnEnable()
        {
            _propertyLight                 = serializedObject.FindProperty("_light");
            _propertyAnimationMode         = serializedObject.FindProperty("_animationMode");
            _propertyValueSpeed            = serializedObject.FindProperty("_valueSpeed");
            _propertyValueSpeedDuration    = serializedObject.FindProperty("_valueSpeedDurationSeconds");
            _propertyValueStart            = serializedObject.FindProperty("_valueStart");
            _propertyValueEnd              = serializedObject.FindProperty("_valueEnd");
            _propertyValueDisabled         = serializedObject.FindProperty("_valueDisabled");
            _propertyInterpolationSettings = serializedObject.FindProperty("_interpolationSettings");
            _propertyValueNoiseTimeStart   = serializedObject.FindProperty("_valueNoiseTimeStart");
            _propertyValueNoiseDuration    = serializedObject.FindProperty("_valueNoiseDuration");
            _propertyValueNoiseValueStart  = serializedObject.FindProperty("_valueNoiseValueStart");
            _propertyValueNoiseValueEnd    = serializedObject.FindProperty("_valueNoiseValueEnd");
            _propertyValueNoiseValueMin    = serializedObject.FindProperty("_valueNoiseValueMin");
            _propertyValueNoiseValueMax    = serializedObject.FindProperty("_valueNoiseValueMax");
            _propertyValueNoiseFrequency   = serializedObject.FindProperty("_valueNoiseFrequency");
            _propertyValueNoiseOffset      = serializedObject.FindProperty("_valueNoiseOffset");
            _propertyUseUnscaledTime       = serializedObject.FindProperty("_useUnscaledTime");
        }

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrAnimatedLightIntensity animatedLightIntensity = (UxrAnimatedLightIntensity)serializedObject.targetObject;

            if (animatedLightIntensity == null)
            {
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propertyLight, ContentLight);

            if (animatedLightIntensity.HasFinished == false)
            {
                EditorGUILayout.PropertyField(_propertyAnimationMode, ContentAnimationMode);
            }
            else
            {
                EditorGUILayout.LabelField("Curve finished");
            }

            if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.None)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Speed)
            {
                Vector4AsFloatPropertyField(_propertyValueSpeed, ContentValueSpeed);
                EditorGUILayout.PropertyField(_propertyValueSpeedDuration, ContentValueSpeedDuration);
                EditorGUILayout.PropertyField(_propertyUseUnscaledTime);
            }
            else if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Interpolate)
            {
                Vector4AsFloatPropertyField(_propertyValueStart,    ContentValueStart);
                Vector4AsFloatPropertyField(_propertyValueEnd,      ContentValueEnd);
                Vector4AsFloatPropertyField(_propertyValueDisabled, ContentValueDisabled);
                EditorGUILayout.PropertyField(_propertyInterpolationSettings, ContentInterpolationSettings);
            }
            else if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Noise)
            {
                EditorGUILayout.PropertyField(_propertyValueNoiseTimeStart, ContentValueNoiseTimeStart);
                EditorGUILayout.PropertyField(_propertyValueNoiseDuration,  ContentValueNoiseDuration);
                Vector4AsFloatPropertyField(_propertyValueNoiseValueStart, ContentValueNoiseValueStart);
                Vector4AsFloatPropertyField(_propertyValueNoiseValueEnd,   ContentValueNoiseValueEnd);
                Vector4AsFloatPropertyField(_propertyValueNoiseValueMin,   ContentValueNoiseValueMin);
                Vector4AsFloatPropertyField(_propertyValueNoiseValueMax,   ContentValueNoiseValueMax);
                Vector4AsFloatPropertyField(_propertyValueNoiseFrequency,  ContentValueNoiseFrequency);
                Vector4AsFloatPropertyField(_propertyValueNoiseOffset,     ContentValueNoiseOffset);
                EditorGUILayout.PropertyField(_propertyUseUnscaledTime, ContentUseUnscaledTime);
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Draws a float property field but assigns it to a Vector4 serialized property.
        /// </summary>
        /// <param name="property">Serialized property that targets a Vector4 value</param>
        /// <param name="guiContent">UI information</param>
        private void Vector4AsFloatPropertyField(SerializedProperty property, GUIContent guiContent)
        {
            property.vector4Value = new Vector4(EditorGUILayout.FloatField(guiContent, property.vector4Value.x), 0.0f, 0.0f, 0.0f);
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentLight                 { get; } = new GUIContent("Light");
        private GUIContent ContentAnimationMode         { get; } = new GUIContent("Animation Mode");
        private GUIContent ContentValueSpeed            { get; } = new GUIContent("Speed");
        private GUIContent ContentValueSpeedDuration    { get; } = new GUIContent("Duration (seconds)");
        private GUIContent ContentValueStart            { get; } = new GUIContent("Start Value");
        private GUIContent ContentValueEnd              { get; } = new GUIContent("End Value");
        private GUIContent ContentValueDisabled         { get; } = new GUIContent("Value When Disabled");
        private GUIContent ContentInterpolationSettings { get; } = new GUIContent("Interpolation Settings");
        private GUIContent ContentValueNoiseTimeStart   { get; } = new GUIContent("Noise Time Start");
        private GUIContent ContentValueNoiseDuration    { get; } = new GUIContent("Noise Duration");
        private GUIContent ContentValueNoiseValueStart  { get; } = new GUIContent("Value Start");
        private GUIContent ContentValueNoiseValueEnd    { get; } = new GUIContent("Value End");
        private GUIContent ContentValueNoiseValueMin    { get; } = new GUIContent("Noise Value Min");
        private GUIContent ContentValueNoiseValueMax    { get; } = new GUIContent("Noise Value Max");
        private GUIContent ContentValueNoiseFrequency   { get; } = new GUIContent("Noise Frequency");
        private GUIContent ContentValueNoiseOffset      { get; } = new GUIContent("Noise Offset");
        private GUIContent ContentUseUnscaledTime       { get; } = new GUIContent("Use Unscaled Time");

        private SerializedProperty _propertyLight;
        private SerializedProperty _propertyAnimationMode;
        private SerializedProperty _propertyValueSpeed;
        private SerializedProperty _propertyValueSpeedDuration;
        private SerializedProperty _propertyValueStart;
        private SerializedProperty _propertyValueEnd;
        private SerializedProperty _propertyValueDisabled;
        private SerializedProperty _propertyInterpolationSettings;
        private SerializedProperty _propertyValueNoiseTimeStart;
        private SerializedProperty _propertyValueNoiseDuration;
        private SerializedProperty _propertyValueNoiseValueStart;
        private SerializedProperty _propertyValueNoiseValueEnd;
        private SerializedProperty _propertyValueNoiseValueMin;
        private SerializedProperty _propertyValueNoiseValueMax;
        private SerializedProperty _propertyValueNoiseFrequency;
        private SerializedProperty _propertyValueNoiseOffset;
        private SerializedProperty _propertyUseUnscaledTime;

        #endregion
    }
}