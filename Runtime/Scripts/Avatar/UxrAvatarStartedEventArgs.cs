// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarStartedEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Arguments for the avatar started event.
    /// </summary>
    public class UxrAvatarStartedEventArgs : UxrAvatarEventArgs
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        public UxrAvatarStartedEventArgs(UxrAvatar avatar) : base(avatar)
        {
        }

        #endregion
    }
}