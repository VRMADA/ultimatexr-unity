// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPreviewHandBoneInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar.Rig;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    /// <summary>
    ///     Stores bone information for preview meshes.
    /// </summary>
    public class UxrPreviewHandBoneInfo
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the bind pose.
        /// </summary>
        public Matrix4x4 BindPose { get; private set; }

        /// <summary>
        ///     Gets the matrix representing the bone transform relative to the hand bone.
        /// </summary>
        public Matrix4x4 TransformRelativeToHand { get; private set; }

        /// <summary>
        ///     Gets the matrix representing the bone transform relative to the parent, in hand bone space.
        /// </summary>
        public Matrix4x4 TransformRelativeToParent { get; private set; }

        /// <summary>
        ///     Gets the index of the parent bone.
        /// </summary>
        public int ParentBoneIndex { get; private set; }

        /// <summary>
        ///     Gets the currently computed transform in hand space (relative to the hand bone).
        /// </summary>
        public Matrix4x4 CurrentRelativeTransform { get; set; } = Matrix4x4.identity;

        /// <summary>
        ///     Gets the currently computed transform in grabber space (relative to where the <see cref="UxrGrabber" /> component
        ///     is located in the hand).
        /// </summary>
        public Matrix4x4 CurrentTransform { get; set; } = Matrix4x4.identity;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates the hand bone data.
        /// </summary>
        /// <param name="skinnedMeshRenderer">Skinned mesh renderer with the mesh data that contains the hand</param>
        /// <param name="bindPoses">Bind poses</param>
        /// <param name="handBindPose">Hand bind pose</param>
        /// <param name="handDescriptor">Hand descriptor</param>
        /// <param name="hand">Hand rig</param>
        /// <returns>List of bone information</returns>
        public static List<UxrPreviewHandBoneInfo> CreateHandBoneData(SkinnedMeshRenderer skinnedMeshRenderer,
                                                                      Matrix4x4[]         bindPoses,
                                                                      Matrix4x4           handBindPose,
                                                                      UxrHandDescriptor   handDescriptor,
                                                                      UxrAvatarHand       hand)
        {
            List<UxrPreviewHandBoneInfo> boneList = new List<UxrPreviewHandBoneInfo>();

            for (int i = 0; i < skinnedMeshRenderer.bones.Length; ++i)
            {
                boneList.Add(new UxrPreviewHandBoneInfo());
            }

            FillHandBoneData(skinnedMeshRenderer, bindPoses, handBindPose, handDescriptor, hand, boneList);
            ResolveBoneTransformsRelativeToParent(boneList);

            return boneList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Fills hand bone information.
        /// </summary>
        /// <param name="skinnedMeshRenderer">The skin that has the hand mesh</param>
        /// <param name="bindPoses">The bind poses</param>
        /// <param name="handBindPose">The hand bind pose</param>
        /// <param name="handDescriptor">The hand descriptor</param>
        /// <param name="hand">The hand rig</param>
        /// <param name="boneList">The bone information to fill</param>
        private static void FillHandBoneData(SkinnedMeshRenderer skinnedMeshRenderer, Matrix4x4[] bindPoses, Matrix4x4 handBindPose, UxrHandDescriptor handDescriptor, UxrAvatarHand hand, List<UxrPreviewHandBoneInfo> boneList)
        {
            FillFingerBoneData(skinnedMeshRenderer, bindPoses, handBindPose, hand.Wrist, handDescriptor.Index,  hand.Index,  boneList);
            FillFingerBoneData(skinnedMeshRenderer, bindPoses, handBindPose, hand.Wrist, handDescriptor.Middle, hand.Middle, boneList);
            FillFingerBoneData(skinnedMeshRenderer, bindPoses, handBindPose, hand.Wrist, handDescriptor.Ring,   hand.Ring,   boneList);
            FillFingerBoneData(skinnedMeshRenderer, bindPoses, handBindPose, hand.Wrist, handDescriptor.Little, hand.Little, boneList);
            FillFingerBoneData(skinnedMeshRenderer, bindPoses, handBindPose, hand.Wrist, handDescriptor.Thumb,  hand.Thumb,  boneList);

            for (int i = 0; i < boneList.Count; ++i)
            {
                if (boneList[i].Initialized == false)
                {
                    boneList[i].Initialized             = true;
                    boneList[i].BindPose                = bindPoses[i];
                    boneList[i].TransformRelativeToHand = hand.Wrist.worldToLocalMatrix * skinnedMeshRenderer.bones[i].localToWorldMatrix;
                    boneList[i].ParentBoneIndex         = -1;
                }
            }
        }

        /// <summary>
        ///     Fills finger bone information
        /// </summary>
        /// <param name="skinnedMeshRenderer">The skin that has the hand mesh</param>
        /// <param name="bindPoses">The bind poses</param>
        /// <param name="handBindPose">The hand bind pose</param>
        /// <param name="handTransform">The hand's transform</param>
        /// <param name="fingerDescriptor">The finger descriptor</param>
        /// <param name="finger">The finger rig</param>
        /// <param name="boneList">The bone information to fill</param>
        private static void FillFingerBoneData(SkinnedMeshRenderer          skinnedMeshRenderer,
                                               Matrix4x4[]                  bindPoses,
                                               Matrix4x4                    handBindPose,
                                               Transform                    handTransform,
                                               UxrFingerDescriptor          fingerDescriptor,
                                               UxrAvatarFinger              finger,
                                               List<UxrPreviewHandBoneInfo> boneList)
        {
            TryResolveBoneInfo(skinnedMeshRenderer, bindPoses, finger.Metacarpal,   fingerDescriptor.Metacarpal,   boneList);
            TryResolveBoneInfo(skinnedMeshRenderer, bindPoses, finger.Proximal,     fingerDescriptor.Proximal,     boneList);
            TryResolveBoneInfo(skinnedMeshRenderer, bindPoses, finger.Intermediate, fingerDescriptor.Intermediate, boneList);
            TryResolveBoneInfo(skinnedMeshRenderer, bindPoses, finger.Distal,       fingerDescriptor.Distal,       boneList);
        }

        /// <summary>
        ///     Tries to resolve bone information.
        /// </summary>
        /// <param name="skinnedMeshRenderer">The skin that has the hand mesh</param>
        /// <param name="bindPoses">The bind poses</param>
        /// <param name="boneTransform">The bone's transform</param>
        /// <param name="nodeDescriptor">The finger node descriptor</param>
        /// <param name="boneList">The bone information to fill</param>
        private static void TryResolveBoneInfo(SkinnedMeshRenderer          skinnedMeshRenderer,
                                               Matrix4x4[]                  bindPoses,
                                               Transform                    boneTransform,
                                               UxrFingerNodeDescriptor      nodeDescriptor,
                                               List<UxrPreviewHandBoneInfo> boneList)
        {
            if (boneTransform != null)
            {
                for (int i = 0; i < skinnedMeshRenderer.bones.Length; ++i)
                {
                    if (skinnedMeshRenderer.bones[i] == boneTransform && i < boneList.Count)
                    {
                        boneList[i].Initialized             = true;
                        boneList[i].BindPose                = bindPoses[i];
                        boneList[i].TransformRelativeToHand = nodeDescriptor.TransformRelativeToHand;

                        boneList[i].ParentBoneIndex = -1;

                        for (int j = 0; j < skinnedMeshRenderer.bones.Length; ++j)
                        {
                            if (skinnedMeshRenderer.bones[j] == boneTransform.parent && j < boneList.Count)
                            {
                                boneList[i].ParentBoneIndex = j;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Resolves the parent information in the list of bones.
        /// </summary>
        /// <param name="boneList">The bone information to fill</param>
        private static void ResolveBoneTransformsRelativeToParent(List<UxrPreviewHandBoneInfo> boneList)
        {
            foreach (UxrPreviewHandBoneInfo bone in boneList)
            {
                if (bone.ParentBoneIndex != -1)
                {
                    bone.TransformRelativeToParent = boneList[bone.ParentBoneIndex].TransformRelativeToHand.inverse * bone.TransformRelativeToHand;
                }
                else
                {
                    bone.TransformRelativeToParent = bone.TransformRelativeToHand;
                }
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets whether the object has been initialized.
        /// </summary>
        private bool Initialized { get; set; }

        #endregion
    }
}