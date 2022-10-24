// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Base event args to synchronize the state of entities for network synchronization.
    /// </summary>
    /// <seealso cref="IUxrStateSync" />
    public abstract class UxrStateSyncEventArgs : EventArgs
    {
    }
}