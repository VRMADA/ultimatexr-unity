// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationEventType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different manipulation event types.
    /// </summary>
    public enum UxrManipulationEventType
    {
        Grab,
        Release,
        Place,
        Remove,
        GrabTrying,
        AnchorRangeEntered,
        AnchorRangeLeft,
        PlacedObjectRangeEntered,
        PlacedObjectRangeLeft
    }
}