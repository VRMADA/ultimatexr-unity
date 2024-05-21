// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Base class for avatar events.
    /// </summary>
    public class UxrAvatarEventArgs
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
        public UxrAvatarEventArgs(UxrAvatar avatar)
        {
            Avatar = avatar;
        }

        #endregion
    }
}