// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrObjectFade.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.GameObjects
{
    /// <summary>
    ///     Component that allows to fade an object out by making the material progressively more transparent.
    /// </summary>
    public partial class UxrObjectFade : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool  _recursively = true;
        [SerializeField] private float _delaySeconds;
        [SerializeField] private float _duration      = 1.0f;
        [SerializeField] private float _startQuantity = 1.0f;
        [SerializeField] private float _endQuantity;
        [SerializeField] private bool  _useUnscaledTime;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts a fade animation.
        /// </summary>
        /// <param name="gameObject">GameObject whose material transparency will be enabled and animated.</param>
        /// <param name="startAlphaQuantity">Start alpha</param>
        /// <param name="endFadeQuantity">End alpha</param>
        /// <param name="delaySeconds">Seconds to wait before the animation starts</param>
        /// <param name="durationSeconds">Fade duration in seconds</param>
        /// <param name="recursively">Whether to also process all other child objects in the hierarchy</param>
        /// <param name="useUnscaledTime">
        ///     Whether to use unscaled time (<see cref="Time.unscaledTime" />) or not (
        ///     <see cref="Time.time" />)
        /// </param>
        /// <param name="finishedCallback">Optional callback executed when the animation finished</param>
        /// <returns>Animation component</returns>
        public static UxrObjectFade Fade(GameObject gameObject,
                                         float      startAlphaQuantity,
                                         float      endFadeQuantity,
                                         float      delaySeconds,
                                         float      durationSeconds,
                                         bool       recursively      = true,
                                         bool       useUnscaledTime  = false,
                                         Action     finishedCallback = null)
        {
            UxrObjectFade objectFade = gameObject.GetOrAddComponent<UxrObjectFade>();

            objectFade._startQuantity    = startAlphaQuantity;
            objectFade._endQuantity      = endFadeQuantity;
            objectFade._delaySeconds     = delaySeconds;
            objectFade._duration         = durationSeconds;
            objectFade._recursively      = recursively;
            objectFade._useUnscaledTime  = useUnscaledTime;
            objectFade._finishedCallback = finishedCallback;

            objectFade.CheckInitialize(true);

            return objectFade;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CheckInitialize();
        }

        /// <summary>
        ///     Starts or re-starts the animation.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _fadeStartTime = CurrentTime;
            _finished      = false;
        }

        /// <summary>
        ///     Stops the animation and restores the material.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (ObjectEntry objectEntry in _objects)
            {
                objectEntry.Restore();
            }
        }

        /// <summary>
        ///     Updates the animation.
        /// </summary>
        private void Update()
        {
            if (_finished)
            {
                return;
            }

            float fadeTime = CurrentTime - _fadeStartTime - _delaySeconds;

            if (fadeTime <= 0)
            {
                return;
            }

            float fadeT = Mathf.Clamp01(fadeTime / _duration);

            foreach (ObjectEntry entry in _objects)
            {
                entry.Fade(_startQuantity, _endQuantity, fadeT);
            }

            if (fadeTime > _duration)
            {
                _finishedCallback?.Invoke();
                _finished = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the component if necessary.
        /// </summary>
        /// <param name="forceInitialize">Forces initializing the component even if it already may have been initialized</param>
        private void CheckInitialize(bool forceInitialize = false)
        {
            if (_objects.Count == 0 || forceInitialize)
            {
                Renderer[] objectRenderers = _recursively ? gameObject.GetComponentsInChildren<Renderer>() : new[] { gameObject.GetComponent<Renderer>() };

                foreach (Renderer renderer in objectRenderers)
                {
                    _objects.Add(new ObjectEntry(renderer));
                }
            }
        }

        #endregion

        #region Private Types & Data

        private float CurrentTime => _useUnscaledTime ? Time.unscaledTime : Time.time;

        private readonly List<ObjectEntry> _objects = new List<ObjectEntry>();

        private float  _fadeStartTime;
        private bool   _finished;
        private Action _finishedCallback;

        #endregion
    }
}