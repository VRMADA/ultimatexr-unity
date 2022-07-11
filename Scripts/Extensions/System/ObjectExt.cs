// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     <see cref="object" /> extensions.
    /// </summary>
    public static class ObjectExt
    {
        #region Public Methods

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
    }
}