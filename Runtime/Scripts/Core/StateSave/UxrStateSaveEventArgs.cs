// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Serialization;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Event args for <see cref="UxrStateSaveMonitor" />.
    /// </summary>
    public class UxrStateSaveEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the serializer. Useful to know whether it is reading or writing using <see cref="IUxrSerializer.IsReading" />.
        /// </summary>
        public IUxrSerializer Serializer { get; private set; }

        /// <summary>
        ///     Gets the serialization level.
        /// </summary>
        public UxrStateSaveLevel Level { get; private set; }

        /// <summary>
        ///     Gets the serialization options.
        /// </summary>
        public UxrStateSaveOptions Options { get; private set; }

        /// <summary>
        ///     Gets the serialized var name. Only for <see cref="UxrStateSaveMonitor.VarSerializing" /> and
        ///     <see cref="UxrStateSaveMonitor.VarSerialized" />, or global events
        ///     <see cref="UxrStateSaveImplementer.VarSerializing" /> and <see cref="UxrStateSaveImplementer.VarSerialized" />.
        /// </summary>
        public string VarName { get; private set; }

        /// <summary>
        ///     Gets the value. For <see cref="UxrStateSaveMonitor.VarSerializing" /> and
        ///     <see cref="UxrStateSaveMonitor.VarSerialized" /> it will contain the value before serialization. For
        ///     <see cref="UxrStateSaveImplementer.VarSerializing" /> and <see cref="UxrStateSaveImplementer.VarSerialized" /> it
        ///     will contain the value after serialization.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///     For <see cref="UxrStateSaveMonitor.VarSerializing" /> and <see cref="UxrStateSaveMonitor.VarSerialized" /> it will
        ///     contain the same value as <see cref="Value" />.
        ///     For <see cref="UxrStateSaveImplementer.VarSerializing" /> and <see cref="UxrStateSaveImplementer.VarSerialized" />
        ///     it will contain the value before serialization.
        ///     In this case, <see cref="OldValue" /> will contain the value before serialization and <see cref="Value" /> will
        ///     contain the value after serialization.
        /// </summary>
        public object OldValue { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="serializer">The serializer</param>
        /// <param name="level">The serialization level</param>
        /// <param name="options">The serialization options</param>
        /// <param name="name">The variable name or null when not serializing any var</param>
        /// <param name="value">The variable value or null when not serializing any var</param>
        /// <param name="oldValue">The value of the variable before assigning the new one. Only when reading.</param>
        public UxrStateSaveEventArgs(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string name, object value, object oldValue = null)
        {
            Set(serializer, level, options, name, value, oldValue);
        }

        /// <summary>
        ///     Default Constructor.
        /// </summary>
        internal UxrStateSaveEventArgs()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sets the current values.
        /// </summary>
        /// <param name="serializer">The serializer</param>
        /// <param name="level">The serialization level</param>
        /// <param name="options">The serialization options</param>
        /// <param name="name">The variable name or null when not serializing any var</param>
        /// <param name="value">The variable value or null when not serializing any var</param>
        /// <param name="oldValue">Only when deserializing, tells the old value before assigning the new one</param>
        public void Set(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string name, object value, object oldValue = null)
        {
            Serializer = serializer;
            Level      = level;
            Options    = options;
            VarName    = name;
            Value      = value;
            OldValue   = oldValue;
        }

        #endregion
    }
}