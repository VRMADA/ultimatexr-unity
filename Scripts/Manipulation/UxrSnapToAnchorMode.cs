// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSnapToAnchorMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates how a <see cref="UxrGrabbableObject" /> can snap to a <see cref="UxrGrabbableObjectAnchor" /> when being
    ///     placed on it.
    /// </summary>
    public enum UxrSnapToAnchorMode
    {
        /// <summary>
        ///     Don't snap.
        /// </summary>
        DontSnap,

        /// <summary>
        ///     Snap the <see cref="UxrGrabbableObject" /> position and keep the rotation.
        /// </summary>
        PositionOnly,

        /// <summary>
        ///     Snap the <see cref="UxrGrabbableObject" /> rotation and keep the position.
        /// </summary>
        RotationOnly,

        /// <summary>
        ///     Snap the position and rotation.
        /// </summary>
        PositionAndRotation
    }
}