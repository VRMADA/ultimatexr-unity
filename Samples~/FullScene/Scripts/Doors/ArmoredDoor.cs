// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArmoredDoor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.CameraUtils;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Doors
{
    /// <summary>
    ///     Component to model an automatic door that can not be opened from the outside unless
    ///     <see cref="AutomaticDoor.OpenDoor" /> is called explicitly.
    /// </summary>
    public class ArmoredDoor : AutomaticDoor
    {
        #region Protected Overrides AutomaticDoor

        /// <summary>
        ///     Gets whether automatic opening is allowed. Only from the inside and the if avatar is not peeking through geometry.
        /// </summary>
        protected override bool IsOpeningAllowed => IsAvatarInside(UxrAvatar.LocalAvatar) && !UxrCameraWallFade.IsAvatarPeekingThroughGeometry(UxrAvatar.LocalAvatar);

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the avatar is on the back side of the door.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <returns>
        ///     Whether the avatar is on the back side of the door. This means it is behind the plane pointed by the floor
        ///     center and looking towards the negative Z.
        /// </returns>
        private bool IsAvatarInside(UxrAvatar avatar)
        {
            Plane doorPlane = new Plane(FloorCenter.forward, FloorCenter.position);
            return !doorPlane.GetSide(avatar.CameraPosition);
        }

        #endregion
    }
}