// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlyPropertyDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Attributes;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Attributes
{
    /// <summary>
    ///     Custom property drawer for inspector fields that use the ReadOnly attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <summary>
        ///     Called when the GUI wants to know the height needed to draw the property.
        /// </summary>
        /// <param name="property">SerializedProperty that needs to be drawn</param>
        /// <param name="label">Label used</param>
        /// <returns>Height in pixels</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var roAttr = (ReadOnlyAttribute)attribute;
            return roAttr.HideInEditMode && !Application.isPlaying ? 0 : EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called when the GUI needs to draw the property.
        /// </summary>
        /// <param name="position">GUI position</param>
        /// <param name="property">Property to draw</param>
        /// <param name="label">Property label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var roAttr = (ReadOnlyAttribute)attribute;

            if (roAttr.HideInEditMode && !Application.isPlaying)
            {
                return;
            }

            GUI.enabled = !Application.isPlaying && roAttr.OnlyWhilePlaying;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }

        #endregion
    }
}