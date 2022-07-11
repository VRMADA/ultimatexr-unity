// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleEmitParticles.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;

namespace UltimateXR.Animation.ParticleSystems
{
    /// <summary>
    ///     Component that allows to toggle particle emission enabled state back and forth at random times.
    /// </summary>
    public class UxrToggleEmitParticles : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private GameObject[]   _toggleAdditionalGameObjects;
        [SerializeField] private float          _enabledDurationMin;
        [SerializeField] private float          _enabledDurationMax;
        [SerializeField] private float          _disabledDurationMin;
        [SerializeField] private float          _disabledDurationMax;
        [SerializeField] private bool           _useUnscaledTime;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     The particle system to toggle.
        /// </summary>
        public ParticleSystem TargetParticleSystem
        {
            get => _particleSystem;
            set => _particleSystem = value;
        }

        /// <summary>
        ///     Additional objects whose active state is toggled too.
        /// </summary>
        public GameObject[] ToggleAdditionalGameObjects
        {
            get => _toggleAdditionalGameObjects;
            set => _toggleAdditionalGameObjects = value;
        }

        /// <summary>
        ///     The minimum amount of seconds the emission will be enabled when toggled on.
        /// </summary>
        public float EnabledSecondsMin
        {
            get => _enabledDurationMin;
            set => _enabledDurationMin = value;
        }

        /// <summary>
        ///     The maximum amount of seconds the emission will be enabled when toggled on.
        /// </summary>
        public float EnabledSecondsMax
        {
            get => _enabledDurationMax;
            set => _enabledDurationMax = value;
        }

        /// <summary>
        ///     The minimum amount of seconds the emission will be disabled when toggled off.
        /// </summary>
        public float DisabledSecondsMin
        {
            get => _disabledDurationMin;
            set => _disabledDurationMin = value;
        }

        /// <summary>
        ///     The minimum amount of seconds the emission will be disabled when toggled off.
        /// </summary>
        public float DisabledSecondsMax
        {
            get => _disabledDurationMax;
            set => _disabledDurationMax = value;
        }

        /// <summary>
        ///     Whether to use <see cref="Time.time" /> or <see cref="Time.unscaledTime" /> for timing.
        /// </summary>
        public bool UseUnscaledTime
        {
            get => _useUnscaledTime;
            set => _useUnscaledTime = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called each time the component is enabled. Sets up the next toggle time.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _startTime      = _useUnscaledTime ? Time.unscaledTime : Time.time;
            _nextToggleTime = GetNextRelativeToggleTime();
        }

        /// <summary>
        ///     Called on each update. Checks if it is time to toggle the emission state.
        /// </summary>
        private void Update()
        {
            float time = CurrentTime - _startTime;

            if (time > _nextToggleTime)
            {
                // Toggle GameObjects

                _toggleAdditionalGameObjects.ForEach(go => go.SetActive(!go.activeSelf));

                // Toggle particle emission

                if (_particleSystem != null)
                {
                    ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;
                    emissionModule.enabled = !emissionModule.enabled;
                }

                // Setup next toggle time

                _startTime      = CurrentTime;
                _nextToggleTime = GetNextRelativeToggleTime();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the next time the components will be toggled
        /// </summary>
        /// <returns>Next toggle time in seconds relative to the current time</returns>
        private float GetNextRelativeToggleTime()
        {
            if (_particleSystem != null && _particleSystem.emission.enabled)
            {
                return Random.Range(_enabledDurationMin, _enabledDurationMax);
            }
            if (_particleSystem != null && !_particleSystem.emission.enabled)
            {
                return Random.Range(_disabledDurationMin, _disabledDurationMax);
            }

            return 0.0f;
        }

        #endregion

        #region Private Types & Data

        private float CurrentTime => _useUnscaledTime ? Time.unscaledTime : Time.time;

        private float _startTime;
        private float _nextToggleTime;

        #endregion
    }
}