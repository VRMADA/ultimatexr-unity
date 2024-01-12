// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrVarTypeExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UnityEngine;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Extensions for <see cref="UxrVarType" />.
    /// </summary>
    public static class UxrVarTypeExt
    {
        #region Public Methods

        /// <summary>
        ///     Tries to get the type of an object that support serialization/deserialization using UltimateXR.
        /// </summary>
        /// <param name="obj">Object to get the type of</param>
        /// <returns>Type from <see cref="UxrVarType" /> or <see cref="UxrVarType.Unknown" /> if not supported</returns>
        public static UxrVarType GetType(object obj)
        {
            if (obj == null)
            {
                return UxrVarType.Unknown;
            }

            Type type = obj.GetType();
            
            // C# types

            if (obj is bool)
            {
                return UxrVarType.Bool;
            }

            if (obj is sbyte)
            {
                return UxrVarType.SignedByte;
            }

            if (obj is byte)
            {
                return UxrVarType.Byte;
            }

            if (obj is char)
            {
                return UxrVarType.Char;
            }

            if (obj is int)
            {
                return UxrVarType.Int;
            }

            if (obj is uint)
            {
                return UxrVarType.UnsignedInt;
            }

            if (obj is long)
            {
                return UxrVarType.Long;
            }

            if (obj is ulong)
            {
                return UxrVarType.UnsignedLong;
            }

            if (obj is float)
            {
                return UxrVarType.Float;
            }

            if (obj is double)
            {
                return UxrVarType.Double;
            }

            if (obj is decimal)
            {
                return UxrVarType.Decimal;
            }

            if (obj is string)
            {
                return UxrVarType.String;
            }

            if (obj is Enum)
            {
                return UxrVarType.Enum;
            }

            if (obj is Type)
            {
                return UxrVarType.Type;
            }

            // C# collections
            
            if (type.IsArray)
            {
                return type.GetElementType() == typeof(object) ? UxrVarType.ObjectArray : UxrVarType.Array;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetElementType() == typeof(object) ? UxrVarType.ObjectList : UxrVarType.List;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return UxrVarType.Dictionary;
            }
            
            // Unity types

            if (obj is Vector2)
            {
                return UxrVarType.Vector2;
            }

            if (obj is Vector3)
            {
                return UxrVarType.Vector3;
            }

            if (obj is Vector4)
            {
                return UxrVarType.Vector4;
            }

            if (obj is Color)
            {
                return UxrVarType.Color;
            }

            if (obj is Color32)
            {
                return UxrVarType.Color32;
            }

            if (obj is Quaternion)
            {
                return UxrVarType.Quaternion;
            }

            if (obj is Matrix4x4)
            {
                return UxrVarType.Matrix4x4;
            }
            
            // UXR types

            if (obj is IUxrSerializable)
            {
                return UxrVarType.IUxrSerializable;
            }

            if (obj is UxrComponent)
            {
                return UxrVarType.UxrComponent;
            }

            if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
            {
                Debug.LogError($"{UxrConstants.CoreModule} Unknown type in {nameof(GetType)} ({type.FullName})");
            }

            return UxrVarType.Unknown;
        }

        #endregion
    }
}