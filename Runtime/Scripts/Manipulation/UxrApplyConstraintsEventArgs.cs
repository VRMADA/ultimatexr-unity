// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrApplyConstraintsEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

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
        ///     Gets the grabber that is grabbing the object being constrained.
        /// </summary>
        public UxrGrabber Grabber { get; }

        /// <summary>
        ///     Gets the grabber position given by the actual tracking data (controller, hand tracking...), without any
        ///     manipulation constraints applied.
        /// </summary>
        public Vector3 UnprocessedGrabberPos { get; }

        /// <summary>
        ///     Gets the grabber rotation given by the actual tracking data (controller, hand tracking...), without any
        ///     manipulation constraints applied.
        /// </summary>
        public Quaternion UnprocessedGrabberRot { get; }

        /// <summary>
        ///     Gets the grabber position with the current manipulation constraints applied. This position might be different from
        ///     <see cref="UnprocessedGrabberPos" /> if the object being manipulated has a position constraint that prevents the
        ///     grabber from moving.
        /// </summary>
        public Vector3 ProcessedGrabberPos { get; }

        /// <summary>
        ///     Gets the grabber rotation with the current manipulation constraints applied. This rotation might be different from
        ///     <see cref="UnprocessedGrabberRot" /> if the object being manipulated has a rotation constraint that prevents the
        ///     grabber from rotating.
        /// </summary>
        public Quaternion ProcessedGrabberRot { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="grabber">The grabber that is grabbing the object being constrained</param>
        /// <param name="grabPoint">The grab point that is being grabbed</param>
        public UxrApplyConstraintsEventArgs(UxrGrabber grabber, int grabPoint)
        {
            Grabber = grabber;

            UnprocessedGrabberPos = grabber.UnprocessedGrabberPosition;
            UnprocessedGrabberRot = grabber.UnprocessedGrabberRotation;
            ProcessedGrabberPos   = grabber.GrabbedObject.GetGrabPointSnapModeAffectsPosition(grabPoint) ? grabber.GrabbedObject.GetGrabbedPointGrabAlignPosition(grabber, grabPoint) : grabber.transform.position;
            ProcessedGrabberRot   = grabber.GrabbedObject.GetGrabPointSnapModeAffectsRotation(grabPoint) ? grabber.GrabbedObject.GetGrabbedPointGrabAlignRotation(grabber, grabPoint) : grabber.transform.rotation;
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