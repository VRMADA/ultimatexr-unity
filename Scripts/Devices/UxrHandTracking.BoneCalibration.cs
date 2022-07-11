// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandTracking.BoneCalibration.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Attributes;
using UnityEngine;

namespace UltimateXR.Devices
{
    public abstract partial class UxrHandTracking
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores the relative rotation of a transform to a reference pose.
        /// </summary>
        [Serializable]
        private class BoneCalibration
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField]            private Transform _transform;
            [SerializeField] [ReadOnly] private float     _x;
            [SerializeField] [ReadOnly] private float     _y;
            [SerializeField] [ReadOnly] private float     _z;
            [SerializeField] [ReadOnly] private float     _w;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the transform the calibration data is for.
            /// </summary>
            public Transform Transform => _transform;

            /// <summary>
            ///     Gets or sets the relative rotation to the reference calibration pose.
            /// </summary>
            public Quaternion Rotation
            {
                get => new Quaternion(_x, _y, _z, _w);
                set
                {
                    _x = value.x;
                    _y = value.y;
                    _z = value.z;
                    _w = value.w;
                }
            }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="transform">Transform</param>
            /// <param name="rotation">Rotation relative to the calibration pose</param>
            public BoneCalibration(Transform transform, Quaternion rotation)
            {
                _transform = transform;
                Rotation   = rotation;
            }

            #endregion
        }

        #endregion
    }
}