// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrStateSync.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Serialization;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     <para>
    ///         Interface for classes whose state can be synchronized, enabling them to be used in networking environments or
    ///         saved to disk.<br />
    ///         They have 2 main features:
    ///         <list type="bullet">
    ///             <item>
    ///                 They expose relevant internal state changes through a <see cref="StateChanged" /> event. The state
    ///                 change is described by a <see cref="UxrSyncEventArgs" /> object. Each <see cref="UxrSyncEventArgs" />
    ///                 can be reproduced back using the <see cref="SyncState" /> method. This architecture can be used to
    ///                 listen for changes and reproduce them on the other clients, since <see cref="UxrSyncEventArgs" />
    ///                 objects can be serialized.
    ///             </item>
    ///             <item>
    ///                 They can load and save their complete state using <see cref="SerializeGlobalState" />. This can be used
    ///                 to synchronize the current state of a scene when a user joins an existing multi-user session. It also
    ///                 allows to save the state of a scene to disk, enabling the creation of save-state functionality or
    ///                 replays.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    public interface IUxrStateSync
    {
        #region Public Types & Data

        /// <summary>
        ///     Event raised when a relevant state of an object changed and requires storage/synchronization.
        /// </summary>
        event EventHandler<UxrSyncEventArgs> StateChanged;

        /// <summary>
        ///     Gets the name of the entity.
        /// </summary>
        /// <returns>Name of the entity</returns>
        public string SyncTargetName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Executes the state change described by <see cref="e" />.
        /// </summary>
        /// <param name="e">State change information</param>
        void SyncState(UxrSyncEventArgs e);

        /// <summary>
        ///     Serializes or deserializes the current global object state.
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        void SerializeGlobalState(IUxrSerializer serializer);

        #endregion
    }
}