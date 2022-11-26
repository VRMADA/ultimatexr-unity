// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHiScoresPanelEntries.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Animation.UI;
using UltimateXR.Core.Components;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.Helpers.HiScores
{
    /// <summary>
    ///     UI component a hi-scores panel that shows user+score entries.
    /// </summary>
    public class UxrHiScoresPanelEntries : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Text             _textTitle;
        [SerializeField] private CanvasGroup      _canvasGroup;
        [SerializeField] private Transform        _entriesRoot;
        [SerializeField] private UxrHiScoresEntry _prefabEntry;
        [SerializeField] private UxrControlInput  _buttonOk;
        [SerializeField] private Text             _textButtonOk;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called whenever the OK button was clicked.
        /// </summary>
        public event Action OkButtonClicked;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Enables the Hi-scores panel so that it shows up with a fade-in effect. The hi-scores panel's root GameObject should
        ///     be in an inactive state.
        /// </summary>
        /// <param name="title">Text to show on the title</param>
        /// <param name="buttonOk">Text to show as the OK button</param>
        /// <param name="fadeDurationSeconds">Seconds it takes for the panel to fade in</param>
        /// <param name="fadeDelaySeconds">Seconds to wait before the panel fades in</param>
        public void Show(string title, string buttonOk, float fadeDurationSeconds, float fadeDelaySeconds = 0.0f)
        {
            _textTitle.text    = title;
            _textButtonOk.text = buttonOk;

            gameObject.SetActive(true);

            UxrCanvasAlphaTween.FadeIn(_canvasGroup, fadeDurationSeconds, fadeDelaySeconds);
        }

        /// <summary>
        ///     Adds a hi-score entry to the panel.
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <param name="entryValue">Entry score</param>
        /// <param name="icon">Optional icon to show next to the score</param>
        public void AddEntry(string entryName, string entryValue, Sprite icon)
        {
            UxrHiScoresEntry entry = Instantiate(_prefabEntry, _entriesRoot);
            entry.Setup(entryName, entryValue, icon);
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

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever the OK button was clicked. Closes the panel.
        /// </summary>
        /// <param name="controlInput">Control that was clicked</param>
        /// <param name="eventData">Event parameters</param>
        private void ButtonOk_Clicked(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (!UxrTween.HasActiveTween<UxrCanvasAlphaTween>(_canvasGroup))
            {
                UxrCanvasAlphaTween.FadeOut(_canvasGroup, 0.2f, 0.2f, t => OkButtonClicked?.Invoke()).SetFinishedActions(UxrTweenFinishedActions.DeactivateGameObject);
            }
        }

        #endregion
    }
}