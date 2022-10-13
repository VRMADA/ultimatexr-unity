// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRotationConstraintMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the ways a <see cref="UxrGrabbableObject" /> rotation can be constrained when being manipulated.
    /// </summary>
    public enum UxrRotationConstraintMode
    {
        /// <summary>
        ///     No constraints.
        /// </summary>
        Free,

        /// <summary>
        ///     Local rotation constraint.
        /// </summary>
        RestrictLocalRotation,

        /// <summary>
        ///     The <see cref="UxrGrabbableObject"/> cannot rotate.
        /// </summary>
        Locked
    }
}