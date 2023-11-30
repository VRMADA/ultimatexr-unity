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
        ///     Avatar isn't rendered. All components will still work, which means the avatar can still interact with the
        ///     environment. It can be used in mixed reality for example to let the hand colliders interact with
        ///     the scenario even though the hands aren't rendered.
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