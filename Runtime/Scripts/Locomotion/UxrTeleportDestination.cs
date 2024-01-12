// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportDestination.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Describes a teleportation destination.
    /// </summary>
    public class UxrTeleportDestination
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="Transform" /> where the avatar will be positioned on.
        /// </summary>
        public Transform Destination { get; }

        /// <summary>
        ///     Gets the raycast hit information that was used to select the destination.
        /// </summary>
        public RaycastHit HitInfo { get; }

        /// <summary>
        ///     Gets the new avatar world position.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        ///     Gets the new avatar world rotation.
        /// </summary>
        public Quaternion Rotation { get; }

        /// <summary>
        ///     Gets the new avatar position in local <see cref="Destination" /> space.
        /// </summary>
        public Vector3 LocalDestinationPosition { get; }

        /// <summary>
        ///     Gets the new avatar rotation in local <see cref="Destination" /> space.
        /// </summary>
        public Quaternion LocalDestinationRotation { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="hitInfo">The raycast hit information that was used to select the destination</param>
        /// <param name="position">New avatar world position</param>
        /// <param name="rotation">New avatar world rotation</param>
        public UxrTeleportDestination(RaycastHit hitInfo, Vector3 position, Quaternion rotation)
        {
            Destination              = hitInfo.collider != null ? hitInfo.collider.transform : null;
            HitInfo                  = hitInfo;
            Position                 = position;
            Rotation                 = rotation;
            LocalDestinationPosition = Destination != null ? Destination.InverseTransformPoint(position) : Vector3.zero;
            LocalDestinationRotation = Destination != null ? Quaternion.Inverse(Destination.rotation) * rotation : Quaternion.identity;
        }

        #endregion
    }
}