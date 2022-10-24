// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrLogger.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    /// <summary>
    ///     Interface for all components that output log messages and want to provide a way to control the amount of
    ///     information sent.
    /// </summary>
    public interface IUxrLogger
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the current log level. This controls the amount of information sent.
        /// </summary>
        public UxrLogLevel LogLevel { get; set; }

        #endregion
    }
}