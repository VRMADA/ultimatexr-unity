// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportSpawnUsedEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Event parameters when an <see cref="UxrAvatar" /> used a <see cref="UxrTeleportSpawnCollider" />.
    /// </summary>
    public class UxrTeleportSpawnUsedEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the avatar that used the <see cref="UxrTeleportSpawnCollider" />.
        /// </summary>
        public UxrAvatar Avatar { get; }

        /// <summary>
        ///     Gets the move event information.
        /// </summary>
        public UxrAvatarMoveEventArgs AvatarMoveEventArgs { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar that used the spawn collider</param>
        /// <param name="moveEventArgs">Move parameters</param>
        public UxrTeleportSpawnUsedEventArgs(UxrAvatar avatar, UxrAvatarMoveEventArgs moveEventArgs)
        {
            Avatar              = avatar;
            AvatarMoveEventArgs = moveEventArgs;
        }

        #endregion
    }
}