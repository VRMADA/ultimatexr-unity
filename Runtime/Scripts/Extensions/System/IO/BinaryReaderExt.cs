// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryReaderExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UltimateXR.Core.Math;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
using UltimateXR.Exceptions;
using UnityEngine;

namespace UltimateXR.Extensions.System.IO
{
    /// <summary>
    ///     <see cref="BinaryReader" /> extensions.
    /// </summary>
    public static class BinaryReaderExt
    {
        #region Public Methods

        /// <summary>
        ///     Reads a 32-bit integer in compressed format, using only the amount of bytes that are necessary.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>A 32-bit integer</returns>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        public static int ReadCompressedInt32(this BinaryReader reader, int serializationVersion)
        {
            return (int)ReadCompressedUInt32(reader, serializationVersion);
        }

        /// <summary>
        ///     Reads a 32-bit unsigned integer in compressed format, using only the amount of bytes that are necessary.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>A 32-bit unsigned integer</returns>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        public static uint ReadCompressedUInt32(this BinaryReader reader, int serializationVersion)
        {
            uint num1 = 0;
            int  num2 = 0;

            while (num2 != 35)
            {
                byte num3 = reader.ReadByte();
                num1 |= (uint)(num3 & sbyte.MaxValue) << num2;
                num2 += 7;

                if ((num3 & 128) == 0)
                {
                    return num1;
                }
            }

            throw new FormatException("ReadCompressedInt32()/ReadCompressedUInt32() found bad format");
        }

        /// <summary>
        ///     Reads a 64-bit integer in compressed format, using only the amount of bytes that are necessary.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>A 64-bit integer</returns>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        public static long ReadCompressedInt64(this BinaryReader reader, int serializationVersion)
        {
            return (long)ReadCompressedUInt64(reader, serializationVersion);
        }

        /// <summary>
        ///     Reads a 64-bit unsigned integer in compressed format, using only the amount of bytes that are necessary.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>A 64-bit unsigned integer</returns>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        public static ulong ReadCompressedUInt64(this BinaryReader reader, int serializationVersion)
        {
            ulong num1 = 0;
            int   num2 = 0;

            while (num2 != 63)
            {
                byte num3 = reader.ReadByte();
                num1 |= (ulong)(num3 & sbyte.MaxValue) << num2;
                num2 += 7;

                if ((num3 & 128) == 0)
                {
                    return num1;
                }
            }

            throw new FormatException("ReadCompressedInt64()/ReadCompressedUInt64() found bad format");
        }

        /// <summary>
        ///     Reads an enum value.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Enum object</returns>
        public static T ReadEnum<T>(this BinaryReader reader, int serializationVersion)
        {
            return (T)Enum.ToObject(typeof(T), reader.ReadCompressedInt32(serializationVersion));
        }

        /// <summary>
        ///     Reads a string written using <see cref="BinaryWriterExt.WriteStringWithNullCheck" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>String or null</returns>
        public static string ReadStringWithNullCheck(this BinaryReader reader)
        {
            // Serialized as: null-check (bool), string

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            return reader.ReadString();
        }

        /// <summary>
        ///     Reads a type, which has been serialized as two strings: the full type name plus the assembly. If the type is from
        ///     the same assembly as UltimateXR, the assembly will be an empty string.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The type</returns>
        public static Type ReadType(this BinaryReader reader, int serializationVersion)
        {
            return reader.ReadType(serializationVersion, out string _, out string _);
        }

        /// <summary>
        ///     Reads a type, which has been serialized as two strings: the full type name plus the assembly. If the type is from
        ///     the same assembly as UltimateXR, the assembly will be an empty string.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <param name="typeName">Returns the type name</param>
        /// <param name="assembly">Returns the type assembly or null/empty if the type is in the same assembly as UltimateXR</param>
        /// <returns>The type</returns>
        public static Type ReadType(this BinaryReader reader, int serializationVersion, out string typeName, out string assembly)
        {
            typeName = reader.ReadString();

            if (typeName != string.Empty)
            {
                assembly = reader.ReadString();
                return TypeExt.GetType(typeName, assembly);
            }

            assembly = string.Empty;
            return null;
        }

