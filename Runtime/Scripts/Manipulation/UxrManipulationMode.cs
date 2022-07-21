// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different ways a <see cref="UxrGrabbableObject" /> can be manipulated.
    /// </summary>
    public enum UxrManipulationMode
    {
        /// <summary>
        ///     Object can be moved around.
        /// </summary>
        GrabAndMove,

        /// <summary>
        ///     Object can only be rotated around an axis.
        /// </summary>
        RotateAroundAxis
    }
}