// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrBlendPoseType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     Enumerates the different poses in a blend pose.
    /// </summary>
    public enum UxrBlendPoseType
    {
        /// <summary>
        ///     Not a blend pose.
        /// </summary>
        None,

        /// <summary>
        ///     Pose with the open hand.
        /// </summary>
        OpenGrip,

        /// <summary>
        ///     Pose with the closed hand.
        /// </summary>
        ClosedGrip
    }
}