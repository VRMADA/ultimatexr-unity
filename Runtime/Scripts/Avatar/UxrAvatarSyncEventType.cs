// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarSyncEventType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Enumerates the different events that can be synced for networking using <see cref="UxrAvatarSyncEventArgs" />
    ///     .
    /// </summary>
    public enum UxrAvatarSyncEventType
    {
        AvatarMove,
        HandPose
    }
}