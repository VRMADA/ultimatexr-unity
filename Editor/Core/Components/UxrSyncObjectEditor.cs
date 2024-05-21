// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncObjectEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Core.Components;
using UltimateXR.Core.StateSave;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core.Components
{
    [CustomEditor(typeof(UxrSyncObject))]
    [CanEditMultipleObjects]
    public class UxrSyncObjectEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertySyncTransform        = serializedObject.FindProperty(PropertyNameSyncTransform);
            _propertyTransformSpace       = serializedObject.FindProperty(PropertyNameTransformSpace);
            _propertySyncActiveAndEnabled = serializedObject.FindProperty(PropertyNameSyncActiveAndEnabled);
            _propertySyncWhileDisabled    = serializedObject.FindProperty(PropertyNameSyncWhileDisabled);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propertySyncTransform, ContentSyncTransform);

            if (_propertySyncTransform.boolValue)
            {
                EditorGUILayout.PropertyField(_propertyTransformSpace, ContentTransformSpace);
            }

            foreach (Object selectedObject in targets)
            {
                UxrSyncObject syncObject         = selectedObject as UxrSyncObject;
                IUxrStateSave stateSaveTransform = syncObject.GetComponents<IUxrStateSave>().FirstOrDefault(c => c != syncObject && c.RequiresTransformSerialization(UxrStateSaveLevel.ChangesSinceBeginning));

                if (syncObject.SyncTransform && stateSaveTransform != null)
                {
                    if (targets.Length > 1)
                    {
                        EditorGUILayout.HelpBox($"The transform in {syncObject.name} is already synced by a {stateSaveTransform.Component.GetType().Name} component on the same GameObject. Consider disabling transform syncing.", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"The transform is already synced by a {stateSaveTransform.Component.GetType().Name} component on the same GameObject. Consider disabling transform syncing.", MessageType.Error);
                    }
                }
            }

            EditorGUILayout.PropertyField(_propertySyncActiveAndEnabled, ContentSyncActiveAndEnabled);
            EditorGUILayout.PropertyField(_propertySyncWhileDisabled,    ContentSyncWhileDisabled);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private static GUIContent ContentSyncTransform        { get; } = new GUIContent("Sync Transform",      "Synchronizes the transform in multiplayer and save stats.");
        private static GUIContent ContentTransformSpace       { get; } = new GUIContent("Space",               "Space that the transform is saved in.");
        private static GUIContent ContentSyncActiveAndEnabled { get; } = new GUIContent("Sync Active/Enabled", "Synchronizes the GameObject's active state and the component's enabled state.");
        private static GUIContent ContentSyncWhileDisabled    { get; } = new GUIContent("Sync While Disabled", "Synchronizes even while the Component/GameObject is disabled.");

        private const string PropertyNameSyncTransform        = "_syncTransform";
        private const string PropertyNameTransformSpace       = "_transformSpace";
        private const string PropertyNameSyncActiveAndEnabled = "_syncActiveAndEnabled";
        private const string PropertyNameSyncWhileDisabled    = "_syncWhileDisabled";

        private SerializedProperty _propertySyncTransform;
        private SerializedProperty _propertyTransformSpace;
        private SerializedProperty _propertySyncActiveAndEnabled;
        private SerializedProperty _propertySyncWhileDisabled;

        #endregion
    }
}