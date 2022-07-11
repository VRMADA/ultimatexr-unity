// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrReorientationType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     How the avatar can be reoriented when executing a teleportation.
    /// </summary>
    public enum UxrReorientationType
    {
        /// <summary>
        ///     Avatar will teleport and keep the same orientation.
        /// </summary>
        KeepOrientation,

        /// <summary>
        ///     Avatar new orientation will be the (source, destination) vector.
        /// </summary>
        UseTeleportFromToDirection,

        /// <summary>
        ///     User can control the new orientation using the joystick.
        /// </summary>
        AllowUserJoystickRedirect
    }
}