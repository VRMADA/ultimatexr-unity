// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManagerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Instantiation;
using UltimateXR.Core.Settings;
using UltimateXR.Core.Unique;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UltimateXR.Editor.Core.Instantiation
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrInstanceManager" />.
    /// </summary>
    [CustomEditor(typeof(UxrInstanceManager))]
    public class UxrInstanceManagerEditor : UnityEditor.Editor
    {
        #region Public Methods

        /// <summary>
        ///     Checks if the prefab list contains only elements with a IUxrUnique component in the root. If not, it will return
        ///     false, keep the serialized property elements to null and return a message to show.
        /// </summary>
        /// <param name="propertyPrefabList">SerializedProperty with a list of prefabs (GameObject)</param>
        /// <param name="message">If the method return false, the message will contain a string to show on screen</param>
        /// <returns>Whether the prefab list was valid (true) or contained one or more invalid elements (false)</returns>
        public static bool ValidatePrefabsWithUniqueId(SerializedProperty propertyPrefabList, out string message)
        {
            List<string> invalidPrefabNames = null;
            message = null;

            for (int i = 0; i < propertyPrefabList.arraySize; ++i)
            {
                GameObject prefab = propertyPrefabList.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;

                if (prefab != null && prefab.GetComponent<IUxrUniqueId>() == null)
                {
                    if (invalidPrefabNames == null)
                    {
                        invalidPrefabNames = new List<string>();
                    }

                    invalidPrefabNames.Add(prefab.name);
                    propertyPrefabList.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
            }

            if (invalidPrefabNames != null)
            {
                int maxEntries = 10;

                if (invalidPrefabNames.Count <= maxEntries)
                {
                    message = $"One or more prefabs don't have a component to track it. Consider adding a {nameof(UxrSyncObject)} to the root of the following prefab(s):\n\n{string.Join("\n", invalidPrefabNames)}";
                }
                else
                {
                    message = $"The list of prefabs contains many elements without a component to track it. Consider adding a {nameof(UxrSyncObject)} on the root to be able to instantiate the prefab using the {nameof(UxrInstanceManager)}.\nInvalid prefabs have been removed from the list.";
                }
            }

            return invalidPrefabNames == null;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyRegisterAutomatically   = serializedObject.FindProperty(PropertyNameRegisterAutomatically);
            _propertyIncludeFrameworkPrefabs = serializedObject.FindProperty(PropertyNameIncludeFrameworkPrefabs);
            _propertyAutomaticPrefabs        = serializedObject.FindProperty(PropertyNameAutomaticPrefabs);
            _propertyUserDefinedPrefabs      = serializedObject.FindProperty(PropertyNameUserDefinedPrefabs);

            if (_propertyAutomaticPrefabs.arraySize == 0 && _propertyRegisterAutomatically.boolValue)
            {
                FindPrefabsAutomatically();
                serializedObject.ApplyModifiedProperties();
            }

            _propertyAutomaticPrefabs.isExpanded = false;
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox($"Use {nameof(UxrInstanceManager)} to instantiate objects at runtime that need to be synchronized on all devices in multiplayer, save-files or replays.\nTo be able to track instantiation, prefabs need at least one component with the {nameof(IUxrUniqueId)} interface on the root, such as any component derived from {nameof(UxrComponent)}. A {nameof(UxrSyncObject)} component can be used when there is no other present.",
                                    MessageType.Info);

            EditorGUILayout.PropertyField(_propertyRegisterAutomatically, ContentRegisterAutomatically);

            if (_propertyRegisterAutomatically.boolValue)
            {
                EditorGUILayout.PropertyField(_propertyIncludeFrameworkPrefabs, ContentIncludeFrameworkPrefabs);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);

                if (GUILayout.Button(ContentFindPrefabs, GUILayout.ExpandWidth(true)))
                {
                    FindPrefabsAutomatically();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_propertyAutomaticPrefabs, ContentAutomaticPrefabs);

                if (!ValidatePrefabsWithUniqueId(_propertyAutomaticPrefabs, out string message))
                {
                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, message, UxrConstants.Editor.Ok);
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Use prefab lists to keep big projects well organized and to avoid long generation times of the automatic list. To create a new prefab list use the Create->UltimateXR menu in the Project Window.", MessageType.Info);
                EditorGUILayout.PropertyField(_propertyUserDefinedPrefabs, ContentUserDefinedPrefabs);                
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to populate the prefab list with all instantiable prefabs in the project.
        /// </summary>
        private void FindPrefabsAutomatically()
        {
            List<GameObject> prefabs = new List<GameObject>();

            Stopwatch sw = Stopwatch.StartNew();

            EditorUtility.DisplayCancelableProgressBar("Finding Prefabs", "Building asset list", 0.0f);

            List<string> paths = AssetDatabase.GetAllAssetPaths().Where(p => p.EndsWith(".prefab")).OrderBy(p => p).ToList();

            for (var i = 0; i < paths.Count; i++)
            {
                string path = paths[i];

                if (!_propertyIncludeFrameworkPrefabs.boolValue && UxrEditorUtils.PathIsInUltimateXR(path))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

                if (prefab != null)
                {
                    EditorUtility.DisplayCancelableProgressBar("Finding Prefabs", prefab.name, paths.Count == 1 ? 1.0f : i / (paths.Count - 1));

                    IUxrUniqueId component = prefab.GetComponent<IUxrUniqueId>();

                    if (component != null)
                    {
                        prefabs.Add(prefab);
                    }
                }
            }

            UxrEditorUtils.AssignSerializedPropertyArray(_propertyAutomaticPrefabs, prefabs);

            EditorUtility.ClearProgressBar();

            if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.CoreModule} Found {prefabs.Count} instantiable prefabs with Unique ID in {sw.ElapsedMilliseconds}ms.");
            }
        }

        #endregion

        #region Private Types & Data

        private static GUIContent ContentRegisterAutomatically   { get; } = new GUIContent("Create List Automatically",             "Create a list automatically of all instantiable prefabs in the project. Use manual lists in big projects to avoid long refresh times.");
        private static GUIContent ContentIncludeFrameworkPrefabs { get; } = new GUIContent($"Include {nameof(UltimateXR)} Prefabs", "Include all instantiable prefabs from the framework in the list too.");
        private static GUIContent ContentFindPrefabs             { get; } = new GUIContent("Regenerate List",                       "Finds all instantiable prefabs in the project.");
        private static GUIContent ContentAutomaticPrefabs        { get; } = new GUIContent("Registered Prefabs",                    "List of prefabs");
        private static GUIContent ContentUserDefinedPrefabs      { get; } = new GUIContent("User Defined Prefab Lists",             "List of prefabs");

        private const string PropertyNameRegisterAutomatically   = "_registerAutomatically";
        private const string PropertyNameIncludeFrameworkPrefabs = "_includeFrameworkPrefabs";
        private const string PropertyNameAutomaticPrefabs        = "_automaticPrefabs";
        private const string PropertyNameUserDefinedPrefabs      = "_userDefinedPrefabs";

        private SerializedProperty _propertyRegisterAutomatically;
        private SerializedProperty _propertyIncludeFrameworkPrefabs;
        private SerializedProperty _propertyAutomaticPrefabs;
        private SerializedProperty _propertyUserDefinedPrefabs;

        #endregion
    }
}