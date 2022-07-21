// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSync;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Describes an event raised by the <see cref="UxrGrabManager" /> that can also be played back. This facilitates the
    ///     manipulation synchronization through network.
    /// </summary>
    public class UxrManipulationSyncEventArgs : UxrStateSyncEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the event type described by <see cref="EventArgs" />.
        /// </summary>
        public UxrManipulationSyncEventType EventType { get; }

        /// <summary>
        ///     Gets the event parameters.
        /// </summary>
        public UxrManipulationEventArgs EventArgs { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="eventType">The event type described by <see cref="EventArgs" /></param>
        /// <param name="eventArgs">The event parameters</param>
        public UxrManipulationSyncEventArgs(UxrManipulationSyncEventType eventType, UxrManipulationEventArgs eventArgs)
        {
            EventType = eventType;
            EventArgs = eventArgs;
        }

        #endregion
    }
}