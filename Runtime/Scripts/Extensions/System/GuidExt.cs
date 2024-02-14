// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Security.Cryptography;

namespace UltimateXR.Extensions.System
{
    public static class GuidExt
    {
        #region Public Methods

        /// <summary>
        ///     Throws an exception if the object has the default Guid.
        /// </summary>
        /// <param name="self">Guid to check</param>
        /// <param name="paramName">Parameter name, used as argument for the exception message or null to not use it</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is the default Guid (default or <see cref="Guid.Empty" />)</exception>
        public static void ThrowIfDefault(this Guid self, string paramName)
        {
            if (self == Guid.Empty)
            {
                if (paramName == null)
                {
                    throw new Exception("Guid cannot be empty");
                }
                
                throw new Exception($"Guid is empty for parameter {paramName}");
            }
        }

        /// <summary>
        ///     Combines two Guid instances to create a new unique Guid.
        /// </summary>
        /// <param name="guid1">The first Guid to combine</param>
        /// <param name="guid2">The second Guid to combine</param>
        /// <returns>A new Guid that is a combination of the input Guids</returns>
        public static Guid Combine(Guid guid1, Guid guid2)
        {
            byte[] bytes1 = guid1.ToByteArray();
            byte[] bytes2 = guid2.ToByteArray();

            // Concatenate the byte arrays
            byte[] combinedBytes = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, combinedBytes, 0,             bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, combinedBytes, bytes1.Length, bytes2.Length);

            // Use SHA-256 hash function to generate a unique hash
            using SHA256 sha256    = SHA256.Create();
            byte[]       hashBytes = sha256.ComputeHash(combinedBytes);

            // Take the first 16 bytes of the hash to create a new Guid
            byte[] guidBytes = new byte[16];
            Buffer.BlockCopy(hashBytes, 0, guidBytes, 0, guidBytes.Length);

            return new Guid(guidBytes);
        }

        #endregion
    }
}