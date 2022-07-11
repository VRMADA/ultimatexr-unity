// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInput2D.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates the possible elements in a VR controller that have a 2-axis input.
    /// </summary>
    public enum UxrInput2D
    {
        /// <summary>
        ///     No element.
        /// </summary>
        None,

        /// <summary>
        ///     Controller joystick.
        /// </summary>
        Joystick,

        /// <summary>
        ///     Secondary joystick in a device that has two joysticks.
        /// </summary>
        Joystick2
    }
}