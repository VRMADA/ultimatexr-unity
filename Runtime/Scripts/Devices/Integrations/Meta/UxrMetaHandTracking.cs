// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMetaHandTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
#if ULTIMATEXR_USE_OCULUS_SDK
using UltimateXR.Avatar.Rig;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;
#endif

namespace UltimateXR.Devices.Integrations.Meta
{
    /// <summary>
    ///     Hand tracking for Meta devices.
    /// </summary>
    public partial class UxrMetaHandTracking : UxrHandTracking
    {
        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override string SDKDependency => UxrManager.SdkOculus;

        #endregion

        #region Public Overrides UxrHandTracking

        /// <inheritdoc />
        public override bool IsLeftHandAvailable
        {
            get
            {
#if ULTIMATEXR_USE_OCULUS_SDK
                if (OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft, ref _leftHandState))
                {
                    _isLeftHandAvailable = _leftHandState.Status.HasFlag(OVRPlugin.HandStatus.HandTracked);
                }
                else
                {
                    _isLeftHandAvailable = false;
                }
                return _isLeftHandAvailable;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override bool IsRightHandAvailable
        {
            get
            {
#if ULTIMATEXR_USE_OCULUS_SDK
                if (OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref _rightHandState))
                {
                    _isRightHandAvailable = _rightHandState.Status.HasFlag(OVRPlugin.HandStatus.HandTracked);
                }
                else
                {
                    _isRightHandAvailable = false;
                }
                return _isRightHandAvailable;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events so that the component can be enabled or disabled based on the presence of hand tracking.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if ULTIMATEXR_USE_OCULUS_SDK
            // Initialize axis system

            if (Avatar != null && Avatar.LeftHand.HasFullHandData())
            {
                _leftHandOculusRotation   = Quaternion.LookRotation(Vector3.right,  -Vector3.up);
                _leftFingerOculusRotation = Quaternion.LookRotation(-Vector3.right, -Vector3.up);
            }

            if (Avatar != null && Avatar.RightHand.HasFullHandData())
            {
                _rightHandOculusRotation   = Quaternion.LookRotation(Vector3.right, Vector3.up);
                _rightFingerOculusRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            }
#endif
        }

        #endregion

        #region Protected Overrides UxrTrackingDevice

        /// <inheritdoc />
        protected override void UpdateSensors()
        {
#if ULTIMATEXR_USE_OCULUS_SDK
            _isLeftHandAvailable  = OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft,  ref _leftHandState);
            _isRightHandAvailable = OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref _rightHandState);
#endif
        }

        /// <inheritdoc />
        protected override void UpdateAvatar()
        {
#if ULTIMATEXR_USE_OCULUS_SDK

            Transform wristLeft  = Avatar.LeftHandBone;
            Transform wristRight = Avatar.RightHandBone;

            if (_isLeftHandAvailable && wristLeft != null)
            {
                if (UseCalibration)
                {
                    SetCalibrationPose(UxrHandSide.Left);
                }

                UxrAvatarArmInfo      leftArmInfo        = Avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Left);
                UxrUniversalLocalAxes leftHandParentAxes = wristLeft.parent == Avatar.AvatarRig.LeftArm.Forearm ? leftArmInfo.ForearmUniversalLocalAxes : leftArmInfo.HandUniversalLocalAxes;

                Vector3    sensorLeftPos = Avatar.transform.TransformPoint(_leftHandState.RootPose.Position.FromFlippedZVector3f());
                Quaternion sensorLeftRot = Avatar.transform.rotation * ToCorrectCoordinateSystem(_leftHandState.RootPose.Orientation, FlipMode.FlipZ, _leftHandOculusRotation, leftHandParentAxes, leftArmInfo.HandUniversalLocalAxes);

                wristLeft.position = sensorLeftPos;
                wristLeft.rotation = sensorLeftRot;

                UpdateFinger(UxrHandSide.Left, Avatar.LeftHand.Index,  OVRPlugin.BoneId.Hand_Index1,  3, _leftFingerOculusRotation, leftArmInfo.HandUniversalLocalAxes, leftArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Left, Avatar.LeftHand.Middle, OVRPlugin.BoneId.Hand_Middle1, 3, _leftFingerOculusRotation, leftArmInfo.HandUniversalLocalAxes, leftArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Left, Avatar.LeftHand.Ring,   OVRPlugin.BoneId.Hand_Ring1,   3, _leftFingerOculusRotation, leftArmInfo.HandUniversalLocalAxes, leftArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Left, Avatar.LeftHand.Little, OVRPlugin.BoneId.Hand_Pinky0,  4, _leftFingerOculusRotation, leftArmInfo.HandUniversalLocalAxes, leftArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Left, Avatar.LeftHand.Thumb,  OVRPlugin.BoneId.Hand_Thumb0,  4, _leftFingerOculusRotation, leftArmInfo.HandUniversalLocalAxes, leftArmInfo.FingerUniversalLocalAxes);
            }

