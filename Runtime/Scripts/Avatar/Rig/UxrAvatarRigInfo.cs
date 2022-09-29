// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRigInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     <para>
    ///         Stores information about the rig to facilitate transformations no matter the coordinate system used by
    ///         the avatar hierarchy (See <see cref="UxrUniversalLocalAxes" />).
    ///     </para>
    ///     It also stores lengths and sizes that can facilitate operations such as Inverse Kinematics, calibration, etc.
    /// </summary>
    [Serializable]
    public class UxrAvatarRigInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAvatar             _avatar;
        [SerializeField] private float                 _leftUpperArmLength;
        [SerializeField] private float                 _rightUpperArmLength;
        [SerializeField] private float                 _leftForearmLength;
        [SerializeField] private float                 _rightForearmLength;
        [SerializeField] private UxrUniversalLocalAxes _leftFingerUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _rightFingerUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _leftHandUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _rightHandUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _leftArmUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _rightArmUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _leftForearmUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _rightForearmUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _leftClavicleUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _rightClavicleUniversalLocalAxes;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the left upper arm length. From shoulder to elbow.
        /// </summary>
        public float LeftUpperArmLength => _leftUpperArmLength;

        /// <summary>
        ///     Gets the right upper arm length. From shoulder to elbow.
        /// </summary>
        public float RightUpperArmLength => _rightUpperArmLength;

        /// <summary>
        ///     Gets the left forearm length. From elbow to wrist.
        /// </summary>
        public float LeftForearmLength => _leftForearmLength;

        /// <summary>
        ///     Gets the right forearm length. From elbow to wrist.
        /// </summary>
        public float RightForearmLength => _rightForearmLength;

        /// <summary>
        ///     Gets the universal coordinate system for the left fingers: right = axis around which the finger curls, up =
        ///     knuckles up, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes LeftFingerUniversalLocalAxes => _leftFingerUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the right fingers: right = axis around which the finger curls, up =
        ///     knuckles up, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes RightFingerUniversalLocalAxes => _rightFingerUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the left hand: right = from wrist to right (thumb direction), up = -palm
        ///     facing vector, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes LeftHandUniversalLocalAxes => _leftHandUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the left hand: right = from wrist to right (opposite of thumb direction),
        ///     up = -palm facing vector, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes RightHandUniversalLocalAxes => _rightHandUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the left arm: forward is arm->elbow, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes LeftArmUniversalLocalAxes => _leftArmUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the right arm: forward is arm->elbow, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes RightArmUniversalLocalAxes => _rightArmUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the left forearm: forward is arm->hand, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes LeftForearmUniversalLocalAxes => _leftForearmUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the right forearm: forward is arm->hand, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes RightForearmUniversalLocalAxes => _rightForearmUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the left clavicle: forward is clavicle->arm, up is avatar up axis
        /// </summary>
        public UxrUniversalLocalAxes LeftClavicleUniversalLocalAxes => _leftClavicleUniversalLocalAxes;

        /// <summary>
        ///     Gets the universal coordinate system for the right clavicle: forward is clavicle->arm, up is avatar up axis
        /// </summary>
        public UxrUniversalLocalAxes RightClavicleUniversalLocalAxes => _rightClavicleUniversalLocalAxes;

        // Updated every frame

        /// <summary>
        ///     Gets the left wrist torsion info.
        /// </summary>
        public UxrWristTorsionInfo LeftWristTorsionInfo { get; private set; } = new UxrWristTorsionInfo();

        /// <summary>
        ///     Gets the right wrist torsion info.
        /// </summary>
        public UxrWristTorsionInfo RightWristTorsionInfo { get; private set; } = new UxrWristTorsionInfo();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to find out which axes are pointing right/up/forward in the hand and finger nodes. These "universal" axes
        ///     will be used to rotate the nodes, so that any coordinate system can be used no matter how the hand was authored.
        /// </summary>
        /// <param name="hand">The hand to compute the axes for</param>
        /// <param name="handSide">Whether it is a left hand or right hand</param>
        /// <param name="handAxes">Universal axes for the hand</param>
        /// <param name="fingerAxes">Universal axes for the fingers</param>
        /// <returns>Boolean telling whether the axes could be solved. If any necessary transform is missing it will fail</returns>
        public static bool SolveHandAndFingerAxes(UxrAvatarHand hand, UxrHandSide handSide, out UxrUniversalLocalAxes handAxes, out UxrUniversalLocalAxes fingerAxes)
        {
            Transform indexProximal  = hand.Index.Proximal;
            Transform indexDistal    = hand.Index.Distal;
            Transform middleProximal = hand.Middle.Proximal;
            Transform ringProximal   = hand.Ring.Proximal;

            handAxes   = null;
            fingerAxes = null;

            if (!hand.Wrist || !indexProximal || !indexDistal || !middleProximal || !ringProximal)
            {
                return false;
            }

            float handCenter = 0.5f; // [0, 1]

            Vector3 handAxesRight   = hand.Wrist.InverseTransformDirection(indexProximal.position - middleProximal.position).GetClosestAxis() * (handSide == UxrHandSide.Left ? 1.0f : -1.0f);
            Vector3 handAxesForward = hand.Wrist.InverseTransformDirection((Vector3.Lerp(ringProximal.position, middleProximal.position, handCenter) - hand.Wrist.position).normalized);
            Vector3 handAxesUp      = Vector3.Cross(handAxesForward, handAxesRight).normalized;
            handAxesRight = Vector3.Cross(handAxesUp, handAxesForward).normalized;

            handAxes = UxrUniversalLocalAxes.FromAxes(hand.Wrist, handAxesRight, handAxesUp, handAxesForward);

            Vector3 fingerAxesRight   = indexProximal.InverseTransformDirection(indexProximal.position - middleProximal.position).GetClosestAxis() * (handSide == UxrHandSide.Left ? 1.0f : -1.0f);
            Vector3 fingerAxesForward = indexProximal.InverseTransformDirection(indexDistal.position - indexProximal.position).GetClosestAxis();
            Vector3 fingerAxesUp      = Vector3.Cross(fingerAxesForward, fingerAxesRight);

            fingerAxes = UxrUniversalLocalAxes.FromAxes(indexProximal, fingerAxesRight, fingerAxesUp, fingerAxesForward);

            return true;
        }

        /// <summary>
        ///     Gets a hand's universal local axes.
        /// </summary>
        /// <param name="side">Which side to get</param>
        /// <returns>Hand universal local axes</returns>
        /// <seealso cref="LeftHandUniversalLocalAxes" />
        /// <seealso cref="RightHandUniversalLocalAxes" />
        public UxrUniversalLocalAxes GetHandUniversalLocalAxes(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftHandUniversalLocalAxes : _rightHandUniversalLocalAxes;
        }

        /// <summary>
        ///     Gets the finger universal local axes.
        /// </summary>
        /// <param name="side">Which side to get</param>
        /// <returns>Finger universal local axes</returns>
        /// <seealso cref="LeftFingerUniversalLocalAxes" />
        /// <seealso cref="RightFingerUniversalLocalAxes" />
        public UxrUniversalLocalAxes GetFingerUniversalLocalAxes(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftFingerUniversalLocalAxes : _rightFingerUniversalLocalAxes;
        }

        /// <summary>
        ///     Gets an arm's universal local axes.
        /// </summary>
        /// <param name="side">Which side to get</param>
        /// <returns>Arm universal local axes</returns>
        /// <seealso cref="LeftArmUniversalLocalAxes" />
        /// <seealso cref="RightArmUniversalLocalAxes" />
        public UxrUniversalLocalAxes GetArmUniversalLocalAxes(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftArmUniversalLocalAxes : _rightArmUniversalLocalAxes;
        }

        /// <summary>
        ///     Gets a forearm's universal local axes.
        /// </summary>
        /// <param name="side">Which side to get</param>
        /// <returns>Forearm universal local axes</returns>
        /// <seealso cref="LeftForearmUniversalLocalAxes" />
        /// <seealso cref="RightForearmUniversalLocalAxes" />
        public UxrUniversalLocalAxes GetForearmUniversalLocalAxes(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftForearmUniversalLocalAxes : _rightForearmUniversalLocalAxes;
        }

        /// <summary>
        ///     Gets a clavicle's universal local axes.
        /// </summary>
        /// <param name="side">Which side to get</param>
        /// <returns>Clavicle's universal local axes</returns>
        /// <seealso cref="LeftClavicleUniversalLocalAxes" />
        /// <seealso cref="RightClavicleUniversalLocalAxes" />
        public UxrUniversalLocalAxes GetClavicleUniversalLocalAxes(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftClavicleUniversalLocalAxes : _rightClavicleUniversalLocalAxes;
        }

        /// <summary>
        ///     Computes the information of an avatar and a rig.
        /// </summary>
        /// <param name="avatar">Avatar to compute the information of</param>
        /// <param name="rig">Rig to compute the information of</param>
        public void ComputeFromAvatar(UxrAvatar avatar, UxrAvatarRig rig)
        {
            _avatar = avatar;

            SolveHandAndFingerAxes(rig.LeftArm.Hand,  UxrHandSide.Left,  out _leftHandUniversalLocalAxes,  out _leftFingerUniversalLocalAxes);
            SolveHandAndFingerAxes(rig.RightArm.Hand, UxrHandSide.Right, out _rightHandUniversalLocalAxes, out _rightFingerUniversalLocalAxes);

            ComputeArmRigInfo(avatar, rig.LeftArm,  ref _leftClavicleUniversalLocalAxes,  ref _leftArmUniversalLocalAxes,  ref _leftForearmUniversalLocalAxes,  ref _leftUpperArmLength,  ref _leftForearmLength);
            ComputeArmRigInfo(avatar, rig.RightArm, ref _rightClavicleUniversalLocalAxes, ref _rightArmUniversalLocalAxes, ref _rightForearmUniversalLocalAxes, ref _rightUpperArmLength, ref _rightForearmLength);
        }

        /// <summary>
        ///     Updates information for the current frame.
        /// </summary>
        public void UpdateInfo()
        {
            LeftWristTorsionInfo.UpdateInfo(_avatar.AvatarRig.LeftArm.Forearm, _avatar.LeftHandBone, LeftForearmUniversalLocalAxes, LeftHandUniversalLocalAxes);
            RightWristTorsionInfo.UpdateInfo(_avatar.AvatarRig.RightArm.Forearm, _avatar.RightHandBone, RightForearmUniversalLocalAxes, RightHandUniversalLocalAxes);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes information of the arm.
        /// </summary>
        /// <param name="avatar">Avatar whose right arm to compute information of</param>
        /// <param name="arm">Arm to compute the information of</param>
        /// <param name="clavicleLocalAxes">Will return the universal coordinate system for the clavicle</param>
        /// <param name="armLocalAxes">Will return the universal coordinate system for the arm</param>
        /// <param name="forearmLocalAxes">Will return the universal coordinate system for the forearm</param>
        /// <param name="upperArmLength">Will return the upper arm length</param>
        /// <param name="forearmLength">Will return the forearm length</param>
        private static void ComputeArmRigInfo(UxrAvatar                 avatar,
                                              UxrAvatarArm              arm,
                                              ref UxrUniversalLocalAxes clavicleLocalAxes,
                                              ref UxrUniversalLocalAxes armLocalAxes,
                                              ref UxrUniversalLocalAxes forearmLocalAxes,
                                              ref float                 upperArmLength,
                                              ref float                 forearmLength)
        {
            if (arm.UpperArm != null && arm.Forearm != null && arm.Hand.Wrist != null)
            {
                Vector3 armForward            = (arm.Forearm.position - arm.UpperArm.position).normalized;
                Vector3 forearmForward        = (arm.Hand.Wrist.position - arm.Forearm.position).normalized;
                Vector3 elbowAxis             = GetWorldElbowAxis(avatar, arm.Forearm, armForward, forearmForward);
                Vector3 armLocalForward       = arm.UpperArm.InverseTransformDirection(armForward);
                Vector3 armLocalElbowAxis     = arm.UpperArm.InverseTransformDirection(elbowAxis);
                Vector3 forearmLocalForward   = arm.Forearm.InverseTransformDirection(forearmForward);
                Vector3 forearmLocalElbowAxis = arm.Forearm.InverseTransformDirection(elbowAxis);

                armLocalAxes     = UxrUniversalLocalAxes.FromUpForward(arm.UpperArm, armLocalElbowAxis.GetClosestAxis(), armLocalForward);
                forearmLocalAxes = UxrUniversalLocalAxes.FromUpForward(arm.Forearm,  armLocalElbowAxis.GetClosestAxis(), forearmLocalForward);
                upperArmLength   = Vector3.Distance(arm.UpperArm.position, arm.Forearm.position);
                forearmLength    = Vector3.Distance(arm.Forearm.position,  arm.Hand.Wrist.position);

                if (arm.Clavicle != null)
                {
                    Vector3 clavicleForward          = (arm.UpperArm.position - arm.Clavicle.position).normalized;
                    Vector3 clavicleLocalForwardAxis = arm.Clavicle.InverseTransformDirection(clavicleForward).GetClosestAxis();
                    Vector3 clavicleLocalUpAxis      = arm.Clavicle.InverseTransformDirection(avatar.transform.up).GetClosestAxis();

                    clavicleLocalAxes = UxrUniversalLocalAxes.FromUpForward(arm.Clavicle, clavicleLocalUpAxis, clavicleLocalForwardAxis);
                }
            }
        }

        /// <summary>
        ///     Gets the outwards-pointing elbow rotation axis in world coordinates.
        /// </summary>
        /// <param name="avatar">The avatar the arm nodes belong to</param>
        /// <param name="forearm">The arm's forearm transform</param>
        /// <param name="armForward">The arm forward looking vector, the one pointing from shoulder to elbow</param>
        /// <param name="forearmForward">The forearm forward looking vector, the one pointing from elbow to hand</param>
        /// <returns>Elbow rotation axis</returns>
        private static Vector3 GetWorldElbowAxis(UxrAvatar avatar, Transform forearm, Vector3 armForward, Vector3 forearmForward)
        {
            bool isLeft = avatar.transform.InverseTransformPoint(forearm.position).x < 0.0f;

            float   elbowAngle = Vector3.Angle(armForward, forearmForward);
            Vector3 elbowAxis  = Vector3.Cross(forearmForward, armForward).normalized;

            if (elbowAngle > ElbowMinAngleThreshold)
            {
                elbowAxis = Vector3.up; // Assume T-pose if elbow angle is too small
            }
            else
            {
                elbowAxis = isLeft ? -elbowAxis : elbowAxis;
            }

            return forearm.TransformDirection(forearm.InverseTransformDirection(elbowAxis).GetClosestAxis());
        }

        #endregion

        #region Private Types & Data

        private const float ElbowMinAngleThreshold = 3.0f; // Minimum angle between arm and forearm to compute elbow axis using cross product

        #endregion
    }
}