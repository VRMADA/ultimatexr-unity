// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInputButtonEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Stores information of a <see cref="UxrInputButtons" /> input event.
    /// </summary>
    public sealed class UxrInputButtonEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets which hand performed the input.
        /// </summary>
        public UxrHandSide HandSide { get; }

        /// <summary>
        ///     Gets which button changed.
        /// </summary>
        public UxrInputButtons Button { get; }

        /// <summary>
        ///     Gets the button input event type.
        /// </summary>
        public UxrButtonEventType ButtonEventType { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        /// <param name="button">Target button</param>
        /// <param name="buttonEventType">Button input event type</param>
        public UxrInputButtonEventArgs(UxrHandSide handSide, UxrInputButtons button, UxrButtonEventType buttonEventType)
        {
            HandSide        = handSide;
            Button          = button;
            ButtonEventType = buttonEventType;
        }

        #endregion
    }
}