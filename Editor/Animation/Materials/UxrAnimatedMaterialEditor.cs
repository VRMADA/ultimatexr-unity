// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedMaterialEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation;
using UltimateXR.Animation.Materials;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Materials
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrAnimatedMaterial" />.
    /// </summary>
    [CustomEditor(typeof(UxrAnimatedMaterial))]
    [CanEditMultipleObjects]
    public class UxrAnimatedMaterialEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyAnimationMode         = serializedObject.FindProperty("_animationMode");
            _propertyAnimateSelf           = serializedObject.FindProperty("_animateSelf");
            _propertyTargetGameObject      = serializedObject.FindProperty("_targetGameObject");
            _propertyMaterialSlot          = serializedObject.FindProperty("_materialSlot");
            _propertyMaterialMode          = serializedObject.FindProperty("_materialMode");
            _propertyRestoreWhenFinished   = serializedObject.FindProperty("_restoreWhenFinished");
            _propertyParameterType         = serializedObject.FindProperty("_parameterType");
            _propertyParameterName         = serializedObject.FindProperty("_parameterName");
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
        ///     Draws the inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrAnimatedMaterial animatedMaterial = (UxrAnimatedMaterial)serializedObject.targetObject;

            if (animatedMaterial == null)
            {
                return;
            }

            EditorGUILayout.Space();

            if (animatedMaterial.HasFinished == false)
            {
                EditorGUILayout.PropertyField(_propertyAnimationMode, ContentAnimationMode);
                EditorGUILayout.PropertyField(_propertyAnimateSelf,   ContentAnimateSelf);

                if (!_propertyAnimateSelf.boolValue)
                {
                    EditorGUILayout.PropertyField(_propertyTargetGameObject, ContentTargetGameObject);
                }
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

            EditorGUILayout.PropertyField(_propertyMaterialSlot, ContentMaterialSlot);
            EditorGUILayout.PropertyField(_propertyMaterialMode, ContentMaterialMode);

            if (_propertyMaterialMode.enumValueIndex == (int)UxrMaterialMode.InstanceOnly)
            {
                EditorGUILayout.PropertyField(_propertyRestoreWhenFinished, ContentRestoreWhenFinished);
            }

            EditorGUILayout.PropertyField(_propertyParameterType, ContentParameterType);
            EditorGUILayout.PropertyField(_propertyParameterName, ContentParameterName);

            if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Speed)
            {
                ParameterPropertyField(ContentValueSpeed, _propertyValueSpeed);
                EditorGUILayout.PropertyField(_propertyValueSpeedDuration, ContentValueSpeedDuration);
                EditorGUILayout.PropertyField(_propertyUseUnscaledTime,    ContentUseUnscaledTime);
            }
            else if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Interpolate)
            {
                ParameterPropertyField(ContentValueStart,    _propertyValueStart,    true);
                ParameterPropertyField(ContentValueEnd,      _propertyValueEnd,      true);
                ParameterPropertyField(ContentValueDisabled, _propertyValueDisabled, true);
                EditorGUILayout.PropertyField(_propertyInterpolationSettings, ContentInterpolationSettings, true);
            }
            else if (_propertyAnimationMode.enumValueIndex == (int)UxrAnimationMode.Noise)
            {
                EditorGUILayout.PropertyField(_propertyValueNoiseTimeStart, ContentValueNoiseTimeStart);
                EditorGUILayout.PropertyField(_propertyValueNoiseDuration,  ContentValueNoiseDuration);
                ParameterPropertyField(ContentValueNoiseValueStart, _propertyValueNoiseValueStart, true);
                ParameterPropertyField(ContentValueNoiseValueEnd,   _propertyValueNoiseValueEnd,   true);
                ParameterPropertyField(ContentValueNoiseValueMin,   _propertyValueNoiseValueMin,   true);
                ParameterPropertyField(ContentValueNoiseValueMax,   _propertyValueNoiseValueMax,   true);
                ParameterPropertyField(ContentValueNoiseFrequency,  _propertyValueNoiseFrequency);
                ParameterPropertyField(ContentValueNoiseOffset,     _propertyValueNoiseOffset);
                EditorGUILayout.PropertyField(_propertyUseUnscaledTime, ContentUseUnscaledTime);
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Draws an inspector property field depending on the material parameter type.
        /// </summary>
        /// <param name="guiContent">The label and tooltip to show in the inspector</param>
        /// <param name="property">The serialized property</param>
        /// <param name="isParameterValue">
        ///     When using colors, Whether to force to show the field as a vector4 instead of a color
        ///     picker
        /// </param>
        private void ParameterPropertyField(GUIContent guiContent, SerializedProperty property, bool isParameterValue = false)
        {
            switch (_propertyParameterType.enumValueIndex)
            {
                case (int)UxrMaterialParameterType.Int:
                    property.vector4Value = new Vector4(EditorGUILayout.IntField(guiContent, Mathf.RoundToInt(property.vector4Value.x)), 0, 0, 0);
                    break;

                case (int)UxrMaterialParameterType.Float:
                    property.vector4Value = new Vector4(EditorGUILayout.FloatField(guiContent, property.vector4Value.x), 0, 0, 0);
                    break;

                case (int)UxrMaterialParameterType.Vector2:
                    property.vector4Value = EditorGUILayout.Vector2Field(guiContent, new Vector2(property.vector4Value.x, property.vector4Value.y));
                    break;

                case (int)UxrMaterialParameterType.Vector3:
                    property.vector4Value = EditorGUILayout.Vector3Field(guiContent, new Vector3(property.vector4Value.x, property.vector4Value.y, property.vector4Value.z));
                    break;

                case (int)UxrMaterialParameterType.Vector4:
                    property.vector4Value = EditorGUILayout.Vector4Field(guiContent, property.vector4Value);
                    break;

                case (int)UxrMaterialParameterType.Color:

                    if (!isParameterValue)
                    {
                        property.vector4Value = EditorGUILayout.Vector4Field(guiContent, property.vector4Value);
                    }
                    else
                    {
                        property.vector4Value = EditorGUILayout.ColorField(guiContent, property.vector4Value);
                    }

                    break;
            }
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentAnimationMode         { get; } = new GUIContent("Animation Mode");
        private GUIContent ContentAnimateSelf           { get; } = new GUIContent("Animate Self");
        private GUIContent ContentTargetGameObject      { get; } = new GUIContent("Target GameObject");
        private GUIContent ContentMaterialSlot          { get; } = new GUIContent("Material Slot");
        private GUIContent ContentMaterialMode          { get; } = new GUIContent("Material Mode");
        private GUIContent ContentRestoreWhenFinished   { get; } = new GUIContent("Restore When Finished", "Restores the original material when the instance animation finished. Use this for performance since shared materials can save draw calls and render state changes");
        private GUIContent ContentParameterType         { get; } = new GUIContent("Parameter Type");
        private GUIContent ContentParameterName         { get; } = new GUIContent("Parameter Name");
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

        private SerializedProperty _propertyAnimationMode;
        private SerializedProperty _propertyAnimateSelf;
        private SerializedProperty _propertyTargetGameObject;
        private SerializedProperty _propertyMaterialSlot;
        private SerializedProperty _propertyMaterialMode;
        private SerializedProperty _propertyRestoreWhenFinished;
        private SerializedProperty _propertyParameterType;
        private SerializedProperty _propertyParameterName;
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