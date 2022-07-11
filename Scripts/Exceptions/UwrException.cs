// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UwrException.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Exceptions
{
    /// <summary>
    ///     Base class for Unity Web Request exceptions.
    /// </summary>
    public abstract class UwrException : Exception
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        protected UwrException(string message) : base(message)
        {
        }

        #endregion
    }
}