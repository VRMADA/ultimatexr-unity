// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrColorTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Tweening component to animate the <see cref="Graphic.color" /> of a <see cref="Graphic" /> component
    ///     programatically or using the inspector.
    /// </summary>
    public class UxrColorTween : UxrGraphicTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Color _startColor;
        [SerializeField] private Color _endColor;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Animation start color
        /// </summary>
        public Color StartColor
        {
            get => _startColor;
            set => _startColor = value;
        }

        /// <summary>
        ///     Animation end color
        /// </summary>
        public Color EndColor
        {
            get => _endColor;
            set => _endColor = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a color tweening animation for the <see cref="Graphic.color" /> of a Unity UI component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="startColor">Start color</param>
        /// <param name="endColor">End color</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrColorTween Animate(Graphic graphic, Color startColor, Color endColor, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrColorTween colorTween = graphic.GetOrAddComponent<UxrColorTween>();

            colorTween.StartColor            = startColor;
            colorTween.EndColor              = endColor;
            colorTween.InterpolationSettings = settings;
            colorTween.FinishedCallback      = finishedCallback;
            colorTween.Restart();

            return colorTween;
        }

        /// <summary>
        ///     Creates and starts an alpha tweening animation for the <see cref="Graphic.color" /> of a Unity UI component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="startAlpha">Start alpha</param>
        /// <param name="endAlpha">End alpha</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrColorTween AnimateAlpha(Graphic graphic, float startAlpha, float endAlpha, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            return Animate(graphic, graphic.color.WithAlpha(startAlpha), graphic.color.WithAlpha(endAlpha), settings, finishedCallback);
        }

        /// <summary>
        ///     Creates and starts an alpha blinking tweening animation for the <see cref="Graphic.color" /> of a Unity UI
        ///     component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="blinksPerSecond">Number of blinks per second</param>
        /// <param name="durationSeconds">Total duration of the animation in seconds</param>
        /// <param name="restoreWhenFinished">Whether to restore the original color after the animation finished</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <param name="delaySeconds"></param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrColorTween AnimateBlinkAlpha(Graphic graphic, float blinksPerSecond, float durationSeconds, float delaySeconds = 0.0f, bool restoreWhenFinished = true, Action<UxrTween> finishedCallback = null)
        {
            return Animate(graphic,
                           graphic.color.WithAlpha(1.0f),
                           graphic.color.WithAlpha(0.0f),
                           new UxrInterpolationSettings(1.0f / (blinksPerSecond * 2.0f), delaySeconds, UxrEasing.Linear, UxrLoopMode.PingPong, durationSeconds),
                           t =>
                           {
                               if (restoreWhenFinished && t is UxrColorTween colorTween)
                               {
                                   colorTween.RestoreOriginalValue();
                               }
                               finishedCallback?.Invoke(t);
                           });
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            RestoreColor();
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetGraphic.color = Color.Lerp(StartColor, EndColor, t);
        }

        #endregion
    }
}