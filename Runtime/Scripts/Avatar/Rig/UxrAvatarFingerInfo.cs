// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarFingerInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores information of an avatar rig's finger.
    /// </summary>
    [Serializable]
    public class UxrAvatarFingerInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAvatar               _avatar;
        [SerializeField] private UxrHandSide             _side;
        [SerializeField] private UxrFingerType           _finger;
        [SerializeField] private UxrAvatarFingerBoneInfo _metacarpalInfo;
        [SerializeField] private UxrAvatarFingerBoneInfo _proximalInfo;
        [SerializeField] private UxrAvatarFingerBoneInfo _intermediateInfo;
        [SerializeField] private UxrAvatarFingerBoneInfo _distalInfo;
        [SerializeField] private Vector3                 _distalLocalTip;
        [SerializeField] private Vector3                 _distalLocalFingerPrintCenter;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the metacarpal bone info.
        /// </summary>
        public UxrAvatarFingerBoneInfo MetacarpalInfo => _metacarpalInfo;

        /// <summary>
        ///     Gets the proximal bone info.
        /// </summary>
        public UxrAvatarFingerBoneInfo ProximalInfo => _proximalInfo;

        /// <summary>
        ///     Gets the intermediate bone info.
        /// </summary>
        public UxrAvatarFingerBoneInfo IntermediateInfo => _intermediateInfo;

        /// <summary>
        ///     Gets the distal bone info.
        /// </summary>
        public UxrAvatarFingerBoneInfo DistalInfo => _distalInfo;

        /// <summary>
        ///     Gets the finger tip in local coordinates of the distal bone.
        /// </summary>
        public Vector3 DistalLocalTip => _distalLocalTip;

        /// <summary>
        ///     Gets an approximate position of the finger print center in local coordinates of the distal bone. The position is
        ///     computed as a position at 2/3 of the distance between the distal bone start and the tip and at the bottom part of
        ///     the distal using the distal radius.
        /// </summary>
        public Vector3 DistalLocalFingerPrintCenter => _distalLocalFingerPrintCenter;

        /// <summary>
        ///     Gets the tip position in world-space.
        /// </summary>
        public Vector3 TipPosition
        {
            get
            {
                if (_avatar)
                {
                    UxrAvatarFinger avatarFinger = _avatar.GetHand(_side).GetFinger(_finger);

                    if (avatarFinger.Distal)
                    {
                        return avatarFinger.Distal.TransformPoint(_distalLocalTip);
                    }
                }

                return Vector3.zero;
            }
        }

        /// <summary>
        ///     Gets the tip forward direction in world-space.
        /// </summary>
        public Vector3 TipDirection
        {
            get
            {
                if (_avatar)
                {
                    UxrAvatarFinger avatarFinger = _avatar.GetHand(_side).GetFinger(_finger);

                    if (avatarFinger.Distal)
                    {
                        UxrUniversalLocalAxes fingerAxes = _avatar.AvatarRigInfo.GetArmInfo(_side).FingerUniversalLocalAxes;
                        return avatarFinger.Distal.TransformVector(fingerAxes.LocalForward);
                    }
                }

                return Vector3.zero;
            }
        }

        /// <summary>
        ///     Gets the finger print approximate position. The position is computed as a position at 2/3 of the distance between
        ///     the distal bone start and the tip and at the bottom part of the distal using the distal radius.
        /// </summary>
        public Vector3 FingerPrintPosition
        {
            get
            {
                if (_avatar)
                {
                    UxrAvatarFinger avatarFinger = _avatar.GetHand(_side).GetFinger(_finger);

                    if (avatarFinger.Distal)
                    {
                        return avatarFinger.Distal.TransformPoint(_distalLocalFingerPrintCenter);
                    }
                }

                return Vector3.zero;
            }
        }

        /// <summary>
        ///     Gets the finger print direction in world-space. The direction points from the finger print center downwards.
        /// </summary>
        public Vector3 FingerPrintDirection
        {
            get
            {
                if (_avatar)
                {
                    UxrAvatarFinger avatarFinger = _avatar.GetHand(_side).GetFinger(_finger);

                    if (avatarFinger.Distal)
                    {
                        UxrUniversalLocalAxes fingerAxes = _avatar.AvatarRigInfo.GetArmInfo(_side).FingerUniversalLocalAxes;
                        return -avatarFinger.Distal.TransformVector(fingerAxes.LocalUp);
                    }
                }

                return Vector3.zero;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Computes the finger information.
        /// </summary>
        /// <param name="avatar">Avatar whose finger information to compute</param>
        /// <param name="handRenderer">Hand renderer</param>
        /// <param name="side">Which hand side the finger belongs to</param>
        /// <param name="finger">Which finger to compute</param>
        internal void Compute(UxrAvatar avatar, SkinnedMeshRenderer handRenderer, UxrHandSide side, UxrFingerType finger)
        {
            _avatar = avatar;
            _side   = side;
            _finger = finger;

            UxrUniversalLocalAxes fingerAxes   = avatar.AvatarRigInfo.GetArmInfo(side).FingerUniversalLocalAxes;
            UxrAvatarFinger       avatarFinger = avatar.GetHand(side).GetFinger(finger);

            _metacarpalInfo   = new UxrAvatarFingerBoneInfo();
            _proximalInfo     = new UxrAvatarFingerBoneInfo();
            _intermediateInfo = new UxrAvatarFingerBoneInfo();
            _distalInfo       = new UxrAvatarFingerBoneInfo();

            _metacarpalInfo.Compute(handRenderer, avatarFinger.Metacarpal, avatarFinger.Proximal, fingerAxes);
            _proximalInfo.Compute(handRenderer, avatarFinger.Proximal, avatarFinger.Intermediate, fingerAxes);
            _intermediateInfo.Compute(handRenderer, avatarFinger.Intermediate, avatarFinger.Distal, fingerAxes);
            _distalInfo.Compute(handRenderer, avatarFinger.Distal, null, fingerAxes);

            _distalLocalTip               = fingerAxes.LocalForward * _distalInfo.Length;
            _distalLocalFingerPrintCenter = fingerAxes.LocalForward * (_distalInfo.Length * 0.66f) - fingerAxes.LocalUp * (_distalInfo.Radius * 0.66f);
        }

        #endregion
    }
}