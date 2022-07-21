// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRotationTween.cs" company="VRMADA">
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
    ///     Tweening component to animate <see cref="RectTransform.localRotation" /> programatically or using the inspector.
    /// </summary>
    public class UxrRotationTween : UxrGraphicTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Vector3 _startAngles;
        [SerializeField] private Vector3 _endAngles;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     The start local Euler angles in the rotation animation.
        /// </summary>
        public Vector3 StartAngles
        {
            get => _startAngles;
            set => _startAngles = value;
        }

        /// <summary>
        ///     The end local Euler angles in the rotation animation.
        /// </summary>
        public Vector3 EndAngles
        {
            get => _endAngles;
            set => _endAngles = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a rotation tweening animation for the <see cref="RectTransform.localEulerAngles" /> of a Unity
        ///     UI <see cref="Graphic" /> component.
        /// </summary>
        /// <param name="graphic">Target graphic</param>
        /// <param name="startAngles">Start local angles</param>
        /// <param name="endAngles">End local angles</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrRotationTween Animate(Graphic graphic, Vector3 startAngles, Vector3 endAngles, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrRotationTween rotationTween = graphic.GetOrAddComponent<UxrRotationTween>();

            rotationTween.StartAngles           = startAngles;
            rotationTween.EndAngles             = endAngles;
            rotationTween.InterpolationSettings = settings;
            rotationTween.FinishedCallback      = finishedCallback;
            rotationTween.Restart();

            return rotationTween;
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            RestoreLocalRotation();
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            TargetRectTransform.localEulerAngles = Vector3.Lerp(StartAngles, EndAngles, t);
        }

        #endregion
    }
}