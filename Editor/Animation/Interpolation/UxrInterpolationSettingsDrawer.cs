// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInterpolationSettingsDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core;
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
        #region Constructors & Finalizer

        /// <summary>
        ///     Creates the temporal material to draw the graph.
        /// </summary>
        public UxrInterpolationSettingsDrawer()
        {
            var shader = Shader.Find(UxrConstants.Shaders.HiddenInternalColoredShader);
            _lineMaterial = new Material(shader);
        }

        /// <summary>
        ///     Destroys the temporal material to draw the graph.
        /// </summary>
        ~UxrInterpolationSettingsDrawer()
        {
            Object.DestroyImmediate(_lineMaterial);
        }

        #endregion

        #region Public Overrides PropertyDrawer

        /// <summary>
        ///     Gets the height in pixels required to draw the property.
        /// </summary>
        /// <param name="property">Serialized property describing an <see cref="UxrInterpolationSettings" /></param>
        /// <param name="label">UI label</param>
        /// <returns>Height in pixels</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount       = 5;
            int loopGraphHeight = 0;

            if (property.FindPropertyRelative(PropertyLoopMode).enumValueIndex != (int)UxrLoopMode.None)
            {
                lineCount++;
                loopGraphHeight += UxrEasingDrawer.GraphHeight;
            }

            if (property.FindPropertyRelative(PropertyDelay).floatValue > 0.0f)
            {
                lineCount++;
            }

            return lineCount * EditorGUIUtility.singleLineHeight + UxrEasingDrawer.GraphHeight + loopGraphHeight;
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
            position.y += UxrEasingDrawer.GraphHeight;
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyLoopMode), ContentLoopMode);

            UxrLoopMode loopMode = (UxrLoopMode)property.FindPropertyRelative(PropertyLoopMode).enumValueIndex;

            if (loopMode != (int)UxrLoopMode.None)
            {
                // Draw preview graph
                
                Rect graphRect = UxrEditorUtils.GetRect(position, line);
                graphRect.height =  UxrEasingDrawer.GraphHeight;
                graphRect.xMin   += EditorGUIUtility.labelWidth;

                UxrEasing easing = (UxrEasing)property.FindPropertyRelative(PropertyEasing).enumValueIndex;
                
                UxrEasingDrawer.DrawGraph(graphRect, _lineMaterial, Color.green, easing, loopMode, 5);

                position.y += UxrEasingDrawer.GraphHeight;
                
                // Draw looped duration property
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

        private GUIContent ContentDurationSeconds       { get; } = new GUIContent("Duration (Seconds)",          "The duration in seconds of the interpolation. In a looped animation it specifies the duration of each loop.");
        private GUIContent ContentDelay                 { get; } = new GUIContent("Delay (Seconds)",             "The seconds to wait before the interpolation starts");
        private GUIContent ContentEasing                { get; } = new GUIContent("Easing",                      "The animation curve to use for the interpolation");
        private GUIContent ContentLoopMode              { get; } = new GUIContent("Loop Mode",                   "The type of loop to use");
        private GUIContent ContentLoopedDurationSeconds { get; } = new GUIContent("Looped Duration (Seconds)",   "The total duration in seconds in a looped interpolation. Use -1 to loop indefinitely.");
        private GUIContent ContentUnscaledTime          { get; } = new GUIContent("Use Unscaled Time",           "Whether to use unscaled time, which is unaffected by the timescale");
        private GUIContent ContentDelayUsingEndValue    { get; } = new GUIContent("Use End Value During Delay?", "Whether to use the end value in the interpolation during the initial delay");

        private const string PropertyDurationSeconds       = "_durationSeconds";
        private const string PropertyDelay                 = "_delaySeconds";
        private const string PropertyEasing                = "_easing";
        private const string PropertyLoopMode              = "_loopMode";
        private const string PropertyLoopedDurationSeconds = "_loopedDurationSeconds";
        private const string PropertyUnscaledTime          = "_useUnscaledTime";
        private const string PropertyDelayUsingEndValue    = "_delayUsingEndValue";

        private readonly Material _lineMaterial;

        #endregion
    }
}