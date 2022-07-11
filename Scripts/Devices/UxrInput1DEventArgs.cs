// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInput1DEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Stores information of a <see cref="UxrInput1D" /> input event.
    /// </summary>
    public sealed class UxrInput1DEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets which hand performed the input.
        /// </summary>
        public UxrHandSide HandSide { get; }

        /// <summary>
        ///     Gets the input target.
        /// </summary>
        public UxrInput1D Target { get; }

        /// <summary>
        ///     Gets the new input value.
        /// </summary>
        public float Value { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        /// <param name="target">Input target</param>
        /// <param name="newValue">New input value</param>
        public UxrInput1DEventArgs(UxrHandSide handSide, UxrInput1D target, float newValue)
        {
            HandSide = handSide;
            Target   = target;
            Value    = newValue;
        }

        #endregion
    }
}