// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarArm.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores bone references of an Avatar's arm.
    /// </summary>
    [Serializable]
    public class UxrAvatarArm
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform     _clavicle;
        [SerializeField] private Transform     _upperArm;
        [SerializeField] private Transform     _forearm;
        [SerializeField] private UxrAvatarHand _hand;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the arm.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                if (Clavicle != null)
                {
                    yield return Clavicle;
                }

                if (UpperArm != null)
                {
                    yield return UpperArm;
                }

                if (Forearm != null)
                {
                    yield return Forearm;
                }

                foreach (Transform transform in _hand.Transforms)
                {
                    yield return transform;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the clavicle transform.
        /// </summary>
        public Transform Clavicle
        {
            get => _clavicle;
            set => _clavicle = value;
        }

        /// <summary>
        ///     Gets or sets the upper arm transform.
        /// </summary>
        public Transform UpperArm
        {
            get => _upperArm;
            set => _upperArm = value;
        }

        /// <summary>
        ///     Gets or sets the forearm transform.
        /// </summary>
        public Transform Forearm
        {
            get => _forearm;
            set => _forearm = value;
        }

        /// <summary>
        ///     Gets or sets the hand.
        /// </summary>
        public UxrAvatarHand Hand
        {
            get => _hand;
            set => _hand = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrAvatarArm()
        {
            _hand = new UxrAvatarHand();
        }

        #endregion
    }
}