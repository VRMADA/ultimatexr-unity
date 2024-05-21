// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Serialization.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Core.Instantiation;
using UltimateXR.Core.StateSave;

namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Serialization constants, used in state synchronization.
        /// </summary>
        public static class Serialization
        {
            #region Public Types & Data

            /// <summary>
            ///     Each time the serialization format of a component in UltimateXR is changed, this version number gets incremented by
            ///     one. The goal is to provide backwards compatibility and be able to deserialize old data as well as new.<br />
            ///     When serializing data, this version will be included somewhere. When deserializing data, the version it was
            ///     serialized with will be provided, enabling backwards compatibility.
            /// </summary>
            public const int CurrentBinaryVersion = 0;

            /// <summary>
            ///     Default <see cref="IUxrStateSave.SerializationOrder" /> value for <see cref="UxrInstanceManager" />.
            /// </summary>
            public const int SerializationOrderInstanceManager = SerializationOrderSingleton - 1000;

            /// <summary>
            ///     Default <see cref="IUxrStateSave.SerializationOrder" /> value for components derived from
            ///     <see cref="UxrAbstractSingleton" />.
            /// </summary>
            public const int SerializationOrderSingleton = -10000;

            /// <summary>
            ///     Default <see cref="IUxrStateSave.SerializationOrder" /> value for regular components.
            /// </summary>
            public const int SerializationOrderDefault = 0;

            #endregion
        }

        #endregion
    }
}