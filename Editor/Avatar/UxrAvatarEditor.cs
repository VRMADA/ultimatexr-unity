// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.IK;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Avatar.Rig;
using UltimateXR.Devices;
using UltimateXR.Editor.Animation.IK;
using UltimateXR.Editor.Avatar.Controllers;
using UltimateXR.Editor.Manipulation.HandPoses;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltimateXR.Editor.Avatar
{
    /// <summary>
    ///     Custom editor for <see cref="UxrAvatar" />.
    /// </summary>
    [CustomEditor(typeof(UxrAvatar))]
    public class UxrAvatarEditor : UnityEditor.Editor
    {
        #region Public Types & Data

        public const string PropertyParentPrefab    = "_parentPrefab";
        public const string PropertyPrefabGuid      = "_prefabGuid";
        public const string PropertyHandPoses       = "_handPoses";
        public const string PropertyDefaultHandPose = "_defaultHandPose";

        #endregion

        #region Unity

        /// <summary>
        ///     Caches serialized properties and initializes the avatar rig expandable field.
        /// </summary>
        private void OnEnable()
        {
            _propertyPrefabGuid             = serializedObject.FindProperty(PropertyPrefabGuid);
            _propertyParentPrefab           = serializedObject.FindProperty(PropertyParentPrefab);
            _propertyAvatarMode             = serializedObject.FindProperty("_avatarMode");
            _propertyRenderMode             = serializedObject.FindProperty("_renderMode");
            _propertyShowControllerHands    = serializedObject.FindProperty("_showControllerHands");
            _propertyAvatarRenderers        = serializedObject.FindProperty("_avatarRenderers");
            _propertyRigType                = serializedObject.FindProperty("_rigType");
            _propertyRigExpandedInitialized = serializedObject.FindProperty("_rigExpandedInitialized");
            _propertyRigFoldout             = serializedObject.FindProperty("_rigFoldout");
            _propertyRig                    = serializedObject.FindProperty("_rig");
            _propertyHandPosesFoldout       = serializedObject.FindProperty("_handPosesFoldout");
            _propertyHandPoses              = serializedObject.FindProperty(PropertyHandPoses);

            // Expand rig when created, only once.

            if (_propertyRigExpandedInitialized.boolValue == false)
            {
                _propertyRigExpandedInitialized.boolValue = true;
                _propertyRig.isExpanded                   = true;

                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        ///     Draws the custom inspector and handles events.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrAvatar avatar = (UxrAvatar)serializedObject.targetObject;

            if (avatar == null)
            {
                return;
            }

            GameObject prefab       = null;
            GameObject parentPrefab = null;

            if (avatar.gameObject.IsPrefab())
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage != null && prefabStage.prefabContentsRoot == avatar.gameObject)
                {
                    // Open in prefab window
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabStage.assetPath);
                }
                else
                {
                    prefab = avatar.gameObject;
                }
            }
            else
            {
                UxrEditorUtils.GetPrefab(avatar.gameObject, out prefab);
                
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage == null || prefabStage.prefabContentsRoot != avatar.gameObject)
                {
                    // Not open in prefab window.
                    // Fix for avatar prefabs nested in another higher-level prefab
                    while (prefab != null && prefab.gameObject.transform.parent != null)
                    {
                        UxrEditorUtils.GetPrefab(prefab.gameObject, out prefab);
                    }
                }
            }

            if (prefab != null)
            {
                UxrEditorUtils.GetPrefab(prefab.gameObject, out parentPrefab);
            }

            UxrAvatar avatarPrefab       = prefab != null ? prefab.GetComponent<UxrAvatar>() : null;
            string    avatarPrefabGuid   = prefab != null ? AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(prefab)).ToString() : null;
            UxrAvatar avatarParentPrefab = parentPrefab != null ? parentPrefab.GetComponent<UxrAvatar>() : null;

            if (!Application.isPlaying)
            {
                // Check if we need to get and store the prefab information:

                SerializedObject   targetAvatarPrefabObject = avatarPrefab != null ? new SerializedObject(avatarPrefab) : null;
                SerializedProperty propertyPrefabGuid       = targetAvatarPrefabObject?.FindProperty(PropertyPrefabGuid);
                SerializedProperty propertyParentPrefab     = targetAvatarPrefabObject?.FindProperty(PropertyParentPrefab);

                if (propertyPrefabGuid != null && !string.IsNullOrEmpty(avatarPrefabGuid) && propertyPrefabGuid.stringValue != avatarPrefabGuid)
                {
                    // Prefab information not added yet: add.
                    propertyPrefabGuid.stringValue = avatarPrefabGuid;
                    targetAvatarPrefabObject.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }

                if (propertyParentPrefab != null && !ReferenceEquals(propertyParentPrefab.objectReferenceValue, parentPrefab))
                {
                    // Parent prefab information not added yet: add.
                    propertyParentPrefab.objectReferenceValue = parentPrefab;
                    targetAvatarPrefabObject.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }

                // Force instances getting parent prefab from their source prefab
                if (prefab != null && avatar.gameObject != prefab && propertyParentPrefab != null)
                {
                    _propertyParentPrefab.prefabOverride = false;
                }

                if (targetAvatarPrefabObject == null && (_propertyPrefabGuid.stringValue != string.Empty || _propertyParentPrefab.objectReferenceValue != null))
                {
                    // Prefab information unlinked. Probably used "unpack prefab" functionality in Unity. Set references to null.
                    _propertyPrefabGuid.stringValue            = string.Empty;
                    _propertyParentPrefab.objectReferenceValue = null;
                }

                SerializedProperty propertyHandPoses = targetAvatarPrefabObject?.FindProperty(PropertyHandPoses);

                if (propertyHandPoses != null)
                {
                    // Check if we need to eliminate empty hand poses, references deleted without using the editor.

                    int deletedCount = 0;

                    for (int i = 0; i < propertyHandPoses.arraySize; ++i)
                    {
                        if (propertyHandPoses.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        {
                            propertyHandPoses.DeleteArrayElementAtIndex(i);
                            deletedCount++;
                            i--;
                        }
                    }

                    if (deletedCount > 0)
                    {
                        targetAvatarPrefabObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                    }

                    // Check if we need to eliminate duplicated hand poses generated through prefab variants.

                    if (avatarPrefab != null && avatarParentPrefab != null)
                    {
                        IEnumerable<UxrHandPoseAsset> netHandPoses = avatarPrefab.GetHandPoses().Where(handPose => avatarParentPrefab.GetHandPoses().All(handPose2 => handPose != handPose2));

                        if (netHandPoses.Count() != propertyHandPoses.arraySize)
                        {
                            UxrEditorUtils.AssignSerializedPropertyArray(propertyHandPoses, netHandPoses);
                            targetAvatarPrefabObject.ApplyModifiedProperties();
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            if (!avatar.gameObject.IsPrefab())
            {
                // Assistant

                Camera camera       = avatar.GetComponentInChildren<Camera>();
                Scene  currentScene = avatar.gameObject.scene;

                if (!UxrEditorUtils.PathIsInUltimateXR(currentScene.path) && prefab != null && UxrEditorUtils.PathIsInUltimateXR(AssetDatabase.GetAssetPath(prefab)))
                {
                    EditorGUILayout.HelpBox(NeedsPrefabVariant, MessageType.Warning);

                    if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                    {
                        if (UxrEditorUtils.CreateAvatarPrefab(avatar, "Save prefab variant", avatar.gameObject.name + "Variant", out prefab, out GameObject newInstance))
                        {
                            if (newInstance == null)
                            {
                                EditorUtility.DisplayDialog("Error", "The prefab variant was created but it could not be instantiated in the scene. Try doing it manually.", "OK");
                            }
                            else
                            {
                                Selection.activeGameObject = newInstance.gameObject;
                                avatar                     = newInstance.GetComponent<UxrAvatar>();
                                avatar.name                = prefab.name;
                            }

                            // If in prefab stage, save
                            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                            if (prefabStage != null && avatar.transform.parent != null)
                            {
                                // @TODO: Should save in innermost parent prefab, but currently the only way I found to force a save is using MarkSceneDirty
                                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                            }
                            
                            return;
                        }
                    }
                }
                else if (camera == null)
                {
                    camera = FindObjectOfType<Camera>();

                    if (camera == null || camera.SafeGetComponentInParent<UxrAvatar>() != null)
                    {
                        EditorGUILayout.HelpBox(NeedsCameraNewHelp, MessageType.Warning);

                        if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                        {
                            GameObject cameraController = new GameObject("Camera Controller");
                            cameraController.transform.SetPositionAndRotation(avatar.transform.position, avatar.transform.rotation);
                            cameraController.transform.parent   = avatar.transform;
                            cameraController.transform.SetAsFirstSibling();
                            Undo.RegisterCreatedObjectUndo(cameraController, "Create Camera Controller");

                            GameObject cameraObject = new GameObject("Camera");
                            cameraObject.transform.SetPositionAndRotation(cameraController.transform.position, cameraController.transform.rotation);
                            cameraObject.transform.parent   = cameraController.transform;
                            cameraObject.tag                = "MainCamera";
                            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Camera");

                            Camera newCamera = cameraObject.AddComponent<Camera>();
                            newCamera.nearClipPlane = 0.01f;
                            cameraObject.AddComponent<AudioListener>();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(NeedsCameraReparentHelp, MessageType.Warning);

                        if (camera.transform.parent == null)
                        {
                            if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                            {
                                GameObject cameraController = new GameObject("Camera Controller");
                                cameraController.transform.SetPositionAndRotation(avatar.transform.position, avatar.transform.rotation);
                                cameraController.transform.parent   = avatar.transform;
                                cameraController.transform.SetAsFirstSibling();
                                Undo.RegisterCreatedObjectUndo(cameraController, "Create Camera Controller");

                                Undo.RecordObject(camera, "Set Camera Near");
                                camera.nearClipPlane = 0.01f;

                                Undo.RecordObject(camera.transform, "Move Camera");
                                camera.transform.SetPositionAndRotation(cameraController.transform.position, cameraController.transform.rotation);

                                Undo.SetTransformParent(camera.transform, cameraController.transform, "Re-parent Camera");
                            }
                        }
                    }
                }
                else
                {
                    if (camera.transform.parent == avatar.transform)
                    {
                        EditorGUILayout.HelpBox(NeedsCameraParentHelp, MessageType.Warning);

                        if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                        {
                            GameObject cameraController = new GameObject("Camera Controller");
                            cameraController.transform.SetPositionAndRotation(camera.transform.position, camera.transform.rotation);
                            cameraController.transform.parent = avatar.transform;
                            cameraController.transform.SetAsFirstSibling();
                            Undo.RegisterCreatedObjectUndo(cameraController, "Create Camera Controller");
                            Undo.SetTransformParent(camera.transform, cameraController.transform, "Re-parent Camera");
                            Undo.RecordObject(camera, "Set Camera Near");
                            camera.nearClipPlane = 0.01f;
                        }
                    }
                    else
                    {
                        UxrAvatarController avatarController = avatar.GetComponentInChildren<UxrAvatarController>();

                        if (avatarController == null)
                        {
                            EditorGUILayout.HelpBox(NeedsAvatarControllerHelp, MessageType.Warning);

                            if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                            {
                                UxrStandardAvatarController standardAvatarController = avatar.gameObject.AddComponent<UxrStandardAvatarController>();
                                Undo.RegisterCreatedObjectUndo(standardAvatarController, "Create Avatar Controller");
                            }
                        }
                        else if (!avatar.AvatarRig.HasFullHandData())
                        {
                            EditorGUILayout.HelpBox(NeedsFingerSetup, MessageType.Warning);

                            if (GUILayout.Button(ContentFix, GUILayout.Width(FixButtonWidth)))
                            {
                                avatar.TryToInferMissingRigElements();
                                
                                if (avatar.AvatarRig.HasAnyUpperBodyIKReference())
                                {
                                    // Make avatar full-body if it has any upper body reference
                                    _propertyRigType.enumValueIndex = (int)UxrAvatarRigType.HalfOrFullBody;
                                }
                                
                                RefreshRigSerializedProperty(avatar);

                                if (!avatar.AvatarRig.HasFullHandData())
                                {
                                    EditorUtility.DisplayDialog("Missing data", "Could not try to figure out all hand and finger bone references. Try setting them up manually under the Avatar rig section.", "OK");
                                }
                            }
                        }
                        else
                        {
                            UxrTrackingDevice avatarTracking = avatar.GetComponentInChildren<UxrTrackingDevice>();

                            if (avatarTracking == null)
                            {
                                EditorGUILayout.HelpBox(NeedsIntegrationHelp, MessageType.Warning);

                                EditorGUILayout.BeginHorizontal();

                                string integrationPrefabGuid = null;

                                if (GUILayout.Button(ContentBigHandsIntegration, GUILayout.Width(FixButtonWidth)))
                                {
                                    integrationPrefabGuid = BigHandsIntegrationGuid;
                                }

                                if (GUILayout.Button(ContentSmallHandsIntegration, GUILayout.Width(FixButtonWidth)))
                                {
                                    integrationPrefabGuid = SmallHandsIntegrationGuid;
                                }

                                if (integrationPrefabGuid != null)
                                {
                                    // Add hand integration and try to place on avatar hands

                                    GameObject handsIntegrationAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(integrationPrefabGuid));

                                    if (handsIntegrationAsset != null)
                                    {
                                        GameObject handsIntegration = PrefabUtility.InstantiatePrefab(handsIntegrationAsset, avatar.transform) as GameObject;

                                        if (handsIntegration)
                                        {
                                            handsIntegration.name = handsIntegrationAsset.name;
                                            Undo.RegisterCreatedObjectUndo(handsIntegration, "Add Hands Integration");

                                            handsIntegration.GetComponentsInChildren<UxrHandIntegration>().ForEach(i => i.TryToMatchHand());
                                        }
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Avatar is ready to rock!", MessageType.Info);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            // Rest of inspector

            _foldoutGeneral = UxrEditorUtils.FoldoutStylish("General parameters:", _foldoutGeneral);

            if (_foldoutGeneral)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(ContentPrefab, prefab, typeof(GameObject));
                EditorGUILayout.PropertyField(_propertyParentPrefab, ContentParentPrefab);
                GUI.enabled = true;
                EditorGUILayout.PropertyField(_propertyAvatarMode, ContentAvatarMode);
            }

            // Renderers

            _foldoutRendering = UxrEditorUtils.FoldoutStylish("Avatar rendering:", _foldoutRendering);

            if (_foldoutRendering)
            {
                _propertyRenderMode.intValue = EditorGUILayout.MaskField(ContentRenderMode, _propertyRenderMode.intValue, UxrEditorUtils.GetAvatarRenderModeNames().SplitCamelCase().ToArray());
                EditorGUILayout.PropertyField(_propertyShowControllerHands, ContentShowControllerHands);
                EditorGUILayout.PropertyField(_propertyAvatarRenderers,     ContentAvatarRenderers, true);
            }

            // Rig

            _propertyRigFoldout.boolValue = UxrEditorUtils.FoldoutStylish("Avatar rig:", _propertyRigFoldout.boolValue);

            if (_propertyRigFoldout.boolValue)
            {
                if (_propertyRigType.enumValueIndex == (int)UxrAvatarRigType.HandsOnly)
                {
                    EditorGUILayout.PropertyField(_propertyRigType, ContentRigType);

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(ContentTryToFillGuessing, GUILayout.Width(ButtonWidth)))
                    {
                        avatar.TryToInferMissingRigElements();
                        RefreshRigSerializedProperty(avatar);
                    }

                    if (GUILayout.Button(ContentClearRig, GUILayout.Width(ButtonWidth)))
                    {
                        avatar.ClearRigElements();
                        RefreshRigSerializedProperty(avatar);
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(_propertyRig.FindPropertyRelative("_head").FindPropertyRelative("_head"),     ContentHead);
                    EditorGUILayout.PropertyField(_propertyRig.FindPropertyRelative("_leftArm").FindPropertyRelative("_hand"),  ContentLeftHand);
                    EditorGUILayout.PropertyField(_propertyRig.FindPropertyRelative("_rightArm").FindPropertyRelative("_hand"), ContentRightHand);
                }
                else if (_propertyRigType.enumValueIndex == (int)UxrAvatarRigType.HalfOrFullBody)
                {
                    EditorGUILayout.PropertyField(_propertyRigType, ContentRigType);

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(ContentAutoFillFromAnimator, GUILayout.Width(ButtonWidth)))
                    {
                        if (!avatar.SetupRigElementsFromAnimator())
                        {
                            EditorUtility.DisplayDialog("Animator data is missing", "Could not find any Animator component with humanoid data. Please use a model with a humanoid avatar to use autofill.", "OK");
                        }
                        else
                        {
                            RefreshRigSerializedProperty(avatar);
                        }
                    }

                    if (GUILayout.Button(ContentTryToFillGuessing, GUILayout.Width(ButtonWidth)))
                    {
                        avatar.TryToInferMissingRigElements();
                        RefreshRigSerializedProperty(avatar);
                    }

                    if (GUILayout.Button(ContentClearRig, GUILayout.Width(ButtonWidth)))
                    {
                        avatar.ClearRigElements();
                        RefreshRigSerializedProperty(avatar);
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(_propertyRig, ContentRig);
                }
            }

            // Hand poses

            bool createPrefab = false;

            _propertyHandPosesFoldout.boolValue = UxrEditorUtils.FoldoutStylish("Hand poses:", _propertyHandPosesFoldout.boolValue);

            if (_propertyHandPosesFoldout.boolValue)
            {
                if (string.IsNullOrEmpty(avatar.PrefabGuid))
                {
                    GUILayout.Label("Avatar requires a prefab in order to use hand poses.");

                    if (UxrEditorUtils.CenteredButton(ContentCreatePrefab))
                    {
                        // Create prefab at the end.
                        createPrefab = true;
                    }
                }
                else if (!avatar.GetAllHandPoses().Any())
                {
                    GUILayout.Label("No hand poses available. Use the Hand Pose Editor to add hand poses.");
                }
                else
                {
                    foreach (UxrAvatar avatarChainPrefab in avatar.GetPrefabChain())
                    {
                        IEnumerable<UxrHandPoseAsset> handPoses = avatarChainPrefab.GetHandPoses();

                        if (handPoses.Any())
                        {
                            EditorGUILayout.BeginHorizontal();

                            if (avatarChainPrefab != avatarPrefab)
                            {
                                GUILayout.Label($"{handPoses.Count()} inherited from {avatarChainPrefab.name}:");

                                if (GUILayout.Button(ContentSelectPrefab))
                                {
                                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UxrAvatar>(AssetDatabase.GetAssetPath(avatarChainPrefab));
                                }
                            }
                            else
                            {
                                GUILayout.Label($"{handPoses.Count()} in {avatarChainPrefab.name}:");
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            EditorGUI.indentLevel++;

                            foreach (UxrHandPoseAsset pose in handPoses)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{pose.name}: ", $"Type: {pose.PoseType.ToString()}");

                                if (GUILayout.Button(ContentSelectPoseAsset))
                                {
                                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UxrHandPoseAsset>(AssetDatabase.GetAssetPath(pose));
                                }

                                if (!avatar.gameObject.IsPrefab())
                                {
                                    if (!avatar.IsHandPoseOverriden(pose) && GUILayout.Button(ContentOpenPose))
                                    {
                                        UxrHandPoseEditorWindow.Open(avatar, pose);
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    // EditorGUILayout.PropertyField(serializedObject.FindProperty("_handPoses"));
                }

                if (!string.IsNullOrEmpty(avatar.PrefabGuid) && !avatar.gameObject.IsPrefab() && !UxrHandPoseEditorWindow.IsVisible && UxrEditorUtils.CenteredButton(ContentOpenHandPoseEditor, -1))
                {
                    UxrHandPoseEditorWindow.Open(avatar);
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (createPrefab)
            {
                if (UxrEditorUtils.CreateAvatarPrefab(avatar, "Save prefab", avatar.gameObject.name, out prefab, out GameObject newInstance))
                {
                    if (newInstance == null)
                    {
                        EditorUtility.DisplayDialog("Error", "The prefab was created but it could not be instantiated in the scene. Try doing it manually.", "OK");
                    }
                    else
                    {
                        Selection.activeGameObject = newInstance.gameObject;
                    }
                }
            }
        }

        /// <summary>
        ///     Draws visual guides on the avatar.
        /// </summary>
        private void OnSceneGUI()
        {
            UxrAvatar avatar = (UxrAvatar)serializedObject.targetObject;

            if (avatar == null)
            {
                return;
            }

            /*
            // Draw finger tips and finger print positions
            
            Color handlesColor = Handles.color;
            Handles.matrix = Matrix4x4.identity;

            foreach (UxrAvatarArmInfo arm in avatar.AvatarRigInfo.Arms)
            {
                foreach (UxrAvatarFingerInfo finger in arm.Fingers)
                {
                    Handles.color = ColorExt.ColorAlpha(Color.blue, UxrEditorUtils.HandlesAlpha);
                    Handles.DrawSolidDisc(finger.TipPosition, finger.TipDirection, 0.004f);
                    Handles.color = ColorExt.ColorAlpha(Color.green, UxrEditorUtils.HandlesAlpha);
                    Handles.DrawSolidDisc(finger.FingerPrintPosition, finger.FingerPrintDirection, 0.004f);
                }
            }

            Handles.color = handlesColor;
            */
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Refreshes the rig information.
        /// </summary>
        /// <param name="avatar">Avatar whose rig to refresh</param>
        private void RefreshRigSerializedProperty(UxrAvatar avatar)
        {
            // Head

            serializedObject.FindProperty("_rig._head._leftEye").objectReferenceValue  = avatar.AvatarRig.Head.LeftEye;
            serializedObject.FindProperty("_rig._head._rightEye").objectReferenceValue = avatar.AvatarRig.Head.RightEye;
            serializedObject.FindProperty("_rig._head._jaw").objectReferenceValue      = avatar.AvatarRig.Head.Jaw;
            serializedObject.FindProperty("_rig._head._head").objectReferenceValue     = avatar.AvatarRig.Head.Head;
            serializedObject.FindProperty("_rig._head._neck").objectReferenceValue     = avatar.AvatarRig.Head.Neck;

            // Body

            serializedObject.FindProperty("_rig._upperChest").objectReferenceValue = avatar.AvatarRig.UpperChest;
            serializedObject.FindProperty("_rig._chest").objectReferenceValue      = avatar.AvatarRig.Chest;
            serializedObject.FindProperty("_rig._spine").objectReferenceValue      = avatar.AvatarRig.Spine;
            serializedObject.FindProperty("_rig._hips").objectReferenceValue       = avatar.AvatarRig.Hips;

            // Left arm

            serializedObject.FindProperty("_rig._leftArm._clavicle").objectReferenceValue    = avatar.AvatarRig.LeftArm.Clavicle;
            serializedObject.FindProperty("_rig._leftArm._upperArm").objectReferenceValue    = avatar.AvatarRig.LeftArm.UpperArm;
            serializedObject.FindProperty("_rig._leftArm._forearm").objectReferenceValue     = avatar.AvatarRig.LeftArm.Forearm;
            serializedObject.FindProperty("_rig._leftArm._hand._wrist").objectReferenceValue = avatar.LeftHandBone;

            serializedObject.FindProperty("_rig._leftArm._hand._thumb._metacarpal").objectReferenceValue   = avatar.LeftHand.Thumb.Metacarpal;
            serializedObject.FindProperty("_rig._leftArm._hand._thumb._proximal").objectReferenceValue     = avatar.LeftHand.Thumb.Proximal;
            serializedObject.FindProperty("_rig._leftArm._hand._thumb._intermediate").objectReferenceValue = avatar.LeftHand.Thumb.Intermediate;
            serializedObject.FindProperty("_rig._leftArm._hand._thumb._distal").objectReferenceValue       = avatar.LeftHand.Thumb.Distal;

            serializedObject.FindProperty("_rig._leftArm._hand._index._metacarpal").objectReferenceValue   = avatar.LeftHand.Index.Metacarpal;
            serializedObject.FindProperty("_rig._leftArm._hand._index._proximal").objectReferenceValue     = avatar.LeftHand.Index.Proximal;
            serializedObject.FindProperty("_rig._leftArm._hand._index._intermediate").objectReferenceValue = avatar.LeftHand.Index.Intermediate;
            serializedObject.FindProperty("_rig._leftArm._hand._index._distal").objectReferenceValue       = avatar.LeftHand.Index.Distal;

            serializedObject.FindProperty("_rig._leftArm._hand._middle._metacarpal").objectReferenceValue   = avatar.LeftHand.Middle.Metacarpal;
            serializedObject.FindProperty("_rig._leftArm._hand._middle._proximal").objectReferenceValue     = avatar.LeftHand.Middle.Proximal;
            serializedObject.FindProperty("_rig._leftArm._hand._middle._intermediate").objectReferenceValue = avatar.LeftHand.Middle.Intermediate;
            serializedObject.FindProperty("_rig._leftArm._hand._middle._distal").objectReferenceValue       = avatar.LeftHand.Middle.Distal;

            serializedObject.FindProperty("_rig._leftArm._hand._ring._metacarpal").objectReferenceValue   = avatar.LeftHand.Ring.Metacarpal;
            serializedObject.FindProperty("_rig._leftArm._hand._ring._proximal").objectReferenceValue     = avatar.LeftHand.Ring.Proximal;
            serializedObject.FindProperty("_rig._leftArm._hand._ring._intermediate").objectReferenceValue = avatar.LeftHand.Ring.Intermediate;
            serializedObject.FindProperty("_rig._leftArm._hand._ring._distal").objectReferenceValue       = avatar.LeftHand.Ring.Distal;

            serializedObject.FindProperty("_rig._leftArm._hand._little._metacarpal").objectReferenceValue   = avatar.LeftHand.Little.Metacarpal;
            serializedObject.FindProperty("_rig._leftArm._hand._little._proximal").objectReferenceValue     = avatar.LeftHand.Little.Proximal;
            serializedObject.FindProperty("_rig._leftArm._hand._little._intermediate").objectReferenceValue = avatar.LeftHand.Little.Intermediate;
            serializedObject.FindProperty("_rig._leftArm._hand._little._distal").objectReferenceValue       = avatar.LeftHand.Little.Distal;

            // Right arm

            serializedObject.FindProperty("_rig._rightArm._clavicle").objectReferenceValue    = avatar.AvatarRig.RightArm.Clavicle;
            serializedObject.FindProperty("_rig._rightArm._upperArm").objectReferenceValue    = avatar.AvatarRig.RightArm.UpperArm;
            serializedObject.FindProperty("_rig._rightArm._forearm").objectReferenceValue     = avatar.AvatarRig.RightArm.Forearm;
            serializedObject.FindProperty("_rig._rightArm._hand._wrist").objectReferenceValue = avatar.RightHandBone;

            serializedObject.FindProperty("_rig._rightArm._hand._thumb._metacarpal").objectReferenceValue   = avatar.RightHand.Thumb.Metacarpal;
            serializedObject.FindProperty("_rig._rightArm._hand._thumb._proximal").objectReferenceValue     = avatar.RightHand.Thumb.Proximal;
            serializedObject.FindProperty("_rig._rightArm._hand._thumb._intermediate").objectReferenceValue = avatar.RightHand.Thumb.Intermediate;
            serializedObject.FindProperty("_rig._rightArm._hand._thumb._distal").objectReferenceValue       = avatar.RightHand.Thumb.Distal;

            serializedObject.FindProperty("_rig._rightArm._hand._index._metacarpal").objectReferenceValue   = avatar.RightHand.Index.Metacarpal;
            serializedObject.FindProperty("_rig._rightArm._hand._index._proximal").objectReferenceValue     = avatar.RightHand.Index.Proximal;
            serializedObject.FindProperty("_rig._rightArm._hand._index._intermediate").objectReferenceValue = avatar.RightHand.Index.Intermediate;
            serializedObject.FindProperty("_rig._rightArm._hand._index._distal").objectReferenceValue       = avatar.RightHand.Index.Distal;

            serializedObject.FindProperty("_rig._rightArm._hand._middle._metacarpal").objectReferenceValue   = avatar.RightHand.Middle.Metacarpal;
            serializedObject.FindProperty("_rig._rightArm._hand._middle._proximal").objectReferenceValue     = avatar.RightHand.Middle.Proximal;
            serializedObject.FindProperty("_rig._rightArm._hand._middle._intermediate").objectReferenceValue = avatar.RightHand.Middle.Intermediate;
            serializedObject.FindProperty("_rig._rightArm._hand._middle._distal").objectReferenceValue       = avatar.RightHand.Middle.Distal;

            serializedObject.FindProperty("_rig._rightArm._hand._ring._metacarpal").objectReferenceValue   = avatar.RightHand.Ring.Metacarpal;
            serializedObject.FindProperty("_rig._rightArm._hand._ring._proximal").objectReferenceValue     = avatar.RightHand.Ring.Proximal;
            serializedObject.FindProperty("_rig._rightArm._hand._ring._intermediate").objectReferenceValue = avatar.RightHand.Ring.Intermediate;
            serializedObject.FindProperty("_rig._rightArm._hand._ring._distal").objectReferenceValue       = avatar.RightHand.Ring.Distal;

            serializedObject.FindProperty("_rig._rightArm._hand._little._metacarpal").objectReferenceValue   = avatar.RightHand.Little.Metacarpal;
            serializedObject.FindProperty("_rig._rightArm._hand._little._proximal").objectReferenceValue     = avatar.RightHand.Little.Proximal;
            serializedObject.FindProperty("_rig._rightArm._hand._little._intermediate").objectReferenceValue = avatar.RightHand.Little.Intermediate;
            serializedObject.FindProperty("_rig._rightArm._hand._little._distal").objectReferenceValue       = avatar.RightHand.Little.Distal;

            // Left leg

            serializedObject.FindProperty("_rig._leftLeg._upperLeg").objectReferenceValue = avatar.AvatarRig.LeftLeg.UpperLeg;
            serializedObject.FindProperty("_rig._leftLeg._lowerLeg").objectReferenceValue = avatar.AvatarRig.LeftLeg.LowerLeg;
            serializedObject.FindProperty("_rig._leftLeg._foot").objectReferenceValue     = avatar.AvatarRig.LeftLeg.Foot;
            serializedObject.FindProperty("_rig._leftLeg._toes").objectReferenceValue     = avatar.AvatarRig.LeftLeg.Toes;

            // Right leg

            serializedObject.FindProperty("_rig._rightLeg._upperLeg").objectReferenceValue = avatar.AvatarRig.RightLeg.UpperLeg;
            serializedObject.FindProperty("_rig._rightLeg._lowerLeg").objectReferenceValue = avatar.AvatarRig.RightLeg.LowerLeg;
            serializedObject.FindProperty("_rig._rightLeg._foot").objectReferenceValue     = avatar.AvatarRig.RightLeg.Foot;
            serializedObject.FindProperty("_rig._rightLeg._toes").objectReferenceValue     = avatar.AvatarRig.RightLeg.Toes;
            
            // Clear torsion nodes if they exist
            
            if (avatar.AvatarRig.LeftArm.Forearm == null && avatar.AvatarRig.RightArm.Forearm == null)
            {
                avatar.GetComponentsInChildren<UxrWristTorsionIKSolver>().ForEach(Undo.DestroyObjectImmediate);
            }
            
            // Update standard avatar controller info

            UxrAvatarController avatarController = avatar.GetComponent<UxrAvatarController>();

            if (avatarController is UxrStandardAvatarController standardAvatarController)
            {
                SerializedObject   serializedAvatarController = new SerializedObject(standardAvatarController);
                SerializedProperty propIKSettings             = serializedAvatarController.FindProperty(UxrStandardAvatarControllerEditor.PropBodyIKSettings);

                serializedAvatarController.Update();
                
                // Try to set eye base height and forward offset

                if (avatar.AvatarRig.Head.LeftEye && avatar.AvatarRig.Head.RightEye)
                {
                    Vector3 eyeLeft  = avatar.transform.InverseTransformPoint(avatar.AvatarRig.Head.LeftEye.position);
                    Vector3 eyeRight = avatar.transform.InverseTransformPoint(avatar.AvatarRig.Head.RightEye.position);

                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesBaseHeight).floatValue    = (avatar.AvatarRig.Head.LeftEye.position.y + avatar.AvatarRig.Head.RightEye.position.y) * 0.5f - avatar.transform.position.y;
                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesForwardOffset).floatValue = (eyeLeft.z + eyeRight.z) * 0.5f + 0.02f;
                }
                else if (avatar.AvatarRig.Head.Head != null)
                {
                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesBaseHeight).floatValue    = (avatar.AvatarRig.Head.Head.position.y - avatar.transform.position.y) + 0.1f;
                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesForwardOffset).floatValue = 0.15f;
                }
                
                // If a neck wasn't found, try to set neck base height and forward setting using the head node 

                if (avatar.AvatarRig.Head.Neck == null && avatar.AvatarRig.Head.Head != null)
                {
                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyNeckBaseHeight).floatValue    = (avatar.AvatarRig.Head.Head.position.y - avatar.transform.position.y) - 0.1f;
                    propIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyNeckForwardOffset).floatValue = 0.0f;
                }

                serializedAvatarController.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentFix                   { get; } = new GUIContent("Fix",                      "Fixes the issue above automatically");
        private GUIContent ContentBigHandsIntegration   { get; } = new GUIContent("Use Big hands",            "Adds the BigHandsIntegration prefab to the avatar, which includes functionality to use tracking/input devices and also interact with the environment using manipulation, locomotion etc.");
        private GUIContent ContentSmallHandsIntegration { get; } = new GUIContent("Use Small hands",          "Adds the SmallHandsIntegration prefab to the avatar, which includes functionality to use tracking/input devices and also interact with the environment using manipulation, locomotion etc.");
        private GUIContent ContentCreatePrefab          { get; } = new GUIContent("Create Prefab",            "Creates a prefab for this avatar");
        private GUIContent ContentAutoFillFromAnimator  { get; } = new GUIContent("Autofill from Animator",   "");
        private GUIContent ContentTryToFillGuessing     { get; } = new GUIContent("Try to fill",              "");
        private GUIContent ContentClearRig              { get; } = new GUIContent("Clear",                    "");
        private GUIContent ContentSelectPrefab          { get; } = new GUIContent("Select prefab",            "");
        private GUIContent ContentSelectPoseAsset       { get; } = new GUIContent("Select",                   "");
        private GUIContent ContentOpenPose              { get; } = new GUIContent("Open",                     "");
        private GUIContent ContentOpenHandPoseEditor    { get; } = new GUIContent("Open Hand Pose Editor...", "");
        private GUIContent ContentHead                  { get; } = new GUIContent("Head (optional)",          "");
        private GUIContent ContentLeftHand              { get; } = new GUIContent("Left Hand",                "");
        private GUIContent ContentRightHand             { get; } = new GUIContent("Right Hand",               "");
        private GUIContent ContentPrefab                { get; } = new GUIContent("Prefab",                   "");
        private GUIContent ContentParentPrefab          { get; } = new GUIContent("Parent Prefab",            "");
        private GUIContent ContentAvatarMode            { get; } = new GUIContent("Avatar Mode",              "Local Avatars are updated automatically using the headset and controllers, while UpdateExternally avatars are not updated and act as puppets that should be updated manually. They are useful in multiplayer applications for the remote avatars.");
        private GUIContent ContentRenderMode            { get; } = new GUIContent("Render Mode",              "Controls the way the avatar will be rendered. Avatar mode is the default but for tutorials, menus and other cases rendering the controllers may be more convenient");
        private GUIContent ContentShowControllerHands   { get; } = new GUIContent("Show Controller Hands",    "If the render mode is set to render the controllers, will the hands that come with them also be rendered? Set it to false to render the controllers only. Set it to true if you want some fancy hands with IK on top of them");
        private GUIContent ContentAvatarRenderers       { get; } = new GUIContent("Avatar Renderers",         "The list of renderers that make up the avatar when rendered in-game. This is used to switch between rendering the avatar or the controllers or both");
        private GUIContent ContentRigType               { get; } = new GUIContent("Rig Type",                 "");
        private GUIContent ContentRig                   { get; } = new GUIContent("Rig",                      "");

        private const int    ButtonWidth               = 140;
        private const int    FixButtonWidth            = 120;
        private const string NeedsPrefabVariant        = "It is recommended to create a prefab variant in your project instead of using an UltimateXR prefab directly. When UltimateXR is updated, all the prefabs could be overwritten and changes would be lost. Using a prefab variant in your project allows to keep all changes and still get the improvements included in the updates.";
        private const string NeedsCameraNewHelp        = "The avatar has no Camera in its hierarchy. It needs a Camera to render the view in VR";
        private const string NeedsCameraReparentHelp   = "The scene camera needs to be placed in the avatar hierarchy to render the view correctly in VR";
        private const string NeedsCameraParentHelp     = "In order to be able to reposition the camera at runtime please parent the camera to a child of the avatar root GameObject instead of being a child directly. We will call this the Camera Controller.";
        private const string NeedsAvatarControllerHelp = "The avatar needs an UxrAvatarController component that will take care of updating all its components. You can add an UxrStandardAvatarController component or provide your own for advanced custom avatar handling.";
        private const string NeedsFingerSetup          = "The avatar rig has no hand or finger bone references assigned yet. They are required to use hand poses and set up hand integrations.";
        private const string NeedsIntegrationHelp      = "The avatar has no support for tracking/input devices yet. You can use one of the below integrations to leverage this work for you. The hand size will only determine the type of hands that will be shown when the input controllers are visualized instead of the avatar hands.";
        private const string BigHandsIntegrationGuid   = "2f7a5d0166ab0d041bed38f7cc6affef";
        private const string SmallHandsIntegrationGuid = "dde1cc2a360069149a781772e8410006";

        private SerializedProperty _propertyPrefabGuid;
        private SerializedProperty _propertyParentPrefab;
        private SerializedProperty _propertyAvatarMode;
        private SerializedProperty _propertyRenderMode;
        private SerializedProperty _propertyShowControllerHands;
        private SerializedProperty _propertyAvatarRenderers;
        private SerializedProperty _propertyRigType;
        private SerializedProperty _propertyRigExpandedInitialized;
        private SerializedProperty _propertyRigFoldout;
        private SerializedProperty _propertyRig;
        private SerializedProperty _propertyHandPosesFoldout;
        private SerializedProperty _propertyHandPoses;

        private bool _foldoutGeneral   = true;
        private bool _foldoutRendering = true;

        #endregion
    }
}