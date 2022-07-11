// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSnapReference.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates which transforms can be used to align a <see cref="UxrGrabbableObject" /> to a <see cref="UxrGrabber" />
    /// </summary>
    public enum UxrSnapReference
    {
        /// <summary>
        ///     The <see cref="UxrGrabbableObject" /> own <see cref="Transform" /> is used.
        /// </summary>
        UseSelfTransform,

        /// <summary>
        ///     Another <see cref="Transform" /> is used.
        /// </summary>
        UseOtherTransform
    }
}