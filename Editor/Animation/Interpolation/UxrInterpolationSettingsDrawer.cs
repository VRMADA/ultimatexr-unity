// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInterpolationSettingsDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Interpolation
{
    /// <summary>
    ///     Custom inspector drawer for <see cref="UxrInterpolationSettings" />.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrInterpolationSettings))]
    public class UxrInterpolationSettingsDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <summary>
        ///     Gets the height in pixels required to draw the property.
        /// </summary>
        /// <param name="property">Serialized property describing an <see cref="UxrInterpolationSettings" /></param>
        /// <param name="label">UI label</param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 5;

            if (property.FindPropertyRelative(PropertyLoopMode).enumValueIndex != (int)UxrLoopMode.None)
            {
                lineCount++;
            }

            if (property.FindPropertyRelative(PropertyDelay).floatValue > 0.0f)
            {
                lineCount++;
            }

            return lineCount * EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        /// <param name="position">Position where to draw the inspector</param>
        /// <param name="property">Serialized property to draw</param>
        /// <param name="label">UI label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int line = 0;

            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyDurationSeconds), ContentDurationSeconds);
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyDelay),           ContentDelay);
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyEasing),          ContentEasing);
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyLoopMode),        ContentLoopMode);

            if (property.FindPropertyRelative(PropertyLoopMode).enumValueIndex != (int)UxrLoopMode.None)
            {
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyLoopedDurationSeconds), ContentLoopedDurationSeconds);
            }

            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyUnscaledTime), ContentUnscaledTime);

            if (property.FindPropertyRelative(PropertyDelay).floatValue > 0.0f)
            {
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line), property.FindPropertyRelative(PropertyDelayUsingEndValue), ContentDelayUsingEndValue);
            }
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentDurationSeconds       { get; } = new GUIContent("Duration (Seconds)",          "");
        private GUIContent ContentDelay                 { get; } = new GUIContent("Delay (Seconds)",             "");
        private GUIContent ContentEasing                { get; } = new GUIContent("Easing",                      "");
        private GUIContent ContentLoopMode              { get; } = new GUIContent("Loop Mode",                   "");
        private GUIContent ContentLoopedDurationSeconds { get; } = new GUIContent("Looped Duration (Seconds)",   "");
        private GUIContent ContentUnscaledTime          { get; } = new GUIContent("Use Unscaled Time",           "");
        private GUIContent ContentDelayUsingEndValue    { get; } = new GUIContent("Use End Value During Delay?", "");

        private const string PropertyDurationSeconds       = "_durationSeconds";
        private const string PropertyDelay                 = "_delaySeconds";
        private const string PropertyEasing                = "_easing";
        private const string PropertyLoopMode              = "_loopMode";
        private const string PropertyLoopedDurationSeconds = "_loopedDurationSeconds";
        private const string PropertyUnscaledTime          = "_useUnscaledTime";
        private const string PropertyDelayUsingEndValue    = "_delayUsingEndValue";

        #endregion
    }
}