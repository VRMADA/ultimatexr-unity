// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPropertyChangedSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Serialization;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Event args for the state sync of a property whose value was changed.
    /// </summary>
    public class UxrPropertyChangedSyncEventArgs : UxrSyncEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        public string PropertyName
        {
            get => _propertyName;
            private set => _propertyName = value;
        }

        /// <summary>
        ///     Gets the new property value.
        /// </summary>
        public object Value
        {
            get => _value;
            private set => _value = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The new value of the property</param>
        public UxrPropertyChangedSyncEventArgs(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value        = value;
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            string newValue = Value == null ? "null" : Value.ToString();
            return $"Property change {PropertyName ?? "Unknown"} = {newValue ?? "null/unknown"}";
        }

        #endregion

        #region Protected Overrides UxrSyncEventArgs

        /// <inheritdoc />
        protected override void SerializeEventInternal(IUxrSerializer serializer)
        {
            serializer.Serialize(ref _propertyName);
            serializer.SerializeAnyVar(ref _value);
        }

        #endregion

        #region Private Types & Data

        private string _propertyName;
        private object _value;

        #endregion
    }
}