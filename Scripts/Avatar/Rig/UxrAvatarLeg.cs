// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarLeg.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores bone references of an Avatar's leg.
    /// </summary>
    [Serializable]
    public class UxrAvatarLeg
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _upperLeg;
        [SerializeField] private Transform _lowerLeg;
        [SerializeField] private Transform _foot;
        [SerializeField] private Transform _toes;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the leg.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                if (UpperLeg != null)
                {
                    yield return UpperLeg;
                }

                if (LowerLeg != null)
                {
                    yield return LowerLeg;
                }

                if (Foot != null)
                {
                    yield return Foot;
                }

                if (Toes != null)
                {
                    yield return Toes;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the upper leg transform.
        /// </summary>
        public Transform UpperLeg
        {
            get => _upperLeg;
            set => _upperLeg = value;
        }

        /// <summary>
        ///     Gets or sets the lower leg transform.
        /// </summary>
        public Transform LowerLeg
        {
            get => _lowerLeg;
            set => _lowerLeg = value;
        }

        /// <summary>
        ///     Gets or sets the foot transform.
        /// </summary>
        public Transform Foot
        {
            get => _foot;
            set => _foot = value;
        }

        /// <summary>
        ///     Gets or sets the toes transform.
        /// </summary>
        public Transform Toes
        {
            get => _toes;
            set => _toes = value;
        }

        #endregion
    }
}