// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkManagerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Instantiation;
using UltimateXR.Core.Settings;
using UltimateXR.Editor.Avatar;
using UltimateXR.Editor.Core;
using UltimateXR.Editor.Sdks;
using UltimateXR.Editor.Utilities;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UltimateXR.Networking;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Networking
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrNetworkManager" />.
    /// </summary>
    [CustomEditor(typeof(UxrNetworkManager))]
    public partial class UxrNetworkManagerEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _networkManager = serializedObject.targetObject as UxrNetworkManager;

            // Register networking implementations

            RegisterNetworkingSystems();
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("The network manager automatically gives multi-user capabilities to the application using any of the available systems. Add it to the first scene in the project.", MessageType.Info);

            IEnumerable<UxrAvatar> sceneMultiplayerAvatars = FindObjectsOfType<UxrAvatar>().Where(a => a.GetComponent<IUxrNetworkAvatar>() != null);

            if (sceneMultiplayerAvatars.Any())
            {
                EditorGUILayout.HelpBox($"In multiplayer sessions, avatar prefabs will be spawned at runtime instead of being pre-instantiated in the scene. Consider removing the following multiplayer avatars from the scene: {string.Join(", ", sceneMultiplayerAvatars.Select(a => a.name))}", MessageType.Warning);
            }
            
            // Initialize

            _networkImplementation      = PropNetworkImplementation.objectReferenceValue as UxrNetworkImplementation;
            _networkVoiceImplementation = PropNetworkVoiceImplementation.objectReferenceValue as UxrNetworkVoiceImplementation;
            _networkingIndex            = _networkImplementation ? GetAvailableNetworkSdks().IndexOf(_networkImplementation.SdkName) : 0;
            _networkingVoiceIndex       = _networkVoiceImplementation ? GetAvailableNetworkVoiceSdks(_networkImplementation).IndexOf(_networkVoiceImplementation.SdkName) : 0;

            // Networking configuration

            _showNetworking = UxrEditorUtils.FoldoutStylish("SDKs", _showNetworking);

            if (_showNetworking)
            {
                GUI.enabled = _networkImplementations.Count > 0;

                List<string> availableNetworkSdks = GetAvailableNetworkSdks();

                int newNetworkIndex      = _networkingIndex;
                int newNetworkVoiceIndex = _networkingVoiceIndex;

                // List Networking SDKs

                EditorGUI.BeginChangeCheck();
                newNetworkIndex = EditorGUILayout.Popup(ContentNetworkSystem, _networkingIndex, UxrEditorUtils.ToGUIContentArray(availableNetworkSdks));
                if (EditorGUI.EndChangeCheck())
                {
                    // Reset voice
                    newNetworkVoiceIndex = -1;
                }

                // List Networking Voice SDKs

                GUI.enabled = true;
                List<string> availableNetworkVoiceSdks = GetAvailableNetworkVoiceSdks(_networkImplementation);

                if (newNetworkVoiceIndex == -1)
                {
                    newNetworkVoiceIndex = 0;
                }
                else if ((_networkingIndex > 0 && _networkImplementations != null && _networkImplementations.ContainsKey(availableNetworkSdks[_networkingIndex])))
                {
                    IUxrNetworkImplementation networkImplementation = _networkImplementations[availableNetworkSdks[_networkingIndex]];

                    // List voice SDKs only if current networking SDK doesn't support voice communication.

                    if (!networkImplementation.Capabilities.HasFlag(UxrNetworkCapabilities.Voice))
                    {
                        GUI.enabled          = _networkImplementations.Count > 0;
                        newNetworkVoiceIndex = EditorGUILayout.Popup(ContentNetworkVoiceSystem, _networkingVoiceIndex, UxrEditorUtils.ToGUIContentArray(GetAvailableNetworkVoiceSdks(_networkImplementation)));
                        GUI.enabled          = true;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(PropUseSameSdkVoice, ContentUseSameSdkVoice);
                    }
                }

                // Now change voice if needed. We change voice first because some components might be dependent on the networking SDK.

                if (newNetworkVoiceIndex != _networkingVoiceIndex)
                {
                    _networkingVoiceIndex = newNetworkVoiceIndex;
                    UxrNetworkVoiceImplementation oldImplementation = _networkVoiceImplementation;

                    _networkVoiceImplementation = null;
                    _networkVoiceImplementations.TryGetValue(availableNetworkVoiceSdks[_networkingVoiceIndex], out _networkVoiceImplementation);

                    SetupGlobal(_networkVoiceImplementation);
                    SetupAvatars(_networkVoiceImplementation);

                    // Disable old
                    if (oldImplementation != null)
                    {
                        oldImplementation.ShowInInspector(false);
                        oldImplementation.SetEnabled(false);
                    }

                    // Enable new
                    if (_networkVoiceImplementation != null)
                    {
                        _networkVoiceImplementation.ShowInInspector();
                        _networkVoiceImplementation.SetEnabled(true);
                    }

                    if (_networkVoiceImplementation != null)
                    {
                        Undo.SetCurrentGroupName($"Switch to {_networkVoiceImplementation.SdkName}");
                    }
                    else
                    {
                        Undo.SetCurrentGroupName($"Switch to {NoSdk}");
                    }
                }

                // Now change networking SDK.

                if (newNetworkIndex != _networkingIndex)
                {
                    _networkingIndex = newNetworkIndex;

                    UxrNetworkImplementation oldImplementation = _networkImplementation;

                    _networkImplementation = null;
                    _networkImplementations.TryGetValue(availableNetworkSdks[_networkingIndex], out _networkImplementation);

                    SetupGlobal(_networkImplementation);
                    SetupAvatars(_networkImplementation);
                    SetupPostProcess(_networkImplementation);

                    // Disable old
                    if (oldImplementation != null)
                    {
                        oldImplementation.ShowInInspector(false);
                        oldImplementation.SetEnabled(false);
                    }

                    // Enable new
                    if (_networkImplementation != null)
                    {
                        _networkImplementation.ShowInInspector();
                        _networkImplementation.SetEnabled(true);
                    }

                    if (_networkImplementation != null)
                    {
                        Undo.SetCurrentGroupName($"Switch to {_networkImplementation.SdkName}");
                    }
                    else
                    {
                        Undo.SetCurrentGroupName($"Switch to {NoSdk}");
                    }
                }

                // Show messages if no SDKs are installed

                if (_networkImplementations.Count == 0)
                {
                    EditorGUILayout.HelpBox("To use networking you need to install any of the supported systems. Check the SDK Manager for setup:", MessageType.Info);

                    if (UxrEditorUtils.CenteredButton(ContentOpenSdkManagerWindow))
                    {
                        UxrSdkManagerWindow.ShowWindow(UxrSdkLocator.SupportType.Networking);
                    }
                }
                else if (_networkVoiceImplementations.Count == 0 && _networkImplementation != null)
                {
                    EditorGUILayout.HelpBox("To use voice transmission you need to install any of the supported systems. Check the SDK Manager for setup:", MessageType.Info);

                    if (UxrEditorUtils.CenteredButton(ContentOpenSdkManagerWindow))
                    {
                        UxrSdkManagerWindow.ShowWindow(UxrSdkLocator.SupportType.Networking);
                    }
                }
                
                // If it's not installed, show status

                if (_networkingIndex > 0)
                {
                    UxrSdkLocator locator = UxrSdkManager.SDKLocators.FirstOrDefault(l => l.Support == UxrSdkLocator.SupportType.Networking &&
                                                                                          l.Name == availableNetworkSdks[_networkingIndex] &&
                                                                                          (l.CurrentState == UxrSdkLocator.State.Available || l.CurrentState == UxrSdkLocator.State.CurrentTargetNotSupported));

                    if (locator == null)
                    {
                        EditorGUILayout.HelpBox($"{availableNetworkSdks[_networkingIndex]} is not installed. Use the SDK Manager to review the networking SDKs:", MessageType.Warning);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUIUtility.labelWidth);

                        if (GUILayout.Button(ContentOpenSdkManagerWindow))
                        {
                            UxrSdkManagerWindow.ShowWindow(UxrSdkLocator.SupportType.Networking);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (_networkingVoiceIndex > 0)
                {
                    UxrSdkLocator locator = UxrSdkManager.SDKLocators.FirstOrDefault(l => l.Support == UxrSdkLocator.SupportType.VoiceOverNetwork &&
                                                                                          l.Name == availableNetworkVoiceSdks[_networkingVoiceIndex] &&
                                                                                          (l.CurrentState == UxrSdkLocator.State.Available || l.CurrentState == UxrSdkLocator.State.CurrentTargetNotSupported));

                    if (locator == null)
                    {
                        EditorGUILayout.HelpBox($"{availableNetworkVoiceSdks[_networkingVoiceIndex]} is not installed. Use the SDK Manager to review the networking SDKs:", MessageType.Warning);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUIUtility.labelWidth);

                        if (GUILayout.Button(ContentOpenSdkManagerWindow))
                        {
                            UxrSdkManagerWindow.ShowWindow(UxrSdkLocator.SupportType.VoiceOverNetwork);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                // Show stats

                if (PropCreatedGlobalGameObjects.arraySize > 0 || PropCreatedGlobalComponents.arraySize > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);

                    if (GUILayout.Button(ContentShowGlobalInfo))
                    {
                        IEnumerable<string> logLines = UxrEditorUtils.GetSerializedArrayAsEnumerable(PropCreatedGlobalGameObjects, p => GetLogLine(p.objectReferenceValue as GameObject))
                                                                     .Concat(UxrEditorUtils.GetSerializedArrayAsEnumerable(PropCreatedGlobalComponents,       p => GetLogLine(p.objectReferenceValue as Component)))
                                                                     .Concat(UxrEditorUtils.GetSerializedArrayAsEnumerable(PropCreatedGlobalVoiceGameObjects, p => GetLogLine(p.objectReferenceValue as GameObject)))
                                                                     .Concat(UxrEditorUtils.GetSerializedArrayAsEnumerable(PropCreatedGlobalVoiceComponents,  p => GetLogLine(p.objectReferenceValue as Component)));
                        UxrLogWindow.ShowLog(logLines);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            // Avatar setup

            _showAvatars = UxrEditorUtils.FoldoutStylish("Avatar setup", _showAvatars);

            if (_showAvatars)
            {
                EditorGUILayout.HelpBox("Register avatars to let UltimateXR add all required networking components for the selected system. Remove them to remove all added components.", MessageType.Info);

                UxrAvatar avatar = EditorGUILayout.ObjectField(ContentSetupAvatar, null, typeof(UxrAvatar), true) as UxrAvatar;

                if (avatar != null)
                {
                    // Avatar was selected. Get its prefab.

                    UxrAvatar avatarPrefab = avatar.GetAvatarPrefab();

                    if (avatarPrefab == null)
                    {
                        // No prefab. Allow user to create prefab automatically.
                        if (EditorUtility.DisplayDialog("Avatar has no prefab", "The given avatar doesn't have a source prefab. A prefab is required to register the avatar. Do you want to create one now?", UxrConstants.Editor.Yes, UxrConstants.Editor.Cancel))
                        {
                            if (UxrEditorUtils.CreateAvatarPrefab(avatar, "Save prefab", avatar.name, out GameObject prefab, out GameObject newInstance))
                            {
                                if (newInstance == null)
                                {
                                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "The prefab variant was created but it could not be instantiated in the scene. Try doing it manually.", UxrConstants.Editor.Ok);
                                }
                                else if (newInstance != null)
                                {
                                    avatar = newInstance.GetComponent<UxrAvatar>();
                                    CheckAvatarHasPrefabGuid(avatar, prefab);
                                    avatarPrefab = avatar.GetAvatarPrefab();
                                }
                            }
                        }
                    }
                    else if (!SceneIsInUltimateXR && UxrEditorUtils.PathIsInUltimateXR(AssetDatabase.GetAssetPath(avatarPrefab)))
                    {
                        // Prefab was found, but is from UltimateXR. Allow user to create prefab variant automatically.
                        if (EditorUtility.DisplayDialog("Avatar requires variant", "To avoid losing progress when UltimateXR is updated, it is recommended to create a prefab variant instead of using an UltimateXR prefab directly. Do you want to create one now?", UxrConstants.Editor.Yes, UxrConstants.Editor.Cancel))
                        {
                            if (UxrEditorUtils.CreateAvatarPrefab(avatar, "Save prefab variant", avatar.gameObject.name + "Variant", out GameObject prefab, out GameObject newInstance))
                            {
                                if (newInstance == null)
                                {
                                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "The prefab variant was created but it could not be instantiated in the scene. Try doing it manually.", UxrConstants.Editor.Ok);
                                }
                                else
                                {
                                    avatar = newInstance.GetComponent<UxrAvatar>();
                                    CheckAvatarHasPrefabGuid(avatar, prefab);
                                    avatarPrefab = avatar.GetAvatarPrefab();
                                }
                            }
                        }
                    }

                    // Register prefab if we have one.

                    if (avatarPrefab != null)
                    {
                        // First check if it was already registered

                        bool found = false;

                        for (int i = 0; i < PropRegisteredAvatars.arraySize; ++i)
                        {
                            UxrAvatar avatarPrefabEntry = PropRegisteredAvatars.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;

                            if (ReferenceEquals(avatarPrefabEntry, avatarPrefab))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            EditorUtility.DisplayDialog("Avatar already registered", $"Avatar prefab {avatarPrefab.name} is already registered", UxrConstants.Editor.Ok);
                        }
                        else
                        {
                            // Register
                            int newAvatarIndex = PropRegisteredAvatars.arraySize;
                            PropRegisteredAvatars.InsertArrayElementAtIndex(newAvatarIndex);
                            PropRegisteredAvatars.GetArrayElementAtIndex(newAvatarIndex).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue = avatarPrefab;

                            SetupRegisteredAvatar(newAvatarIndex, _networkImplementation);
                            SetupRegisteredAvatar(newAvatarIndex, _networkVoiceImplementation);
                            AssetDatabase.SaveAssets();

                            Undo.SetCurrentGroupName($"Register {avatarPrefab.name}");
                        }
                    }
                }

                // Refresh registered avatar prefab list if necessary
                SetupPostProcess(_networkImplementation);

                // Show registered avatar prefabs

                if (PropRegisteredAvatars.arraySize == 0)
                {
                    GUILayout.Label("No registered avatars");
                }
                else
                {
                    GUILayout.Label("Registered avatars:");

                    EditorGUI.indentLevel++;

                    for (int i = 0; i < PropRegisteredAvatars.arraySize; ++i)
                    {
                        UxrAvatar avatarPrefabEntry = PropRegisteredAvatars.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;

                        if (avatarPrefabEntry == null)
                        {
                            continue;
                        }

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField($"{avatarPrefabEntry.name}");
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(ContentShowAvatarInfo))
                        {
                            UxrAvatar                       avatarPrefab              = PropRegisteredAvatars.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;
                            UxrNetworkComponentReferences[] avatarComponentReferences = avatarPrefab.GetComponentsInChildren<UxrNetworkComponentReferences>(true);
                            IEnumerable<GameObject>         addedGameObjects          = avatarComponentReferences.SelectMany(r => r.AddedGameObjects.Where(g => g != null));
                            IEnumerable<Component>          addedComponents           = avatarComponentReferences.SelectMany(r => r.AddedComponents.Where(c => c != null));

                            if (addedGameObjects.Any() || addedComponents.Any())
                            {
                                UxrLogWindow.ShowLog(addedGameObjects.Select(GetLogLine).Concat(addedComponents.Select(GetLogLine)));
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Avatar network components", "Avatar has no networking components added", UxrConstants.Editor.Ok);
                            }
                        }

                        if (GUILayout.Button(ContentSelectAvatar))
                        {
                            Selection.activeObject = avatarPrefabEntry.gameObject;
                        }

                        if (GUILayout.Button(ContentRemoveAvatar))
                        {
                            SetupRegisteredAvatar(i, (IUxrNetworkImplementation)null);
                            SetupRegisteredAvatar(i, (IUxrNetworkVoiceImplementation)null);
                            PropRegisteredAvatars.DeleteArrayElementAtIndex(i);
                            i--;

                            Undo.SetCurrentGroupName($"Remove {avatarPrefabEntry.name}");
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }
            }

            // Physics-driven grabbable objects setup

            _showPhysicsGrabbables = UxrEditorUtils.FoldoutStylish("NetworkRigidbody for grabbable objects with a rigidbody", _showPhysicsGrabbables);
            string statsMessage = null;

            if (_showPhysicsGrabbables)
            {
                if (_networkImplementation == null || _networkImplementation.Capabilities.HasFlag(UxrNetworkCapabilities.NetworkRigidbody))
                {
                    EditorGUILayout.HelpBox($"Grabbable objects (using {nameof(UxrGrabbableObject)}) are already synchronized automatically using UltimateXR. If they have a rigidbody, however, they can benefit from an additional setup to enable exact physics synchronization on all network clients. Use this section to let UltimateXR set up automatically all physics synchronization components for the selected system.\nAll changes will be tracked so that they can be replaced or removed later.",
                                            MessageType.Info);
                }

                if (_networkImplementation != null && !string.IsNullOrEmpty(_networkImplementation.NetworkRigidbodyWarning))
                {
                    EditorGUILayout.HelpBox(_networkImplementation.NetworkRigidbodyWarning, MessageType.Warning);
                }

                bool supportsNetworkRigidbodies = _networkImplementation != null && _networkImplementation.Capabilities.HasFlag(UxrNetworkCapabilities.NetworkRigidbody);

                GUI.enabled = supportsNetworkRigidbodies;

                EditorGUILayout.PropertyField(PropPhysicsAddProjectScenes, ContentPhysicsAddProjectScenes);
                EditorGUILayout.PropertyField(PropPhysicsAddPathPrefabs,   ContentPhysicsAddPathPrefabs);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(PropPhysicsAddPathRoot, ContentPhysicsAddPathRoot);

                if (GUILayout.Button(ContentChooseFolder, GUILayout.ExpandWidth(false)) && UxrEditorUtils.OpenFolderPanel(out string path))
                {
                    PropPhysicsAddPathRoot.stringValue = path;
                }

                EditorGUILayout.EndHorizontal();

                string     currentComponentsSdk = PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoSdkUsed).stringValue;
                bool       hasComponents        = !string.IsNullOrEmpty(currentComponentsSdk);
                GUIContent setupButtonContent   = hasComponents ? ContentPhysicsReplace : ContentPhysicsAdd;

                GUI.enabled = supportsNetworkRigidbodies || hasComponents;
                EditorGUILayout.PropertyField(PropPhysicsOnlyLog, ContentPhysicsOnlyLog);

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = supportsNetworkRigidbodies && (PropPhysicsAddProjectScenes.boolValue || PropPhysicsAddPathPrefabs.boolValue);

                if (GUILayout.Button(setupButtonContent))
                {
                    RemoveGrabbablePhysicsComponents(PropPhysicsOnlyLog.boolValue);

                    IEnumerable<string> enabledScenePaths = EditorBuildSettings.scenes.Where(s => !string.IsNullOrEmpty(s.path) && s.enabled).Select(s => s.path);

                    if (!PropPhysicsOnlyLog.boolValue)
                    {
                        PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoSdkUsed).stringValue            = _networkImplementation.SdkName;
                        PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedPathPrefabs).boolValue = PropPhysicsAddPathPrefabs.boolValue;
                        PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedRootPath).stringValue  = PropPhysicsAddPathRoot.stringValue;

                        if (PropPhysicsAddProjectScenes.boolValue)
                        {
                            UxrEditorUtils.AssignSerializedPropertySimpleTypeArray(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedScenePaths), enabledScenePaths);
                        }
                    }

                    IEnumerable<string> scenePaths = PropPhysicsAddProjectScenes.boolValue ? enabledScenePaths : Enumerable.Empty<string>();

                    _lastGrabbableObjectAdditionStats            = new GrabbableObjectStats();
                    _lastGrabbableObjectAdditionStats.SceneCount = scenePaths.Count();

                    UxrEditorUtils.ProcessScenesAndProjectPathPrefabs<UxrGrabbableObject>(scenePaths,
                                                                                          PropPhysicsAddPathPrefabs.boolValue,
                                                                                          PropPhysicsAddPathRoot.stringValue,
                                                                                          AddGrabbableComponentProcessor,
                                                                                          progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                                                          out bool _,
                                                                                          !SceneIsInUltimateXR,
                                                                                          PropPhysicsOnlyLog.boolValue);

                    EditorUtility.ClearProgressBar();
                    AssetDatabase.SaveAssets();

                    if (!PropPhysicsOnlyLog.boolValue)
                    {
                        statsMessage = GetStatsMessage(_lastGrabbableObjectAdditionStats, _lastGrabbableObjectRemovalStats);
                    }
                    else
                    {
                        string action = hasComponents ? "replaced" : "added";
                        statsMessage = $"Information has been written to the console to see which components would be {action}";
                    }

                    Undo.SetCurrentGroupName($"{setupButtonContent.text}");
                }

                GUI.enabled = hasComponents;

                if (GUILayout.Button(ContentPhysicsRemove))
                {
                    RemoveGrabbablePhysicsComponents(PropPhysicsOnlyLog.boolValue);

                    if (!PropPhysicsOnlyLog.boolValue)
                    {
                        statsMessage = GetStatsMessage(null, _lastGrabbableObjectRemovalStats);
                    }
                    else
                    {
                        statsMessage = "Information has been written to the console to see which components would be removed";
                    }

                    Undo.SetCurrentGroupName($"{ContentPhysicsRemove.text}");
                }

                if (GUILayout.Button(ContentPhysicsShowCurrentSetup))
                {
                    List<string>       logLines         = new List<string>();
                    SerializedProperty propertyLogLines = PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoDebugInfoLines);

                    for (int i = 0; i < propertyLogLines.arraySize; ++i)
                    {
                        logLines.Add(propertyLogLines.GetArrayElementAtIndex(i).stringValue);
                    }

                    UxrLogWindow.ShowLog(logLines);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                bool componentsOutOfSync = _networkImplementation != null && !string.IsNullOrEmpty(currentComponentsSdk) && !string.IsNullOrEmpty(_networkImplementation.SdkName) && _networkImplementation.SdkName != currentComponentsSdk;

                if (_networkingIndex == 0 && hasComponents)
                {
                    EditorGUILayout.HelpBox($"No networking is currently selected but there are components from {currentComponentsSdk} SDK still added. Consider removing them if they are not going to be used.", MessageType.Warning);
                }
                else if (componentsOutOfSync)
                {
                    EditorGUILayout.HelpBox($"{_networkImplementation.SdkName} is currently selected but there are components from {currentComponentsSdk} still added. Click on {ContentPhysicsReplace.text} to replace the old components by new ones from {_networkImplementation.SdkName} or on {ContentPhysicsRemove} to remove them.", MessageType.Warning);
                }
            }

            PropNetworkVoiceImplementation.objectReferenceValue = _networkVoiceImplementation;
            PropNetworkImplementation.objectReferenceValue      = _networkImplementation;

            serializedObject.ApplyModifiedProperties();

            if (!string.IsNullOrEmpty(statsMessage))
            {
                EditorUtility.DisplayDialog("Stats", statsMessage, UxrConstants.Editor.Ok);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Instantiates a <see cref="UxrNetworkManager" /> in the scene.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathNetworking + "Create Network Manager", priority = UxrConstants.Editor.PriorityMenuPathNetworking)]
        private static void InstantiateManagerInScene()
        {
            UxrNetworkManager  existingManager         = FindObjectOfType<UxrNetworkManager>();
            UxrInstanceManager existingInstanceManager = FindObjectOfType<UxrInstanceManager>();

            if (existingManager != null)
            {
                if (EditorUtility.DisplayDialog("Manager already exists", $"{nameof(UxrNetworkManager)} already exists in the scene. Press {UxrConstants.Editor.Ok} to select it", UxrConstants.Editor.Ok, UxrConstants.Editor.Cancel))
                {
                    Selection.activeGameObject = existingManager.gameObject;
                }

                return;
            }

            GameObject manager = new GameObject(nameof(UxrNetworkManager));

            if (existingInstanceManager == null)
            {
                manager.AddComponent<UxrInstanceManager>();
            }

            manager.AddComponent<UxrNetworkManager>();
            manager.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            Selection.activeGameObject = manager;
            string undoString = $"Create {nameof(UxrNetworkManager)}";
            Undo.RegisterCreatedObjectUndo(manager, undoString);
            Undo.RegisterFullObjectHierarchyUndo(manager, undoString);
            Undo.SetCurrentGroupName(undoString);
        }

        /// <summary>
        ///     Checks that the given avatar has valid prefab GUID data.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="prefab">Prefab reference</param>
        private static void CheckAvatarHasPrefabGuid(UxrAvatar avatar, GameObject prefab)
        {
            SerializedObject   serializedAvatar   = new SerializedObject(avatar);
            SerializedProperty propertyPrefabGuid = serializedAvatar.FindProperty(UxrAvatarEditor.PropertyPrefabGuid);

            if (propertyPrefabGuid != null)
            {
                serializedAvatar.Update();
                propertyPrefabGuid.stringValue = UxrAvatarEditorExt.GetGuid(prefab);
                serializedAvatar.ApplyModifiedProperties();
            }
        }

        /// <summary>
        ///     Gets a formatted stats string describing the a Grabbable Object processing.
        /// </summary>
        /// <param name="stats">The result stats of added components</param>
        /// <param name="stats">The result stats of removed components</param>
        /// <returns>A formatted message string</returns>
        private static string GetStatsMessage(GrabbableObjectStats additionStats, GrabbableObjectStats removalStats)
        {
            string stats = string.Empty;

            string GetPlural(int number)
            {
                return number == 1 ? string.Empty : "s";
            }

            string GetMessage(string action, GrabbableObjectStats stats, bool showCheckLog)
            {
                string infoInstances = stats.InstanceComponentsAddedOrRemovedCount > 0 ? $"{action} {stats.InstanceComponentsAddedOrRemovedCount} network component{GetPlural(stats.InstanceComponentsAddedOrRemovedCount)} to synchronize {stats.InstanceComponentsProcessedCount} grabbable object{GetPlural(stats.InstanceComponentsProcessedCount)} in {stats.SceneCount} scene{GetPlural(stats.SceneCount)}.\n" : string.Empty;
                string infoPrefabs   = stats.PrefabComponentsAddedOrRemovedCount > 0 ? $"{action} {stats.PrefabComponentsAddedOrRemovedCount} network component{GetPlural(stats.PrefabComponentsAddedOrRemovedCount)} to synchronize {stats.PrefabComponentsProcessedCount} grabbable object{GetPlural(stats.PrefabComponentsProcessedCount)} in prefabs in the project." : string.Empty;
                string info          = string.IsNullOrEmpty(infoInstances) && string.IsNullOrEmpty(infoPrefabs) ? "No changes" : infoInstances + infoPrefabs;

                return info + (showCheckLog ? $"\n\nClick on {ContentPhysicsShowCurrentSetup.text} in the {nameof(UxrNetworkManager)} for complete information." : string.Empty);
            }

            if (removalStats != null && removalStats.HasAny)
            {
                stats =  GetMessage("Removed", removalStats, false);
                stats += "\n\n";
            }

            if (additionStats != null && additionStats.HasAny)
            {
                stats += GetMessage("Added", additionStats, true);
            }

            return stats;
        }

        /// <summary>
        ///     Clears the log information for the grabbable object network components setup.
        /// </summary>
        private void ClearGrabbablePhysicsSetupInfo()
        {
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoSdkUsed).stringValue = string.Empty;
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedScenePaths).ClearArray();
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedPathPrefabs).boolValue = false;
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedRootPath).stringValue  = string.Empty;
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedPrefabs).ClearArray();
            PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoDebugInfoLines).ClearArray();
        }

        /// <summary>
        ///     Removes all the added NetworkRigidbody components from the last setup.
        /// </summary>
        /// <param name="onlyLog">Whether to only log which components would be removed, but do not perform any actual removal</param>
        private void RemoveGrabbablePhysicsComponents(bool onlyLog)
        {
            IEnumerable<string> scenePaths = UxrEditorUtils.GetSerializedArrayAsEnumerable(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedScenePaths), p => p.stringValue);

            _lastGrabbableObjectRemovalStats            = new GrabbableObjectStats();
            _lastGrabbableObjectRemovalStats.SceneCount = scenePaths.Count();

            UxrEditorUtils.ProcessScenesAndProjectPathPrefabs<UxrGrabbableObject>(scenePaths,
                                                                                  PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedPathPrefabs).boolValue,
                                                                                  PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedRootPath).stringValue,
                                                                                  RemoveGrabbableComponentProcessor,
                                                                                  progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                                                  out bool canceled,
                                                                                  !SceneIsInUltimateXR,
                                                                                  onlyLog);
            if (!canceled && !onlyLog)
            {
                ClearGrabbablePhysicsSetupInfo();
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        ///     Component processor to remove networking components to grabbable objects for physics synchronization.
        /// </summary>
        /// <param name="info">Contains the component to process</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <returns>Whether the component required to be changed</returns>
        private bool RemoveGrabbableComponentProcessor(UxrComponentInfo<UxrGrabbableObject> info, bool onlyCheck)
        {
            if (info.TargetComponent.RigidBodySource == null || !info.TargetComponent.CanUseRigidBody || !info.TargetComponent.RigidBodyDynamicOnRelease)
            {
                return false;
            }

            bool isChanged = info.IsInnermostInValidChain;

            if (isChanged)
            {
                UxrNetworkComponentReferences[] allReferences = info.TargetComponent.GetComponents<UxrNetworkComponentReferences>();

                foreach (UxrNetworkComponentReferences references in allReferences)
                {
                    int componentCount = references.AddedComponents.Count(r => r != null);

                    if (componentCount > 0)
                    {
                        string sceneName     = info.TargetComponent.gameObject.scene.name;
                        string processAction = onlyCheck ? $"Would remove components from {info.TargetComponent.GetPathUnderScene()}" : $"Removing components from {info.TargetComponent.GetPathUnderScene()}";

                        if (!string.IsNullOrEmpty(sceneName))
                        {
                            processAction += $" in scene {sceneName}";

                            if (!onlyCheck)
                            {
                                _lastGrabbableObjectRemovalStats.InstanceComponentsProcessedCount++;
                                _lastGrabbableObjectRemovalStats.InstanceComponentsAddedOrRemovedCount += componentCount;
                            }
                        }
                        else if (info.TargetPrefab != null)
                        {
                            processAction += $" in prefab {info.TargetPrefab.name}";

                            if (!onlyCheck)
                            {
                                _lastGrabbableObjectRemovalStats.PrefabComponentsProcessedCount++;
                                _lastGrabbableObjectRemovalStats.PrefabComponentsAddedOrRemovedCount += componentCount;
                            }
                        }

                        processAction += ": " + string.Join(", ", references.AddedComponents.Where(c => c != null).Select(c => c.GetType().Name));
                        Debug.Log(processAction);
                    }

                    if (!onlyCheck)
                    {
                        references.DestroyWithAddedComponents();
                    }
                }
            }

            return isChanged;
        }

        /// <summary>
        ///     Component processor to add networking components to grabbable objects for physics synchronization.
        /// </summary>
        /// <param name="info">Contains the component to process</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <returns>Whether the component required to be changed</returns>
        private bool AddGrabbableComponentProcessor(UxrComponentInfo<UxrGrabbableObject> info, bool onlyCheck)
        {
            if (info.TargetComponent.RigidBodySource == null || !info.TargetComponent.CanUseRigidBody || !info.TargetComponent.RigidBodyDynamicOnRelease)
            {
                return false;
            }

            bool isChanged = info.IsInnermostInValidChain;

            Component componentInParentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(info.TargetComponent);

            string processAction        = onlyCheck ? "Would add components to sync grabbable rigidbody" : "Added components to sync grabbable rigidbody";
            string parentPrefabName     = componentInParentPrefab != null ? componentInParentPrefab.transform.root.name : string.Empty;
            bool   parentIsInUltimateXR = componentInParentPrefab != null && UxrEditorUtils.PathIsInUltimateXR(AssetDatabase.GetAssetPath(componentInParentPrefab));
            string scenePath            = info.TargetComponent.gameObject.scene.path;
            string logLine              = string.Empty;

            if (isChanged)
            {
                if (info.TargetPrefab != null)
                {
                    string additionalInfo = string.Empty;

                    if (info.IsOriginalSource)
                    {
                        additionalInfo = "Prefab is the topmost prefab.";
                    }
                    else if (info.IsInnermostInValidChain && parentIsInUltimateXR)
                    {
                        additionalInfo = $"Prefab is not the topmost prefab but parent prefab {parentPrefabName} belongs to UltimateXR and will remain unmodified.";
                    }
                    else if (info.IsInnermostInValidChain && !parentIsInUltimateXR)
                    {
                        additionalInfo = $"Prefab is not the topmost prefab but parent prefab {parentPrefabName} is not under project path {PropPhysicsAddPathRoot.stringValue} and will remain unmodified.";
                    }

                    logLine = $"{processAction} component {info.TargetComponent.GetPathUnderScene()} in prefab {info.TargetPrefab.name}. {additionalInfo}";
                    Debug.Log(logLine);
                }
                else
                {
                    string additionalInfo = string.Empty;

                    if (info.IsOriginalSource)
                    {
                        additionalInfo = string.Empty;
                    }
                    else if (info.IsInnermostInValidChain && parentIsInUltimateXR)
                    {
                        additionalInfo = $"Component has a parent prefab but prefab {parentPrefabName} belongs to UltimateXR and will remain unmodified.";
                    }
                    else if (info.IsInnermostInValidChain && !parentIsInUltimateXR)
                    {
                        additionalInfo = $"Component has a parent prefab but prefab {parentPrefabName} is not under project path {PropPhysicsAddPathRoot.stringValue} and will remain unmodified.";
                    }

                    logLine = $"{processAction} component {info.TargetComponent.GetPathUnderScene()} in scene {scenePath}. {additionalInfo}";
                    Debug.Log(logLine);
                }

                if (!onlyCheck)
                {
                    UxrEditorUtils.AppendSerializedArrayElement(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoDebugInfoLines), p => p.stringValue = logLine);

                    if (info.TargetPrefab != null)
                    {
                        UxrEditorUtils.AppendSerializedArrayElement(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoProcessedPrefabs), p => p.objectReferenceValue = info.TargetComponent);
                    }

                    if (_networkImplementation != null)
                    {
                        IEnumerable<Component> addedComponents = _networkImplementation.AddNetworkRigidbody(info.TargetComponent.gameObject, !info.TargetComponent.UsesGrabbableParentDependency, UxrNetworkRigidbodyFlags.All);
                        UxrNetworkComponentReferences.RegisterNetworkComponents(info.TargetComponent.gameObject, new List<GameObject>(), addedComponents, UxrNetworkComponentReferences.Origin.Network);

                        if (addedComponents.Any())
                        {
                            string componentLine = $"Components of type: {string.Join(", ", addedComponents.Select(c => c.GetType().Name))}";
                            Debug.Log(componentLine);
                            UxrEditorUtils.AppendSerializedArrayElement(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoDebugInfoLines), p => p.stringValue = "    " + componentLine);
                        }
                        else
                        {
                            string componentLine = $"No components have been added";
                            Debug.Log(componentLine);
                            UxrEditorUtils.AppendSerializedArrayElement(PropPhysicsSetupInfo.FindPropertyRelative(PropertyNamePhysicsInfoDebugInfoLines), p => p.stringValue = "    " + componentLine);
                        }

                        if (info.TargetPrefab != null)
                        {
                            _lastGrabbableObjectAdditionStats.PrefabComponentsProcessedCount++;
                            _lastGrabbableObjectAdditionStats.PrefabComponentsAddedOrRemovedCount += addedComponents.Count();
                        }
                        else if (!string.IsNullOrEmpty(scenePath))
                        {
                            _lastGrabbableObjectAdditionStats.InstanceComponentsProcessedCount++;
                            _lastGrabbableObjectAdditionStats.InstanceComponentsAddedOrRemovedCount += addedComponents.Count();
                        }
                    }
                    else
                    {
                        if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Errors)
                        {
                            Debug.LogError($"{UxrConstants.NetworkingModule} Could not instantiate the network implementation to add the components");
                        }
                    }
                }
            }

            return isChanged;
        }

        /// <summary>
        ///     Registers the available networking systems.
        /// </summary>
        private void RegisterNetworkingSystems()
        {
            // First remove missing components

            Component[] components   = _networkManager.GetComponents<Component>();
            int         removedCount = 0;

            for (int i = 0; i < components.Length; ++i)
            {
                if (components[i] == null)
                {
                    var propComponent = serializedObject.FindProperty("m_Component");

                    propComponent.DeleteArrayElementAtIndex(i - removedCount);
                    removedCount++;

                    serializedObject.ApplyModifiedProperties();
                }
            }

            // Now find or add network implementations

            void RegisterSdks<T>(ref List<string> availableSdkNames, ref Dictionary<string, T> cachedImplementations) where T : UxrComponent
            {
                availableSdkNames     ??= new List<string>();
                cachedImplementations ??= new Dictionary<string, T>();

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type t in assembly.GetTypes().Where(c => c.IsClass && !c.IsAbstract && c.IsSubclassOf(typeof(T))))
                    {
                        IUxrNetworkSdk implementationSdk       = _networkManager.GetComponent(t) as IUxrNetworkSdk;
                        UxrComponent   implementationComponent = implementationSdk as UxrComponent;

                        if (implementationSdk == null)
                        {
                            // If it's not on the manager, add but have it disabled.
                            implementationSdk = Undo.AddComponent(_networkManager.gameObject, t) as IUxrNetworkSdk;

                            implementationComponent         = implementationSdk as UxrComponent;
                            implementationComponent.enabled = false;
                            implementationComponent.ShowInInspector(false);

                            Undo.RegisterFullObjectHierarchyUndo(_networkManager, $"Add {implementationSdk.SdkName} support");
                        }

                        availableSdkNames.Add(implementationSdk.SdkName);

                        if (cachedImplementations.ContainsKey(implementationSdk.SdkName))
                        {
                            EditorUtility.DisplayDialog(UxrConstants.Editor.Error, $"SDK implementation for {implementationSdk.SdkName} is duplicated", UxrConstants.Editor.Ok);
                        }
                        else
                        {
                            cachedImplementations.Add(implementationSdk.SdkName, implementationComponent as T);
                        }
                    }
                }
            }

            RegisterSdks(ref _availableNetworkSdks,      ref _networkImplementations);
            RegisterSdks(ref _availableNetworkVoiceSdks, ref _networkVoiceImplementations);
        }

        /// <summary>
        ///     Gets a list of the available network SDK names.
        /// </summary>
        /// <returns>List of available SDK names</returns>
        private List<string> GetAvailableNetworkSdks()
        {
            return new List<string> { NoSdk }.Concat(_availableNetworkSdks).ToList();
        }

        /// <summary>
        ///     Gets a list of the available network voice SDK names.
        /// </summary>
        /// <param name="networkImplementation">The network implementation to check which network voice SDKs are compatible</param>
        /// <returns>List of available SDK names</returns>
        private List<string> GetAvailableNetworkVoiceSdks(IUxrNetworkImplementation networkImplementation)
        {
            List<string> availableVoiceSdks = new List<string> { NoSdk };

            if (networkImplementation == null)
            {
                return availableVoiceSdks;
            }

            return availableVoiceSdks.Concat(_networkVoiceImplementations.Where(v => v.Value.CompatibleNetworkSDKs.Contains(networkImplementation.SdkName)).Select(v => v.Key)).ToList();
        }

        /// <summary>
        ///     Sets up the global GameObjects/components using the given network implementation.
        /// </summary>
        /// <param name="networkImplementation">
        ///     The network implementation to set up or null to destroy all current GameObjects/components.
        /// </param>
        private void SetupGlobal(IUxrNetworkImplementation networkImplementation)
        {
            // Destroy components

            List<string> destroyErrors = new List<string>();

            for (int i = 0; i < PropCreatedGlobalComponents.arraySize; ++i)
            {
                Component component = (Component)PropCreatedGlobalComponents.GetArrayElementAtIndex(i).objectReferenceValue;

                if (component != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Destroying component {component.GetType().Name} in {component.GetPathUnderScene()}");
                    }

                    Undo.DestroyObjectImmediate(component);
                }
                else
                {
                    string path = i < PropCreatedGlobalComponentPaths.arraySize ? $" at {PropCreatedGlobalComponentPaths.GetArrayElementAtIndex(i).stringValue}" : string.Empty;

                    if (!string.IsNullOrEmpty(path))
                    {
                        destroyErrors.Add($"Network component: {path}");
                    }
                    
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.NetworkingModule} A network component could not be deleted{path}. Double check in your scene for any leftovers.");
                    }
                }
            }

            PropCreatedGlobalComponents.ClearArray();
            PropCreatedGlobalComponentPaths.ClearArray();
            serializedObject.ApplyModifiedProperties();

            // Destroy GameObjects

            for (int i = 0; i < PropCreatedGlobalGameObjects.arraySize; ++i)
            {
                GameObject go = (GameObject)PropCreatedGlobalGameObjects.GetArrayElementAtIndex(i).objectReferenceValue;

                if (go != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Destroying GameObject {go.GetPathUnderScene()}");
                    }

                    Undo.DestroyObjectImmediate(go);
                }
                else
                {
                    string path = i < PropCreatedGlobalGameObjectPaths.arraySize ? $" at {PropCreatedGlobalGameObjectPaths.GetArrayElementAtIndex(i).stringValue}" : string.Empty;

                    if (!string.IsNullOrEmpty(path))
                    {
                        destroyErrors.Add($"Network GameObject: {path}");
                    }
                    
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.NetworkingModule} A network GameObject could not be deleted{path}. Double check in your scene for any leftovers.");
                    }
                }
            }

            PropCreatedGlobalGameObjects.ClearArray();
            PropCreatedGlobalGameObjectPaths.ClearArray();
            serializedObject.ApplyModifiedProperties();
            
            // Setup and register added elements

            if (networkImplementation != null)
            {
                networkImplementation.SetupGlobal(serializedObject.targetObject as UxrNetworkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

                if (newGameObjects != null && newGameObjects.Any())
                {
                    UxrEditorUtils.AssignSerializedPropertyArray(PropCreatedGlobalGameObjects, newGameObjects);
                    UxrEditorUtils.AssignSerializedPropertySimpleTypeArray(PropCreatedGlobalGameObjectPaths, newGameObjects.Select(go => go != null ? go.GetPathUnderScene() : "null"));
                    serializedObject.ApplyModifiedProperties();
                }

                if (newComponents != null && newComponents.Any())
                {
                    UxrEditorUtils.AssignSerializedPropertyArray(PropCreatedGlobalComponents, newComponents);
                    UxrEditorUtils.AssignSerializedPropertySimpleTypeArray(PropCreatedGlobalComponentPaths, newComponents.Select(c => c != null ? c.GetPathUnderScene() : "null"));
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (destroyErrors.Any())
            {
                EditorUtility.DisplayDialog("Warning", $"The following networking elements could not be deleted correctly. Please delete them manually:\n\n{string.Join("\n", destroyErrors)}", UxrConstants.Editor.Ok);
            }
        }

        /// <summary>
        ///     Sets up the global GameObjects/components using the given network voice implementation.
        /// </summary>
        /// <param name="networkVoiceImplementation">
        ///     The network voice implementation to set up or null to destroy all current GameObjects/components.
        /// </param>
        private void SetupGlobal(IUxrNetworkVoiceImplementation networkVoiceImplementation)
        {
            List<string> destroyErrors = new List<string>();
            
            // Destroy components in reverse order because they are added in order that avoids automatic addition of dependencies. Otherwise deleting would add the dependency because it's missing.

            for (int i = 0; i < PropCreatedGlobalVoiceComponents.arraySize; ++i)
            {
                Component component = (Component)PropCreatedGlobalVoiceComponents.GetArrayElementAtIndex(i).objectReferenceValue;

                if (component != null)
                {
                    Debug.Log($"{UxrConstants.NetworkingModule} Destroying component {component.GetType().Name} in {component.GetPathUnderScene()}");
                    Undo.DestroyObjectImmediate(component);
                }
                else
                {
                    string path = i < PropCreatedGlobalVoiceComponentPaths.arraySize ? $" at {PropCreatedGlobalVoiceComponentPaths.GetArrayElementAtIndex(i).stringValue}" : string.Empty;

                    if (!string.IsNullOrEmpty(path))
                    {
                        destroyErrors.Add($"Network voice component: {path}");
                    }
                    
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.NetworkingModule} A network component could not be deleted{path}. Double check in your scene for any leftovers.");
                    }
                }
            }

            PropCreatedGlobalVoiceComponents.ClearArray();
            PropCreatedGlobalVoiceComponentPaths.ClearArray();
            serializedObject.ApplyModifiedProperties();

            // Destroy GameObjects

            for (int i = 0; i < PropCreatedGlobalVoiceGameObjects.arraySize; ++i)
            {
                GameObject go = (GameObject)PropCreatedGlobalVoiceGameObjects.GetArrayElementAtIndex(i).objectReferenceValue;

                if (go != null)
                {
                    Debug.Log($"{UxrConstants.NetworkingModule} Destroying GameObject {go.GetPathUnderScene()}");
                    Undo.DestroyObjectImmediate(go);
                }
                else
                {
                    string path = i < PropCreatedGlobalVoiceGameObjectPaths.arraySize ? $" at {PropCreatedGlobalVoiceGameObjectPaths.GetArrayElementAtIndex(i).stringValue}" : string.Empty;

                    if (!string.IsNullOrEmpty(path))
                    {
                        destroyErrors.Add($"Network voice GameObject: {path}");
                    }
                    
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.NetworkingModule} A network GameObject could not be deleted{path}. Double check in your scene for any leftovers.");
                    }
                }
            }

            PropCreatedGlobalVoiceGameObjects.ClearArray();
            PropCreatedGlobalVoiceGameObjectPaths.ClearArray();
            serializedObject.ApplyModifiedProperties();

            // Setup and register added elements

            if (networkVoiceImplementation != null)
            {
                networkVoiceImplementation.SetupGlobal(_networkImplementation?.SdkName, serializedObject.targetObject as UxrNetworkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

                if (newGameObjects != null && newGameObjects.Any())
                {
                    UxrEditorUtils.AssignSerializedPropertyArray(PropCreatedGlobalVoiceGameObjects, newGameObjects);
                    UxrEditorUtils.AssignSerializedPropertySimpleTypeArray(PropCreatedGlobalVoiceGameObjectPaths, newGameObjects.Select(go => go != null ? go.GetPathUnderScene() : "null"));
                    serializedObject.ApplyModifiedProperties();
                }

                if (newComponents != null && newComponents.Any())
                {
                    UxrEditorUtils.AssignSerializedPropertyArray(PropCreatedGlobalVoiceComponents, newComponents);
                    UxrEditorUtils.AssignSerializedPropertySimpleTypeArray(PropCreatedGlobalVoiceComponentPaths, newComponents.Select(c => c != null ? c.GetPathUnderScene() : "null"));
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (destroyErrors.Any())
            {
                EditorUtility.DisplayDialog("Warning", $"The following networking voice elements could not be deleted correctly. Please delete them manually:\n\n{string.Join("\n", destroyErrors)}", UxrConstants.Editor.Ok);
            }
        }

        /// <summary>
        ///     Sets up the currently registered avatars using the given network implementation.
        /// </summary>
        /// <param name="networkImplementation">
        ///     The network implementation to set up the avatar with or null to destroy all
        ///     components
        /// </param>
        private void SetupAvatars(IUxrNetworkImplementation networkImplementation)
        {
            for (int i = 0; i < PropRegisteredAvatars.arraySize; ++i)
            {
                SetupRegisteredAvatar(i, networkImplementation);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Performs the postprocess using the given network implementation.
        /// </summary>
        /// <param name="networkImplementation">
        ///     The network implementation to perform the postprocess with.
        /// </param>
        private void SetupPostProcess(IUxrNetworkImplementation networkImplementation)
        {
            if (networkImplementation == null)
            {
                return;
            }
            
            List<UxrAvatar> registeredAvatarPrefabs = new List<UxrAvatar>();

            for (int i = 0; i < PropRegisteredAvatars.arraySize; ++i)
            {
                UxrAvatar avatarPrefab = PropRegisteredAvatars.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;

                if (avatarPrefab != null)
                {
                    registeredAvatarPrefabs.Add(avatarPrefab);
                }
            }
            
            networkImplementation.SetupPostProcess(registeredAvatarPrefabs);
        }

        /// <summary>
        ///     Sets up the currently registered avatars using the given network voice implementation.
        /// </summary>
        /// <param name="networkVoiceImplementation">
        ///     The network voice implementation to set up the avatar with or null to destroy all components
        /// </param>
        private void SetupAvatars(IUxrNetworkVoiceImplementation networkVoiceImplementation)
        {
            for (int i = 0; i < PropRegisteredAvatars.arraySize; ++i)
            {
                SetupRegisteredAvatar(i, networkVoiceImplementation);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Sets up an avatar prefab using a given network implementation.
        /// </summary>
        /// <param name="avatarIndex">The registered avatar index</param>
        /// <param name="networkImplementation">The network implementation to use</param>
        private void SetupRegisteredAvatar(int avatarIndex, IUxrNetworkImplementation networkImplementation)
        {
            UxrAvatar avatarPrefab = PropRegisteredAvatars.GetArrayElementAtIndex(avatarIndex).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;

            UxrNetworkComponentReferences.DestroyNetworkComponentsRecursively(avatarPrefab.gameObject, UxrNetworkComponentReferences.Origin.Network);

            PrefabUtility.SavePrefabAsset(avatarPrefab.gameObject);

            if (networkImplementation != null)
            {
                UxrAvatar avatarInstance = null;

                try
                {
                    avatarInstance = PrefabUtility.InstantiatePrefab(avatarPrefab) as UxrAvatar;
                    networkImplementation.SetupAvatar(avatarInstance, out List<GameObject> addedGameObjects, out List<Component> addedComponents);
                    UxrNetworkComponentReferences.RegisterNetworkComponents(avatarInstance.gameObject, addedGameObjects, addedComponents, UxrNetworkComponentReferences.Origin.Network);

                    // Initially external to avoid rendering a frame when a new avatar is instantiated.
                    // The avatar will be switched to the correct mode (local/external) after spawning.
                    avatarInstance.AvatarMode = UxrAvatarMode.UpdateExternally;

                    PrefabUtility.ApplyPrefabInstance(avatarInstance.gameObject, InteractionMode.AutomatedAction);
                }
                finally
                {
                    DestroyImmediate(avatarInstance.gameObject);
                }
            }
        }

        /// <summary>
        ///     Sets up an avatar prefab using a given network voice implementation.
        /// </summary>
        /// <param name="avatarIndex">The registered avatar index</param>
        /// <param name="networkImplementation">The network voice implementation to use</param>
        private void SetupRegisteredAvatar(int avatarIndex, IUxrNetworkVoiceImplementation networkVoiceImplementation)
        {
            UxrAvatar avatarPrefab = PropRegisteredAvatars.GetArrayElementAtIndex(avatarIndex).FindPropertyRelative(PropertyNameAvatarPrefab).objectReferenceValue as UxrAvatar;

            UxrNetworkComponentReferences.DestroyNetworkComponentsRecursively(avatarPrefab.gameObject, UxrNetworkComponentReferences.Origin.NetworkVoice);

            PrefabUtility.SavePrefabAsset(avatarPrefab.gameObject);

            if (networkVoiceImplementation != null)
            {
                UxrAvatar avatarInstance = null;

                try
                {
                    avatarInstance = PrefabUtility.InstantiatePrefab(avatarPrefab) as UxrAvatar;
                    networkVoiceImplementation.SetupAvatar(_networkImplementation?.SdkName, avatarInstance, out List<GameObject> addedGameObjects, out List<Component> addedComponents);
                    UxrNetworkComponentReferences.RegisterNetworkComponents(avatarInstance.gameObject, addedGameObjects, addedComponents, UxrNetworkComponentReferences.Origin.NetworkVoice);
                    PrefabUtility.ApplyPrefabInstance(avatarInstance.gameObject, InteractionMode.AutomatedAction);
                }
                finally
                {
                    DestroyImmediate(avatarInstance.gameObject);
                }
            }
        }

        /// <summary>
        ///     Gets a log string to describe a GameObject that was created to add networking support.
        /// </summary>
        /// <param name="gameObject">GameObject that was added</param>
        /// <returns>Log string</returns>
        private string GetLogLine(GameObject gameObject)
        {
            return gameObject != null ? $"Added GameObject {gameObject.GetPathUnderScene()}." : "Found GameObject with a null reference.";
        }

        /// <summary>
        ///     Gets a log string to describe a GameObject that was created to add networking support.
        /// </summary>
        /// <param name="component">Component that was added</param>
        /// <returns>Log string</returns>
        private string GetLogLine(Component component)
        {
            return component != null ? $"Added component of type {component.GetType().Name} to GameObject {component.gameObject.GetPathUnderScene()}." : "Found Component with a null reference.";
        }

        #endregion

        #region Private Types & Data

        private static GUIContent ContentNetworkSystem           { get; } = new GUIContent("Network System",               "Selects which networking system will be used");
        private static GUIContent ContentUseSameSdkVoice         { get; } = new GUIContent("Use Voice Capabilities",       "Whether to use the voice capabilities of the networking SDK");
        private static GUIContent ContentNetworkVoiceSystem      { get; } = new GUIContent("Network Voice System",         "Selects which system will be used for networked voice transmission");
        private static GUIContent ContentOpenSdkManagerWindow    { get; } = new GUIContent("Open SDK Manager",             "Opens the SDK Manager to review the networking SDKs");
        private static GUIContent ContentShowGlobalInfo          { get; } = new GUIContent("View Component Info",          "Lists the components that were added globally for the current networking SDK");
        private static GUIContent ContentSetupAvatar             { get; } = new GUIContent("Register avatar",              "Sets up the avatar for networking using the currently selected networking SDK");
        private static GUIContent ContentShowAvatarInfo          { get; } = new GUIContent("View Info",                    "Lists the components that were added to the avatar to set it up using the current networking SDK");
        private static GUIContent ContentSelectAvatar            { get; } = new GUIContent("Select",                       "Selects the avatar prefab in the project window");
        private static GUIContent ContentRemoveAvatar            { get; } = new GUIContent("Remove",                       "Removes the avatar from the list and deletes all added network components");
        private static GUIContent ContentPhysicsAddProjectScenes { get; } = new GUIContent("Set Up Scenes In Build",       $"Will add network components to all {nameof(UxrGrabbableObject)} objects and prefabs with rigidbodies referenced by the scenes in the build.");
        private static GUIContent ContentPhysicsAddPathPrefabs   { get; } = new GUIContent("Set Up Project Prefabs",       $"Will also process all {nameof(UxrGrabbableObject)} prefabs that have a rigidbody, located under the path below, that might not be in the build scenes but might be instantiated at runtime.");
        private static GUIContent ContentPhysicsAddPathRoot      { get; } = new GUIContent("Prefab Root Path",             $"Root project path where to look for the {nameof(UxrGrabbableObject)} object prefabs that have a rigidbody. It allows to avoid unwanted modification of prefabs from other folders. All subdirectories will be processed. Leave empty to process the whole project.");
        private static GUIContent ContentChooseFolder            { get; } = new GUIContent("...",                          $"Selects the root folder to process when looking for {nameof(UxrGrabbableObject)} object prefabs that have a rigidbody");
        private static GUIContent ContentPhysicsOnlyLog          { get; } = new GUIContent("Don't Modify, Only Show Info", "Scared to click Add or Remove Components? This option will not add any components and only let you know through the console which objects will have components added.");
        private static GUIContent ContentPhysicsAdd              { get; } = new GUIContent("Add Components",               $"Adds the required network components to handle all {nameof(UxrGrabbableObject)} objects that have a rigidbody");
        private static GUIContent ContentPhysicsReplace          { get; } = new GUIContent("Replace Components",           $"Replaces the current network components added by the previous selected SDK with new ones to handle all {nameof(UxrGrabbableObject)} objects that have a rigidbody");
        private static GUIContent ContentPhysicsRemove           { get; } = new GUIContent("Remove Components",            $"Removes added network components from the selected {nameof(UxrGrabbableObject)} objects that have a rigidbody");
        private static GUIContent ContentPhysicsShowCurrentSetup { get; } = new GUIContent("View Component Info",          $"Shows a log window with the current {nameof(UxrGrabbableObject)} objects that have been set up");

        private bool SceneIsInUltimateXR => UxrEditorUtils.PathIsInUltimateXR((serializedObject.targetObject as UxrNetworkManager).gameObject.scene.path);

        private const string NoSdk = "None";

        private const string PropertyNameNetworkImplementation             = "_networkImplementation";
        private const string PropertyNameNetworkVoiceImplementation        = "_networkVoiceImplementation";
        private const string PropertyNameUseSameSdkVoice                   = "_useSameSdkVoice";
        private const string PropertyNameCreatedGlobalGameObjects          = "_createdGlobalGameObjects";
        private const string PropertyNameCreatedGlobalComponents           = "_createdGlobalComponents";
        private const string PropertyNameCreatedGlobalVoiceGameObjects     = "_createdGlobalVoiceGameObjects";
        private const string PropertyNameCreatedGlobalVoiceComponents      = "_createdGlobalVoiceComponents";
        private const string PropertyNameCreatedGlobalGameObjectPaths      = "_createdGlobalGameObjectPaths";
        private const string PropertyNameCreatedGlobalComponentPaths       = "_createdGlobalComponentPaths";
        private const string PropertyNameCreatedGlobalVoiceGameObjectPaths = "_createdGlobalVoiceGameObjectPaths";
        private const string PropertyNameCreatedGlobalVoiceComponentPaths  = "_createdGlobalVoiceComponentPaths";
        private const string PropertyNameRegisteredAvatars                 = "_registeredAvatars";
        private const string PropertyNameAvatarPrefab                      = "_avatarPrefab";
        private const string PropertyNamePhysicsAddProjectScenes           = "_grabbablePhysicsAddProjectScenes";
        private const string PropertyNamePhysicsAddPathPrefabs             = "_grabbablePhysicsAddPathPrefabs";
        private const string PropertyNamePhysicsPathRoot                   = "_grabbablePhysicsPathRoot";
        private const string PropertyNamePhysicsOnlyLog                    = "_grabbablePhysicsOnlyLog";
        private const string PropertyNamePhysicsSetupInfo                  = "_grabbablePhysicsSetupInfo";
        private const string PropertyNamePhysicsInfoSdkUsed                = "_sdkUsed";
        private const string PropertyNamePhysicsInfoProcessedScenePaths    = "_processedScenePaths";
        private const string PropertyNamePhysicsInfoProcessedPathPrefabs   = "_processedPathPrefabs";
        private const string PropertyNamePhysicsInfoProcessedRootPath      = "_processedRootPath";
        private const string PropertyNamePhysicsInfoProcessedPrefabs       = "_processedPrefabs";
        private const string PropertyNamePhysicsInfoDebugInfoLines         = "_debugInfoLines";

        private SerializedProperty PropNetworkImplementation             => serializedObject.FindProperty(PropertyNameNetworkImplementation);
        private SerializedProperty PropNetworkVoiceImplementation        => serializedObject.FindProperty(PropertyNameNetworkVoiceImplementation);
        private SerializedProperty PropUseSameSdkVoice                   => serializedObject.FindProperty(PropertyNameUseSameSdkVoice);
        private SerializedProperty PropCreatedGlobalGameObjects          => serializedObject.FindProperty(PropertyNameCreatedGlobalGameObjects);
        private SerializedProperty PropCreatedGlobalComponents           => serializedObject.FindProperty(PropertyNameCreatedGlobalComponents);
        private SerializedProperty PropCreatedGlobalVoiceGameObjects     => serializedObject.FindProperty(PropertyNameCreatedGlobalVoiceGameObjects);
        private SerializedProperty PropCreatedGlobalVoiceComponents      => serializedObject.FindProperty(PropertyNameCreatedGlobalVoiceComponents);
        private SerializedProperty PropCreatedGlobalGameObjectPaths      => serializedObject.FindProperty(PropertyNameCreatedGlobalGameObjectPaths);
        private SerializedProperty PropCreatedGlobalComponentPaths       => serializedObject.FindProperty(PropertyNameCreatedGlobalComponentPaths);
        private SerializedProperty PropCreatedGlobalVoiceGameObjectPaths => serializedObject.FindProperty(PropertyNameCreatedGlobalVoiceGameObjectPaths);
        private SerializedProperty PropCreatedGlobalVoiceComponentPaths  => serializedObject.FindProperty(PropertyNameCreatedGlobalVoiceComponentPaths);
        private SerializedProperty PropRegisteredAvatars                 => serializedObject.FindProperty(PropertyNameRegisteredAvatars);
        private SerializedProperty PropPhysicsAddProjectScenes           => serializedObject.FindProperty(PropertyNamePhysicsAddProjectScenes);
        private SerializedProperty PropPhysicsAddPathPrefabs             => serializedObject.FindProperty(PropertyNamePhysicsAddPathPrefabs);
        private SerializedProperty PropPhysicsAddPathRoot                => serializedObject.FindProperty(PropertyNamePhysicsPathRoot);
        private SerializedProperty PropPhysicsOnlyLog                    => serializedObject.FindProperty(PropertyNamePhysicsOnlyLog);
        private SerializedProperty PropPhysicsSetupInfo                  => serializedObject.FindProperty(PropertyNamePhysicsSetupInfo);

        private UxrNetworkManager _networkManager;
        private List<string>      _availableNetworkSdks;
        private List<string>      _availableNetworkVoiceSdks;
        private int               _networkingIndex;
        private int               _networkingVoiceIndex;

        private Dictionary<string, UxrNetworkImplementation>      _networkImplementations;
        private Dictionary<string, UxrNetworkVoiceImplementation> _networkVoiceImplementations;
        private UxrNetworkImplementation                          _networkImplementation;
        private UxrNetworkVoiceImplementation                     _networkVoiceImplementation;

        private bool _showNetworking        = true;
        private bool _showAvatars           = true;
        private bool _showPhysicsGrabbables = true;

        private GrabbableObjectStats _lastGrabbableObjectAdditionStats = new GrabbableObjectStats();
        private GrabbableObjectStats _lastGrabbableObjectRemovalStats  = new GrabbableObjectStats();

        #endregion
    }
}