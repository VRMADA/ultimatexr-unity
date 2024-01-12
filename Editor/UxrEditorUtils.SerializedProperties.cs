// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.SerializedProperties.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Types & Data

        /// <summary>
        ///     Name of the <see cref="UxrComponent" /> field that stores the unique ID, to be used by serialized properties.
        /// </summary>
        public const string PropertyUniqueId = "_uxrUniqueId";
        /// <summary>
        ///     Name of the <see cref="UxrComponent" /> field that stores the prefab Guid.
        /// </summary>
        public const string PropertyPrefabGuid = "__prefabGuid";
        /// <summary>
        ///     Name of the <see cref="UxrComponent" /> field that stores whether the component is stored in a prefab.
        /// </summary>
        public const string PropertyIsInPrefab = "__isInPrefab";

        #endregion

        #region Public Methods

        /// <summary>
        ///     Assigns a serialized property value.
        /// </summary>
        /// <param name="obj">The object (GameObject or component) with the serialized property</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="assigner">Action that gets the serialized property as argument and enables to assign any value</param>
        /// <example>
        ///     <code>
        ///         UxrEditorUtils.AssignSerializedProperty(component, "_myBoolVar", p => p.boolValue = true);
        ///     </code>
        /// </example>
        public static void AssignSerializedProperty(Object obj, string propertyName, Action<SerializedProperty> assigner)
        {
            SerializedObject   serializedObject   = new SerializedObject(obj);
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);

            if (serializedProperty == null)
            {
                Debug.LogError($"{nameof(AssignSerializedProperty)}(): Cannot find property {propertyName}");
                return;
            }

            assigner.Invoke(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     Appends a new element in a serialized array and optionally allows to assign its new value.
        /// </summary>
        /// <param name="arrayProperty">Serialized array property</param>
        /// <param name="assignAction">
        ///     An optional assign action that will receive the new serialized property appended as
        ///     parameter
        /// </param>
        public static void AppendSerializedArrayElement(SerializedProperty arrayProperty, Action<SerializedProperty> assignAction = null)
        {
            int arraySize = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(arraySize);
            assignAction?.Invoke(arrayProperty.GetArrayElementAtIndex(arraySize));
        }

        /// <summary>
        ///     Gets a serialized array as an IEnumerable.
        /// </summary>
        /// <param name="arrayProperty">Serialized array property</param>
        /// <param name="assigner">Function that given the array element as a serialized property returns the given target object</param>
        /// <example>
        ///     <code>
        ///         IEnumerable&lt;string&gt; scenePaths = UxrEditorUtils.GetSerializedArrayAsEnumerable(propertyScenePathsArray, p => p.stringValue);
        ///     </code>
        /// </example>
        public static IEnumerable<T> GetSerializedArrayAsEnumerable<T>(SerializedProperty arrayProperty, Func<SerializedProperty, T> assigner)
        {
            for (int i = 0; i < arrayProperty.arraySize; ++i)
            {
                T element = assigner(arrayProperty.GetArrayElementAtIndex(i));
                yield return element;
            }
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

        #endregion
    }
}