// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarFingerBoneInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores information of the bone in an <see cref="UxrAvatarFingerInfo" />.
    /// </summary>
    [Serializable]
    public class UxrAvatarFingerBoneInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _length;
        [SerializeField] private float _radius;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the bone length.
        /// </summary>
        public float Length => _length;

        /// <summary>
        ///     Gets the radius of the finger along this bone.
        /// </summary>
        public float Radius => _radius;

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Computes the bone length and radius.
        /// </summary>
        /// <param name="handRenderer">Hand renderer</param>
        /// <param name="bone">Finger bone</param>
        /// <param name="nextBone">Next finger bone downwards the hierarchy or null for the last bone</param>
        /// <param name="universalFingerAxes">Finger universal coordinate system</param>
        internal void Compute(SkinnedMeshRenderer handRenderer, Transform bone, Transform nextBone, UxrUniversalLocalAxes universalFingerAxes)
        {
            // Compute bounds representing the influenced part in the mesh in local bone coordinates.
            Bounds localBounds = MeshExt.GetBoneInfluenceBounds(handRenderer, bone);

            _length = 0.0f;
            
            // Compute bone length:

            if (bone && nextBone)
            {
                // If we have a next bone, simply compute distance.
                _length = Vector3.Distance(bone.position, nextBone.position);
            }
            else if (bone)
            {
                // If we have no next bone (for example, the distal bone is the last in the hierarchy), we compute the length using the influenced mesh part's local bounds.
                _length = Vector3.Scale(universalFingerAxes.LocalForward, localBounds.size).magnitude;
            }

            // Compute radius using the influenced part of the mesh's bounds computed earlier.
            _radius = 0.5f * Mathf.Max(Vector3.Scale(universalFingerAxes.LocalRight, localBounds.size).magnitude, Vector3.Scale(universalFingerAxes.LocalUp, localBounds.size).magnitude);
        }

        #endregion
    }
}