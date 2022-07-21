// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.HandTransitionInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information of hand transitions from grabs to releases and from grabs using 2 hands to single hand, for the
        ///     hand that keeps the grab.
        /// </summary>
        private class HandTransitionInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Hand bone position when the grip was released.
            /// </summary>
            public Vector3 StartPosition { get; }

            /// <summary>
            ///     Hand bone rotation when the grip was released.
            /// </summary>
            public Quaternion StartRotation { get; }

            /// <summary>
            ///     Timer value to control the interpolation.
            /// </summary>
            public float Timer { get; set; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="grabber">Grabber</param>
            /// <param name="grabberPosition">Grabber position at the moment of starting the transition</param>
            /// <param name="grabberRotation">Grabber rotation at the moment of starting the transition</param>
            public HandTransitionInfo(UxrGrabber grabber, Vector3 grabberPosition, Quaternion grabberRotation)
            {
                Matrix4x4 grabberMtx = Matrix4x4.TRS(grabberPosition, grabberRotation, Vector3.one);

                Timer         = UxrGrabbableObject.HandLockSeconds;
                StartPosition = grabberMtx.MultiplyPoint(grabber.HandBoneRelativePos);
                StartRotation = grabberMtx.rotation * grabber.HandBoneRelativeRot;
            }

            #endregion
        }

        #endregion
    }
}