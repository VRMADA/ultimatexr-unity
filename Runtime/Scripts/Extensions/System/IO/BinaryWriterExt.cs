// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryWriterExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UltimateXR.Core.Math;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
using UnityEngine;

namespace UltimateXR.Extensions.System.IO
{
    /// <summary>
    ///     <see cref="BinaryWriter" /> extensions.
    /// </summary>
    public static class BinaryWriterExt
    {
        #region Public Methods

        /// <summary>Writes a 32-bit integer in a compressed format, so that only the required number of bytes are used.</summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">Int32 value</param>
        public static void WriteCompressedInt32(this BinaryWriter writer, int value)
        {
            WriteCompressedUInt32(writer, (uint)value);
        }

        /// <summary>Writes a 32-bit unsigned integer in a compressed format, so that only the required number of bytes are used.</summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">UInt32 value</param>
        public static void WriteCompressedUInt32(this BinaryWriter writer, uint value)
        {
            uint num;

            for (num = value; num >= 128U; num >>= 7)
            {
                writer.Write((byte)(num | 128U));
            }

            writer.Write((byte)num);
        }

        /// <summary>Writes a 64-bit integer in a compressed format, so that only the required number of bytes are used.</summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">Int64 value</param>
        public static void WriteCompressedInt64(this BinaryWriter writer, long value)
        {
            WriteCompressedUInt64(writer, (ulong)value);
        }

        /// <summary>Writes a 64-bit unsigned integer in a compressed format, so that only the required number of bytes are used.</summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">UInt64 value</param>
        public static void WriteCompressedUInt64(this BinaryWriter writer, ulong value)
        {
            ulong num;

            for (num = value; num >= 128UL; num >>= 7)
            {
                writer.Write((byte)(num | 128UL));
            }

            writer.Write((byte)num);
        }

        /// <summary>
        ///     Outputs an enum value.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="enumValue">Enum value</param>
        public static void WriteEnum(this BinaryWriter writer, Enum enumValue)
        {
            writer.WriteCompressedInt32((int)(object)enumValue);
        }

        /// <summary>
        ///     Outputs a string with null support. Strings should be read back using
        ///     <see cref="BinaryReaderExt.ReadStringWithNullCheck" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">String value or null</param>
        public static void WriteStringWithNullCheck(this BinaryWriter writer, string value)
        {
            // Serialized as: null-check (bool), string

            writer.Write(value != null);

            if (value != null)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        ///     Outputs a type. It will output two strings: the type full name and the assembly name. If the type belongs to the
        ///     UltimateXR assembly, the assembly will be an empty string.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="type">The type</param>
        public static void Write(this BinaryWriter writer, Type type)
        {
            if (type == null)
            {
                writer.Write(string.Empty);
                return;
            }

            bool isUxrAssembly = type.Assembly == typeof(BinaryWriterExt).Assembly;

            writer.Write(type.FullName);
            writer.Write(isUxrAssembly ? string.Empty : type.Assembly.GetName().Name);
        }

        /// <summary>
        ///     Outputs a Guid as a 16 byte array.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="guid">The Guid</param>
        public static void Write(this BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }

        /// <summary>
        ///     Outputs a tuple.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="tuple">The tuple</param>
        public static void Write<T1, T2>(this BinaryWriter writer, (T1, T2) tuple)
        {
            writer.Write(tuple.Item1);
            writer.Write(tuple.Item2);
        }

        /// <summary>
        ///     Outputs an array.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <typeparam name="T">Tye element type</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write<T>(this BinaryWriter writer, T[] values)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(values != null);

            if (values != null)
            {
                writer.WriteCompressedInt32(values.Length);

                foreach (T value in values)
                {
                    writer.Write(value);
                }
            }
        }

        /// <summary>
        ///     Outputs an array where each element can be of a different type.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write(this BinaryWriter writer, object[] values)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(values != null);

