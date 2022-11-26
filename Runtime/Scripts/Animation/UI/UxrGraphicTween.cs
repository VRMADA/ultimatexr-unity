// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGraphicTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Abstract base class for tweening animations on Unity <see cref="Graphic" /> components.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    public abstract class UxrGraphicTween : UxrTween
    {
        #region Public Types & Data

        public Graphic       TargetGraphic       => GetCachedComponent<Graphic>();
        public RectTransform TargetRectTransform => GetCachedComponent<RectTransform>();

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override Behaviour TargetBehaviour => TargetGraphic;

        /// <inheritdoc />
        protected override void StoreOriginalValue()
        {
            _originalAnchoredPosition = TargetRectTransform.anchoredPosition;
            _originalLocalScale       = TargetRectTransform.localScale;
            _originalLocalRotation    = TargetRectTransform.localRotation;
            _originalColor            = TargetGraphic.color;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Restores the original Graphic's anchored position.
        /// </summary>
        protected void RestoreAnchoredPosition()
        {
            if (HasOriginalValueStored)
            {
                TargetRectTransform.anchoredPosition = _originalAnchoredPosition;
            }
        }

        /// <summary>
        ///     Restores the original Graphic's local scale.
        /// </summary>
        protected void RestoreLocalScale()
        {
            if (HasOriginalValueStored)
            {
                TargetRectTransform.localScale = _originalLocalScale;
            }
        }

        /// <summary>
        ///     Restores the original Graphic's local rotation.
        /// </summary>
        protected void RestoreLocalRotation()
        {
            if (HasOriginalValueStored)
            {
                TargetRectTransform.localRotation = _originalLocalRotation;
            }
        }

        /// <summary>
        ///     Restores the original Graphic's color.
        /// </summary>
        protected void RestoreColor()
        {
            if (HasOriginalValueStored)
            {
                TargetGraphic.color = _originalColor;
            }
        }

        #endregion

        #region Private Types & Data

        private Vector2    _originalAnchoredPosition;
        private Vector3    _originalLocalScale;
        private Quaternion _originalLocalRotation;
        private Color      _originalColor;

        #endregion
    }
}