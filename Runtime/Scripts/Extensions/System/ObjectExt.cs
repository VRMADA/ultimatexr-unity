// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UltimateXR.Core;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.System.Math;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     <see cref="object" /> extensions.
    /// </summary>
    public static class ObjectExt
    {
        #region Public Methods

        /// <summary>
        ///     Compares two objects for equality, taking into account the content of collections for collection types.
        /// </summary>
        /// <param name="a">The first object to compare</param>
        /// <param name="b">The second object to compare</param>
        /// <returns><c>True</c> if the objects are equal; otherwise, <c>false</c></returns>
        public static bool ValuesEqual(this object a, object b)
        {
            return ValuesEqual(a, b, (ea, eb) => EnumerableExt.ContentEqual(ea, eb), (va, vb) => Equals(va, vb));
        }

        /// <summary>
        ///     Same as <see cref="ValuesEqual(object, object)" /> but using a precision threshold for the following types:
        ///     <list type="bullet">
        ///         <item>
        ///             <c>float</c>
        ///         </item>
        ///         <item>
        ///             <see cref="Vector2" />
        ///         </item>
        ///         <item>
        ///             <see cref="Vector3" />
        ///         </item>
        ///         <item>
        ///             <see cref="Vector4" />
        ///         </item>
        ///         <item>
        ///             <see cref="Quaternion" />
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="a">The first object to compare</param>
        /// <param name="b">The second object to compare</param>
        /// <param name="precisionThreshold">
        ///     The floating point precision threshold for the specific types listed above. The
        ///     <see cref="UxrConstants.Math.DefaultPrecisionThreshold" /> constant can be used to provide a standard precision
        ///     across all calls.
        /// </param>
        /// <returns><c>True</c> if the objects are equal; otherwise, <c>false</c></returns>
        public static bool ValuesEqual(this object a, object b, float precisionThreshold)
        {
            bool EqualsUsingPrecision(object a, object b, float precision)
            {
                // Check supported types with specific comparision using floating point precision

                if (a is float fa && b is float fb)
                {
                    return fa.EqualsUsingPrecision(fb, precision);
                }
                if (a is Vector3 v3a && b is Vector3 v3b)
                {
                    return v3a.EqualsUsingPrecision(v3b, precision);
                }
                if (a is Quaternion qa && b is Quaternion qb)
                {
                    return qa.EqualsUsingPrecision(qb, precision);
                }
                if (a is Vector2 v2a && b is Vector2 v2b)
                {
                    return v2a.EqualsUsingPrecision(v2b, precision);
                }
                if (a is Vector4 v4a && b is Vector4 v4b)
                {
                    return v4a.EqualsUsingPrecision(v4b, precision);
                }

                // Default comparison fallback

                return Equals(a, b);
            }

            return ValuesEqual(a, b, (ea, eb) => EnumerableExt.ContentEqual(ea, eb, precisionThreshold), (va, vb) => EqualsUsingPrecision(va, vb, precisionThreshold));
        }

        /// <summary>
        ///     Creates a deep copy of the specified object, including support for arrays, List&lt;T&gt;, and Dictionary&lt;TKey,
        ///     TValue&gt;.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deep copied</typeparam>
        /// <param name="obj">The object to be deep copied</param>
        /// <returns>A deep copy of the original object</returns>
        /// <remarks>
        ///     This method performs a deep copy, recursively copying all objects referenced by the original object.<br/>
        ///     Types derived from <see cref="Component" /> are not supported, and a reference to the same object will be returned
        ///     instead.<br/>
        ///     If the type of the object is an array, List&lt;T&gt;, or Dictionary&lt;TKey, TValue&gt;, it is handled natively.
        ///     If the type implements ICloneable, it uses the Clone method for copying.
        ///     For value types (primitive types and structs), the method returns the original object as they are inherently
        ///     deep-copied.
        ///     For other types, binary serialization is used for deep copying.
        /// </remarks>
        public static T DeepCopy<T>(this T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            if (obj is Component)
            {
                return obj;
            }

            Type type = obj.GetType();

            // Check if the type is an array
            if (type.IsArray)
            {
                Type  elementType   = type.GetElementType();
                Array originalArray = obj as Array;
                Array copiedArray   = Array.CreateInstance(elementType, originalArray.Length);
                for (int i = 0; i < originalArray.Length; i++)
                {
                    copiedArray.SetValue(DeepCopy(originalArray.GetValue(i)), i);
                }
                return (T)(object)copiedArray;
            }

            // Check if it's a List<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type  genericType  = type.GetGenericArguments()[0];
                IList originalList = (IList)obj;
                IList copiedList   = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));
                foreach (object item in originalList)
                {
                    copiedList.Add(DeepCopy(item));
                }
                return (T)copiedList;
            }

            // Check if it's a Dictionary<TKey, TValue>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[]      genericArguments = type.GetGenericArguments();
                Type        keyType          = genericArguments[0];
                Type        valueType        = genericArguments[1];
                IDictionary originalDict     = (IDictionary)obj;
                IDictionary copiedDict       = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                foreach (DictionaryEntry kvp in originalDict)
                {
                    copiedDict.Add(DeepCopy(kvp.Key), DeepCopy(kvp.Value));
                }
                return (T)copiedDict;
            }

            // Check if it's a HashSet<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                IEnumerable originalSet = (IEnumerable)obj;

                // Create a new HashSet<T> with the same element type
                HashSet<object> copiedSet = new HashSet<object>();

                foreach (object item in originalSet)
                {
                    copiedSet.Add(DeepCopy(item));
                }

                return (T)(object)copiedSet;
            }

            // Check if the type implements ICloneable
            if (typeof(ICloneable).IsAssignableFrom(type))
            {
                MethodInfo cloneMethod = type.GetMethod("Clone");
                if (cloneMethod != null)
                {
                    return (T)cloneMethod.Invoke(obj, null);
                }
            }

            // Check if it's a primitive type or string
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            // Use serialization for other types
            return BinarySerializationCopy(obj);
        }

        /// <summary>
        ///     Throws an exception if the object is null.
        /// </summary>
        /// <param name="self">Object to check</param>
        /// <param name="paramName">Parameter name, used as argument for the exceptions</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is null</exception>
        public static void ThrowIfNull(this object self, string paramName)
        {
            if (self is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Compares two objects for equality, taking into account the content of collections for collection types.
        /// </summary>
        /// <param name="a">The first object to compare</param>
        /// <param name="b">The second object to compare</param>
        /// <returns>True if the objects are equal; otherwise, false</returns>
        private static bool ValuesEqual(this object a, object b, Func<IEnumerable, IEnumerable, bool> enumerableComparer, Func<object, object, bool> valueComparer)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            // Check if both objects are collections
            if (a is IEnumerable enumerableA && b is IEnumerable enumerableB)
            {
                // Use enumerable comparer to compare the contents of collections
                return enumerableComparer(enumerableA, enumerableB);
            }

            // Use value comparer for non-collection objects
            return valueComparer(a, b);
        }

        /// <summary>
        ///     Gets a deep copy of an object using serialization.
        /// </summary>
        /// <param name="obj">Object to get a deep copy of</param>
        /// <typeparam name="T">The object type</typeparam>
        /// <returns>A deep copy of the object</returns>
        private static T BinarySerializationCopy<T>(T obj)
        {
            using MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter    formatter    = new BinaryFormatter();
            formatter.Serialize(memoryStream, obj);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(memoryStream);
        }

        #endregion
    }
}