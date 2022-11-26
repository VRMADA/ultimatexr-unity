// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation.Components
{
    /// <summary>
    ///     Component that allows to toggle components enabled state back and forth at random times.
    /// </summary>
    public class UxrToggleComponent : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private MonoBehaviour[] _components;
        [SerializeField] private float           _enabledDurationMin;
        [SerializeField] private float           _enabledDurationMax;
        [SerializeField] private float           _disabledDurationMin;
        [SerializeField] private float           _disabledDurationMax;
        [SerializeField] private bool            _useUnscaledTime;

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
        ///     Called on each update. Checks if it is time to toggle the components.
        /// </summary>
        private void Update()
        {
            float time = (_useUnscaledTime ? Time.unscaledTime : Time.time) - _startTime;

            if (time > _nextToggleTime)
            {
                foreach (MonoBehaviour component in _components)
                {
                    component.enabled = !component.enabled;
                }

                _startTime      = _useUnscaledTime ? Time.unscaledTime : Time.time;
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
            if (_components.Length > 0 && _components[0].enabled)
            {
                return Random.Range(_enabledDurationMin, _enabledDurationMax);
            }
            if (_components.Length > 0 && !_components[0].enabled)
            {
                return Random.Range(_disabledDurationMin, _disabledDurationMax);
            }

            return 0.0f;
        }

        #endregion

        #region Private Types & Data

        private float _startTime;
        private float _nextToggleTime;

        #endregion
    }
}