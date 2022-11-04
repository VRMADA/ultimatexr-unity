// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWristTorsionIKSolver.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     IK solver that distributes a wrist torsion among different bones in a forearm in order to smooth it out.
    /// </summary>
    public class UxrWristTorsionIKSolver : UxrIKSolver
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] [Range(0.0f, 1.0f)] private float _amount = 1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the amount of torsion to apply on this Transform from the source. 0 = no torsion, 1 = full torsion,
        ///     etc.
        /// </summary>
        public float Amount
        {
            get => _amount;
            set => _amount = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _handSide           = transform.HasParent(Avatar.AvatarRig.LeftArm.UpperArm) ? UxrHandSide.Left : UxrHandSide.Right;
            _startLocalRotation = transform.localRotation;

            UxrUniversalLocalAxes handUniversalLocalAxes = Avatar.AvatarRigInfo.GetArmInfo(_handSide).HandUniversalLocalAxes;
            _torsionLocalAxis = transform.InverseTransformDirection(handUniversalLocalAxes.WorldForward).GetClosestAxis();
        }

        #endregion

        #region Protected Overrides UxrIKSolver

        /// <summary>
        ///     Solves the Inverse Kinematics.
        /// </summary>
        protected override void InternalSolveIK()
        {
            float angle = Avatar.AvatarRigInfo.GetArmInfo(_handSide).WristTorsionInfo.WristTorsionAngle;
            transform.localRotation = _startLocalRotation * Quaternion.AngleAxis(angle * Amount, _torsionLocalAxis);
        }

        #endregion

        #region Private Types & Data

        private UxrHandSide _handSide;
        private Quaternion  _startLocalRotation;
        private Vector3     _torsionLocalAxis;

        #endregion
    }
}