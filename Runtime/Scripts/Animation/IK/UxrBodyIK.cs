// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrBodyIK.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Class that provides functionality to compute Inverse Kinematics for a humanoid body.
    /// </summary>
    public sealed partial class UxrBodyIK
    {
        #region Public Methods

        /// <summary>
        ///     Initializes the IK object.
        /// </summary>
        /// <param name="avatar">Avatar that the IK will be computed for</param>
        /// <param name="settings">IK settings to use</param>
        /// <param name="usesExternalArmIK">Whether the avatar uses arm IK from other side</param>
        /// <param name="usesExternalLegIK">Whether the avatar uses leg IK from other side</param>
        public void Initialize(UxrAvatar avatar, UxrBodyIKSettings settings, bool usesExternalArmIK, bool usesExternalLegIK)
        {
            _avatar          = avatar;
            _avatarTransform = avatar.transform;
            _settings        = settings;
            
            // Get body root

            if (avatar.AvatarRig.Head.Head == null)
            {
                Debug.LogError($"Avatar {avatar.name} has no head setup in the {nameof(UxrAvatar)}'s Rig field");
                return;
            }

            _independentBones = new List<IndependentBoneInfo>();

            List<Transform> bodyTransforms = new List<Transform>();

            if (avatar.AvatarRig.UpperChest)
            {
                bodyTransforms.Add(avatar.AvatarRig.UpperChest);
                _upperChestUniversalLocalAxes = new UxrUniversalLocalAxes(avatar.AvatarRig.UpperChest, _avatarTransform);
            }

            if (avatar.AvatarRig.Chest)
            {
                bodyTransforms.Add(avatar.AvatarRig.Chest);
                _chestUniversalLocalAxes = new UxrUniversalLocalAxes(avatar.AvatarRig.Chest, _avatarTransform);
            }

            if (avatar.AvatarRig.Spine)
            {
                bodyTransforms.Add(avatar.AvatarRig.Spine);
                _spineUniversalLocalAxes = new UxrUniversalLocalAxes(avatar.AvatarRig.Spine, _avatarTransform);
            }

            if (avatar.AvatarRig.Hips)
            {
                bodyTransforms.Add(avatar.AvatarRig.Hips);
            }

            if (avatar.AvatarRig.Head.Neck && avatar.AvatarRig.Head.Neck != null && avatar.AvatarRig.Head.Neck != _avatarTransform && !bodyTransforms.Contains(avatar.AvatarRig.Head.Neck.parent))
            {
                bodyTransforms.Add(avatar.AvatarRig.Head.Neck.parent);
            }

            if (avatar.AvatarRig.UpperChest && avatar.AvatarRig.UpperChest.parent != null && avatar.AvatarRig.UpperChest.parent != _avatarTransform && !bodyTransforms.Contains(avatar.AvatarRig.UpperChest.parent))
            {
                bodyTransforms.Add(avatar.AvatarRig.UpperChest.parent);
            }

            if (avatar.AvatarRig.Chest && avatar.AvatarRig.Chest.parent != null && avatar.AvatarRig.Chest.parent != _avatarTransform && !bodyTransforms.Contains(avatar.AvatarRig.Chest.parent))
            {
                bodyTransforms.Add(avatar.AvatarRig.Chest.parent);
            }

            if (avatar.AvatarRig.Spine && avatar.AvatarRig.Spine.parent != null && avatar.AvatarRig.Spine.parent != _avatarTransform && !bodyTransforms.Contains(avatar.AvatarRig.Spine.parent))
            {
                bodyTransforms.Add(avatar.AvatarRig.Spine.parent);
            }

            if (avatar.AvatarRig.Hips && avatar.AvatarRig.Hips.parent != null && avatar.AvatarRig.Hips.parent != _avatarTransform && !bodyTransforms.Contains(avatar.AvatarRig.Hips.parent))
            {
                bodyTransforms.Add(avatar.AvatarRig.Hips.parent);
            }

            _avatarBodyRoot = TransformExt.GetCommonRootTransformFromSet(bodyTransforms.ToArray());

            if (_avatarBodyRoot == null)
            {
                Debug.LogWarning("No common avatar body root found. If there is an avatar body it will not follow the head position.");
                _avatarBodyRoot        = new GameObject("Dummy Root").transform;
                _avatarBodyRoot.parent = _avatarTransform;
                _avatarBodyRoot.SetPositionAndRotation(_avatarTransform.position, _avatarTransform.rotation);
            }

            // Neck

            _avatarNeck = avatar.AvatarRig.Head.Neck;

            if (_avatarNeck == null)
            {
                _avatarNeck        = new GameObject("Dummy Neck").transform;
                _avatarNeck.parent = _avatarBodyRoot;
                _avatarNeck.SetPositionAndRotation(_avatarTransform.position + _avatarTransform.up * _settings.NeckBaseHeight + _avatarTransform.forward * _settings.NeckForwardOffset, _avatarTransform.rotation);

                if (avatar.AvatarRig.Head.Head != null)
                {
                    avatar.AvatarRig.Head.Head.SetParent(_avatarNeck);
                }
            }

            _neckUniversalLocalAxes = new UxrUniversalLocalAxes(_avatarNeck, _avatarTransform);

            // Head

            _headUniversalLocalAxes = new UxrUniversalLocalAxes(avatar.AvatarRig.Head.Head, _avatarTransform);
            _avatarHead             = avatar.AvatarRig.Head.Head;

            // Eyes

            _avatarEyes        = new GameObject("Dummy Eyes").transform;
            _avatarEyes.parent = _avatarHead;
            _avatarEyes.SetPositionAndRotation(_avatarTransform.position + _avatarTransform.up * _settings.EyesBaseHeight + _avatarTransform.forward * _settings.EyesForwardOffset, _avatarTransform.rotation);

            // Avatar Forward

            _avatarForward        = new GameObject("Dummy Forward").transform;
            _avatarForward.parent = _avatarTransform;
            _avatarForward.SetPositionAndRotation(_avatarHead.position - Vector3.up * _avatarHead.position.y, _avatarTransform.rotation);

            _avatarBodyRoot.parent = _avatarForward;

            // Initialize

            _neckPosRelativeToEyes          = _avatarEyes.InverseTransformPoint(_avatarNeck.position);
            _neckRotRelativeToEyes          = Quaternion.Inverse(_avatarEyes.rotation) * _avatarNeck.rotation;
            _avatarForwardPosRelativeToNeck = _avatarForward.position - _avatarNeck.transform.position;
            _avatarForwardTarget            = _avatarForward.forward;
            _straightSpineForward           = _avatarForward.forward;

            if (usesExternalArmIK)
            {
                if (avatar.LeftHandBone)
                {
                    _independentBones.Add(new IndependentBoneInfo(avatar.LeftHandBone));
                }

                if (avatar.RightHandBone)
                {
                    _independentBones.Add(new IndependentBoneInfo(avatar.RightHandBone));
                }
            }
        }

        /// <summary>
        ///     Computes the Pre-pass in the IK solving.
        /// </summary>
        public void PreSolveAvatarIK()
        {
            if (_avatarHead == null)
            {
                return;
            }

            // Push transforms. These transforms hang from the body and need to be kept in place because
            // due to parenting their absolute position/orientation will be altered

            foreach (IndependentBoneInfo boneInfo in _independentBones)
            {
                boneInfo.Rotation = boneInfo.Transform.rotation;
                boneInfo.Position = boneInfo.Transform.position;
            }

            // Reset upper body

            _avatarNeck.localPosition = _neckUniversalLocalAxes.InitialLocalPosition;
            _avatarHead.localPosition = _headUniversalLocalAxes.InitialLocalPosition;
            _avatarNeck.localRotation = _neckUniversalLocalAxes.InitialLocalRotation;
            _avatarHead.localRotation = _headUniversalLocalAxes.InitialLocalRotation;

            if (_avatar.AvatarRig.UpperChest)
            {
                _avatar.AvatarRig.UpperChest.localRotation = _upperChestUniversalLocalAxes.InitialLocalRotation;
            }

            if (_avatar.AvatarRig.Chest)
            {
                _avatar.AvatarRig.Chest.localRotation = _chestUniversalLocalAxes.InitialLocalRotation;
            }

            if (_avatar.AvatarRig.Spine)
            {
                _avatar.AvatarRig.Spine.localRotation = _spineUniversalLocalAxes.InitialLocalRotation;
            }

            // Compute neck position/rotation to make the avatar eyes match the camera

            Transform  cameraTransform = _avatar.CameraComponent.transform;
            Vector3    avatarPivotPos  = _avatarForward.position;
            Vector3    neckPosition    = cameraTransform.TransformPoint(_neckPosRelativeToEyes);
            Quaternion neckRotation    = cameraTransform.rotation * _neckRotRelativeToEyes;

            _avatarNeck.SetPositionAndRotation(neckPosition, neckRotation);

            // Update avatar pivot

            _avatarForward.position = GetAvatarForwardOffset(_avatarForward, _avatarNeck, _avatarForwardPosRelativeToNeck);

            if (Vector3.Angle(cameraTransform.forward, Vector3.up) > CameraUpsideDownAngleThreshold &&
                Vector3.Angle(cameraTransform.forward, -Vector3.up) > CameraUpsideDownAngleThreshold &&
                Vector3.Angle(cameraTransform.forward, _avatarForward.forward) < 90.0f)
            {
                // _straightSpineForward contains the forward direction where the avatar looks (vector.y is 0).
                // This is different from _avatarForward.forward because avatarForward allows the head to rotate
                // some degrees without rotating the whole body along with it.
                _straightSpineForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            }

            float bodyRotationAngle = Vector3.Angle(_straightSpineForward, _avatarForward.forward);
            if (bodyRotationAngle > _settings.HeadFreeRangeTorsion)
            {
                float radians = (bodyRotationAngle - _settings.HeadFreeRangeTorsion) * Mathf.Deg2Rad;
                _avatarForwardTarget = Vector3.RotateTowards(_avatarForwardTarget, _straightSpineForward, radians, 0.0f);
            }

            // Update avatar forward direction

            float rotationSpeedMultiplier = Vector3.Angle(_avatarForward.forward, _avatarForwardTarget) / 30.0f;
            _avatarForward.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(_avatarForward.forward),
                                                               Quaternion.LookRotation(_avatarForwardTarget),
                                                               AvatarRotationDegreesPerSecond * rotationSpeedMultiplier * _settings.BodyPivotRotationSpeed * Time.deltaTime);

            // Since the avatar pivot is parent of all body nodes, move the neck back to its position
            _avatarNeck.SetPositionAndRotation(neckPosition, neckRotation);

            if (_settings.LockBodyPivot)
            {
                _avatarForward.position = avatarPivotPos;
            }

            // We've computed the avatar head orientation using only the neck. Now redistribute it using the _neckHeadBalance parameter so that
            // we can get a smoother head rotation by using both the neck and the head.

            if (_settings.NeckHeadBalance > 0.0f)
            {
                Quaternion neckUniversalRotation = Quaternion.Inverse(_avatarForward.rotation) * _neckUniversalLocalAxes.UniversalRotation * Quaternion.Inverse(_neckUniversalLocalAxes.InitialUniversalLocalReferenceRotation);

                Vector3    headPosition = _avatarHead.position;
                Quaternion headRotation = _avatarHead.rotation;

                // Compute partial neck rotation
                neckRotation = _avatarForward.rotation * (Quaternion.Slerp(Quaternion.identity, neckUniversalRotation, 1.0f - _settings.NeckHeadBalance) *
                                                          _neckUniversalLocalAxes.InitialUniversalLocalReferenceRotation *
                                                          _neckUniversalLocalAxes.UniversalToActualAxesRotation);

                _avatarNeck.rotation = neckRotation;
                _avatarHead.rotation = headRotation;

                _avatarForward.position -= _avatarHead.position - headPosition;

                neckPosition = _avatarNeck.position;
            }

            // Check influence of head rotation on chest/spine transforms

            if (!_settings.LockBodyPivot)
            {
                // Compute head rotation without the rotation around the Y axis:
                Quaternion headPropagateRotation = Quaternion.Inverse(Quaternion.LookRotation(_straightSpineForward)) * _headUniversalLocalAxes.UniversalRotation;

                // Remove the rotation that the head can do without propagation to chest/spine:
                headPropagateRotation = Quaternion.RotateTowards(headPropagateRotation, Quaternion.identity, _settings.HeadFreeRangeBend);

                // Compute influence on chest/spine elements:

                float totalWeight = 0.0f;

                if (_avatar.AvatarRig.UpperChest)
                {
                    totalWeight += _settings.UpperChestBend;
                }

                if (_avatar.AvatarRig.Chest)
                {
                    totalWeight += _settings.ChestBend;
                }

                if (_avatar.AvatarRig.Spine)
                {
                    totalWeight += _settings.SpineBend;
                }

                // Propagate head rotation:

                if (totalWeight > 0.0f)
                {
                    if (totalWeight < 1.0f)
                    {
                        totalWeight = 1.0f;
                    }

                    if (_avatar.AvatarRig.UpperChest)
                    {
                        _avatar.AvatarRig.UpperChest.rotation = _avatarForward.rotation *
                                                                (Quaternion.Slerp(Quaternion.identity, headPropagateRotation, _settings.UpperChestBend / totalWeight) *
                                                                 _upperChestUniversalLocalAxes.InitialUniversalLocalReferenceRotation * _upperChestUniversalLocalAxes.UniversalToActualAxesRotation);
                    }

                    if (_avatar.AvatarRig.Chest)
                    {
                        _avatar.AvatarRig.Chest.rotation = _avatarForward.rotation *
                                                           (Quaternion.Slerp(Quaternion.identity, headPropagateRotation, _settings.ChestBend / totalWeight) *
                                                            _chestUniversalLocalAxes.InitialUniversalLocalReferenceRotation * _chestUniversalLocalAxes.UniversalToActualAxesRotation);
                    }

                    if (_avatar.AvatarRig.Spine)
                    {
                        _avatar.AvatarRig.Spine.rotation = _avatarForward.rotation *
                                                           (Quaternion.Slerp(Quaternion.identity, headPropagateRotation, _settings.SpineBend / totalWeight) *
                                                            _spineUniversalLocalAxes.InitialUniversalLocalReferenceRotation * _spineUniversalLocalAxes.UniversalToActualAxesRotation);
                    }
                }

                // Make whole avatar move back so that head remains with the same position/orientation

                _avatarNeck.rotation    =  neckRotation;
                _avatarForward.position -= _avatarNeck.position - neckPosition;
                _avatarNeck.position    =  neckPosition;
            }

            // If the avatar moves, straighten the forward direction

            float avatarMovedDistance = Vector3.Distance(avatarPivotPos, _avatarForward.position);

            if (avatarMovedDistance / Time.deltaTime > AvatarStraighteningMinSpeed)
            {
                float degreesToStraighten = avatarMovedDistance * DegreesStraightenedPerMeterMoved;
                _avatarForwardTarget = Vector3.RotateTowards(_avatarForwardTarget, _straightSpineForward, degreesToStraighten * Mathf.Deg2Rad, 0.0f);
            }

            // Pop independent transforms (usually hands and other transforms with their own sensors)

            foreach (IndependentBoneInfo boneInfo in _independentBones)
            {
                boneInfo.Transform.SetPositionAndRotation(boneInfo.Position, boneInfo.Rotation);
            }
        }

        /// <summary>
        ///     Computes the post-pass in the IK solving.
        /// </summary>
        public void PostSolveAvatarIK()
        {
            if (_avatarHead == null)
            {
                return;
            }

            // Push transforms. These transforms hang from the body and need to be kept in place because
            // due to parenting their absolute position/orientation will be altered

            foreach (IndependentBoneInfo boneInfo in _independentBones)
            {
                boneInfo.Rotation = boneInfo.Transform.rotation;
                boneInfo.Position = boneInfo.Transform.position;
            }

            Vector3    neckPosition = _avatarNeck.position;
            Quaternion neckRotation = _avatarNeck.rotation;

            // Compute torso rotation

            float torsoRotation = 0.0f; // No degrees, just an abstract quantity we use later to multiply by a factor and get degrees
            float totalWeight   = 0.0f;

            if (_avatar.AvatarRig.UpperChest)
            {
                torsoRotation = GetArmTorsoRotationWeight(_avatar.AvatarRig.UpperChest, _upperChestUniversalLocalAxes, _avatar.AvatarRig.LeftArm,  UxrHandSide.Left) +
                                GetArmTorsoRotationWeight(_avatar.AvatarRig.UpperChest, _upperChestUniversalLocalAxes, _avatar.AvatarRig.RightArm, UxrHandSide.Right);

                totalWeight += _settings.UpperChestTorsion;
            }

            if (_avatar.AvatarRig.Chest)
            {
                torsoRotation = GetArmTorsoRotationWeight(_avatar.AvatarRig.Chest, _chestUniversalLocalAxes, _avatar.AvatarRig.LeftArm,  UxrHandSide.Left) +
                                GetArmTorsoRotationWeight(_avatar.AvatarRig.Chest, _chestUniversalLocalAxes, _avatar.AvatarRig.RightArm, UxrHandSide.Right);

                totalWeight += _settings.ChestTorsion;
            }

            if (_avatar.AvatarRig.Spine)
            {
                torsoRotation = GetArmTorsoRotationWeight(_avatar.AvatarRig.Spine, _spineUniversalLocalAxes, _avatar.AvatarRig.LeftArm,  UxrHandSide.Left) +
                                GetArmTorsoRotationWeight(_avatar.AvatarRig.Spine, _spineUniversalLocalAxes, _avatar.AvatarRig.RightArm, UxrHandSide.Right);

                totalWeight += _settings.SpineTorsion;
            }

            torsoRotation *= 0.5f; // Because range is [-2, +2] since both hands have [-1, 1] range.

            // Rotate upper body elements

            float maxRotationDegrees = 70.0f;

            if (totalWeight > 0.0f)
            {
                if (totalWeight < 1.0f)
                {
                    totalWeight = 1.0f;
                }

                float upperChestTorsionAngle = maxRotationDegrees * torsoRotation * (_settings.UpperChestTorsion / totalWeight);
                float chestTorsionAngle      = maxRotationDegrees * torsoRotation * (_settings.ChestTorsion / totalWeight);
                float spineTorsionAngle      = maxRotationDegrees * torsoRotation * (_settings.SpineTorsion / totalWeight);

                _upperChestTorsionAngle = Mathf.SmoothDampAngle(_upperChestTorsionAngle, upperChestTorsionAngle, ref _upperChestTorsionSpeed, BodyTorsionSmoothTime);
                _chestTorsionAngle      = Mathf.SmoothDampAngle(_chestTorsionAngle,      chestTorsionAngle,      ref _chestTorsionSpeed,      BodyTorsionSmoothTime);
                _spineTorsionAngle      = Mathf.SmoothDampAngle(_spineTorsionAngle,      spineTorsionAngle,      ref _spineTorsionSpeed,      BodyTorsionSmoothTime);

                if (_avatar.AvatarRig.UpperChest)
                {
                    _avatar.AvatarRig.UpperChest.localRotation *= Quaternion.AngleAxis(_spineTorsionAngle, _upperChestUniversalLocalAxes.LocalUp);
                }

                if (_avatar.AvatarRig.Chest)
                {
                    _avatar.AvatarRig.Chest.localRotation *= Quaternion.AngleAxis(_chestTorsionAngle, _chestUniversalLocalAxes.LocalUp);
                }

                if (_avatar.AvatarRig.Spine)
                {
                    _avatar.AvatarRig.Spine.localRotation *= Quaternion.AngleAxis(_spineTorsionAngle, _spineUniversalLocalAxes.LocalUp);
                }
            }

            // Pop independent transforms (usually hands and other transforms with their own sensors)

            foreach (IndependentBoneInfo boneInfo in _independentBones)
            {
                boneInfo.Transform.SetPositionAndRotation(boneInfo.Position, boneInfo.Rotation);
            }

            _avatarNeck.SetPositionAndRotation(neckPosition, neckRotation);
        }

        /// <summary>
        ///     Notifies whenever the avatar was moved in order to update the internal forward looking vector.
        /// </summary>
        /// <param name="e">Move event parameters</param>
        public void NotifyAvatarMoved(UxrAvatarMoveEventArgs e)
        {
            float angle = Vector3.SignedAngle(e.OldForward, e.NewForward, Vector3.up);
            _avatarForwardTarget  = Quaternion.AngleAxis(angle, Vector3.up) * _avatarForwardTarget;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes an offset vector in avatar space.
        /// </summary>
        /// <param name="avatarForward">The avatar forward transform</param>
        /// <param name="transform">The offset origin</param>
        /// <param name="offset">The offset components</param>
        /// <returns>Offset vector</returns>
        private Vector3 GetAvatarForwardOffset(Transform avatarForward, Transform transform, Vector3 offset)
        {
            return transform.position + avatarForward.right * offset.x + avatarForward.up * offset.y + avatarForward.forward * offset.z;
        }

        /// <summary>
        ///     Computes how much of an influence an arm has on the upper body to make it rotate left or right.
        /// </summary>
        /// <param name="upperBodyTransform">Any upper body node (spine, chest...)</param>
        /// <param name="upperBodyUniversalLocalAxes">
        ///     The upper body nodes's universal local axes.
        ///     These will be the upper body transform local axes that map to the avatar right, up and
        ///     forward vectors.
        /// </param>
        /// <param name="arm">The arm</param>
        /// <param name="handSide">Is it a left arm or right arm?</param>
        /// <returns>
        ///     Value [-1.0, 1.0] that tells how much the upper body should rotate to the left or
        ///     right due to the arm position.
        /// </returns>
        private float GetArmTorsoRotationWeight(Transform upperBodyTransform, UxrUniversalLocalAxes upperBodyUniversalLocalAxes, UxrAvatarArm arm, UxrHandSide handSide)
        {
            if (arm.UpperArm && arm.Forearm)
            {
                Vector3 shoulderToElbowVector          = arm.Forearm.position - arm.UpperArm.position;
                Vector3 projectedShoulderToElbowVector = Vector3.ProjectOnPlane(shoulderToElbowVector, upperBodyUniversalLocalAxes.WorldUp);

                float straightFactor = projectedShoulderToElbowVector.magnitude / shoulderToElbowVector.magnitude;
                float armAngle       = Vector3.SignedAngle(upperBodyUniversalLocalAxes.WorldRight * (handSide == UxrHandSide.Left ? -1.0f : 1.0f), projectedShoulderToElbowVector, upperBodyUniversalLocalAxes.WorldUp);

                return armAngle / 180.0f * straightFactor;
            }

            return 0.0f;
        }

        #endregion

        #region Private Types & Data

        private const float CameraUpsideDownAngleThreshold   = 15.0f;   // To avoid gimbal errors we will not update the avatar rotation if the avatar is looking up or down with this angle respect to the vertical axis.
        private const float AvatarRotationDegreesPerSecond   = 1080.0f; // Degrees per second we will move the avatar to compensate for the head torsion
        private const float DegreesStraightenedPerMeterMoved = 120.0f;  // Amount of degrees to straighten the avatar direction with respect to the head direction when the avatar moves.
        private const float AvatarStraighteningMinSpeed      = 0.3f;    // To straighten the avatar direction it will need to be moving at least this amount of units per second
        private const float BodyTorsionSmoothTime            = 0.1f;    // Body torsion smoothening time

        private UxrAvatar                 _avatar;
        private Transform                 _avatarTransform;
        private UxrBodyIKSettings         _settings;
        private List<IndependentBoneInfo> _independentBones;

        private Transform _avatarBodyRoot;
        private Transform _avatarForward;
        private Transform _avatarNeck;
        private Transform _avatarHead;
        private Transform _avatarEyes;

        private Vector3    _neckPosRelativeToEyes;
        private Quaternion _neckRotRelativeToEyes;
        private Vector3    _avatarForwardPosRelativeToNeck;
        private Vector3    _avatarForwardTarget;
        private Vector3    _straightSpineForward;

        private UxrUniversalLocalAxes _spineUniversalLocalAxes;
        private UxrUniversalLocalAxes _chestUniversalLocalAxes;
        private UxrUniversalLocalAxes _upperChestUniversalLocalAxes;
        private UxrUniversalLocalAxes _neckUniversalLocalAxes;
        private UxrUniversalLocalAxes _headUniversalLocalAxes;
        private float                 _spineTorsionAngle;
        private float                 _chestTorsionAngle;
        private float                 _upperChestTorsionAngle;
        private float                 _spineTorsionSpeed;
        private float                 _chestTorsionSpeed;
        private float                 _upperChestTorsionSpeed;

        #endregion
    }
}