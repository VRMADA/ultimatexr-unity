// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLocomotion.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Base class for locomotion components. Locomotion components enable different ways for an <see cref="UxrAvatar" />
    ///     to move around the scenario.
    /// </summary>
    public abstract class UxrLocomotion : UxrAvatarComponent<UxrLocomotion>, IUxrLocomotionUpdater
    {
        #region Public Types & Data

        /// <summary>
        ///     <para>
        ///         Gets whether the locomotion updates the avatar each frame. An example of smooth locomotion is
        ///         <see cref="UxrSmoothLocomotion" /> where the user moves the avatar in an identical way to a FPS video-game.
        ///         An example of non-smooth locomotion is <see cref="UxrTeleportLocomotion" /> where the avatar is moved only on
        ///         specific occasions.
        ///     </para>
        ///     <para>
        ///         The smooth locomotion concept should not be confused with the ability to move the head around each frame.
        ///         Smooth locomotion refers to the avatar position, which is determined by the avatar's root GameObject.
        ///         It should also not be confused with the ability to perform teleportation in a smooth way. Even if some
        ///         teleportation locomotion methods can teleport using smooth transitions, it should not be considered as smooth
        ///         locomotion.
        ///     </para>
        ///     <para>
        ///         The smooth locomotion property can be used to determine whether certain operations, such as LOD switching,
        ///         should be processed each frame or only when the avatar position changed.
        ///     </para>
        /// </summary>
        public abstract bool IsSmoothLocomotion { get; }

        #endregion

        #region Explicit IUxrLocomotionUpdater

        /// <inheritdoc />
        void IUxrLocomotionUpdater.UpdateLocomotion()
        {
            UpdateLocomotion();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Logs if there is a missing <see cref="Avatar" /> component upwards in the hierarchy.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Avatar == null)
            {
                UxrManager.LogMissingAvatarInHierarchyError(this);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Updates the locomotion and the avatar's position/orientation the component belongs to.
        /// </summary>
        protected abstract void UpdateLocomotion();

        /// <summary>
        ///     Checks whether a raycast has anything that is blocking. It filters out invalid raycasts such as against anything
        ///     part of the avatar or a grabbed object.
        /// </summary>
        /// <param name="avatar">The avatar to compute the raycast for</param>
        /// <param name="origin">Ray origin</param>
        /// <param name="direction">Ray direction</param>
        /// <param name="maxDistance">Raycast maximum distance</param>
        /// <param name="layerMaskRaycast">Raycast layer mask</param>
        /// <param name="queryTriggerInteraction">Behaviour against trigger colliders</param>
        /// <param name="outputHit">Result blocking raycast</param>
        /// <returns>Whether there is a blocking raycast returned in <paramref name="outputHit" /></returns>
        protected bool HasBlockingRaycastHit(UxrAvatar avatar, Vector3 origin, Vector3 direction, float maxDistance, int layerMaskRaycast, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit outputHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, maxDistance, layerMaskRaycast, queryTriggerInteraction);
            return HasBlockingRaycastHit(avatar, hits, out outputHit);
        }

        /// <summary>
        ///     Checks whether a capsule cast has anything that is blocking. It filters out invalid positives such as against
        ///     anything part of the avatar or a grabbed object.
        /// </summary>
        /// <param name="avatar">The avatar to compute the capsule cast for</param>
        /// <param name="point1">The center of the sphere at the start of the capsule</param>
        /// <param name="point2">The center of the sphere at the end of the capsule</param>
        /// <param name="radius">The radius of the capsule</param>
        /// <param name="direction">The direction into which to sweep the capsule</param>
        /// <param name="maxDistance">The max length of the sweep</param>
        /// <param name="layerMask">A that is used to selectively ignore colliders when casting a capsule</param>
        /// <param name="queryTriggerInteraction">Specifies whether this query should hit Triggers</param>
        /// <param name="outputHit">Result blocking raycast</param>
        /// <returns>Whether there is a blocking raycast returned in <paramref name="outputHit" /></returns>
        protected bool HasBlockingCapsuleCastHit(UxrAvatar avatar, Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit outputHit)
        {
            RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
            return HasBlockingRaycastHit(avatar, hits, out outputHit);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the given raycast hits have any that are blocking.
        ///     This method filters out invalid raycasts such as against anything part the avatar or a grabbed object.
        /// </summary>
        /// <param name="avatar">The avatar the ray-casting was computed for</param>
        /// <param name="inputHits">Set of raycast hits to check</param>
        /// <param name="outputHit">Result blocking raycast</param>
        /// <returns>Whether there is a blocking raycast returned in <paramref name="outputHit" /></returns>
        private bool HasBlockingRaycastHit(UxrAvatar avatar, RaycastHit[] inputHits, out RaycastHit outputHit)
        {
            bool hasBlockingHit = false;
            outputHit = default;

            if (inputHits.Count() > 1)
            {
                Array.Sort(inputHits, (a, b) => a.distance.CompareTo(b.distance));
            }

            foreach (RaycastHit singleHit in inputHits)
            {
                if (singleHit.collider.GetComponentInParent<UxrAvatar>() == avatar)
                {
                    // Filter out colliding against part of the avatar
                    continue;
                }

                UxrGrabbableObject grabbableObject = singleHit.collider.GetComponentInParent<UxrGrabbableObject>();

                if (grabbableObject != null && UxrGrabManager.Instance.IsBeingGrabbedBy(grabbableObject, avatar))
                {
                    // Filter out colliding against a grabbed object
                    continue;
                }

                outputHit      = singleHit;
                hasBlockingHit = true;
                break;
            }

            return hasBlockingHit;
        }

        #endregion
    }
}