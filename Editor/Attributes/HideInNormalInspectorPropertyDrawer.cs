// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HideInNormalInspectorPropertyDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Attributes;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Attributes
{
    /// <summary>
    ///     Custom property drawer for inspector fields that use the HideInNormalInspector attribute.
    ///     From https://answers.unity.com/questions/157775/hide-from-inspector-interface-but-not-from-the-deb.html
    /// </summary>
    [CustomPropertyDrawer(typeof(HideInNormalInspectorAttribute))]
    public class HideInNormalInspectorPropertyDrawer : PropertyDrawer
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
            return 0.0f;
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Even if GetPropertyHeight() returns 0, sometimes it displays a single row of pixels.
            // We avoid this by showing an empty label. 
            EditorGUI.BeginProperty(position, new GUIContent(""), property);
            EditorGUI.EndProperty();
        }

        #endregion

    }
}