// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationSyncEventType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different events that can be synced for networking using <see cref="UxrManipulationSyncEventArgs" />
    ///     .
    /// </summary>
    public enum UxrManipulationSyncEventType
    {
        Grab,
        Release,
        Place,
        Remove
    }
}