// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrImageFillTween.cs" company="VRMADA">
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
    ///     Tweening component to animate the <see cref="Image.fillAmount" /> of an an <see cref="Image" /> component
    ///     programatically or using the inspector.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UxrImageFillTween : UxrGraphicTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _startFillAmount;
        [SerializeField] private float _endFillAmount;

        #endregion

        #region Public Types & Data

        public Image TargetImage => GetCachedComponent<Image>();

        /// <summary>
        ///     Animation start fill amount
        /// </summary>
        public float StartFillAmount
        {
            get => _startFillAmount;
            set => _startFillAmount = value;
        }

        /// <summary>
        ///     Animation end fill amount
        /// </summary>
        public float EndFillAmount
        {
            get => _endFillAmount;
            set => _endFillAmount = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a tweening animation for the <see cref="Image.fillAmount" /> value in an <see cref="Image" />
        ///     component.
        /// </summary>
        /// <param name="image">Target image</param>
        /// <param name="startFillAmount">Start fill amount</param>
        /// <param name="endFillAmount">End fill amount</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrImageFillTween Animate(Image image, float startFillAmount, float endFillAmount, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrImageFillTween imageFillTween = image.GetOrAddComponent<UxrImageFillTween>();

            imageFillTween.StartFillAmount       = startFillAmount;
            imageFillTween.EndFillAmount         = endFillAmount;
            imageFillTween.InterpolationSettings = settings;
            imageFillTween.FinishedCallback      = finishedCallback;
            imageFillTween.Restart();

            return imageFillTween;
        }

        #endregion

        #region Protected Overrides UxrGraphicTween

        /// <inheritdoc />
        protected override void StoreOriginalValue()
        {
            _originalFillAmount = TargetImage.fillAmount;
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            if (HasOriginalValueStored)
            {
                TargetImage.fillAmount = _originalFillAmount;
            }
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetImage.fillAmount = Mathf.Lerp(StartFillAmount, EndFillAmount, t);
        }

        #endregion

        #region Private Types & Data

        private float _originalFillAmount;

        #endregion
    }
}