            if (_isRightHandAvailable && wristRight != null)
            {
                if (UseCalibration)
                {
                    SetCalibrationPose(UxrHandSide.Right);
                }

                UxrAvatarArmInfo      rightArmInfo        = Avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Right);
                UxrUniversalLocalAxes rightHandParentAxes = wristRight.parent == Avatar.AvatarRig.RightArm.Forearm ? rightArmInfo.ForearmUniversalLocalAxes : rightArmInfo.HandUniversalLocalAxes;

                Vector3    sensorRightPos = Avatar.transform.TransformPoint(_rightHandState.RootPose.Position.FromFlippedZVector3f());
                Quaternion sensorRightRot = Avatar.transform.rotation * ToCorrectCoordinateSystem(_rightHandState.RootPose.Orientation, FlipMode.FlipZ, _rightHandOculusRotation, rightHandParentAxes, rightArmInfo.HandUniversalLocalAxes);

                wristRight.position = sensorRightPos;
                wristRight.rotation = sensorRightRot;

                UpdateFinger(UxrHandSide.Right, Avatar.RightHand.Index,  OVRPlugin.BoneId.Hand_Index1,  3, _rightFingerOculusRotation, rightArmInfo.HandUniversalLocalAxes, rightArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Right, Avatar.RightHand.Middle, OVRPlugin.BoneId.Hand_Middle1, 3, _rightFingerOculusRotation, rightArmInfo.HandUniversalLocalAxes, rightArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Right, Avatar.RightHand.Ring,   OVRPlugin.BoneId.Hand_Ring1,   3, _rightFingerOculusRotation, rightArmInfo.HandUniversalLocalAxes, rightArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Right, Avatar.RightHand.Little, OVRPlugin.BoneId.Hand_Pinky1,  3, _rightFingerOculusRotation, rightArmInfo.HandUniversalLocalAxes, rightArmInfo.FingerUniversalLocalAxes);
                UpdateFinger(UxrHandSide.Right, Avatar.RightHand.Thumb,  OVRPlugin.BoneId.Hand_Thumb0,  4, _rightFingerOculusRotation, rightArmInfo.HandUniversalLocalAxes, rightArmInfo.FingerUniversalLocalAxes);
            }

#endif
        }

        #endregion

