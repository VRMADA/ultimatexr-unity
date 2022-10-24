// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLogLevel.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    /// <summary>
    ///     Enumerates the different log levels
    /// </summary>
    public enum UxrLogLevel
    {
        /// <summary>
        ///     No logging.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Only log errors.
        /// </summary>
        Errors = 1,

        /// <summary>
        ///     Like <see cref="Errors" /> plus warnings.
        /// </summary>
        Warnings = 2,

        /// <summary>
        ///     Like <see cref="Warnings" /> plus relevant information.
        /// </summary>
        Relevant = 3,

        /// <summary>
        ///     All loggable information.
        /// </summary>
        Verbose = 4
    }
}