// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWalkDirection.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Enumerates the different options available to decide which direction the avatar will move when using locomotion
    ///     components such as <see cref="UxrSmoothLocomotion" />.
    /// </summary>
    public enum UxrWalkDirection
    {
        /// <summary>
        ///     User will move in the direction pointed by the controller.
        /// </summary>
        ControllerForward,

        /// <summary>
        ///     User will move in the direction currently pointed by the avatar's root transform forward vector.
        /// </summary>
        AvatarForward,

        /// <summary>
        ///     User will move in the direction currently being looking at.
        /// </summary>
        LookDirection
    }
}