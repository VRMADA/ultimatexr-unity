// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTransformations.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core
{
    /// <summary>
    ///     Enumerates the different transformations that can be applied.
    /// </summary>
    [Flags]
    public enum UxrTransformations
    {
        None      = 0,
        Translate = 1 << 0,
        Rotate    = 1 << 1,
        Scale     = 1 << 2,
        All       = Translate | Rotate | Scale
    }
}