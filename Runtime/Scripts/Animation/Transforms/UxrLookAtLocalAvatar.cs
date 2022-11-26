// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLookAtLocalAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     Component that allows to continuously orientate an object looking at the local avatar camera
    /// </summary>
    public class UxrLookAtLocalAvatar : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _allowRotateAroundY = true;
        [SerializeField] private bool _allowRotateAroundX = true;
        [SerializeField] private bool _invertedForwardAxis;
        [SerializeField] private bool _onlyOnce;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Should the lookAt alter the rotation around the vertical axis?
        /// </summary>
        public bool AllowRotateAroundVerticalAxis
        {
            get => _allowRotateAroundY;
            set => _allowRotateAroundY = value;
        }

        /// <summary>
        ///     Should the lookAt alter the rotation around the horizontal axis?
        /// </summary>
        public bool AllowRotateAroundHorizontalAxis
        {
            get => _allowRotateAroundX;
            set => _allowRotateAroundX = value;
        }

        /// <summary>
        ///     If true, the target's forward axis will try to point at the opposite direction where the
        ///     avatar is. By default this is false, meaning the forward vector will try to point at
        ///     the avatar.
        /// </summary>
        public bool InvertedForwardAxis
        {
            get => _invertedForwardAxis;
            set => _invertedForwardAxis = value;
        }

        /// <summary>
        ///     If true, will only perform the lookat the first time it is called. Useful for explosions
        ///     or similar effects in VR.
        /// </summary>
        public bool OnlyOnce
        {
            get => _onlyOnce;
            set => _onlyOnce = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Makes an object look at the local avatar.
        /// </summary>
        /// <param name="gameObject">The object that will look at the local avatar</param>
        /// <param name="allowRotateAroundVerticalAxis">
        ///     Should the lookAt alter the rotation around the
        ///     vertical axis?
        /// </param>
        /// <param name="allowRotateAroundHorizontalAxis">
        ///     Should the lookAt alter the rotation around
        ///     the horizontal axis?
        /// </param>
        /// <param name="invertedForwardAxis">
        ///     If true, the target's forward axis will try to point at
        ///     the opposite direction where the avatar is. By default this is false, meaning the forward
        ///     vector will try to point at the avatar
        /// </param>
        public static void MakeLookAt(GameObject gameObject, bool allowRotateAroundVerticalAxis, bool allowRotateAroundHorizontalAxis, bool invertedForwardAxis)
        {
            UxrLookAtLocalAvatar lookAtComponent = gameObject.GetOrAddComponent<UxrLookAtLocalAvatar>();

            lookAtComponent._allowRotateAroundY  = allowRotateAroundVerticalAxis;
            lookAtComponent._allowRotateAroundX  = allowRotateAroundHorizontalAxis;
            lookAtComponent._invertedForwardAxis = invertedForwardAxis;
        }

        /// <summary>
        ///     Removes an UxrLookAtLocalAvatar component if it exists.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove the component from</param>
        public static void RemoveLookAt(GameObject gameObject)
        {
            if (gameObject)
            {
                UxrLookAtLocalAvatar lookAtComponent = gameObject.GetComponent<UxrLookAtLocalAvatar>();

                if (lookAtComponent)
                {
                    Destroy(lookAtComponent);
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after avatars are updated. Performs look at.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            PerformLookAt();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Performs look at
        /// </summary>
        private void PerformLookAt()
        {
            if (UxrManager.Instance && UxrAvatar.LocalAvatarCamera && _repeat)
            {
                Vector3 lookAt = UxrAvatar.LocalAvatar.CameraPosition - transform.position;

                if (_allowRotateAroundX == false)
                {
                    lookAt.y = 0.0f;
                }

                if (_allowRotateAroundY == false)
                {
                    lookAt = Vector3.ProjectOnPlane(lookAt, transform.right);
                }

                if (lookAt != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(_invertedForwardAxis ? -lookAt : lookAt);
                }

                if (_onlyOnce)
                {
                    _repeat = false;
                }
            }
        }

        #endregion

        #region Private Types & Data

        private bool _repeat = true;

        #endregion
    }
}