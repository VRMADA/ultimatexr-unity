// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLookAtMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     Enumerates the different modes for a "look at" operation.
    /// </summary>
    public enum UxrLookAtMode
    {
        /// <summary>
        ///     Look at the target.
        /// </summary>
        Target,

        /// <summary>
        ///     Align to a specific target's direction.
        /// </summary>
        MatchTargetDirection,

        /// <summary>
        ///     Use a direction in world-space coordinates.
        /// </summary>
        WorldDirection
    }
}