// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRuntimeGripInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Stores spatial information of a grab that was performed at runtime.
    /// </summary>
    internal class UxrRuntimeGripInfo
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabbableObject" /> rotation relative to the <see cref="UxrGrabber" /> at the moment
        ///     it was grabbed.
        /// </summary>
        public Quaternion RelativeGrabRotation { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabbableObject" /> position in local <see cref="UxrGrabber" /> space at the moment
        ///     it was grabbed.
        /// </summary>
        public Vector3 RelativeGrabPosition { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabber" /> rotation relative to the <see cref="UxrGrabbableObject" /> at the moment
        ///     it was grabbed.
        /// </summary>
        public Quaternion RelativeGrabberRotation { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabber" /> position in local <see cref="UxrGrabbableObject" /> space at the moment
        ///     it was grabbed.
        /// </summary>
        public Vector3 RelativeGrabberPosition { get; set; }

        /// <summary>
        ///     Gets or sets the snap rotation relative to the <see cref="UxrGrabbableObject" /> at the moment it was grabbed.
        /// </summary>
        public Quaternion RelativeGrabAlignRotation { get; set; }

        /// <summary>
        ///     Gets or sets the snap position in local <see cref="UxrGrabbableObject" /> space at the moment it was grabbed.
        /// </summary>
        public Vector3 RelativeGrabAlignPosition { get; set; }

        /// <summary>
        ///     Gets or sets the proximity rotation relative to the <see cref="UxrGrabbableObject" /> at the moment it was grabbed.
        /// </summary>
        public Vector3 RelativeProximityPosition { get; set; }

        // *************************************************************************************************************************
        // For smooth transitions from object to hand or object to target or hand to object where we want to avoid instant snapping.
        // *************************************************************************************************************************

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabbableObject" /> local position at the moment it was grabbed.
        /// </summary>
        public Vector3 LocalPositionOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabbableObject" /> local rotation at the moment it was grabbed.
        /// </summary>
        public Quaternion LocalRotationOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the world-space snap position at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
        /// </summary>
        public Vector3 AlignPositionOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the world-space snap rotation at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
        /// </summary>
        public Quaternion AlignRotationOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the source in local <see cref="UxrGrabber" /> coordinates where the source of leverage will be
        ///     computed for <see cref="UxrRotationProvider.HandPositionAroundPivot" /> manipulation. This will improve rotation
        ///     behaviour when the hands are rotated because otherwise the source of leverage is the grabber itself and rotating
        ///     the hand will keep the grabber more or less stationary.
        /// </summary>
        public Vector3 GrabberLocalLeverageSource { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabber" /> position in local avatar coordinates at the moment the
        ///     <see cref="UxrGrabbableObject" /> was grabbed.
        /// </summary>
        public Vector3 GrabberLocalAvatarPositionOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="UxrGrabber" /> rotation in local avatar coordinates at the moment the
        ///     <see cref="UxrGrabbableObject" /> was grabbed.
        /// </summary>
        public Quaternion GrabberLocalAvatarRotationOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the leverage source <see cref="GrabberLocalLeverageSource" /> in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
        /// </summary>
        public Vector3 GrabberLocalAvatarLeverageSourceOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the hand bone position in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
        ///     was grabbed.
        /// </summary>
        public Vector3 HandBoneLocalAvatarPositionOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the hand bone rotation in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
        ///     was grabbed.
        /// </summary>
        public Quaternion HandBoneLocalAvatarRotationOnGrab { get; set; }

        /// <summary>
        ///     Gets or sets the decreasing timer that is initialized at <see cref="UxrGrabbableObject.ObjectAlignmentSeconds" />
        ///     at the moment the <see cref="UxrGrabbableObject" /> was grabbed. It is used for smooth object-to-hand transitions.
        /// </summary>
        public float GrabTimer { get; set; }

        /// <summary>
        ///     Gets or sets the decreasing timer that is initialized at <see cref="UxrGrabbableObject.HandLockSeconds" /> at the
        ///     moment the <see cref="UxrGrabbableObject" /> was grabbed. It is used for smooth hand-to-object transitions.
        /// </summary>
        public float HandLockTimer { get; set; }

        /// <summary>
        ///     Gets whether the grip is currently in a transition where the hand is locking to the
        ///     <see cref="UxrGrabbableObject" />.
        /// </summary>
        public bool LockHandInTransition { get; set; }

        #endregion
    }
}