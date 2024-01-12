// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportSpawnCollider.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Component that, added to an object with colliders, allows to define volumes that force a fixed teleportation
    ///     destination when they are hit with teleporting pointers (arc, ray, etc.).
    /// </summary>
    public class UxrTeleportSpawnCollider : UxrComponent<UxrTeleportSpawnCollider>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private GameObject _enableWhenSelected;
        [SerializeField] private Transform  _spawnPosOneSide;
        [SerializeField] private Transform  _spawnPosOptionalOtherSide;
        [SerializeField] private Transform  _altTargetPosOneSide;
        [SerializeField] private Transform  _altTargetPosOtherSide;
        [SerializeField] private float      _heightDistanceFactor = 1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event raised when the user was teleported by using the spawn collider.
        /// </summary>
        public event EventHandler<UxrTeleportSpawnUsedEventArgs> Teleported;

        /// <summary>
        ///     Gets or sets the <see cref="GameObject" /> that will be enabled while the component is being pointed at. This can
        ///     be used to enable graphics that help identifying the interactivity and destination.
        /// </summary>
        public GameObject EnableWhenSelected
        {
            get => _enableWhenSelected;
            set => _enableWhenSelected = value;
        }

        /// <summary>
        ///     Gets or sets the spawn point the avatar will be teleported to when using the component.
        ///     In two-sided spawn setups -a ladder that will allow to climb up or down, for example- it represents one of the
        ///     sides. In the ladder example, it will be either the ground or the top.
        /// </summary>
        public Transform SpawnPosOneSide
        {
            get => _spawnPosOneSide;
            set => _spawnPosOneSide = value;
        }

        /// <summary>
        ///     Gets or sets the other spawn point the avatar can be teleported to when using the component.
        ///     In single point spawn setups this should be left null. In two-sided spawn setups -a ladder that will allow to climb
        ///     up or down, for example- it represents the other side where the avatar can be teleported to.
        ///     In the ladder example it will be either the ground or the top.
        /// </summary>
        public Transform SpawnPosOptionalOtherSide
        {
            get => _spawnPosOptionalOtherSide;
            set => _spawnPosOptionalOtherSide = value;
        }

        /// <summary>
        ///     Gets or sets the alternate target position for <see cref="SpawnPosOneSide" />.
        ///     When non-null it will override the preview teleport target position for <see cref="SpawnPosOneSide" />.
        ///     Overriding can be useful to draw the target position on top of a seat, even though the actual spawn position will
        ///     be at the bottom of the seat.
        /// </summary>
        public Transform AltTargetPosOneSide
        {
            get => _altTargetPosOneSide;
            set => _altTargetPosOneSide = value;
        }

        /// <summary>
        ///     Gets or sets the alternate target position for <see cref="SpawnPosOptionalOtherSide" />.
        ///     Same as <see cref="AltTargetPosOneSide" /> but for <see cref="SpawnPosOptionalOtherSide" />.
        /// </summary>
        public Transform AltTargetPosOtherSide
        {
            get => _altTargetPosOtherSide;
            set => _altTargetPosOtherSide = value;
        }

        /// <summary>
        ///     Gets or sets the additional factor that the height difference between <see cref="SpawnPosOneSide" /> and
        ///     <see cref="SpawnPosOptionalOtherSide" /> will play in computing which spawn point is closer.
        ///     In two-sided spawn setups, the collider will teleport the avatar to the farthest spawn point of the two. This
        ///     factor helps give more weight to the spawn point that is at a farther ground level.
        /// </summary>
        public float HeightDistanceFactor
        {
            get => _heightDistanceFactor;
            set => _heightDistanceFactor = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     If there are two spawn positions (one side and other side) it will return the farthest one to the avatar.
        ///     This is useful to simulate ladders and other spawn elements that will allow to go from one side to the other.
        ///     Otherwise it will just return the single spawn position.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        /// <param name="targetPosition">
        ///     Where the target position should be rendered. This is useful when you want
        ///     a user to teleport to a chair, where the spawn position would be the base of the chair, but the target
        ///     should be rendered on top of the seat instead.
        /// </param>
        /// <returns>Farthest spawn position to the player available</returns>
        public Transform GetSpawnPos(UxrAvatar avatar, out Vector3 targetPosition)
        {
            if (SpawnPosOneSide != null && SpawnPosOptionalOtherSide != null)
            {
                Vector3 avatarPos     = avatar.CameraFloorPosition;
                bool    isToOtherSide = Distance(avatarPos, SpawnPosOneSide.position) < Distance(avatarPos, SpawnPosOptionalOtherSide.position);

                if (isToOtherSide)
                {
                    targetPosition = AltTargetPosOtherSide != null ? AltTargetPosOtherSide.position : SpawnPosOptionalOtherSide.position;
                }
                else
                {
                    targetPosition = AltTargetPosOneSide != null ? AltTargetPosOneSide.position : SpawnPosOneSide.position;
                }

                return isToOtherSide ? SpawnPosOptionalOtherSide : SpawnPosOneSide;
            }
            if (SpawnPosOneSide != null)
            {
                targetPosition = AltTargetPosOneSide != null ? AltTargetPosOneSide.position : SpawnPosOneSide.position;
                return SpawnPosOneSide;
            }
            if (SpawnPosOptionalOtherSide != null)
            {
                targetPosition = AltTargetPosOtherSide != null ? AltTargetPosOtherSide.position : SpawnPosOptionalOtherSide.position;
                return SpawnPosOptionalOtherSide;
            }

            targetPosition = transform.position;
            return transform;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the _enableWhenSelected GameObject if there is one.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (_enableWhenSelected != null)
            {
                _enableWhenSelected.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Raises the <see cref="Teleported" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseTeleported(UxrAvatar avatar, UxrAvatarMoveEventArgs e)
        {
            Teleported?.Invoke(this, new UxrTeleportSpawnUsedEventArgs(avatar, e));
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes a distance value between the avatar and a spawn position by adding up the linear distance at floor level
        ///     and the vertical distance. The distance is used to check which spawn position is closer to the user when two spawn
        ///     positions are available.
        /// </summary>
        /// <param name="avatarPosition">Avatar position</param>
        /// <param name="spawnPosition">Spawn position</param>
        /// <returns>Distance (horizontal + vertical).</returns>
        private float Distance(Vector3 avatarPosition, Vector3 spawnPosition)
        {
            float verticalDistance = Mathf.Abs(avatarPosition.y - spawnPosition.y) * HeightDistanceFactor;

            Vector3 a = avatarPosition;
            Vector3 b = spawnPosition;
            a.y = 0.0f;
            b.y = 0.0f;

            return Vector3.Distance(a, b) + verticalDistance;
        }

        #endregion
    }
}