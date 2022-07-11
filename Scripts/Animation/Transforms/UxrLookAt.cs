// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLookAt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     Component that allows to continuously orientate an object looking at a specified point or along an axis.
    /// </summary>
    public sealed class UxrLookAt : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrLookAtMode _mode = UxrLookAtMode.Target;
        [SerializeField] private Transform     _target;
        [SerializeField] private Vector3       _direction          = Vector3.forward;
        [SerializeField] private bool          _allowRotateAroundY = true;
        [SerializeField] private bool          _allowRotateAroundX = true;
        [SerializeField] private bool          _invertedForwardAxis;
        [SerializeField] private bool          _onlyOnce;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Makes an object look at a specific target.
        /// </summary>
        /// <param name="gameObject">The object that will look at the target</param>
        /// <param name="target">The target</param>
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
        ///     the opposite direction where the target is. By default this is false, meaning the forward
        ///     vector will try to point at the target
        /// </param>
        public static void MakeLookAt(GameObject gameObject, Transform target, bool allowRotateAroundVerticalAxis, bool allowRotateAroundHorizontalAxis, bool invertedForwardAxis)
        {
            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                = UxrLookAtMode.Target;
            lookAtComponent._target              = target;
            lookAtComponent._allowRotateAroundY  = allowRotateAroundVerticalAxis;
            lookAtComponent._allowRotateAroundX  = allowRotateAroundHorizontalAxis;
            lookAtComponent._invertedForwardAxis = invertedForwardAxis;
        }

        /// <summary>
        ///     Makes an object look along a specific target's direction.
        /// </summary>
        /// <param name="gameObject">The object that will look at the target</param>
        /// <param name="target">The target</param>
        /// <param name="direction">The direction in target's coordinate system</param>
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
        ///     the opposite direction where the target is. By default this is false, meaning the forward
        ///     vector will try to point at the target
        /// </param>
        public static void MakeLookAt(GameObject gameObject, Transform target, Vector3 direction, bool allowRotateAroundVerticalAxis, bool allowRotateAroundHorizontalAxis, bool invertedForwardAxis)
        {
            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                = UxrLookAtMode.MatchTargetDirection;
            lookAtComponent._target              = target;
            lookAtComponent._allowRotateAroundY  = allowRotateAroundVerticalAxis;
            lookAtComponent._allowRotateAroundX  = allowRotateAroundHorizontalAxis;
            lookAtComponent._invertedForwardAxis = invertedForwardAxis;
        }

        /// <summary>
        ///     Makes an object look in a specific direction.
        /// </summary>
        /// <param name="gameObject">The object that will look at the given direction</param>
        /// <param name="direction">The direction that the object will look at, in world space</param>
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
        ///     the opposite direction where the target is. By default this is false, meaning the forward
        ///     vector will try to point at the target
        /// </param>
        public static void MakeLookAt(GameObject gameObject, Vector3 direction, bool allowRotateAroundVerticalAxis, bool allowRotateAroundHorizontalAxis, bool invertedForwardAxis)
        {
            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                = UxrLookAtMode.WorldDirection;
            lookAtComponent._direction           = direction;
            lookAtComponent._allowRotateAroundY  = allowRotateAroundVerticalAxis;
            lookAtComponent._allowRotateAroundX  = allowRotateAroundHorizontalAxis;
            lookAtComponent._invertedForwardAxis = invertedForwardAxis;
        }

        /// <summary>
        ///     Removes an UxrLookAt component if it exists.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove the component from</param>
        public static void RemoveLookAt(GameObject gameObject)
        {
            if (gameObject)
            {
                UxrLookAt lookAtComponent = gameObject.GetComponent<UxrLookAt>();

                if (lookAtComponent)
                {
                    Destroy(lookAtComponent);
                }
            }
        }

        /// <summary>
        ///     Forces to execute an instant LookAt.
        /// </summary>
        public void ForceLookAt()
        {
            PerformLookAt();
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
        ///     Called when the avatars finished updating. Performs the look at.
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
            if (_repeat)
            {
                Vector3 lookAt = _direction;

                if (_mode == UxrLookAtMode.Target)
                {
                    if (_target == null)
                    {
                        return;
                    }

                    lookAt = _target.position - transform.position;
                }
                else if (_mode == UxrLookAtMode.MatchTargetDirection)
                {
                    if (_target == null)
                    {
                        return;
                    }

                    lookAt = _target.TransformDirection(_direction);
                }

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