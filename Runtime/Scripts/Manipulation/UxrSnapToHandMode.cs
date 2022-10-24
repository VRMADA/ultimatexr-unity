// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSnapToHandMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the ways a <see cref="UxrGrabbableObject" /> can snap to a <see cref="UxrGrabber" />.
    /// </summary>
    public enum UxrSnapToHandMode
    {
        /// <summary>
        ///     Don't snap. The <see cref="UxrGrabbableObject" /> is simply linked to the <see cref="UxrGrabber" /> and from then
        ///     on it will move along with it.
        /// </summary>
        DontSnap,

        /// <summary>
        ///     Keep the current <see cref="UxrGrabbableObject" /> orientation and snap the position.
        /// </summary>
        PositionOnly,

        /// <summary>
        ///     Keep the current <see cref="UxrGrabbableObject" /> position and snap the rotation.
        /// </summary>
        RotationOnly,

        /// <summary>
        ///     Snap the <see cref="UxrGrabbableObject" /> position and rotation to the <see cref="UxrGrabber" />.
        /// </summary>
        PositionAndRotation
    }
}