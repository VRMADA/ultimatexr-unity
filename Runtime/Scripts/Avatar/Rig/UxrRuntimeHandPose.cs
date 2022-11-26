// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRuntimeHandPose.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Runtime, lightweight version of <see cref="UxrHandPoseAsset" />. It is used to describe the local orientations of
    ///     finger bones of a <see cref="UxrHandPoseAsset" /> for a given <see cref="UxrAvatar" />.
    ///     <see cref="UxrHandPoseAsset" /> objects contain orientations in a well-known space. They are used to adapt hand
    ///     poses independently of the coordinate system used by each avatar. This means an additional transformation needs to
    ///     be performed to get to each avatar's coordinate system. <see cref="UxrRuntimeHandPose" /> is used
    ///     to have a high performant version that already contains the bone orientations in each avatar's coordinate system
    ///     so that hand pose blending can be computed using much less processing power.
    /// </summary>
    public class UxrRuntimeHandPose
    {
        #region Public Types & Data

        public string          PoseName { get; }
        public UxrHandPoseType PoseType { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar to compute the runtime hand pose for</param>
        /// <param name="handPoseAsset">Hand pose in a well-known coordinate system</param>
        public UxrRuntimeHandPose(UxrAvatar avatar, UxrHandPoseAsset handPoseAsset)
        {
            PoseName                  = handPoseAsset.name;
            PoseType                  = handPoseAsset.PoseType;
            HandDescriptorLeft        = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Left,  handPoseAsset, UxrHandPoseType.Fixed, UxrBlendPoseType.None);
            HandDescriptorRight       = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Right, handPoseAsset, UxrHandPoseType.Fixed, UxrBlendPoseType.None);
            HandDescriptorOpenLeft    = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Left,  handPoseAsset, UxrHandPoseType.Blend, UxrBlendPoseType.OpenGrip);
            HandDescriptorOpenRight   = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Right, handPoseAsset, UxrHandPoseType.Blend, UxrBlendPoseType.OpenGrip);
            HandDescriptorClosedLeft  = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Left,  handPoseAsset, UxrHandPoseType.Blend, UxrBlendPoseType.ClosedGrip);
            HandDescriptorClosedRight = new UxrRuntimeHandDescriptor(avatar, UxrHandSide.Right, handPoseAsset, UxrHandPoseType.Blend, UxrBlendPoseType.ClosedGrip);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the given hand descriptor, based on the <see cref="PoseType" />.
        /// </summary>
        /// <param name="handSide">Hand to get the descriptor for</param>
        /// <param name="blendPoseType">
        ///     If <see cref="PoseType" /> is <see cref="UxrHandPoseType.Blend" />, whether to get the open or
        ///     closed pose descriptor.
        /// </param>
        /// <returns>Hand descriptor</returns>
        public UxrRuntimeHandDescriptor GetHandDescriptor(UxrHandSide handSide, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            return PoseType switch
                   {
                               UxrHandPoseType.Fixed                                                   => handSide == UxrHandSide.Left ? HandDescriptorLeft : HandDescriptorRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.OpenGrip   => handSide == UxrHandSide.Left ? HandDescriptorOpenLeft : HandDescriptorOpenRight,
                               UxrHandPoseType.Blend when blendPoseType == UxrBlendPoseType.ClosedGrip => handSide == UxrHandSide.Left ? HandDescriptorClosedLeft : HandDescriptorClosedRight,
                               _                                                                       => throw new ArgumentOutOfRangeException(nameof(blendPoseType), blendPoseType, null)
                   };
        }

        #endregion

        #region Private Types & Data

        private UxrRuntimeHandDescriptor HandDescriptorLeft        { get; }
        private UxrRuntimeHandDescriptor HandDescriptorRight       { get; }
        private UxrRuntimeHandDescriptor HandDescriptorOpenLeft    { get; }
        private UxrRuntimeHandDescriptor HandDescriptorOpenRight   { get; }
        private UxrRuntimeHandDescriptor HandDescriptorClosedLeft  { get; }
        private UxrRuntimeHandDescriptor HandDescriptorClosedRight { get; }

        #endregion
    }
}