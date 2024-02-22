// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncObjectEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Core.Components;
using UltimateXR.Core.StateSave;
using UltimateXR.Networking;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core.Components
{
    [CustomEditor(typeof(UxrSyncObject))]
    public class UxrSyncObjectEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertySyncTransform  = serializedObject.FindProperty(PropertyNameSyncTransform);
            _propertyTransformSpace = serializedObject.FindProperty(PropertyNameTransformSpace);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrSyncObject                 syncObject            = serializedObject.targetObject as UxrSyncObject;
            IUxrStateSave                 stateSaveTransform    = syncObject.GetComponents<IUxrStateSave>().FirstOrDefault(c => c != syncObject && c.RequiresTransformSerialization(UxrStateSaveLevel.ChangesSinceBeginning));
            UxrNetworkComponentReferences networkComponents     = syncObject.GetComponent<UxrNetworkComponentReferences>();
            int                           networkComponentCount = networkComponents != null ? networkComponents.AddedGameObjects.Count + networkComponents.AddedComponents.Count : 0;

            EditorGUILayout.PropertyField(_propertySyncTransform, ContentSyncTransform);

            if (_propertySyncTransform.boolValue)
            {
                EditorGUILayout.PropertyField(_propertyTransformSpace, ContentTransformSpace);
            }

            if (_propertySyncTransform.boolValue && stateSaveTransform != null)
            {
                EditorGUILayout.HelpBox($"The transform is already synced by a {stateSaveTransform.Component.GetType().Name} component on the same GameObject. Consider disabling transform syncing.", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private static GUIContent ContentSyncTransform  { get; } = new GUIContent("Sync Transform", "Synchronizes the transform in multiplayer and save stats.");
        private static GUIContent ContentTransformSpace { get; } = new GUIContent("Space",          "Space that the transform is saved in.");

        private const string PropertyNameSyncTransform  = "_syncTransform";
        private const string PropertyNameTransformSpace = "_transformSpace";

        private SerializedProperty _propertySyncTransform;
        private SerializedProperty _propertyTransformSpace;

        #endregion
    }
}