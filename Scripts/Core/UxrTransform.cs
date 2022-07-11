// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTransform.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Core
{
    /// <summary>
    ///     Stores transform information.
    /// </summary>
    public class UxrTransform
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the position.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        ///     Gets the local position.
        /// </summary>
        public Vector3 LocalPosition { get; }

        /// <summary>
        ///     Gets the rotation.
        /// </summary>
        public Quaternion Rotation { get; }

        /// <summary>
        ///     Gets the local rotation.
        /// </summary>
        public Quaternion LocalRotation { get; }

        /// <summary>
        ///     Gets the lossy scale.
        /// </summary>
        public Vector3 LossyScale { get; }

        /// <summary>
        ///     Gets the local scale.
        /// </summary>
        public Vector3 LocalScale { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor. Stores all the current transform information.
        /// </summary>
        /// <param name="transform">Transform to store the information of</param>
        public UxrTransform(Transform transform)
        {
            Position      = transform.position;
            LocalPosition = transform.localPosition;
            Rotation      = transform.rotation;
            LocalRotation = transform.localRotation;
            LossyScale    = transform.lossyScale;
            LocalScale    = transform.localScale;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Applies the stored values to a given transform.
        /// </summary>
        /// <param name="transform">The transform to apply the values to</param>
        public void ApplyTo(Transform transform)
        {
            transform.localPosition = LocalPosition;
            transform.localRotation = LocalRotation;
            transform.localScale    = LocalScale;
        }

        #endregion
    }
}