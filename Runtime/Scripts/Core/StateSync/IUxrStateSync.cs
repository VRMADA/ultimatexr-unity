// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrStateSync.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     <para>
    ///         Interface for entities that are able to expose internal state changes described by a
    ///         <see cref="UxrStateSyncEventArgs" /> raised through a <see cref="StateChanged" /> event.
    ///         To support the synchronization, classes that implement this interface are also able to reproduce
    ///         state changes using <see cref="SyncState" />.
    ///     </para>
    ///     This interface should be implemented in entities relevant in network synchronization.
    /// </summary>
    public interface IUxrStateSync
    {
        #region Public Types & Data

        /// <summary>
        ///     Event raised when a relevant state of an object changed and requires storage/synchronization.
        /// </summary>
        event EventHandler<UxrStateSyncEventArgs> StateChanged;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Executes the state change described by <see cref="e" />.
        /// </summary>
        /// <param name="e">State change information</param>
        /// <param name="propagateEvents">Whether the event should propagate other internal events</param>
        void SyncState(UxrStateSyncEventArgs e, bool propagateEvents);

        #endregion
    }
}