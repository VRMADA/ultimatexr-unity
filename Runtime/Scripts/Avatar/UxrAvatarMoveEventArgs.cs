// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarMoveEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
    ///                 cref="UxrManager.MoveAvatarTo(UltimateXR.Avatar.UxrAvatar,UnityEngine.Vector3,UnityEngine.Vector3,bool)">
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
    ///     avatar
    ///     will generate <see cref="UxrAvatarMoveEventArgs" /> events.
    /// </summary>
    public class UxrAvatarMoveEventArgs : UxrAvatarEventArgs
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
        public Vector3 OldForward { get; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> forward vector.
        /// </summary>
        public Vector3 NewForward { get; }

        /// <summary>
        ///     Gets the old <see cref="UxrAvatar" /> local to world matrix.
        /// </summary>
        public Matrix4x4 OldWorldMatrix { get; }

        /// <summary>
        ///     Gets the new <see cref="UxrAvatar" /> local to world matrix.
        /// </summary>
        public Matrix4x4 NewWorldMatrix { get; }

        /// <summary>
        ///     Gets whether the avatar has changed its position.
        /// </summary>
        public bool HasTranslation { get; }

        /// <summary>
        ///     Gets whether the avatar has changed its rotation.
        /// </summary>
        public bool HasRotation { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar that moved</param>
        /// <param name="oldPosition">Old <see cref="UxrAvatar" /> position</param>
        /// <param name="oldRotation">Old <see cref="UxrAvatar" /> rotation</param>
        /// <param name="newPosition">New <see cref="UxrAvatar" /> position</param>
        /// <param name="newRotation">New <see cref="UxrAvatar" /> rotation</param>
        public UxrAvatarMoveEventArgs(UxrAvatar avatar, Vector3 oldPosition, Quaternion oldRotation, Vector3 newPosition, Quaternion newRotation) : base(avatar)
        {
            OldPosition = oldPosition;
            OldRotation = oldRotation;
            NewPosition = newPosition;
            NewRotation = newRotation;

            OldForward     = OldRotation * Vector3.forward;
            NewForward     = NewRotation * Vector3.forward;
            OldWorldMatrix = Matrix4x4.TRS(oldPosition, oldRotation, Vector3.one);
            NewWorldMatrix = Matrix4x4.TRS(NewPosition, NewRotation, Vector3.one);

            _oldWorldMatrixInverse = OldWorldMatrix.inverse;
            _oldRotationInverse    = Quaternion.Inverse(oldRotation);

            HasTranslation = OldPosition != NewPosition;
            HasRotation    = OldRotation != NewRotation;
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
            Vector3    relativePos = _oldWorldMatrixInverse.MultiplyPoint(transform.position);
            Quaternion relativeRot = _oldRotationInverse * transform.rotation;

            transform.SetPositionAndRotation(NewWorldMatrix.MultiplyPoint(relativePos), NewRotation * relativeRot);
        }

        #endregion

        #region Private Types & Data

        private readonly Matrix4x4  _oldWorldMatrixInverse;
        private readonly Quaternion _oldRotationInverse;

        #endregion
    }
}