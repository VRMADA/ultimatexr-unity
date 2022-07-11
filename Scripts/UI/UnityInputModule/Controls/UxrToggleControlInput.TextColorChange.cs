// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleControlInput.TextColorChange.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule.Controls
{
    public partial class UxrToggleControlInput
    {
        #region Private Types & Data

        /// <summary>
        ///     Allows to specify separate selected/not-selected colors for a text.
        /// </summary>
        [Serializable]
        private class TextColorChange
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private Text  _text;
            [SerializeField] private Color _colorSelected;
            [SerializeField] private Color _colorNotSelected;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the text component.
            /// </summary>
            public Text TextComponent => _text;

            /// <summary>
            ///     Gets the color used when the element is selected.
            /// </summary>
            public Color ColorSelected => _colorSelected;

            /// <summary>
            ///     Gets the color used when the element is not selected.
            /// </summary>
            public Color ColorNotSelected => _colorNotSelected;

            #endregion
        }

        #endregion
    }
}