        /// <summary>
        ///     Reads a Guid, which has been serialized as a 16 byte array.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The Guid</returns>
        public static Guid ReadGuid(this BinaryReader reader, int serializationVersion)
        {
            return new Guid(reader.ReadBytes(16));
        }

        /// <summary>
        ///     Reads a tuple.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The tuple</returns>
        public static (T1, T2) ReadTuple<T1, T2>(this BinaryReader reader, int serializationVersion)
        {
            return ((T1, T2))(reader.Read<T1>(serializationVersion), reader.Read<T2>(serializationVersion));
        }

        /// <summary>
        ///     Reads an array.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <typeparam name="T">The type of elements in the array</typeparam>
        /// <returns>The array</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static T[] ReadArray<T>(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements   

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            T[] array = new T[reader.ReadCompressedInt32(serializationVersion)];

            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = (T)reader.Read<T>(serializationVersion);
            }

            return array;
        }

        /// <summary>
        ///     Reads an array of objects where each element can be of a different type.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The object array</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static object[] ReadObjectArray(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            object[]  array          = new object[reader.ReadCompressedInt32(serializationVersion)];
            Exception firstException = null;

            for (int i = 0; i < array.Length; ++i)
            {
                try
                {
                    array[i] = reader.ReadAnyVar(serializationVersion);
                }
                catch (Exception e)
                {
                    if (firstException == null)
                    {
                        firstException = e;
                    }
                }
            }

            if (firstException != null)
            {
                // This helps debugging UxrMethodInvokedSyncEventArgs. We keep deserializing event parameters even if there are errors.
                throw firstException;
            }

            return array;
        }

        /// <summary>
        ///     Reads a list.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <returns>The list</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static List<T> ReadList<T>(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            List<T> list = new List<T>();

            int count = reader.ReadCompressedInt32(serializationVersion);

            for (int i = 0; i < count; ++i)
            {
                list.Add((T)reader.Read<T>(serializationVersion));
            }

            return list;
        }

        /// <summary>
        ///     Reads a list of objects where each element can be of a different type.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The list</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static List<object> ReadObjectList(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            List<object> list = new List<object>();

            int count = reader.ReadCompressedInt32(serializationVersion);

            for (int i = 0; i < count; ++i)
            {
                list.Add(reader.ReadAnyVar(serializationVersion));
            }

            return list;
        }

        /// <summary>
        ///     Reads a dictionary.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The dictionary</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            int count = reader.ReadCompressedInt32(serializationVersion);

            for (int i = 0; i < count; ++i)
            {
                TKey   key   = (TKey)reader.Read<TKey>(serializationVersion);
                TValue value = (TValue)reader.Read<TValue>(serializationVersion);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        /// <summary>
        ///     Reads a HashSet.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <typeparam name="T">The type of elements in the HashSet</typeparam>
        /// <returns>The hash set</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static HashSet<T> ReadHashSet<T>(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            HashSet<T> hashSet = new HashSet<T>();

            int count = reader.ReadCompressedInt32(serializationVersion);

            for (int i = 0; i < count; ++i)
            {
                hashSet.Add((T)reader.Read<T>(serializationVersion));
            }

            return hashSet;
        }

        /// <summary>
        ///     Reads a hash set of objects where each element can be of a different type.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>The hash set</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static HashSet<object> ReadObjectHashSet(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), count (int32), elements

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            HashSet<object> hashSet = new HashSet<object>();

            int count = reader.ReadCompressedInt32(serializationVersion);

            for (int i = 0; i < count; ++i)
            {
                hashSet.Add(reader.ReadAnyVar(serializationVersion));
            }

            return hashSet;
        }

        /// <summary>
        ///     Reads a <see cref="DateTime" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>DateTime</returns>
        public static DateTime ReadDateTime(this BinaryReader reader, int serializationVersion)
        {
            // Read ticks (64 bits)
            return new DateTime(reader.ReadCompressedInt64(serializationVersion));
        }

        /// <summary>
        ///     Reads a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan ReadTimeSpan(this BinaryReader reader, int serializationVersion)
        {
            // Read ticks (64 bits)
            return new TimeSpan(reader.ReadCompressedInt64(serializationVersion));
        }

        /// <summary>
        ///     Reads a <see cref="Vector2" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Vector</returns>
        public static Vector2 ReadVector2(this BinaryReader reader, int serializationVersion)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        ///     Reads a <see cref="Vector3" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Vector</returns>
        public static Vector3 ReadVector3(this BinaryReader reader, int serializationVersion)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        ///     Reads a <see cref="Vector4" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Vector</returns>
        public static Vector4 ReadVector4(this BinaryReader reader, int serializationVersion)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        ///     Reads a <see cref="Color" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Color</returns>
        public static Color ReadColor(this BinaryReader reader, int serializationVersion)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        ///     Reads a <see cref="Color32" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Color32</returns>
        public static Color32 ReadColor32(this BinaryReader reader, int serializationVersion)
        {
            return new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        /// <summary>
        ///     Reads a <see cref="Quaternion" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Quaternion</returns>
        public static Quaternion ReadQuaternion(this BinaryReader reader, int serializationVersion)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        ///     Reads a <see cref="Matrix4x4" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Matrix</returns>
        public static Matrix4x4 ReadMatrix(this BinaryReader reader, int serializationVersion)
        {
            Matrix4x4 matrix;

            matrix.m00 = reader.ReadSingle();
            matrix.m10 = reader.ReadSingle();
            matrix.m20 = reader.ReadSingle();
            matrix.m30 = reader.ReadSingle();

            matrix.m01 = reader.ReadSingle();
            matrix.m11 = reader.ReadSingle();
            matrix.m21 = reader.ReadSingle();
            matrix.m31 = reader.ReadSingle();

            matrix.m02 = reader.ReadSingle();
            matrix.m12 = reader.ReadSingle();
            matrix.m22 = reader.ReadSingle();
            matrix.m32 = reader.ReadSingle();

            matrix.m03 = reader.ReadSingle();
            matrix.m13 = reader.ReadSingle();
            matrix.m23 = reader.ReadSingle();
            matrix.m33 = reader.ReadSingle();

            return matrix;
        }

        /// <summary>
        ///     Reads a component that implements <see cref="IUxrUniqueId" />. Only the unique ID (string) will be read.
        ///     The component will be retrieved using the ID with <see cref="UxrUniqueIdImplementer.TryGetComponentById" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Component that implements the <see cref="IUxrUniqueId" /> interface</returns>
        /// <exception cref="UxrComponentNotFoundException">The given component could not be found using the Id</exception>
        public static IUxrUniqueId ReadUniqueComponent(this BinaryReader reader, int serializationVersion)
        {
            // Serialized as: null-check (bool), Guid

            bool nullCheck = reader.ReadBoolean();

            if (!nullCheck)
            {
                return null;
            }

            Guid componentId = reader.ReadGuid(serializationVersion);

            if (componentId == default)
            {
                return null;
            }

            if (UxrUniqueIdImplementer.TryGetComponentById(componentId, out IUxrUniqueId component))
            {
                return component;
            }

            throw new UxrComponentNotFoundException(componentId);
        }

        /// <summary>
        ///     Reads a component that implements <see cref="IUxrUniqueId" />. Only the unique ID (string) will be read.
        ///     The component will be retrieved using the ID with <see cref="UxrUniqueIdImplementer.TryGetComponentById" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Component that implements the <see cref="IUxrUniqueId" /> interface</returns>
        /// <exception cref="UxrComponentNotFoundException">The given component could not be found using the Id</exception>
        public static T ReadUniqueComponent<T>(this BinaryReader reader, int serializationVersion) where T : Component, IUxrUniqueId
        {
            return ReadUniqueComponent(reader, serializationVersion) as T;
        }

        /// <summary>
        ///     Reads an object that implements the <see cref="IUxrSerializable" /> interface. It has support for null references.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Object with the IUxrSerializable interface, which can be casted to the correct type</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        public static IUxrSerializable ReadUxrSerializable(this BinaryReader reader, int serializationVersion)
        {
            bool hasData = reader.ReadBoolean();

            if (!hasData)
            {
                return null;
            }

            int    version   = reader.ReadCompressedInt32(serializationVersion);
            Type   type      = reader.ReadType(serializationVersion, out string typeName, out string assemblyName);
            object newObject = type != null ? FormatterServices.GetUninitializedObject(type) : null;

            if (newObject == null)
            {
                throw new UxrSerializableClassNotFoundException(typeName, assemblyName, $"GetUninitializedObject({TypeExt.GetTypeString(typeName, assemblyName)}) returned null");
            }

            if (newObject is IUxrSerializable serializable)
            {
                serializable.Serialize(new UxrBinarySerializer(reader, serializationVersion), version);
                return serializable;
            }

            throw new UxrSerializableClassNotFoundException(typeName, assemblyName, $"Object {typeName} does not implement {nameof(IUxrSerializable)} interface");
        }

        /// <summary>
        ///     Reads an object that implements the <see cref="IUxrSerializable" /> interface. It has support for null references.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Object with the IUxrSerializable interface, which can be casted to the correct type</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Failed to cast the value to <see cref="IUxrSerializable"/>.
        /// </exception>
        public static T ReadUxrSerializable<T>(this BinaryReader reader, int serializationVersion) where T : IUxrSerializable
        {
            IUxrSerializable serializable = ReadUxrSerializable(reader, serializationVersion);

            if (serializable is T typedSerializable)
            {
                return typedSerializable;
            }

            throw new InvalidCastException($"Failed to cast {typeof(T).Name} to {nameof(IUxrSerializable)}");
        }

        /// <summary>
        ///     Reads an <see cref="UxrAxis" /> value.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <returns>Axis value</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        public static UxrAxis ReadAxis(this BinaryReader reader, int serializationVersion)
        {
            return ReadCompressedInt32(reader, serializationVersion);
        }

        /// <summary>
        ///     Reads an <see cref="object" /> supported by <see cref="UxrVarType" />. This is used together with
        ///     <see cref="BinaryWriterExt.WriteAnyVar" /> to serialize/deserialize variables whose type are unknown at compile
        ///     time.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <param name="varType">Returns the type of the object found, or <see cref="UxrVarType.Unknown" /></param>
        /// <returns>
        ///     Object or null. Null can be both because the object is unsupported or the object itself is null. Check
        ///     <paramref name="varType" /> to tell between the two cases
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Failed to cast the value to <see cref="IUxrSerializable"/>.
        /// </exception>
        public static object ReadAnyVar(this BinaryReader reader, int serializationVersion)
        {
            return reader.ReadAnyVar(serializationVersion, out UxrVarType _);
        }

        /// <summary>
        ///     Reads an <see cref="object" /> supported by <see cref="UxrVarType" />. This is used together with
        ///     <see cref="BinaryWriterExt.WriteAnyVar" /> to serialize/deserialize variables whose type are unknown at compile
        ///     time.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <param name="varType">Returns the type of the object found, or <see cref="UxrVarType.Unknown" /></param>
        /// <returns>
        ///     Object or null. Null can be both because the object is unknown or the object itself is null. Check
        ///     <paramref name="varType" /> to tell between the two cases
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Failed to cast the value to <see cref="IUxrSerializable"/>.
        /// </exception>
        public static object ReadAnyVar(this BinaryReader reader, int serializationVersion, out UxrVarType varType)
        {
            varType = (UxrVarType)reader.ReadByte();

            if (varType == UxrVarType.Unknown)
            {
                return null;
            }

            if (varType is UxrVarType.Bool)
            {
                return reader.ReadBoolean();
            }

            if (varType is UxrVarType.SignedByte)
            {
                return reader.ReadSByte();
            }

            if (varType is UxrVarType.Byte)
            {
                return reader.ReadByte();
            }

            if (varType is UxrVarType.Char)
            {
                return reader.ReadChar();
            }

            if (varType is UxrVarType.Int)
            {
                return reader.ReadCompressedInt32(serializationVersion);
            }

            if (varType is UxrVarType.UnsignedInt)
            {
                return reader.ReadCompressedUInt32(serializationVersion);
            }

            if (varType is UxrVarType.Long)
            {
                return reader.ReadCompressedInt64(serializationVersion);
            }

            if (varType is UxrVarType.UnsignedLong)
            {
                return reader.ReadCompressedUInt64(serializationVersion);
            }

            if (varType is UxrVarType.Float)
            {
                return reader.ReadSingle();
            }

            if (varType is UxrVarType.Double)
            {
                return reader.ReadDouble();
            }

            if (varType is UxrVarType.Decimal)
            {
                return reader.ReadDecimal();
            }

            if (varType is UxrVarType.String)
            {
                return reader.ReadString();
            }

            if (varType is UxrVarType.Enum)
            {
                Type enumType = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadEnum)).MakeGenericMethod(enumType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.Type)
            {
                return reader.ReadType(serializationVersion);
            }

            if (varType is UxrVarType.Guid)
            {
                return reader.ReadGuid(serializationVersion);
            }

            if (varType is UxrVarType.Tuple)
            {
                Type typeItem1 = reader.ReadType(serializationVersion);
                Type typeItem2 = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadTuple)).MakeGenericMethod(typeItem1, typeItem2).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.Array)
            {
                Type elementType = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadArray)).MakeGenericMethod(elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.ObjectArray)
            {
                return reader.ReadObjectArray(serializationVersion);
            }

            if (varType is UxrVarType.List)
            {
                Type elementType = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadList)).MakeGenericMethod(elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.ObjectList)
            {
                return reader.ReadObjectList(serializationVersion);
            }

            if (varType is UxrVarType.Dictionary)
            {
                Type keyType     = reader.ReadType(serializationVersion);
                Type elementType = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadDictionary)).MakeGenericMethod(keyType, elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.HashSet)
            {
                Type elementType = reader.ReadType(serializationVersion);
                return typeof(BinaryReaderExt).GetMethod(nameof(ReadHashSet)).MakeGenericMethod(elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (varType is UxrVarType.ObjectHashSet)
            {
                return reader.ReadObjectHashSet(serializationVersion);
            }

            if (varType is UxrVarType.DateTime)
            {
                return reader.ReadDateTime(serializationVersion);
            }

            if (varType is UxrVarType.TimeSpan)
            {
                return reader.ReadTimeSpan(serializationVersion);
            }

            if (varType is UxrVarType.Vector2)
            {
                return reader.ReadVector2(serializationVersion);
            }

            if (varType is UxrVarType.Vector3)
            {
                return reader.ReadVector3(serializationVersion);
            }

            if (varType is UxrVarType.Vector4)
            {
                return reader.ReadVector4(serializationVersion);
            }

            if (varType is UxrVarType.Color)
            {
                return reader.ReadColor(serializationVersion);
            }

            if (varType is UxrVarType.Color32)
            {
                return reader.ReadColor32(serializationVersion);
            }

            if (varType is UxrVarType.Quaternion)
            {
                return reader.ReadQuaternion(serializationVersion);
            }

            if (varType is UxrVarType.Matrix4x4)
            {
                return reader.ReadMatrix(serializationVersion);
            }

            if (varType is UxrVarType.IUxrUnique)
            {
                return reader.ReadUniqueComponent(serializationVersion);
            }

            if (varType is UxrVarType.IUxrSerializable)
            {
                return reader.ReadUxrSerializable(serializationVersion);
            }

            if (varType is UxrVarType.UxrAxis)
            {
                return reader.ReadAxis(serializationVersion);
            }

            throw new ArgumentOutOfRangeException(nameof(varType), varType, $"Deserializing unknown {nameof(UxrVarType)} ({varType})");
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Reads an object supported by <see cref="UxrVarType" />.
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>The object</returns>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="FormatException">The compressed data is corrupt</exception>
        /// <exception cref="UxrComponentNotFoundException">An <see cref="IUxrUniqueId" /> was not found when deserializing</exception>
        /// <exception cref="UxrSerializableClassNotFoundException">
        ///     A class that implements the <see cref="IUxrSerializable" />
        ///     interface was not found when deserializing
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Failed to cast the value to <see cref="IUxrSerializable"/>.
        /// </exception>
        private static object Read<T>(this BinaryReader reader, int serializationVersion)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                return reader.ReadBoolean();
            }

            if (type == typeof(sbyte))
            {
                return reader.ReadSByte();
            }

            if (type == typeof(byte))
            {
                return reader.ReadByte();
            }

            if (type == typeof(char))
            {
                return reader.ReadChar();
            }

            if (type == typeof(int))
            {
                return reader.ReadCompressedInt32(serializationVersion);
            }

            if (type == typeof(uint))
            {
                return reader.ReadCompressedUInt32(serializationVersion);
            }

            if (type == typeof(long))
            {
                return reader.ReadCompressedInt64(serializationVersion);
            }

            if (type == typeof(ulong))
            {
                return reader.ReadCompressedUInt64(serializationVersion);
            }

            if (type == typeof(float))
            {
                return reader.ReadSingle();
            }

            if (type == typeof(double))
            {
                return reader.ReadDouble();
            }

            if (type == typeof(decimal))
            {
                return reader.ReadDecimal();
            }

            if (type == typeof(string))
            {
                return reader.ReadString();
            }

            if (type.IsEnum)
            {
                return reader.ReadEnum<T>(serializationVersion);
            }

            if (type == typeof(Type))
            {
                return reader.ReadType(serializationVersion);
            }

            if (type == typeof(Guid))
            {
                return reader.ReadGuid(serializationVersion);
            }

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Tuple<,>) || type.GetGenericTypeDefinition() == typeof(ValueTuple<,>)))
            {
                Type typeItem1 = type.GetGenericArguments()[0];
                Type typeItem2 = type.GetGenericArguments()[1];

                return typeof(BinaryReaderExt).GetMethod(nameof(ReadTuple)).MakeGenericMethod(typeItem1, typeItem2).Invoke(reader, new object[] { reader, serializationVersion });
            }
            
            if (type.IsArray)
            {
                if (type.GetElementType() == typeof(object))
                {
                    return reader.ReadObjectArray(serializationVersion);
                }
                return reader.ReadArray<T>(serializationVersion);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = type.GetElementType();

                if (elementType == typeof(object))
                {
                    return reader.ReadObjectList(serializationVersion);
                }
                typeof(BinaryReaderExt).GetMethod(nameof(ReadList)).MakeGenericMethod(elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType   = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];

                return typeof(BinaryReaderExt).GetMethod(nameof(ReadDictionary)).MakeGenericMethod(keyType, valueType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                Type elementType = type.GetElementType();

                if (elementType == typeof(object))
                {
                    return reader.ReadObjectHashSet(serializationVersion);
                }
                typeof(BinaryReaderExt).GetMethod(nameof(ReadHashSet)).MakeGenericMethod(elementType).Invoke(reader, new object[] { reader, serializationVersion });
            }

            if (type == typeof(Vector2))
            {
                return reader.ReadVector2(serializationVersion);
            }

            if (type == typeof(Vector3))
            {
                return reader.ReadVector3(serializationVersion);
            }

            if (type == typeof(Vector4))
            {
                return reader.ReadVector4(serializationVersion);
            }

            if (type == typeof(Color))
            {
                return reader.ReadColor(serializationVersion);
            }

            if (type == typeof(Color32))
            {
                return reader.ReadColor32(serializationVersion);
            }

            if (type == typeof(Quaternion))
            {
                return reader.ReadQuaternion(serializationVersion);
            }

            if (type == typeof(Matrix4x4))
            {
                return reader.ReadMatrix(serializationVersion);
            }

            if (type == typeof(Matrix4x4))
            {
                return reader.ReadMatrix(serializationVersion);
            }

            if (typeof(IUxrUniqueId).IsAssignableFrom(type))
            {
                return reader.ReadUniqueComponent(serializationVersion);
            }

            if (typeof(IUxrSerializable).IsAssignableFrom(type))
            {
                return reader.ReadUxrSerializable(serializationVersion);
            }

            if (type == typeof(UxrAxis))
            {
                return reader.ReadAxis(serializationVersion);
            }

            throw new ArgumentOutOfRangeException(nameof(T), typeof(T).FullName, $"Deserializing to unknown target type ({typeof(T).FullName}");
        }

        #endregion
    }
}