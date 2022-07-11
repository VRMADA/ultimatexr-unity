// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrElementType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     Enumerates the different input element types in a VR controller.
    /// </summary>
    public enum UxrElementType
    {
        /// <summary>
        ///     Not set.
        /// </summary>
        NotSet,

        /// <summary>
        ///     A button.
        /// </summary>
        Button,

        /// <summary>
        ///     An analog button that is rotated.
        /// </summary>
        Input1DRotate,

        /// <summary>
        ///     An analog button that is pushed.
        /// </summary>
        Input1DPush,

        /// <summary>
        ///     An analog joystick.
        /// </summary>
        Input2DJoystick,

        /// <summary>
        ///     An analog touch pad.
        /// </summary>
        Input2DTouch,

        /// <summary>
        ///     A digital directional pad.
        /// </summary>
        DPad
    }
}