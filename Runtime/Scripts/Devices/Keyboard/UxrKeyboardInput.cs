// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyboardInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
using UnityEngine.InputSystem;
#else
using UnityEngine;
using System.Collections.Generic;
#endif

namespace UltimateXR.Devices.Keyboard
{
    /// <summary>
    ///     Static class to wrap keyboard input and be able to use legacy and new input system using a common class.
    /// </summary>
    public static class UxrKeyboardInput
    {
        #region Public Methods

        /// <summary>
        ///     Gets whether a given keyboard key was pressed during the current frame.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key was pressed during the current frame</returns>
        public static bool GetPressDown(UxrKey key)
        {
#if !ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            CheckCreateLegacyToInputSystemMapping();
            
            if (s_mapInputSystemToLegacy.TryGetValue((int) key, out int mappedKey))
            {
                return Input.GetKeyDown((KeyCode)mappedKey);
            }

            Debug.LogError($"{nameof(UxrKeyboardInput)}.{nameof(GetPressDown)}: Key {key} has no mapping for Unity's legacy input system");
            return false;
#else

            // Our keyboard mapping is the same as Unity's InputSystem.
            return UnityEngine.InputSystem.Keyboard.current[(Key)key].wasPressedThisFrame;
#endif
        }

        /// <summary>
        ///     Gets if a given keyboard key was released during the current frame.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key was released during the current frame</returns>
        public static bool GetPressUp(UxrKey key)
        {
#if !ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            CheckCreateLegacyToInputSystemMapping();
            
            if (s_mapInputSystemToLegacy.TryGetValue((int) key, out int mappedKey))
            {
                return Input.GetKeyUp((KeyCode)mappedKey);
            }

            Debug.LogError($"{nameof(UxrKeyboardInput)}.{nameof(GetPressUp)}: Key {key} has no mapping for Unity's legacy input system");
            return false;
#else

            // Our keyboard mapping is the same as Unity's InputSystem.
            return UnityEngine.InputSystem.Keyboard.current[(Key)key].wasReleasedThisFrame;
#endif
        }

        /// <summary>
        ///     Gets if a given keyboard key is being pressed.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is being pressed</returns>
        public static bool GetPressed(UxrKey key)
        {
#if !ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
            CheckCreateLegacyToInputSystemMapping();
            
            if (s_mapInputSystemToLegacy.TryGetValue((int) key, out int mappedKey))
            {
                return Input.GetKey((KeyCode) mappedKey);
            }

            Debug.LogError($"{nameof(UxrKeyboardInput)}.{nameof(GetPressed)}: Key {key} has no mapping for Unity's legacy input system");
            return false;
#else

            // Our keyboard mapping is the same as Unity's InputSystem.
            return UnityEngine.InputSystem.Keyboard.current[(Key)key].isPressed;
#endif
        }

        #endregion

