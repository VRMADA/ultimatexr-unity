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
        public event EventHandler<UxrAvatarMoveEventArgs> Teleported;

        /// <summary>
        ///     Gets or sets the <see cref="GameObject" /> that will be enabled while the component is being pointed at. This can
        ///     be used to enable graphics that help identifying the interactivity and destination.
        /// </summary>
        public GameObject EnableWhenSelected
        {
            get => _enableWhenSelected;
            set => _enableWhenSelected = value;
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
            if (_spawnPosOneSide != null && _spawnPosOptionalOtherSide != null)
            {
                Vector3 avatarPos     = avatar.CameraFloorPosition;
                bool    isToOtherSide = Distance(avatarPos, _spawnPosOneSide.position) < Distance(avatarPos, _spawnPosOptionalOtherSide.position);

                if (isToOtherSide)
                {
                    targetPosition = _altTargetPosOtherSide != null ? _altTargetPosOtherSide.position : _spawnPosOptionalOtherSide.position;
                }
                else
                {
                    targetPosition = _altTargetPosOneSide != null ? _altTargetPosOneSide.position : _spawnPosOneSide.position;
                }

                return isToOtherSide ? _spawnPosOptionalOtherSide : _spawnPosOneSide;
            }
            if (_spawnPosOneSide != null)
            {
                targetPosition = _altTargetPosOneSide != null ? _altTargetPosOneSide.position : _spawnPosOneSide.position;
                return _spawnPosOneSide;
            }
            targetPosition = _altTargetPosOtherSide != null ? _altTargetPosOtherSide.position : _spawnPosOptionalOtherSide.position;
            return _spawnPosOptionalOtherSide;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Raises the <see cref="Teleported" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseTeleported(UxrAvatarMoveEventArgs e)
        {
            Teleported?.Invoke(this, e);
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
            float verticalDistance = Mathf.Abs(avatarPosition.y - spawnPosition.y) * _heightDistanceFactor;

            Vector3 a = avatarPosition;
            Vector3 b = spawnPosition;
            a.y = 0.0f;
            b.y = 0.0f;

            return Vector3.Distance(a, b) + verticalDistance;
        }

        #endregion
    }
}