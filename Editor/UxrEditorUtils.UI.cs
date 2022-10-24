// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.UI.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Types & Data

        /// <summary>
        ///     Default <see cref="HandlesAlpha" /> value.
        /// </summary>
        public const float DefaultHandlesAlpha = 0.3f;

        /// <summary>
        ///     Gets or sets the transparency value for handles used in Editor.OnSceneGUI.
        /// </summary>
        public static float HandlesAlpha { get; set; } = DefaultHandlesAlpha;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a stylish foldout editor widget.
        ///     From http://tips.hecomi.com/entry/2016/10/15/004144
        /// </summary>
        /// <param name="title">Title shown</param>
        /// <param name="display">Whether the foldout is expanded or not</param>
        /// <returns></returns>
        public static bool FoldoutStylish(string title, bool display)
        {
            GUIStyle style = new GUIStyle("ShurikenModuleTitle");

            style.font          = new GUIStyle(EditorStyles.label).font;
            style.border        = new RectOffset(15, 7, 4, 4);
            style.fixedHeight   = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        /// <summary>
        ///     Helper editor UI method to draw a centered button.
        /// </summary>
        /// <param name="content">Button text and tooltip</param>
        /// <param name="width">Width in pixels. A negative value will assign the required width for the label</param>
        /// <returns>Whether the button was pressed during the current frame</returns>
        public static bool CenteredButton(GUIContent content, int width = ButtonWidth)
        {
            bool pressed = false;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (width < 0)
            {
                if (GUILayout.Button(content))
                {
                    pressed = true;
                }
            }
            else if (GUILayout.Button(content, GUILayout.Width(width)))
            {
                pressed = true;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return pressed;
        }

        /// <summary>
        ///     Utility to get the rect where a property line needs to be drawn.
        /// </summary>
        /// <param name="position">Position passed to the OnGUI method</param>
        /// <param name="line">Line that needs to be drawn</param>
        /// <param name="includeIndentation">Include indentation in the rect?</param>
        /// <returns>Rect to use the property of the given line</returns>
        public static Rect GetRect(Rect position, int line, bool includeIndentation = false)
        {
            Rect indentRect = includeIndentation ? EditorGUI.IndentedRect(position) : position;
            return new Rect(indentRect.x, indentRect.y + EditorGUIUtility.singleLineHeight * line, indentRect.width, EditorGUIUtility.singleLineHeight);
        }

        /// <summary>
        ///     Utility to get the rect where a property in a horizontal layout needs to be drawn.
        /// </summary>
        /// <param name="position">Position passed to the OnGUI method</param>
        /// <param name="line">Line that needs to be drawn</param>
        /// <param name="totalColumns">Total number of columns in the horizontal line</param>
        /// <param name="column">Column in the horizontal line to get the <see cref="Rect" /> for</param>
        /// <param name="separation">Separation between columns in the horizontal line</param>
        /// <param name="leftPadding">Padding on the left side</param>
        /// <param name="rightPadding">Padding on the right side</param>
        /// <param name="includeIndentation">Include indentation in the rect?</param>
        /// <returns>Rect to use the property of the given line</returns>
        public static Rect GetRect(Rect position, int line, int totalColumns, int column, int separation, int leftPadding = 0, int rightPadding = 0, bool includeIndentation = false)
        {
            Rect indentRect = includeIndentation ? EditorGUI.IndentedRect(position) : position;
            Rect rect       = new Rect(indentRect.x, indentRect.y + EditorGUIUtility.singleLineHeight * line, indentRect.width, EditorGUIUtility.singleLineHeight);

            float elementWidth = (rect.width - leftPadding - rightPadding - (totalColumns - 1) * separation) / totalColumns;
            float posX         = rect.x + leftPadding + column * (separation + elementWidth);

            return new Rect(posX, rect.y, elementWidth, rect.height);
        }

        /// <summary>
        ///     Stores a set of elements in an array serialized property. The elements to store derive from
        ///     <see cref="UnityEngine.Object" />.
        /// </summary>
        /// <typeparam name="T">Type of elements to store</typeparam>
        /// <param name="propertyArray">The <see cref="SerializedProperty" /> to assign</param>
        /// <param name="elements">The elements to store</param>
        public static void AssignSerializedPropertyArray<T>(SerializedProperty propertyArray, IEnumerable<T> elements) where T : Object
        {
            propertyArray.ClearArray();
            propertyArray.arraySize = elements.Count();

            int index = 0;

            foreach (T element in elements)
            {
                propertyArray.GetArrayElementAtIndex(index).objectReferenceValue = element;
                index++;
            }
        }

        /// <summary>
        ///     Stores a set of elements in an array serialized property. The elements to store should be of a simple type (bool,
        ///     int, float, string) or Unity type (Vector3, Color).
        /// </summary>
        /// <typeparam name="T">Type of elements to store</typeparam>
        /// <param name="propertyArray">The <see cref="SerializedProperty" /> to assign</param>
        /// <param name="elements">The elements to store</param>
        public static void AssignSerializedPropertySimpleTypeArray<T>(SerializedProperty propertyArray, IEnumerable<T> elements)
        {
            propertyArray.ClearArray();
            propertyArray.arraySize = elements.Count();

            int index = 0;

            foreach (T element in elements)
            {
                if (element == null)
                {
                    continue;
                }

                SerializedProperty property = propertyArray.GetArrayElementAtIndex(index);

                switch (element)
                {
                    case bool b:
                        property.boolValue = b;
                        break;

                    case int i:
                        property.intValue = i;
                        break;

                    case float f:
                        property.floatValue = f;
                        break;

                    case string s:
                        property.stringValue = s;
                        break;

                    case Color col:
                        property.colorValue = col;
                        break;

                    case Vector3 v3:
                        property.vector3Value = v3;
                        break;

                    default: throw new NotSupportedException($"Conversion to {typeof(T)} serialized property array is not supported yet");
                }

                index++;
            }
        }

        /// <summary>
        ///     Builds a GUIContent array from a set of strings. Some editor UI methods in Unity need GUIContent arrays.
        /// </summary>
        /// <param name="strings">Source strings</param>
        /// <returns>GUIContent array</returns>
        public static GUIContent[] ToGUIContentArray(IEnumerable<string> strings)
        {
            GUIContent[] returnArray = new GUIContent[strings.Count()];

            int i = 0;

            foreach (string str in strings)
            {
                returnArray[i++] = new GUIContent(str);
            }

            return returnArray;
        }

        #endregion
    }
}