// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Attributes;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     <para>
    ///         Base abstract class to create tweening components to animate Unity UI elements.
    ///     </para>
    ///     <para>
    ///         Tweens are <see cref="UxrComponent" /> components to allow access to the global list of tweens
    ///         or filter by type.
    ///     </para>
    ///     <para>
    ///         They are also <see cref="UxrComponent{TP,TC}" /> to allow access to the global list of tweens in a
    ///         given parent canvas.
    ///     </para>
    /// </summary>
    public abstract class UxrTween : UxrComponent<Canvas, UxrTween>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] [ReadOnly(HideInEditMode = true)] private bool                     _hasFinished;
        [SerializeField]                                   private UxrInterpolationSettings _interpolationSettings = new UxrInterpolationSettings();
        [SerializeField]                                   private UxrTweenFinishedActions  _finishedActions       = UxrTweenFinishedActions.None;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when the animation finished.
        /// </summary>
        public event Action Finished;

        /// <summary>
        ///     Gets the current animation time in seconds. The animation time is the scaled or unscaled time relative to the time
        ///     the component was enabled.
        /// </summary>
        public float AnimationTime => CurrentTime - _startTime;

        /// <summary>
        ///     Gets or sets the interpolation settings.
        /// </summary>
        public UxrInterpolationSettings InterpolationSettings
        {
            get => _interpolationSettings;
            protected set => _interpolationSettings = value;
        }

        /// <summary>
        ///     Gets whether the animation finished.
        /// </summary>
        public bool HasFinished
        {
            get => _hasFinished;
            private set => _hasFinished = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the given behaviour has a running tween of a specific type.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="UxrTween" />s to check for.</typeparam>
        /// <returns>Whether there is a running animation of the given type.</returns>
        public static bool HasActiveTween<T>(Behaviour behaviour) where T : UxrTween
        {
            T tween = behaviour.GetComponent<T>();
            return tween && !tween.HasFinished;
        }

        /// <summary>
        ///     Stops all enabled tweens.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset each animated component to the state before its animation started</param>
        public static void StopAll(bool restoreOriginal = true)
        {
            EnabledComponents.ForEach(t => t.Stop(restoreOriginal));
        }

        /// <summary>
        ///     Stops all enabled tweens of a given type.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset each animated component to the state before its animation started</param>
        /// <typeparam name="T">Type of <see cref="UxrTween" />s to stop</typeparam>
        public static void StopAll<T>(bool restoreOriginal = true) where T : UxrTween
        {
            EnabledComponents.OfType<T>().ForEach(t => t.Stop(restoreOriginal));
        }

        /// <summary>
        ///     Stops all enabled tweens that are in a given canvas.
        /// </summary>
        /// <param name="canvas">Canvas to disable all enabled tweens from</param>
        /// <param name="restoreOriginal">Whether to reset each animated component to the state before its animation started</param>
        public static void StopAllInParentCanvas(Canvas canvas, bool restoreOriginal = true)
        {
            GetParentChildren(canvas).ForEach(t => t.Stop(restoreOriginal));
        }

        /// <summary>
        ///     Stops all enabled tweens of a given type that are in a given canvas.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset each animated component to the state before its animation started</param>
        /// <param name="canvas">Canvas to disable all enabled tweens from</param>
        /// <typeparam name="T">Type of <see cref="UxrTween" />s to stop</typeparam>
        public static void StopAllInParentCanvas<T>(Canvas canvas, bool restoreOriginal = true) where T : UxrTween
        {
            GetParentChildren(canvas).OfType<T>().ForEach(t => t.Stop(restoreOriginal));
        }

        /// <summary>
        ///     Stops all the tweening components of a <see cref="Behaviour" />.
        /// </summary>
        /// <param name="behaviour"><see cref="Behaviour" /> whose tweens to stop</param>
        /// <param name="restoreOriginal">Whether to reset each animated component to the state before its animation started</param>
        public static void StopAll(Behaviour behaviour, bool restoreOriginal = true)
        {
            foreach (UxrTween tween in behaviour.gameObject.GetComponents<UxrTween>())
            {
                tween.Stop(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the tweening animation on an object if it has a <typeparamref name="T" /> component currently added.
        /// </summary>
        /// <param name="behaviour">UI Component whose GameObject has the tween added</param>
        /// <param name="restoreOriginal">Whether to reset the animated component to the state before the animation started</param>
        /// <typeparam name="T">Type of <see cref="UxrTween" /> to stop</typeparam>
        public static void Stop<T>(Behaviour behaviour, bool restoreOriginal = true) where T : UxrTween
        {
            T tween = behaviour.GetComponent<T>();

            if (tween)
            {
                tween.Stop(restoreOriginal);
            }
        }

        /// <summary>
        ///     Stops the tweening animation.
        /// </summary>
        /// <param name="restoreOriginal">Whether to reset the animated component to the state before the animation started</param>
        public void Stop(bool restoreOriginal = true)
        {
            HasFinished = true;

            if (restoreOriginal)
            {
                RestoreOriginalValue();
            }
        }

        /// <summary>
        ///     Sets the actions to perform when the animation finished.
        /// </summary>
        /// <param name="actions">Action flags</param>
        /// <returns>The <see cref="UxrTween" /> component to concatenate additional calls if desired.</returns>
        public UxrTween SetFinishedActions(UxrTweenFinishedActions actions)
        {
            _finishedActions = actions;
            return this;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stores the start time each time the component is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _startTime = CurrentTime;

            if (!HasOriginalValueStored)
            {
                HasOriginalValueStored = true;
                StoreOriginalValue();
            }

            if (InterpolationSettings != null)
            {
                Interpolate(InterpolationSettings.GetInterpolationFactor(0.0f));
            }

            HasFinished = false;
        }

        /// <summary>
        ///     Updates the interpolation.
        /// </summary>
        private void Update()
        {
            if (HasFinished || InterpolationSettings == null)
            {
                return;
            }

            Interpolate(InterpolationSettings.GetInterpolationFactor(AnimationTime));

            if (InterpolationSettings.CheckInterpolationHasFinished(AnimationTime))
            {
                Interpolate(1.0f);
                HasFinished = true;
                OnFinished();
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="Finished" /> event.
        /// </summary>
        protected virtual void OnFinished()
        {
            Finished?.Invoke();
            FinishedCallback?.Invoke(this);

            if (_finishedActions.HasFlag(UxrTweenFinishedActions.RestoreOriginalValue))
            {
                RestoreOriginalValue();
            }
            if (_finishedActions.HasFlag(UxrTweenFinishedActions.DisableTargetComponent) && TargetBehaviour)
            {
                TargetBehaviour.enabled = false;
            }
            if (_finishedActions.HasFlag(UxrTweenFinishedActions.DeactivateGameObject) && TargetBehaviour && TargetBehaviour.gameObject)
            {
                TargetBehaviour.gameObject.SetActive(false);
            }
            if (_finishedActions.HasFlag(UxrTweenFinishedActions.DestroyTween))
            {
                Destroy(this);
            }
            if (_finishedActions.HasFlag(UxrTweenFinishedActions.DestroyTargetComponent) && TargetBehaviour)
            {
                Destroy(TargetBehaviour);
            }
            if (_finishedActions.HasFlag(UxrTweenFinishedActions.DestroyGameObject))
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Restores the animated component to the state before the animation started.
        /// </summary>
        protected abstract void RestoreOriginalValue();

        /// <summary>
        ///     Stores the original value before the animation, in order to be able to restore it later using
        ///     <see cref="RestoreOriginalValue" />.
        /// </summary>
        protected abstract void StoreOriginalValue();

        /// <summary>
        ///     Interpolates and assigns the value corresponding to the given LERP value.
        /// </summary>
        /// <param name="t">LERP interpolation t value [0.0, 1.0]</param>
        protected abstract void Interpolate(float t);

        /// <summary>
        ///     Restarts the animation with the current parameters.
        /// </summary>
        protected void Restart()
        {
            if (InterpolationSettings != null)
            {
                _startTime       = CurrentTime;
                _finishedActions = UxrTweenFinishedActions.None;
                HasFinished      = false;
            }
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets the <see cref="Behaviour" /> the tween animates.
        /// </summary>
        protected abstract Behaviour TargetBehaviour { get; }

        /// <summary>
        ///     Gets if the tween has gathered the original animated parameter value.
        /// </summary>
        protected bool HasOriginalValueStored { get; private set; }

        /// <summary>
        ///     Optional finished callback assigned by child classes.
        /// </summary>
        protected Action<UxrTween> FinishedCallback { get; set; }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the current time in seconds. It computes the correct time, either <see cref="Time.unscaledTime" /> or
        ///     <see cref="Time.time" />, depending on the animation configuration.
        /// </summary>
        private float CurrentTime
        {
            get
            {
                if (InterpolationSettings != null)
                {
                    return InterpolationSettings.UseUnscaledTime ? Time.unscaledTime : Time.time;
                }

                return Time.time;
            }
        }

        private float _startTime;

        #endregion
    }
}