#if ULTIMATEXR_USE_OCULUS_SDK

        /// <summary>
        ///     Updates a finger using tracking information.
        /// </summary>
        /// <param name="handSide">Which hand the finger belongs to</param>
        /// <param name="avatarFinger">The avatar finger to update</param>
        /// <param name="baseBoneId">The oculus bone base id</param>
        /// <param name="boneCount">The number of bones to update, usually 2, 3 or 4</param>
        /// <param name="fingerOculusRotation">Oculus finger coordinate system</param>
        /// <param name="wristUniversalLocalAxes">Avatar wrist coordinate system</param>
        /// <param name="fingerUniversalLocalAxes">Avatar finger coordinate system</param>
        private void UpdateFinger(UxrHandSide handSide, UxrAvatarFinger avatarFinger, OVRPlugin.BoneId baseBoneId, int boneCount, Quaternion fingerOculusRotation, UxrUniversalLocalAxes wristUniversalLocalAxes, UxrUniversalLocalAxes fingerUniversalLocalAxes)
        {
            int baseIndex = (int)baseBoneId;

            OVRPlugin.HandState handState = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;
            FlipMode            flipMode  = handSide == UxrHandSide.Left ? FlipMode.FlipX : FlipMode.FlipZ;

            if (boneCount > 3)
            {
                if (avatarFinger.Metacarpal != null)
                {
                    avatarFinger.Metacarpal.localRotation = ToCorrectCoordinateSystem(handState.BoneRotations[baseIndex], flipMode, fingerOculusRotation, wristUniversalLocalAxes, fingerUniversalLocalAxes);
                    ApplyBoneCalibration(avatarFinger.Metacarpal);
                }

                baseIndex++;
            }

            if (boneCount > 2)
            {
                avatarFinger.Proximal.localRotation = ToCorrectCoordinateSystem(handState.BoneRotations[baseIndex], flipMode, fingerOculusRotation, avatarFinger.Metacarpal == null ? wristUniversalLocalAxes : fingerUniversalLocalAxes, fingerUniversalLocalAxes);
                ApplyBoneCalibration(avatarFinger.Proximal);

                baseIndex++;
            }

            avatarFinger.Intermediate.localRotation = ToCorrectCoordinateSystem(handState.BoneRotations[baseIndex],     flipMode, fingerOculusRotation, fingerUniversalLocalAxes, fingerUniversalLocalAxes);
            avatarFinger.Distal.localRotation       = ToCorrectCoordinateSystem(handState.BoneRotations[baseIndex + 1], flipMode, fingerOculusRotation, fingerUniversalLocalAxes, fingerUniversalLocalAxes);

            ApplyBoneCalibration(avatarFinger.Intermediate);
            ApplyBoneCalibration(avatarFinger.Distal);
        }

        /// <summary>
        ///     Converts a rotation from the Oculus SDK coordinate system to the avatar coordinate system.
        /// </summary>
        /// <param name="oculusRotation">Oculus rotation to convert</param>
        /// <param name="flipRotation">How to process the rotation</param>
        /// <param name="oculusAxes">
        ///     Information that converts from the "universal" coordinate system to the coordinate
        ///     system used by the parent's node.
        /// </param>
        /// <param name="parentUniversalLocalAxes"></param>
        /// <param name="universalLocalAxes">
        ///     Information that converts from the "universal" coordinate system to the coordinate
        ///     system used by the node.
        /// </param>
        /// <returns>Rotation in the avatar coordinate system</returns>
        private Quaternion ToCorrectCoordinateSystem(OVRPlugin.Quatf oculusRotation, FlipMode flipRotation, Quaternion oculusAxes, UxrUniversalLocalAxes parentUniversalLocalAxes, UxrUniversalLocalAxes universalLocalAxes)
        {
            Quaternion rotation = oculusRotation.FromQuatf();

            switch (flipRotation)
            {
                case FlipMode.FlipX:
                    rotation = oculusRotation.FromFlippedXQuatf();
                    break;

                case FlipMode.FlipZ:
                    rotation = oculusRotation.FromFlippedZQuatf();
                    break;
            }

            Quaternion finalRotation = Quaternion.Inverse(parentUniversalLocalAxes.UniversalToActualAxesRotation) * universalLocalAxes.UniversalToActualAxesRotation * rotation * Quaternion.Inverse(oculusAxes) * universalLocalAxes.UniversalToActualAxesRotation;
            return finalRotation.IsValid() ? finalRotation : Quaternion.identity;
        }

#endif

#if ULTIMATEXR_USE_OCULUS_SDK

        private bool                _isLeftHandAvailable;
        private bool                _isRightHandAvailable;
        private OVRPlugin.HandState _leftHandState;
        private OVRPlugin.HandState _rightHandState;
        private Quaternion          _leftHandOculusRotation;
        private Quaternion          _rightHandOculusRotation;
        private Quaternion          _leftFingerOculusRotation;
        private Quaternion          _rightFingerOculusRotation;

#endif
    }
}