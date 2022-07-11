// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRuntimeHandDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Runtime, lightweight version of <see cref="UxrHandDescriptor" />. It is used to describe the local orientations of
    ///     finger bones of a <see cref="UxrHandPoseAsset" /> for a given <see cref="UxrAvatar" />.
    ///     <see cref="UxrHandPoseAsset" /> objects contain orientations in a well-known space. They are used to adapt hand
    ///     poses independently of the coordinate system used by each avatar. This means an additional transformation needs to
    ///     be performed to get to each avatar's coordinate system. <see cref="UxrRuntimeHandDescriptor" /> is used
    ///     to have a high performant version that already contains the bone orientations in each avatar's coordinate system
    ///     so that hand pose blending can be computed using much less processing power.
    /// </summary>
    public class UxrRuntimeHandDescriptor
    {
        #region Public Types & Data

        public UxrRuntimeFingerDescriptor Index  { get; }
        public UxrRuntimeFingerDescriptor Middle { get; }
        public UxrRuntimeFingerDescriptor Ring   { get; }
        public UxrRuntimeFingerDescriptor Little { get; }
        public UxrRuntimeFingerDescriptor Thumb  { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrRuntimeHandDescriptor()
        {
            Index  = new UxrRuntimeFingerDescriptor();
            Middle = new UxrRuntimeFingerDescriptor();
            Ring   = new UxrRuntimeFingerDescriptor();
            Little = new UxrRuntimeFingerDescriptor();
            Thumb  = new UxrRuntimeFingerDescriptor();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar to compute the runtime hand descriptor for</param>
        /// <param name="handSide">Which hand to store</param>
        /// <param name="handPoseAsset">Hand pose to transform</param>
        /// <param name="handPoseType">Which hand pose information to store</param>
        /// <param name="blendPoseType">
        ///     If <paramref name="handPoseType" /> is <see cref="UxrHandPoseType.Blend" />, which pose to
        ///     store
        /// </param>
        public UxrRuntimeHandDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrHandPoseAsset handPoseAsset, UxrHandPoseType handPoseType, UxrBlendPoseType blendPoseType)
        {
            UxrHandDescriptor handDescriptor = handPoseAsset.GetHandDescriptor(handSide, handPoseType, blendPoseType);

            Index  = new UxrRuntimeFingerDescriptor(avatar, handSide, handDescriptor, UxrFingerType.Index);
            Middle = new UxrRuntimeFingerDescriptor(avatar, handSide, handDescriptor, UxrFingerType.Middle);
            Ring   = new UxrRuntimeFingerDescriptor(avatar, handSide, handDescriptor, UxrFingerType.Ring);
            Little = new UxrRuntimeFingerDescriptor(avatar, handSide, handDescriptor, UxrFingerType.Little);
            Thumb  = new UxrRuntimeFingerDescriptor(avatar, handSide, handDescriptor, UxrFingerType.Thumb);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Copies the data from another descriptor.
        /// </summary>
        /// <param name="handDescriptor">Descriptor to compute the data from</param>
        public void CopyFrom(UxrRuntimeHandDescriptor handDescriptor)
        {
            if (handDescriptor == null)
            {
                return;
            }

            Index.CopyFrom(handDescriptor.Index);
            Middle.CopyFrom(handDescriptor.Middle);
            Ring.CopyFrom(handDescriptor.Ring);
            Little.CopyFrom(handDescriptor.Little);
            Thumb.CopyFrom(handDescriptor.Thumb);
        }

        /// <summary>
        ///     Interpolates towards another runtime hand descriptor.
        /// </summary>
        /// <param name="handDescriptor">Runtime hand descriptor to interpolate towards</param>
        /// <param name="blend">Interpolation value [0.0, 1.0]</param>
        public void InterpolateTo(UxrRuntimeHandDescriptor handDescriptor, float blend)
        {
            if (handDescriptor == null)
            {
                return;
            }

            Index.InterpolateTo(handDescriptor.Index, blend);
            Middle.InterpolateTo(handDescriptor.Middle, blend);
            Ring.InterpolateTo(handDescriptor.Ring, blend);
            Little.InterpolateTo(handDescriptor.Little, blend);
            Thumb.InterpolateTo(handDescriptor.Thumb, blend);
        }

        #endregion
    }
}