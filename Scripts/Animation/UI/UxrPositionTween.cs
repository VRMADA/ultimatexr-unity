// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPositionTween.cs" company="VRMADA">
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
    ///     Tweening component to animate <see cref="RectTransform.anchoredPosition" /> programatically or using the inspector.
    /// </summary>
    public class UxrPositionTween : UxrGraphicTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Vector2 _startPosition;
        [SerializeField] private Vector2 _endPosition;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Animation start position
        /// </summary>
        public Vector2 StartPosition
        {
            get => _startPosition;
            set => _startPosition = value;
        }

        /// <summary>
        ///     Animation end position
        /// </summary>
        public Vector2 EndPosition
        {
            get => _endPosition;
            set => _endPosition = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a position tweening animation for the <see cref="RectTransform.anchoredPosition" /> of a Unity
        ///     UI <see cref="Graphic" /> component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="startPosition">Start position</param>
        /// <param name="endPosition">End position</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrPositionTween Animate(Graphic graphic, Vector2 startPosition, Vector2 endPosition, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrPositionTween positionTween = graphic.GetOrAddComponent<UxrPositionTween>();

            positionTween.StartPosition         = startPosition;
            positionTween.EndPosition           = endPosition;
            positionTween.InterpolationSettings = settings;
            positionTween.FinishedCallback      = finishedCallback;
            positionTween.Restart();

            return positionTween;
        }

        /// <summary>
        ///     Creates and starts a position tweening animation for the <see cref="RectTransform.anchoredPosition" /> of a Unity
        ///     UI <see cref="Graphic" /> component. The animation will start using an offset with respect to the initial graphic
        ///     position and will end in the initial graphic position.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="horizontalOffset">Horizontal offset to the initial x position where the animation will start.</param>
        /// <param name="verticalOffset">Vertical offset to the initial y position where the animation will start.</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrPositionTween MoveIn(Graphic graphic, float horizontalOffset, float verticalOffset, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrPositionTween positionTween = graphic.GetOrAddComponent<UxrPositionTween>();

            positionTween.RestoreOriginalValue();
            positionTween.StartPosition         = positionTween.TargetRectTransform.anchoredPosition + new Vector2(horizontalOffset, verticalOffset);
            positionTween.EndPosition           = positionTween.TargetRectTransform.anchoredPosition;
            positionTween.InterpolationSettings = settings;
            positionTween.FinishedCallback      = finishedCallback;
            positionTween.Restart();

            return positionTween;
        }

        /// <summary>
        ///     Creates and starts a position tweening animation for the <see cref="RectTransform.anchoredPosition" /> of a Unity
        ///     UI <see cref="Graphic" /> component. The animation will start in the initial graphic position and end with an
        ///     offset with respect to the initial graphic position.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="horizontalOffset">Horizontal offset to the initial x position where the animation will end.</param>
        /// <param name="verticalOffset">Vertical offset to the initial y position where the animation will end.</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrPositionTween MoveOut(Graphic graphic, float horizontalOffset, float verticalOffset, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrPositionTween positionTween = graphic.GetOrAddComponent<UxrPositionTween>();

            positionTween.RestoreOriginalValue();
            positionTween.StartPosition         = positionTween.TargetRectTransform.anchoredPosition;
            positionTween.EndPosition           = positionTween.TargetRectTransform.anchoredPosition + new Vector2(horizontalOffset, verticalOffset);
            positionTween.InterpolationSettings = settings;
            positionTween.FinishedCallback      = finishedCallback;
            positionTween.Restart();

            return positionTween;
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            RestoreAnchoredPosition();
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetRectTransform.anchoredPosition = Vector2.Lerp(StartPosition, EndPosition, t);
        }

        #endregion
    }
}