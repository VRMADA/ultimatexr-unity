// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrGrabbableModifier.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Interface that can be implemented in components that modify a <see cref="UxrGrabbableObject" /> in the same
    ///     <see cref="GameObject" /> so that the inspector shows which information is being controlled by the modifier.
    /// </summary>
    public interface IUxrGrabbableModifier
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the flags representing the parts of the <see cref="UxrGrabbableObject" /> that are overriden/controlled by the
        ///     modifier.
        /// </summary>
        UxrGrabbableModifierFlags GrabbableModifierFlags { get; }

        #endregion
    }
}