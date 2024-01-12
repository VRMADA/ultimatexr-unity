// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRestoreOnRelease.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Component that will smoothly restore the original position and orientation of a <see cref="UxrGrabbableObject" />
    ///     when released.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public class UxrRestoreOnRelease : UxrGrabbableObjectComponent<UxrRestoreOnRelease>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrEasing _transitionType    = UxrEasing.Linear;
        [SerializeField] private float     _transitionSeconds = 0.1f;

        #endregion

        #region Unity

        /// <summary>
        ///     Updates the transition if it's active.
        /// </summary>
        private void Update()
        {
            if (_isTransitioning)
            {
                _transitionTimer -= Time.deltaTime;
                
                float t = 1.0f;

                if (_transitionTimer <= 0.0f)
                {
                    _transitionTimer = 0.0f;
                    _isTransitioning = false;
                }
                else
                {
                    t = UxrInterpolator.Interpolate(0.0f, 1.0f, 1.0f - _transitionTimer / _transitionSeconds, _transitionType);
                }
                
                GrabbableObject.transform.localPosition = Vector3.LerpUnclamped(_initialLocalPosition, GrabbableObject.InitialLocalPosition, t);

                if (_singleRotationAxis != -1 && t >= 0.0f && t <= 1.0f)
                {
                    // Do not rotate manually here to let UxrGrabbableObject keep track of single axis rotation.
                    // We allow using localRotation outside of the [0, 1] range to support overshooting since otherwise GrabbableObject.SingleRotationAxisDegrees
                    // will clamp the rotation.
                    GrabbableObject.SingleRotationAxisDegrees = Mathf.LerpUnclamped(_initialSingleAxisDegrees, 0.0f, t);
                }
                else
                {
                    GrabbableObject.transform.localRotation = Quaternion.SlerpUnclamped(_initialLocalRotation, GrabbableObject.InitialLocalRotation, t);                    
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called by the base class whenever the object is grabbed.
        /// </summary>
        /// <param name="e">Contains all grab event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            if (e.IsGrabbedStateChanged)
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        ///     Called by the base class whenever the object is released.
        /// </summary>
        /// <param name="e">Contains all grab event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            base.OnObjectReleased(e);

            if (e.IsGrabbedStateChanged)
            {
                _isTransitioning      = true;
                _transitionTimer      = _transitionSeconds;
                _initialLocalPosition = e.GrabbableObject.transform.localPosition;
                _initialLocalRotation = e.GrabbableObject.transform.localRotation;
                _singleRotationAxis   = e.GrabbableObject.SingleRotationAxisIndex;

                if (_singleRotationAxis != -1)
                {
                    _initialSingleAxisDegrees = e.GrabbableObject.SingleRotationAxisDegrees;
                }
                
                // Avoid transitions getting in the way or being executed after the restore ended.
                e.GrabbableObject.FinishSmoothTransitions();
            }
        }

        #endregion

        #region Private Types & Data

        private bool       _isTransitioning;
        private float      _transitionTimer = -1.0f;
        private Vector3    _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private int        _singleRotationAxis;
        private float      _initialSingleAxisDegrees;

        #endregion
    }
}