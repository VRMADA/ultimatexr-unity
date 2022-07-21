// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabProximityMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates how the distance from a <see cref="UxrGrabber" /> to a <see cref="UxrGrabbableObject" /> can be
    ///     computed.
    /// </summary>
    public enum UxrGrabProximityMode
    {
        /// <summary>
        ///     Use the proximity transform in the <see cref="UxrGrabber" />.
        /// </summary>
        UseProximity,

        /// <summary>
        ///     Use a <see cref="BoxCollider" /> inside which the <see cref="UxrGrabber" /> needs to be in order to grab the
        ///     object.
        /// </summary>
        BoxConstrained
    }
}