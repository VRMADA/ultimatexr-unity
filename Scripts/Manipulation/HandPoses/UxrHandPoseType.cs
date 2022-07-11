// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     Enumerates the different pose types.
    /// </summary>
    public enum UxrHandPoseType
    {
        /// <summary>
        ///     Not initialized.
        /// </summary>
        None,

        /// <summary>
        ///     Fixed pose (pose with a single state).
        /// </summary>
        Fixed,

        /// <summary>
        ///     Blend pose. A blend pose has two states, open and closed, and allows to blend between them. In the grabbing system
        ///     it allows using a single blend grab pose for multiple objects with different sizes.
        /// </summary>
        Blend
    }
}