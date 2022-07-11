// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyboardKeyEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.UI.Helpers.Keyboard
{
    /// <summary>
    ///     Key press/release event parameters.
    /// </summary>
    public class UxrKeyboardKeyEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the key that was pressed/released.
        /// </summary>
        public UxrKeyboardKeyUI Key { get; }

        /// <summary>
        ///     Gets whether it was a press (true) or release (false).
        /// </summary>
        public bool IsPress { get; }

        /// <summary>
        ///     Gets the current line content. If it was a keypress event and the the keypress was the ENTER key then the line
        ///     before pressing ENTER is passed.
        /// </summary>
        public string Line { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="key">Key that was pressed</param>
        /// <param name="isPress">Is it a press or a release?</param>
        /// <param name="line">Current line</param>
        public UxrKeyboardKeyEventArgs(UxrKeyboardKeyUI key, bool isPress, string line)
        {
            Key     = key;
            IsPress = isPress;
            Line    = line;
        }

        #endregion
    }
}