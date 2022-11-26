// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyboardKeyUI.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0414

namespace UltimateXR.UI.Helpers.Keyboard
{
    /// <summary>
    ///     UI component for a keyboard key.
    /// </summary>
    [ExecuteInEditMode]
    public class UxrKeyboardKeyUI : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrKeyType                 _keyType;
        [SerializeField] private UxrKeyLayoutType           _layout;
        [SerializeField] private string                     _printShift;
        [SerializeField] private string                     _printNoShift;
        [SerializeField] private string                     _printAltGr;
        [SerializeField] private string                     _forceLabel;
        [SerializeField] private Text                       _singleLayoutValue;
        [SerializeField] private Text                       _multipleLayoutValueTopLeft;
        [SerializeField] private Text                       _multipleLayoutValueBottomLeft;
        [SerializeField] private Text                       _multipleLayoutValueBottomRight;
        [SerializeField] private List<UxrToggleSymbolsPage> _toggleSymbols;

        // Hidden in the custom inspector
        [SerializeField] private bool _nameDirty;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the key type.
        /// </summary>
        public UxrKeyType KeyType => _keyType;

        /// <summary>
        ///     Gets the layout use for the labels on the key.
        /// </summary>
        public UxrKeyLayoutType KeyLayoutType => _layout;

        /// <summary>
        ///     Gets the character used when the key has a single label.
        /// </summary>
        public char SingleLayoutValue => _singleLayoutValue != null && _singleLayoutValue.text.Length > 0 ? _singleLayoutValue.text[0] : '?';

        /// <summary>
        ///     Gets the character used in the top left corner when the key has multiple labels, because it supports combination
        ///     with shift and alt gr.
        /// </summary>
        public char MultipleLayoutValueTopLeft => _multipleLayoutValueTopLeft != null && _multipleLayoutValueTopLeft.text.Length > 0 ? _multipleLayoutValueTopLeft.text[0] : '?';

        /// <summary>
        ///     Gets the character used in the bottom left corner when the key has multiple labels, because it supports combination
        ///     with shift and alt gr.
        /// </summary>
        public char MultipleLayoutValueBottomLeft => _multipleLayoutValueBottomLeft != null && _multipleLayoutValueBottomLeft.text.Length > 0 ? _multipleLayoutValueBottomLeft.text[0] : '?';

        /// <summary>
        ///     Gets the character used in the bottom right corner when the key has multiple labels, because it supports
        ///     combination with shift and alt gr.
        /// </summary>
        public char MultipleLayoutValueBottomRight => _multipleLayoutValueBottomRight != null ? _multipleLayoutValueBottomRight.text[0] : '?';

        /// <summary>
        ///     Gets whether the key supports combination with shift and alt gr, and has a character specified for the bottom
        ///     right.
        /// </summary>
        public bool HasMultipleLayoutValueBottomRight => _multipleLayoutValueBottomRight != null && _multipleLayoutValueBottomRight.text.Length > 0;

        /// <summary>
        ///     Gets whether the key is a letter.
        /// </summary>
        public bool IsLetterKey => KeyType == UxrKeyType.Printable && char.IsLetter(SingleLayoutValue);

        /// <summary>
        ///     Gets the current symbols group selected, for a key that has a <see cref="KeyType" /> role of
        ///     <see cref="UxrKeyType.ToggleSymbols" />.
        /// </summary>
        public UxrToggleSymbolsPage CurrentToggleSymbolsPage => KeyType == UxrKeyType.ToggleSymbols && _toggleSymbols != null && _currentSymbolsIndex < _toggleSymbols.Count ? _toggleSymbols[_currentSymbolsIndex] : null;

        /// <summary>
        ///     Gets the next symbols group, for a key that has a <see cref="KeyType" /> role of
        ///     <see cref="UxrKeyType.ToggleSymbols" />, that would be selected if pressed.
        /// </summary>
        public UxrToggleSymbolsPage NextToggleSymbolsPage => KeyType == UxrKeyType.ToggleSymbols && _toggleSymbols != null && _toggleSymbols.Count > 0 ? _toggleSymbols[(_currentSymbolsIndex + 1) % _toggleSymbols.Count] : null;

        /// <summary>
        ///     Gets the <see cref="UxrKeyboardKeyUI" /> component the key belongs to.
        /// </summary>
        public UxrKeyboardUI Keyboard
        {
            get
            {
                if (_keyboard == null)
                {
                    _keyboard = GetComponentInParent<UxrKeyboardUI>();
                }

                return _keyboard;
            }
        }

