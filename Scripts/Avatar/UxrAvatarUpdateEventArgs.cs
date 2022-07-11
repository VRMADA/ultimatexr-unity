// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarUpdateEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Contains information about an avatar update event.
    /// </summary>
    public class UxrAvatarUpdateEventArgs : UxrAvatarEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the update stage the update event belongs to.
        /// </summary>
        public UxrUpdateStage UpdateStage { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar the event describes</param>
        /// <param name="updateStage">Update stage the event belongs to</param>
        public UxrAvatarUpdateEventArgs(UxrAvatar avatar, UxrUpdateStage updateStage) : base(avatar)
        {
            UpdateStage = updateStage;
        }

        #endregion
    }
}