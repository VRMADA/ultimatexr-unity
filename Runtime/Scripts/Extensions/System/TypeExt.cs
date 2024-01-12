// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     <see cref="Type" /> extensions.
    /// </summary>
    public static class TypeExt
    {
        #region Public Methods

        /// <summary>
        ///     Throws an <see cref="InvalidCastException" /> if the type defined by <paramref name="from" /> can't be casted to
        ///     the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="from">Source type</param>
        /// <param name="to">Destination type</param>
        /// <exception cref="InvalidCastException">Thrown when the source type can't be casted to the destination type</exception>
        public static void ThrowIfInvalidCast(Type from, Type to)
        {
            if (!IsValidCast(from, to))
            {
                throw new InvalidCastException($"{from.Name} is not assignable to {to.Name}");
            }
        }

        /// <summary>
        ///     Checks whether the type defined by <paramref name="from" /> can be casted to the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="from">Source type</param>
        /// <param name="to">Destination type</param>
        /// <returns>Whether it can be casted</returns>
        public static bool IsValidCast(Type from, Type to)
        {
            return to.IsAssignableFrom(from);
        }

        /// <summary>
        ///     Throws an <see cref="InvalidCastException" /> if the type defined by <paramref name="self" /> can't be casted to
        ///     the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="self">Source type</param>
        /// <param name="to">Destination type</param>
        /// <exception cref="InvalidCastException">Thrown when the source type can't be casted to the destination type</exception>
        public static void ThrowIfCannotCastTo(this Type self, Type to)
        {
            ThrowIfInvalidCast(self, to);
        }

        /// <summary>
        ///     Checks whether the type defined by <paramref name="self" /> can be casted to the type defined by <see cref="to" />.
        /// </summary>
        /// <param name="self">Source type</param>
        /// <param name="to">Destination type</param>
        /// <returns>Whether it can be casted</returns>
        public static bool CanCastTo(this Type self, Type to)
        {
            return IsValidCast(self, to);
        }

        /// <summary>
        ///     Gets a type given the assembly and the type name.
        /// </summary>
        /// <param name="typeName">Name of the type to get</param>
        /// <param name="assemblyName">Assembly name or null if the same assembly from the caller is used</param>
        /// <returns>Type or null if there was an error</returns>
        public static Type GetType(string typeName, string assemblyName)
        {
            string fullTypeName = string.IsNullOrEmpty(assemblyName) ? typeName : $"{typeName}, {assemblyName}";
            return Type.GetType(fullTypeName);
        }

        /// <summary>
        ///     Returns a string that describes the type given by the type name and the assembly where it is located. If the
        ///     assembly is empty or null, it won't return any assembly information and the type is considered to be in the same
        ///     assembly as UltimateXR.
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <param name="assemblyName">The name of the assembly. Null or empty to ignore assembly information</param>
        /// <returns>A string in the form of Type or Type, Assembly</returns>
        public static string GetTypeString(string typeName, string assemblyName)
        {
            string assemblyString = string.IsNullOrEmpty(assemblyName) ? string.Empty : $", {assemblyName}";
            return $"{typeName}{assemblyString}";
        }

        /// <summary>
        ///     Instantiates a given type which may be in the same or different assembly as the caller.
        /// </summary>
        /// <param name="typeName">Name of the type to instantiate</param>
        /// <param name="assemblyName">Assembly name or null/empty if the same assembly from the caller is used</param>
        /// <returns>New object or null if there was an error</returns>
        public static object CreateInstance(string typeName, string assemblyName)
        {
            Type type = GetType(typeName, assemblyName);
            return type == null ? null : Activator.CreateInstance(type, true);
        }

        /// <summary>
        ///     Instantiates a given type which may be in the same or different assembly as the caller.
        /// </summary>
        /// <param name="typeName">Name of the type to instantiate</param>
        /// <param name="assemblyName">Assembly name or null/empty if the same assembly from the caller is used</param>
        /// <param name="parameters">Optional parameters to call a specific constructor</param>
        /// <returns>New object or null if there was an error</returns>
        public static object CreateInstance(string typeName, string assemblyName, params object[] parameters)
        {
            Type type = GetType(typeName, assemblyName);
            return type == null ? null : Activator.CreateInstance(type, parameters);
        }

        #endregion
    }
}