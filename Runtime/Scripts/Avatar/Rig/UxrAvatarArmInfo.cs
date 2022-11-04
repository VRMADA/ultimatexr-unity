// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarArmInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores information of an avatar rig's arm.
    /// </summary>
    [Serializable]
    public class UxrAvatarArmInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAvatar             _avatar;
        [SerializeField] private UxrHandSide           _side;
        [SerializeField] private float                 _upperArmLength;
        [SerializeField] private float                 _forearmLength;
        [SerializeField] private UxrUniversalLocalAxes _fingerUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _handUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _armUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _forearmUniversalLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _clavicleUniversalLocalAxes;
        [SerializeField] private UxrAvatarFingerInfo   _thumbInfo;
        [SerializeField] private UxrAvatarFingerInfo   _indexInfo;
        [SerializeField] private UxrAvatarFingerInfo   _middleInfo;
        [SerializeField] private UxrAvatarFingerInfo   _ringInfo;
        [SerializeField] private UxrAvatarFingerInfo   _littleInfo;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the thumb finger information.
        /// </summary>
        public UxrAvatarFingerInfo ThumbInfo => _thumbInfo;

        /// <summary>
        ///     Gets the index finger information.
        /// </summary>
        public UxrAvatarFingerInfo IndexInfo => _indexInfo;

        /// <summary>
        ///     Gets the middle finger information.
        /// </summary>
        public UxrAvatarFingerInfo MiddleInfo => _middleInfo;

        /// <summary>
        ///     Gets the ring finger information.
        /// </summary>
        public UxrAvatarFingerInfo RingInfo => _ringInfo;

        /// <summary>
        ///     Gets the little finger information.
        /// </summary>
        public UxrAvatarFingerInfo LittleInfo => _littleInfo;

        /// <summary>
        ///     Enumerates all the finger information.
        /// </summary>
        public IEnumerable<UxrAvatarFingerInfo> Fingers
        {
            get
            {
                yield return _thumbInfo;
                yield return _indexInfo;
                yield return _middleInfo;
                yield return _ringInfo;
                yield return _littleInfo;
            }
        }

        /// <summary>
        ///     Gets the upper arm length. From shoulder to elbow.
        /// </summary>
        public float UpperArmLength
        {
            get => _upperArmLength;
            private set => _upperArmLength = value;
        }

        /// <summary>
        ///     Gets the forearm length. From elbow to wrist.
        /// </summary>
        public float ForearmLength
        {
            get => _forearmLength;
            private set => _forearmLength = value;
        }

        /// <summary>
        ///     Gets the universal coordinate system for the fingers: right = axis around which the finger curls, up =
        ///     knuckles up, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes FingerUniversalLocalAxes
        {
            get => _fingerUniversalLocalAxes;
            private set => _fingerUniversalLocalAxes = value;
        }

        /// <summary>
        ///     Gets the universal coordinate system for the hand: right = from wrist to right (thumb direction), up = -palm
        ///     facing vector, forward = finger direction.
        /// </summary>
        public UxrUniversalLocalAxes HandUniversalLocalAxes
        {
            get => _handUniversalLocalAxes;
            private set => _handUniversalLocalAxes = value;
        }

        /// <summary>
        ///     Gets the universal coordinate system for the arm: forward is arm->elbow, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes ArmUniversalLocalAxes
        {
            get => _armUniversalLocalAxes;
            private set => _armUniversalLocalAxes = value;
        }

        /// <summary>
        ///     Gets the universal coordinate system for the forearm: forward is arm->hand, up is elbow rotation axis
        /// </summary>
        public UxrUniversalLocalAxes ForearmUniversalLocalAxes
        {
            get => _forearmUniversalLocalAxes;
            private set => _forearmUniversalLocalAxes = value;
        }

        /// <summary>
        ///     Gets the universal coordinate system for the clavicle: forward is clavicle->arm, up is avatar up axis
        /// </summary>
        public UxrUniversalLocalAxes ClavicleUniversalLocalAxes
        {
            get => _clavicleUniversalLocalAxes;
            private set => _clavicleUniversalLocalAxes = value;
        }

        // Updated every frame

        /// <summary>
        ///     Gets the wrist torsion info.
        /// </summary>
        public UxrWristTorsionInfo WristTorsionInfo { get; private set; } = new UxrWristTorsionInfo();

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Computes and stores all the arm information of an avatar.
        /// </summary>
        /// <param name="avatar">Avatar whose arm to compute the information of</param>
        /// <param name="side">Which side to compute</param>
        internal void Compute(UxrAvatar avatar, UxrHandSide side)
        {
            _avatar = avatar;
            _side   = side;
            SolveHandAndFingerAxes(avatar.GetHand(side), side);
            ComputeArmRigInfo(avatar, avatar.GetArm(side), side);
        }

        #endregion

        #region Private Methods

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

            if (elbowAngle < ElbowMinAngleThreshold)
            {
                elbowAxis = Vector3.up; // Assume T-pose if elbow angle is too small
            }
            else
            {
                elbowAxis = isLeft ? -elbowAxis : elbowAxis;
            }

            return forearm.TransformDirection(forearm.InverseTransformDirection(elbowAxis).GetClosestAxis());
        }

        /// <summary>
        ///     Tries to find out which axes are pointing right/up/forward in the hand and finger nodes. These "universal" axes
        ///     will be used to rotate the nodes, so that any coordinate system can be used no matter how the hand was authored.
        /// </summary>
        /// <param name="hand">The hand to compute the axes for</param>
        /// <param name="side">Whether it is a left hand or right hand</param>
        /// <returns>Boolean telling whether the axes could be solved. If any necessary transform is missing it will fail</returns>
        private bool SolveHandAndFingerAxes(UxrAvatarHand hand, UxrHandSide side)
        {
            Transform indexProximal  = hand.Index.Proximal;
            Transform indexDistal    = hand.Index.Distal;
            Transform middleProximal = hand.Middle.Proximal;
            Transform ringProximal   = hand.Ring.Proximal;

            if (!hand.Wrist || !indexProximal || !indexDistal || !middleProximal || !ringProximal)
            {
                return false;
            }

            float handCenter = 0.5f; // [0, 1]

            Vector3 handAxesRight   = hand.Wrist.InverseTransformDirection(indexProximal.position - middleProximal.position).GetClosestAxis() * (side == UxrHandSide.Left ? 1.0f : -1.0f);
            Vector3 handAxesForward = hand.Wrist.InverseTransformDirection((Vector3.Lerp(ringProximal.position, middleProximal.position, handCenter) - hand.Wrist.position).normalized);
            Vector3 handAxesUp      = Vector3.Cross(handAxesForward, handAxesRight).normalized;
            handAxesRight = Vector3.Cross(handAxesUp, handAxesForward).normalized;

            HandUniversalLocalAxes = UxrUniversalLocalAxes.FromAxes(hand.Wrist, handAxesRight, handAxesUp, handAxesForward);

            Vector3 fingerAxesRight   = indexProximal.InverseTransformDirection(indexProximal.position - middleProximal.position).GetClosestAxis() * (side == UxrHandSide.Left ? 1.0f : -1.0f);
            Vector3 fingerAxesForward = indexProximal.InverseTransformDirection(indexDistal.position - indexProximal.position).GetClosestAxis();
            Vector3 fingerAxesUp      = Vector3.Cross(fingerAxesForward, fingerAxesRight);

            FingerUniversalLocalAxes = UxrUniversalLocalAxes.FromAxes(indexProximal, fingerAxesRight, fingerAxesUp, fingerAxesForward);

            return true;
        }

        /// <summary>
        ///     Computes and stores information of the arm's rig.
        /// </summary>
        /// <param name="avatar">Avatar whose right arm to compute information of</param>
        /// <param name="arm">Arm to compute the information of</param>
        /// <param name="side">Which side it is</param>
        private void ComputeArmRigInfo(UxrAvatar avatar, UxrAvatarArm arm, UxrHandSide side)
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

                ArmUniversalLocalAxes     = UxrUniversalLocalAxes.FromUpForward(arm.UpperArm, armLocalElbowAxis.GetClosestAxis(), armLocalForward);
                ForearmUniversalLocalAxes = UxrUniversalLocalAxes.FromUpForward(arm.Forearm,  armLocalElbowAxis.GetClosestAxis(), forearmLocalForward);
                UpperArmLength            = Vector3.Distance(arm.UpperArm.position, arm.Forearm.position);
                ForearmLength             = Vector3.Distance(arm.Forearm.position,  arm.Hand.Wrist.position);

                if (arm.Clavicle != null)
                {
                    Vector3 clavicleForward          = (arm.UpperArm.position - arm.Clavicle.position).normalized;
                    Vector3 clavicleLocalForwardAxis = arm.Clavicle.InverseTransformDirection(clavicleForward).GetClosestAxis();
                    Vector3 clavicleLocalUpAxis      = arm.Clavicle.InverseTransformDirection(avatar.transform.up).GetClosestAxis();

                    ClavicleUniversalLocalAxes = UxrUniversalLocalAxes.FromUpForward(arm.Clavicle, clavicleLocalUpAxis, clavicleLocalForwardAxis);
                }
            }

            UxrGrabber grabber = avatar.GetGrabber(side);

            if (grabber && grabber.HandRenderer && grabber.HandRenderer is SkinnedMeshRenderer handRenderer)
            {
                _thumbInfo  = new UxrAvatarFingerInfo();
                _indexInfo  = new UxrAvatarFingerInfo();
                _middleInfo = new UxrAvatarFingerInfo();
                _ringInfo   = new UxrAvatarFingerInfo();
                _littleInfo = new UxrAvatarFingerInfo();

                _thumbInfo.Compute(avatar, handRenderer, side, UxrFingerType.Thumb);
                _indexInfo.Compute(avatar, handRenderer, side, UxrFingerType.Index);
                _middleInfo.Compute(avatar, handRenderer, side, UxrFingerType.Middle);
                _ringInfo.Compute(avatar, handRenderer, side, UxrFingerType.Ring);
                _littleInfo.Compute(avatar, handRenderer, side, UxrFingerType.Little);
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Minimum angle between arm and forearm to compute elbow axis using cross product.
        /// </summary>
        private const float ElbowMinAngleThreshold = 3.0f;

        #endregion
    }
}