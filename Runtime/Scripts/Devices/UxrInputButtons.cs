// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInputButtons.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates all buttons in a VR controller. They are flags so that they can be combined when calling
    ///     different methods.
    /// </summary>
    [Flags]
    public enum UxrInputButtons
    {
        None           = 0,
        Joystick       = 1,
        JoystickLeft   = 1 << 1,
        JoystickRight  = 1 << 2,
        JoystickUp     = 1 << 3,
        JoystickDown   = 1 << 4,
        Joystick2      = 1 << 5,
        Joystick2Left  = 1 << 6,
        Joystick2Right = 1 << 7,
        Joystick2Up    = 1 << 8,
        Joystick2Down  = 1 << 9,
        DPadLeft       = 1 << 10,
        DPadRight      = 1 << 11,
        DPadUp         = 1 << 12,
        DPadDown       = 1 << 13,
        Trigger        = 1 << 14,
        Trigger2       = 1 << 15,
        Grip           = 1 << 16,
        Button1        = 1 << 17,
        Button2        = 1 << 18,
        Button3        = 1 << 19,
        Button4        = 1 << 20,
        Bumper         = 1 << 21,
        Bumper2        = 1 << 22,
        Back           = 1 << 23,
        Menu           = 1 << 24,
        ThumbCapSense  = 1 << 25,
        IndexCapSense  = 1 << 26,
        MiddleCapSense = 1 << 27,
        RingCapSense   = 1 << 28,
        LittleCapSense = 1 << 29,
        Any            = 1 << 31,
        Everything     = ~None
    }
} 