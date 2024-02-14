// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSerializableClassNotFoundException.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.StateSync;

namespace UltimateXR.Exceptions
{
    /// <summary>
    ///     Exception thrown when trying to deserialize an object that implements the <see cref="IUxrSerializable" /> interface
    ///     but can't be instantiated using the name/assembly provided.
    /// </summary>
    public sealed class UxrSerializableClassNotFoundException : Exception
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the class type name.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        ///     Gets the assembly where the type is located.
        /// </summary>
        public string AssemblyName { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="assemblyName">Assembly name where the type is located</param>
        /// <param name="message">Exception message</param>
        public UxrSerializableClassNotFoundException(string className, string assemblyName, string message = null) : base(FormatMessage(className, assemblyName, message))
        {
            ClassName    = className;
            AssemblyName = assemblyName;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets a formatted exception message.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="assemblyName">Assembly name where the type is located</param>
        /// <param name="message">Original message</param>
        /// <returns>Exception message</returns>
        private static string FormatMessage(string className, string assemblyName, string message)
        {
            string assemblyInformation = !string.IsNullOrEmpty(assemblyName) ? $" in assembly {assemblyName}" : string.Empty;
            string prefix              = !string.IsNullOrEmpty(message) ? $"{message}: " : string.Empty;

            return $"{prefix}Can't instantiate type {className}{assemblyInformation} or it does not implement the {nameof(IUxrSerializable)} interface.";
        }

        #endregion
    }
}