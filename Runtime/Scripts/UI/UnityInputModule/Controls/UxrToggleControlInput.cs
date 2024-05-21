// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleControlInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#pragma warning disable 67 // Disable warnings due to unused events

namespace UltimateXR.UI.UnityInputModule.Controls
{
    /// <summary>
    ///     Type of <see cref="UxrControlInput" /> that implements toggle functionality.
    /// </summary>
    public partial class UxrToggleControlInput : UxrControlInput
    {
        #region Inspector Properties/Serialized Fields

        [FormerlySerializedAs("_initialStateIsSelected")] [SerializeField] private InitState             _initialState = InitState.DontChange;
        [SerializeField]                                                   private bool                  _canToggleOnlyOnce;
        [SerializeField]                                                   private Text                  _text;
        [SerializeField]                                                   private List<GameObject>      _enableWhenSelected;
        [SerializeField]                                                   private List<GameObject>      _enableWhenNotSelected;
        [SerializeField]                                                   private List<TextColorChange> _textColorChanges;
        [SerializeField]                                                   private AudioClip             _audioToggleOn;
        [SerializeField]                                                   private AudioClip             _audioToggleOff;
        [SerializeField] [Range(0, 1)]                                     private float                 _audioToggleOnVolume  = 1.0f;
        [SerializeField] [Range(0, 1)]                                     private float                 _audioToggleOffVolume = 1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called whenever the state is toggled.
        /// </summary>
        public event Action<UxrToggleControlInput> Toggled;

        /// <summary>
        ///     Gets or sets whether the current toggled state.
        ///     To set the state of the control without triggering any events, use <see cref="SetIsSelected" /> instead.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetIsSelected(value, true);
        }

        /// <summary>
        ///     Gets or sets whether the control can be toggled or not.
        /// </summary>
        public bool CanBeToggled { get; set; } = true;

        /// <summary>
        ///     Gets or sets the text value. If no <see cref="Text" /> component is configured it will return
        ///     <see cref="string.Empty" />.
        /// </summary>
        public string Text
        {
            get => _text != null ? _text.text : string.Empty;
            set
            {
                if (_text != null)
                {
                    _text.text = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Changes the current state of the control like <see cref="IsSelected" /> but allowing to control whether
        ///     <see cref="Toggled" /> events are propagated or not.
        /// </summary>
        /// <param name="value">State (selected/not-selected)</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void SetIsSelected(bool value, bool propagateEvents)
        {
            if (_isSelected == value && _isInitialized)
            {
                return;
            }
            
            _isSelected = value;

            foreach (GameObject goToEnable in _enableWhenSelected)
            {
                if (goToEnable == null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelUI >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.UiModule} {transform.GetPathUnderScene()} has null enableWhenSelected entry");
                    }
                }
                else
                {
                    goToEnable.SetActive(_isSelected);
                }
            }

            foreach (GameObject goToEnable in _enableWhenNotSelected)
            {
                if (goToEnable == null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelUI >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.UiModule} {transform.GetPathUnderScene()} has null enableWhenNotSelected entry");
                    }
                }
                else
                {
                    goToEnable.SetActive(!_isSelected);
                }
            }

            foreach (TextColorChange textEntry in _textColorChanges)
            {
                textEntry.TextComponent.color = _isSelected ? textEntry.ColorSelected : textEntry.ColorNotSelected;
            }

            _isInitialized = true;

            if (propagateEvents)
            {
                Toggled?.Invoke(this);
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Sets up the events and initializes the current state.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (!_isInitialized && _initialState != InitState.DontChange)
            {
                SetIsSelected(_initialState == InitState.ToggledOn, true);
            }

            _alreadyToggled = false;
        }

        /// <summary>
        ///     Called when the component is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _text                  = null;
            _enableWhenSelected    = null;
            _enableWhenNotSelected = null;
            _textColorChanges      = null;
        }

        /// <summary>
        ///     Checks for a <see cref="UxrToggleGroup" /> in any parent object to refresh the content.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrToggleGroup group = GetComponentInParent<UxrToggleGroup>();

            if (group != null)
            {
                group.RefreshToggleChildrenList();
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Control was clicked. Toggle state.
        /// </summary>
        /// <param name="eventData">Event data</param>
        protected override void OnClicked(PointerEventData eventData)
        {
            base.OnClicked(eventData);

            if (!CanBeToggled || (_alreadyToggled && _canToggleOnlyOnce))
            {
                return;
            }

            if (Interactable)
            {
                _alreadyToggled = true;

                if (_canToggleOnlyOnce)
                {
                    Enabled = false;
                }

                Vector3 audioPosition = UxrAvatar.LocalAvatarCamera ? UxrAvatar.LocalAvatar.CameraPosition : transform.position;

                if (_audioToggleOff && !_isSelected)
                {
                    AudioSource.PlayClipAtPoint(_audioToggleOff, audioPosition, _audioToggleOffVolume);
                }
                else if (_audioToggleOn && _isSelected)
                {
                    AudioSource.PlayClipAtPoint(_audioToggleOn, audioPosition, _audioToggleOnVolume);
                }

                SetIsSelected(!_isSelected, true);
            }
        }

        #endregion

        #region Private Types & Data

        private bool _isInitialized;
        private bool _isSelected;
        private bool _alreadyToggled;

        #endregion
    }
}

#pragma warning restore 67