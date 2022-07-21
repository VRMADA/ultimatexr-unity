// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrArmSolveOptions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Different clavicle options supported by <see cref="UxrArmIKSolver.SolveIKPass" /> when clavicle data is present in
    ///     the rig.
    /// </summary>
    [Flags]
    public enum UxrArmSolveOptions
    {
        /// <summary>
        ///     No options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Reset the clavicle position.
        /// </summary>
        ResetClavicle = 1,

        /// <summary>
        ///     Solve the clavicle position. Can be used together with <see cref="ResetClavicle" /> so that the clavicle is solved
        ///     without using the current position data.
        /// </summary>
        SolveClavicle = 1 << 1
    }
}