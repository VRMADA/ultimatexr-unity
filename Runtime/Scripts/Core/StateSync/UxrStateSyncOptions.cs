// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncOptions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Enumerates the different options when using a <see cref="UxrSyncEventArgs" /> with BeginSync().
    /// </summary>
    [Flags]
    public enum UxrStateSyncOptions
    {
        /// <summary>
        ///     No save/use.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Specifies the event should be synchronized over the network.
        ///     This feature helps save bandwidth on events that do not need network synchronization.
        ///     For instance, a Transform update might not be needed in a networking scenario because it has already been
        ///     synchronized using a NetworkTransform component more efficiently. For replays, where the networking component is
        ///     not working, the transform update will still be needed.
        /// </summary>
        Network = 1 << 0,

        /// <summary>
        ///     Specifies the event should be saved in replays.
        ///     For example, UxrGrabbableObject uses a coroutine that syncs grabbable objects with rigidbodies so that the
        ///     position/rotation and speed keep in sync in all devices every x seconds. This is used only in objects
        ///     that do not have specific networking components such as NetworkRigidbody or NetworkTransform.
        ///     In replays, the position and rotation are already sampled so there is no need to save these events.
        /// </summary>
        Replay = 1 << 1,

        /// <summary>
        ///     Forces to output a new sampling frame before and after the sync event when recording a replay. This can be used
        ///     to avoid interpolation errors when a certain event affects how values are interpolated.
        ///     For example re-parenting an object between two frames will create a jump if the position is recorded
        ///     in local space. Forcing to output a new frame will avoid this.
        /// </summary>
        GenerateNewFrame = 1 << 8,
        
        /// <summary>
        ///     Ignores nesting checks, which will generate <see cref="IUxrStateSync.StateChanged"/> events even when
        ///     the BeginSync/EndSync block is nested. 
        /// </summary>
        IgnoreNestingCheck = 1 << 9,

        /// <summary>
        ///     Save/use in all environments.
        /// </summary>
        Default = Network | Replay
    }
}