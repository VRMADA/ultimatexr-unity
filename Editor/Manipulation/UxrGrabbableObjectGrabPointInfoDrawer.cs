// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectGrabPointInfoDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation
{
    /// <summary>
    ///     Custom property drawer for <see cref="UxrGrabPointInfo" />.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrGrabPointInfo))]
    public class UxrGrabbableObjectGrabPointInfoDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.FindPropertyRelative(PropertyFoldout).boolValue)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            int lines = 10;

            if (property.FindPropertyRelative(PropertyUseDefaultGrabButtons).boolValue == false)
            {
                // Grab buttons
                lines += 1;
            }

            if (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false)
            {
                // Which hand
                lines += 1;
            }

            // Check if we have an avatar controller selected and if so, also if the pose is a blend one

            SerializedProperty gripPoseInfoProperty = GetGripPoseInfoSerializedProperty(property);
            GameObject         selectedPrefab       = (property.serializedObject.targetObject as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabForGrips();
            UxrAvatar          selectedAvatarPrefab = selectedPrefab != null ? selectedPrefab.GetComponent<UxrAvatar>() : null;

            if (selectedAvatarPrefab)
            {
                if (property.FindPropertyRelative(PropertyHideHandGrabberRenderer).boolValue == false)
                {
                    // Pose
                    lines += 1;

                    UxrHandPoseAsset selectedHandPose = gripPoseInfoProperty.FindPropertyRelative(PropertyGripHandPose).objectReferenceValue as UxrHandPoseAsset;
                    UxrHandPoseAsset avatarHandPose   = selectedHandPose != null ? selectedAvatarPrefab.GetAllHandPoses().FirstOrDefault(p => p.name == selectedHandPose.name) : null;

                    if (avatarHandPose != null && avatarHandPose.PoseType == UxrHandPoseType.Blend)
                    {
                        // Blend
                        lines += 1;
                    }
                }
            }

            if (property.FindPropertyRelative(PropertySnapMode).enumValueIndex != (int)UxrSnapToHandMode.DontSnap)
            {
                // Snap direction + use self or other transform reference
                lines += 2;

                if (property.FindPropertyRelative(PropertySnapReference).enumValueIndex == (int)UxrSnapReference.UseOtherTransform)
                {
                    bool leftHandCompatible = property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue ||
                                              (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false && property.FindPropertyRelative(PropertyHandSide).enumValueIndex == (int)UxrHandSide.Left);
                    bool rightHandCompatible = property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue ||
                                               (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false && property.FindPropertyRelative(PropertyHandSide).enumValueIndex == (int)UxrHandSide.Right);

                    if (leftHandCompatible)
                    {
                        lines++; // left snap transform
                    }
                    if (rightHandCompatible)
                    {
                        lines++; // right snap transform
                    }

                    if ((leftHandCompatible && gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue == null) ||
                        (rightHandCompatible && gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue == null))
                    {
                        lines++; // Create snap button(s)
                    }
                }

                if (property.FindPropertyRelative(PropertySnapMode).enumValueIndex == (int)UxrSnapToHandMode.RotationOnly ||
                    property.FindPropertyRelative(PropertySnapMode).enumValueIndex == (int)UxrSnapToHandMode.PositionAndRotation)
                {
                    lines += 1; // Align to controller

                    if (property.FindPropertyRelative(PropertyAlignToController).boolValue)
                    {
                        lines += 1; // Align to controller axes
                    }
                }
            }

            if (property.FindPropertyRelative(PropertyGrabProximityMode).enumValueIndex == (int)UxrGrabProximityMode.UseProximity)
            {
                // Distance grab + reference + optional other reference transform
                lines += property.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue ? 2 : 3;
            }
            else if (property.FindPropertyRelative(PropertyGrabProximityMode).enumValueIndex == (int)UxrGrabProximityMode.BoxConstrained)
            {
                // Box
                lines++;
            }

            if (property.FindPropertyRelative(PropertyGrabberProximityUseDefault).boolValue == false)
            {
                // Grabber proximity point index
                lines++;
            }

            return lines * EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Copies relevant properties from a grab point to another. It is used when adding new grab points to a
        ///     <see cref="UxrGrabbableObject" />, copying values from the previous grab point since it's probable that most will
        ///     be the same.
        /// </summary>
        /// <param name="propertySrc">Source <see cref="UxrGrabPointInfo" /> serialized property</param>
        /// <param name="propertyDst">Destination <see cref="UxrGrabPointInfo" /> serialized property</param>
        /// <param name="foldout">Whether to foldout the destination property</param>
        internal static void CopyRelevantProperties(SerializedProperty propertySrc, SerializedProperty propertyDst, bool foldout)
        {
            propertyDst.FindPropertyRelative(PropertyFoldout).boolValue                           = foldout;
            propertyDst.FindPropertyRelative(PropertyGrabMode).enumValueIndex                     = propertySrc.FindPropertyRelative(PropertyGrabMode).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertyUseDefaultGrabButtons).boolValue             = propertySrc.FindPropertyRelative(PropertyUseDefaultGrabButtons).boolValue;
            propertyDst.FindPropertyRelative(PropertyInputButtons).intValue                       = propertySrc.FindPropertyRelative(PropertyInputButtons).intValue;
            propertyDst.FindPropertyRelative(PropertyBothHandsCompatible).boolValue               = propertySrc.FindPropertyRelative(PropertyBothHandsCompatible).boolValue;
            propertyDst.FindPropertyRelative(PropertyHandSide).enumValueIndex                     = propertySrc.FindPropertyRelative(PropertyHandSide).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertyHideHandGrabberRenderer).boolValue           = propertySrc.FindPropertyRelative(PropertyHideHandGrabberRenderer).boolValue;
            propertyDst.FindPropertyRelative(PropertySnapMode).enumValueIndex                     = propertySrc.FindPropertyRelative(PropertySnapMode).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertySnapDirection).enumValueIndex                = propertySrc.FindPropertyRelative(PropertySnapDirection).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertySnapReference).enumValueIndex                = propertySrc.FindPropertyRelative(PropertySnapReference).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertyAlignToController).boolValue                 = propertySrc.FindPropertyRelative(PropertyAlignToController).boolValue;
            propertyDst.FindPropertyRelative(PropertyAlignToControllerAxes).objectReferenceValue  = propertySrc.FindPropertyRelative(PropertyAlignToControllerAxes).objectReferenceValue;
            propertyDst.FindPropertyRelative(PropertyGrabProximityMode).enumValueIndex            = propertySrc.FindPropertyRelative(PropertyGrabProximityMode).enumValueIndex;
            propertyDst.FindPropertyRelative(PropertyGrabProximityBox).objectReferenceValue       = null;
            propertyDst.FindPropertyRelative(PropertyMaxDistanceGrab).floatValue                  = propertySrc.FindPropertyRelative(PropertyMaxDistanceGrab).floatValue;
            propertyDst.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue     = propertySrc.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue;
            propertyDst.FindPropertyRelative(PropertyGrabProximityTransform).objectReferenceValue = null;
            propertyDst.FindPropertyRelative(PropertyGrabberProximityUseDefault).boolValue        = propertySrc.FindPropertyRelative(PropertyGrabberProximityUseDefault).boolValue;
            propertyDst.FindPropertyRelative(PropertyGrabberProximityIndex).intValue              = propertySrc.FindPropertyRelative(PropertyGrabberProximityIndex).intValue;
            propertyDst.FindPropertyRelative(PropertyEnableOnHandNear).objectReferenceValue       = null;
            
            // Create grip pose entries

            propertyDst.FindPropertyRelative(PropertyAvatarGripPoseEntries).arraySize = propertySrc.FindPropertyRelative(PropertyAvatarGripPoseEntries).arraySize;

            for (int i = 0; i < propertySrc.FindPropertyRelative(PropertyAvatarGripPoseEntries).arraySize; ++i)
            {
                SerializedProperty gripPoseEntriesSrc = propertySrc.FindPropertyRelative(PropertyAvatarGripPoseEntries).GetArrayElementAtIndex(i);
                SerializedProperty gripPoseEntriesDst = propertyDst.FindPropertyRelative(PropertyAvatarGripPoseEntries).GetArrayElementAtIndex(i);

                gripPoseEntriesDst.FindPropertyRelative(PropertyGripPoseAvatarGuid).stringValue                   = gripPoseEntriesSrc.FindPropertyRelative(PropertyGripPoseAvatarGuid).stringValue;
                gripPoseEntriesDst.FindPropertyRelative(PropertyGripHandPose).objectReferenceValue                = null;
                gripPoseEntriesDst.FindPropertyRelative(PropertyGripPoseBlendValue).floatValue                    = 0.5f;
                gripPoseEntriesDst.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue  = null;
                gripPoseEntriesDst.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue = null;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called when the GUI for a GrabPointInfo needs to be drawn
        /// </summary>
        /// <param name="position">Position inside the window</param>
        /// <param name="property">Property that contains the GrabPointInfo data</param>
        /// <param name="label">Property label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Find grab point index

            SerializedProperty propertyGrabPoint            = property.serializedObject.FindProperty(UxrGrabbableObjectEditor.PropertyGrabPoint);
            SerializedProperty propertyAdditionalGrabPoints = property.serializedObject.FindProperty(UxrGrabbableObjectEditor.PropertyAdditionalGrabPoints);

            int grabPointIndex = property.propertyPath == propertyGrabPoint.propertyPath ? 0 : -1;

            if (grabPointIndex == -1)
            {
                for (int i = 0; i < propertyAdditionalGrabPoints.arraySize; ++i)
                {
                    if (propertyAdditionalGrabPoints.GetArrayElementAtIndex(i).propertyPath == property.propertyPath)
                    {
                        grabPointIndex = i + 1;
                        break;
                    }
                }
            }

            string grabPointEditorName = UxrGrabPointIndex.GetIndexDisplayName(property.serializedObject.targetObject as UxrGrabbableObject, grabPointIndex);

            // Create foldout

            property.FindPropertyRelative(PropertyFoldout).boolValue = EditorGUI.Foldout(UxrEditorUtils.GetRect(position, 0), property.FindPropertyRelative(PropertyFoldout).boolValue, grabPointEditorName);

            int line = 1;

            if (property.FindPropertyRelative(PropertyFoldout).boolValue)
            {
                SerializedProperty gripPoseInfoProperty = GetGripPoseInfoSerializedProperty(property);
                GameObject         selectedPrefab       = (property.serializedObject.targetObject as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabForGrips();
                UxrAvatar          selectedAvatarPrefab = selectedPrefab != null ? selectedPrefab.GetComponent<UxrAvatar>() : null;

                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyEditorName),            ContentEditorName);
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabMode),              ContentGrabMode);
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyUseDefaultGrabButtons), ContentUseDefaultGrabButtons);

                if (property.FindPropertyRelative(PropertyUseDefaultGrabButtons).boolValue == false)
                {
                    int buttons = EditorGUI.MaskField(UxrEditorUtils.GetRect(position, line++), ContentInputButtons, property.FindPropertyRelative(PropertyInputButtons).intValue, UxrEditorUtils.GetControllerButtonNames().ToArray());
                    property.FindPropertyRelative(PropertyInputButtons).intValue = buttons;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyBothHandsCompatible), ContentBothHandsCompatible);
                if (EditorGUI.EndChangeCheck())
                {
                    SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                }

                if (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyHandSide), ContentHandSide);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                    }
                }

                bool leftHandCompatible = property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue ||
                                          (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false && property.FindPropertyRelative(PropertyHandSide).enumValueIndex == (int)UxrHandSide.Left);
                bool rightHandCompatible = property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue ||
                                           (property.FindPropertyRelative(PropertyBothHandsCompatible).boolValue == false && property.FindPropertyRelative(PropertyHandSide).enumValueIndex == (int)UxrHandSide.Right);

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyHideHandGrabberRenderer), ContentHideHandGrabberRenderer);
                if (EditorGUI.EndChangeCheck())
                {
                    SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertySnapMode), ContentSnapMode);
                if (EditorGUI.EndChangeCheck())
                {
                    SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                }

                if (property.FindPropertyRelative(PropertySnapMode).enumValueIndex != (int)UxrSnapToHandMode.DontSnap)
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertySnapDirection), ContentSnapDirection);

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertySnapReference), ContentSnapReference);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                    }
                }

                Color guiColor = GUI.color;

                if (selectedAvatarPrefab != null)
                {
                    GUI.color = UxrGrabbableObjectEditor.GUIColorAvatarControllerParameter;

                    if (property.FindPropertyRelative(PropertyHideHandGrabberRenderer).boolValue == false)
                    {
                        // Pose name

                        List<UxrHandPoseAsset> handPoses         = selectedAvatarPrefab.GetAllHandPoses().ToList();
                        int                    selectedPoseIndex = -1;

                        if (handPoses.Any())
                        {
                            EditorGUI.BeginChangeCheck();
                            UxrHandPoseAsset handPose = gripPoseInfoProperty.FindPropertyRelative(PropertyGripHandPose).objectReferenceValue as UxrHandPoseAsset;
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (handPose == null)
                                {
                                    SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                                }
                            }

                            if (handPose != null)
                            {
                                UxrHandPoseAsset selectedHandPoseAsset = handPoses.FirstOrDefault(p => p.name == handPose.name);

                                if (selectedHandPoseAsset != null)
                                {
                                    selectedPoseIndex = handPoses.IndexOf(selectedHandPoseAsset);
                                }
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        selectedPoseIndex = EditorGUI.Popup(UxrEditorUtils.GetRect(position, line++),
                                                            ContentGripPoseName,
                                                            selectedPoseIndex,
                                                            UxrEditorUtils.ToGUIContentArray(handPoses.Select(p => p.name)));
                        if (EditorGUI.EndChangeCheck())
                        {
                            gripPoseInfoProperty.FindPropertyRelative(PropertyGripHandPose).objectReferenceValue = handPoses[selectedPoseIndex];
                            SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                        }

                        // Pose blend slider?

                        if (selectedPoseIndex != -1 && handPoses[selectedPoseIndex].PoseType == UxrHandPoseType.Blend)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.Slider(UxrEditorUtils.GetRect(position, line++), gripPoseInfoProperty.FindPropertyRelative(PropertyGripPoseBlendValue), 0.0f, 1.0f, ContentGripPoseBlendVarValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex, UxrPreviewPoseRegeneration.OnlyBlend);
                            }
                        }
                    }
                }
                else
                {
                    // Default GripPoseInfo. No hand pose.

                    gripPoseInfoProperty.FindPropertyRelative(PropertyGripHandPose).objectReferenceValue = null;
                    gripPoseInfoProperty.FindPropertyRelative(PropertyGripPoseBlendValue).floatValue     = -1.0f;
                }

                if (property.FindPropertyRelative(PropertySnapMode).enumValueIndex != (int)UxrSnapToHandMode.DontSnap && property.FindPropertyRelative(PropertySnapReference).enumValueIndex == (int)UxrSnapReference.UseOtherTransform)
                {
                    if (leftHandCompatible)
                    {
                        EditorGUI.BeginChangeCheck();
                        Transform snapTransformOld = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue as Transform;
                        EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft), ContentGripAlignTransformHandLeft);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Transform snapTransformNew = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue as Transform;
                            Object[]  targetObjects    = property.serializedObject.targetObjects;

                            if (targetObjects.Length == 1 && snapTransformNew != null && !snapTransformNew.HasParent((targetObjects[0] as UxrGrabbableObject).transform))
                            {
                                EditorUtility.DisplayDialog("Invalid snap transform", "Snap transform must be a child GameObject of the GameObject containing this component", "OK");
                                gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue = snapTransformOld;
                            }
                            else
                            {
                                SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                            }
                        }
                    }

                    if (rightHandCompatible)
                    {
                        EditorGUI.BeginChangeCheck();
                        Transform snapTransformOld = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue as Transform;
                        EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight), ContentGripAlignTransformHandRight);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Transform snapTransformNew = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue as Transform;
                            Object[]  targetObjects    = property.serializedObject.targetObjects;

                            if (targetObjects.Length == 1 && snapTransformNew && !snapTransformNew.HasParent((targetObjects[0] as UxrGrabbableObject).transform))
                            {
                                EditorUtility.DisplayDialog("Invalid snap transform", "Snap transform must be a child GameObject of the GameObject containing this component", "OK");
                                gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue = snapTransformOld;
                            }
                            else
                            {
                                SetGrabbableObjectGrabPoseMeshesDirty(property, grabPointIndex);
                            }
                        }
                    }

                    int       snapButtonPadding    = 20;
                    int       snapButtonSeparation = 20;
                    bool      buttonDrawn          = false;
                    Transform leftSnap             = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue as Transform;
                    Transform rightSnap            = gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue as Transform;

                    if (leftHandCompatible && leftSnap == null && property.serializedObject.targetObjects.Count() == 1)
                    {
                        if (GUI.Button(UxrEditorUtils.GetRect(position, line, 2, 0, snapButtonSeparation, snapButtonPadding, snapButtonPadding, true), ContentCreateGripAlignTransformHandLeft))
                        {
                            leftSnap = CreateNewSnapTransform(property, UxrHandSide.Left, selectedAvatarPrefab, grabPointEditorName, leftSnap, rightSnap);

                            if (leftSnap != null)
                            {
                                gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandLeft).objectReferenceValue = leftSnap;
                            }
                        }

                        buttonDrawn = true;
                    }

                    if (rightHandCompatible && rightSnap == null && property.serializedObject.targetObjects.Count() == 1)
                    {
                        if (GUI.Button(UxrEditorUtils.GetRect(position, line, 2, 1, snapButtonSeparation, snapButtonPadding, snapButtonPadding, true), ContentCreateGripAlignTransformHandRight))
                        {
                            rightSnap = CreateNewSnapTransform(property, UxrHandSide.Right, selectedAvatarPrefab, grabPointEditorName, leftSnap, rightSnap);

                            if (rightSnap != null)
                            {
                                gripPoseInfoProperty.FindPropertyRelative(PropertyGripAlignTransformHandRight).objectReferenceValue = rightSnap;
                            }
                        }

                        buttonDrawn = true;
                    }

                    if (buttonDrawn)
                    {
                        line++;
                    }
                }

                if (GUI.color != guiColor)
                {
                    GUI.color = guiColor;
                }

                if (property.FindPropertyRelative(PropertySnapMode).enumValueIndex == (int)UxrSnapToHandMode.RotationOnly ||
                    property.FindPropertyRelative(PropertySnapMode).enumValueIndex == (int)UxrSnapToHandMode.PositionAndRotation)
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAlignToController), ContentAlignToController);

                    if (property.FindPropertyRelative(PropertyAlignToController).boolValue)
                    {
                        EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAlignToControllerAxes), ContentAlignToControllerAxes);
                    }
                }

                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabProximityMode), ContentGrabProximityMode);

                if (property.FindPropertyRelative(PropertyGrabProximityMode).enumValueIndex == (int)UxrGrabProximityMode.UseProximity)
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyMaxDistanceGrab), ContentMaxDistanceGrab);

                    int popup = EditorGUI.Popup(UxrEditorUtils.GetRect(position, line++),
                                                ContentGrabbableDistanceReference,
                                                property.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue ? 0 : 1,
                                                new[] { new GUIContent("Grip"), new GUIContent("Use other transform") });

                    if (popup == 1)
                    {
                        EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabProximityTransform), ContentGrabProximityTransform);
                        property.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue = false;
                    }
                    else
                    {
                        property.FindPropertyRelative(PropertyGrabProximityTransformUseSelf).boolValue = true;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabProximityBox), ContentGrabProximityBox);
                }

                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabberProximityUseDefault), ContentGrabberProximityUseDefault);

                if (property.FindPropertyRelative(PropertyGrabberProximityUseDefault).boolValue == false)
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyGrabberProximityIndex), ContentGrabberProximityIndex);
                }

                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyEnableOnHandNear), ContentEnableOnHandNear);
            }

            EditorGUI.EndProperty();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates a new snap GameObject.
        /// </summary>
        /// <param name="property"><see cref="UxrGrabPointInfo" /> serialized property</param>
        /// <param name="handSide">Which hand to create the snap transform for</param>
        /// <param name="selectedAvatarPrefab">The selected avatar prefab or null if the default avatar is selected</param>
        /// <param name="grabPointEditorName">The grab point name</param>
        /// <param name="leftSnap">The current snap transform for the right side</param>
        /// <param name="rightSnap">The current snap transform for the left side</param>
        /// <returns>New snap transform or null if there was an error</returns>
        private static Transform CreateNewSnapTransform(SerializedProperty property, UxrHandSide handSide, UxrAvatar selectedAvatarPrefab, string grabPointEditorName, Transform leftSnap, Transform rightSnap)
        {
            Transform snapParent = leftSnap != null  ? leftSnap.parent :
                                   rightSnap != null ? rightSnap.parent : null;
            string prefabName = selectedAvatarPrefab != null ? selectedAvatarPrefab.name : UxrGrabbableObject.DefaultAvatarName;

            UxrGrabbableObject grabbableObject = property.serializedObject.targetObject as UxrGrabbableObject;

            if (grabbableObject)
            {
                // Create parent?

                if (snapParent == null)
                {
                    // Root was created?

                    Transform grabsRoot = grabbableObject.transform.Find($"{GrabsRootName}");

                    if (grabsRoot == null)
                    {
                        grabsRoot = new GameObject(GrabsRootName).transform;
                        grabsRoot.SetParent(grabbableObject.transform);
                        grabsRoot.SetPositionAndRotation(grabbableObject.transform);
                        Undo.RegisterCreatedObjectUndo(grabsRoot.gameObject, "New snap root");
                    }

                    // Avatar root was created?

                    Transform avatarRoot = grabsRoot.transform.Find($"{prefabName}");

                    if (avatarRoot == null)
                    {
                        avatarRoot = new GameObject(prefabName).transform;
                        avatarRoot.SetParent(grabsRoot.transform);
                        avatarRoot.SetPositionAndRotation(grabsRoot.transform);
                        Undo.RegisterCreatedObjectUndo(avatarRoot.gameObject, "New avatar root");
                    }

                    // Create snap parent

                    snapParent = new GameObject(grabPointEditorName).transform;
                    snapParent.SetParent(avatarRoot);
                    snapParent.SetPositionAndRotation(avatarRoot);
                    Undo.RegisterCreatedObjectUndo(snapParent.gameObject, "New snap parent");
                }

                // Try to expand parent

                var hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

                if (hierarchyWindowType != null)
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                    var methodInfo      = hierarchyWindowType.GetMethod("SetExpandedRecursive");
                    var hierarchyWindow = EditorWindow.focusedWindow;

                    if (methodInfo != null && hierarchyWindow != null)
                    {
                        methodInfo.Invoke(hierarchyWindow, new object[] { snapParent.gameObject.GetInstanceID(), true });
                    }
                }

                // Create snap transform

                Transform newSnap = new GameObject($"{handSide} Grab").transform;
                newSnap.SetParent(snapParent);
                newSnap.SetPositionAndRotation(snapParent);
                Undo.RegisterCreatedObjectUndo(newSnap.gameObject, "New snap transform");

                return newSnap;
            }

            return null;
        }

        /// <summary>
        ///     Using the SerializedProperty representing a GrabPointInfo, returns the SerializedProperty
        ///     representing the GripPoseInfo of the selected Avatar Controller for that grab point.
        /// </summary>
        /// <param name="property">SerializedProperty that represents a GrabPointInfo</param>
        /// <returns>
        ///     SerializedProperty representing the GripPoseInfo for the given
        ///     GrabPoint/AvatarController combo. If no Avatar Controller is selected then the default
        ///     GripPoseInfo is returned.
        /// </returns>
        private static SerializedProperty GetGripPoseInfoSerializedProperty(SerializedProperty property)
        {
            string selectedPrefabGuid = (property.serializedObject.targetObject as UxrGrabbableObject).Editor_GetSelectedAvatarPrefabGuidForGrips();

            if (!string.IsNullOrEmpty(selectedPrefabGuid))
            {
                for (int i = 0; i < property.FindPropertyRelative(PropertyAvatarGripPoseEntries).arraySize; ++i)
                {
                    SerializedProperty elementProperty           = property.FindPropertyRelative(PropertyAvatarGripPoseEntries).GetArrayElementAtIndex(i);
                    SerializedProperty elementAvatarGuidProperty = elementProperty.FindPropertyRelative(PropertyGripPoseAvatarGuid);

                    if (elementAvatarGuidProperty.stringValue == selectedPrefabGuid)
                    {
                        return elementProperty;
                    }
                }
            }

            return property.FindPropertyRelative(PropertyDefaultGripPoseInfo);
        }

        /// <summary>
        ///     Sets the dirty flag of the UxrGrabbableObject(s) this grabPoint belongs to.
        ///     Supports multi-selection.
        /// </summary>
        /// <param name="property">SerializedProperty source</param>
        /// <param name="grabPointIndex">The grab point index whose preview meshes should be re-generated. -1 to regenerate all.</param>
        /// <param name="regeneration">Which level of regeneration is required</param>
        private void SetGrabbableObjectGrabPoseMeshesDirty(SerializedProperty property, int grabPointIndex, UxrPreviewPoseRegeneration regeneration = UxrPreviewPoseRegeneration.Complete)
        {
            property.serializedObject.FindProperty(PropertyPreviewPosesRegenerateType).intValue  = (int)regeneration;
            property.serializedObject.FindProperty(PropertyPreviewPosesRegenerateIndex).intValue = grabPointIndex;
        }

        #endregion

        #region Private Types & Data

        // Property labels and tooltip help 

        private GUIContent ContentEditorName                        { get; } = new GUIContent("Name In Editor",                "The display name showed for this grab point in the editor's foldout label.");
        private GUIContent ContentGrabMode                          { get; } = new GUIContent("Grab Mode",                     "Whether the object will a) Be grabbed while keeping the grab button pressed, b) Keep being grabbed until the grab button is pressed again or c) Keep being grabbed until another hand grabs it or it is requested through scripting.");
        private GUIContent ContentUseDefaultGrabButtons             { get; } = new GUIContent("Default Grab Button(s)",        "Whether the object is grabbed using the grab button specified in the Avatar's Standard Controller component. This allows to override the grab button for certain objects.");
        private GUIContent ContentInputButtons                      { get; } = new GUIContent("Grab Button(s)",                "The button or combination of buttons that will be required to grab this object.");
        private GUIContent ContentBothHandsCompatible               { get; } = new GUIContent("Both Hands Compatible",         "Whether this object can be grabbed using both hands.");
        private GUIContent ContentHandSide                          { get; } = new GUIContent("Compatible Hand",               "The hand/controller this object can be picked with, if the controller can only be grabbed with one hand.");
        private GUIContent ContentHideHandGrabberRenderer           { get; } = new GUIContent("Hide Hand Renderer",            $"Whether the hand renderers specified in the {nameof(UxrGrabber)} component should be hidden while grabbing.");
        private GUIContent ContentGripPoseName                      { get; } = new GUIContent("Grip Pose",                     "Selects the hand grip pose to use for the selected avatar/grab point combo.");
        private GUIContent ContentGripPoseBlendVarValue             { get; } = new GUIContent("Pose Blend",                    "Move the slider to open/close the grip and adjust it to the object size and shape.");
        private GUIContent ContentGripAlignTransformHandLeft        { get; } = new GUIContent("Grip Snap Left Hand",           $"The transform that will be aligned to the left {nameof(UxrGrabber)} transform if AlignToHandGrabAxes and/or PlaceInHandGrabPivot are active.");
        private GUIContent ContentGripAlignTransformHandRight       { get; } = new GUIContent("Grip Snap Right Hand",          $"The transform that will be aligned to the right {nameof(UxrGrabber)} transform if AlignToHandGrabAxes and/or PlaceInHandGrabPivot are active.");
        private GUIContent ContentCreateGripAlignTransformHandLeft  { get; } = new GUIContent("Create Left Snap",              "Create a dummy as left snap transform.");
        private GUIContent ContentCreateGripAlignTransformHandRight { get; } = new GUIContent("Create Right Snap",             "Create a dummy as right snap transform.");
        private GUIContent ContentSnapMode                          { get; } = new GUIContent("Snap Mode",                     $"How this object's grab-alignment-transform axes will snap to the {nameof(UxrGrabber)} transform after being grabbed.");
        private GUIContent ContentSnapDirection                     { get; } = new GUIContent("Snap Direction",                "Whether this object will be snapped to the hand or the hand will snap to the object.");
        private GUIContent ContentSnapReference                     { get; } = new GUIContent("Grip Snap Transform",           $"Whether the grabbed object will be aligned with the {nameof(UxrGrabber)} transform while being grabbed or another snap transform will be used.");
        private GUIContent ContentAlignToController                 { get; } = new GUIContent("Align To Controller",           "Aligns the object to the controller when it is being grabbed. This is very important for objects like weapons where aiming correctly is key.");
        private GUIContent ContentAlignToControllerAxes             { get; } = new GUIContent("Align To Controller Axes",      "By default, if no transform specified, it will use the objects axes as reference to align (z forward, etc.). Otherwise it can use another transform as reference.");
        private GUIContent ContentGrabProximityMode                 { get; } = new GUIContent("Grabbable Valid Distance",      $"Tells which method to use to detect if a {nameof(UxrGrabber)} can grab this object.");
        private GUIContent ContentGrabProximityBox                  { get; } = new GUIContent("Grabbable Valid Box",           $"Volume the {nameof(UxrGrabber)} needs to be in in order to grab this object.");
        private GUIContent ContentMaxDistanceGrab                   { get; } = new GUIContent("Max Distance Grab",             $"The maximum distance the {nameof(UxrGrabber)} can be to be able to grab it. This is called the proximity.");
        private GUIContent ContentGrabbableDistanceReference        { get; } = new GUIContent("Grabbable Distance Reference",  $"The reference from the grabbable object that will be used to know if the {nameof(UxrGrabber)} is close enough to grab it.");
        private GUIContent ContentGrabProximityTransform            { get; } = new GUIContent("Grabbable Proximity Transform", $"Position the {nameof(UxrGrabber)} needs to be close to in order to grab this object.");
        private GUIContent ContentGrabberProximityUseDefault        { get; } = new GUIContent("Use Grabber Default Proximity", "Uses the grabber's own transform for proximity computation (distance from hand to object). Optionally you can specify different transforms in the grabber component for more precise interactions, for example one for precise distance to a dummy in the palm of the hand and other for precise distance to the a dummy near the index finger.");
        private GUIContent ContentGrabberProximityIndex             { get; } = new GUIContent("Grabber Proximity Index",       $"Allows to specify a different transform for proximity computation (distance from hand to object). This index tells which transform from the {nameof(UxrGrabber)} component's proximity list is used.");
        private GUIContent ContentEnableOnHandNear                  { get; } = new GUIContent("Enable When Hand Near",         "Optional GameObject that will be enabled if a hand is close enough to grab this object.");

        // Property names

        private const string PropertyFoldout                 = "_editorFoldout";
        private const string PropertyEditorName              = "_editorName";
        private const string PropertyGrabMode                = "_grabMode";
        private const string PropertyUseDefaultGrabButtons   = "_useDefaultGrabButtons";
        private const string PropertyInputButtons            = "_inputButtons";
        private const string PropertyBothHandsCompatible     = "_bothHandsCompatible";
        private const string PropertyHandSide                = "_handSide";
        private const string PropertyHideHandGrabberRenderer = "_hideHandGrabberRenderer";
        private const string PropertyDefaultGripPoseInfo     = "_defaultGripPoseInfo";
        private const string PropertyAvatarGripPoseEntries   = "_avatarGripPoseEntries";

        private const string PropertyGripPoseAvatarGuid          = "_avatarPrefabGuid";
        private const string PropertyGripHandPose                = "_handPose";
        private const string PropertyGripPoseBlendValue          = "_poseBlendValue";
        private const string PropertyGripAlignTransformHandLeft  = "_gripAlignTransformHandLeft";
        private const string PropertyGripAlignTransformHandRight = "_gripAlignTransformHandRight";

        private const string PropertySnapMode                      = "_snapMode";
        private const string PropertySnapDirection                 = "_snapDirection";
        private const string PropertySnapReference                 = "_snapReference";
        private const string PropertyAlignToController             = "_alignToController";
        private const string PropertyAlignToControllerAxes         = "_alignToControllerAxes";
        private const string PropertyGrabProximityMode             = "_grabProximityMode";
        private const string PropertyGrabProximityBox              = "_grabProximityBox";
        private const string PropertyMaxDistanceGrab               = "_maxDistanceGrab";
        private const string PropertyGrabProximityTransformUseSelf = "_grabProximityTransformUseSelf";
        private const string PropertyGrabProximityTransform        = "_grabProximityTransform";
        private const string PropertyGrabberProximityUseDefault    = "_grabberProximityUseDefault";
        private const string PropertyGrabberProximityIndex         = "_grabberProximityIndex";
        private const string PropertyEnableOnHandNear              = "_enableOnHandNear";

        private const string PropertyPreviewPosesRegenerateType  = "_previewPosesRegenerationType";
        private const string PropertyPreviewPosesRegenerateIndex = "_previewPosesRegenerationIndex";

        // Other constants

        private const string GrabsRootName = "Grabs";

        #endregion
    }
}