// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSync;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Describes an event raised by an <see cref="UxrAvatar" /> that can also be played back. This facilitates the
    ///     manipulation synchronization through network.
    /// </summary>
    public class UxrAvatarSyncEventArgs : UxrStateSyncEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the event type.
        /// </summary>
        public UxrAvatarSyncEventType EventType { get; }

        /// <summary>
        ///     Gets the event parameters for an <see cref="UxrAvatarSyncEventType.AvatarMove" /> event.
        /// </summary>
        public UxrAvatarMoveEventArgs AvatarMoveEventArgs { get; }

        /// <summary>
        ///     Gets the event parameters for an <see cref="UxrAvatarSyncEventType.HandPose" /> event.
        /// </summary>
        public UxrAvatarHandPoseChangeEventArgs HandPoseChangeEventArgs { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor for an <see cref="UxrAvatarSyncEventType.AvatarMove" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        public UxrAvatarSyncEventArgs(UxrAvatarMoveEventArgs e)
        {
            EventType           = UxrAvatarSyncEventType.AvatarMove;
            AvatarMoveEventArgs = e;
        }

        /// <summary>
        ///     Constructor for an <see cref="UxrAvatarSyncEventType.HandPose" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        public UxrAvatarSyncEventArgs(UxrAvatarHandPoseChangeEventArgs e)
        {
            EventType               = UxrAvatarSyncEventType.HandPose;
            HandPoseChangeEventArgs = e;
        }

        #endregion
    }
}