        /// <summary>
        ///     Gets the <see cref="UxrControlInput" /> component for the key.
        /// </summary>
        public UxrControlInput ControlInput { get; private set; }

        /// <summary>
        ///     Gets or sets whether the key can be interacted with.
        /// </summary>
        public bool Enabled
        {
            get => ControlInput.Enabled;
            set => ControlInput.Enabled = value;
        }

        /// <summary>
        ///     Gets or sets the string that, if non-empty, will override the label content on the key.
        /// </summary>
        public string ForceLabel
        {
            get => _forceLabel;
            set
            {
                _forceLabel = value;
                SetupKeyLabels();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the character that would be printed if the key was pressed.
        /// </summary>
        /// <param name="shift">Whether shift is pressed</param>
        /// <param name="altGr">Whether alt gr is pressed</param>
        /// <returns>Character that would be printed</returns>
        public char GetSingleLayoutValueNoForceLabel(bool shift, bool altGr)
        {
            if (shift && !string.IsNullOrEmpty(_printShift))
            {
                return _printShift[0];
            }
            if (altGr && !string.IsNullOrEmpty(_printAltGr))
            {
                return _printAltGr[0];
            }

            return !string.IsNullOrEmpty(_printNoShift) ? _printNoShift[0] : ' ';
        }

        /// <summary>
        ///     Updates the label on the key.
        /// </summary>
        /// <param name="shiftEnabled">Whether shift is enabled</param>
        public void UpdateLetterKeyLabel(bool shiftEnabled)
        {
            if (KeyType == UxrKeyType.Printable && _singleLayoutValue)
            {
                _singleLayoutValue.text = shiftEnabled ? _printShift : _printNoShift;
            }
        }

        /// <summary>
        ///     Sets up the toggle symbol entries.
        /// </summary>
        /// <param name="entries">Entries</param>
        public void SetupToggleSymbolsPages(List<UxrToggleSymbolsPage> entries)
        {
            if (_keyType == UxrKeyType.ToggleSymbols)
            {
                _toggleSymbols       = entries;
                _currentSymbolsIndex = 0;

                if (entries != null)
                {
                    for (int i = 0; i < entries.Count; ++i)
                    {
                        entries[i].KeysRoot.SetActive(i == 0);
                    }
                }

                SetupKeyLabels();
            }
        }

        /// <summary>
        ///     Sets the default symbols as the ones currently active.
        /// </summary>
        public void SetDefaultSymbols()
        {
            if (_keyType == UxrKeyType.ToggleSymbols && _toggleSymbols != null && _toggleSymbols.Count > 0)
            {
                _currentSymbolsIndex = 0;

                for (int i = 0; i < _toggleSymbols.Count; ++i)
                {
                    _toggleSymbols[i].KeysRoot.SetActive(i == _currentSymbolsIndex);
                }

                SetupKeyLabels();
            }
        }

        /// <summary>
        ///     Toggles to the next symbols.
        /// </summary>
        public void ToggleSymbols()
        {
            if (_keyType == UxrKeyType.ToggleSymbols && _toggleSymbols != null && _toggleSymbols.Count > 0)
            {
                _currentSymbolsIndex = (_currentSymbolsIndex + 1) % _toggleSymbols.Count;

                for (int i = 0; i < _toggleSymbols.Count; ++i)
                {
                    _toggleSymbols[i].KeysRoot.SetActive(i == _currentSymbolsIndex);
                }

                SetupKeyLabels();
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_keyboard == null)
            {
                _keyboard = GetComponentInParent<UxrKeyboardUI>();
            }

            if (_keyboard == null && !Application.isEditor)
            {
                Debug.LogWarning($"{nameof(UxrKeyboardUI)} component not found in parent hierarchy of key " + name);
            }

            ControlInput = GetComponent<UxrControlInput>();

            if (ControlInput == null)
            {
                Debug.LogError($"Keyboard key {name} has no control input");
            }

            SetupKeyLabels();

            if (_keyboard && Application.isPlaying)
            {
                _keyboard.RegisterKey(this);
            }
        }

        /// <summary>
        ///     Called when the component is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_keyboard && Application.isPlaying)
            {
                _keyboard.UnregisterKey(this);
            }
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Updates the labels and the GameObject's name in editor-mode depending on the labels that are set in the inspector.
        /// </summary>
        private void Update()
        {
            if (Application.isEditor)
            {
                if (!Application.isPlaying)
                {
                    SetupKeyLabels();
                }

                if (_nameDirty)
                {
                    UpdateName();
                }
            }
        }

#endif

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sets up the labels on the key based on the current values in the inspector.
        /// </summary>
        private void SetupKeyLabels()
        {
            if (_singleLayoutValue)
            {
                _singleLayoutValue.text = "";
            }

            if (_multipleLayoutValueTopLeft)
            {
                _multipleLayoutValueTopLeft.text = "";
            }
            if (_multipleLayoutValueBottomLeft)
            {
                _multipleLayoutValueBottomLeft.text = "";
            }
            if (_multipleLayoutValueBottomRight)
            {
                _multipleLayoutValueBottomRight.text = "";
            }

            if (!string.IsNullOrEmpty(_forceLabel) && _singleLayoutValue)
            {
                _singleLayoutValue.text = _forceLabel;
                return;
            }

            if (_keyType == UxrKeyType.Printable)
            {
                if (_layout == UxrKeyLayoutType.SingleChar)
                {
                    if (_singleLayoutValue)
                    {
                        _singleLayoutValue.text = _printShift.Length > 0 && (_keyboard.ShiftEnabled || _keyboard.CapsLockEnabled) ? _printShift : _printNoShift;
                    }
                }
                else
                {
                    if (_multipleLayoutValueTopLeft)
                    {
                        _multipleLayoutValueTopLeft.text = _printShift;
                    }
                    if (_multipleLayoutValueBottomLeft)
                    {
                        _multipleLayoutValueBottomLeft.text = _printNoShift;
                    }
                    if (_multipleLayoutValueBottomRight)
                    {
                        _multipleLayoutValueBottomRight.text = _printAltGr;
                    }
                }
            }
            else if (_keyType == UxrKeyType.Tab)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Tab";
                }
            }
            else if (_keyType == UxrKeyType.Shift)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Shift";
                }
            }
            else if (_keyType == UxrKeyType.CapsLock)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Caps Lock";
                }
            }
            else if (_keyType == UxrKeyType.Control)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Ctrl";
                }
            }
            else if (_keyType == UxrKeyType.Alt)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Alt";
                }
            }
            else if (_keyType == UxrKeyType.AltGr)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Alt Gr";
                }
            }
            else if (_keyType == UxrKeyType.Enter)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Enter";
                }
            }
            else if (_keyType == UxrKeyType.Backspace)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Backspace";
                }
            }
            else if (_keyType == UxrKeyType.Del)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Del";
                }
            }
            else if (_keyType == UxrKeyType.ToggleSymbols)
            {
                _singleLayoutValue.text = NextToggleSymbolsPage != null    ? NextToggleSymbolsPage.Label :
                                          CurrentToggleSymbolsPage != null ? CurrentToggleSymbolsPage.Label : string.Empty;
            }
            else if (_keyType == UxrKeyType.Escape)
            {
                if (_singleLayoutValue)
                {
                    _singleLayoutValue.text = "Esc";
                }
            }
        }

        /// <summary>
        ///     Updates the GameObject name based on the labels set up in the inspector.
        /// </summary>
        private void UpdateName()
        {
            if (_keyType == UxrKeyType.Printable)
            {
                if (_singleLayoutValue && _layout == UxrKeyLayoutType.SingleChar)
                {
                    if (_singleLayoutValue.text == " ")
                    {
                        name = "Key Space";
                    }
                    else
                    {
                        name = "Key " + _singleLayoutValue.text;
                    }
                }
                else
                {
                    name = "Key" + (!string.IsNullOrEmpty(_printShift) ? " " + _printShift : "") + (!string.IsNullOrEmpty(_printNoShift) ? " " + _printNoShift : "") + (!string.IsNullOrEmpty(_printAltGr) ? " " + _printAltGr : "");
                }
            }
            else if (_keyType == UxrKeyType.ToggleSymbols)
            {
                name = "Key Toggle Symbols";
            }
            else if (_keyType == UxrKeyType.ToggleViewPassword)
            {
                name = "Key Toggle View Password";
            }
            else if (_singleLayoutValue)
            {
                name = $"Key {_singleLayoutValue.text}";
            }

            _nameDirty = false;
        }

        #endregion

        #region Private Types & Data

        private UxrKeyboardUI _keyboard;
        private int           _currentSymbolsIndex;

        #endregion
    }
}

#pragma warning restore 0414