// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCcdIKSolver.IterationResult.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.IK
{
    public partial class UxrCcdIKSolver
    {
        #region Private Types & Data

        /// <summary>
        ///     Result of the CCD algorithm iteration
        /// </summary>
        private enum IterationResult
        {
            /// <summary>
            ///     The effector has reached the goal.
            /// </summary>
            GoalReached,

            /// <summary>
            ///     The effector is still trying to reach the goal.
            /// </summary>
            ReachingGoal,

            /// <summary>
            ///     There was an error and no links were rotated in order to reach the goal.
            /// </summary>
            Error,
        }

        #endregion
    }
}