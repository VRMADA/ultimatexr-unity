// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomaticDoor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UltimateXR.Audio;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Doors
{
    /// <summary>
    ///     Component to model de behavior of a door that opens automatically when the user gets near and closes
    ///     when the user moves away from it.
    /// </summary>
    public class AutomaticDoor : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform      _floorCenter;
        [SerializeField] private Transform      _leftDoor;
        [SerializeField] private Transform      _rightDoor;
        [SerializeField] private Vector3        _leftOpenLocalOffset;
        [SerializeField] private Vector3        _rightOpenLocalOffset;
        [SerializeField] private float          _openDelaySeconds;
        [SerializeField] private float          _openDurationSeconds = 0.8f;
        [SerializeField] private float          _openDistance        = 1.5f;
        [SerializeField] private float          _closeDistance       = 2.0f;
        [SerializeField] private UxrEasing      _openEasing          = UxrEasing.EaseOutCubic;
        [SerializeField] private UxrEasing      _closeEasing         = UxrEasing.EaseInCubic;
        [SerializeField] private UxrAudioSample _audioOpen;
        [SerializeField] private UxrAudioSample _audioClose;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a value from 0.0 (completely closed) to 1.0 (completely open) telling how open the door currently is.
        /// </summary>
        public float OpenValue { get; private set; }

        /// <summary>
        ///     Gets if the door is open or opening.
        /// </summary>
        public bool IsOpen { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Forces to open the door. This can be used in child implementations where opening can be disallowed under certain
        ///     conditions. See <see cref="ArmoredDoor" /> for an example.
        /// </summary>
        public void OpenDoor()
        {
            IsOpen = true;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stores initial state.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _leftStartLocalPosition  = _leftDoor.localPosition;
            _rightStartLocalPosition = _rightDoor.localPosition;
        }

        /// <summary>
        ///     Updates the door.
        /// </summary>
        private void Update()
        {
            if (UxrAvatar.LocalAvatar == null)
            {
                return;
            }

            // Check distance to door

            float distance = Vector3.Distance(UxrAvatar.LocalAvatar.CameraFloorPosition, FloorCenter.position);

            if (distance < _openDistance && Mathf.Approximately(OpenValue, 0.0f))
            {
                _openDelayTimer += Time.deltaTime;

                if (_openDelayTimer > _openDelaySeconds && IsOpeningAllowed)
                {
                    // Within opening distance, door completely closed and opening allowed: open door
                    IsOpen = true;
                    _audioOpen.Play(FloorCenter.position);
                }
            }
            else if (distance > _closeDistance && Mathf.Approximately(OpenValue, 1.0f))
            {
                // Over closing distance and door completely open: close door
                IsOpen          = false;
                _openDelayTimer = 0.0f;
                _audioClose.Play(FloorCenter.position);
            }

            // Update timer and perform interpolation

            OpenValue = Mathf.Clamp01(OpenValue + Time.deltaTime * (1.0f / _openDurationSeconds) * (IsOpen ? 1.0f : -1.0f));
            float t = UxrInterpolator.GetInterpolationFactor(OpenValue, IsOpen ? _openEasing : _closeEasing);

            _leftDoor.transform.localPosition  = Vector3.Lerp(_leftStartLocalPosition,  _leftStartLocalPosition + _leftOpenLocalOffset,   t);
            _rightDoor.transform.localPosition = Vector3.Lerp(_rightStartLocalPosition, _rightStartLocalPosition + _rightOpenLocalOffset, t);
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets if opening is allowed. Can be used by child implementations to disallow opening under certain conditions. See
        ///     <see cref="ArmoredDoor" /> for an example.
        /// </summary>
        protected virtual bool IsOpeningAllowed => true;

        /// <summary>
        ///     Gets the door center at floor level.
        /// </summary>
        protected Transform FloorCenter => _floorCenter != null ? _floorCenter : transform;

        #endregion

        #region Private Types & Data

        private float   _openDelayTimer;
        private Vector3 _leftStartLocalPosition;
        private Vector3 _rightStartLocalPosition;

        #endregion
    }
}