// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerTip.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.UI
{
    /// <summary>
    ///     Component that, added to the tip of an <see cref="UxrAvatar" /> finger, allows it to interact with user interfaces.
    ///     It is normally added only to the index fingers so that other fingers don't generate unwanted interactions, but it
    ///     can be added to any finger.
    /// </summary>
    public class UxrFingerTip : UxrAvatarComponent<UxrFingerTip>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the <see cref="UxrGrabber" /> component belonging to the same hand the finger tip is, is currently
        ///     grabbing something.
        /// </summary>
        public bool IsHandGrabbing => HandGrabber && HandGrabber.GrabbedObject != null;

        /// <summary>
        ///     Gets the current world position.
        /// </summary>
        public Vector3 WorldPos => transform.position;

        /// <summary>
        ///     Gets the current world direction. The direction points in the direction the finger would be pointing. It is used to
        ///     filter out interactions where the forward vector is not perpendicular enough to the UI to interact with it.
        /// </summary>
        public Vector3 WorldDir => transform.forward;

        /// <summary>
        ///     Gets the current world speed the finger tip is travelling at.
        /// </summary>
        public Vector3 WorldSpeed => _updateCount >= 2 ? (_currentWorldPos - _lastWorldPos) / Time.deltaTime : Vector3.zero;

        /// <summary>
        ///     Gets the hand the finger tip belongs to.
        /// </summary>
        public UxrHandSide Side { get; private set; }

        /// <summary>
        ///     Gets the <see cref="UxrGrabber" /> component of the hand the finger tip belongs to.
        /// </summary>
        public UxrGrabber HandGrabber { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the finger tip is inside a box collider.
        /// </summary>
        /// <param name="box">Box collider</param>
        /// <param name="allowWhileGrabbing">
        ///     Whether the operation that the check is for allows the hand to be grabbing something.
        ///     This means if the value is false and the hand is currently grabbing, it will always return false no matter the
        ///     finger tip is inside the box or not
        /// </param>
        /// <returns>Whether the finger tip is inside</returns>
        public bool IsInside(BoxCollider box, bool allowWhileGrabbing = false)
        {
            if (!allowWhileGrabbing && IsHandGrabbing)
            {
                return false;
            }

            return box != null && transform.position.IsInsideBox(box);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrAvatarRig.GetHandSide(transform, out UxrHandSide handSide);
            Side = handSide;
        }

        /// <summary>
        ///     Resets the internal state.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _updateCount = 0;
        }

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (!(Avatar && Avatar.AvatarController))
            {
                return;
            }

            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(Avatar, true))
            {
                if (grabber.Side == Side)
                {
                    HandGrabber = grabber;
                    break;
                }
            }
        }

        /// <summary>
        ///     Updates the component.
        /// </summary>
        private void Update()
        {
            _lastWorldPos    = _currentWorldPos;
            _currentWorldPos = transform.position;
            _updateCount++;
        }

        #endregion

        #region Private Types & Data

        private int     _updateCount;
        private Vector3 _currentWorldPos;
        private Vector3 _lastWorldPos;

        #endregion
    }
}