// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core.Math;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     Stores base-independent node orientations for a finger.
    /// </summary>
    [Serializable]
    public struct UxrFingerDescriptor
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                    _hasMetacarpalInfo;
        [SerializeField] private UxrFingerNodeDescriptor _metacarpal;
        [SerializeField] private UxrFingerNodeDescriptor _proximal;
        [SerializeField] private UxrFingerNodeDescriptor _proximalNoMetacarpal;
        [SerializeField] private UxrFingerNodeDescriptor _intermediate;
        [SerializeField] private UxrFingerNodeDescriptor _distal;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether metacarpal bone information is present. Metacarpal information is optional.
        /// </summary>
        public bool HasMetacarpalInfo => _hasMetacarpalInfo;

        /// <summary>
        ///     Gets the metacarpal bone transform information.
        /// </summary>
        public UxrFingerNodeDescriptor Metacarpal => _metacarpal;

        /// <summary>
        ///     Gets the proximal bone transform information.
        /// </summary>
        public UxrFingerNodeDescriptor Proximal => _proximal;

        /// <summary>
        ///     Gets the proximal bone transform information with respect to the wrist even if there is metacarpal information. It
        ///     is used in case a pose including metacarpal information wants to be mapped to a hand that has no metacarpal bones.
        /// </summary>
        public UxrFingerNodeDescriptor ProximalNoMetacarpal => _proximalNoMetacarpal;

        /// <summary>
        ///     Gets the intermediate bone transform information.
        /// </summary>
        public UxrFingerNodeDescriptor Intermediate => _intermediate;

        /// <summary>
        ///     Gets the distal bone transform information.
        /// </summary>
        public UxrFingerNodeDescriptor Distal => _distal;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Computes well-known axes systems for all finger bones, to handle transforms independently of the coordinate system
        ///     being used by a hand rig.
        /// </summary>
        /// <param name="wrist">Wrist transform</param>
        /// <param name="finger">Finger rig information</param>
        /// <param name="handLocalAxes">Well-known axes system for the hand</param>
        /// <param name="fingerLocalAxes">Well-known axes system for the finger elements</param>
        /// <param name="computeRelativeMatrixOnly">Whether to compute only the relative transform to the hand</param>
        public void Compute(Transform wrist, UxrAvatarFinger finger, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes fingerLocalAxes, bool computeRelativeMatrixOnly)
        {
            if (finger.Metacarpal)
            {
                _hasMetacarpalInfo = true;
                _metacarpal.Compute(wrist, wrist, finger.Metacarpal, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
                _proximal.Compute(wrist, finger.Metacarpal, finger.Proximal, fingerLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
                _proximalNoMetacarpal.Compute(wrist, wrist, finger.Proximal, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            }
            else
            {
                _hasMetacarpalInfo = false;
                _proximal.Compute(wrist, wrist, finger.Proximal, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
                _proximalNoMetacarpal.Compute(wrist, wrist, finger.Proximal, handLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            }

            _intermediate.Compute(wrist, finger.Proximal, finger.Intermediate, fingerLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
            _distal.Compute(wrist, finger.Intermediate, finger.Distal, fingerLocalAxes, fingerLocalAxes, computeRelativeMatrixOnly);
        }

        /// <summary>
        ///     Mirrors the bone information, so that it can be used for the opposite hand.
        /// </summary>
        public void Mirror()
        {
            if (_hasMetacarpalInfo)
            {
                _metacarpal.Mirror();
            }

            _proximal.Mirror();
            _proximalNoMetacarpal.Mirror();
            _intermediate.Mirror();
            _distal.Mirror();
        }

        /// <summary>
        ///     Interpolates the data towards another descriptor.
        /// </summary>
        /// <param name="to">Descriptor to interpolate the data to</param>
        /// <param name="t">Interpolation factor [0.0, 1.0]</param>
        public void InterpolateTo(UxrFingerDescriptor to, float t)
        {
            if (_hasMetacarpalInfo)
            {
                _metacarpal.InterpolateTo(to._metacarpal, t);
            }

            _proximal.InterpolateTo(to._proximal, t);
            _proximalNoMetacarpal.InterpolateTo(to._proximalNoMetacarpal, t);
            _intermediate.InterpolateTo(to._intermediate, t);
            _distal.InterpolateTo(to._distal, t);
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Outputs transform information to the editor window.
        /// </summary>
        /// <param name="prefix">String to prefix the information with</param>
        public void DrawEditorDebugLabels(string prefix)
        {
            EditorGUILayout.LabelField(prefix + _proximal.Right);
            EditorGUILayout.LabelField(prefix + _proximal.Up);
            EditorGUILayout.LabelField(prefix + _proximal.Forward);
        }

#endif

        /// <summary>
        ///     Compares the transform information with another finger.
        /// </summary>
        /// <param name="other">Finger information to compare it to</param>
        /// <returns>Whether both fingers describe the same transform information</returns>
        public bool Equals(UxrFingerDescriptor other)
        {
            if (_hasMetacarpalInfo != other._hasMetacarpalInfo)
            {
                return false;
            }

            if (_hasMetacarpalInfo)
            {
                return _metacarpal.Equals(other._metacarpal) && _proximal.Equals(other._proximal) && _intermediate.Equals(other._intermediate) && _distal.Equals(other._distal);
            }
            return _proximal.Equals(other._proximal) && _intermediate.Equals(other._intermediate) && _distal.Equals(other._distal);
        }

        #endregion
    }
}