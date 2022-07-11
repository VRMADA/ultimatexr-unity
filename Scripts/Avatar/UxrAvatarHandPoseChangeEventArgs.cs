// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarHandPoseChangeEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Event args for a hand pose change in an <see cref="UxrAvatar" />.
    /// </summary>
    public class UxrAvatarHandPoseChangeEventArgs : UxrAvatarEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets which hand the event belongs to.
        /// </summary>
        public UxrHandSide HandSide { get; }

        /// <summary>
        ///     Gets the name of the pose to change to.
        /// </summary>
        public string PoseName { get; }

        /// <summary>
        ///     Gets the new blend value if the pose type is <see cref="UxrHandPoseType.Blend" />.
        /// </summary>
        public float BlendValue { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">The avatar</param>
        /// <param name="handSide">Which hand the event belongs to</param>
        /// <param name="poseName">Name of the pose to change to</param>
        /// <param name="blendValue">New blend value</param>
        public UxrAvatarHandPoseChangeEventArgs(UxrAvatar   avatar,
                                                UxrHandSide handSide,
                                                string      poseName   = "",
                                                float       blendValue = 0.0f) : base(avatar)
        {
            HandSide   = handSide;
            PoseName   = poseName;
            BlendValue = blendValue;
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Event: Hand={HandSide}, PoseName={PoseName}, BlendValue={BlendValue}";
        }

        #endregion
    }
}