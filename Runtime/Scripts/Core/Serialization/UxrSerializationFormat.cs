// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSerializationFormat.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Enumerates the different serialization formats that are supported.
    /// </summary>
    public enum UxrSerializationFormat
    {
        /// <summary>
        ///     Binary uncompressed format.
        /// </summary>
        BinaryUncompressed,

        /// <summary>
        ///     Binary compressed using Gzip.
        /// </summary>
        BinaryGzip
    }
}