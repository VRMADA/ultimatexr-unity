// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRaycastStepsQuality.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     The number of raycasts to perform over a teleport arc to check where it intersects with the scene.
    ///     Higher quality steps use more raycasts.
    /// </summary>
    public enum UxrRaycastStepsQuality
    {
        LowQuality,
        MediumQuality,
        HighQuality,
        VeryHighQuality
    }
}