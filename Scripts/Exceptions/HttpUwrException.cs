// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpUwrException.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Exceptions
{
    /// <summary>
    ///     Unity Web Request HTTP exception.
    /// </summary>
    public sealed class HttpUwrException : UwrException
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the HTTP response code.
        /// </summary>
        public long ResponseCode { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="responseCode">HTTP response code</param>
        public HttpUwrException(string error, long responseCode) : base(error)
        {
            ResponseCode = responseCode;
        }

        #endregion
    }
}