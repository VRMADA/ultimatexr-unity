// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStandardAvatarControllerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Animation.IK;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Avatar.Rig;
using UltimateXR.Editor.Animation.IK;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UltimateXR.Editor.Avatar.Controllers
{
    /// <summary>
    ///     Custom inspector for the <see cref="UxrStandardAvatarController" /> component.
    /// </summary>
    [CustomEditor(typeof(UxrStandardAvatarController))]
    public sealed class UxrStandardAvatarControllerEditor : UnityEditor.Editor
    {
        #region Public Types & Data

        public const string PropAllowHandTracking    = "_allowHandTracking";
        public const string PropUseArmIK             = "_useArmIK";
        public const string PropArmIKElbowAperture   = "_armIKElbowAperture";
        public const string PropArmIKOverExtendMode  = "_armIKOverExtendMode";
        public const string PropUseBodyIK            = "_useBodyIK";
        public const string PropBodyIKSettings       = "_bodyIKSettings";
        //public const string PropUseLegIK             = "_useLegIK";
        public const string PropListControllerEvents = "_listControllerEvents";

        #endregion

        #region Unity

        /// <summary>
        ///     Caches the serialized properties and initializes data.
        /// </summary>
        private void OnEnable()
        {
            UxrStandardAvatarController selectedController = (UxrStandardAvatarController)serializedObject.targetObject;
            UxrAvatar                   avatar             = selectedController.Avatar;

            _propAllowHandTracking   = serializedObject.FindProperty(PropAllowHandTracking);
            _propUseArmIK            = serializedObject.FindProperty(PropUseArmIK);
            _propArmIKElbowAperture  = serializedObject.FindProperty(PropArmIKElbowAperture);
            _propArmIKOverExtendMode = serializedObject.FindProperty(PropArmIKOverExtendMode);
            _propUseBodyIK           = serializedObject.FindProperty(PropUseBodyIK);
            _propBodyIKSettings      = serializedObject.FindProperty(PropBodyIKSettings);
            //_propUseLegIK             = serializedObject.FindProperty(PropUseLegIK);
            _propListControllerEvents = serializedObject.FindProperty(PropListControllerEvents);

            if (_propListControllerEvents != null)
            {
                _reorderableEventList = CreateReorderableList(serializedObject, _propListControllerEvents, DrawControllerEventCallback);
            }

            // Get avatar info

            _reorderableEventList.elementHeightCallback = index =>
                                                          {
                                                              if (index >= selectedController.ControllerEvents.Count)
                                                              {
                                                                  return 0;
                                                              }

                                                              UxrHandPoseAsset handPoseAsset = avatar.GetHandPose(selectedController.ControllerEvents[index].PoseName);
                                                              return EditorGUIUtility.singleLineHeight * 3.5f + (handPoseAsset && handPoseAsset.PoseType == UxrHandPoseType.Blend ? EditorGUIUtility.singleLineHeight : 0.0f);
                                                          };
        }

        /// <summary>
        ///     Called by Unity to draw the inspector for the selected component(s).
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrAvatar avatar = ((UxrStandardAvatarController)serializedObject.targetObject).GetComponent<UxrAvatar>();

            // Handle UI

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            _foldoutGeneral = UxrEditorUtils.FoldoutStylish("General", _foldoutGeneral);

            if (_foldoutGeneral)
            {
                EditorGUILayout.PropertyField(_propAllowHandTracking, ContentAllowHandTracking);
            }

            if (avatar.AvatarRigType == UxrAvatarRigType.HalfOrFullBody)
            {
                _foldoutIK = UxrEditorUtils.FoldoutStylish("Inverse Kinematics", _foldoutIK);

                if (_foldoutIK)
                {
                    EditorGUILayout.PropertyField(_propUseArmIK, ContentUseArmIK);
                    
                    if (!avatar.AvatarRig.HasArmData())
                    {
                        EditorGUILayout.HelpBox($"To use arm IK, the {nameof(UxrAvatar)} component needs arm references in the Avatar Rig section", MessageType.Warning);
                    }
                    else
                    {
                        if (_propUseArmIK.boolValue)
                        {
                            EditorGUI.indentLevel++;

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.Slider(_propArmIKElbowAperture, 0.0f, 1.0f, ContentArmIKElbowAperture);
                            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                            {
                                UxrIKSolver.GetComponents(avatar, true).OfType<UxrArmIKSolver>().ForEach(s => s.RelaxedElbowAperture = _propArmIKElbowAperture.floatValue);
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(_propArmIKOverExtendMode, ContentArmIKOverExtendMode);
                            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                            {
                                UxrIKSolver.GetComponents(avatar, true).OfType<UxrArmIKSolver>().ForEach(s => s.OverExtendMode = (UxrArmOverExtendMode)_propArmIKOverExtendMode.enumValueIndex);
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    bool hasUpperBodyReferences = avatar.AvatarRig.HasAnyUpperBodyIKReference();

                    EditorGUILayout.PropertyField(_propUseBodyIK, ContentUseBodyIK);

                    if (!hasUpperBodyReferences)
                    {
                        EditorGUILayout.HelpBox($"To use body IK, the {nameof(UxrAvatar)} component needs upper body references in the Avatar Rig section", MessageType.Warning);
                    }

                    GUI.enabled = hasUpperBodyReferences;

                    if (_propUseBodyIK.boolValue && hasUpperBodyReferences)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_propBodyIKSettings, ContentBodyIKSettings);
                        EditorGUI.indentLevel--;
                    }

                    GUI.enabled = false;
                    //EditorGUILayout.PropertyField(_propUseLegIK);
                    EditorGUILayout.Toggle(ContentUseLegIK, false);
                    GUI.enabled = true;
                }
            }

            _foldoutHandEvents = UxrEditorUtils.FoldoutStylish("Special Hand Pose Events", _foldoutHandEvents);

            if (_foldoutHandEvents)
            {
                EditorGUILayout.LabelField("Hand poses based on controller input events:", EditorStyles.boldLabel);

                if (string.IsNullOrEmpty(avatar.PrefabGuid) || !avatar.GetAllHandPoses().Any())
                {
                    EditorGUILayout.HelpBox($"To start using this functionality add hand poses to the {nameof(UxrAvatar)} component first.", MessageType.Info);
                }
                else
                {
                    _reorderableEventList?.DoLayoutList();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     Called by Unity to draw gizmos. It's used to display some visual aids when the avatar has a
        ///     <see cref="UxrAvatarRigType.HalfOrFullBody" /> configuration.
        /// </summary>
        private void OnSceneGUI()
        {
            UxrStandardAvatarController standardAvatarController = (UxrStandardAvatarController)serializedObject.targetObject;
            UxrAvatar                   avatar                   = standardAvatarController.Avatar;

            if (standardAvatarController && standardAvatarController.UseBodyIK && avatar && avatar.AvatarRigType == UxrAvatarRigType.HalfOrFullBody)
            {
                Transform avatarTransform = avatar.transform;

                float neckBaseHeight    = _propBodyIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyNeckBaseHeight).floatValue;
                float neckForwardOffset = _propBodyIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyNeckForwardOffset).floatValue;
                float eyesBaseHeight    = _propBodyIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesBaseHeight).floatValue;
                float eyesForwardOffset = _propBodyIKSettings.FindPropertyRelative(UxrIKBodySettingsDrawer.PropertyEyesForwardOffset).floatValue;

                float neckRadius     = 0.08f;
                float eyesSeparation = 0.065f;
                float eyesRadius     = 0.01f;

                if (avatar.AvatarRig.Head.Neck == null)
                {
                    Handles.DrawSolidDisc(avatarTransform.position + avatarTransform.GetScaledVector(0.0f, neckBaseHeight, neckForwardOffset), avatarTransform.up, neckRadius);
                }

                Handles.DrawSolidDisc(avatarTransform.position + avatarTransform.GetScaledVector(-eyesSeparation * 0.5f, eyesBaseHeight, eyesForwardOffset), avatarTransform.forward, eyesRadius);
                Handles.DrawSolidDisc(avatarTransform.position + avatarTransform.GetScaledVector(eyesSeparation * 0.5f,  eyesBaseHeight, eyesForwardOffset), avatarTransform.forward, eyesRadius);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates a reorderable list for the given controller events.
        /// </summary>
        /// <param name="serializedObject">Serialized object representing the selected component(s)</param>
        /// <param name="propertyListControllerEvents">The serialized property with the controller events</param>
        /// <param name="drawCallback">A callback to draw the list</param>
        /// <returns></returns>
        private static ReorderableList CreateReorderableList(SerializedObject serializedObject, SerializedProperty propertyListControllerEvents, ReorderableList.ElementCallbackDelegate drawCallback)
        {
            ReorderableList reorderableEventList = new ReorderableList(serializedObject, propertyListControllerEvents, true, true, true, true);
            reorderableEventList.drawHeaderCallback  = rect => { EditorGUI.LabelField(rect, "Drag elements to reorder them by priority from top to bottom"); };
            reorderableEventList.drawElementCallback = drawCallback;
            return reorderableEventList;
        }

        /// <summary>
        ///     Helper method that draws an event entry in the inspector.
        /// </summary>
        /// <param name="rect">The rect where to draw the element</param>
        /// <param name="index">The element index in the list</param>
        /// <param name="isActive">Whether the element is active</param>
        /// <param name="isFocused">Whether the element is focused</param>
        private void DrawControllerEventCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            UxrStandardAvatarController selectedController = (UxrStandardAvatarController)serializedObject.targetObject;
            UxrAvatar                   avatar             = selectedController.Avatar;
            SerializedProperty          element            = _reorderableEventList.serializedProperty.GetArrayElementAtIndex(index);

            int nLineIndex = 0;

            Rect GetCurrentRect()
            {
                return new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * nLineIndex, rect.width, EditorGUIUtility.singleLineHeight);
            }

            // Animation type

            EditorGUI.PropertyField(GetCurrentRect(), element.FindPropertyRelative("_animationType"), new GUIContent("Animation type"));

            nLineIndex++;

            // Controller button mask

            int buttons = EditorGUI.MaskField(GetCurrentRect(), new GUIContent("Controller button(s)"), element.FindPropertyRelative("_buttons").intValue, UxrEditorUtils.GetControllerButtonNames().ToArray());
            element.FindPropertyRelative("_buttons").intValue = buttons;

            nLineIndex++;

            // List animator parameters

            UxrEditorUtils.HandPoseDropdown(GetCurrentRect(), new GUIContent("Hand Pose"), avatar, element.FindPropertyRelative("_handPose"), out UxrHandPoseAsset selectedHandPose);

            nLineIndex++;

            if (selectedHandPose && selectedHandPose.PoseType == UxrHandPoseType.Blend)
            {
                element.FindPropertyRelative("_poseBlendValue").floatValue = EditorGUI.Slider(GetCurrentRect(), new GUIContent("Pose Blend"), element.FindPropertyRelative("_poseBlendValue").floatValue, 0.0f, 1.0f);
            }
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentAllowHandTracking   { get; } = new GUIContent("Allow Hand Tracking",    "Switches to hand-tracking to update the avatar hands when available");
        private GUIContent ContentUseArmIK            { get; } = new GUIContent("Use Arm IK",             "Whether to try to naturally orient the arms using the position of the hands");
        private GUIContent ContentArmIKElbowAperture  { get; } = new GUIContent("Elbow Neutral Aperture", "Controls how close the elbows will be to the body when arms are computed using inverse kinematics");
        private GUIContent ContentArmIKOverExtendMode { get; } = new GUIContent("Arm Over-Extend",        "Controls what to do when the user extends the hands over the avatar's arm reach");
        private GUIContent ContentUseBodyIK           { get; } = new GUIContent("Use Body IK",            "Whether to try to naturally orient the avatar body using the positions of the head and hand");
        private GUIContent ContentBodyIKSettings      { get; } = new GUIContent("Body IK Settings");
        private GUIContent ContentUseLegIK            { get; } = new GUIContent("Use Leg IK (TBD)", "");

        private SerializedProperty _propAllowHandTracking;
        private SerializedProperty _propUseArmIK;
        private SerializedProperty _propArmIKElbowAperture;
        private SerializedProperty _propArmIKOverExtendMode;
        private SerializedProperty _propUseBodyIK;
        private SerializedProperty _propBodyIKSettings;
        //private SerializedProperty _propUseLegIK;
        private SerializedProperty _propListControllerEvents;

        private bool            _foldoutGeneral    = true;
        private bool            _foldoutIK         = true;
        private bool            _foldoutHandEvents = true;
        private ReorderableList _reorderableEventList;

        #endregion
    }
}