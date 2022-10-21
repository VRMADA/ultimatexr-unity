// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHandEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Devices.Visualization
{
    [CustomEditor(typeof(UxrControllerHand))]
    public class UxrControllerHandEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyHasAvatarSource = serializedObject.FindProperty("_hasAvatarSource");
            _propertyAvatarPrefab    = serializedObject.FindProperty("_avatarPrefab");
            _propertyAvatarHandSide  = serializedObject.FindProperty("_avatarHandSide");
            _propertyHandPose        = serializedObject.FindProperty("_handPose");
            _propertyVariations      = serializedObject.FindProperty("_variations");
            _propertyHandRig         = serializedObject.FindProperty("_hand");
            _propertyThumb           = serializedObject.FindProperty("_thumb");
            _propertyIndex           = serializedObject.FindProperty("_index");
            _propertyMiddle          = serializedObject.FindProperty("_middle");
            _propertyRing            = serializedObject.FindProperty("_ring");
            _propertyLittle          = serializedObject.FindProperty("_little");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrControllerHand controllerHand = serializedObject.targetObject as UxrControllerHand;
            UxrAvatar         avatar         = _propertyAvatarPrefab.objectReferenceValue as UxrAvatar;
            UxrHandSide       handSide       = _propertyAvatarHandSide.enumValueIndex == (int)UxrHandSide.Left ? UxrHandSide.Left : UxrHandSide.Right;

            _foldoutPoseBaking = UxrEditorUtils.FoldoutStylish("Pose Baking:", _foldoutPoseBaking);

            if (_foldoutPoseBaking)
            {
                EditorGUILayout.PropertyField(_propertyHasAvatarSource, ContentHasAvatarSource);

                if (_propertyHasAvatarSource.boolValue)
                {
                    EditorGUILayout.PropertyField(_propertyAvatarPrefab, ContentAvatarPrefab);

                    if (avatar != null)
                    {
                        UxrEditorUtils.HandPoseDropdown(ContentHandPose, avatar, _propertyHandPose, out UxrHandPoseAsset selectedHandPose);
                        EditorGUILayout.PropertyField(_propertyAvatarHandSide, ContentAvatarHandSide);

                        GUI.enabled = avatar != null;

                        if (UxrEditorUtils.CenteredButton(ContentLoadRig))
                        {
                            UxrAvatarHand srcHand = avatar.GetHand(handSide);

                            if (srcHand.Wrist != null && _propertyHandRig.FindPropertyRelative("_wrist").objectReferenceValue == null)
                            {
                                _propertyHandRig.FindPropertyRelative("_wrist").objectReferenceValue = controllerHand.transform.FindRecursive(srcHand.Wrist.name);
                            }

                            GetFingerTransforms(_propertyHandRig, "_thumb",  controllerHand, srcHand.Thumb);
                            GetFingerTransforms(_propertyHandRig, "_index",  controllerHand, srcHand.Index);
                            GetFingerTransforms(_propertyHandRig, "_middle", controllerHand, srcHand.Middle);
                            GetFingerTransforms(_propertyHandRig, "_ring",   controllerHand, srcHand.Ring);
                            GetFingerTransforms(_propertyHandRig, "_little", controllerHand, srcHand.Little);
                        }

                        GUI.enabled = true;

                        if (UxrEditorUtils.CenteredButton(ContentClearRig))
                        {
                            _propertyHandRig.FindPropertyRelative("_wrist").objectReferenceValue = null;
                            ClearFingerTransforms(_propertyHandRig, "_thumb");
                            ClearFingerTransforms(_propertyHandRig, "_index");
                            ClearFingerTransforms(_propertyHandRig, "_middle");
                            ClearFingerTransforms(_propertyHandRig, "_ring");
                            ClearFingerTransforms(_propertyHandRig, "_little");
                        }

                        GUI.enabled = selectedHandPose != null && controllerHand.Hand.HasFingerData();

                        if (UxrEditorUtils.CenteredButton(ContentBakePose))
                        {
                            UxrAvatarRig.UpdateHandUsingDescriptor(controllerHand.Hand,
                                                                   selectedHandPose.GetHandDescriptor(handSide, selectedHandPose.PoseType, UxrBlendPoseType.OpenGrip),
                                                                   avatar.AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes,
                                                                   avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes);
                        }

                        GUI.enabled = true;
                    }
                }
            }

            _foldoutHand = UxrEditorUtils.FoldoutStylish("Hand information:", _foldoutHand);

            if (_foldoutHand)
            {
                EditorGUILayout.PropertyField(_propertyVariations, ContentVariations, true);

                if (avatar != null)
                {
                    EditorGUILayout.PropertyField(_propertyHandRig, ContentHandRig, true);
                }

                EditorGUILayout.PropertyField(_propertyThumb,  ContentThumb,  true);
                EditorGUILayout.PropertyField(_propertyIndex,  ContentIndex,  true);
                EditorGUILayout.PropertyField(_propertyMiddle, ContentMiddle, true);
                EditorGUILayout.PropertyField(_propertyRing,   ContentRing,   true);
                EditorGUILayout.PropertyField(_propertyLittle, ContentLittle, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to solve the finger bone references using an avatar as source.
        /// </summary>
        /// <param name="propertyHandRig">The serialized property with the destination hand rig</param>
        /// <param name="fingerField">The name of the finger field</param>
        /// <param name="controllerHand">The controller hand</param>
        /// <param name="srcFinger">The avatar finger with data that is already solved and that will be used as reference</param>
        private void GetFingerTransforms(SerializedProperty propertyHandRig, string fingerField, UxrControllerHand controllerHand, UxrAvatarFinger srcFinger)
        {
            GetFingerTransform(controllerHand.transform, srcFinger.Metacarpal,   propertyHandRig.FindPropertyRelative($"{fingerField}._metacarpal"));
            GetFingerTransform(controllerHand.transform, srcFinger.Proximal,     propertyHandRig.FindPropertyRelative($"{fingerField}._proximal"));
            GetFingerTransform(controllerHand.transform, srcFinger.Intermediate, propertyHandRig.FindPropertyRelative($"{fingerField}._intermediate"));
            GetFingerTransform(controllerHand.transform, srcFinger.Distal,       propertyHandRig.FindPropertyRelative($"{fingerField}._distal"));
        }

        /// <summary>
        ///     Tries to solve a transform reference looking for a Transform that has the same name in an avatar as reference.
        /// </summary>
        /// <param name="root">Root of all the hand transforms in the <see cref="UxrControllerHand" /></param>
        /// <param name="srcTransform">The transform that is used in the avatar and that will be used as reference</param>
        /// <param name="dstPropertyTransform">The <see cref="SerializedProperty" /> of the transform that needs to be solved</param>
        private void GetFingerTransform(Transform root, Transform srcTransform, SerializedProperty dstPropertyTransform)
        {
            if (srcTransform != null && dstPropertyTransform.objectReferenceValue == null)
            {
                dstPropertyTransform.objectReferenceValue = root.FindRecursive(srcTransform.name);
            }
        }

        /// <summary>
        ///     Clears all the transforms in a finger.
        /// </summary>
        /// <param name="propertyHandRig">The serialized property with the hand rig</param>
        /// <param name="fingerField">The name of the finger field</param>
        private void ClearFingerTransforms(SerializedProperty propertyHandRig, string fingerField)
        {
            propertyHandRig.FindPropertyRelative($"{fingerField}._metacarpal").objectReferenceValue   = null;
            propertyHandRig.FindPropertyRelative($"{fingerField}._proximal").objectReferenceValue     = null;
            propertyHandRig.FindPropertyRelative($"{fingerField}._intermediate").objectReferenceValue = null;
            propertyHandRig.FindPropertyRelative($"{fingerField}._distal").objectReferenceValue       = null;
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentHasAvatarSource => new GUIContent("Has Avatar Source",   "Allows to specify an avatar that uses the same rig as this hand. It allows to bake avatar hand poses for controllers into the hand");
        private GUIContent ContentAvatarPrefab    => new GUIContent("Avatar Prefab",       "Source avatar prefab with the same hand rig");
        private GUIContent ContentAvatarHandSide  => new GUIContent("Hand Side",           "Which hand side it is");
        private GUIContent ContentHandPose        => new GUIContent("Hand Pose",           "The hand pose from the avatar to bake into this hand");
        private GUIContent ContentVariations      => new GUIContent("Hand Variations",     "Registers the different available hands and materials if there are any");
        private GUIContent ContentHandRig         => new GUIContent("Hand Rig",            "The hand transforms");
        private GUIContent ContentLoadRig         => new GUIContent("Load Rig Data",       "Loads the rig data from the avatar into the component");
        private GUIContent ContentClearRig        => new GUIContent("Clear Rig Data",      "Clears all the hand rig references");
        private GUIContent ContentBakePose        => new GUIContent("Bake Pose into Hand", "Bakes the selected pose into the hand");
        private GUIContent ContentThumb           => new GUIContent("Thumb",               "Thumb finger information");
        private GUIContent ContentIndex           => new GUIContent("Index",               "Index finger information");
        private GUIContent ContentMiddle          => new GUIContent("Middle",              "Middle finger information");
        private GUIContent ContentRing            => new GUIContent("Ring",                "Ring finger information");
        private GUIContent ContentLittle          => new GUIContent("Little",              "Little finger information");

        private SerializedProperty _propertyHasAvatarSource;
        private SerializedProperty _propertyAvatarPrefab;
        private SerializedProperty _propertyAvatarHandSide;
        private SerializedProperty _propertyHandPose;
        private SerializedProperty _propertyVariations;
        private SerializedProperty _propertyHandRig;
        private SerializedProperty _propertyThumb;
        private SerializedProperty _propertyIndex;
        private SerializedProperty _propertyMiddle;
        private SerializedProperty _propertyRing;
        private SerializedProperty _propertyLittle;

        private bool _foldoutPoseBaking = true;
        private bool _foldoutHand       = true;

        #endregion
    }
}