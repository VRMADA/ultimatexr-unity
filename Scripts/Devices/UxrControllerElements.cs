// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerElements.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates all possible elements that can be interacted with in a VR controller. Each controller can
    ///     describe which elements are supported through <see cref="UxrControllerInput.HasControllerElements" />.
    /// </summary>
    [Flags]
    public enum UxrControllerElements
    {
        None           = 0,
        Joystick       = 1,
        Joystick2      = 1 << 1,
        DPad           = 1 << 2,
        Trigger        = 1 << 3,
        Trigger2       = 1 << 4,
        Grip           = 1 << 5,
        ThumbCapSense  = 1 << 6,
        IndexCapSense  = 1 << 7,
        MiddleCapSense = 1 << 8,
        RingCapSense   = 1 << 9,
        LittleCapSense = 1 << 10,
        Button1        = 1 << 11,
        Button2        = 1 << 12,
        Button3        = 1 << 13,
        Button4        = 1 << 14,
        Bumper         = 1 << 15,
        Bumper2        = 1 << 16,
        Back           = 1 << 17,
        Menu           = 1 << 18,
        Everything     = ~None
    }
}