        #region Private Methods

#if !ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
        /// <summary>
        /// Creates, if necessary, the key mapping dictionary to transform our key IDs to Unity's legacy input key IDs.
        /// </summary>
        private static void CheckCreateLegacyToInputSystemMapping()
        {
            void RegisterKey(UxrKey key, KeyCode value)
            {
                if (!s_mapInputSystemToLegacy.ContainsKey((int)key))
                {
                    s_mapInputSystemToLegacy.Add((int)key, (int)value);
                }
            }
            
            if (s_mapInputSystemToLegacy.Count == 0)
            {
                RegisterKey(UxrKey.None,           KeyCode.None);
                RegisterKey(UxrKey.Space,          KeyCode.Space);
                RegisterKey(UxrKey.Enter,          KeyCode.Return);
                RegisterKey(UxrKey.Tab,            KeyCode.Tab);
                RegisterKey(UxrKey.Backquote,      KeyCode.BackQuote);
                RegisterKey(UxrKey.Quote,          KeyCode.Quote);
                RegisterKey(UxrKey.Semicolon,      KeyCode.Semicolon);
                RegisterKey(UxrKey.Comma,          KeyCode.Comma);
                RegisterKey(UxrKey.Period,         KeyCode.Period);
                RegisterKey(UxrKey.Slash,          KeyCode.Slash);
                RegisterKey(UxrKey.Backslash,      KeyCode.Backslash);
                RegisterKey(UxrKey.LeftBracket,    KeyCode.LeftBracket);
                RegisterKey(UxrKey.RightBracket,   KeyCode.RightBracket);
                RegisterKey(UxrKey.Minus,          KeyCode.Minus);
                RegisterKey(UxrKey.Equals,         KeyCode.Equals);
                RegisterKey(UxrKey.A,              KeyCode.A);
                RegisterKey(UxrKey.B,              KeyCode.B);
                RegisterKey(UxrKey.C,              KeyCode.C);
                RegisterKey(UxrKey.D,              KeyCode.D);
                RegisterKey(UxrKey.E,              KeyCode.E);
                RegisterKey(UxrKey.F,              KeyCode.F);
                RegisterKey(UxrKey.G,              KeyCode.G);
                RegisterKey(UxrKey.H,              KeyCode.H);
                RegisterKey(UxrKey.I,              KeyCode.I);
                RegisterKey(UxrKey.J,              KeyCode.J);
                RegisterKey(UxrKey.K,              KeyCode.K);
                RegisterKey(UxrKey.L,              KeyCode.L);
                RegisterKey(UxrKey.M,              KeyCode.M);
                RegisterKey(UxrKey.N,              KeyCode.N);
                RegisterKey(UxrKey.O,              KeyCode.O);
                RegisterKey(UxrKey.P,              KeyCode.P);
                RegisterKey(UxrKey.Q,              KeyCode.Q);
                RegisterKey(UxrKey.R,              KeyCode.R);
                RegisterKey(UxrKey.S,              KeyCode.S);
                RegisterKey(UxrKey.T,              KeyCode.T);
                RegisterKey(UxrKey.U,              KeyCode.U);
                RegisterKey(UxrKey.V,              KeyCode.V);
                RegisterKey(UxrKey.W,              KeyCode.W);
                RegisterKey(UxrKey.X,              KeyCode.X);
                RegisterKey(UxrKey.Y,              KeyCode.Y);
                RegisterKey(UxrKey.Z,              KeyCode.Z);
                RegisterKey(UxrKey.Digit1,         KeyCode.Alpha1);
                RegisterKey(UxrKey.Digit2,         KeyCode.Alpha2);
                RegisterKey(UxrKey.Digit3,         KeyCode.Alpha3);
                RegisterKey(UxrKey.Digit4,         KeyCode.Alpha4);
                RegisterKey(UxrKey.Digit5,         KeyCode.Alpha5);
                RegisterKey(UxrKey.Digit6,         KeyCode.Alpha6);
                RegisterKey(UxrKey.Digit7,         KeyCode.Alpha7);
                RegisterKey(UxrKey.Digit8,         KeyCode.Alpha8);
                RegisterKey(UxrKey.Digit9,         KeyCode.Alpha9);
                RegisterKey(UxrKey.Digit0,         KeyCode.Alpha0);
                RegisterKey(UxrKey.LeftShift,      KeyCode.LeftShift);
                RegisterKey(UxrKey.RightShift,     KeyCode.RightShift);
                RegisterKey(UxrKey.LeftAlt,        KeyCode.LeftAlt);
                RegisterKey(UxrKey.AltGr,          KeyCode.AltGr);
                RegisterKey(UxrKey.RightAlt,       KeyCode.RightAlt);
                RegisterKey(UxrKey.LeftCtrl,       KeyCode.LeftControl);
                RegisterKey(UxrKey.RightCtrl,      KeyCode.RightControl);
                RegisterKey(UxrKey.LeftApple,      KeyCode.LeftApple);
                RegisterKey(UxrKey.LeftCommand,    KeyCode.LeftCommand);
                RegisterKey(UxrKey.LeftMeta,       KeyCode.LeftMeta);
                RegisterKey(UxrKey.LeftWindows,    KeyCode.LeftWindows);
                RegisterKey(UxrKey.RightApple,     KeyCode.RightApple);
                RegisterKey(UxrKey.RightCommand,   KeyCode.RightCommand);
                RegisterKey(UxrKey.RightMeta,      KeyCode.RightMeta);
                RegisterKey(UxrKey.RightWindows,   KeyCode.RightWindows);
                RegisterKey(UxrKey.ContextMenu,    KeyCode.Menu);
                RegisterKey(UxrKey.Escape,         KeyCode.Escape);
                RegisterKey(UxrKey.LeftArrow,      KeyCode.LeftArrow);
                RegisterKey(UxrKey.RightArrow,     KeyCode.RightArrow);
                RegisterKey(UxrKey.UpArrow,        KeyCode.UpArrow);
                RegisterKey(UxrKey.DownArrow,      KeyCode.DownArrow);
                RegisterKey(UxrKey.Backspace,      KeyCode.Backspace);
                RegisterKey(UxrKey.PageDown,       KeyCode.PageDown);
                RegisterKey(UxrKey.PageUp,         KeyCode.PageUp);
                RegisterKey(UxrKey.Home,           KeyCode.Home);
                RegisterKey(UxrKey.End,            KeyCode.End);
                RegisterKey(UxrKey.Insert,         KeyCode.Insert);
                RegisterKey(UxrKey.Delete,         KeyCode.Delete);
                RegisterKey(UxrKey.CapsLock,       KeyCode.CapsLock);
                RegisterKey(UxrKey.NumLock,        KeyCode.Numlock);
                RegisterKey(UxrKey.PrintScreen,    KeyCode.Print);
                RegisterKey(UxrKey.ScrollLock,     KeyCode.ScrollLock);
                RegisterKey(UxrKey.Pause,          KeyCode.Pause);
                RegisterKey(UxrKey.NumpadEnter,    KeyCode.KeypadEnter);
                RegisterKey(UxrKey.NumpadDivide,   KeyCode.KeypadDivide);
                RegisterKey(UxrKey.NumpadMultiply, KeyCode.KeypadMultiply);
                RegisterKey(UxrKey.NumpadPlus,     KeyCode.KeypadPlus);
                RegisterKey(UxrKey.NumpadMinus,    KeyCode.KeypadMinus);
                RegisterKey(UxrKey.NumpadPeriod,   KeyCode.KeypadPeriod);
                RegisterKey(UxrKey.NumpadEquals,   KeyCode.KeypadEquals);
                RegisterKey(UxrKey.Numpad0,        KeyCode.Keypad0);
                RegisterKey(UxrKey.Numpad1,        KeyCode.Keypad1);
                RegisterKey(UxrKey.Numpad2,        KeyCode.Keypad2);
                RegisterKey(UxrKey.Numpad3,        KeyCode.Keypad3);
                RegisterKey(UxrKey.Numpad4,        KeyCode.Keypad4);
                RegisterKey(UxrKey.Numpad5,        KeyCode.Keypad5);
                RegisterKey(UxrKey.Numpad6,        KeyCode.Keypad6);
                RegisterKey(UxrKey.Numpad7,        KeyCode.Keypad7);
                RegisterKey(UxrKey.Numpad8,        KeyCode.Keypad8);
                RegisterKey(UxrKey.Numpad9,        KeyCode.Keypad9);
                RegisterKey(UxrKey.F1,             KeyCode.F1);
                RegisterKey(UxrKey.F2,             KeyCode.F2);
                RegisterKey(UxrKey.F3,             KeyCode.F3);
                RegisterKey(UxrKey.F4,             KeyCode.F4);
                RegisterKey(UxrKey.F5,             KeyCode.F5);
                RegisterKey(UxrKey.F6,             KeyCode.F6);
                RegisterKey(UxrKey.F7,             KeyCode.F7);
                RegisterKey(UxrKey.F8,             KeyCode.F8);
                RegisterKey(UxrKey.F9,             KeyCode.F9);
                RegisterKey(UxrKey.F10,            KeyCode.F10);
                RegisterKey(UxrKey.F11,            KeyCode.F11);
                RegisterKey(UxrKey.F12,            KeyCode.F12);
            }
        }
#endif

        #endregion

        #region Private types & Data

#if !ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
        private static readonly Dictionary<int, int> s_mapInputSystemToLegacy = new Dictionary<int, int>();
#endif

        #endregion
    }
}