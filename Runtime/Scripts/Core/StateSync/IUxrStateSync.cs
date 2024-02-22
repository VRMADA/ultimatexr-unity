// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrStateSync.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Unique;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     <para>
    ///         Interface for components to synchronize their state changes. State changes can be intercepted,
    ///         serialized, deserialized and be reproduced back in a different environment. This can be used to
    ///         synchronize state changes in a network session or save state changes to disk.
    ///     </para>
    ///     <para>
    ///         Relevant internal state changes are notified through a <see cref="StateChanged" /> event. The state
    ///         change is described by a <see cref="UxrSyncEventArgs" /> object. Each <see cref="UxrSyncEventArgs" />
    ///         can be reproduced back using the <see cref="SyncState" /> method. This architecture can be used to
    ///         listen for changes and reproduce them on the other clients, since <see cref="UxrSyncEventArgs" />
    ///         objects can be serialized.
    ///     </para>
    ///     <para>
    ///         To leverage the implementation of this interface, consider using <see cref="UxrStateSyncImplementer{T}" />.
    ///     </para>
    /// </summary>
    public interface IUxrStateSync : IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Event raised when a relevant state of a component changed and requires synchronization.
        /// </summary>
        event EventHandler<UxrSyncEventArgs> StateChanged;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Executes the state change described by <see cref="e" />.
        /// </summary>
        /// <param name="e">State change information</param>
        void SyncState(UxrSyncEventArgs e);

        #endregion
    }
}