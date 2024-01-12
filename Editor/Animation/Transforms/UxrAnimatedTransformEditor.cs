// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedTransformEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation;
using UltimateXR.Animation.Transforms;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Transforms
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrAnimatedTransform" />.
    /// </summary>
    [CustomEditor(typeof(UxrAnimatedTransform))]
    public class UxrAnimatedTransformEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyTranslationMode                 = serializedObject.FindProperty("_translationMode");
            _propertyTranslationSpace                = serializedObject.FindProperty("_translationSpace");
            _propertyTranslationSpeed                = serializedObject.FindProperty("_translationSpeed");
            _propertyTranslationStart                = serializedObject.FindProperty("_translationStart");
            _propertyTranslationEnd                  = serializedObject.FindProperty("_translationEnd");
            _propertyTranslationUseUnscaledTime      = serializedObject.FindProperty("_translationUseUnscaledTime");
            _propertyTranslationInterpolationSetting = serializedObject.FindProperty("_translationInterpolationSettings");
            _propertyRotationMode                    = serializedObject.FindProperty("_rotationMode");
            _propertyRotationSpace                   = serializedObject.FindProperty("_rotationSpace");
            _propertyEulerSpeed                      = serializedObject.FindProperty("_eulerSpeed");
            _propertyEulerStart                      = serializedObject.FindProperty("_eulerStart");
            _propertyEulerEnd                        = serializedObject.FindProperty("_eulerEnd");
            _propertyRotationUseUnscaledTime         = serializedObject.FindProperty("_rotationUseUnscaledTime");
            _propertyRotationInterpolationSettings   = serializedObject.FindProperty("_rotationInterpolationSettings");
            _propertyScalingMode                     = serializedObject.FindProperty("_scalingMode");
            _propertyScalingSpeed                    = serializedObject.FindProperty("_scalingSpeed");
            _propertyScalingStart                    = serializedObject.FindProperty("_scalingStart");
            _propertyScalingEnd                      = serializedObject.FindProperty("_scalingEnd");
            _propertyScalingUseUnscaledTime          = serializedObject.FindProperty("_scalingUseUnscaledTime");
            _propertyScalingInterpolationSettings    = serializedObject.FindProperty("_scalingInterpolationSettings");
        }

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrAnimatedTransform animatedTransform = (UxrAnimatedTransform)serializedObject.targetObject;

            if (animatedTransform == null)
            {
                return;
            }

            EditorGUILayout.Space();

            if (animatedTransform.HasTranslationFinished == false)
            {
                EditorGUILayout.PropertyField(_propertyTranslationMode, ContentTranslationMode);
            }
            else
            {
                EditorGUILayout.LabelField("Translation curve finished");
            }

            if (_propertyTranslationMode.enumValueIndex == (int)UxrAnimationMode.Speed)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyTranslationSpace,           ContentTranslationSpace);
                EditorGUILayout.PropertyField(_propertyTranslationSpeed,           ContentTranslationSpeed);
                EditorGUILayout.PropertyField(_propertyTranslationUseUnscaledTime, ContentTranslationUseUnscaledTime);
                EditorGUI.indentLevel--;
            }
            else if (_propertyTranslationMode.enumValueIndex == (int)UxrAnimationMode.Interpolate)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyTranslationSpace,                ContentTranslationSpace);
                EditorGUILayout.PropertyField(_propertyTranslationStart,                ContentTranslationStart);
                EditorGUILayout.PropertyField(_propertyTranslationEnd,                  ContentTranslationEnd);
                EditorGUILayout.PropertyField(_propertyTranslationInterpolationSetting, ContentTranslationInterpolationSetting);
                EditorGUI.indentLevel--;
            }
            else if (_propertyTranslationMode.enumValueIndex == (int)UxrAnimationMode.Noise)
            {
                EditorGUILayout.LabelField("Unsupported for now");
            }

            EditorGUILayout.Space();

            if (animatedTransform.HasRotationFinished == false)
            {
                EditorGUILayout.PropertyField(_propertyRotationMode, ContentRotationMode);
            }
            else
            {
                EditorGUILayout.LabelField("Rotation curve finished");
            }

            if (_propertyRotationMode.enumValueIndex == (int)UxrAnimationMode.Speed)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyRotationSpace,           ContentRotationSpace);
                EditorGUILayout.PropertyField(_propertyEulerSpeed,              ContentEulerSpeed);
                EditorGUILayout.PropertyField(_propertyRotationUseUnscaledTime, ContentRotationUseUnscaledTime);
                EditorGUI.indentLevel--;
            }
            else if (_propertyRotationMode.enumValueIndex == (int)UxrAnimationMode.Interpolate)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyRotationSpace,                 ContentRotationSpace);
                EditorGUILayout.PropertyField(_propertyEulerStart,                    ContentEulerStart);
                EditorGUILayout.PropertyField(_propertyEulerEnd,                      ContentEulerEnd);
                EditorGUILayout.PropertyField(_propertyRotationInterpolationSettings, ContentRotationInterpolationSettings);
                EditorGUI.indentLevel--;
            }
            else if (_propertyRotationMode.enumValueIndex == (int)UxrAnimationMode.Noise)
            {
                EditorGUILayout.LabelField("Unsupported for now");
            }

            EditorGUILayout.Space();

            if (animatedTransform.HasScalingFinished == false)
            {
                EditorGUILayout.PropertyField(_propertyScalingMode, ContentScalingMode);
            }
            else
            {
                EditorGUILayout.LabelField("Scaling curve finished");
            }

            if (_propertyScalingMode.enumValueIndex == (int)UxrAnimationMode.Speed)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyScalingSpeed,           ContentScalingSpeed);
                EditorGUILayout.PropertyField(_propertyScalingUseUnscaledTime, ContentScalingUseUnscaledTime);
                EditorGUI.indentLevel--;
            }
            else if (_propertyScalingMode.enumValueIndex == (int)UxrAnimationMode.Interpolate)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_propertyScalingStart,                 ContentScalingStart);
                EditorGUILayout.PropertyField(_propertyScalingEnd,                   ContentScalingEnd);
                EditorGUILayout.PropertyField(_propertyScalingInterpolationSettings, ContentScalingInterpolationSettings);
                EditorGUI.indentLevel--;
            }
            else if (_propertyScalingMode.enumValueIndex == (int)UxrAnimationMode.Noise)
            {
                EditorGUILayout.LabelField("Unsupported for now");
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentTranslationMode                 { get; } = new GUIContent("Translation Mode",       "Selects the translation mode to use");
        private GUIContent ContentTranslationSpace                { get; } = new GUIContent("Translation Space",      "Selects the space in which the translation will be performed");
        private GUIContent ContentTranslationSpeed                { get; } = new GUIContent("Translation Speed",      "The number of units per second to move");
        private GUIContent ContentTranslationStart                { get; } = new GUIContent("Start Position",         "The start position. In a looped animation, the start position of each loop");
        private GUIContent ContentTranslationEnd                  { get; } = new GUIContent("End Position",           "The end position. In a looped animation, the end position of each loop");
        private GUIContent ContentTranslationUseUnscaledTime      { get; } = new GUIContent("Use Unscaled Time",      "Whether to use unscaled time, which is the timer without considering time scaling for effects such as bullet-time");
        private GUIContent ContentTranslationInterpolationSetting { get; } = new GUIContent("Interpolation Settings", "The translation interpolation settings");
        private GUIContent ContentRotationMode                    { get; } = new GUIContent("Rotation Mode",          "Selects the rotation mode to use");
        private GUIContent ContentRotationSpace                   { get; } = new GUIContent("Rotation Space",         "Selects the space in which the rotation will be performed");
        private GUIContent ContentEulerSpeed                      { get; } = new GUIContent("Angular Speed",          "The number of degrees per second to rotate");
        private GUIContent ContentEulerStart                      { get; } = new GUIContent("Start Angles",           "The start Euler angles. In a looped animation, the start Euler angles of each loop");
        private GUIContent ContentEulerEnd                        { get; } = new GUIContent("End Angles",             "The end Euler angles. In a looped animation, the end Euler angles of each loop");
        private GUIContent ContentRotationUseUnscaledTime         { get; } = new GUIContent("Use Unscaled Time",      "Whether to use unscaled time, which is the timer without considering time scaling for effects such as bullet-time");
        private GUIContent ContentRotationInterpolationSettings   { get; } = new GUIContent("Interpolation Settings", "The rotation interpolation settings");
        private GUIContent ContentScalingMode                     { get; } = new GUIContent("Scaling Mode",           "Selects the scaling mode to use");
        private GUIContent ContentScalingSpeed                    { get; } = new GUIContent("Scaling Speed",          "The number of units per second to scale");
        private GUIContent ContentScalingStart                    { get; } = new GUIContent("Start Scale",            "The start scale. In a looped animation, the start scale of each loop");
        private GUIContent ContentScalingEnd                      { get; } = new GUIContent("End Scale",              "The end scale. In a looped animation, the end scale of each loop");
        private GUIContent ContentScalingUseUnscaledTime          { get; } = new GUIContent("Use Unscaled Time",      "Whether to use unscaled time, which is the timer without considering time scaling for effects such as bullet-time");
        private GUIContent ContentScalingInterpolationSettings    { get; } = new GUIContent("Interpolation Settings", "The scale interpolation settings");

        private SerializedProperty _propertyTranslationMode;
        private SerializedProperty _propertyTranslationSpace;
        private SerializedProperty _propertyTranslationSpeed;
        private SerializedProperty _propertyTranslationStart;
        private SerializedProperty _propertyTranslationEnd;
        private SerializedProperty _propertyTranslationUseUnscaledTime;
        private SerializedProperty _propertyTranslationInterpolationSetting;
        private SerializedProperty _propertyRotationMode;
        private SerializedProperty _propertyRotationSpace;
        private SerializedProperty _propertyEulerSpeed;
        private SerializedProperty _propertyEulerStart;
        private SerializedProperty _propertyEulerEnd;
        private SerializedProperty _propertyRotationUseUnscaledTime;
        private SerializedProperty _propertyRotationInterpolationSettings;
        private SerializedProperty _propertyScalingMode;
        private SerializedProperty _propertyScalingSpeed;
        private SerializedProperty _propertyScalingStart;
        private SerializedProperty _propertyScalingEnd;
        private SerializedProperty _propertyScalingUseUnscaledTime;
        private SerializedProperty _propertyScalingInterpolationSettings;

        #endregion
    }
}