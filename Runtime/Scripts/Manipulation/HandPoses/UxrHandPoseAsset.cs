// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseAsset.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     ScriptableObject that stores custom hand poses. Data is stored in a well-known axes system so that poses can be
    ///     exchanged between different avatars.
    /// </summary>
    [Serializable]
    public class UxrHandPoseAsset : ScriptableObject
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int               _handPoseAssetVersion;
        [SerializeField] private UxrHandPoseType   _poseType;
        [SerializeField] private UxrHandDescriptor _handDescriptorLeft;
        [SerializeField] private UxrHandDescriptor _handDescriptorRight;
        [SerializeField] private UxrHandDescriptor _handDescriptorOpenLeft;
        [SerializeField] private UxrHandDescriptor _handDescriptorOpenRight;
        [SerializeField] private UxrHandDescriptor _handDescriptorClosedLeft;
        [SerializeField] private UxrHandDescriptor _handDescriptorClosedRight;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Current data version.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        ///     Gets the version the pose was stored in.
        /// </summary>
        public int Version
        {
            get => _handPoseAssetVersion;
            set => _handPoseAssetVersion = value;
        }

        /// <summary>
        ///     Gets the pose type.
        /// </summary>
        public UxrHandPoseType PoseType
        {
            get => _poseType;
            set => _poseType = value;
        }

        /// <summary>
        ///     Gets the left fixed pose hand descriptor.
        /// </summary>
        public UxrHandDescriptor HandDescriptorLeft
        {
            get => _handDescriptorLeft;
            set => _handDescriptorLeft = value;
        }

        /// <summary>
        ///     Gets the right fixed pose hand descriptor.
        /// </summary>
        public UxrHandDescriptor HandDescriptorRight
        {
            get => _handDescriptorRight;
            set => _handDescriptorRight = value;
        }

        /// <summary>
        ///     Gets the left blend pose hand descriptor for the open state.
        /// </summary>
        public UxrHandDescriptor HandDescriptorOpenLeft
        {
            get => _handDescriptorOpenLeft;
            set => _handDescriptorOpenLeft = value;
        }

        /// <summary>
        ///     Gets the right blend pose hand descriptor for the open state.
        /// </summary>
        public UxrHandDescriptor HandDescriptorOpenRight
        {
            get => _handDescriptorOpenRight;
            set => _handDescriptorOpenRight = value;
        }

        /// <summary>
        ///     Gets the left blend pose hand descriptor for the closed state.
        /// </summary>
        public UxrHandDescriptor HandDescriptorClosedLeft
        {
            get => _handDescriptorClosedLeft;
            set => _handDescriptorClosedLeft = value;
        }

        /// <summary>
        ///     Gets the right blend pose hand descriptor for the closed state.
        /// </summary>
        public UxrHandDescriptor HandDescriptorClosedRight
        {
            get => _handDescriptorClosedRight;
            set => _handDescriptorClosedRight = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the hand descriptor for the given hand, based on the <see cref="PoseType" />.
        /// </summary>
        /// <param name="handSide">Hand to get the descriptor for</param>
        /// <param name="blendPoseType">
        ///     If <see cref="PoseType" /> is <see cref="UxrHandPoseType.Blend" />, whether to get the open or
        ///     closed pose descriptor.
        /// </param>
        /// <returns>Hand descriptor</returns>
        public UxrHandDescriptor GetHandDescriptor(UxrHandSide handSide, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            return PoseType switch
                   {
                               UxrHandPoseType.Fixed                                                   => handSide == UxrHandSide.Left ? _handDescriptorLeft : _handDescriptorRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.OpenGrip   => handSide == UxrHandSide.Left ? _handDescriptorOpenLeft : _handDescriptorOpenRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.ClosedGrip => handSide == UxrHandSide.Left ? _handDescriptorClosedLeft : _handDescriptorClosedRight,
                               _                                                                       => null
                   };
        }

        /// <summary>
        ///     Gets the hand descriptor for the given hand, based on an external <see cref="UxrHandPoseType" /> parameter.
        /// </summary>
        /// <param name="handSide">Hand to get the descriptor for</param>
        /// <param name="poseType">The pose type to get the descriptor for</param>
        /// <param name="blendPoseType">
        ///     If <see cref="PoseType" /> is <see cref="UxrHandPoseType.Blend" />, whether to get the open or
        ///     closed pose descriptor.
        /// </param>
        /// <returns>Hand descriptor</returns>
        public UxrHandDescriptor GetHandDescriptor(UxrHandSide handSide, UxrHandPoseType poseType, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            return poseType switch
                   {
                               UxrHandPoseType.Fixed                                                   => handSide == UxrHandSide.Left ? _handDescriptorLeft : _handDescriptorRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.OpenGrip   => handSide == UxrHandSide.Left ? _handDescriptorOpenLeft : _handDescriptorOpenRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.ClosedGrip => handSide == UxrHandSide.Left ? _handDescriptorClosedLeft : _handDescriptorClosedRight,
                               _                                                                       => null
                   };
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Outputs transform debug data to the editor window.
        /// </summary>
        /// <param name="handSide">Hand to output the data for</param>
        /// <param name="blendPoseType">The blend pose type or <see cref="UxrBlendPoseType.None" /> if it is a fixed pose</param>
        public void DrawEditorDebugLabels(UxrHandSide handSide, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            UxrHandDescriptor handDescriptor = handSide == UxrHandSide.Left ? HandDescriptorLeft : HandDescriptorRight;

            if (blendPoseType == UxrBlendPoseType.OpenGrip)
            {
                handDescriptor = handSide == UxrHandSide.Left ? HandDescriptorOpenLeft : HandDescriptorOpenRight;
            }
            else if (blendPoseType == UxrBlendPoseType.ClosedGrip)
            {
                handDescriptor = handSide == UxrHandSide.Left ? HandDescriptorClosedLeft : HandDescriptorClosedRight;
            }

            handDescriptor?.DrawEditorDebugLabels();
        }

#endif

        #endregion
    }
}