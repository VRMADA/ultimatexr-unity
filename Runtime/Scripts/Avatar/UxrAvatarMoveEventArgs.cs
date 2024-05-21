// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarMoveEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Contains information about an <see cref="UxrAvatar" /> that has moved/rotated. Avatars are moved/rotated
    ///     through <see cref="UxrManager" /> functionality such as:
    ///     <list type="bullet">
    ///         <item>
    ///             <see
    ///                 cref="UxrManager.MoveAvatarTo(UxrAvatar,UnityEngine.Vector3,UnityEngine.Vector3,bool)">
    ///                 UxrManager.Instance.MoveAvatarTo
    ///             </see>
    ///         </item>
    ///         <item>
    ///             <see cref="UxrManager.RotateAvatar">UxrManager.Instance.RotateAvatar</see>
    ///         </item>
    ///         <item>
    ///             <see
    ///                 cref="UxrManager.TeleportLocalAvatar">
    ///                 UxrManager.Instance.TeleportLocalAvatar
    ///             </see>
    ///         </item>
    ///     </list>
    ///     These methods will move/rotate the root transform of the avatar. If a user moves or rotates in the real-world, the
    ///     camera transform will be updated but the root avatar transform will remain fixed. Only moving or teleporting the
    ///     avatar will generate <see cref="UxrAvatarMoveEventArgs" /> events.
    /// </summary>
    public class UxrAvatarMoveEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the old <see cref="UxrAvatar" /> position.
        /// </summary>
        public Vector3 OldPosition { get; }

        /// <summary>
        ///     Gets the old <see cref="UxrAvatar" /> rotation.
        /// </summary>
        public Quaternion OldRotation { get; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> position.
        /// </summary>
        public Vector3 NewPosition { get; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> rotation.
        /// </summary>
        public Quaternion NewRotation { get; }

        /// <summary>
        ///     Gets the old <see cref="UxrAvatar" /> forward vector.
        /// </summary>
        public Vector3 OldForward { get; private set; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> forward vector.
        /// </summary>
        public Vector3 NewForward { get; private set; }

        /// <summary>
        ///     Gets the old <see cref="UxrAvatar" /> local to world matrix.
        /// </summary>
        public Matrix4x4 OldWorldMatrix { get; private set; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> local to world matrix.
        /// </summary>
        public Matrix4x4 NewWorldMatrix { get; private set; }

        /// <summary>
        ///     Gets whether the avatar has changed its position.
        /// </summary>
        public bool HasTranslation { get; private set; }

        /// <summary>
        ///     Gets whether the avatar has changed its rotation.
        /// </summary>
        public bool HasRotation { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="oldPosition">Old <see cref="UxrAvatar" /> position</param>
        /// <param name="oldRotation">Old <see cref="UxrAvatar" /> rotation</param>
        /// <param name="newPosition">New <see cref="UxrAvatar" /> position</param>
        /// <param name="newRotation">New <see cref="UxrAvatar" /> rotation</param>
        public UxrAvatarMoveEventArgs(Vector3 oldPosition, Quaternion oldRotation, Vector3 newPosition, Quaternion newRotation)
        {
            OldPosition = oldPosition;
            OldRotation = oldRotation;
            NewPosition = newPosition;
            NewRotation = newRotation;

            ComputeInternalData();
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            if (HasTranslation && HasRotation)
            {
                return $"Avatar moved (OldPosition={OldPosition}, OldRotation={OldRotation}, NewPosition={NewPosition}, NewRotation={NewRotation})";
            }

            if (HasTranslation)
            {
                return $"Avatar moved (OldPosition={OldPosition}, NewPosition={NewPosition})";
            }

            return $"Avatar moved (OldRotation={OldPosition}, NewRotation={NewPosition})";
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Reorients and repositions a transform so that it keeps the relative position/orientation to the avatar after the
        ///     position changed event.
        /// </summary>
        /// <param name="transform">Transform to reorient/reposition</param>
        public void ReorientRelativeToAvatar(Transform transform)
        {
            GetKeepRelativeOrientationToAvatar(transform, out Vector3 position, out Quaternion rotation);
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        ///     Gets the new position and rotation an object would need to have to keep the same relative position/rotation to
        ///     the avatar after moving.
        /// </summary>
        /// <param name="transform">The transform to get the new position/rotation of</param>
        /// <param name="position">The new position</param>
        /// <param name="rotation">The new rotation</param>
        public void GetKeepRelativeOrientationToAvatar(Transform transform, out Vector3 position, out Quaternion rotation)
        {
            Vector3    relativePos = _oldWorldMatrixInverse.MultiplyPoint(transform.position);
            Quaternion relativeRot = _oldRotationInverse * transform.rotation;

            position = NewWorldMatrix.MultiplyPoint(relativePos);
            rotation = NewRotation * relativeRot;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes the helper properties and internal variables.
        /// </summary>
        private void ComputeInternalData()
        {
            OldForward     = OldRotation * Vector3.forward;
            NewForward     = NewRotation * Vector3.forward;
            OldWorldMatrix = Matrix4x4.TRS(OldPosition, OldRotation, Vector3.one);
            NewWorldMatrix = Matrix4x4.TRS(NewPosition, NewRotation, Vector3.one);

            _oldWorldMatrixInverse = OldWorldMatrix.inverse;
            _oldRotationInverse    = Quaternion.Inverse(OldRotation);

            HasTranslation = OldPosition != NewPosition;
            HasRotation    = OldRotation != NewRotation;
        }

        #endregion

        #region Private Types & Data

        private Matrix4x4  _oldWorldMatrixInverse;
        private Quaternion _oldRotationInverse;

        #endregion
    }
}