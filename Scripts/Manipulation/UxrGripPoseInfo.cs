// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGripPoseInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Describes how an object is grabbed. It tells the pose that will be used and how it will be snapped to the hand.
    ///     The key is stored in the object, ideally we would have Dictionary(key, GripPoseInfo) but since Unity does not
    ///     serialize Dictionaries we use a List(GripPoseInfo) containing the key (<see cref="AvatarPrefabGuid" />) as well.
    /// </summary>
    [Serializable]
    public class UxrGripPoseInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private string           _avatarPrefabGuid;
        [SerializeField] private UxrHandPoseAsset _handPose;
        [SerializeField] private float            _poseBlendValue;
        [SerializeField] private Transform        _gripAlignTransformHandLeft;
        [SerializeField] private Transform        _gripAlignTransformHandRight;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the GUID of the avatar prefab the grip pose info belongs to.
        /// </summary>
        public string AvatarPrefabGuid => _avatarPrefabGuid;

        /// <summary>
        ///     Gets or sets the left grab pose preview mesh.
        /// </summary>
        public Mesh GrabPoseMeshLeft { get; set; }

        /// <summary>
        ///     Gets or sets the right grab pose preview mesh.
        /// </summary>
        public Mesh GrabPoseMeshRight { get; set; }

        /// <summary>
        ///     Gets or sets the pose that will be used when grabbing.
        /// </summary>
        public UxrHandPoseAsset HandPose
        {
            get => _handPose;
            set => _handPose = value;
        }

        /// <summary>
        ///     Gets or sets the pose blend value if the pose has the possibility of blending. Blending is used to blend between
        ///     open/closed grips or other animations.
        /// </summary>
        public float PoseBlendValue
        {
            get => _poseBlendValue;
            set => _poseBlendValue = value;
        }

        /// <summary>
        ///     Gets or sets the <see cref="Transform" /> that will be used to align the object grab point to the left
        ///     <see cref="UxrGrabber" /> that grabbed it.
        /// </summary>
        public Transform GripAlignTransformHandLeft
        {
            get => _gripAlignTransformHandLeft;
            set => _gripAlignTransformHandLeft = value;
        }

        /// <summary>
        ///     Gets or sets the <see cref="Transform" /> that will be used to align the object grab point to the right
        ///     <see cref="UxrGrabber" /> that grabbed it.
        /// </summary>
        public Transform GripAlignTransformHandRight
        {
            get => _gripAlignTransformHandRight;
            set => _gripAlignTransformHandRight = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatarPrefabGuid">
        ///     Avatar prefab GUID. Using prefabs allows to share poses among instances and also prefab variants to inherit poses
        ///     from their parent prefabs in the chain
        /// </param>
        public UxrGripPoseInfo(string avatarPrefabGuid)
        {
            if (!string.IsNullOrEmpty(avatarPrefabGuid))
            {
                _avatarPrefabGuid = avatarPrefabGuid;
            }
        }

        #endregion
    }
}