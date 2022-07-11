// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHand.FingerInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.IK;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    public partial class UxrControllerHand
    {
        #region Private Types & Data

        /// <summary>
        ///     Describes a finger used by a hand when interacting with a VR controller. It allows to graphically represent fingers
        ///     that interact with VR controllers by using Inverse Kinematics.
        /// </summary>
        [Serializable]
        private class FingerIK
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private UxrCcdIKSolver _fingerIKSolver;
            [SerializeField] private float          _fingerToGoalDuration = 0.1f;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the finger IK solver.
            /// </summary>
            public UxrCcdIKSolver FingerIKSolver => _fingerIKSolver;

            /// <summary>
            ///     Gets the seconds it will take for a finger to reach the input element whenever it is pressed.
            /// </summary>
            public float FingerToGoalDuration => _fingerToGoalDuration;

            /// <summary>
            ///     Gets or sets the effector initial local position. The effector is the part of the finger in the IK chain that will
            ///     try to reach the goal.
            /// </summary>
            public Vector3 LocalEffectorInitialPos { get; set; }

            /// <summary>
            ///     Gets or sets the initial local position of the IK goal in a transition to a pressed state.
            /// </summary>
            public Vector3 LocalGoalTransitionStartPos { get; set; }

            /// <summary>
            ///     Gets or sets the current goal transform. The goal is the transform that the finger will try to reach whenever the
            ///     input element is pressed.
            /// </summary>
            public Transform CurrentFingerGoal { get; set; }

            /// <summary>
            ///     The or sets current timer in the transition to a pressed state, to smoothly transition the finger from its default
            ///     position to the pressed position.
            /// </summary>
            public float TimerToGoal { get; set; }

            /// <summary>
            ///     Gets or sets whether the component is enabled. If it is disabled no IK will take place.
            /// </summary>
            public bool ComponentEnabled { get; set; }

            #endregion
        }

        #endregion
    }
}