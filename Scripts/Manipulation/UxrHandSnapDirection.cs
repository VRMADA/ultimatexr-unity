// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandSnapDirection.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different ways snapping can be handled when grabbing an object.
    ///     For constrained objects, this will always be hand to the object
    /// </summary>
    public enum UxrHandSnapDirection
    {
        /// <summary>
        ///     The object will snap to the hand.
        /// </summary>
        ObjectToHand,

        /// <summary>
        ///     The hand will snap to the object.
        /// </summary>
        HandToObject
    }
}