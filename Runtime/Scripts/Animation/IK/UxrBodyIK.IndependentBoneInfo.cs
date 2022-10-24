// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrBodyIK.IndependentBoneInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    public sealed partial class UxrBodyIK
    {
        #region Private Types & Data

        /// <summary>
        ///     Independent bones are bones that are driven externally and not using IK, such as the hands (wrist bones), which are
        ///     driven by the tracked input controllers. They need to be kept track of when parent bones are modified by Inverse
        ///     Kinematics to make sure that they are kept in the same place afterwards. Otherwise due to parenting the position
        ///     they should have would change.
        /// </summary>
        private class IndependentBoneInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the bone transform.
            /// </summary>
            public Transform Transform { get; }

            /// <summary>
            ///     Gets or sets the correct current position.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            ///     Gets or sets the correct current position.
            /// </summary>
            public Quaternion Rotation { get; set; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="transform">Bone transform</param>
            public IndependentBoneInfo(Transform transform)
            {
                Transform = transform;
                Position  = transform.position;
                Rotation  = transform.rotation;
            }

            #endregion
        }

        #endregion
    }
}