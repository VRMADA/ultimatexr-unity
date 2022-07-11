// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarHand.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Stores bone references of an Avatar's hand.
    /// </summary>
    [Serializable]
    public class UxrAvatarHand
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform       _wrist;
        [SerializeField] private UxrAvatarFinger _thumb;
        [SerializeField] private UxrAvatarFinger _index;
        [SerializeField] private UxrAvatarFinger _middle;
        [SerializeField] private UxrAvatarFinger _ring;
        [SerializeField] private UxrAvatarFinger _little;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets a sequence of all the non-null transforms in the hand, including the wrist.
        /// </summary>
        public IEnumerable<Transform> Transforms
        {
            get
            {
                if (Wrist != null)
                {
                    yield return Wrist;
                }

                foreach (Transform transform in FingerTransforms)
                {
                    yield return transform;
                }
            }
        }

        /// <summary>
        ///     Gets a sequence of all the non-null finger transforms in the hand.
        /// </summary>
        public IEnumerable<Transform> FingerTransforms
        {
            get
            {
                foreach (Transform transform in Index.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in Middle.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in Ring.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in Little.Transforms)
                {
                    yield return transform;
                }

                foreach (Transform transform in Thumb.Transforms)
                {
                    yield return transform;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the wrist transform. The wrist is the root transform in the hand.
        /// </summary>
        public Transform Wrist
        {
            get => _wrist;
            set => _wrist = value;
        }

        /// <summary>
        ///     Gets or sets the thumb finger.
        /// </summary>
        public UxrAvatarFinger Thumb
        {
            get => _thumb;
            set => _thumb = value;
        }

        /// <summary>
        ///     Gets or sets the index finger.
        /// </summary>
        public UxrAvatarFinger Index
        {
            get => _index;
            set => _index = value;
        }

        /// <summary>
        ///     Gets or sets the middle finger.
        /// </summary>
        public UxrAvatarFinger Middle
        {
            get => _middle;
            set => _middle = value;
        }

        /// <summary>
        ///     Gets or sets the ring finger.
        /// </summary>
        public UxrAvatarFinger Ring
        {
            get => _ring;
            set => _ring = value;
        }

        /// <summary>
        ///     Gets or sets the little finger.
        /// </summary>
        public UxrAvatarFinger Little
        {
            get => _little;
            set => _little = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrAvatarHand()
        {
            _thumb  = new UxrAvatarFinger();
            _index  = new UxrAvatarFinger();
            _middle = new UxrAvatarFinger();
            _ring   = new UxrAvatarFinger();
            _little = new UxrAvatarFinger();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the hand has all finger references plus the wrist.
        /// </summary>
        /// <returns>Whether the hand has all finger bone data plus the wrist.</returns>
        public bool HasFullHandData()
        {
            return Wrist != null && Thumb.HasData() && Index.HasData() && Middle.HasData() && Ring.HasData() && Little.HasData();
        }

        /// <summary>
        ///     Checks if the hand has all finger references.
        /// </summary>
        /// <returns>Whether the hand has all finger bone data.</returns>
        public bool HasFingerData()
        {
            return Thumb.HasData() && Index.HasData() && Middle.HasData() && Ring.HasData() && Little.HasData();
        }

        /// <summary>
        ///     Gets the information of a given finger.
        /// </summary>
        /// <param name="fingerType">Finger to get</param>
        /// <returns>Finger information</returns>
        public UxrAvatarFinger GetFinger(UxrFingerType fingerType)
        {
            switch (fingerType)
            {
                case UxrFingerType.Thumb:  return Thumb;
                case UxrFingerType.Index:  return Index;
                case UxrFingerType.Middle: return Middle;
                case UxrFingerType.Ring:   return Ring;
                case UxrFingerType.Little: return Little;
                default:                   throw new ArgumentOutOfRangeException(nameof(fingerType), fingerType, null);
            }
        }

        /// <summary>
        ///     Tries to compute the palm center in world coordinates.
        /// </summary>
        /// <param name="center">Returns the palm center in world coordinates</param>
        /// <returns>Whether the center could be computed. False if some required bone references are missing</returns>
        public bool GetPalmCenter(out Vector3 center)
        {
            center = Vector3.zero;

            if (Wrist == null || Index.Proximal == null || Little.Proximal == null)
            {
                return false;
            }

            Vector3 a = Vector3.zero;
            Vector3 b = Wrist.InverseTransformPoint(Index.Proximal.position);
            Vector3 c = Wrist.InverseTransformPoint(Little.Proximal.position);

            center = _wrist.TransformPoint(Vector3Ext.Average(a, b, c));
            return true;
        }

        /// <summary>
        ///     Tries to compute the direction that goes out of the palm in world coordinates.
        /// </summary>
        /// <param name="handSide">Which hand it is</param>
        /// <param name="direction">Returns the palm vector in world coordinates</param>
        /// <returns>Whether the vector could be computed. False if some required bone references are missing</returns>
        public bool GetPalmOutDirection(UxrHandSide handSide, out Vector3 direction)
        {
            direction = Vector3.zero;

            if (Wrist == null || Index.Proximal == null || Little.Proximal == null)
            {
                return false;
            }

            direction = Wrist.TransformDirection(Vector3.Cross(Wrist.InverseTransformPoint(Index.Proximal.position), Wrist.InverseTransformPoint(Little.Proximal.position)).normalized);

            if (handSide == UxrHandSide.Right)
            {
                direction = -direction;
            }

            return true;
        }

        /// <summary>
        ///     Tries to compute the palm-to-finger direction in world coordinates.
        /// </summary>
        /// <param name="direction">Returns the palm-to-finger direction in world coordinates</param>
        /// <returns>Whether the vector could be computed. False if some required bone references are missing</returns>
        public bool GetPalmToFingerDirection(out Vector3 direction)
        {
            direction = Vector3.zero;

            if (Wrist == null || Index.Proximal == null || Little.Proximal == null)
            {
                return false;
            }

            direction = ((Index.Proximal.position + Little.Proximal.position) * 0.5f - _wrist.position).normalized;
            return true;
        }

        #endregion
    }
}