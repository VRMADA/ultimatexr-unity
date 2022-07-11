// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerPointingVolume.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Component that describes a box volume where any <see cref="UxrAvatar" /> hand that gets inside automatically adopts
    ///     a finger pointing pose. This is useful to place in front of UI screens or where precise finger pressing interaction
    ///     is required.
    /// </summary>
    /// <remarks>
    ///     The finger pointing pose should only be adopted if it doesn't interfere with any other interaction, such
    ///     as the grab pose while grabbing an object inside the volume.
    /// </remarks>
    [RequireComponent(typeof(BoxCollider))]
    public class UxrFingerPointingVolume : UxrComponent<UxrFingerPointingVolume>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _leftHand  = true;
        [SerializeField] private bool _rightHand = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="BoxCollider" /> component describing the enclosed space where to adopt the finger pointing
        ///     pose.
        /// </summary>
        public BoxCollider Box => GetCachedComponent<BoxCollider>();

        /// <summary>
        ///     Gets or sets whether the left hand should adopt the pose when inside.
        /// </summary>
        public bool UseLeftHand
        {
            get => _leftHand;
            set => _leftHand = value;
        }

        /// <summary>
        ///     Gets or sets whether the right hand should adopt the pose when inside.
        /// </summary>
        public bool UseRightHand
        {
            get => _rightHand;
            set => _rightHand = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if a point is inside the <see cref="BoxCollider" /> attached to the <see cref="GameObject" /> this component
        ///     is attached to.
        /// </summary>
        /// <param name="point">Point in world coordinates</param>
        /// <param name="margin">Margin to add to the box sides</param>
        /// <returns>True if it is inside, false if not</returns>
        public bool IsPointInside(Vector3 point, float margin = 0.0f)
        {
            return point.IsInsideBox(Box, Vector3.one * margin);
        }

        /// <summary>
        ///     Checks if the volume is compatible with the given hand. This allows some volumes to work for the left or
        ///     right hand only.
        /// </summary>
        /// <param name="handSide">Hand to check</param>
        /// <returns>Boolean telling whether the given hand is compatible or not</returns>
        public bool IsCompatible(UxrHandSide handSide)
        {
            return (handSide == UxrHandSide.Left && UseLeftHand) || (handSide == UxrHandSide.Right && UseRightHand);
        }

        #endregion
    }
}