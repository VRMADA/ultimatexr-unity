// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpringOnRelease.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Adds a spring behavior to an object whenever it is released.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public class SpringOnRelease : UxrGrabbableObjectComponent<SpringOnRelease>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _springAmplitudeMultiplier;
        [SerializeField] private float _springMaxAmplitude;
        [SerializeField] private float _springRotAmplitudeMultiplier;
        [SerializeField] private float _springMaxRotAmplitude;
        [SerializeField] private float _springDuration;
        [SerializeField] private float _springFrequency;

        #endregion

        #region Unity

        /// <summary>
        ///     Updates the spring if it is currently active.
        /// </summary>
        private void Update()
        {
            if (_timer > 0.0f)
            {
                _timer -= Time.deltaTime;

                if (_timer < 0.0f)
                {
                    transform.position    = _releasePosition;
                    transform.eulerAngles = _releaseEuler;
                }
                else
                {
                    float   t     = (_springDuration - _timer) / _springDuration;
                    Vector3 delta = _filteredVelocity * Mathf.Sin(-(_springDuration - _timer) * Mathf.PI * 2.0f * _springFrequency) * (1.0f - t);
                    transform.position = _releasePosition + delta;

                    Vector3 deltaEuler = _filteredAngularVelocity * Mathf.Sin(-(_springDuration - _timer) * Mathf.PI * 2.0f * _springFrequency) * (1.0f - t);
                    transform.eulerAngles = _releaseEuler + deltaEuler;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called right after the object was grabbed.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            _timer = -1.0f;
        }

        /// <summary>
        ///     Called right after the object was released.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            if (e.IsOwnershipChanged)
            {
                _releasePosition        = transform.position;
                _releaseEuler           = transform.eulerAngles;
                _releaseVelocity        = e.ReleaseVelocity;
                _releaseAngularVelocity = e.ReleaseAngularVelocity;
                _timer                  = _springDuration;

                _filteredVelocity = _releaseVelocity * _springAmplitudeMultiplier;

                if (_filteredVelocity.magnitude > _springMaxAmplitude)
                {
                    _filteredVelocity = _filteredVelocity.normalized * _springMaxAmplitude;
                }

                _filteredAngularVelocity = _releaseAngularVelocity * _springRotAmplitudeMultiplier;

                if (Mathf.Abs(_filteredAngularVelocity.x) > _springMaxRotAmplitude)
                {
                    _filteredAngularVelocity.x = _filteredAngularVelocity.x > 0.0f ? _springMaxRotAmplitude : -_springMaxRotAmplitude;
                }
                if (Mathf.Abs(_filteredAngularVelocity.y) > _springMaxRotAmplitude)
                {
                    _filteredAngularVelocity.y = _filteredAngularVelocity.y > 0.0f ? _springMaxRotAmplitude : -_springMaxRotAmplitude;
                }
                if (Mathf.Abs(_filteredAngularVelocity.z) > _springMaxRotAmplitude)
                {
                    _filteredAngularVelocity.z = _filteredAngularVelocity.z > 0.0f ? _springMaxRotAmplitude : -_springMaxRotAmplitude;
                }
            }
        }

        #endregion

        #region Private Types & Data

        private Vector3 _releasePosition;
        private Vector3 _releaseEuler;
        private Vector3 _releaseVelocity;
        private Vector3 _releaseAngularVelocity;
        private Vector3 _filteredVelocity;
        private Vector3 _filteredAngularVelocity;
        private float   _timer;

        #endregion
    }
}