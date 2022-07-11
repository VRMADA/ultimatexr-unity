// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRigType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Enumerates the different rig types handled by the <see cref="UxrAvatar" /> inspector to make sure
    ///     that only the relevant rig elements are shown.
    /// </summary>
    public enum UxrAvatarRigType
    {
        /// <summary>
        ///     Simple setup: head and two hands
        /// </summary>
        HandsOnly,

        /// <summary>
        ///     Full body, including torso, neck... etc.
        /// </summary>
        HalfOrFullBody
    }
}