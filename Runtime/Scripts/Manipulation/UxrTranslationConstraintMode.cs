// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTranslationConstraintMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the ways a <see cref="UxrGrabbableObject" /> position can be constrained when being manipulated.
    /// </summary>
    public enum UxrTranslationConstraintMode
    {
        /// <summary>
        ///     No constraints.
        /// </summary>
        Free,

        /// <summary>
        ///     The <see cref="UxrGrabbableObject" /> position is constrained to a box defined by a <see cref="BoxCollider" />.
        /// </summary>
        RestrictToBox,

        /// <summary>
        ///     The local position is constrained between minimum and maximum offsets pointed by the initial local axes.
        /// </summary>
        RestrictLocalOffset,

        /// <summary>
        ///     The <see cref="UxrGrabbableObject" /> position is constrained to a sphere defined by a
        ///     <see cref="SphereCollider" />.
        /// </summary>
        RestrictToSphere,

        /// <summary>
        ///     The <see cref="UxrGrabbableObject"/> cannot move. 
        /// </summary>
        Locked
    }
}