// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPrefabListEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Instantiation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core.Instantiation
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrPrefabList" />.
    /// </summary>
    [CustomEditor(typeof(UxrPrefabList))]
    public class UxrGlobalSettingsEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propPrefabList = serializedObject.FindProperty(PropertyPrefabList);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propPrefabList, ContentPrefabList);

            if (!UxrInstanceManagerEditor.ValidatePrefabsWithUniqueId(_propPrefabList, out string message))
            {
                EditorUtility.DisplayDialog(UxrConstants.Editor.Error, message, UxrConstants.Editor.Ok);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentPrefabList { get; } = new GUIContent("Prefab List", "List of prefabs");

        private const string PropertyPrefabList = "_prefabList";

        private SerializedProperty _propPrefabList;

        #endregion
    }
}