// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrIKSolver.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Base IK Solver class. IK solvers should inherit from it and override the <see cref="InternalSolveIK" /> method.
    ///     Not all solvers need to be part of an avatar, but the <see cref="UxrAvatarComponent{T}" /> inheritance is used to
    ///     be able to enumerate all the solvers that are part of an avatar.
    /// </summary>
    public abstract class UxrIKSolver : UxrAvatarComponent<UxrIKSolver>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _enabled = true;
        [SerializeField] private bool _manualUpdate;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called right before the IK is about to be solved during the current frame
        /// </summary>
        public event Action Solving;

        /// <summary>
        ///     Called right after the IK was solved during the current frame
        /// </summary>
        public event Action Solved;

        /// <summary>
        ///     Gets if the solver needs to be updated automatically.
        /// </summary>
        public bool NeedsAutoUpdate => gameObject.activeInHierarchy && enabled && SolverEnabled && !ManualUpdate;

        /// <summary>
        ///     Gets or sets the IK solver enabled state?
        /// </summary>
        public bool SolverEnabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        ///     Gets if the IK solver will update itself. Otherwise the user will be responsible of calling <see cref="SolveIK" />.
        /// </summary>
        public bool ManualUpdate
        {
            get => _manualUpdate;
            set => _manualUpdate = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Solves the IK. Calls <see cref="InternalSolveIK" />,which is implemented in child classes, but calls the
        ///     appropriate <see cref="Solving" /> and <see cref="Solved" /> events.
        /// </summary>
        public void SolveIK()
        {
            Solving?.Invoke();
            InternalSolveIK();
            Solved?.Invoke();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            UxrManager.StageUpdating += UxrManager_StageUpdating;
        }

        /// <summary>
        ///     Unsubscribes from events
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            UxrManager.StageUpdating -= UxrManager_StageUpdating;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Will solve the IK chain in case it is not part of an avatar. If it is part of a VR avatar, the VR avatar will take
        ///     care of calling the SolveIK method so that it is processed in the correct order, after the hands are updated.
        /// </summary>
        private void UxrManager_StageUpdating(UxrUpdateStage stage)
        {
            if (stage == UxrUpdateStage.PostProcess && Avatar == null && NeedsAutoUpdate)
            {
                SolveIK();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     To be implemented in child classes to execute the actual IK solving algorithm for the current frame
        /// </summary>
        protected abstract void InternalSolveIK();

        #endregion
    }
}