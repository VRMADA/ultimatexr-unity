// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     Stores base-independent node orientations for all fingers of a hand.
    /// </summary>
    [Serializable]
    public class UxrHandDescriptor
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrFingerDescriptor _index;
        [SerializeField] private UxrFingerDescriptor _middle;
        [SerializeField] private UxrFingerDescriptor _ring;
        [SerializeField] private UxrFingerDescriptor _little;
        [SerializeField] private UxrFingerDescriptor _thumb;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the index finger information.
        /// </summary>
        public UxrFingerDescriptor Index => _index;

        /// <summary>
        ///     Gets the middle finger information.
        /// </summary>
        public UxrFingerDescriptor Middle => _middle;

        /// <summary>
        ///     Gets the ring finger information.
        /// </summary>
        public UxrFingerDescriptor Ring => _ring;

        /// <summary>
        ///     Gets the little finger information.
        /// </summary>
        public UxrFingerDescriptor Little => _little;

        /// <summary>
        ///     Gets the thumb finger information.
        /// </summary>
        public UxrFingerDescriptor Thumb => _thumb;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrHandDescriptor()
        {
            _index  = new UxrFingerDescriptor();
            _middle = new UxrFingerDescriptor();
            _ring   = new UxrFingerDescriptor();
            _little = new UxrFingerDescriptor();
            _thumb  = new UxrFingerDescriptor();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar whose hand to compute the descriptor for</param>
        /// <param name="handSide">Which hand to process</param>
        public UxrHandDescriptor(UxrAvatar avatar, UxrHandSide handSide)
        {
            _index  = new UxrFingerDescriptor();
            _middle = new UxrFingerDescriptor();
            _ring   = new UxrFingerDescriptor();
            _little = new UxrFingerDescriptor();
            _thumb  = new UxrFingerDescriptor();

            Compute(avatar, handSide);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="arm">Arm whose hand to compute the descriptor for</param>
        /// <param name="handLocalAxes">Hand axes system</param>
        /// <param name="fingerLocalAxes">Finger axes system</param>
        public UxrHandDescriptor(UxrAvatarArm arm, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes fingerLocalAxes)
        {
            _index  = new UxrFingerDescriptor();
            _middle = new UxrFingerDescriptor();
            _ring   = new UxrFingerDescriptor();
            _little = new UxrFingerDescriptor();
            _thumb  = new UxrFingerDescriptor();

            Compute(arm, handLocalAxes, fingerLocalAxes);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the given finger.
        /// </summary>
        /// <param name="fingerType">Which finger to get</param>
        /// <returns>Finger information</returns>
        public UxrFingerDescriptor GetFinger(UxrFingerType fingerType)
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
        ///     Computes the hand data.
        /// </summary>
        /// <param name="avatar">Avatar to compute the hand data of</param>
        /// <param name="handSide">Which hand to compute the hand data of</param>
        /// <param name="computeRelativeMatrixOnly">Whether to compute the relative transform to the hand only</param>
        public void Compute(UxrAvatar avatar, UxrHandSide handSide, bool computeRelativeMatrixOnly = false)
        {
            Compute(handSide == UxrHandSide.Left ? avatar.AvatarRig.LeftArm : avatar.AvatarRig.RightArm, avatar.AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes, avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes, computeRelativeMatrixOnly);
        }

        /// <summary>
        ///     Computes the hand data.
        /// </summary>
        /// <param name="arm">Arm where the hand is</param>
        /// <param name="handLocalAxes">Hand axes system</param>
        /// <param name="fingerLocalAxes">Finger axes system</param>
        /// <param name="computeRelativeMatrixOnly">Whether to compute the relative transform to the hand only</param>
        public void Compute(UxrAvatarArm arm, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes fingerLocalAxes, bool computeRelativeMatrixOnly = false)
        {
            _index.Compute(arm.Hand.Wrist, arm.Hand.Index, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            _middle.Compute(arm.Hand.Wrist, arm.Hand.Middle, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            _ring.Compute(arm.Hand.Wrist, arm.Hand.Ring, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            _little.Compute(arm.Hand.Wrist, arm.Hand.Little, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            _thumb.Compute(arm.Hand.Wrist, arm.Hand.Thumb, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
        }

        /// <summary>
        ///     Copies the data from another descriptor.
        /// </summary>
        /// <param name="src">Source data</param>
        public void CopyFrom(UxrHandDescriptor src)
        {
            _index  = src._index;
            _middle = src._middle;
            _ring   = src._ring;
            _little = src._little;
            _thumb  = src._thumb;
        }

        /// <summary>
        ///     Interpolates the data towards another descriptor.
        /// </summary>
        /// <param name="to">Descriptor to interpolate the data to</param>
        /// <param name="t">Interpolation factor [0.0, 1.0]</param>
        public void InterpolateTo(UxrHandDescriptor to, float t)
        {
            _index.InterpolateTo(to._index, t);
            _middle.InterpolateTo(to._middle, t);
            _ring.InterpolateTo(to._ring, t);
            _little.InterpolateTo(to._little, t);
            _thumb.InterpolateTo(to._thumb, t);
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Outputs transform data in the editor window.
        /// </summary>
        public void DrawEditorDebugLabels()
        {
            _index.DrawEditorDebugLabels("index: ");
            _middle.DrawEditorDebugLabels("middle: ");
            _ring.DrawEditorDebugLabels("ring: ");
            _little.DrawEditorDebugLabels("little: ");
            _thumb.DrawEditorDebugLabels("thumb: ");
        }

#endif

        /// <summary>
        ///     Returns a hand descriptor with mirrored transforms, so that the data can be used for the opposite hand.
        /// </summary>
        /// <returns>Mirrored hand descriptor</returns>
        public UxrHandDescriptor Mirrored()
        {
            UxrHandDescriptor mirroredHandDescriptor = new UxrHandDescriptor();
            mirroredHandDescriptor.CopyFrom(this);

            mirroredHandDescriptor._index.Mirror();
            mirroredHandDescriptor._middle.Mirror();
            mirroredHandDescriptor._ring.Mirror();
            mirroredHandDescriptor._little.Mirror();
            mirroredHandDescriptor._thumb.Mirror();

            return mirroredHandDescriptor;
        }

        /// <summary>
        ///     Checks whether a hand descriptor contains the same transform data.
        /// </summary>
        /// <param name="other">Hand descriptor to compare it to</param>
        /// <returns>Whether the hand descriptor contains the same transform data</returns>
        public bool Equals(UxrHandDescriptor other)
        {
            if (other != null)
            {
                return _index.Equals(other._index) && _middle.Equals(other._middle) && _ring.Equals(other._ring) && _little.Equals(other._little) && _thumb.Equals(other._thumb);
            }

            return false;
        }

        #endregion
    }
}