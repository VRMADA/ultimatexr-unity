// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Avatar.Editor;
using UltimateXR.Core;
using UltimateXR.Editor;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation.HandPoses;
using UltimateXR.Manipulation.HandPoses.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateXR.Manipulation.Editor
{
    /// <summary>
    ///     Custom inspector editor for the <see cref="UxrGrabbableObject" /> component.
    /// </summary>
    [CustomEditor(typeof(UxrGrabbableObject))]
    [CanEditMultipleObjects]
    public class UxrGrabbableObjectEditor : UnityEditor.Editor
    {
        #region Public Types & Data

        public const string PropertyGrabPoint            = "_grabPoint";
        public const string PropertyAdditionalGrabPoints = "_additionalGrabPoints";

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to get an avatar in the scene that comes from the given prefab.
        /// </summary>
        /// <param name="avatarPrefab">Registered avatar prefab</param>
        /// <returns>First avatar found that comes from the given prefab</returns>
        public static UxrAvatar TryGetSceneAvatar(GameObject avatarPrefab)
        {
            if (avatarPrefab == null)
            {
                return null;
            }

            UxrAvatar[] avatars = FindObjectsOfType<UxrAvatar>();

            // Look first for the exact match

            foreach (UxrAvatar avatar in avatars)
            {
                if (avatar.PrefabGuid == UxrAvatarEditorExt.GetGuid(avatarPrefab))
                {
                    return avatar;
                }
            }

            // Look for the prefab in any of the parent prefabs

            foreach (UxrAvatar avatar in avatars)
            {
                if (avatar.IsInPrefabChain(avatarPrefab))
                {
                    return avatar;
                }
            }

            return null;
        }

        /// <summary>
        ///     Refreshes the preview poses for a specific avatar and hand pose. Designed to be called
        ///     from the Hand Pose Editor to keep the preview meshes updated. Preview hand pose meshes that share the same prefab
        ///     will be updated accordingly.
        /// </summary>
        /// <param name="avatar">The avatar to update the poses for</param>
        /// <param name="handPoseAsset">The pose asset to look for and update</param>
        public static void RefreshGrabPoseMeshes(UxrAvatar avatar, UxrHandPoseAsset handPoseAsset)
        {
            foreach (KeyValuePair<SerializedObject, UxrGrabbableObjectEditor> editorPair in s_openEditors)
            {
                editorPair.Value.RefreshGrabPoseMeshes(handPoseAsset, avatar.GetPrefab(), editorPair.Value._propPreviewGrabPosesMode);
            }
        }

        /// <summary>
        ///     Replaces a preview mesh in a grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to process</param>
        /// <param name="oldMesh">Old mesh to replace</param>
        /// <param name="newMesh">New mesh to assign</param>
        public static void ReplacePreviewMesh(UxrGrabbableObject grabbableObject, Mesh oldMesh, Mesh newMesh)
        {
            UxrGrabbableObjectPreviewMesh[] previewMeshComponents = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>();

            foreach (UxrGrabbableObjectPreviewMesh previewMeshComponent in previewMeshComponents)
            {
                MeshFilter meshFilter = previewMeshComponent.GetComponent<MeshFilter>();

                if (ReferenceEquals(meshFilter.sharedMesh, oldMesh))
                {
                    meshFilter.sharedMesh = newMesh;
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties and creates temporal grab pose editor objects
        /// </summary>
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;

            _propManipulationMode                   = serializedObject.FindProperty("_manipulationMode");
            _propIgnoreGrabbableParentDependency    = serializedObject.FindProperty("_ignoreGrabbableParentDependency");
            _propControlParentDirection             = serializedObject.FindProperty("_controlParentDirection");
            _propPriority                           = serializedObject.FindProperty("_priority");
            _propRigidBodySource                    = serializedObject.FindProperty("_rigidBodySource");
            _propRigidBodyDynamicOnRelease          = serializedObject.FindProperty("_rigidBodyDynamicOnRelease");
            _propVerticalReleaseMultiplier          = serializedObject.FindProperty("_verticalReleaseMultiplier");
            _propHorizontalReleaseMultiplier        = serializedObject.FindProperty("_horizontalReleaseMultiplier");
            _propNeedsTwoHandsToRotate              = serializedObject.FindProperty("_needsTwoHandsToRotate");
            _propAllowMultiGrab                     = serializedObject.FindProperty("_allowMultiGrab");
            _propPreviewGrabPosesMode               = serializedObject.FindProperty("_previewGrabPosesMode");
            _propPreviewPosesRegenerationType       = serializedObject.FindProperty("_previewPosesRegenerationType");
            _propPreviewPosesRegenerationIndex      = serializedObject.FindProperty("_previewPosesRegenerationIndex");
            _propFirstGrabPointIsMain               = serializedObject.FindProperty("_firstGrabPointIsMain");
            _propGrabPoint                          = serializedObject.FindProperty(PropertyGrabPoint);
            _propAdditionalGrabPoints               = serializedObject.FindProperty(PropertyAdditionalGrabPoints);
            _propUseParenting                       = serializedObject.FindProperty("_useParenting");
            _propAutoCreateStartAnchor              = serializedObject.FindProperty("_autoCreateStartAnchor");
            _propStartAnchor                        = serializedObject.FindProperty("_startAnchor");
            _propTag                                = serializedObject.FindProperty("_tag");
            _propDropAlignTransformUseSelf          = serializedObject.FindProperty("_dropAlignTransformUseSelf");
            _propDropAlignTransform                 = serializedObject.FindProperty("_dropAlignTransform");
            _propDropSnapMode                       = serializedObject.FindProperty("_dropSnapMode");
            _propDropProximityTransformUseSelf      = serializedObject.FindProperty("_dropProximityTransformUseSelf");
            _propDropProximityTransform             = serializedObject.FindProperty("_dropProximityTransform");
            _propTranslationConstraintMode          = serializedObject.FindProperty("_translationConstraintMode");
            _propRestrictToBox                      = serializedObject.FindProperty("_restrictToBox");
            _propRestrictToSphere                   = serializedObject.FindProperty("_restrictToSphere");
            _propTranslationLimitsMin               = serializedObject.FindProperty("_translationLimitsMin");
            _propTranslationLimitsMax               = serializedObject.FindProperty("_translationLimitsMax");
            _propTranslationLimitsReferenceIsParent = serializedObject.FindProperty("_translationLimitsReferenceIsParent");
            _propTranslationLimitsParent            = serializedObject.FindProperty("_translationLimitsParent");
            _propRotationConstraintMode             = serializedObject.FindProperty("_rotationConstraintMode");
            _propRotationAngleLimitsMin             = serializedObject.FindProperty("_rotationAngleLimitsMin");
            _propRotationAngleLimitsMax             = serializedObject.FindProperty("_rotationAngleLimitsMax");
            _propRotationLimitsReferenceIsParent    = serializedObject.FindProperty("_rotationLimitsReferenceIsParent");
            _propRotationLimitsParent               = serializedObject.FindProperty("_rotationLimitsParent");
            _propLockedGrabReleaseDistance          = serializedObject.FindProperty("_lockedGrabReleaseDistance");
            _propTranslationResistance              = serializedObject.FindProperty("_translationResistance");
            _propRotationResistance                 = serializedObject.FindProperty("_rotationResistance");

            // Try to remember the grip selection we have if possible, considering we may start the inspector in multi-editing mode

            List<GameObject> commonAvatars = GetCommonSelectedAvatars();

            if (serializedObject.targetObjects.Length > 1)
            {
                bool       allSameGrip          = true;
                GameObject selectedAvatarPrefab = (serializedObject.targetObjects[0] as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabForGrips();

                for (int i = 1; i < serializedObject.targetObjects.Length; ++i)
                {
                    if ((serializedObject.targetObjects[i] as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabForGrips() != selectedAvatarPrefab)
                    {
                        allSameGrip = false;
                        break;
                    }
                }

                if (allSameGrip == false)
                {
                    SetSelectedAvatarForGrips(commonAvatars.Count > 0 ? commonAvatars[0] : null);
                    _selectedAvatarGripPoseIndex = commonAvatars.Count > 0 ? 1 : 0;
                }
                else
                {
                    _selectedAvatarGripPoseIndex = commonAvatars.IndexOf(selectedAvatarPrefab) + 1; // +1 because of default grip
                }
            }
            else
            {
                _selectedAvatarGripPoseIndex = commonAvatars.IndexOf(SelectedAvatarForGrips);

                if (_selectedAvatarGripPoseIndex == -1)
                {
                    SetSelectedAvatarForGrips(null);
                    _selectedAvatarGripPoseIndex = 0;
                }
                else
                {
                    _selectedAvatarGripPoseIndex += 1; // +1 because of default grip
                }
            }

            ComputeGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode);

            // Add a static reference so that we can update the grip preview meshes from the Hand Pose Editor

            if (s_openEditors.ContainsKey(serializedObject) == false)
            {
                s_openEditors.Add(serializedObject, this);
            }
        }

        /// <summary>
        ///     Destroys temporal grab pose editor objects
        /// </summary>
        private void OnDisable()
        {
            DestroyGrabPoseTempEditorObjects();

            if (s_openEditors.ContainsKey(serializedObject))
            {
                s_openEditors.Remove(serializedObject);
            }

            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        ///     Draws the custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Help

            if (!EditorUtility.IsPersistent((serializedObject.targetObject as UxrGrabbableObject).gameObject))
            {
                if (!UxrEditorUtils.CheckAvatarInScene())
                {
                    EditorGUILayout.HelpBox(NeedsUxrAvatar, MessageType.Warning);
                }
                else if (!UxrEditorUtils.CheckAvatarInSceneWithGrabbing())
                {
                    EditorGUILayout.HelpBox(NeedsUxrAvatarWithGrabbing, MessageType.Warning);
                }
                else if (!UxrEditorUtils.CheckAvatarInSceneWithGrabController())
                {
                    EditorGUILayout.HelpBox(NeedsUxrAvatarWithGrabController, MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(DefaultHelp, MessageType.Info);
                }
            }

            // General parameters

            UxrGrabbableObject grabbableObject           = (UxrGrabbableObject)serializedObject.targetObject;
            UxrGrabPointShape  grabPointShape            = grabbableObject.GetComponent<UxrGrabPointShape>();
            Transform          grabbableParentDependency = grabbableObject.GrabbableParentDependency;

            if (grabbableParentDependency != null)
            {
                EditorGUILayout.Space();

                if (_propTranslationConstraintMode.enumValueIndex != (int)UxrTranslationConstraintMode.Free && _propIgnoreGrabbableParentDependency.boolValue == false)
                {
                    EditorGUILayout.HelpBox("Object will be constrained by " + grabbableParentDependency.name + " because this object has translation constraints applied. You may even use it to control its direction with the parameter below",
                                            MessageType.Info);
                    EditorGUILayout.PropertyField(_propControlParentDirection,
                                                  new GUIContent("Control " + grabbableParentDependency.name + " Direction", "Allows to control the direction of the parent grabbable object when this object is grabbed"));
                }

                if (_propTranslationConstraintMode.enumValueIndex != (int)UxrTranslationConstraintMode.Free)
                {
                    EditorGUILayout.PropertyField(_propIgnoreGrabbableParentDependency, ContentIgnoreGrabbableParentDependency);
                }
            }

            EditorGUILayout.Space();

            _foldoutManipulation = UxrEditorUtils.FoldoutStylish("Manipulation", _foldoutManipulation);

            if (_foldoutManipulation)
            {
                EditorGUILayout.PropertyField(_propManipulationMode, ContentManipulationMode);
            }

            bool usesGeneralParametersFoldout = false;

            if (!grabbableObject.UsesGrabbableParentDependency && _propManipulationMode.enumValueIndex == (int)UxrManipulationMode.GrabAndMove)
            {
                EditorGUILayout.Space();
                _foldoutGeneralParameters = UxrEditorUtils.FoldoutStylish("General parameters", _foldoutGeneralParameters);

                usesGeneralParametersFoldout = true;

                if (_foldoutGeneralParameters)
                {
                    EditorGUILayout.PropertyField(_propPriority,        ContentPriority);
                    EditorGUILayout.PropertyField(_propRigidBodySource, ContentRigidBodySource);

                    if (_propRigidBodySource.objectReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(_propRigidBodyDynamicOnRelease, ContentRigidBodyDynamicOnRelease);

                        if (_propRigidBodyDynamicOnRelease.boolValue)
                        {
                            EditorGUILayout.PropertyField(_propVerticalReleaseMultiplier,   ContentVerticalReleaseMultiplier);
                            EditorGUILayout.PropertyField(_propHorizontalReleaseMultiplier, ContentHorizontalReleaseMultiplier);
                        }
                    }
                }
            }
            else
            {
                if (_foldoutManipulation)
                {
                    EditorGUILayout.PropertyField(_propPriority, ContentPriority);
                }
            }

            if (!grabbableObject.UsesGrabbableParentDependency && _propManipulationMode.enumValueIndex == (int)UxrManipulationMode.RotateAroundAxis && _propAdditionalGrabPoints.arraySize > 0)
            {
                if (_foldoutManipulation)
                {
                    EditorGUILayout.PropertyField(_propNeedsTwoHandsToRotate, ContentNeedsTwoHandsToRotate);
                }
            }

            if (_propAdditionalGrabPoints.arraySize > 0 || grabPointShape != null)
            {
                bool show = usesGeneralParametersFoldout ? _foldoutGeneralParameters : _foldoutManipulation;

                if (show)
                {
                    EditorGUILayout.PropertyField(_propAllowMultiGrab, ContentAllowMultiGrab);
                }
            }

            EditorGUILayout.Space();

            // These will indicate operations after ApplyModifiedProperties() because we cannot use serialized properties with them:

            bool       removeSelectedAvatar = false;
            bool       changeSelectedAvatar = false;
            GameObject newSelectedAvatar    = null;

            // Avatar grips

            UxrAvatar        avatarRegister = null;
            List<GameObject> commonAvatars  = GetCommonSelectedAvatars();

            _foldoutAvatarGrips = UxrEditorUtils.FoldoutStylish("Avatar grips", _foldoutAvatarGrips);

            if (_foldoutAvatarGrips)
            {
                avatarRegister = EditorGUILayout.ObjectField(ContentRegisterAvatar, null, typeof(UxrAvatar), true) as UxrAvatar;

                if (commonAvatars.Count > 0)
                {
                    Color guiColor = GUI.color;

                    if (_selectedAvatarGripPoseIndex > 0)
                    {
                        GUI.color = GUIColorAvatarControllerParameter;
                    }

                    EditorGUI.BeginChangeCheck();
                    IEnumerable<string> avatarList = new List<string> { UxrGrabbableObject.DefaultAvatarName }.Concat(commonAvatars.Select(a => a.name));
                    _selectedAvatarGripPoseIndex = EditorGUILayout.Popup(ContentSelectAvatar, _selectedAvatarGripPoseIndex, UxrEditorUtils.ToGUIContentArray(avatarList));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (_selectedAvatarGripPoseIndex == 0)
                        {
                            changeSelectedAvatar = true;
                        }
                        else
                        {
                            changeSelectedAvatar = true;
                            newSelectedAvatar    = commonAvatars[_selectedAvatarGripPoseIndex - 1]; // -1 because of default avatar
                        }
                    }

                    GUI.color = guiColor;
                }
                else
                {
                    _selectedAvatarGripPoseIndex = 0;
                    changeSelectedAvatar         = true;
                }

                if (commonAvatars.Count > 0 && _selectedAvatarGripPoseIndex > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Remove Selected Avatar Grip", GUILayout.ExpandWidth(false)))
                    {
                        removeSelectedAvatar = true;
                    }

                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                // Preview meshes

                if (commonAvatars.Count > 0)
                {
                    EditorGUI.BeginChangeCheck();
                    int previewGrabPosesModeIntValue = EditorGUILayout.Popup(ContentPreviewGrabPosesMode, GetPreviewGrabPosesIndexFromIntValue(_propPreviewGrabPosesMode.intValue), GetPreviewGrabPosesModeStrings());
                    _propPreviewGrabPosesMode.intValue = GetPreviewGrabPosesIntValueFromIndex(previewGrabPosesModeIntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ComputeGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode);
                    }

                    EditorGUILayout.Space();
                }
            }

            // Grab points

            IEnumerable<Transform> listGrabAlignTransformsBefore = Enumerable.Empty<Transform>();
            IEnumerable<Transform> listGrabAlignTransformsAfter  = Enumerable.Empty<Transform>();

            int additionalPointsBefore = _propAdditionalGrabPoints.arraySize;
            int additionalPointsAfter  = additionalPointsBefore;

            bool foldOutGrabPointsBefore = _foldoutGrabPoints;
            _foldoutGrabPoints = UxrEditorUtils.FoldoutStylish("Grab points", _foldoutGrabPoints);
            if (foldOutGrabPointsBefore != _foldoutGrabPoints)
            {
                MarkRegenerationRequired();
            }

            if (_foldoutGrabPoints)
            {
                EditorGUI.indentLevel += 2;

                foreach (Object targetObject in serializedObject.targetObjects)
                {
                    listGrabAlignTransformsBefore = listGrabAlignTransformsBefore.Concat((targetObject as UxrGrabbableObject).Editor_GetAllRegisteredAvatarGrabAlignTransforms());
                }

                if (_propAdditionalGrabPoints.arraySize > 0)
                {
                    EditorGUILayout.PropertyField(_propFirstGrabPointIsMain, ContentFirstGrabPointIsMain, true);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_propGrabPoint, ContentGrabPoint, true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_propPreviewPosesRegenerationType.intValue == (int)UxrPreviewPoseRegeneration.None)
                    {
                        // A parameter changed but the regeneration flag was not set. This will be used to detect foldout toggles:
                        MarkRegenerationRequired();
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_propAdditionalGrabPoints, ContentAdditionalGrabPoints, true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_propPreviewPosesRegenerationType.intValue == (int)UxrPreviewPoseRegeneration.None)
                    {
                        // A parameter changed but the regeneration flag was not set. This will be used to detect foldout toggles:
                        MarkRegenerationRequired();
                    }

                    // Keep track of added/removed grab points:
                    additionalPointsAfter = _propAdditionalGrabPoints.arraySize;
                }

                EditorGUI.indentLevel -= 2;
            }

            // Object placement parameters

            if (!grabbableObject.UsesGrabbableParentDependency && _propManipulationMode.enumValueIndex == (int)UxrManipulationMode.GrabAndMove)
            {
                EditorGUILayout.Space();
                _foldoutObjectPlacing = UxrEditorUtils.FoldoutStylish("Object placement", _foldoutObjectPlacing);

                if (_foldoutObjectPlacing)
                {
                    EditorGUILayout.PropertyField(_propUseParenting,          ContentUseParenting);
                    EditorGUILayout.PropertyField(_propAutoCreateStartAnchor, ContentAutoCreateStartAnchor);

                    if (!_propAutoCreateStartAnchor.boolValue)
                    {
                        EditorGUILayout.PropertyField(_propStartAnchor, ContentStartAnchor);
                    }

                    EditorGUILayout.PropertyField(_propTag, ContentTag);

                    int popup = EditorGUILayout.Popup("Anchor Snap Transform", _propDropAlignTransformUseSelf.boolValue ? 0 : 1, new[] { "Use self transform", "Use other transform" });

                    if (popup == 1)
                    {
                        EditorGUILayout.PropertyField(_propDropAlignTransform, ContentDropAlignTransform);
                        _propDropAlignTransformUseSelf.boolValue = false;
                    }
                    else
                    {
                        _propDropAlignTransformUseSelf.boolValue = true;
                    }

                    EditorGUILayout.PropertyField(_propDropSnapMode, ContentDropSnapMode);

                    popup = EditorGUILayout.Popup(ContentDropProximityTransform, _propDropProximityTransformUseSelf.boolValue ? 0 : 1, new[] { "Use self transform", "Use other transform" });

                    if (popup == 1)
                    {
                        EditorGUILayout.PropertyField(_propDropProximityTransform);
                        _propDropProximityTransformUseSelf.boolValue = false;
                    }
                    else
                    {
                        _propDropProximityTransformUseSelf.boolValue = true;
                    }
                }
            }

            // Constraints

            EditorGUILayout.Space();
            _foldoutConstraints = UxrEditorUtils.FoldoutStylish("Constraints", _foldoutConstraints);

            if (_foldoutConstraints)
            {
                if (_propManipulationMode.enumValueIndex == (int)UxrManipulationMode.GrabAndMove)
                {
                    EditorGUILayout.PropertyField(_propTranslationConstraintMode, ContentTranslationConstraintMode);
                }

                if (_propManipulationMode.enumValueIndex == (int)UxrManipulationMode.RotateAroundAxis ||
                    _propTranslationConstraintMode.enumValueIndex == (int)UxrTranslationConstraintMode.RestrictLocalOffset)
                {
                    EditorGUILayout.PropertyField(_propTranslationLimitsMin,               ContentTranslationLimitsMin);
                    EditorGUILayout.PropertyField(_propTranslationLimitsMax,               ContentTranslationLimitsMax);
                    EditorGUILayout.PropertyField(_propTranslationLimitsReferenceIsParent, ContentTranslationLimitsReferenceIsParent);

                    if (_propTranslationLimitsReferenceIsParent.boolValue == false)
                    {
                        EditorGUILayout.PropertyField(_propTranslationLimitsParent, ContentTranslationLimitsParent);
                    }
                }
                else if (_propTranslationConstraintMode.enumValueIndex == (int)UxrTranslationConstraintMode.RestrictToBox)
                {
                    EditorGUILayout.PropertyField(_propRestrictToBox, ContentRestrictToBox);
                }
                else if (_propTranslationConstraintMode.enumValueIndex == (int)UxrTranslationConstraintMode.RestrictToSphere)
                {
                    EditorGUILayout.PropertyField(_propRestrictToSphere, ContentRestrictToSphere);
                }

                if (_propManipulationMode.enumValueIndex == (int)UxrManipulationMode.GrabAndMove)
                {
                    EditorGUILayout.PropertyField(_propRotationConstraintMode, ContentRotationConstraintMode);
                }

                if (_propManipulationMode.enumValueIndex == (int)UxrManipulationMode.RotateAroundAxis ||
                    _propRotationConstraintMode.enumValueIndex == (int)UxrRotationConstraintMode.RestrictLocalRotation)
                {
                    EditorGUILayout.PropertyField(_propRotationAngleLimitsMin,          ContentRotationAngleLimitsMin);
                    EditorGUILayout.PropertyField(_propRotationAngleLimitsMax,          ContentRotationAngleLimitsMax);
                    EditorGUILayout.PropertyField(_propRotationLimitsReferenceIsParent, ContentRotationLimitsReferenceIsParent);

                    if (_propRotationLimitsReferenceIsParent.boolValue == false)
                    {
                        EditorGUILayout.PropertyField(_propRotationLimitsParent, ContentRotationLimitsParent);
                    }
                }

                if ((_propManipulationMode.enumValueIndex == (int)UxrManipulationMode.GrabAndMove && _propAdditionalGrabPoints.arraySize > 0) || !grabbableObject.UsesGrabbableParentDependency)
                {
                    EditorGUILayout.PropertyField(_propLockedGrabReleaseDistance, ContentLockedGrabReleaseDistance);
                }

                EditorGUILayout.Slider(_propTranslationResistance, 0.0f, 1.0f, ContentTranslationResistance);
                EditorGUILayout.Slider(_propRotationResistance,    0.0f, 1.0f, ContentRotationResistance);
            }

            // Added new grab points? Set their values from previous grab points to make life easier

            if (additionalPointsAfter > additionalPointsBefore)
            {
                for (int i = additionalPointsBefore; i < additionalPointsAfter; ++i)
                {
                    if (i == 0)
                    {
                        UxrGrabbableObjectGrabPointInfoDrawer.CopyRelevantProperties(_propGrabPoint, _propAdditionalGrabPoints.GetArrayElementAtIndex(i), true);
                    }
                    else if (i > 0)
                    {
                        UxrGrabbableObjectGrabPointInfoDrawer.CopyRelevantProperties(_propAdditionalGrabPoints.GetArrayElementAtIndex(i - 1), _propAdditionalGrabPoints.GetArrayElementAtIndex(i), true);
                    }
                }
            }

            // Apply modified properties

            serializedObject.ApplyModifiedProperties();

            // After modified properties were applied, check if we need to add/remove avatars from the selection

            if (avatarRegister)
            {
                // New avatar registering was requested

                if (RegisterNewAvatarGrip(ref avatarRegister))
                {
                    commonAvatars = GetCommonSelectedAvatars();
                    if (commonAvatars.Contains(avatarRegister.GetPrefab()))
                    {
                        _selectedAvatarGripPoseIndex = commonAvatars.IndexOf(avatarRegister.GetPrefab()) + 1; // + 1 Because of default avatar
                        changeSelectedAvatar         = true;
                        newSelectedAvatar            = commonAvatars[_selectedAvatarGripPoseIndex - 1];
                    }
                }
            }

            if (removeSelectedAvatar)
            {
                RemoveAvatarGrip(commonAvatars[_selectedAvatarGripPoseIndex - 1].GetComponent<UxrAvatar>());          // -1 because of default avatar
                _selectedAvatarGripPoseIndex = Mathf.Clamp(_selectedAvatarGripPoseIndex - 2, 0, commonAvatars.Count); // -2 because of default avatar and removed one

                changeSelectedAvatar = true;
                newSelectedAvatar    = _selectedAvatarGripPoseIndex > 0 && _selectedAvatarGripPoseIndex <= commonAvatars.Count ? commonAvatars[_selectedAvatarGripPoseIndex - 1] : null; // -1 because of default value
            }

            if (changeSelectedAvatar)
            {
                SetSelectedAvatarForGrips(newSelectedAvatar);
                ComputeGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode);
            }

            // Get current align transforms

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                listGrabAlignTransformsAfter = listGrabAlignTransformsAfter.Concat((targetObject as UxrGrabbableObject).Editor_GetAllRegisteredAvatarGrabAlignTransforms());
            }

            // Check for deleted align transforms

            foreach (Transform alignTransform in listGrabAlignTransformsBefore)
            {
                if (listGrabAlignTransformsAfter.Contains(alignTransform) == false)
                {
                    UxrGrabbableObjectSnapTransform alignTransformComponent = alignTransform.GetComponent<UxrGrabbableObjectSnapTransform>();

                    if (alignTransformComponent != null)
                    {
                        DestroyImmediate(alignTransformComponent);
                    }
                }
            }

            // Check for added align transforms

            foreach (Transform t in listGrabAlignTransformsAfter)
            {
                t.gameObject.GetOrAddComponent<UxrGrabbableObjectSnapTransform>();
            }

            // Check if grab points marked the dirty flag

            if (_propPreviewPosesRegenerationType.intValue != (int)UxrPreviewPoseRegeneration.None)
            {
                if (_propPreviewPosesRegenerationType.intValue == (int)UxrPreviewPoseRegeneration.OnlyBlend)
                {
                    RefreshGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode, _propPreviewPosesRegenerationIndex.intValue);
                }
                else
                {
                    ComputeGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode);
                }

                MarkRegenerationRequired(UxrPreviewPoseRegeneration.None);
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Recomputes the preview poses
        /// </summary>
        private void OnUndoRedo()
        {
            ComputeGrabPoseMeshes(SelectedAvatarForGrips, _propPreviewGrabPosesMode);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Notifies that the preview grab pose meshes should be recomputed.
        /// </summary>
        /// <param name="regeneration">The required regeneration</param>
        /// <param name="grabPointIndex">
        ///     -1 to regenerate all grab point preview meshes, otherwise it will tell which one to
        ///     regenerate
        /// </param>
        private void MarkRegenerationRequired(UxrPreviewPoseRegeneration regeneration = UxrPreviewPoseRegeneration.Complete, int grabPointIndex = -1)
        {
            _propPreviewPosesRegenerationType.intValue  = (int)regeneration;
            _propPreviewPosesRegenerationIndex.intValue = regeneration != UxrPreviewPoseRegeneration.None ? grabPointIndex : -1;
        }

        /// <summary>
        ///     Gets the list of common registered avatar prefabs for all selected GameObjects in the scene.
        /// </summary>
        /// <returns>
        ///     List of prefabs registered for grips that are common to all selected GameObjects with the
        ///     <see cref="UxrGrabbableObject" /> component
        /// </returns>
        private List<GameObject> GetCommonSelectedAvatars()
        {
            List<string> commonAvatarGuids = (serializedObject.targetObjects[0] as UxrGrabbableObject).Editor_GetRegisteredAvatarsGuids();

            for (int i = 1; i < serializedObject.targetObjects.Length; ++i)
            {
                List<string> targetAvatars = (serializedObject.targetObjects[i] as UxrGrabbableObject).Editor_GetRegisteredAvatarsGuids();
                commonAvatarGuids = commonAvatarGuids.Intersect(targetAvatars).ToList();
            }

            List<GameObject> prefabs = commonAvatarGuids.Select(a =>
                                                                {
                                                                    UxrAvatar avatarPrefab = UxrAvatarEditorExt.GetFromGuid(a);
                                                                    if (avatarPrefab != null)
                                                                    {
                                                                        return avatarPrefab.gameObject;
                                                                    }
                                                                    return null;
                                                                }).Where(g => g != null).ToList();

            prefabs.Sort((o1, o2) => string.Compare(o1.name, o2.name, StringComparison.Ordinal));
            return prefabs;
        }

        /// <summary>
        ///     Sets the currently selected avatar to edit the custom grip information for.
        /// </summary>
        /// <param name="avatarPrefab">The prefab to select</param>
        private void SetSelectedAvatarForGrips(GameObject avatarPrefab)
        {
            for (int i = 0; i < serializedObject.targetObjects.Length; ++i)
            {
                (serializedObject.targetObjects[i] as UxrGrabbableObject).Editor_SetSelectedAvatarForGrips(avatarPrefab);
            }
        }

        /// <summary>
        ///     Registers a new avatar for custom grip information. We need this method to support multi-editing since part of the
        ///     selection may have it already registered and part not.
        /// </summary>
        /// <param name="avatar">
        ///     Avatar to register. The avatar should have a prefab source, which is what will actually be
        ///     registered. If it doesn't have a prefab source it will prompt the user to create a new prefab.
        ///     If a new prefab was created, this parameter will return the new prefab instance in the scene.
        /// </param>
        /// <returns>Whether the avatar was registered successfully</returns>
        private bool RegisterNewAvatarGrip(ref UxrAvatar avatar)
        {
            if (avatar == null)
            {
                return false;
            }

            if (avatar.GetAvatarPrefab() == null)
            {
                if (EditorUtility.DisplayDialog("Avatar has no prefab", "The given avatar doesn't have a source prefab. A prefab is required to register the avatar. Do you want to create one now?", "Yes", "Cancel"))
                {
                    if (UxrEditorUtils.CreateAvatarPrefab(avatar, "Save prefab", avatar.name, out GameObject prefab, out GameObject newInstance))
                    {
                        if (newInstance != null)
                        {
                            avatar = newInstance.GetComponent<UxrAvatar>();
                        }

                        SerializedObject   serializedAvatar   = new SerializedObject(avatar);
                        SerializedProperty propertyPrefabGuid = serializedAvatar.FindProperty(UxrAvatarEditor.VarNamePrefabGuid);

                        if (propertyPrefabGuid != null)
                        {
                            serializedAvatar.Update();
                            propertyPrefabGuid.stringValue = UxrAvatarEditorExt.GetGuid(prefab);
                            serializedAvatar.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                if (avatar.GetAvatarPrefab() == null)
                {
                    return false;
                }
            }

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObject grabbableObject = targetObject as UxrGrabbableObject;
                Undo.RecordObject(grabbableObject, "Register Avatar for grips");
                grabbableObject.Editor_RegisterAvatarForGrips(avatar);
            }

            return true;
        }

        /// <summary>
        ///     Removes the given avatar from the available grips.
        /// </summary>
        /// <param name="avatar">
        ///     The avatar to remove. The avatar should have a prefab source, which is what actually will be
        ///     unregistered
        /// </param>
        private void RemoveAvatarGrip(UxrAvatar avatar)
        {
            foreach (Object targetObject in serializedObject.targetObjects)
            {
                Undo.RecordObject(targetObject as UxrGrabbableObject, "Remove avatar grip");
                (targetObject as UxrGrabbableObject).Editor_RemoveAvatarFromGrips(avatar);
            }
        }

        /// <summary>
        ///     Computes the grab pose preview meshes for a given avatar prefab.
        /// </summary>
        /// <param name="avatarPrefab">The registered avatar to compute the preview meshes for</param>
        /// <param name="previewGrabPosesProperty">
        ///     Serialized property which tells which hands should be generated.
        /// </param>
        /// <param name="grabPoint">-1 to compute all grab points, otherwise will compute the given grab point preview mesh</param>
        private void ComputeGrabPoseMeshes(GameObject avatarPrefab, SerializedProperty previewGrabPosesProperty, int grabPoint = -1)
        {
            DestroyGrabPoseTempEditorObjects(grabPoint);
            CreateGrabPoseTempEditorObjects(avatarPrefab, GetPreviewGrabPosesMode(previewGrabPosesProperty), grabPoint);
        }

        /// <summary>
        ///     Creates the temporal grab pose GameObjects that are used to quickly preview/edit grabbing mechanics
        ///     in the editor.
        /// </summary>
        /// <param name="avatarPrefab">The registered avatar to create the temp poses for</param>
        /// <param name="previewGrabPosesMode">Which preview hands should be rendered</param>
        /// <param name="grabPoint">-1 to compute all grab points, otherwise will compute the given grab point preview mesh</param>
        private void CreateGrabPoseTempEditorObjects(GameObject avatarPrefab, UxrPreviewGrabPoses previewGrabPosesMode, int grabPoint = -1)
        {
            if (previewGrabPosesMode == UxrPreviewGrabPoses.DontShow || avatarPrefab == null)
            {
                return;
            }

            UxrAvatar avatar = TryGetSceneAvatar(avatarPrefab);

            if (avatar == null)
            {
                return;
            }

            // Get grabber renderers which will be used to know which materials to use when rendering grab poses

            UxrGrabber[] grabbers             = avatar.GetComponentsInChildren<UxrGrabber>(false);
            Renderer     leftGrabberRenderer  = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Left && g.HandRenderer != null)?.HandRenderer;
            Renderer     rightGrabberRenderer = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Right && g.HandRenderer != null)?.HandRenderer;

            // Create temporal grab pose objects

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObject grabbableObject = targetObject as UxrGrabbableObject;

                if (!EditorUtility.IsPersistent(grabbableObject.gameObject))
                {
                    DestroyGrabPoseTempEditorObjects(grabbableObject, grabPoint);
                    CreateGrabPoseTempEditorObjects(grabbableObject, avatar, leftGrabberRenderer, rightGrabberRenderer, previewGrabPosesMode, grabPoint);
                }
            }
        }

        /// <summary>
        ///     Creates the temporal grab pose objects for a given <see cref="UxrGrabbableObject" />.
        /// </summary>
        /// <param name="grabbableObject">The UxrGrabbableObject to create the temporal grab pose objects for</param>
        /// <param name="avatar">The avatar component that is used to generate previews with</param>
        /// <param name="leftGrabberRenderer">The avatar left hand renderer</param>
        /// <param name="rightGrabberRenderer">The avatar right hand renderer</param>
        /// <param name="previewGrabPosesMode">Which preview hands should be rendered</param>
        /// <param name="grabPoint">-1 to compute all grab points, otherwise will compute the given grab point preview mesh</param>
        /// <returns>List with the GameObjects that were created</returns>
        private List<GameObject> CreateGrabPoseTempEditorObjects(UxrGrabbableObject  grabbableObject,
                                                                 UxrAvatar           avatar,
                                                                 Renderer            leftGrabberRenderer,
                                                                 Renderer            rightGrabberRenderer,
                                                                 UxrPreviewGrabPoses previewGrabPosesMode,
                                                                 int                 grabPoint)
        {
            List<GameObject> newGameObjects = new List<GameObject>();

            if (grabbableObject)
            {
                grabbableObject.Editor_CheckRegisterAvatarsInNewGrabPoints();

                for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                {
                    if (grabbableObject.GetGrabPoint(i).HideHandGrabberRenderer == false && grabbableObject.GetGrabPoint(i).SnapMode == UxrSnapToHandMode.PositionAndRotation)
                    {
                        // Check if we need to actually show the preview meshes. Process conditions first. Group them by category to make them more readable.

                        bool isGrabPointFoldedOut = _foldoutGrabPoints && ((i == 0 && grabbableObject.GetGrabPoint(i).IsEditorFoldedOut) || (i > 0 && _propAdditionalGrabPoints.isExpanded && grabbableObject.GetGrabPoint(i).IsEditorFoldedOut));

                        bool isLeftHandUsed  = (!grabbableObject.GetGrabPoint(i).BothHandsCompatible && grabbableObject.GetGrabPoint(i).HandSide == UxrHandSide.Left) || grabbableObject.GetGrabPoint(i).BothHandsCompatible;
                        bool isRightHandUsed = (!grabbableObject.GetGrabPoint(i).BothHandsCompatible && grabbableObject.GetGrabPoint(i).HandSide == UxrHandSide.Right) || grabbableObject.GetGrabPoint(i).BothHandsCompatible;

                        bool previewLeft  = (previewGrabPosesMode & UxrPreviewGrabPoses.ShowLeftHand) != 0 && isLeftHandUsed && isGrabPointFoldedOut;
                        bool previewRight = (previewGrabPosesMode & UxrPreviewGrabPoses.ShowRightHand) != 0 && isRightHandUsed && isGrabPointFoldedOut;

                        if (previewLeft)
                        {
                            UxrPreviewHandGripMesh previewMeshLeft = UxrPreviewHandGripMesh.Build(grabbableObject, avatar, i, UxrHandSide.Left);

                            if (previewMeshLeft != null && previewMeshLeft.UnityMesh != null && leftGrabberRenderer != null)
                            {
                                grabbableObject.GetGrabPoint(i).GetGripPoseInfo(avatar).GrabPoseMeshLeft = previewMeshLeft.UnityMesh;
                                Transform  grabTransform = grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatar, i, UxrHandSide.Left);
                                GameObject grabPose      = UxrGrabbableObjectSnapTransformEditor.CreateAndSetupPreviewMeshObject(grabTransform, previewMeshLeft, previewMeshLeft.UnityMesh, leftGrabberRenderer.sharedMaterials);
                                newGameObjects.Add(grabPose);
                            }
                        }

                        if (previewRight)
                        {
                            UxrPreviewHandGripMesh previewMeshRight = UxrPreviewHandGripMesh.Build(grabbableObject, avatar, i, UxrHandSide.Right);

                            if (previewMeshRight != null && previewMeshRight.UnityMesh != null && rightGrabberRenderer != null)
                            {
                                grabbableObject.GetGrabPoint(i).GetGripPoseInfo(avatar).GrabPoseMeshRight = previewMeshRight.UnityMesh;
                                Transform  grabTransform = grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatar, i, UxrHandSide.Right);
                                GameObject grabPose      = UxrGrabbableObjectSnapTransformEditor.CreateAndSetupPreviewMeshObject(grabTransform, previewMeshRight, previewMeshRight.UnityMesh, rightGrabberRenderer.sharedMaterials);
                                newGameObjects.Add(grabPose);
                            }
                        }
                    }
                }
            }

            return newGameObjects;
        }

        /// <summary>
        ///     Refreshes the grab pose preview meshes for a given avatar only when they match a given handPose.
        ///     We don't create or destroy objects because when this is called we can assume that the objects have already
        ///     been created.
        /// </summary>
        /// <param name="handPoseAsset">The hand pose that we need to look for and update</param>
        /// <param name="avatarPrefab">The registered avatar to recompute the preview meshes for</param>
        /// <param name="previewGrabPosesProperty">
        ///     Serialized property which tells which hands should be generated (enum from
        ///     UxrGrabbableObject.PreviewGrabPoses)
        /// </param>
        private void RefreshGrabPoseMeshes(UxrHandPoseAsset handPoseAsset, GameObject avatarPrefab, SerializedProperty previewGrabPosesProperty)
        {
            UxrAvatar avatar = TryGetSceneAvatar(avatarPrefab);

            if (avatar == null)
            {
                return;
            }

            // Iterate over selected objects that currently have poses being previewed matching the given avatar and hand pose

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObject grabbableObject          = targetObject as UxrGrabbableObject;
                GameObject         selectedAvatarGameObject = grabbableObject.Editor_GetSelectedAvatarPrefabForGrips();

                if (!selectedAvatarGameObject)
                {
                    continue;
                }

                UxrAvatar selectedAvatar = selectedAvatarGameObject.GetComponent<UxrAvatar>();

                if (!EditorUtility.IsPersistent(grabbableObject.gameObject) && selectedAvatar != null && selectedAvatar.IsPrefabCompatibleWith(avatarPrefab.GetComponent<UxrAvatar>()))
                {
                    for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                    {
                        // Does this grab point use the pose?

                        UxrGripPoseInfo  gripPose      = grabbableObject.GetGrabPoint(i).GetGripPoseInfo(selectedAvatar);
                        UxrHandPoseAsset gripPoseAsset = gripPose?.HandPose;

                        if (gripPoseAsset != null && gripPoseAsset.name == handPoseAsset.name)
                        {
                            if (GetPreviewGrabPosesMode(previewGrabPosesProperty).HasFlag(UxrPreviewGrabPoses.ShowLeftHand))
                            {
                                Mesh                          oldMeshLeft          = grabbableObject.GetGrabPoint(i).GetGripPoseInfo(selectedAvatar).GrabPoseMeshLeft;
                                UxrGrabbableObjectPreviewMesh previewMeshComponent = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>().FirstOrDefault(m => m.GetComponent<MeshFilter>().sharedMesh == oldMeshLeft);

                                if (previewMeshComponent != null && previewMeshComponent.PreviewMesh is UxrPreviewHandGripMesh previewMeshLeft)
                                {
                                    previewMeshLeft.Refresh(grabbableObject, avatar, i, UxrHandSide.Left, true);
                                }
                            }

                            if (GetPreviewGrabPosesMode(previewGrabPosesProperty).HasFlag(UxrPreviewGrabPoses.ShowRightHand))
                            {
                                Mesh                          oldMeshRight         = grabbableObject.GetGrabPoint(i).GetGripPoseInfo(selectedAvatar).GrabPoseMeshRight;
                                UxrGrabbableObjectPreviewMesh previewMeshComponent = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>().FirstOrDefault(m => m.GetComponent<MeshFilter>().sharedMesh == oldMeshRight);

                                if (previewMeshComponent != null && previewMeshComponent.PreviewMesh is UxrPreviewHandGripMesh previewMeshRight)
                                {
                                    previewMeshRight.Refresh(grabbableObject, avatar, i, UxrHandSide.Right, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Refreshes the grab pose preview meshes for a given avatar.
        ///     We don't create or destroy objects because when this is called we can assume that the objects have already
        ///     been created.
        /// </summary>
        /// <param name="avatarPrefab">The registered avatar to recompute the preview meshes for</param>
        /// <param name="previewGrabPosesProperty">
        ///     Serialized property which tells which hands should be generated (enum from UxrGrabbableObject.PreviewGrabPoses)
        /// </param>
        /// <param name="grabPoint">-1 to compute all grab points, otherwise will compute the given grab point preview mesh</param>
        private void RefreshGrabPoseMeshes(GameObject avatarPrefab, SerializedProperty previewGrabPosesProperty, int grabPoint)
        {
            UxrAvatar avatar = TryGetSceneAvatar(avatarPrefab);

            if (avatar == null)
            {
                return;
            }

            // Iterate over selected objects that currently have poses being previewed matching the given avatar and hand pose

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObject grabbableObject          = targetObject as UxrGrabbableObject;
                GameObject         selectedAvatarGameObject = grabbableObject.Editor_GetSelectedAvatarPrefabForGrips();

                if (!selectedAvatarGameObject)
                {
                    continue;
                }

                UxrAvatar selectedAvatar = selectedAvatarGameObject.GetComponent<UxrAvatar>();

                if (!EditorUtility.IsPersistent(grabbableObject.gameObject) && selectedAvatar != null && selectedAvatar.IsPrefabCompatibleWith(avatarPrefab.GetComponent<UxrAvatar>()))
                {
                    for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                    {
                        if (i != grabPoint && grabPoint != -1)
                        {
                            continue;
                        }

                        if (GetPreviewGrabPosesMode(previewGrabPosesProperty).HasFlag(UxrPreviewGrabPoses.ShowLeftHand))
                        {
                            Mesh                          oldMeshLeft          = grabbableObject.GetGrabPoint(i).GetGripPoseInfo(selectedAvatar).GrabPoseMeshLeft;
                            UxrGrabbableObjectPreviewMesh previewMeshComponent = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>().FirstOrDefault(m => m.GetComponent<MeshFilter>().sharedMesh == oldMeshLeft);

                            if (previewMeshComponent.PreviewMesh is UxrPreviewHandGripMesh previewMeshLeft)
                            {
                                previewMeshLeft.Refresh(grabbableObject, avatar, i, UxrHandSide.Left);
                            }
                        }

                        if (GetPreviewGrabPosesMode(previewGrabPosesProperty).HasFlag(UxrPreviewGrabPoses.ShowRightHand))
                        {
                            Mesh                          oldMeshRight         = grabbableObject.GetGrabPoint(i).GetGripPoseInfo(selectedAvatar).GrabPoseMeshRight;
                            UxrGrabbableObjectPreviewMesh previewMeshComponent = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>().FirstOrDefault(m => m.GetComponent<MeshFilter>().sharedMesh == oldMeshRight);

                            if (previewMeshComponent.PreviewMesh is UxrPreviewHandGripMesh previewMeshRight)
                            {
                                previewMeshRight.Refresh(grabbableObject, avatar, i, UxrHandSide.Right);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Destroys the temporal grab pose objects created by CreateGrabPoseTempEditorObjects()
        /// </summary>
        /// <param name="grabPoint">
        ///     -1 to destroy preview meshes for all grab points, otherwise will destroy the given grab point
        ///     preview mesh
        /// </param>
        private void DestroyGrabPoseTempEditorObjects(int grabPoint = -1)
        {
            foreach (Object targetObject in serializedObject.targetObjects)
            {
                DestroyGrabPoseTempEditorObjects(targetObject as UxrGrabbableObject, grabPoint);
            }
        }

        /// <summary>
        ///     Destroys the temporal grab pose objects created by CreateGrabPoseTempEditorObjects() for
        ///     a specific UxrGrabbableObject
        /// </summary>
        /// <param name="grabbableObject">GrabbableObject to delete the temporal objects for</param>
        /// <param name="grabPoint">
        ///     -1 to destroy preview meshes for all grab points, otherwise will destroy the given grab point
        ///     preview mesh
        /// </param>
        private void DestroyGrabPoseTempEditorObjects(UxrGrabbableObject grabbableObject, int grabPoint)
        {
            if (grabbableObject)
            {
                UxrGrabbableObjectPreviewMesh[] previewMeshComponents = grabbableObject.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>();

                foreach (UxrGrabbableObjectPreviewMesh previewMeshComponent in previewMeshComponents)
                {
                    DestroyImmediate(previewMeshComponent.gameObject);
                }
            }
        }

        /// <summary>
        ///     Gets the previewGrabPosesMode enumeration value given a property.
        /// </summary>
        /// <param name="previewGrabPosesProperty">Property representing an UxrGrabbableObject.PreviewGrabPoses</param>
        /// <returns>Enumeration value</returns>
        private UxrPreviewGrabPoses GetPreviewGrabPosesMode(SerializedProperty previewGrabPosesProperty)
        {
            return (UxrPreviewGrabPoses)previewGrabPosesProperty.intValue;
        }

        /// <summary>
        ///     Returns the index in a popup control containing the PreviewGrabPoses enum value.
        /// </summary>
        /// <param name="intValue">Int containing the enum value</param>
        /// <returns>Index in the popup </returns>
        private int GetPreviewGrabPosesIndexFromIntValue(int intValue)
        {
            if (intValue == (int)UxrPreviewGrabPoses.DontShow)
            {
                return 0;
            }
            if (intValue == (int)UxrPreviewGrabPoses.ShowLeftHand)
            {
                return 1;
            }
            if (intValue == (int)UxrPreviewGrabPoses.ShowRightHand)
            {
                return 2;
            }
            if (intValue == (int)UxrPreviewGrabPoses.ShowBothHands)
            {
                return 3;
            }

            return 3;
        }

        /// <summary>
        ///     Returns the PreviewGrabPoses enum value (as int) given the index in a popup control.
        /// </summary>
        /// <param name="index">Index in the popup control</param>
        /// <returns>Int containing the enum value</returns>
        private int GetPreviewGrabPosesIntValueFromIndex(int index)
        {
            if (index == 1)
            {
                return (int)UxrPreviewGrabPoses.ShowLeftHand;
            }
            if (index == 2)
            {
                return (int)UxrPreviewGrabPoses.ShowRightHand;
            }
            if (index == 3)
            {
                return (int)UxrPreviewGrabPoses.ShowBothHands;
            }

            return (int)UxrPreviewGrabPoses.DontShow;
        }

        /// <summary>
        ///     Returns the possible entries for the UxrGrabbableObject.PreviewGrabPoses flags
        /// </summary>
        /// <returns>Array of string to use in an Editor Popup</returns>
        private string[] GetPreviewGrabPosesModeStrings()
        {
            return Enum.GetNames(typeof(UxrPreviewGrabPoses));
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Some parameters are specific to an avatar controller. We will signal them using this color.
        /// </summary>
        public static Color GUIColorAvatarControllerParameter = Color.green;

        #endregion

        #region Private Types & Data

        private GameObject SelectedAvatarForGrips => (serializedObject.targetObjects[0] as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabForGrips();

        private GUIContent ContentRegisterAvatar                     { get; } = new GUIContent("Register Avatar For Grips",                    "Registers an avatar to have the possibility to have different grip parameters for each avatar. This allows to fine-tune how different hand shapes and sizes wrap around the same object. Parameters that can be adjusted for each avatar will be colored to help in the process.");
        private GUIContent ContentSelectAvatar                       { get; } = new GUIContent("Selected Avatar Grips:",                       "Switches the avatar currently selected to edit its grip parameters. Being able to register different avatars allows to fine-tune how different hand shapes and sizes wrap around the same object. Parameters that can be adjusted for each avatar will be colored to help in the process.");
        private GUIContent ContentManipulationMode                   { get; } = new GUIContent("Manipulation Mode",                            "Use Grab And Move to pick up objects and move/throw/place them. Use Rotate Around Axis for wheels, levers and similar objects.");
        private GUIContent ContentIgnoreGrabbableParentDependency    { get; } = new GUIContent("Ignore Parent Dependency",                     "Mark this to ignore the parent constraint and tell this object is independent.");
        private GUIContent ContentPriority                           { get; } = new GUIContent("Priority",                                     "By default, closer objects will be always grabbed over far objects. Using priority, objects with higher priority will always be grabbed if they are in range.");
        private GUIContent ContentRigidBodySource                    { get; } = new GUIContent("Rigidbody",                                    "References the object's rigidbody when physics are required. The object will be made kinematic when grabbed and optionally dynamic when released.");
        private GUIContent ContentRigidBodyDynamicOnRelease          { get; } = new GUIContent("Rigidbody Dynamic On Release",                 "When a rigidbody is specified it controls whether it will be marked as dynamic after being grabbed and released. Otherwise it will continue to be kinematic after being released.");
        private GUIContent ContentVerticalReleaseMultiplier          { get; } = new GUIContent("Vertical Release Multiplier",                  "When throwing a rigidbody this parameter will enable increasing or decreasing the actual release velocity (vertical component).");
        private GUIContent ContentHorizontalReleaseMultiplier        { get; } = new GUIContent("Horizontal Release Multiplier",                "When throwing a rigidbody this parameter will enable increasing or decreasing the actual release velocity (horizontal component).");
        private GUIContent ContentNeedsTwoHandsToRotate              { get; } = new GUIContent("Needs 2 Hands To Rotate",                      "When manipulation mode is set to Rotate Around Axis this will tell if the user will be able to rotate the object using one hand only or the object needs to be grabbed with two hands in order to rotate it.");
        private GUIContent ContentAllowMultiGrab                     { get; } = new GUIContent("Allow 2 Hands Grab",                           "When more than one grab point has been specified, this parameter will tell if the object can be grabbed with two hands at the same time.");
        private GUIContent ContentPreviewGrabPosesMode               { get; } = new GUIContent("Preview Grip Pose Meshes",                     "Will show/hide the preview grip pose meshes in the Scene Window.");
        private GUIContent ContentFirstGrabPointIsMain               { get; } = new GUIContent("First Grab Point Is Main",                     "Whether the first grab point in the list is the main grab in objects with more than one grab point. When an object is grabbed with both hands, the main grab controls the actual position while the secondary grab controls the direction. Set it to true in objects like a rifle, where the trigger hand should be the first grab in order to keep the object in place, and the front grab will control the aiming direction. If false, the grab point order is irrelevant and the hand that grabbed the object first will be considered as the main grab.");
        private GUIContent ContentGrabPoint                          { get; } = new GUIContent("Grab Point",                                   "Parameters of the grabbing point.");
        private GUIContent ContentAdditionalGrabPoints               { get; } = new GUIContent("Additional Grab Points",                       "Parameters of the additional grabbing points which enables grabbing the object using more than one hand.");
        private GUIContent ContentUseParenting                       { get; } = new GUIContent("Parent When Placing",                          "Will parent this GameObject to the anchor when placing it.");
        private GUIContent ContentAutoCreateStartAnchor              { get; } = new GUIContent("Create Anchor at Startup",                     $"Will generate an {nameof(UxrGrabbableObjectAnchor)} at startup and place this object on it, keeping the position and rotation. If the object has an Anchor Compatible Tag assigned, the tag will be the only compatible tag allowed by the anchor. This is useful to have a fixed anchor where the object can be placed back again after picking it up.");
        private GUIContent ContentStartAnchor                        { get; } = new GUIContent("Start Anchor",                                 $"If this object is initially placed on an {nameof(UxrGrabbableObjectAnchor)}, select here which anchor it is placed on.");
        private GUIContent ContentTag                                { get; } = new GUIContent("Anchor Compatible Tag",                        $"String identifier that can be used to filter which objects can be placed on which {nameof(UxrGrabbableObjectAnchor)} objects.");
        private GUIContent ContentDropAlignTransform                 { get; } = new GUIContent("Anchor Snap",                                  "The transform where objects will be snapped to when being placed.");
        private GUIContent ContentDropSnapMode                       { get; } = new GUIContent("Anchor Snap Mode",                             $"How this object will snap to the {nameof(UxrGrabbableObjectAnchor)} transform after being placing on it.");
        private GUIContent ContentDropProximityTransform             { get; } = new GUIContent("Anchor Proximity Position",                    $"The reference that will be used to know if this object is close enough to an {nameof(UxrGrabbableObjectAnchor)} to place it there.");
        private GUIContent ContentTranslationConstraintMode          { get; } = new GUIContent("Anchor Proximity Translation Constraint Mode", "Allows to constrain the translation of this object while being grabbed.");
        private GUIContent ContentRestrictToBox                      { get; } = new GUIContent("Restrict To Box",                              "Allowed volume where this object's pivot will be allowed to move while being grabbed.");
        private GUIContent ContentRestrictToSphere                   { get; } = new GUIContent("Restrict To Sphere",                           "Allowed volume where this object's pivot will be allowed to move while being grabbed.");
        private GUIContent ContentTranslationLimitsMin               { get; } = new GUIContent("Translation Offset Min",                       "Minimum allowed offset in local coordinates.");
        private GUIContent ContentTranslationLimitsMax               { get; } = new GUIContent("Translation Offset Max",                       "Maximum allowed offset in local coordinates.");
        private GUIContent ContentTranslationLimitsReferenceIsParent { get; } = new GUIContent("Translation Reference Is Parent",              "Tells whether the local translation limits specified are with respect to the parent or another transform.");
        private GUIContent ContentTranslationLimitsParent            { get; } = new GUIContent("Translation Parent Transform",                 "Allows to specify a different transform as parent reference for the local translation limits.");
        private GUIContent ContentRotationConstraintMode             { get; } = new GUIContent("Rotation Constraint Mode",                     "Allows to constrain the rotation of this object while being grabbed.");
        private GUIContent ContentRotationAngleLimitsMin             { get; } = new GUIContent("Rotation Angles Offset Min",                   "Minimum allowed rotation offset degrees in local axes.");
        private GUIContent ContentRotationAngleLimitsMax             { get; } = new GUIContent("Rotation Angles Offset Max",                   "Maximum allowed rotation offset degrees in local axes.");
        private GUIContent ContentRotationLimitsReferenceIsParent    { get; } = new GUIContent("Rotation Reference Is Parent",                 "Tells whether the local rotation limits specified are with respect to the parent or another transform.");
        private GUIContent ContentRotationLimitsParent               { get; } = new GUIContent("Rotation Parent Transform",                    "Allows to specify a different transform as parent reference for the local rotation limits.");
        private GUIContent ContentLockedGrabReleaseDistance          { get; } = new GUIContent("Locked Grab Release Distance",                 "Maximum allowed distance of a locked grab to move away from the grab point before being released.");
        private GUIContent ContentTranslationResistance              { get; } = new GUIContent("Translation Resistance",                       "Resistance of the object to being moved. Values higher than zero may be used to simulate heavy objects.");
        private GUIContent ContentRotationResistance                 { get; } = new GUIContent("Rotation Resistance",                          "Resistance of the object to being rotated. Values higher than zero may be used to simulate heavy objects.");

        private const string DefaultHelp = "The grabbable object is ready-to-use before setting up any additional parameter.\n" +
                                           "For advanced manipulation you can get assistance by hovering the mouse over each parameter.\n" +
                                           "Use Grab Points to specify different grabbable parts on the object. Register avatars to adjust grab poses and snapping differently for each avatar prefab. Child avatar prefabs will automatically inherit the parent prefab's parameters but can also be registered to override settings.\n" +
                                           "The Object Placement section lets you control where and how the object can be placed.\n" +
                                           "The Constraints section lets you set up manipulation constraints.";
        private static readonly string NeedsUxrAvatar = $"The {nameof(UxrGrabbableObject)} component needs an {nameof(UxrAvatar)} in the scene. Add an avatar to the scene unless you know that you are instancing an avatar at runtime. You can find prefabs in UltimateXR/Prefabs";
        private static readonly string NeedsUxrAvatarWithGrabbing = $"Could not find {nameof(UxrAvatar)}(s) in the scene with {nameof(UxrGrabber)} components. These components are the ones where grabbable objects are attached to when picking them up." +
                                                                    $"\nCreate two child GameObjects -one on each hand of the avatar- and set up an {nameof(UxrGrabber)} component on each.";
        private static readonly string NeedsUxrAvatarWithGrabController = $"Almost there! Your avatar has {nameof(UxrGrabber)} components but there are no actions set up to trigger grab events on the avatar itself. Add an {nameof(UxrStandardAvatarController)} component on your avatar to set up grab events for the left and right hand";

        private static readonly Dictionary<SerializedObject, UxrGrabbableObjectEditor> s_openEditors = new Dictionary<SerializedObject, UxrGrabbableObjectEditor>();

        private SerializedProperty _propManipulationMode;
        private SerializedProperty _propIgnoreGrabbableParentDependency;
        private SerializedProperty _propControlParentDirection;

        private SerializedProperty _propPriority;
        private SerializedProperty _propRigidBodySource;
        private SerializedProperty _propRigidBodyDynamicOnRelease;
        private SerializedProperty _propVerticalReleaseMultiplier;
        private SerializedProperty _propHorizontalReleaseMultiplier;
        private SerializedProperty _propNeedsTwoHandsToRotate;
        private SerializedProperty _propAllowMultiGrab;

        private SerializedProperty _propPreviewGrabPosesMode;
        private SerializedProperty _propPreviewPosesRegenerationType;
        private SerializedProperty _propPreviewPosesRegenerationIndex;
        private SerializedProperty _propFirstGrabPointIsMain;
        private SerializedProperty _propGrabPoint;
        private SerializedProperty _propAdditionalGrabPoints;

        private SerializedProperty _propUseParenting;
        private SerializedProperty _propAutoCreateStartAnchor;
        private SerializedProperty _propStartAnchor;
        private SerializedProperty _propTag;
        private SerializedProperty _propDropAlignTransformUseSelf;
        private SerializedProperty _propDropAlignTransform;
        private SerializedProperty _propDropSnapMode;
        private SerializedProperty _propDropProximityTransformUseSelf;
        private SerializedProperty _propDropProximityTransform;

        private SerializedProperty _propTranslationConstraintMode;
        private SerializedProperty _propRestrictToBox;
        private SerializedProperty _propRestrictToSphere;
        private SerializedProperty _propTranslationLimitsMin;
        private SerializedProperty _propTranslationLimitsMax;
        private SerializedProperty _propTranslationLimitsReferenceIsParent;
        private SerializedProperty _propTranslationLimitsParent;
        private SerializedProperty _propRotationConstraintMode;
        private SerializedProperty _propRotationAngleLimitsMin;
        private SerializedProperty _propRotationAngleLimitsMax;
        private SerializedProperty _propRotationLimitsReferenceIsParent;
        private SerializedProperty _propRotationLimitsParent;
        private SerializedProperty _propLockedGrabReleaseDistance;
        private SerializedProperty _propTranslationResistance;
        private SerializedProperty _propRotationResistance;

        private int _selectedAvatarGripPoseIndex;

        private bool _foldoutManipulation      = true;
        private bool _foldoutGeneralParameters = true;
        private bool _foldoutAvatarGrips       = true;
        private bool _foldoutGrabPoints        = true;
        private bool _foldoutObjectPlacing     = true;
        private bool _foldoutConstraints       = true;

        #endregion
    }
}