// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLaserPointerTargetTypes.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.UI;

namespace UltimateXR
{
    /// <summary>
    /// Enumerates the different elements that a <see cref="UxrLaserPointer"/> can interact with. 
    /// </summary>
    [Flags]
    public enum UxrLaserPointerTargetTypes
    {
        UI          = 1 << 0,
        Colliders2D = 1 << 1,
        Colliders3D = 1 << 2
    }
}