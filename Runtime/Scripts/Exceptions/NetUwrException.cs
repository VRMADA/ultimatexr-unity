// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetUwrException.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Exceptions
{
    /// <summary>
    ///     Unity Web Request Net exception.
    /// </summary>
    public sealed class NetUwrException : UwrException
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="message">Error message</param>
        public NetUwrException(string message) : base(message)
        {
        }

        #endregion
    }
}