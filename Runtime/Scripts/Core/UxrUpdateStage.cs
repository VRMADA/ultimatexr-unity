// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUpdateStage.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Core
{
    /// <summary>
    ///     Enumerates the different update stages during a frame used by <see cref="UxrManager" /> events.
    /// </summary>
    public enum UxrUpdateStage
    {
        /// <summary>
        ///     Stage where avatars update their internal state, input, tracking and locomotion (root avatar
        ///     <see cref="Transform" />).
        /// </summary>
        Update,

        /// <summary>
        ///     Stage where avatars update bones that are tracked using tracking devices.
        /// </summary>
        AvatarUsingTracking,

        /// <summary>
        ///     Stage where the <see cref="UxrGrabManager" /> updates grabbable objects and avatar hand position/orientation
        ///     constraints as a result of manipulation.
        /// </summary>
        Manipulation,

        /// <summary>
        ///     Stage where avatars update the different <see cref="Transform" /> components for hand animation and poses.
        /// </summary>
        Animation,
        
        /// <summary>
        ///     Post-processing stage where post-processing such as Inverse Kinematics are applied.
        /// </summary>
        PostProcess
    }
}