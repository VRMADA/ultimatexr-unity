// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Base class for hand tracking. Includes base functionality to update the avatar and calibrate the skeleton
    ///     based on a well-known pose.
    /// </summary>
    public abstract partial class UxrHandTracking : UxrTrackingDevice
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrHandPoseAsset      _calibrationPose;
        [SerializeField] private List<BoneCalibration> _leftCalibrationData  = new List<BoneCalibration>();
        [SerializeField] private List<BoneCalibration> _rightCalibrationData = new List<BoneCalibration>();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether there is tracking data currently available for the left hand.
        /// </summary>
        public abstract bool IsLeftHandAvailable { get; }

        /// <summary>
        ///     Gets whether there is tracking data currently available for the right hand.
        /// </summary>
        public abstract bool IsRightHandAvailable { get; }

        /// <summary>
        ///     Gets whether tracking data is currently available for any hand.
        /// </summary>
        public bool IsAvailable => IsLeftHandAvailable || IsRightHandAvailable;

        /// <summary>
        ///     Gets whether the component contains calibration data collected by using the inspector.
        /// </summary>
        public bool HasCalibrationData => _leftCalibrationData != null && _leftCalibrationData.Count > 0 && _rightCalibrationData != null && _rightCalibrationData.Count > 0;

        /// <summary>
        ///     Gets or sets whether to use calibration data to minimize the mismatches between the particular hand rig used and
        ///     the tracking values.
        /// </summary>
        public bool UseCalibration { get; set; } = true;

        #endregion

        #region Public Overrides UxrTrackingDevice

        /// <inheritdoc />
        public override int TrackingUpdateOrder => OrderPostprocess;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Collects the calibration data for a given hand.
        /// </summary>
        /// <param name="handSide">Which hand to collect calibration data for</param>
        public bool CollectCalibrationData(UxrHandSide handSide)
        {
            // Check conditions

            if (Avatar == null || _calibrationPose == null)
            {
                return false;
            }

            UxrAvatarHand avatarHand = Avatar.GetHand(handSide);

            if (!avatarHand.HasFingerData())
            {
                return false;
            }

            // Store current rotations without calibration

            bool useCalibration = UseCalibration;
            UseCalibration = false;

            UpdateSensors();
            UpdateAvatar();

            List<BoneCalibration> calibrationData = new List<BoneCalibration>();

            foreach (Transform boneTransform in avatarHand.FingerTransforms)
            {
                calibrationData.Add(new BoneCalibration(boneTransform, boneTransform.localRotation));
            }

            UseCalibration = useCalibration;

            // Compute relative rotations to calibration pose

            Avatar.SetCurrentHandPoseImmediately(handSide, _calibrationPose);

            foreach (BoneCalibration boneCalibration in calibrationData)
            {
                boneCalibration.Rotation = Quaternion.Inverse(boneCalibration.Rotation) * boneCalibration.Transform.localRotation;
            }

            // Store calibration

            if (handSide == UxrHandSide.Left)
            {
                _leftCalibrationData = calibrationData;
            }
            else if (handSide == UxrHandSide.Right)
            {
                _rightCalibrationData = calibrationData;
            }

            // Re-build cache
            BuildCalibrationCache();

            return true;
        }

        /// <summary>
        ///     Clears the calibration data for a given hand.
        /// </summary>
        /// <param name="handSide">Which hand to clear</param>
        public void ClearCalibrationData(UxrHandSide handSide)
        {
            if (handSide == UxrHandSide.Left)
            {
                _leftCalibrationData = new List<BoneCalibration>();
            }
            else if (handSide == UxrHandSide.Right)
            {
                _rightCalibrationData = new List<BoneCalibration>();
            }

            // Re-build cache
            BuildCalibrationCache();
        }

        /// <summary>
        ///     Creates the calibration cache to be able to get calibration of a given transform using a dictionary.
        /// </summary>
        public void BuildCalibrationCache()
        {
            _calibrationCache = new Dictionary<Transform, BoneCalibration>();

            foreach (BoneCalibration boneCalibration in _leftCalibrationData)
            {
                _calibrationCache.Add(boneCalibration.Transform, boneCalibration);
            }

            foreach (BoneCalibration boneCalibration in _rightCalibrationData)
            {
                _calibrationCache.Add(boneCalibration.Transform, boneCalibration);
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

            UxrManager.AvatarsUpdating += UxrManager_AvatarsUpdating;

            BuildCalibrationCache();

            // Will be enabled/disabled using the UxrManager_AvatarsUpdating event.
            enabled = false;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            UxrManager.AvatarsUpdating -= UxrManager_AvatarsUpdating;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Checks whether the component should be enabled or disabled.
        /// </summary>
        private void UxrManager_AvatarsUpdating()
        {
#if ULTIMATEXR_USE_OCULUS_SDK

            if (enabled != IsAvailable && Avatar.AvatarController != null && Avatar.AvatarController.AllowHandTracking)
            {
                enabled = IsAvailable;

                string newStatus = enabled ? "Enabled" : "Disabled";
                Debug.Log($"{GetType().Name}: Status changed to {newStatus}");
            }

#endif
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Adopts the calibration pose.
        /// </summary>
        /// <param name="handSide">Which hand side should adopt the calibration pose</param>
        protected void SetCalibrationPose(UxrHandSide handSide)
        {
            Avatar.SetCurrentHandPoseImmediately(handSide, _calibrationPose);
        }

        /// <summary>
        ///     Applies the calibration data collected by <see cref="CollectCalibrationData" /> so that the hands look as close to
        ///     the tracking data as possible.
        ///     The goal is to remove the slight differences between a random rigged hand and the tracked skeleton data.
        /// </summary>
        protected void ApplyBoneCalibration(Transform boneTransform)
        {
            if (!UseCalibration)
            {
                return;
            }

            if (_calibrationCache.TryGetValue(boneTransform, out BoneCalibration calibrationData))
            {
                boneTransform.localRotation = boneTransform.localRotation * calibrationData.Rotation;
            }
        }

        #endregion

        #region Private Types & Data

        private Dictionary<Transform, BoneCalibration> _calibrationCache = new Dictionary<Transform, BoneCalibration>();

        #endregion
    }
}