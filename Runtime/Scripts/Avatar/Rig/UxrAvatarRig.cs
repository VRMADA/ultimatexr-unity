// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRig.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     <para>
    ///         Stores references to all elements in an avatar rig. These are the <see cref="Transform" /> components of the
    ///         bones that drive the visual representation of a humanoid avatar.
    ///     </para>
    ///     It also contains functionality to transform the hand using hand poses.
    /// </summary>
    [Serializable]
    public partial class UxrAvatarRig
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAvatarHead _head;
        [SerializeField] private UxrAvatarArm  _leftArm;
        [SerializeField] private UxrAvatarArm  _rightArm;
        [SerializeField] private Transform     _upperChest;
        [SerializeField] private Transform     _chest;
        [SerializeField] private Transform     _spine;
        [SerializeField] private Transform     _hips;
        [SerializeField] private UxrAvatarLeg  _leftLeg;
        [SerializeField] private UxrAvatarLeg  _rightLeg;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Number of fingers in a hand.
        /// </summary>
        public const int HandFingerCount = 5;

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the avatar rig.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                foreach (Transform transform in Head.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in LeftArm.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in RightArm.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in LeftLeg.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in RightLeg.Transforms)
                {
                    yield return transform;
                }

                if (UpperChest != null)
                {
                    yield return UpperChest;
                }

                if (Chest != null)
                {
                    yield return Chest;
                }

                if (Spine != null)
                {
                    yield return Spine;
                }

                if (Hips != null)
                {
                    yield return Hips;
                }
            }
        }

        /// <summary>
        ///     Gets the head.
        /// </summary>
        public UxrAvatarHead Head => _head;

        /// <summary>
        ///     Gets the left arm.
        /// </summary>
        public UxrAvatarArm LeftArm => _leftArm;

        /// <summary>
        ///     Gets the right arm.
        /// </summary>
        public UxrAvatarArm RightArm => _rightArm;

        /// <summary>
        ///     Gets the upper chest transform or null if there isn't any.
        /// </summary>
        public Transform UpperChest => _upperChest;

        /// <summary>
        ///     Gets the chest transform or null if there isn't any.
        /// </summary>
        public Transform Chest => _chest;

        /// <summary>
        ///     Gets the spine transform or null if there isn't any.
        /// </summary>
        public Transform Spine => _spine;

        /// <summary>
        ///     Gets the hips transform or null if there isn't any.
        /// </summary>
        public Transform Hips => _hips;

        /// <summary>
        ///     Gets the left leg.
        /// </summary>
        public UxrAvatarLeg LeftLeg => _leftLeg;

        /// <summary>
        ///     Gets the right leg.
        /// </summary>
        public UxrAvatarLeg RightLeg => _rightLeg;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        public UxrAvatarRig()
        {
            ClearRigElements();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks which side a transform is part of, based on which wrist it hangs from or if it hangs from an
        ///     <see cref="UxrHandIntegration" />.
        /// </summary>
        /// <param name="transform">Transform to check which side it is part of</param>
        /// <param name="side">Returns the side, if found</param>
        /// <returns>Whether a side was found</returns>
        public static bool GetHandSide(Transform transform, out UxrHandSide side)
        {
            UxrAvatar avatar = transform.SafeGetComponentInParent<UxrAvatar>();

            if (avatar)
            {
                if (transform.HasParent(avatar.LeftHandBone))
                {
                    side = UxrHandSide.Left;
                    return true;
                }
                if (transform.HasParent(avatar.RightHandBone))
                {
                    side = UxrHandSide.Right;
                    return true;
                }
                UxrHandIntegration handIntegration = transform.SafeGetComponentInParent<UxrHandIntegration>();

                if (handIntegration)
                {
                    side = handIntegration.HandSide;
                    return true;
                }
            }

            side = UxrHandSide.Left;
            return false;
        }

        /// <summary>
        ///     Sets all the rig element references to null.
        /// </summary>
        public void ClearRigElements()
        {
            _head       = new UxrAvatarHead();
            _leftArm    = new UxrAvatarArm();
            _rightArm   = new UxrAvatarArm();
            _leftLeg    = new UxrAvatarLeg();
            _rightLeg   = new UxrAvatarLeg();
            _upperChest = null;
            _chest      = null;
            _spine      = null;
            _hips       = null;
        }

        /// <summary>
        ///     Gets the avatar arms.
        /// </summary>
        /// <returns>Avatar arms</returns>
        public IEnumerable<UxrAvatarArm> GetArms()
        {
            if (_leftArm != null)
            {
                yield return _leftArm;
            }

            if (_rightArm != null)
            {
                yield return _rightArm;
            }
        }

        /// <summary>
        ///     Gets whether the given rig has any of the references used in upper body IK (head, neck, upper chest, chest or
        ///     spine).
        /// </summary>
        /// <returns>Whether the given rig has any upper body reference used in IK</returns>
        public bool HasAnyUpperBodyIKReference()
        {
            return _head.Head != null || _head.Neck != null || _upperChest != null || _chest != null || _spine != null;
        }

        /// <summary>
        ///     Gets whether the given rig has all arm references (upper arm, forearm and hand).
        /// </summary>
        /// <returns>Whether the given rig has arm references for both sides</returns>
        public bool HasArmData()
        {
            return LeftArm.Hand.Wrist != null && RightArm.Hand.Wrist != null && LeftArm.Forearm != null && RightArm.Forearm != null && LeftArm.UpperArm != null && RightArm.UpperArm != null;
        }

        /// <summary>
        ///     Gets whether the given rig has all hand and finger bone references.
        /// </summary>
        /// <returns>Whether the given rig has all the references</returns>
        public bool HasFullHandData()
        {
            return LeftArm.Hand.Wrist != null && RightArm.Hand.Wrist != null && LeftArm.Hand.HasFingerData() && RightArm.Hand.HasFingerData();
        }

        /// <summary>
        ///     Gets whether the given rig has all finger data.
        /// </summary>
        /// <returns>Whether the given rig has all finger data of both hands</returns>
        public bool HasFingerData()
        {
            return LeftArm.Hand.HasFingerData() && RightArm.Hand.HasFingerData();
        }

        /// <summary>
        ///     Gets whether the given rig has all finger data.
        /// </summary>
        /// <param name="handSide">Which hand to check</param>
        /// <returns>Whether the given rig has the given hand finger data</returns>
        public bool HasFingerData(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? LeftArm.Hand.HasFingerData() : RightArm.Hand.HasFingerData();
        }

        #endregion
    }
}