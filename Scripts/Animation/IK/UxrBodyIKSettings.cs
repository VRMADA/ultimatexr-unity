// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrBodyIKSettings.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Stores parameters that drive Inverse Kinematics for full-body avatars.
    /// </summary>
    /// <remarks>
    ///     For now only half-body Inverse Kinematics is supported. Full-body will be implemented at some point.
    /// </remarks>
    [Serializable]
    public class UxrBodyIKSettings
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool  _lockBodyPivot;
        [SerializeField] private float _bodyPivotRotationSpeed = 0.2f;
        [SerializeField] private float _headFreeRangeBend      = 20.0f;
        [SerializeField] private float _headFreeRangeTorsion   = 30.0f;
        [SerializeField] private float _neckHeadBalance        = 0.5f;
        [SerializeField] private float _spineBend              = 0.05f;
        [SerializeField] private float _spineTorsion           = 0.4f;
        [SerializeField] private float _chestBend              = 0.3f;
        [SerializeField] private float _chestTorsion           = 0.8f;
        [SerializeField] private float _upperChestBend         = 0.4f;
        [SerializeField] private float _upperChestTorsion      = 0.2f;
        [SerializeField] private float _neckBaseHeight         = 1.6f;
        [SerializeField] private float _neckForwardOffset;
        [SerializeField] private float _eyesBaseHeight    = 1.75f;
        [SerializeField] private float _eyesForwardOffset = 0.1f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the avatar pivot will be kept in place so that it will only rotate around.
        /// </summary>
        public bool LockBodyPivot => _lockBodyPivot;

        /// <summary>
        ///     Gets the speed the body will turn around with. This is used to smooth out rotation.
        /// </summary>
        public float BodyPivotRotationSpeed => _bodyPivotRotationSpeed;

        /// <summary>
        ///     Gets the amount of degrees the head can bend before requiring rotation of other bones down the spine.
        /// </summary>
        public float HeadFreeRangeBend => _headFreeRangeBend;

        /// <summary>
        ///     Gets the amount of degrees the head can turn before requiring rotation of other bones down the spine.
        /// </summary>
        public float HeadFreeRangeTorsion => _headFreeRangeTorsion;

        /// <summary>
        ///     Gets a value in [0.0, 1.0] range that tells how rotation will be distributed between the head and the neck. 0.0
        ///     will fully use the neck and 1.0 will fully use the head. Values in between will distribute it among the two.
        /// </summary>
        public float NeckHeadBalance => _neckHeadBalance;

        /// <summary>
        ///     Gets the amount the spine will bend when the head bends.
        /// </summary>
        public float SpineBend => _spineBend;

        /// <summary>
        ///     Gets the amount the spine will turn when the head turns.
        /// </summary>
        public float SpineTorsion => _spineTorsion;

        /// <summary>
        ///     Gets the amount the chest will bend when the head bends.
        /// </summary>
        public float ChestBend => _chestBend;

        /// <summary>
        ///     Gets the amount the chest will turn when the head turns.
        /// </summary>
        public float ChestTorsion => _chestTorsion;

        /// <summary>
        ///     Gets the amount the upper chest will bend when the head bends.
        /// </summary>
        public float UpperChestBend => _upperChestBend;

        /// <summary>
        ///     Gets the amount the upper chest will turn when the head turns.
        /// </summary>
        public float UpperChestTorsion => _upperChestTorsion;

        /// <summary>
        ///     Gets the height of the base of the neck starting from the avatar root Y. This is used to create a dummy neck when
        ///     the avatar is lacking a neck bone.
        /// </summary>
        public float NeckBaseHeight => _neckBaseHeight;

        /// <summary>
        ///     Gets the forward offset of the neck starting from the avatar root Z. This is used to create a dummy neck when the
        ///     avatar is lacking a neck bone.
        /// </summary>
        public float NeckForwardOffset => _neckForwardOffset;

        /// <summary>
        ///     Gets the height of the eyes starting from the avatar root Y. This is used to know where to place the avatar head
        ///     knowing the camera will be positioned on the eyes.
        /// </summary>
        public float EyesBaseHeight => _eyesBaseHeight;

        /// <summary>
        ///     Gets the forward offset of the eyes starting from the avatar root Z. This is used to know where to place the avatar
        ///     head knowing the camera will be positioned on the eyes.
        /// </summary>
        public float EyesForwardOffset => _eyesForwardOffset;

        #endregion
    }
}