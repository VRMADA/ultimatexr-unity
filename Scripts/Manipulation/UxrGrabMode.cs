// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     What controller input we need to grab and release.
    /// </summary>
    public enum UxrGrabMode
    {
        /// <summary>
        ///     Object is grabbed while the grab button is pressed.
        /// </summary>
        GrabWhilePressed,

        /// <summary>
        ///     One click on the grab button to grab, and another click to release it.
        /// </summary>
        GrabToggle,

        /// <summary>
        ///     Object will keep being grabbed. It can be released manually through <see cref="UxrGrabbableObject.ReleaseGrabs" />.
        /// </summary>
        GrabAndKeepAlways
    }
}