// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveOptions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Flags for state saving options.
    /// </summary>
    [Flags]
    public enum UxrStateSaveOptions
    {
        /// <summary>
        ///     No options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Simulate the process, but do not read or write any data. The serialization counter
        ///     <see cref="UxrStateSaveImplementer{T}.SerializeCounter" /> will still be updated to know how many vars would be
        ///     serialized. The changes cache will be updated unless <see cref="DontCacheChanges" /> is also used.
        /// </summary>
        DontSerialize = 1 << 1,

        /// <summary>
        ///     Do not update the changes cache. The changes cache stores the last values that were serialized to make
        ///     sure to serialize changes only in incremental serializations (see <see cref="UxrStateSaveLevel" />).
        /// </summary>
        DontCacheChanges = 1 << 1,

        /// <summary>
        ///     Forces to set the current options as the initial and latest states.
        /// </summary>
        ForceResetChangesCache = 1 << 2
    }
}