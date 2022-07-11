// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarHead.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores bone references of an Avatar's head.
    /// </summary>
    [Serializable]
    public class UxrAvatarHead
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _leftEye;
        [SerializeField] private Transform _rightEye;
        [SerializeField] private Transform _jaw;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _neck;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the head.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                if (LeftEye != null)
                {
                    yield return LeftEye;
                }

                if (RightEye != null)
                {
                    yield return RightEye;
                }

                if (Jaw != null)
                {
                    yield return Jaw;
                }

                if (Head != null)
                {
                    yield return Head;
                }

                if (Neck != null)
                {
                    yield return Neck;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the left eye transform.
        /// </summary>
        public Transform LeftEye
        {
            get => _leftEye;
            set => _leftEye = value;
        }

        /// <summary>
        ///     Gets or sets the upper leg transform.
        /// </summary>
        public Transform RightEye
        {
            get => _rightEye;
            set => _rightEye = value;
        }

        /// <summary>
        ///     Gets or sets the jaw transform.
        /// </summary>
        public Transform Jaw
        {
            get => _jaw;
            set => _jaw = value;
        }

        /// <summary>
        ///     Gets or sets the head transform.
        /// </summary>
        public Transform Head
        {
            get => _head;
            set => _head = value;
        }

        /// <summary>
        ///     Gets or sets the neck transform.
        /// </summary>
        public Transform Neck
        {
            get => _neck;
            set => _neck = value;
        }

        #endregion
    }
}