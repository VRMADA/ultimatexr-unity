﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneratorDoor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.Helpers;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Component that handles the generator door, which has battery locks to lock the battery in place.
    /// </summary>
    public class GeneratorDoor : UxrComponent<GeneratorDoor>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAutoSlideInAnchor _batteryAnchor;
        [SerializeField] private UxrGrabbableObject   _grabbableLock;
        [SerializeField] private Transform[]          _locks;
        [SerializeField] private float                _locksOpenRotation = 45.0f;
        [SerializeField] private bool                 _startLockOpen     = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the battery lock is open.
        /// </summary>
        public bool IsLockOpen
        {
            get => LockHandleOpenValue > 0.5f;
            private set
            {
                // Set rotation using the correct property to avoid interference between grabbable object constraint calculation and manually setting its transform.
                _grabbableLock.SingleRotationAxisDegrees = value ? _grabbableLock.MaxSingleRotationDegrees : _grabbableLock.MinSingleRotationDegrees;

                for (int i = 0; i < _locks.Length; ++i)
                {
                    _locks[i].transform.localRotation = _lockInitialRotation[i] * Quaternion.AngleAxis((value ? 1.0f : 0.0f) * _locksOpenRotation, Vector3.right);
                }
            }
        }

        /// <summary>
        ///     Gets whether there is a battery placed inside the generator and it is currently in contact with the bottom. This
        ///     allows to switch things on when the battery is actually in contact instead of being placed, because there is a
        ///     slide-in animation after the battery has been placed.
        /// </summary>
        public bool IsBatteryInContact { get; set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _lockInitialRotation = new Quaternion[_locks.Length];

            for (int i = 0; i < _locks.Length; ++i)
            {
                _lockInitialRotation[i] = _locks[i].localRotation;
            }
        }

        /// <summary>
        ///     Subscribes to the events that help model the behavior.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _batteryAnchor.Anchor.Placed      += Battery_Placed;
            _batteryAnchor.Anchor.Removed     += Battery_Removed;
            _grabbableLock.ConstraintsApplied += Lock_ConstraintsApplied;
        }

        /// <summary>
        ///     Unsubscribes from the events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            _batteryAnchor.Anchor.Placed      -= Battery_Placed;
            _batteryAnchor.Anchor.Removed     -= Battery_Removed;
            _grabbableLock.ConstraintsApplied -= Lock_ConstraintsApplied;
        }

        /// <summary>
        ///     Initializes the lock open state.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            IsBatteryInContact = _batteryAnchor.Anchor.CurrentPlacedObject != null;
            IsLockOpen         = _startLockOpen;
        }

        /// <summary>
        ///     Updates the ability of the door to let a battery be placed inside.
        /// </summary>
        private void Update()
        {
            // The battery door anchor is disabled if the lock is closed and there is no battery inside

            if (_batteryAnchor.Anchor.CurrentPlacedObject == null && !IsLockOpen)
            {
                _batteryAnchor.enabled = false;
            }
            else
            {
                _batteryAnchor.enabled = true;
            }
            
            ApplyLockConstraints();
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after the lock has finished being manipulated so that additional constraints can be applied to it.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Lock_ConstraintsApplied(object sender, UxrApplyConstraintsEventArgs e)
        {
            ApplyLockConstraints();
        }

        /// <summary>
        ///     Called right after a battery was placed inside.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Battery_Placed(object sender, UxrManipulationEventArgs e)
        {
            // To turn the lights on only when the battery reached the bottom, we control this from the Battery component.
        }

        /// <summary>
        ///     Called right after the battery was removed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Battery_Removed(object sender, UxrManipulationEventArgs e)
        {
            IsBatteryInContact = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Applies the lock constraints.
        /// </summary>
        private void ApplyLockConstraints()
        {
            float lockHandleOpenValue = LockHandleOpenValue;
            float locksOpenValue      = 1.0f - (1.0f - lockHandleOpenValue) * (1.0f - lockHandleOpenValue);

            // Update small locks based on the main lock open value

            for (int i = 0; i < _locks.Length; ++i)
            {
                _locks[i].transform.localRotation = _lockInitialRotation[i] * Quaternion.AngleAxis(locksOpenValue * _locksOpenRotation, Vector3.right);
            }

            // Main lock can be manipulated only while the battery is completely inside or there is no battery

            if (_batteryAnchor.Anchor.CurrentPlacedObject != null && _batteryAnchor.Anchor.CurrentPlacedObject.transform.localPosition.z > 0.01f)
            {
                _grabbableLock.IsLockedInPlace = true;
            }
            else
            {
                _grabbableLock.IsLockedInPlace = false;
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Returns a value between 0.0 and 1.0 telling how open the lock is.
        /// </summary>
        private float LockHandleOpenValue => _grabbableLock.SingleRotationAxisDegrees / (_grabbableLock.MaxSingleRotationDegrees - _grabbableLock.MinSingleRotationDegrees);

        private bool         _isBatteryInContact;
        private Quaternion[] _lockInitialRotation;

        #endregion
    }
}