// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWristTorsionInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores information about wrist torsion. Updated by <see cref="UxrAvatarRig" />.
    /// </summary>
    public class UxrWristTorsionInfo
    {
        #region Public Types & Data

        /// <summary>
        ///     The wrist rotation along the elbow->hand axis.
        /// </summary>
        public float WristTorsionAngle { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        public UxrWristTorsionInfo()
        {
            _wristTorsionMinAngle = WristTorsionLimitSideLong;
            _wristTorsionMaxAngle = -WristTorsionLimitSideShort;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Updates the wrist torsion information to the current frame.
        /// </summary>
        /// <param name="forearm">Forearm bone</param>
        /// <param name="hand">Hand bone</param>
        /// <param name="forearmUniversalLocalAxes">Forearm universal axes</param>
        /// <param name="handUniversalLocalAxes">Hand universal axes</param>
        public void UpdateInfo(Transform forearm, Transform hand, UxrUniversalLocalAxes forearmUniversalLocalAxes, UxrUniversalLocalAxes handUniversalLocalAxes)
        {
            if (hand && forearm && forearmUniversalLocalAxes != null && handUniversalLocalAxes != null)
            {
                Vector3 currentHandForwardInForearm = forearm.InverseTransformDirection(handUniversalLocalAxes.WorldForward);
                Vector3 currentHandUpInForearm      = forearm.InverseTransformDirection(handUniversalLocalAxes.WorldUp);
                Vector3 currentHandRightInForearm   = forearm.InverseTransformDirection(handUniversalLocalAxes.WorldRight);

                currentHandUpInForearm    = Vector3.ProjectOnPlane(currentHandUpInForearm,    forearmUniversalLocalAxes.LocalForward);
                currentHandRightInForearm = Vector3.ProjectOnPlane(currentHandRightInForearm, forearmUniversalLocalAxes.LocalForward);

                float angleRight = Vector3.SignedAngle(forearmUniversalLocalAxes.LocalRight, currentHandRightInForearm, forearmUniversalLocalAxes.LocalForward);
                float angle      = angleRight; // Works better than angleUp because of hand movement constraints

                // Check for twists greater than 180 degrees, to know which way to turn to

                if (WristTorsionAngle > WristTorsionOvershootAngleThreshold && angle < -WristTorsionOvershootAngleThreshold)
                {
                    _wristTorsionOvershoot++;
                }
                else if (WristTorsionAngle < -WristTorsionOvershootAngleThreshold && angle > WristTorsionOvershootAngleThreshold)
                {
                    _wristTorsionOvershoot--;
                }

                // Compute the overshoot if necessary

                float finalAngle = angle;

                if (_wristTorsionOvershoot > 0)
                {
                    finalAngle = 180.0f + 360.0f * (_wristTorsionOvershoot - 1) + (angle + 180.0f);
                }
                else if (_wristTorsionOvershoot < 0)
                {
                    finalAngle = -180.0f - -360.0f * (_wristTorsionOvershoot + 1) - (180.0f - angle);
                }

                if (finalAngle > _wristTorsionMinAngle && _wristTorsionOvershoot != 0)
                {
                    _wristTorsionOvershoot = 0;
                    finalAngle             = finalAngle + 360.0f;
                }
                else if (finalAngle < _wristTorsionMaxAngle && _wristTorsionOvershoot != 0)
                {
                    _wristTorsionOvershoot = 0;
                    finalAngle             = finalAngle - 360.0f;
                }

                // Rotation

                WristTorsionAngle = angleRight;
            }
        }

        #endregion

        #region Private Types & Data

        // Constants

        private const float WristTorsionOvershootAngleThreshold = 150.0f;
        private const float WristTorsionLimitSideShort          = 200.0f;
        private const float WristTorsionLimitSideLong           = 300.0f;

        // Internal vars

        private readonly float _wristTorsionMinAngle;
        private readonly float _wristTorsionMaxAngle;
        private          int   _wristTorsionOvershoot;

        #endregion
    }
}