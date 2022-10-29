// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableModifierFlags.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Flags that represent parts in an <see cref="UxrGrabbableObject" /> that can be modified/hidden by components in the
    ///     same <see cref="GameObject" /> that implement the <see cref="IUxrGrabbableModifier" /> interface.
    /// </summary>
    [Flags]
    public enum UxrGrabbableModifierFlags
    {
        None                  = 0,
        ParentControl         = 1 << 0,
        Priority              = 1 << 2,
        MultiGrab             = 1 << 3,
        TranslationConstraint = 1 << 8,
        RotationConstraint    = 1 << 12,
        TranslationResistance = 1 << 16,
        RotationResistance    = 1 << 17,
        Anchored              = 1 << 18
    }
}