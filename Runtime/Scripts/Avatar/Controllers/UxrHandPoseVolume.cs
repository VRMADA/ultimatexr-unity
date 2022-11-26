// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseVolume.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Component that describes a box volume where any <see cref="UxrAvatar" /> hand that gets inside automatically adopts
    ///     a given hand pose.
    /// </summary>
    /// <remarks>
    ///     The finger pointing pose should only be adopted if it doesn't interfere with any other interaction, such
    ///     as the grab pose while grabbing an object inside the volume.
    /// </remarks>
    [RequireComponent(typeof(BoxCollider))]
    public class UxrHandPoseVolume : UxrComponent<UxrHandPoseVolume>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private string _poseName;
        [SerializeField] private bool   _leftHand  = true;
        [SerializeField] private bool   _rightHand = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="BoxCollider" /> component describing the enclosed space where to adopt the pose.
        /// </summary>
        public BoxCollider Box => GetCachedComponent<BoxCollider>();

        /// <summary>
        ///     Gets or sets whether the left hand should adopt the pose when inside.
        /// </summary>
        public bool UseLeftHand
        {
            get => _leftHand;
            set => _leftHand = value;
        }

        /// <summary>
        ///     Gets or sets whether the right hand should adopt the pose when inside.
        /// </summary>
        public bool UseRightHand
        {
            get => _rightHand;
            set => _rightHand = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to the avatars updated event.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from the avatars updated event.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called every frame after the avatars have been updated. Performs the hand check.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (UxrAvatar.LocalAvatar == null)
            {
                return;
            }

            if (UxrAvatar.LocalAvatar.AvatarController is UxrStandardAvatarController avatarController)
            {
            }
            else
            {
                return;
            }

            if (IsCompatible(UxrHandSide.Left) && IsPointInside(UxrAvatar.LocalAvatar.GetHand(UxrHandSide.Left).Wrist.position))
            {
                avatarController.LeftHandDefaultPoseNameOverride = _poseName;
                _leftWasInside                                   = true;
            }
            else if (_leftWasInside)
            {
                avatarController.LeftHandDefaultPoseNameOverride = null;
                _leftWasInside                                   = false;
            }

            if (IsCompatible(UxrHandSide.Right) && IsPointInside(UxrAvatar.LocalAvatar.GetHand(UxrHandSide.Right).Wrist.position))
            {
                avatarController.RightHandDefaultPoseNameOverride = _poseName;
                _rightWasInside                                   = true;
            }
            else if (_rightWasInside)
            {
                avatarController.RightHandDefaultPoseNameOverride = null;
                _rightWasInside                                   = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks if a point is inside the <see cref="BoxCollider" /> attached to the <see cref="GameObject" /> this component
        ///     is attached to.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="margin">Margin to add to the box sides</param>
        /// <returns>True if it is inside, false if not</returns>
        private bool IsPointInside(Vector3 point, float margin = 0.0f)
        {
            return point.IsInsideBox(Box, Vector3.one * margin);
        }

        /// <summary>
        ///     Checks if the volume is compatible with the given hand. This allows some volumes to work for the left or
        ///     right hand only.
        /// </summary>
        /// <param name="handSide">Hand to check</param>
        /// <returns>Boolean telling whether the given hand is compatible or not</returns>
        private bool IsCompatible(UxrHandSide handSide)
        {
            return (handSide == UxrHandSide.Left && UseLeftHand) || (handSide == UxrHandSide.Right && UseRightHand);
        }

        #endregion

        #region Private Types & Data

        private bool _leftWasInside;
        private bool _rightWasInside;

        #endregion
    }
}