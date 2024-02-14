// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncEnvironments.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Enumerates the different environments a <see cref="UxrSyncEventArgs" /> must be used in.
    /// </summary>
    [Flags]
    public enum UxrStateSyncEnvironments
    {
        /// <summary>
        ///     No save/use.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Specifies the event should be synchronized over the network.
        ///     This feature helps saving bandwidth on events that do not need network synchronization.
        ///     For instance, a Transform update might not be needed in a networking scenario because it has already been
        ///     synchronized using a NetworkTransform component more efficiently. For replays, where the networking component is
        ///     not working, the transform update will still be needed.
        /// </summary>
        Network = 1 << 0,

        /// <summary>
        ///     Specifies the event should be saved in replays.
        ///     For example, UxrGrabManager uses a coroutine that syncs grabbable objects with rigidbodies so that the
        ///     position/rotation and speed keep in sync in all devices every x seconds. This is used only in objects
        ///     that do not have specific networking components such as NetworkRigidbody or NetworkTransform.
        ///     In replays, the position and rotation are already sampled so there is no need to save these events.
        /// </summary>
        Replay = 1 << 1,

        /// <summary>
        ///     Save/use in all environments.
        /// </summary>
        All = Network | Replay
    }
}