// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Avatar operating modes.
    /// </summary>
    public enum UxrAvatarMode
    {
        /// <summary>
        ///     Avatar is updated automatically using input/tracking components. This is the avatar that is controlled by the user.
        /// </summary>
        Local,

        /// <summary>
        ///     "Puppet" mode where avatar is not updated internally and transforms are required to be updated externally instead.
        ///     These are remote avatars controlled by other users in a networking scenario, avatars in replay mode, etc...
        /// </summary>
        UpdateExternally
    }
}