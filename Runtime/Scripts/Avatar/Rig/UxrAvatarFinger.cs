// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarFinger.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores bone references of an Avatar's finger.
    /// </summary>
    [Serializable]
    public class UxrAvatarFinger
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _metacarpal;
        [SerializeField] private Transform _proximal;
        [SerializeField] private Transform _intermediate;
        [SerializeField] private Transform _distal;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the finger.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                if (Metacarpal != null)
                {
                    yield return Metacarpal;
                }

                if (Proximal != null)
                {
                    yield return Proximal;
                }

                if (Intermediate != null)
                {
                    yield return Intermediate;
                }

                if (Distal != null)
                {
                    yield return Distal;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the metacarpal bone transform. Metacarpal bones are optional.
        /// </summary>
        public Transform Metacarpal
        {
            get => _metacarpal;
            set => _metacarpal = value;
        }

        /// <summary>
        ///     Gets or sets the proximal bone transform.
        /// </summary>
        public Transform Proximal
        {
            get => _proximal;
            set => _proximal = value;
        }

        /// <summary>
        ///     Gets or sets the intermediate bone transform.
        /// </summary>
        public Transform Intermediate
        {
            get => _intermediate;
            set => _intermediate = value;
        }

        /// <summary>
        ///     Gets or sets the distal bone transform.
        /// </summary>
        public Transform Distal
        {
            get => _distal;
            set => _distal = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the finger has the required bone references. The only optional bone is the metacarpal bone, which may be
        ///     null.
        /// </summary>
        /// <returns>Whether the finger has all the required bone data.</returns>
        public bool HasData()
        {
            return Proximal != null && Intermediate != null && Distal != null;
        }

        /// <summary>
        ///     Sets up the finger bones using a list starting from the metacarpal (if there are 4 elements) or the proximal (if
        ///     there are 3).
        /// </summary>
        /// <param name="bones">Finger bone list. It may be either 4 or 3 bones, depending if the metacarpal bone is included.</param>
        public void SetupFingerBones(List<Transform> bones)
        {
            if (Metacarpal == null && bones.Count == 4)
            {
                Metacarpal = bones[0];
            }

            if (Proximal == null)
            {
                Proximal = bones[bones.Count == 4 ? 1 : 0];
            }

            if (Intermediate == null)
            {
                Intermediate = bones[bones.Count == 4 ? 2 : 1];
            }

            if (Distal == null)
            {
                Distal = bones[bones.Count == 4 ? 3 : 2];
            }
        }

        #endregion
    }
}