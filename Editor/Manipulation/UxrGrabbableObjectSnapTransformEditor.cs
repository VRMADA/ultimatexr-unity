// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectSnapTransformEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Editor.Avatar;
using UltimateXR.Editor.Manipulation.HandPoses;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation
{
    /// <summary>
    ///     Custom editor for <see cref="UxrGrabbableObjectSnapTransform" />.
    /// </summary>
    [CustomEditor(typeof(UxrGrabbableObjectSnapTransform))]
    public class UxrGrabbableObjectSnapTransformEditor : UnityEditor.Editor
    {
        #region Public Methods

        /// <summary>
        ///     Refreshes the preview poses for a specific avatar and hand pose. Designed to be called from the Hand Pose Editor to
        ///     keep the preview meshes updated. Preview hand pose meshes that share the same prefab will be updated accordingly.
        /// </summary>
        /// <param name="avatar">The avatar to update the poses for</param>
        /// <param name="handPoseAsset">The pose asset to look for and update</param>
        public static void RefreshGrabPoseMeshes(UxrAvatar avatar, UxrHandPoseAsset handPoseAsset)
        {
            foreach (KeyValuePair<SerializedObject, UxrGrabbableObjectSnapTransformEditor> editorPair in s_openEditors)
            {
                editorPair.Value.RefreshGrabPoseMeshesInternal(avatar.GetAvatarPrefab(), handPoseAsset);
            }
        }

        /// <summary>
        ///     Creates a hand pose preview object.
        /// </summary>
        /// <param name="parent">Parent that will be assigned to the new object</param>
        /// <param name="previewMesh">The preview mesh object</param>
        /// <param name="mesh">Mesh assigned to the object</param>
        /// <param name="sharedMaterials">Shared materials to be assigned to the mesh</param>
        /// <returns>New preview GameObject</returns>
        public static GameObject CreateAndSetupPreviewMeshObject(Transform parent, UxrPreviewHandGripMesh previewMesh, Mesh mesh, Material[] sharedMaterials)
        {
            GameObject grabPose = EditorUtility.CreateGameObjectWithHideFlags("GrabPose",
                                                                              HideFlags.HideAndDontSave,
                                                                              typeof(UxrGrabbableObjectPreviewMesh),
                                                                              typeof(MeshFilter),
                                                                              typeof(MeshRenderer));

            grabPose.transform.parent                                          = parent;
            grabPose.transform.localPosition                                   = Vector3.zero;
            grabPose.transform.localRotation                                   = Quaternion.identity;
            grabPose.GetComponent<MeshFilter>().sharedMesh                     = mesh;
            grabPose.GetComponent<MeshRenderer>().sharedMaterials              = sharedMaterials;
            grabPose.GetComponent<UxrGrabbableObjectPreviewMesh>().PreviewMesh = previewMesh;

            return grabPose;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Creates temporal grab pose editor objects.
        /// </summary>
        private void OnEnable()
        {
            for (int i = 0; i < serializedObject.targetObjects.Length; ++i)
            {
                UxrGrabbableObjectSnapTransform snapTransform = serializedObject.targetObject as UxrGrabbableObjectSnapTransform;
                snapTransform.hideFlags = HideFlags.DontSaveInBuild;
            }

            Undo.undoRedoPerformed += OnUndoRedo;

            CreateGrabPoseTempEditorObjects();

            // Add a static reference so that we can update the grip preview meshes from the Hand Pose Editor

            if (s_openEditors.ContainsKey(serializedObject) == false)
            {
                s_openEditors.Add(serializedObject, this);
            }

            // Destroy objects that don't belong to any UxrGrabbableObject (duplicated in editor or moved from hierarchy)

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                CheckUnusedTransform(targetObject as UxrGrabbableObjectSnapTransform);
            }
        }

        /// <summary>
        ///     Destroys temporal grab pose editor objects.
        /// </summary>
        private void OnDisable()
        {
            DestroyGrabPoseTempEditorObjects();

            Undo.undoRedoPerformed -= OnUndoRedo;

            if (s_openEditors.ContainsKey(serializedObject))
            {
                s_openEditors.Remove(serializedObject);
            }
        }

        /// <summary>
        ///     Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObjects.Length == 1)
            {
                UxrGrabbableObjectSnapTransform snapTransform   = serializedObject.targetObject as UxrGrabbableObjectSnapTransform;
                UxrGrabbableObject              grabbableObject = snapTransform.SafeGetComponentInParent<UxrGrabbableObject>();

                if (grabbableObject == null)
                {
                    return;
                }

                UxrAvatar avatarForGrip = UxrAvatarEditorExt.GetFromGuid(grabbableObject.Editor_GetGrabAlignTransformAvatar(snapTransform.transform));

                if (avatarForGrip)
                {
                    IEnumerable<int> grabPointIndices = grabbableObject.Editor_GetGrabPointsForGrabAlignTransform(avatarForGrip, snapTransform.transform);

                    if (grabPointIndices.Count() > 1)
                    {
                        EditorGUILayout.HelpBox($"This transform is being used in more than one grab point of {grabbableObject.name}", MessageType.Warning);
                    }
                    else if (grabPointIndices.Count() == 1)
                    {
                        UxrGrabPointInfo grabPoint     = grabbableObject.GetGrabPoint(grabPointIndices.First());
                        UxrGripPoseInfo  gripPose      = grabPoint.GetGripPoseInfo(avatarForGrip);
                        UxrHandPoseAsset gripPoseAsset = gripPose?.HandPose;

                        if (gripPoseAsset != null && gripPoseAsset.PoseType == UxrHandPoseType.Blend)
                        {
                            EditorGUILayout.HelpBox($"This slider will affect both the left and right hand of pose {gripPoseAsset.name}. Grab points in {nameof(UxrGrabbableObject)} use the same slider for both hands.",
                                                    MessageType.Info);

                            Undo.RecordObject(grabbableObject, "Change Pose Blend");
                            EditorGUILayout.Space();

                            float oldBlend = gripPose.PoseBlendValue;
                            float newBlend = EditorGUILayout.Slider($"Pose Blend ({gripPoseAsset.name})", oldBlend, 0.0f, 1.0f);
                            gripPose.PoseBlendValue = newBlend;

                            if (!Mathf.Approximately(oldBlend, newBlend))
                            {
                                _previewMeshLeft?.Refresh(grabbableObject, _cachedSingleObjectAvatar, grabPointIndices.First(), UxrHandSide.Left);
                                _previewMeshRight?.Refresh(grabbableObject, _cachedSingleObjectAvatar, grabPointIndices.First(), UxrHandSide.Right);
                            }

                            EditorGUILayout.Space();
                        }
                    }

                    UxrGrabber[] grabbers     = avatarForGrip.GetComponentsInChildren<UxrGrabber>();
                    UxrGrabber   leftGrabber  = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Left);
                    UxrGrabber   rightGrabber = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Right);
                    UxrGrabber grabber = _previewMeshLeft != null  ? leftGrabber :
                                         _previewMeshRight != null ? rightGrabber : null;

                    if (grabber)
                    {
                        if (UxrEditorUtils.CenteredButton(new GUIContent("Mirror Up/Down", "")))
                        {
                            Undo.RegisterCompleteObjectUndo(snapTransform.transform, "Mirror transform");
                            snapTransform.transform.ApplyMirroring(snapTransform.transform.position, snapTransform.transform.TransformDirection(-grabber.LocalFingerDirection), grabber.RequiredMirrorType);
                        }

                        if (UxrEditorUtils.CenteredButton(new GUIContent("Mirror Front/Back", "")))
                        {
                            Undo.RegisterCompleteObjectUndo(snapTransform.transform, "Mirror transform");
                            snapTransform.transform.ApplyMirroring(snapTransform.transform.position, snapTransform.transform.TransformDirection(-grabber.LocalPalmOutDirection), grabber.RequiredMirrorType);
                        }

                        if (grabPointIndices.Count() == 1)
                        {
                            UxrGrabPointInfo grabPoint = grabbableObject.GetGrabPoint(grabPointIndices.First());
                            UxrGripPoseInfo  gripPose  = grabPoint.GetGripPoseInfo(avatarForGrip);

                            if (gripPose.GripAlignTransformHandLeft != null && gripPose.GripAlignTransformHandRight != null)
                            {
                                if (UxrEditorUtils.CenteredButton(new GUIContent("Copy From Other Hand", "")))
                                {
                                    Undo.RegisterCompleteObjectUndo(snapTransform.transform, "Set transform");
                                    snapTransform.transform.SetPositionAndRotation(snapTransform.transform == gripPose.GripAlignTransformHandLeft ? gripPose.GripAlignTransformHandRight : gripPose.GripAlignTransformHandLeft);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Recomputes the preview poses.
        /// </summary>
        private void OnUndoRedo()
        {
            ComputeGrabPoseMeshes();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to obtain the left and right grab pose meshes of an align transform if they are available.
        /// </summary>
        /// <param name="avatarPrefab">The registered avatar prefab to get the pose meshes for</param>
        /// <param name="transform">The transform that is potentially an align transform for a UxrGrabbableObject grab point</param>
        /// <param name="previewMeshLeft">Will contain the left grab pose mesh if it was found</param>
        /// <param name="previewMeshRight">Will contain the right grab pose mesh if it was found</param>
        private static void TryGetGrabPoseMeshes(UxrAvatar avatarPrefab, Transform transform, out UxrPreviewHandGripMesh previewMeshLeft, out UxrPreviewHandGripMesh previewMeshRight)
        {
            previewMeshLeft  = null;
            previewMeshRight = null;
            UxrGrabbableObject[] grabbableObjects = transform.GetComponentsInParent<UxrGrabbableObject>();

            UxrAvatar avatar = UxrGrabbableObjectEditor.TryGetSceneAvatar(avatarPrefab.gameObject);

            foreach (UxrGrabbableObject grabbableObject in grabbableObjects)
            {
                for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                {
                    UxrGrabPointInfo grabPoint = grabbableObject.GetGrabPoint(i);

                    if (grabPoint.HideHandGrabberRenderer == false && grabPoint.SnapMode == UxrSnapToHandMode.PositionAndRotation)
                    {
                        if (grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatarPrefab, i, UxrHandSide.Left) == transform && (grabPoint.HandSide == UxrHandSide.Left || grabPoint.BothHandsCompatible))
                        {
                            previewMeshLeft = UxrPreviewHandGripMesh.Build(grabbableObject, avatar, i, UxrHandSide.Left);

                            if (previewMeshLeft != null)
                            {
                                grabPoint.GetGripPoseInfo(avatarPrefab).GrabPoseMeshLeft = previewMeshLeft.UnityMesh;
                            }
                        }

                        if (grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatarPrefab, i, UxrHandSide.Right) == transform && (grabPoint.HandSide == UxrHandSide.Right || grabPoint.BothHandsCompatible))
                        {
                            previewMeshRight = UxrPreviewHandGripMesh.Build(grabbableObject, avatar, i, UxrHandSide.Right);

                            if (previewMeshRight != null)
                            {
                                grabPoint.GetGripPoseInfo(avatarPrefab).GrabPoseMeshRight = previewMeshRight.UnityMesh;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Regenerates the grab pose meshes
        /// </summary>
        private void ComputeGrabPoseMeshes()
        {
            DestroyGrabPoseTempEditorObjects();
            CreateGrabPoseTempEditorObjects();
        }

        /// <summary>
        ///     Creates the temporal grab pose GameObjects that are used to quickly preview/edit grabbing mechanics
        ///     in the editor
        /// </summary>
        private void CreateGrabPoseTempEditorObjects()
        {
            // Create temporal grab pose objects

            _previewMeshLeft  = null;
            _previewMeshRight = null;

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObjectSnapTransform snapTransform   = targetObject as UxrGrabbableObjectSnapTransform;
                UxrGrabbableObject              grabbableObject = snapTransform.SafeGetComponentInParent<UxrGrabbableObject>();

                if (grabbableObject == null)
                {
                    continue;
                }

                // Get the avatar prefab

                UxrAvatar avatarPrefab = UxrAvatarEditorExt.GetFromGuid(grabbableObject.Editor_GetGrabAlignTransformAvatar(snapTransform.transform));

                if (avatarPrefab == null)
                {
                    return;
                }

                // Try to get a compatible instance of the prefab in the scene

                UxrAvatar avatar = UxrGrabbableObjectEditor.TryGetSceneAvatar(avatarPrefab.gameObject);

                if (avatar == null)
                {
                    continue;
                }

                _cachedSingleObjectAvatar = avatar;

                // Get grabber renderers which will be used to know which materials to use when rendering grab poses

                UxrGrabber[] grabbers             = avatar.GetComponentsInChildren<UxrGrabber>(false);
                Renderer     leftGrabberRenderer  = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Left && g.HandRenderer != null)?.HandRenderer;
                Renderer     rightGrabberRenderer = grabbers.FirstOrDefault(g => g.Side == UxrHandSide.Right && g.HandRenderer != null)?.HandRenderer;

                if (snapTransform && avatar)
                {
                    TryGetGrabPoseMeshes(avatarPrefab, snapTransform.transform, out _previewMeshLeft, out _previewMeshRight);

                    if (_previewMeshLeft != null && leftGrabberRenderer)
                    {
                        GameObject grabPose = CreateAndSetupPreviewMeshObject(snapTransform.transform, _previewMeshLeft, _previewMeshLeft.UnityMesh, leftGrabberRenderer.sharedMaterials);
                    }

                    if (_previewMeshRight != null && rightGrabberRenderer)
                    {
                        GameObject grabPose = CreateAndSetupPreviewMeshObject(snapTransform.transform, _previewMeshRight, _previewMeshRight.UnityMesh, rightGrabberRenderer.sharedMaterials);
                    }
                }
            }
        }

        /// <summary>
        ///     Refreshes the preview poses for a specific avatar and hand pose. Designed to be called
        ///     from the Hand Pose Editor to keep the preview meshes updated.
        /// </summary>
        /// <param name="avatarPrefab">The registered avatar to look for</param>
        /// <param name="handPoseAsset">The pose asset to look for and update</param>
        private void RefreshGrabPoseMeshesInternal(UxrAvatar avatarPrefab, UxrHandPoseAsset handPoseAsset)
        {
            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObjectSnapTransform snapTransform   = targetObject as UxrGrabbableObjectSnapTransform;
                UxrGrabbableObject              grabbableObject = snapTransform.SafeGetComponentInParent<UxrGrabbableObject>();

                UxrAvatar selectedAvatar = UxrAvatarEditorExt.GetFromGuid(grabbableObject.Editor_GetGrabAlignTransformAvatar(snapTransform.transform));

                if (selectedAvatar == null || grabbableObject == null || !selectedAvatar.IsPrefabCompatibleWith(avatarPrefab))
                {
                    continue;
                }

                UxrAvatar avatar = UxrGrabbableObjectEditor.TryGetSceneAvatar(avatarPrefab.gameObject);

                if (avatar == null)
                {
                    continue;
                }

                for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                {
                    UxrGrabPointInfo grabPoint     = grabbableObject.GetGrabPoint(i);
                    UxrGripPoseInfo  gripPose      = grabPoint.GetGripPoseInfo(avatar);
                    UxrHandPoseAsset gripPoseAsset = gripPose?.HandPose;

                    if (gripPoseAsset == null)
                    {
                        continue;
                    }

                    // Does this grab point use the pose?

                    if ((grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatar, i, UxrHandSide.Left) == snapTransform.transform ||
                         grabbableObject.Editor_GetGrabPointGrabAlignTransform(avatar, i, UxrHandSide.Right) == snapTransform.transform) && gripPoseAsset.name == handPoseAsset.name)
                    {
                        _previewMeshLeft?.Refresh(grabbableObject, _cachedSingleObjectAvatar, i, UxrHandSide.Left, true);
                        _previewMeshRight?.Refresh(grabbableObject, _cachedSingleObjectAvatar, i, UxrHandSide.Right, true);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a given UxrGrabbableObjectSnapTransform's transform is referenced by
        ///     an UxrGrabbableObject. Components that are not being referenced are usually the
        ///     result of duplicating manually in the editor a GameObject that is used as a
        ///     snap transform for an UxrGrabbableObject. It also happens if a transform is
        ///     moved out of the UxrGrabbableObject hierarchy.
        ///     In case of not being referenced, the preview meshes hanging from it should be
        ///     deleted. The method will also remove the UxrGrabbableObjectSnapTransform
        ///     component itself.
        /// </summary>
        /// <param name="snapTransform">The transform to check</param>
        private void CheckUnusedTransform(UxrGrabbableObjectSnapTransform snapTransform)
        {
            if (snapTransform == null)
            {
                return;
            }

            UxrGrabbableObject grabbableObject = snapTransform.SafeGetComponentInParent<UxrGrabbableObject>();

            if (grabbableObject)
            {
                // Check if it's still being referenced by grabbableObject

                bool referenced = false;

                for (int i = 0; i < grabbableObject.GrabPointCount; ++i)
                {
                    if (grabbableObject.Editor_HasGrabPointWithGrabAlignTransform(snapTransform.transform))
                    {
                        referenced = true;
                        break;
                    }
                }

                if (!referenced)
                {
                    // Hanging from UxrGrabbableObject but not referenced
                    DestroyGrabPoseTempEditorObjects(snapTransform);
                    DestroyImmediate(snapTransform);
                }
            }
            else
            {
                // Moved out of UxrGrabbableObject hierarchy
                DestroyGrabPoseTempEditorObjects(snapTransform);
                DestroyImmediate(snapTransform);
            }
        }

        /// <summary>
        ///     Destroys the temporal grab pose objects created by CreateGrabPoseTempEditorObjects()
        /// </summary>
        private void DestroyGrabPoseTempEditorObjects()
        {
            foreach (Object targetObject in serializedObject.targetObjects)
            {
                UxrGrabbableObjectSnapTransform snapTransform = targetObject as UxrGrabbableObjectSnapTransform;
                DestroyGrabPoseTempEditorObjects(snapTransform);
            }
        }

        /// <summary>
        ///     Destroys all temporal editor objects belonging to the snap transform
        /// </summary>
        /// <param name="snapTransform">Snap transform to delete temporal objects for</param>
        private void DestroyGrabPoseTempEditorObjects(UxrGrabbableObjectSnapTransform snapTransform)
        {
            if (snapTransform)
            {
                UxrGrabbableObjectPreviewMesh[] previewMeshComponents = snapTransform.GetComponentsInChildren<UxrGrabbableObjectPreviewMesh>();

                foreach (UxrGrabbableObjectPreviewMesh previewMeshComponent in previewMeshComponents)
                {
                    DestroyImmediate(previewMeshComponent.gameObject);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private static readonly Dictionary<SerializedObject, UxrGrabbableObjectSnapTransformEditor> s_openEditors = new Dictionary<SerializedObject, UxrGrabbableObjectSnapTransformEditor>();

        private UxrAvatar              _cachedSingleObjectAvatar;
        private UxrPreviewHandGripMesh _previewMeshLeft;
        private UxrPreviewHandGripMesh _previewMeshRight;

        #endregion
    }
}