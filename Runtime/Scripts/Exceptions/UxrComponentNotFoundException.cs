// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentNotFoundException.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Unique;

namespace UltimateXR.Exceptions
{
    /// <summary>
    ///     UltimateXR Component not found exception. This exception is normally thrown when a method that uses
    ///     <see cref="UxrUniqueIdImplementer.TryGetComponentById" /> could not find the component.
    ///     <see cref="UxrUniqueIdImplementer.TryGetComponentById" /> itself doesn't throw any exception.
    /// </summary>
    public sealed class UxrComponentNotFoundException : Exception
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the component's unique ID that could not be found.
        /// </summary>
        public Guid UniqueId { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="uniqueId">The unique ID of the component that was going to be retrieved</param>
        /// <param name="message">Exception message</param>
        public UxrComponentNotFoundException(Guid uniqueId, string message = null) : base(FormatMessage(uniqueId, message))
        {
            UniqueId = uniqueId;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets a formatted exception message.
        /// </summary>
        /// <param name="uniqueId">The unique ID of the component that was going to be retrieved</param>
        /// <param name="message">Original message</param>
        /// <returns>Exception message</returns>
        private static string FormatMessage(Guid uniqueId, string message)
        {
            string prefix = string.IsNullOrEmpty(message) ? $"{message}: " : string.Empty;
            return $"{prefix}Could not find the given component using {nameof(UxrUniqueIdImplementer)}.{nameof(UxrUniqueIdImplementer.TryGetComponentById)}(). Id is {(uniqueId != null ? uniqueId == default ? "empty" : uniqueId : "null")}.";
        }

        #endregion
    }
}