// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrSerializer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Math;
using UltimateXR.Core.Unique;
using UnityEngine;

namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Interface to serialize and deserialize data. It uses a single method that performs the correct operation (read or
    ///     write) transparently, avoiding inconsistencies that can happen when using separate serialize/deserialize methods
    ///     on complex data with versioning.
    /// </summary>
    public interface IUxrSerializer : IDisposable
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the serialization version. When reading it tells the version the data was serialized with. When writing it
        ///     uses the latest version.
        /// </summary>
        int Version { get; }

        /// <summary>
        ///     Gets whether the operation is writing data (serializing) or reading data (deserializing).
        /// </summary>
        bool IsReading { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Serializes or deserializes a boolean value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref bool value);

        /// <summary>
        ///     Serializes or deserializes an sbyte value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref sbyte value);

        /// <summary>
        ///     Serializes or deserializes a byte value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref byte value);

        /// <summary>
        ///     Serializes or deserializes a char value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref char value);

        /// <summary>
        ///     Serializes or deserializes an int value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref int value);

        /// <summary>
        ///     Serializes or deserializes a uint value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref uint value);

        /// <summary>
        ///     Serializes or deserializes a long value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref long value);

        /// <summary>
        ///     Serializes or deserializes a ulong value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref ulong value);

        /// <summary>
        ///     Serializes or deserializes a float value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref float value);

        /// <summary>
        ///     Serializes or deserializes double value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref double value);

        /// <summary>
        ///     Serializes or deserializes decimal value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref decimal value);

        /// <summary>
        ///     Serializes or deserializes string value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support serializing null strings</remarks>
        void Serialize(ref string value);

        /// <summary>
        ///     Serializes or deserializes an Enum value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeEnum<T>(ref T value) where T : Enum;

        /// <summary>
        ///     Serializes or deserializes a type value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Type value);

        /// <summary>
        ///     Serializes or deserializes a <see cref="Guid" /> value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Guid value);

        /// <summary>
        ///     Serializes or deserializes a tuple.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize<T1, T2>(ref (T1, T2) value);

        /// <summary>
        ///     Serializes or deserializes an array.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <typeparam name="T">The type of the elements</typeparam>
        /// <remarks>Implementations must support null</remarks>
        void Serialize<T>(ref T[] values);

        /// <summary>
        ///     Serializes or deserializes an array, where each element might be of a different type. The type of each element will
        ///     be stored next to the object.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void Serialize(ref object[] values);

        /// <summary>
        ///     Serializes or deserializes a list.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <typeparam name="T">The type of the elements</typeparam>
        /// <remarks>Implementations must support null</remarks>
        void Serialize<T>(ref List<T> values);

        /// <summary>
        ///     Serializes or deserializes a list of objects, where each element might be of a different type. The type of each
        ///     element will be stored next to the object.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void Serialize(ref List<object> values);

        /// <summary>
        ///     Serializes or deserializes a hash set.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <typeparam name="T">The type of the elements</typeparam>
        /// <remarks>Implementations must support null</remarks>
        void Serialize<T>(ref HashSet<T> values);

        /// <summary>
        ///     Serializes or deserializes a hash set of objects, where each element might be of a different type. The type of each
        ///     element will be stored next to the object.
        /// </summary>
        /// <param name="values">The elements to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void Serialize(ref HashSet<object> values);

        /// <summary>
        ///     Serializes or deserializes a dictionary.
        /// </summary>
        /// <param name="values">The values</param>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <remarks>Implementations must support null</remarks>
        void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> values);

        /// <summary>
        ///     Serializes or deserializes a <see cref="DateTime" /> value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref DateTime dateTime);

        /// <summary>
        ///     Serializes or deserializes a <see cref="TimeSpan" /> value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref TimeSpan timeSpan);

        /// <summary>
        ///     Serializes or deserializes a Vector2 value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Vector2 value);

        /// <summary>
        ///     Serializes or deserializes a Vector3 value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Vector3 value);

        /// <summary>
        ///     Serializes or deserializes a Vector4 value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Vector4 value);

        /// <summary>
        ///     Serializes or deserializes Color value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Color value);

        /// <summary>
        ///     Serializes or deserializes Color32 value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Color32 value);

        /// <summary>
        ///     Serializes or deserializes Quaternion value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Quaternion value);

        /// <summary>
        ///     Serializes or deserializes Matrix4x4 value.
        /// </summary>
        /// <param name="value">The element to serialize or deserialize</param>
        void Serialize(ref Matrix4x4 value);

        /// <summary>
        ///     Serializes or deserializes a component with the <see cref="IUxrUniqueId" /> interface, storing only the
        ///     <see cref="IUxrUniqueId.UniqueId" />.
        /// </summary>
        /// <param name="unique">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeUniqueComponent(ref IUxrUniqueId unique);

        /// <summary>
        ///     Serializes or deserializes a component with the <see cref="IUxrUniqueId" /> interface, storing only the
        ///     <see cref="IUxrUniqueId.UniqueId" />.
        /// </summary>
        /// <param name="component">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeUniqueComponent<T>(ref T component) where T : Component, IUxrUniqueId;

        /// <summary>
        ///     Serializes or deserializes an object that implements the <see cref="IUxrSerializable" /> interface.
        /// </summary>
        /// <param name="serializable">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeUxrSerializable(ref IUxrSerializable serializable);

        /// <summary>
        ///     Serializes or deserializes an object that implements the <see cref="IUxrSerializable" /> interface.
        /// </summary>
        /// <param name="obj">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeUxrSerializable<T>(ref T obj) where T : IUxrSerializable;

        /// <summary>
        ///     Serializes or deserializes an <see cref="UxrAxis" /> value.
        /// </summary>
        /// <param name="axis">The axis to serialize or deserialize</param>
        void SerializeAxis(ref UxrAxis axis);

        /// <summary>
        ///     Serializes a variable of a type that is known only at runtime. When writing it will serialize the type
        ///     together with the value so that it can be deserialized back when reading.
        /// </summary>
        /// <param name="obj">The element to serialize or deserialize</param>
        /// <remarks>Implementations must support null</remarks>
        void SerializeAnyVar<T>(ref T obj);

        #endregion
    }
}