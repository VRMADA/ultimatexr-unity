// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWallFadeMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.CameraUtils
{
    /// <summary>
    ///     Enumerates the different working modes for <see cref="UxrCameraWallFade" />.
    /// </summary>
    public enum UxrWallFadeMode
    {
        /// <summary>
        ///     Fades to black when getting inside the geometry but allows to traverse and exit through the other side.
        /// </summary>
        AllowTraverse,

        /// <summary>
        ///     Doesn't allow traversing. The condition to fade-in back again is that the camera needs to have a straight line
        ///     without any traversing between the point where the head got in and the current position.
        /// </summary>
        Strict
    }
}