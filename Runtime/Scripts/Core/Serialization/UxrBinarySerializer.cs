// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrBinarySerializer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using UltimateXR.Core.Math;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.System.IO;
using UnityEngine;

namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Class that helps serializing/deserializing data using a single method instead of two separate Serialize
    ///     and Deserialize methods. It also stores the serialization version when reading or writing data.
    /// </summary>
    public class UxrBinarySerializer : IUxrSerializer
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor for deserialization.
        /// </summary>
        /// <param name="reader">Binary reader with the data</param>
        /// <param name="version">Version that the data was serialized with</param>
        public UxrBinarySerializer(BinaryReader reader, int version)
        {
            Version   = version;
            IsReading = true;
            Reader    = reader;
        }

        /// <summary>
        ///     Constructor for serialization.
        /// </summary>
        /// <param name="writer">Binary writer to output the data</param>
        public UxrBinarySerializer(BinaryWriter writer)
        {
            Version   = UxrConstants.Serialization.CurrentBinaryVersion;
            IsReading = false;
            Writer    = writer;
        }

        #endregion

        #region Implicit IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion

        #region Implicit IUxrSerializer

        /// <summary>
        ///     Gets, when reading, the version that the data was serialized with. When writing, it gets the latest version that it
        ///     is being serialized with, which is equal to <see cref="UxrConstants.Serialization.CurrentBinaryVersion" />.
        /// </summary>
        public int Version { get; }

        /// <summary>
        ///     Gets whether the serializer is reading (using <see cref="Reader" />) or writing (using <see cref="Writer" />).
        /// </summary>
        public bool IsReading { get; }

        /// <inheritdoc />
        public void Serialize(ref bool value)
        {
            if (IsReading)
            {
                value = Reader.ReadBoolean();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref sbyte value)
        {
            if (IsReading)
            {
                value = Reader.ReadSByte();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref byte value)
        {
            if (IsReading)
            {
                value = Reader.ReadByte();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref char value)
        {
            if (IsReading)
            {
                value = Reader.ReadChar();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref int value)
        {
            if (IsReading)
            {
                value = Reader.ReadCompressedInt32(Version);
            }
            else
            {
                Writer.WriteCompressedInt32(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref uint value)
        {
            if (IsReading)
            {
                value = Reader.ReadCompressedUInt32(Version);
            }
            else
            {
                Writer.WriteCompressedUInt32(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref long value)
        {
            if (IsReading)
            {
                value = Reader.ReadCompressedInt64(Version);
            }
            else
            {
                Writer.WriteCompressedInt64(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref ulong value)
        {
            if (IsReading)
            {
                value = Reader.ReadCompressedUInt64(Version);
            }
            else
            {
                Writer.WriteCompressedUInt64(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref float value)
        {
            if (IsReading)
            {
                value = Reader.ReadSingle();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref double value)
        {
            if (IsReading)
            {
                value = Reader.ReadDouble();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref decimal value)
        {
            if (IsReading)
            {
                value = Reader.ReadDecimal();
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref string value)
        {
            if (IsReading)
            {
                bool nullCheck = Reader.ReadBoolean();
                value = nullCheck ? Reader.ReadString() : null;
            }
            else
            {
                Writer.Write(value != null);

                if (value != null)
                {
                    Writer.Write(value);
                }
            }
        }

        /// <inheritdoc />
        public void SerializeEnum<T>(ref T value) where T : Enum
        {
            if (IsReading)
            {
                value = Reader.ReadEnum<T>(Version);
            }
            else
            {
                Writer.WriteEnum(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Type value)
        {
            if (IsReading)
            {
                value = Reader.ReadType(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Guid value)
        {
            if (IsReading)
            {
                value = Reader.ReadGuid(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize<T1, T2>(ref (T1, T2) value)
        {
            if (IsReading)
            {
                value = Reader.ReadTuple<T1, T2>(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize<T>(ref T[] values)
        {
            if (IsReading)
            {
                values = Reader.ReadArray<T>(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref object[] values)
        {
            if (IsReading)
            {
                values = Reader.ReadObjectArray(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize<T>(ref List<T> values)
        {
            if (IsReading)
            {
                values = Reader.ReadList<T>(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref List<object> values)
        {
            if (IsReading)
            {
                values = Reader.ReadObjectList(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize<TKey, TValue>(ref Dictionary<TKey, TValue> values)
        {
            if (IsReading)
            {
                values = Reader.ReadDictionary<TKey, TValue>(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize<T>(ref HashSet<T> values)
        {
            if (IsReading)
            {
                values = Reader.ReadHashSet<T>(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref HashSet<object> values)
        {
            if (IsReading)
            {
                values = Reader.ReadObjectHashSet(Version);
            }
            else
            {
                Writer.Write(values);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref DateTime value)
        {
            if (IsReading)
            {
                value = Reader.ReadDateTime(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref TimeSpan value)
        {
            if (IsReading)
            {
                value = Reader.ReadTimeSpan(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Vector2 value)
        {
            if (IsReading)
            {
                value = Reader.ReadVector2(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Vector3 value)
        {
            if (IsReading)
            {
                value = Reader.ReadVector3(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Vector4 value)
        {
            if (IsReading)
            {
                value = Reader.ReadVector4(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Color value)
        {
            if (IsReading)
            {
                value = Reader.ReadColor(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Color32 value)
        {
            if (IsReading)
            {
                value = Reader.ReadColor32(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Quaternion value)
        {
            if (IsReading)
            {
                value = Reader.ReadQuaternion(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void Serialize(ref Matrix4x4 value)
        {
            if (IsReading)
            {
                value = Reader.ReadMatrix(Version);
            }
            else
            {
                Writer.Write(value);
            }
        }

        /// <inheritdoc />
        public void SerializeUniqueComponent(ref IUxrUniqueId unique)
        {
            if (IsReading)
            {
                unique = Reader.ReadUniqueComponent(Version);
            }
            else
            {
                Writer.WriteUniqueComponent(unique);
            }
        }

        /// <inheritdoc />
        public void SerializeUniqueComponent<T>(ref T component) where T : Component, IUxrUniqueId
        {
            if (IsReading)
            {
                component = Reader.ReadUniqueComponent(Version) as T;
            }
            else
            {
                Writer.WriteUniqueComponent(component);
            }
        }

        /// <inheritdoc />
        public void SerializeUxrSerializable(ref IUxrSerializable serializable)
        {
            if (IsReading)
            {
                serializable = Reader.ReadUxrSerializable(Version);
            }
            else
            {
                Writer.WriteUxrSerializable(serializable);
            }
        }

        /// <inheritdoc />
        public void SerializeUxrSerializable<T>(ref T obj) where T : IUxrSerializable
        {
            if (IsReading)
            {
                obj = (T)Reader.ReadUxrSerializable(Version);
            }
            else
            {
                Writer.WriteUxrSerializable(obj);
            }
        }

        /// <inheritdoc />
        public void SerializeAxis(ref UxrAxis axis)
        {
            if (IsReading)
            {
                axis = Reader.ReadAxis(Version);
            }
            else
            {
                Writer.WriteAxis(axis);
            }
        }

        /// <inheritdoc />
        public void SerializeAnyVar<T>(ref T obj)
        {
            if (IsReading)
            {
                obj = (T)Reader.ReadAnyVar(Version);
            }
            else
            {
                Writer.WriteAnyVar(obj);
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the reader, if <see cref="IsReading" /> is true. Otherwise it is null.
        /// </summary>
        private BinaryReader Reader { get; }

        /// <summary>
        ///     Gets the writer, if <see cref="IsReading" /> is false. Otherwise it is null.
        /// </summary>
        private BinaryWriter Writer { get; }

        #endregion
    }
}