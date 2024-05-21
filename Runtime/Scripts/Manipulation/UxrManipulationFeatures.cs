// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationFeatures.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enumerates the different manipulation features that can be used when the <see cref="UxrGrabManager" /> is being
    ///     updated.
    /// </summary>
    [Flags]
    public enum UxrManipulationFeatures
    {
        /// <summary>
        ///     Update the transform of <see cref="UxrGrabbableObject" /> objects based on user interactions using grabbers.
        /// </summary>
        ObjectManipulation = 1 << 0,

        /// <summary>
        ///     Applies constraints defined in the <see cref="UxrGrabbableObject" /> component.
        /// </summary>
        ObjectConstraints = 1 << 1,

        /// <summary>
        ///     Applies resistance defined by <see cref="UxrGrabbableObject.TranslationResistance" /> and
        ///     <see cref="UxrGrabbableObject.RotationResistance" />.
        /// </summary>
        ObjectResistance = 1 << 2,

        /// <summary>
        ///     Applies constraints defined by users through <see cref="UxrGrabbableObject.ConstraintsApplying" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsApplied" />/<see cref="UxrGrabbableObject.ConstraintsFinished" />.
        /// </summary>
        UserConstraints = 1 << 3,

        /// <summary>
        ///     Forces to keep the grips in place to avoid hands drifting from an object that has constraints applied.
        /// </summary>
        KeepGripsInPlace = 1 << 4,

        /// <summary>
        ///     Smooth transitions in grabbing hands and objects that are being manipulated.
        /// </summary>
        SmoothTransitions = 1 << 5,

        /// <summary>
        ///     Updates the affordances.
        /// </summary>
        Affordances = 1 << 6,

        /// <summary>
        ///     Uses all features.
        /// </summary>
        All = 0x7FFFFFFF
    }
}