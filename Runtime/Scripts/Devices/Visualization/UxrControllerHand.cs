// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHand.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     Component that represents a hand holding a VR controller. It allows to graphically render a hand that
    ///     mimics the interaction that the user performs on a VR controller.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class UxrControllerHand : UxrAvatarComponent<UxrControllerHand>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                  _hasAvatarSource;
        [SerializeField] private UxrAvatar             _avatarPrefab;
        [SerializeField] private UxrHandSide           _avatarHandSide;
        [SerializeField] private UxrHandPoseAsset      _handPose;
        [SerializeField] private List<ObjectVariation> _variations;
        [SerializeField] private UxrAvatarHand         _hand;
        [SerializeField] private FingerIK              _thumb;
        [SerializeField] private FingerIK              _index;
        [SerializeField] private FingerIK              _middle;
        [SerializeField] private FingerIK              _ring;
        [SerializeField] private FingerIK              _little;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the hand references.
        /// </summary>
        public UxrAvatarHand Hand => _hand;

        /// <summary>
        ///     Gets the hand variations, if there are any. Variations allow to provide different visual representations of the
        ///     hand. It can be different objects and each object may have different materials.
        /// </summary>
        public IEnumerable<ObjectVariation> Variations => _variations;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes the component when the controller hand is dynamic, such as when used through an
        ///     <see cref="UxrAvatar" /> that changes poses.
        /// </summary>
        /// <param name="avatar">Avatar the hand belongs to. The current enabled pose will be used to initialize the finger IK.</param>
        /// <param name="handSide">Which hand side</param>
        public void InitializeFromCurrentHandPose(UxrAvatar avatar, UxrHandSide handSide)
        {
            UxrRuntimeHandPose runtimeHandPose = avatar.GetCurrentRuntimeHandPose(handSide);

            if (runtimeHandPose != null)
            {
                // Take snapshot of transforms

                var transforms = UxrAvatarRig.PushHandTransforms(avatar.GetHand(handSide));

                // Briefly switch to pose. This is used because we may be in the middle of a transition and the pose hasn't been acquired yet.

                UxrAvatarRig.UpdateHandUsingRuntimeDescriptor(avatar, handSide, runtimeHandPose.GetHandDescriptor(handSide));

                // Initialize IK

                InitializeFinger(_thumb);
                InitializeFinger(_index);
                InitializeFinger(_middle);
                InitializeFinger(_ring);
                InitializeFinger(_little);

                // Restore transforms from snapshot

                UxrAvatarRig.PopHandTransforms(avatar.GetHand(handSide), transforms);
            }
        }

        /// <summary>
        ///     Updates a given finger.
        /// </summary>
        /// <param name="finger">Finger to update</param>
        /// <param name="fingerContactInfo">Finger contact information</param>
        public void UpdateFinger(UxrFingerType finger, UxrFingerContactInfo fingerContactInfo)
        {
            if (_fingers == null)
            {
                return;
            }

            if (_fingers.TryGetValue(finger, out FingerIK fingerInfo))
            {
                if (fingerInfo.FingerIKSolver == null)
                {
                    return;
                }

                Transform fingerIKParent = fingerInfo.FingerIKSolver.Links[0].Bone.parent;

                if (fingerInfo.CurrentFingerGoal != fingerContactInfo.Transform)
                {
                    fingerInfo.CurrentFingerGoal           = fingerContactInfo.Transform;
                    fingerInfo.TimerToGoal                 = fingerInfo.FingerToGoalDuration;
                    fingerInfo.LocalGoalTransitionStartPos = fingerIKParent.InverseTransformPoint(fingerInfo.FingerIKSolver.Goal.position);
                }

                if (fingerInfo.TimerToGoal > 0.0f)
                {
                    fingerInfo.TimerToGoal -= Time.deltaTime;
                    float t = 1.0f - Mathf.Clamp01(fingerInfo.TimerToGoal / fingerInfo.FingerToGoalDuration);
                    fingerInfo.FingerIKSolver.Goal.position = Vector3.Lerp(fingerIKParent.TransformPoint(fingerInfo.LocalGoalTransitionStartPos),
                                                                           fingerContactInfo.Transform != null ? fingerContactInfo.Transform.position : fingerIKParent.TransformPoint(fingerInfo.LocalEffectorInitialPos),
                                                                           t);

                    if (fingerContactInfo.Transform != null)
                    {
                        fingerInfo.FingerIKSolver.SolverEnabled = true;
                    }
                }
                else
                {
                    if (fingerContactInfo.Transform != null)
                    {
                        fingerInfo.FingerIKSolver.Goal.position = fingerContactInfo.Transform.position;
                    }
                    else
                    {
                        fingerInfo.FingerIKSolver.SolverEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Allows to manually update the Inverse Kinematics of all fingers.
        /// </summary>
        public void UpdateIKManually()
        {
            foreach (KeyValuePair<UxrFingerType, FingerIK> fingerPair in _fingers)
            {
                if (fingerPair.Value.FingerIKSolver && fingerPair.Value.FingerIKSolver.SolverEnabled)
                {
                    fingerPair.Value.FingerIKSolver.SolveIK();
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Generates the internal list of fingers.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _fingers = new Dictionary<UxrFingerType, FingerIK>();

            _thumb.ComponentEnabled  = _thumb.FingerIKSolver != null && _thumb.FingerIKSolver.enabled;
            _index.ComponentEnabled  = _index.FingerIKSolver != null && _index.FingerIKSolver.enabled;
            _middle.ComponentEnabled = _middle.FingerIKSolver != null && _middle.FingerIKSolver.enabled;
            _ring.ComponentEnabled   = _ring.FingerIKSolver != null && _ring.FingerIKSolver.enabled;
            _little.ComponentEnabled = _little.FingerIKSolver != null && _little.FingerIKSolver.enabled;

            _fingers.Add(UxrFingerType.Thumb,  _thumb);
            _fingers.Add(UxrFingerType.Index,  _index);
            _fingers.Add(UxrFingerType.Middle, _middle);
            _fingers.Add(UxrFingerType.Ring,   _ring);
            _fingers.Add(UxrFingerType.Little, _little);
        }

        /// <summary>
        ///     Enables the finger IK solvers.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.StageUpdating += UxrManager_StageUpdating;
        }

        /// <summary>
        ///     Disables the finger IK solvers.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.StageUpdating -= UxrManager_StageUpdating;
        }

        /// <summary>
        ///     Make sure the hand rig is allocated when the component is reset.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            
            _hand = new UxrAvatarHand();
        }

        /// <summary>
        ///     Initializes the fingers.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (Avatar == null)
            {
                UxrManager.LogMissingAvatarInHierarchyError(this);
            }

            InitializeFinger(_thumb);
            InitializeFinger(_index);
            InitializeFinger(_middle);
            InitializeFinger(_ring);
            InitializeFinger(_little);
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the <see cref="UxrManager" /> is about to update a stage.
        /// </summary>
        /// <param name="stage"></param>
        private void UxrManager_StageUpdating(UxrUpdateStage stage)
        {
            if (stage == UxrUpdateStage.Animation)
            {
                // Check resetting bone transforms?
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes a finger.
        /// </summary>
        /// <param name="finger">The finger to initialize</param>
        private void InitializeFinger(FingerIK finger)
        {
            if (finger.FingerIKSolver != null)
            {
                // We recompute the link data because after the finger is initialized it has changed its animation pose
                finger.FingerIKSolver.ComputeLinkData();
                finger.FingerIKSolver.enabled  = finger.ComponentEnabled;
                finger.LocalEffectorInitialPos = finger.FingerIKSolver.Links[0].Bone.parent.InverseTransformPoint(finger.FingerIKSolver.EndEffector.position);
                finger.FingerIKSolver.Goal.SetPositionAndRotation(finger.FingerIKSolver.EndEffector.position, finger.FingerIKSolver.EndEffector.rotation);
                finger.CurrentFingerGoal = null;
                finger.TimerToGoal       = -1.0f;
            }
        }

        #endregion

        #region Private Types & Data

        private Dictionary<UxrFingerType, FingerIK> _fingers;

        #endregion
    }
}