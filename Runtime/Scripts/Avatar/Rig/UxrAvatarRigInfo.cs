// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRigInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Math;
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

        [SerializeField] private int              _version;
        [SerializeField] private UxrAvatar        _avatar;
        [SerializeField] private UxrAvatarArmInfo _leftArmInfo  = new UxrAvatarArmInfo();
        [SerializeField] private UxrAvatarArmInfo _rightArmInfo = new UxrAvatarArmInfo();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Enumerates the arm information.
        /// </summary>
        public IEnumerable<UxrAvatarArmInfo> Arms
        {
            get
            {
                yield return _leftArmInfo;
                yield return _rightArmInfo;
            }
        }

        #endregion

        #region Internal Types & Data

        internal const int CurrentVersion = 1;

        /// <summary>
        ///     Gets the version this data was serialized for. It allows to control if new data needs to be computed.
        /// </summary>
        internal int SerializedVersion => _version;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the arm information.
        /// </summary>
        /// <param name="side">Which side to retrieve</param>
        /// <returns>Arm information</returns>
        public UxrAvatarArmInfo GetArmInfo(UxrHandSide side)
        {
            return side == UxrHandSide.Left ? _leftArmInfo : _rightArmInfo;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Computes the information of an avatar's rig.
        /// </summary>
        /// <param name="avatar">Avatar whose rig to compute the information of</param>
        internal void Compute(UxrAvatar avatar)
        {
            _version = CurrentVersion;
            _avatar  = avatar;

            _leftArmInfo.Compute(avatar, UxrHandSide.Left);
            _rightArmInfo.Compute(avatar, UxrHandSide.Right);
        }

        /// <summary>
        ///     Updates information to the current frame.
        /// </summary>
        internal void UpdateInfo()
        {
            GetArmInfo(UxrHandSide.Left).WristTorsionInfo.UpdateInfo(_avatar.AvatarRig.LeftArm.Forearm, _avatar.LeftHandBone, GetArmInfo(UxrHandSide.Left));
            GetArmInfo(UxrHandSide.Right).WristTorsionInfo.UpdateInfo(_avatar.AvatarRig.RightArm.Forearm, _avatar.RightHandBone, GetArmInfo(UxrHandSide.Right));
        }

        #endregion
    }
}