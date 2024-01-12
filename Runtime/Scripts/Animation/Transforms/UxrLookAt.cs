// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLookAt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Math;
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
        [SerializeField] private UxrAxis       _lookAxis               = UxrAxis.Z;
        [SerializeField] private UxrAxis       _upAxis                 = UxrAxis.Y;
        [SerializeField] private UxrAxis       _matchDirection         = UxrAxis.Z;
        [SerializeField] private bool          _allowRotateAroundUp    = true;
        [SerializeField] private bool          _allowRotateAroundRight = true;
        [SerializeField] private bool          _invertedLookAxis;
        [SerializeField] private bool          _onlyOnce;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Makes an object look at a specific target.
        /// </summary>
        /// <param name="gameObject">The object that will look at the target</param>
        /// <param name="target">The target</param>
        /// <param name="lookAxis">The object look axis</param>
        /// <param name="upAxis">The object up vector</param>
        /// <param name="allowRotateAroundObjectUp">
        ///     Should the lookAt alter the rotation around the vertical axis?
        /// </param>
        /// <param name="allowRotateAroundObjectRight">
        ///     Should the lookAt alter the rotation around the horizontal axis?
        /// </param>
        /// <param name="invertedLookAxis">
        ///     If true, the target's look axis will try to point at the opposite direction where the target is. By default this is
        ///     false, meaning the look vector will try to point at the target
        /// </param>
        public static void MakeLookAt(GameObject gameObject,
                                      Transform  target,
                                      UxrAxis    lookAxis,
                                      UxrAxis    upAxis,
                                      bool       allowRotateAroundObjectUp    = true,
                                      bool       allowRotateAroundObjectRight = true,
                                      bool       invertedLookAxis             = false,
                                      bool       onlyOnce                     = false)
        {
            if (gameObject == null)
            {
                return;
            }

            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                   = UxrLookAtMode.Target;
            lookAtComponent._target                 = target;
            lookAtComponent._lookAxis               = lookAxis;
            lookAtComponent._upAxis                 = upAxis;
            lookAtComponent._allowRotateAroundUp    = allowRotateAroundObjectUp;
            lookAtComponent._allowRotateAroundRight = allowRotateAroundObjectRight;
            lookAtComponent._invertedLookAxis       = invertedLookAxis;
            lookAtComponent._onlyOnce               = onlyOnce;

            lookAtComponent.PerformLookAt(false);
        }

        /// <summary>
        ///     Makes an object look along a specific target's direction.
        /// </summary>
        /// <param name="gameObject">The object that will look at the target</param>
        /// <param name="target">The target</param>
        /// <param name="direction">The target axis</param>
        /// <param name="lookAxis">The object look axis</param>
        /// <param name="upAxis">The object up vector</param>
        /// <param name="allowRotateAroundUp">
        ///     Should the lookAt alter the rotation around the vertical axis?
        /// </param>
        /// <param name="allowRotateAroundRight">
        ///     Should the lookAt alter the rotation around the horizontal axis?
        /// </param>
        /// <param name="invertedForwardAxis">
        ///     If true, the target's forward axis will try to point at the opposite direction where the target is. By default this
        ///     is false, meaning the forward vector will try to point at the target
        /// </param>
        /// <param name="onlyOnce">Whether to perform the look at only once</param>
        public static void MatchTargetDirection(GameObject gameObject,
                                                Transform  target,
                                                UxrAxis    direction,
                                                UxrAxis    lookAxis,
                                                UxrAxis    upAxis,
                                                bool       allowRotateAroundUp    = true,
                                                bool       allowRotateAroundRight = true,
                                                bool       invertedForwardAxis    = false,
                                                bool       onlyOnce               = false)
        {
            if (gameObject == null)
            {
                return;
            }

            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                   = UxrLookAtMode.MatchTargetDirection;
            lookAtComponent._target                 = target;
            lookAtComponent._matchDirection         = direction;
            lookAtComponent._lookAxis               = lookAxis;
            lookAtComponent._upAxis                 = upAxis;
            lookAtComponent._allowRotateAroundUp    = allowRotateAroundUp;
            lookAtComponent._allowRotateAroundRight = allowRotateAroundRight;
            lookAtComponent._invertedLookAxis       = invertedForwardAxis;
            lookAtComponent._onlyOnce               = onlyOnce;

            lookAtComponent.PerformLookAt(false);
        }

        /// <summary>
        ///     Makes an object look in a specific direction.
        /// </summary>
        /// <param name="gameObject">The object that will look at the given direction</param>
        /// <param name="direction">The world direction that the object will look at</param>
        /// <param name="lookAxis">The object look axis</param>
        /// <param name="upAxis">The object up vector</param>
        /// <param name="allowRotateAroundObjectUp">
        ///     Should the lookAt alter the rotation around the vertical axis?
        /// </param>
        /// <param name="allowRotateAroundObjectRight">
        ///     Should the lookAt alter the rotation around the horizontal axis?
        /// </param>
        /// <param name="invertedForwardAxis">
        ///     If true, the target's forward axis will try to point at the opposite direction where the target is. By default this
        ///     is false, meaning the forward vector will try to point at the target
        /// </param>
        /// <param name="onlyOnce">Whether to perform the look at only once</param>
        public static void MatchWorldDirection(GameObject gameObject,
                                               UxrAxis    direction,
                                               UxrAxis    lookAxis,
                                               UxrAxis    upAxis,
                                               bool       allowRotateAroundUp    = true,
                                               bool       allowRotateAroundRight = true,
                                               bool       invertedForwardAxis    = false,
                                               bool       onlyOnce               = false)
        {
            if (gameObject == null)
            {
                return;
            }

            UxrLookAt lookAtComponent = gameObject.GetOrAddComponent<UxrLookAt>();

            lookAtComponent._mode                   = UxrLookAtMode.MatchWorldDirection;
            lookAtComponent._matchDirection         = direction;
            lookAtComponent._lookAxis               = lookAxis;
            lookAtComponent._upAxis                 = upAxis;
            lookAtComponent._allowRotateAroundUp    = allowRotateAroundUp;
            lookAtComponent._allowRotateAroundRight = allowRotateAroundRight;
            lookAtComponent._invertedLookAxis       = invertedForwardAxis;
            lookAtComponent._onlyOnce               = onlyOnce;

            lookAtComponent.PerformLookAt(false);
        }

        /// <summary>
        ///     Removes an UxrLookAt component if it exists.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove the component from</param>
        public static void RemoveLookAt(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            UxrLookAt lookAtComponent = gameObject.GetComponent<UxrLookAt>();

            if (lookAtComponent)
            {
                Destroy(lookAtComponent);
            }
        }

        /// <summary>
        ///     Forces to execute an instant LookAt.
        /// </summary>
        public void ForceLookAt()
        {
            PerformLookAt(true);
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

            _initialLocalRotation = gameObject.transform.localRotation;
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
            PerformLookAt(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Performs look at.
        /// </summary>
        private static void PerformLookAt(GameObject    gameObject,
                                          Quaternion    initialLocalRotation,
                                          UxrLookAtMode mode,
                                          Transform     target,
                                          UxrAxis       direction,
                                          UxrAxis       lookAxis,
                                          UxrAxis       upAxis,
                                          bool          allowRotateAroundUp    = true,
                                          bool          allowRotateAroundRight = true,
                                          bool          invertedForwardAxis    = false)
        {
            if (gameObject == null)
            {
                return;
            }

            if (!allowRotateAroundUp && !allowRotateAroundRight)
            {
                return;
            }

            Vector3 worldDir = Vector3.forward;

            if (mode == UxrLookAtMode.Target)
            {
                if (target == null)
                {
                    return;
                }

                worldDir = target.position - gameObject.transform.position;
            }
            else if (mode == UxrLookAtMode.MatchTargetDirection)
            {
                if (target == null)
                {
                    return;
                }

                worldDir = target.TransformDirection(direction);
            }
            else if (mode == UxrLookAtMode.MatchWorldDirection)
            {
                worldDir = direction;
            }

            if (invertedForwardAxis)
            {
                worldDir = -worldDir;
            }

            Quaternion sourceRot = TransformExt.GetWorldRotation(gameObject.transform.parent, initialLocalRotation);
            Quaternion rotation  = Quaternion.identity;

            if (allowRotateAroundUp && allowRotateAroundRight)
            {
                rotation = Quaternion.FromToRotation(sourceRot * lookAxis, worldDir);
            }
            else if (allowRotateAroundUp)
            {
                Vector3 axis = sourceRot * upAxis;
                            
                // Project on the up plane
                worldDir = Vector3.ProjectOnPlane(worldDir, axis);
                
                float degrees = Vector3.SignedAngle(sourceRot * lookAxis, worldDir, axis);
                rotation = Quaternion.AngleAxis(degrees, axis);
            }
            else if (allowRotateAroundRight)
            {
                Vector3 axis = sourceRot * Vector3.Cross(upAxis, lookAxis);
                
                // Project on the right plane
                worldDir = Vector3.ProjectOnPlane(worldDir, axis);

                float degrees = Vector3.SignedAngle(sourceRot * lookAxis, worldDir, axis);
                rotation = Quaternion.AngleAxis(degrees, axis);
            }

            gameObject.transform.rotation = rotation * sourceRot;
        }

        /// <summary>
        ///     Performs look at.
        /// </summary>
        /// <param name="force">Whether to force the look-at</param>
        private void PerformLookAt(bool force)
        {
            if (_repeat || force)
            {
                PerformLookAt(gameObject, _initialLocalRotation, _mode, _target, _matchDirection, _lookAxis, _upAxis, _allowRotateAroundUp, _allowRotateAroundRight, _invertedLookAxis);

                if (_onlyOnce)
                {
                    _repeat = false;
                }
            }
        }

        #endregion

        #region Private Types & Data

        private bool       _repeat = true;
        private Quaternion _initialLocalRotation;

        #endregion
    }
}