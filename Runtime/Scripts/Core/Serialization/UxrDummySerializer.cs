// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDummySerializer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Math;
using UltimateXR.Core.StateSave;
using UltimateXR.Core.Unique;
using UnityEngine;

namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Serializer that doesn't read or write data. It can be used by <see cref="IUxrStateSave" /> implementations to cache
    ///     initial object data without any read or write operations.
    /// </summary>
    public class UxrDummySerializer : IUxrSerializer
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor for deserialization.
        /// </summary>
        /// <param name="isReading">Whether to initialize in read mode or write mode</param>
        /// <param name="version">Version that the data was serialized with</param>
        public UxrDummySerializer(bool isReading, int version)
        {
            Version   = version;
            IsReading = isReading;
        }

        #endregion

        #region Implicit IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion

        #region Implicit IUxrSerializer

        /// <inheritdoc />
        public int Version { get; }

        /// <inheritdoc />
        public bool IsReading { get; }

        /// <inheritdoc />
        public void Serialize(ref bool value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref sbyte value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref byte value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref char value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref int value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref uint value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref long value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref ulong value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref float value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref double value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref decimal value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref string value)
        {
        }

        /// <inheritdoc />
        public void SerializeEnum<T>(ref T value) where T : Enum
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Type value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Guid value)
        {
        }

        /// <inheritdoc />
        public void Serialize<T1, T2>(ref (T1, T2) value)
        {
        }

        /// <inheritdoc />
        public void Serialize<T>(ref T[] values)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref object[] values)
        {
        }

        /// <inheritdoc />
        public void Serialize<T>(ref List<T> values)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref List<object> values)
        {
        }

        /// <inheritdoc />
        public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> values)
        {
        }

        /// <inheritdoc />
        public void Serialize<T>(ref HashSet<T> values)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref HashSet<object> values)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref DateTime value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref TimeSpan value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Vector2 value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Vector3 value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Vector4 value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Color value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Color32 value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Quaternion value)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref Matrix4x4 value)
        {
        }

        /// <inheritdoc />
        public void SerializeUniqueComponent(ref IUxrUniqueId unique)
        {
        }

        /// <inheritdoc />
        public void SerializeUniqueComponent<T>(ref T component) where T : Component, IUxrUniqueId
        {
        }

        /// <inheritdoc />
        public void SerializeUxrSerializable(ref IUxrSerializable serializable)
        {
        }

        /// <inheritdoc />
        public void SerializeUxrSerializable<T>(ref T obj) where T : IUxrSerializable
        {
        }

        /// <inheritdoc />
        public void SerializeAxis(ref UxrAxis axis)
        {
        }

        /// <inheritdoc />
        public void SerializeAnyVar<T>(ref T obj)
        {
        }

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a dummy serializer initialized in read mode.
        /// </summary>
        public static UxrDummySerializer ReadModeSerializer = new UxrDummySerializer(true, UxrConstants.Serialization.CurrentBinaryVersion);

        /// <summary>
        ///     Gets a dummy serializer initialized in write mode.
        /// </summary>
        public static UxrDummySerializer WriteModeSerializer = new UxrDummySerializer(false, UxrConstants.Serialization.CurrentBinaryVersion);

        #endregion
    }
}