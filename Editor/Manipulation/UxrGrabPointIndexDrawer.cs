// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabPointIndexDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Manipulation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation
{
    /// <summary>
    ///     Custom property drawer for the <see cref="UxrGrabPointInfo" /> class.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrGrabPointIndex))]
    public class UxrGrabPointIndexDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UxrGrabbableObject grabbableObject        = ((MonoBehaviour)property.serializedObject.targetObject).GetComponent<UxrGrabbableObject>();
            SerializedProperty propertyGrabPointIndex = property.FindPropertyRelative("_index");

            List<string> elements = new List<string>();

            for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
            {
                elements.Add(UxrGrabPointIndex.GetIndexDisplayName(grabbableObject, i));
            }

            propertyGrabPointIndex.intValue = EditorGUI.Popup(UxrEditorUtils.GetRect(position, 0), property.displayName, propertyGrabPointIndex.intValue, elements.ToArray());
        }

        #endregion
    }
}