            if (values != null)
            {
                writer.WriteCompressedInt32(values.Length);

                foreach (object value in values)
                {
                    writer.WriteAnyVar(value);
                }
            }
        }

        /// <summary>
        ///     Outputs a list.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <typeparam name="T">The element type</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write<T>(this BinaryWriter writer, List<T> values)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(values != null);

            if (values != null)
            {
                writer.WriteCompressedInt32(values.Count);

                foreach (T value in values)
                {
                    writer.Write(value);
                }
            }
        }

        /// <summary>
        ///     Outputs a list where each element can be of a different type.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write(this BinaryWriter writer, List<object> values)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(values != null);

            if (values != null)
            {
                writer.WriteCompressedInt32(values.Count);

                foreach (object value in values)
                {
                    writer.WriteAnyVar(value);
                }
            }
        }

        /// <summary>
        ///     Outputs a dictionary.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="dictionary">The dictionary</param>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the values</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write<TKey, TValue>(this BinaryWriter writer, Dictionary<TKey, TValue> dictionary)
        {
            // Serialized as: null-check (bool), count (int32), elements (pairs)

            writer.Write(dictionary != null);

            if (dictionary != null)
            {
                writer.WriteCompressedInt32(dictionary.Count);

                foreach (KeyValuePair<TKey, TValue> element in dictionary)
                {
                    writer.Write(element.Key);
                    writer.Write(element.Value);
                }
            }
        }

        /// <summary>
        ///     Outputs a HashSet.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write<T>(this BinaryWriter writer, HashSet<T> hashSet)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(hashSet != null);

            if (hashSet != null)
            {
                writer.WriteCompressedInt32(hashSet.Count);

                foreach (T value in hashSet)
                {
                    writer.Write(value);
                }
            }
        }

        /// <summary>
        ///     Outputs a hash set where each element can be of a different type.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="values">Values</param>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write(this BinaryWriter writer, HashSet<object> values)
        {
            // Serialized as: null-check (bool), count (int32), elements

            writer.Write(values != null);

            if (values != null)
            {
                writer.WriteCompressedInt32(values.Count);

                foreach (object value in values)
                {
                    writer.WriteAnyVar(value);
                }
            }
        }

        /// <summary>
        ///     Outputs a <see cref="DateTime" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">Value</param>
        public static void Write(this BinaryWriter writer, in DateTime value)
        {
            writer.WriteCompressedInt64(value.Ticks);
        }

        /// <summary>
        ///     Outputs a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="value">Value</param>
        public static void Write(this BinaryWriter writer, in TimeSpan value)
        {
            writer.WriteCompressedInt64(value.Ticks);
        }

        /// <summary>
        ///     Outputs a <see cref="Vector2" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="vec">Vector</param>
        public static void Write(this BinaryWriter writer, in Vector2 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
        }

        /// <summary>
        ///     Outputs a <see cref="Vector3" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="vec">Vector</param>
        public static void Write(this BinaryWriter writer, in Vector3 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
        }

        /// <summary>
        ///     Outputs a <see cref="Vector4" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="vec">Vector</param>
        public static void Write(this BinaryWriter writer, in Vector4 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
            writer.Write(vec.w);
        }

        /// <summary>
        ///     Outputs a <see cref="Color" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="color">Color</param>
        public static void Write(this BinaryWriter writer, in Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        /// <summary>
        ///     Outputs a <see cref="Color32" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="color">Color32</param>
        public static void Write(this BinaryWriter writer, in Color32 color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        /// <summary>
        ///     Outputs a <see cref="Quaternion" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="quaternion">Quaternion</param>
        public static void Write(this BinaryWriter writer, in Quaternion quaternion)
        {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }

        /// <summary>
        ///     Outputs a <see cref="Matrix4x4" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="matrix">Matrix</param>
        public static void Write(this BinaryWriter writer, in Matrix4x4 matrix)
        {
            writer.Write(matrix.m00);
            writer.Write(matrix.m10);
            writer.Write(matrix.m20);
            writer.Write(matrix.m30);

            writer.Write(matrix.m01);
            writer.Write(matrix.m11);
            writer.Write(matrix.m21);
            writer.Write(matrix.m31);

            writer.Write(matrix.m02);
            writer.Write(matrix.m12);
            writer.Write(matrix.m22);
            writer.Write(matrix.m32);

            writer.Write(matrix.m03);
            writer.Write(matrix.m13);
            writer.Write(matrix.m23);
            writer.Write(matrix.m33);
        }

        /// <summary>
        ///     Outputs a component with the <see cref="IUxrUniqueId" /> interface. Only the unique ID will be serialized.
        ///     The component can be retrieved using the ID with <see cref="UxrUniqueIdImplementer.TryGetComponentById" />. It has
        ///     support for null references.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="component">Component with the <see cref="IUxrUniqueId" /> interface</param>
        public static void WriteUniqueComponent(this BinaryWriter writer, IUxrUniqueId component)
        {
            writer.Write(component != null);

            if (component != null)
            {
                writer.Write(component.UniqueId);
            }
        }

        /// <summary>
        ///     Outputs an object that implements the <see cref="IUxrSerializable" /> interface. It has support for null
        ///     references.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="serializable">Object with the <see cref="IUxrSerializable" /> interface</param>
        /// <exception cref="ArgumentOutOfRangeException">A type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void WriteUxrSerializable(this BinaryWriter writer, IUxrSerializable serializable)
        {
            if (serializable == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);

            // Write serialization version of the serializable, to provide backwards compatibility
            writer.WriteCompressedInt32(serializable.SerializationVersion);

            // Write serializable type
            writer.Write(serializable.GetType());

            // Serialization using interface
            serializable.Serialize(new UxrBinarySerializer(writer), serializable.SerializationVersion);
        }

        /// <summary>
        ///     Outputs an UxrAxis value.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="axis"><see cref="UxrAxis" /> value</param>
        public static void WriteAxis(this BinaryWriter writer, UxrAxis axis)
        {
            // Serialize as a compressed int (0, 1 or 2).
            writer.WriteCompressedInt32((int)(axis ?? 0));
        }

        /// <summary>
        ///     Outputs an object supported by <see cref="UxrVarType" /> together with its type, so that it can be
        ///     deserialized using <see cref="BinaryReaderExt.ReadAnyVar(System.IO.BinaryReader,int)" />. This can be useful when
        ///     the serialized object type is only known at runtime.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="obj">Value</param>
        /// <exception cref="ArgumentOutOfRangeException">The type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void WriteAnyVar<T>(this BinaryWriter writer, T obj)
        {
            // First write type

            UxrVarType varType = UxrVarTypeExt.GetType(obj);
            writer.Write((byte)varType);

            if (varType == UxrVarType.Unknown)
            {
                return;
            }

            // Write additional data for some types

            Type type = obj.GetType();

            if (obj is Enum enumValue)
            {
                // Enum type
                writer.Write(enumValue.GetType());
            }
            
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Tuple<,>) || type.GetGenericTypeDefinition() == typeof(ValueTuple<,>)))
            {
                // Item types
                writer.Write(type.GetGenericArguments()[0]);
                writer.Write(type.GetGenericArguments()[1]);
            }
            
            if (type.IsArray && type.GetElementType() != typeof(object))
            {
                // Array element type
                writer.Write(type.GetElementType());
            }

            if (type.IsGenericType && type.GetElementType() != typeof(object) && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // List element type
                writer.Write(type.GetGenericArguments()[0]);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Key & value types
                writer.Write(type.GetGenericArguments()[0]);
                writer.Write(type.GetGenericArguments()[1]);
            }

            // Write value

            writer.Write(obj, type);
        }

        /// <summary>
        ///     Generic method that outputs an object supported by <see cref="UxrVarType" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="obj">The object</param>
        /// <typeparam name="T">The type</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">The type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        public static void Write<T>(this BinaryWriter writer, T obj)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                writer.Write(obj is true);
                return;
            }

            if (type == typeof(sbyte))
            {
                writer.Write(obj is sbyte sbyteValue ? sbyteValue : (sbyte)0);
                return;
            }

            if (type == typeof(byte))
            {
                writer.Write(obj is byte byteValue ? byteValue : (byte)0);
                return;
            }

            if (type == typeof(char))
            {
                writer.Write(obj is char charValue ? charValue : (char)0);
                return;
            }

            if (type == typeof(int))
            {
                writer.WriteCompressedInt32(obj is int intValue ? intValue : 0);
                return;
            }

            if (type == typeof(uint))
            {
                writer.WriteCompressedUInt32(obj is uint uintValue ? uintValue : 0);
                return;
            }

            if (type == typeof(long))
            {
                writer.WriteCompressedInt64(obj is long longValue ? longValue : 0);
                return;
            }

            if (type == typeof(ulong))
            {
                writer.WriteCompressedUInt64(obj is ulong ulongValue ? ulongValue : 0);
                return;
            }

            if (type == typeof(float))
            {
                writer.Write(obj is float floatValue ? floatValue : 0);
                return;
            }

            if (type == typeof(double))
            {
                writer.Write(obj is double doubleValue ? doubleValue : 0);
                return;
            }

            if (type == typeof(decimal))
            {
                writer.Write(obj is decimal decimalValue ? decimalValue : 0);
                return;
            }

            if (type == typeof(string))
            {
                writer.Write(obj as string ?? string.Empty);
                return;
            }

            if (obj is Enum enumValue)
            {
                writer.WriteEnum(enumValue);
                return;
            }

            if (type == typeof(Type))
            {
                writer.Write(obj as Type);
                return;
            }

            if (type == typeof(Guid))
            {
                writer.Write(obj is Guid guid ? guid : default);
                return;
            }

            if (type == typeof(Guid))
            {
                writer.Write(obj is Guid guid ? guid : default);
                return;
            }

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Tuple<,>) || type.GetGenericTypeDefinition() == typeof(ValueTuple<,>)))
            {
                Type typeItem1 = type.GetGenericArguments()[0];
                Type typeItem2 = type.GetGenericArguments()[1];
                GetGenericWriteTupleMethod().MakeGenericMethod(typeItem1, typeItem2).Invoke(writer, new object[] { writer, obj });
                return;
            }

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();

                if (elementType != typeof(object))
                {
                    GetGenericWriteArrayMethod().MakeGenericMethod(elementType).Invoke(writer, new object[] { writer, obj });
                }
                else
                {
                    writer.Write(obj as object[]);
                }

                return;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = type.GetGenericArguments()[0];

                if (elementType != typeof(object))
                {
                    GetGenericWriteListMethod().MakeGenericMethod(elementType).Invoke(writer, new object[] { writer, obj });
                }
                else
                {
                    writer.Write(obj as List<object>);
                }

                return;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType   = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];
                GetDictionaryWriteMethod().MakeGenericMethod(keyType, valueType).Invoke(writer, new object[] { writer, obj });
                return;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                Type elementType = type.GetGenericArguments()[0];

                if (elementType != typeof(object))
                {
                    GetGenericWriteHashSetMethod().MakeGenericMethod(elementType).Invoke(writer, new object[] { writer, obj });
                }
                else
                {
                    writer.Write(obj as HashSet<object>);
                }

                return;
            }

            if (type == typeof(DateTime))
            {
                writer.Write(obj is DateTime dateTime ? dateTime : default);
                return;
            }

            if (type == typeof(TimeSpan))
            {
                writer.Write(obj is TimeSpan timeSpan ? timeSpan : default);
                return;
            }

            if (type == typeof(Vector2))
            {
                writer.Write(obj is Vector2 vector2 ? vector2 : default);
                return;
            }

            if (type == typeof(Vector3))
            {
                writer.Write(obj is Vector3 vector3 ? vector3 : default);
                return;
            }

            if (type == typeof(Vector4))
            {
                writer.Write(obj is Vector4 vector4 ? vector4 : default);
                return;
            }

            if (type == typeof(Color))
            {
                writer.Write(obj is Color color ? color : default);
                return;
            }

            if (type == typeof(Color32))
            {
                writer.Write(obj is Color32 color ? color : default);
                return;
            }

            if (type == typeof(Quaternion))
            {
                writer.Write(obj is Quaternion quaternion ? quaternion : default);
                return;
            }

            if (type == typeof(Matrix4x4))
            {
                writer.Write(obj is Matrix4x4 matrix ? matrix : default);
                return;
            }

            if (typeof(IUxrUniqueId).IsAssignableFrom(type))
            {
                writer.WriteUniqueComponent(obj as IUxrUniqueId);
                return;
            }

            if (typeof(IUxrSerializable).IsAssignableFrom(type))
            {
                writer.WriteUxrSerializable(obj as IUxrSerializable);
                return;
            }

            if (type == typeof(UxrAxis))
            {
                writer.WriteAxis(obj as UxrAxis);
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(obj), obj, $"Serializing unknown target type {typeof(T).FullName}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Outputs an object supported by <see cref="UxrVarType" />.
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="obj">The object</param>
        /// <param name="type">The type of the object</param>
        /// <exception cref="ArgumentOutOfRangeException">The type is not supported</exception>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static void Write<T>(this BinaryWriter writer, T obj, Type type)
        {
            if (s_genericWriteMethod == null)
            {
                // Try to find and cache generic Write<T> method for T types.

                s_genericWriteMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                       m.IsGenericMethod &&
                                                                                       m.GetGenericArguments().Count() == 1 &&
                                                                                       m.GetParameters().Count() == 2 &&
                                                                                       m.GetParameters()[1].ParameterType.GetInterface(nameof(IEnumerable)) == null)
                                                              .FirstOrDefault();

                if (s_genericWriteMethod == null)
                {
                    throw new NotSupportedException($"Cannot serialize because generic {nameof(Write)} method was not found");
                }
            }

            s_genericWriteMethod.MakeGenericMethod(type).Invoke(writer, new object[] { writer, obj });
        }

        /// <summary>
        ///     Tries to find the <see cref="MethodInfo" /> to use the Write method for tuples.
        /// </summary>
        /// <returns>MethodInfo</returns>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static MethodInfo GetGenericWriteTupleMethod()
        {
            if (s_genericWriteTupleMethod != null)
            {
                return s_genericWriteTupleMethod;
            }

            // Try to find and cache Write<T1, T2> method for tuples.

            s_genericWriteTupleMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                        m.IsGenericMethod &&
                                                                                        m.GetGenericArguments().Count() == 2 &&
                                                                                        m.GetParameters().Count() == 2 &&
                                                                                        m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                        (m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Tuple<,>) ||
                                                                                         m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))).FirstOrDefault();

            if (s_genericWriteTupleMethod == null)
            {
                throw new NotSupportedException($"Cannot serialize because {nameof(Write)} method for generic tuples was not found");
            }

            return s_genericWriteTupleMethod;
        }

        /// <summary>
        ///     Tries to find the <see cref="MethodInfo" /> to use the Write method for arrays.
        /// </summary>
        /// <returns>MethodInfo</returns>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static MethodInfo GetGenericWriteArrayMethod()
        {
            if (s_genericWriteArrayMethod != null)
            {
                return s_genericWriteArrayMethod;
            }

            // Try to find and cache Write<T> method for arrays of T.

            s_genericWriteArrayMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                        m.IsGenericMethod &&
                                                                                        m.GetGenericArguments().Count() == 1 &&
                                                                                        m.GetParameters().Count() == 2 &&
                                                                                        m.GetParameters()[1].ParameterType.IsArray).FirstOrDefault();

            if (s_genericWriteArrayMethod == null)
            {
                throw new NotSupportedException($"Cannot serialize because {nameof(Write)} method for generic arrays was not found");
            }

            return s_genericWriteArrayMethod;
        }

        /// <summary>
        ///     Tries to find the <see cref="MethodInfo" /> to use the Write method for lists.
        /// </summary>
        /// <returns>MethodInfo</returns>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static MethodInfo GetGenericWriteListMethod()
        {
            if (s_genericWriteListMethod != null)
            {
                return s_genericWriteListMethod;
            }

            // Try to find and cache Write<T> method for lists of T.

            s_genericWriteListMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                       m.IsGenericMethod &&
                                                                                       m.GetGenericArguments().Count() == 1 &&
                                                                                       m.GetParameters().Count() == 2 &&
                                                                                       m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                       m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(List<>)).FirstOrDefault();

            if (s_genericWriteListMethod == null)
            {
                throw new NotSupportedException($"Cannot serialize because {nameof(Write)} method for generic lists was not found");
            }

            return s_genericWriteListMethod;
        }

        /// <summary>
        ///     Tries to find the <see cref="MethodInfo" /> to use the Write method for hash sets.
        /// </summary>
        /// <returns>MethodInfo</returns>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static MethodInfo GetGenericWriteHashSetMethod()
        {
            if (s_genericWriteHashSetMethod != null)
            {
                return s_genericWriteHashSetMethod;
            }

            // Try to find and cache Write<T> method for hash sets of T.

            s_genericWriteHashSetMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                          m.IsGenericMethod &&
                                                                                          m.GetGenericArguments().Count() == 1 &&
                                                                                          m.GetParameters().Count() == 2 &&
                                                                                          m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                          m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(HashSet<>)).FirstOrDefault();

            if (s_genericWriteHashSetMethod == null)
            {
                throw new NotSupportedException($"Cannot serialize because {nameof(Write)} method for generic hash sets was not found");
            }

            return s_genericWriteHashSetMethod;
        }

        /// <summary>
        ///     Tries to find the <see cref="MethodInfo" /> to use the Write method for dictionaries.
        /// </summary>
        /// <returns>MethodInfo</returns>
        /// <exception cref="NotSupportedException">
        ///     A method obtained through reflection that was required for serialization could
        ///     not be found
        /// </exception>
        private static MethodInfo GetDictionaryWriteMethod()
        {
            if (s_dictionaryWriteMethod != null)
            {
                return s_dictionaryWriteMethod;
            }

            // Try to find and cache Write<TKey, TValue> method for dictionaries.

            s_dictionaryWriteMethod = typeof(BinaryWriterExt).GetMethods().Where(m => m.Name == nameof(Write) &&
                                                                                      m.IsGenericMethod &&
                                                                                      m.GetGenericArguments().Count() == 2 &&
                                                                                      m.GetParameters().Count() == 2 &&
                                                                                      m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                      m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Dictionary<,>)).FirstOrDefault();

            if (s_dictionaryWriteMethod == null)
            {
                throw new NotSupportedException($"Cannot serialize because {nameof(Write)} method for dictionaries was not found");
            }

            return s_dictionaryWriteMethod;
        }

        #endregion

        #region Private Types & Data

        private static MethodInfo s_genericWriteTupleMethod;
        private static MethodInfo s_genericWriteArrayMethod;
        private static MethodInfo s_genericWriteListMethod;
        private static MethodInfo s_genericWriteHashSetMethod;
        private static MethodInfo s_dictionaryWriteMethod;
        private static MethodInfo s_genericWriteMethod;

        #endregion
    }
}