// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInput2DEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Stores information of a <see cref="UxrInput2D" /> input event.
    /// </summary>
    public sealed class UxrInput2DEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets which hand performed the input.
        /// </summary>
        public UxrHandSide HandSide { get; }

        /// <summary>
        ///     Gets the input target.
        /// </summary>
        public UxrInput2D Target { get; }

        /// <summary>
        ///     Gets the new input value.
        /// </summary>
        public Vector2 Value { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        /// <param name="target">Input target</param>
        /// <param name="newValue">New input value</param>
        public UxrInput2DEventArgs(UxrHandSide handSide, UxrInput2D target, Vector2 newValue)
        {
            HandSide = handSide;
            Target   = target;
            Value    = newValue;
        }

        #endregion
    }
}