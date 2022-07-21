// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Base class for avatar events.
    /// </summary>
    public abstract class UxrAvatarEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the Avatar the event belongs to.
        /// </summary>
        public UxrAvatar Avatar { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        protected UxrAvatarEventArgs(UxrAvatar avatar)
        {
            Avatar = avatar;
        }

        #endregion
    }
}