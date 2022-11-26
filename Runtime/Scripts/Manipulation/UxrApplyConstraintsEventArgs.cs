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
        ///     Gets the grabbable object.
        /// </summary>
        public UxrGrabber Grabber { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor is private.
        /// </summary>
        private UxrApplyConstraintsEventArgs()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="grabber">The grabber being processed</param>
        public UxrApplyConstraintsEventArgs(UxrGrabber grabber)
        {
            Grabber = grabber;
        }

        #endregion
    }
}