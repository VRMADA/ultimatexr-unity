// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrButtonEventType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates the event types supported by a controller button action
    /// </summary>
    public enum UxrButtonEventType
    {
        /// <summary>
        ///     A finger has currently contact with the button but without pressing it.
        /// </summary>
        Touching,

        /// <summary>
        ///     A finger started contact started with the button on the current frame.
        /// </summary>
        TouchDown,

        /// <summary>
        ///     A finger removed contact with the button on the current frame.
        /// </summary>
        TouchUp,

        /// <summary>
        ///     A finger is currently pressing the button.
        /// </summary>
        Pressing,

        /// <summary>
        ///     A finger started pressing the button on the current frame.
        /// </summary>
        PressDown,

        /// <summary>
        ///     A finger released the button press on the current frame.
        /// </summary>
        PressUp
    }
}