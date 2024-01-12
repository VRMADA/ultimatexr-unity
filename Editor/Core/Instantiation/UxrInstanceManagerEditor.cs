// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManagerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Instantiation;
using UltimateXR.Manipulation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core.Instantiation
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrInstanceManager" />.
    /// </summary>
    [CustomEditor(typeof(UxrInstanceManager))]
    public class UxrInstanceManagerEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyRegisteredPrefabs = serializedObject.FindProperty(PropertyNameRegisteredPrefabs);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox($"The instance manager allows to synchronize object instantiation across different computers.\nUse the {nameof(UxrInstanceManager)} singleton to instantiate objects at runtime that need to be synchronized.", MessageType.Info);

            EditorGUILayout.PropertyField(_propertyRegisteredPrefabs);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        private static GUIContent ContentRegisteredPrefabs { get; } = new GUIContent("Registered Prefabs", $"Removes added network components from the selected {nameof(UxrGrabbableObject)} objects that have a rigidbody");

        private const string PropertyNameRegisteredPrefabs = "_registeredPrefabs";

        private SerializedProperty _propertyRegisteredPrefabs;

    }
}