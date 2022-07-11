// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInput1D.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates the possible elements in a VR controller that have a single axis input.
    /// </summary>
    public enum UxrInput1D
    {
        /// <summary>
        ///     No single axis element.
        /// </summary>
        None,

        /// <summary>
        ///     Analog grip button.
        /// </summary>
        Grip,

        /// <summary>
        ///     Analog trigger button.
        /// </summary>
        Trigger,

        /// <summary>
        ///     Secondary analog trigger button, in controllers that have two trigger buttons.
        /// </summary>
        Trigger2
    }
}