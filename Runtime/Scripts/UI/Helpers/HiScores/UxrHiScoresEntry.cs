// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHiScoresEntry.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.UI.Helpers.HiScores
{
    /// <summary>
    ///     UI component for a hi-scores entry.
    /// </summary>
    public class UxrHiScoresEntry : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Text  _name;
        [SerializeField] private Text  _value;
        [SerializeField] private Image _image;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sets up the content.
        /// </summary>
        /// <param name="userName">Name to show on the label</param>
        /// <param name="value">Value to show as a score</param>
        /// <param name="sprite">Optional sprite to show next to the score</param>
        public void Setup(string userName, string value, Sprite sprite)
        {
            _name.text  = userName;
            _value.text = value;

            if (sprite != null)
            {
                _image.overrideSprite = sprite;
                _image.gameObject.SetActive(true);
            }
            else
            {
                _image.sprite = null;
                //_image.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}