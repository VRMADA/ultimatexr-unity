// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrApplyConstraintsEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Event arguments for <see cref="UxrGrabbableObject" /> <see cref="UxrGrabbableObject.ConstraintsApplying" /> and
    ///     <see cref="UxrGrabbableObject.ConstraintsApplied" />.
    /// </summary>
    public class UxrApplyConstraintsEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the grabbable object being constrained.
        /// </summary>
        public UxrGrabbableObject GrabbableObject { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="grabbableObject">The object being constrained</param>
        public UxrApplyConstraintsEventArgs(UxrGrabbableObject grabbableObject)
        {
            GrabbableObject = grabbableObject;
        }

        /// <summary>
        ///     Default constructor is private.
        /// </summary>
        private UxrApplyConstraintsEventArgs()
        {
        }

        #endregion
    }
}