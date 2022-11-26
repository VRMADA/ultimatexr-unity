// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Avatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Shows a dropdown hand pose list for an avatar using <see cref="EditorGUILayout" />.
        /// </summary>
        /// <param name="guiContent">GUI content</param>
        /// <param name="avatar">Avatar with the poses that will be listed</param>
        /// <param name="handPosesProperty">
        ///     The serialized property with the hand pose. Should be SerializedProperty for a UxrHandPoseAsset field.
        /// </param>
        /// <param name="selectedHandPose">The selected hand pose</param>
        /// <returns>Whether the current selection changed</returns>
        public static bool HandPoseDropdown(GUIContent guiContent, UxrAvatar avatar, SerializedProperty handPosesProperty, out UxrHandPoseAsset selectedHandPose)
        {
            bool hasChanged = false;
            GetPoseDropdownInfo(avatar, handPosesProperty, out List<UxrHandPoseAsset> handPoses, out selectedHandPose, out int selectedPoseIndex);

            EditorGUI.BeginChangeCheck();
            int popup = EditorGUILayout.Popup(guiContent.text, selectedPoseIndex, handPoses.Select(p => p.name).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                hasChanged                             = true;
                selectedHandPose                       = handPoses[popup];
                handPosesProperty.objectReferenceValue = selectedHandPose;
            }

            return hasChanged;
        }

        /// <summary>
        ///     Shows a dropdown hand pose list for an avatar using <see cref="EditorGUI" />.
        /// </summary>
        /// <param name="rect">GUI rect</param>
        /// <param name="guiContent">GUI content</param>
        /// <param name="avatar">Avatar with the poses that will be listed</param>
        /// <param name="handPosesProperty">
        ///     The serialized property with the hand pose. Should be SerializedProperty for a UxrHandPoseAsset field.
        /// </param>
        /// <param name="selectedHandPose">The selected hand pose</param>
        /// <returns>Whether the current selection changed</returns>
        public static bool HandPoseDropdown(Rect rect, GUIContent guiContent, UxrAvatar avatar, SerializedProperty handPosesProperty, out UxrHandPoseAsset selectedHandPose)
        {
            bool hasChanged = false;
            GetPoseDropdownInfo(avatar, handPosesProperty, out List<UxrHandPoseAsset> handPoses, out selectedHandPose, out int selectedPoseIndex);

            EditorGUI.BeginChangeCheck();
            int popup = EditorGUI.Popup(rect, guiContent.text, selectedPoseIndex, handPoses.Select(p => p.name).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                hasChanged                             = true;
                selectedHandPose                       = handPoses[popup];
                handPosesProperty.objectReferenceValue = selectedHandPose;
            }

            return hasChanged;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets information used by the HandPoseDropdown methods.
        /// </summary>
        /// <param name="avatar">Avatar to get the poses of</param>
        /// <param name="handPosesProperty">SerializedProperty of the field that stores the <see cref="UxrHandPoseAsset" /></param>
        /// <param name="handPoses">Returns the avatar hand poses</param>
        /// <param name="selectedHandPose">Returns the selected avatar hand pose</param>
        /// <param name="selectedPoseIndex">
        ///     Returns the list position of <paramref name="selectedHandPose" /> in
        ///     <paramref name="handPoses" />
        /// </param>
        private static void GetPoseDropdownInfo(UxrAvatar avatar, SerializedProperty handPosesProperty, out List<UxrHandPoseAsset> handPoses, out UxrHandPoseAsset selectedHandPose, out int selectedPoseIndex)
        {
            selectedHandPose  = null;
            handPoses         = avatar != null ? avatar.GetAllHandPoses().ToList() : new List<UxrHandPoseAsset>();
            selectedPoseIndex = -1;

            if (handPoses.Count > 0 && handPosesProperty.objectReferenceValue != null)
            {
                UxrHandPoseAsset handPoseAsset = handPosesProperty.objectReferenceValue as UxrHandPoseAsset;

                // Instead of using the pose directly we make sure it comes from the avatar list, in case a pose is overriden

                if (handPoseAsset)
                {
                    selectedHandPose = handPoses.FirstOrDefault(p => p.name == handPoseAsset.name);

                    if (selectedHandPose != null)
                    {
                        selectedPoseIndex = handPoses.IndexOf(selectedHandPose);
                    }
                }
            }
        }

        #endregion
    }
}