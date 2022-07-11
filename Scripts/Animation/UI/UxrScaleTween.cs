// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrScaleTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Tweening component to animate <see cref="RectTransform.localScale" /> programatically or using the inspector.
    /// </summary>
    public class UxrScaleTween : UxrGraphicTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Vector3 _startScale;
        [SerializeField] private Vector3 _endScale;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Animation start scale
        /// </summary>
        public Vector3 StartScale
        {
            get => _startScale;
            set => _startScale = value;
        }

        /// <summary>
        ///     Animation end scale
        /// </summary>
        public Vector3 EndScale
        {
            get => _endScale;
            set => _endScale = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a scale tweening animation for the <see cref="RectTransform.localScale" /> of a Unity
        ///     UI <see cref="Graphic" /> component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="startScale">Start local scale</param>
        /// <param name="endScale">End local scale</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrScaleTween Animate(Graphic graphic, Vector3 startScale, Vector3 endScale, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrScaleTween scaleTween = graphic.GetOrAddComponent<UxrScaleTween>();

            scaleTween.StartScale            = startScale;
            scaleTween.EndScale              = endScale;
            scaleTween.InterpolationSettings = settings;
            scaleTween.FinishedCallback      = finishedCallback;
            scaleTween.Restart();

            return scaleTween;
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            RestoreLocalScale();
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetRectTransform.localScale = Vector2.Lerp(StartScale, EndScale, t);
        }

        #endregion
    }
}