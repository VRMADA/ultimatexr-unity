// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRenderModes.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Flags describing the different avatar render elements that can be enabled/disabled separately.
    /// </summary>
    [Flags]
    public enum UxrAvatarRenderModes
    {
        /// <summary>
        ///     Avatar isn't rendered.
        /// </summary>
        None,

        /// <summary>
        ///     Left input controller 3D model is rendered. In single controller setups, both left and right will target the same
        ///     controller.
        /// </summary>
        LeftController = 1,

        /// <summary>
        ///     Right input controller 3D model is rendered. In single controller setups, both left and right will target the same
        ///     controller.
        /// </summary>
        RightController = 1 << 1,

        /// <summary>
        ///     Avatar is rendered.
        /// </summary>
        Avatar = 1 << 2,

        /// <summary>
        ///     All input controllers are rendered.
        /// </summary>
        AllControllers = LeftController | RightController,

        /// <summary>
        ///     All input controllers and the avatar are rendered.
        /// </summary>
        AllControllersAndAvatar = Avatar | AllControllers
    }
}