// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     Unity <see cref="UnityEngine.Object" /> extensions
    /// </summary>
    public static class ObjectExt
    {
        #region Public Methods

#if UNITY_EDITOR
        /// <summary>
        ///     Assigns a serialized property value if code that only executes in the editor.
        /// </summary>
        /// <param name="self">The object (GameObject or component) with the serialized property</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="assigner">Action that gets the serialized property as argument and enables to assign any value</param>
        /// <example>
        ///     <code>
        ///         component.AssignSerializedProperty("_myBoolVar", p => p.boolValue = true);
        ///     </code>
        /// </example>
        public static void AssignSerializedProperty(this Object self, string propertyName, Action<SerializedProperty> assigner)
        {
            SerializedObject   serializedObject   = new SerializedObject(self);
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
            
            if (serializedProperty == null)
            {
                Debug.LogError($"{nameof(AssignSerializedProperty)}(): Cannot find property {propertyName}");
                return;
            }
            
            assigner.Invoke(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }
#endif

        /// <summary>
        ///     Controls whether to show a given object in the inspector.
        /// </summary>
        /// <param name="self">The object to show</param>
        /// <param name="show">Whether to show the object or now</param>
        public static void ShowInInspector(this Object self, bool show = true)
        {
            if (show)
            {
                self.hideFlags &= ~HideFlags.HideInInspector;
            }
            else
            {
                self.hideFlags |= HideFlags.HideInInspector;
            }
        }

        /// <summary>
        ///     Controls whether to show a given object in the inspector and whether it is editable.
        /// </summary>
        /// <param name="self">The object to set</param>
        /// <param name="show">Whether to show it in the inspector</param>
        /// <param name="editable">Whether it is editable</param>
        public static void ShowInInspector(this Object self, bool show, bool editable)
        {
            if (show)
            {
                self.hideFlags &= ~HideFlags.HideInInspector;
            }
            else
            {
                self.hideFlags |= HideFlags.HideInInspector;
            }

            if (editable)
            {
                self.hideFlags &= ~HideFlags.NotEditable;
            }
            else
            {
                self.hideFlags |= HideFlags.NotEditable;
            }
        }

        #endregion
    }
}