// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAxisPropertyDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Editor;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Core.Math.Editor
{
    /// <summary>
    ///     Custom property drawer for <see cref="UxrAxis" />.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrAxis))]
    public class UxrAxisPropertyDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Draws an axis popup selector using <see cref="EditorGUILayout" />.
        /// </summary>
        /// <param name="content">Label and tooltip</param>
        /// <param name="axis">Axis value</param>
        /// <returns>New axis value</returns>
        public static UxrAxis EditorGuiLayout(GUIContent content, UxrAxis axis)
        {
            return EditorGUILayout.Popup(content, (int)axis, AxesAsStrings);
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty propertyAxis = property.FindPropertyRelative("_axis");

            propertyAxis.intValue = EditorGUI.Popup(UxrEditorUtils.GetRect(position, 0), property.displayName, propertyAxis.intValue, AxesAsStrings);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the possible axis values as strings.
        /// </summary>
        private static string[] AxesAsStrings { get; } = { "X", "Y", "Z" };

        #endregion
    }
}