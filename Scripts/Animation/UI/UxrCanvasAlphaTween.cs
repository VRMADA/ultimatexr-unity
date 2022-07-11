// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCanvasAlphaTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.System.Threading;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Tweening component to animate the <see cref="CanvasGroup.alpha" /> of a <see cref="CanvasGroup" /> component
    ///     programatically or using the inspector.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UxrCanvasAlphaTween : UxrTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _startAlpha;
        [SerializeField] private float _endAlpha;

        #endregion

        #region Public Types & Data

        public CanvasGroup TargetCanvasGroup => GetCachedComponent<CanvasGroup>();

        /// <summary>
        ///     Animation start alpha
        /// </summary>
        public float StartAlpha
        {
            get => _startAlpha;
            set => _startAlpha = value;
        }

        /// <summary>
        ///     Animation end alpha
        /// </summary>
        public float EndAlpha
        {
            get => _endAlpha;
            set => _endAlpha = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a tweening animation for the <see cref="CanvasGroup.alpha" /> value of a
        ///     <see cref="CanvasGroup" /> component.
        /// </summary>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="startAlpha">Start alpha</param>
        /// <param name="endAlpha">End alpha</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrCanvasAlphaTween Animate(CanvasGroup canvasGroup, float startAlpha, float endAlpha, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrCanvasAlphaTween canvasAlphaTween = canvasGroup.GetOrAddComponent<UxrCanvasAlphaTween>();

            canvasAlphaTween.StartAlpha            = startAlpha;
            canvasAlphaTween.EndAlpha              = endAlpha;
            canvasAlphaTween.InterpolationSettings = settings;
            canvasAlphaTween.FinishedCallback      = finishedCallback;
            canvasAlphaTween.Restart();

            return canvasAlphaTween;
        }

        /// <summary>
        ///     Creates and starts a fade-in tweening animation for the <see cref="CanvasGroup.alpha" /> value of a
        ///     <see cref="CanvasGroup" /> component. The alpha value will go from 0.0 to 1.0.
        /// </summary>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="durationSeconds">Duration in seconds of the fade-in animation</param>
        /// <param name="delaySeconds">Seconds to wait until the animation starts</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrCanvasAlphaTween FadeIn(CanvasGroup canvasGroup, float durationSeconds, float delaySeconds = 0.0f, Action<UxrTween> finishedCallback = null)
        {
            return Animate(canvasGroup, 0.0f, 1.0f, new UxrInterpolationSettings(durationSeconds, delaySeconds), finishedCallback);
        }

        /// <summary>
        ///     Creates and starts a fade-out tweening animation for the <see cref="CanvasGroup.alpha" /> value of a
        ///     <see cref="CanvasGroup" /> component. The alpha value will go from 1.0 to 0.0.
        /// </summary>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="durationSeconds">Duration in seconds of the fade-in animation</param>
        /// <param name="delaySeconds">Seconds to wait until the animation starts</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrCanvasAlphaTween FadeOut(CanvasGroup canvasGroup, float durationSeconds, float delaySeconds = 0.0f, Action<UxrTween> finishedCallback = null)
        {
            return Animate(canvasGroup, 1.0f, 0.0f, new UxrInterpolationSettings(durationSeconds, delaySeconds), finishedCallback);
        }

        /// <summary>
        ///     Same as <see cref="Animate" /> but for use with async/await.
        /// </summary>
        /// <param name="ct">
        ///     Cancellation token to cancel the asynchronous animation. <see cref="CancellationToken.None" /> to
        ///     ignore.
        /// </param>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="startAlpha">Start alpha</param>
        /// <param name="endAlpha">End alpha</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <returns>
        ///     Task representing the asynchronous process.
        /// </returns>
        public static Task AnimateAsync(CancellationToken ct, CanvasGroup canvasGroup, float startAlpha, float endAlpha, UxrInterpolationSettings settings)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            bool hasFinishedFading = false;

            UxrCanvasAlphaTween canvasAlphaTween = Animate(canvasGroup, startAlpha, endAlpha, settings, t => hasFinishedFading = true);
            return TaskExt.WaitUntil(() => hasFinishedFading, settings.DelaySeconds + settings.DurationSeconds, () => canvasAlphaTween.Stop(), ct);
        }

        /// <summary>
        ///     Same as <see cref="FadeIn" /> but for use with async/await.
        /// </summary>
        /// <param name="ct">
        ///     Cancellation token to cancel the asynchronous animation. <see cref="CancellationToken.None" /> to
        ///     ignore.
        /// </param>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="durationSeconds">Duration in seconds of the fade-in animation</param>
        /// <param name="delaySeconds">Seconds to wait until the animation starts</param>
        /// <returns>
        ///     Task representing the asynchronous process.
        /// </returns>
        public static Task FadeInAsync(CancellationToken ct, CanvasGroup canvasGroup, float durationSeconds, float delaySeconds = 0.0f)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            bool hasFinishedFading = false;

            UxrCanvasAlphaTween canvasAlphaTween = FadeIn(canvasGroup, durationSeconds, delaySeconds, t => hasFinishedFading = true);
            return TaskExt.WaitUntil(() => hasFinishedFading, delaySeconds + durationSeconds, () => canvasAlphaTween.Stop(), ct);
        }

        /// <summary>
        ///     Same as <see cref="FadeOut" /> but for use with async/await.
        /// </summary>
        /// <param name="ct">
        ///     Cancellation token to cancel the asynchronous animation. <see cref="CancellationToken.None" /> to
        ///     ignore.
        /// </param>
        /// <param name="canvasGroup">Target <see cref="CanvasGroup" /></param>
        /// <param name="durationSeconds">Duration in seconds of the fade-in animation</param>
        /// <param name="delaySeconds">Seconds to wait until the animation starts</param>
        /// <returns>
        ///     Task representing the asynchronous process.
        /// </returns>
        public static Task FadeOutAsync(CancellationToken ct, CanvasGroup canvasGroup, float durationSeconds, float delaySeconds = 0.0f)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            bool hasFinishedFading = false;

            UxrCanvasAlphaTween canvasAlphaTween = FadeOut(canvasGroup, durationSeconds, delaySeconds, t => hasFinishedFading = true);
            return TaskExt.WaitUntil(() => hasFinishedFading, delaySeconds + durationSeconds, () => canvasAlphaTween.Stop(), ct);
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override Behaviour TargetBehaviour => TargetCanvasGroup;

        /// <inheritdoc />
        protected override void StoreOriginalValue()
        {
            _originalAlpha = TargetCanvasGroup.alpha;
        }

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            TargetCanvasGroup.alpha = _originalAlpha;
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetCanvasGroup.alpha = Mathf.Lerp(StartAlpha, EndAlpha, t);
        }

        #endregion

        #region Private Types & Data

        private float     _originalAlpha;
        private Behaviour _targetBehaviour;

        #endregion
    }
}