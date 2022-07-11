// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHiScoresPanelEnterName.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.UI;
using UltimateXR.Core.Components;
using UltimateXR.UI.Helpers.Keyboard;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.Helpers.HiScores
{
    /// <summary>
    ///     UI component for a hi-scores panel that requests the user name.
    /// </summary>
    public class UxrHiScoresPanelEnterName : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private UxrKeyboardUI   _keyboard;
        [SerializeField] private Text            _textCongratulations;
        [SerializeField] private Text            _textEnterName;
        [SerializeField] private UxrControlInput _buttonOk;
        [SerializeField] private Text            _textButtonOk;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called when the user finished entering the name and pressed the OK button.
        /// </summary>
        public event Action<string> NameEntered;

        /// <summary>
        ///     Gets the <see cref="UxrKeyboardUI" /> component that is used to enter the name.
        /// </summary>
        public UxrKeyboardUI Keyboard => _keyboard;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Shows the panel using a fade effect. The panel GameObject should be in an inactive state.
        /// </summary>
        /// <param name="textCongratulations">Congratulations text</param>
        /// <param name="textEnterName">Enter name text on top</param>
        /// <param name="textEnter">Enter name text right above the name</param>
        /// <param name="fadeDurationSeconds">Seconds it takes for the panel to fade in</param>
        /// <param name="fadeDelaySeconds">Seconds to wait before the panel fades in</param>
        public void Show(string textCongratulations, string textEnterName, string textEnter, float fadeDurationSeconds, float fadeDelaySeconds = 0.0f)
        {
            gameObject.SetActive(true);

            UxrCanvasAlphaTween.FadeIn(_canvasGroup, fadeDurationSeconds, fadeDelaySeconds);

            _textCongratulations.text = textCongratulations;
            _textEnterName.text       = textEnterName;
            _textButtonOk.text        = textEnter;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _buttonOk.Clicked += ButtonOk_Clicked;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            _buttonOk.Clicked -= ButtonOk_Clicked;
        }

        /// <summary>
        ///     Updates the OK button interactive state depending on whether there is currently any content in the name box.
        /// </summary>
        private void Update()
        {
            _buttonOk.Enabled = !string.IsNullOrEmpty(_keyboard.CurrentLine);
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever the OK button was clicked. Closes the panel.
        /// </summary>
        /// <param name="controlInput">Control that was clicked</param>
        /// <param name="eventData">Event parameters</param>
        private void ButtonOk_Clicked(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(_keyboard.CurrentLine) && !UxrTween.HasActiveTween<UxrCanvasAlphaTween>(_canvasGroup))
            {
                UxrCanvasAlphaTween.FadeOut(_canvasGroup, 0.2f, 0.0f, t => NameEntered?.Invoke(_keyboard.CurrentLine)).SetFinishedActions(UxrTweenFinishedActions.DeactivateGameObject);
            }
        }

        #endregion
    }
}