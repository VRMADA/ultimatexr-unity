// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarControllerEvent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Describes an event that maps an XR controller input to a hand pose. This allows to show different poses when
    ///     certain buttons are pressed. It also allows to describe which poses need to be used when grabbing or pointing
    ///     with the finger.
    /// </summary>
    [Serializable]
    public class UxrAvatarControllerEvent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]                     private UxrInputButtons  _buttons;
        [SerializeField]                     private UxrAnimationType _animationType;
        [SerializeField]                     private UxrHandPoseAsset _handPose;
        [SerializeField] [Range(0.0f, 1.0f)] private float            _poseBlendValue;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the hand pose name that should be used on the event.
        /// </summary>
        public string PoseName => string.IsNullOrEmpty(_poseNameOverride) ? _handPose != null ? _handPose.name : null : _poseNameOverride;

        /// <summary>
        ///     Gets or sets the button(s) that trigger the animation event.
        /// </summary>
        public UxrInputButtons Buttons
        {
            get => _buttons;
            set => _buttons = value;
        }

        /// <summary>
        ///     Gets the type of animation the event represents. This allows to keep track of certain key animations such as
        ///     grabbing or pointing with the finger, that are used in the framework.
        /// </summary>
        public UxrAnimationType TypeOfAnimation
        {
            get => _animationType;
            set => _animationType = value;
        }

        /// <summary>
        ///     Gets or sets the pose name that will be used instead of the pose stored. If null, the pose will be used instead.
        /// </summary>
        public string PoseNameOverride
        {
            get => _poseNameOverride;
            set => _poseNameOverride = value;
        }

        /// <summary>
        ///     Gets or sets the pose blend value if the pose is <see cref="UxrHandPoseType.Blend" />.
        /// </summary>
        public float PoseBlendValue
        {
            get => _poseBlendValue;
            set => _poseBlendValue = value;
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Event type: {_animationType}, button(s): {_buttons}, pose: {PoseName}, blend: {_poseBlendValue}";
        }

        #endregion

        #region Private Types & Data

        private string _poseNameOverride;

        #endregion
    }
}