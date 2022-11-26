// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPreviewPoseRegeneration.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor.Manipulation
{
    /// <summary>
    ///     Enumerates the possible preview pose regenerations that could be required when grabbable object properties are
    ///     changed.
    /// </summary>
    public enum UxrPreviewPoseRegeneration
    {
        /// <summary>
        ///     No regeneration required.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Re-create the preview pose.
        /// </summary>
        Complete = 1,

        /// <summary>
        ///     Using the current mesh, interpolate the pose using the current blend parameter.
        /// </summary>
        OnlyBlend = 2
    }
}