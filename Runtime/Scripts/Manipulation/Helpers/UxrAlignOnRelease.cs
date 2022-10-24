// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAlignOnRelease.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Aligns an object smoothly whenever it is released to keep it leveled. Should be used on non physics-driven
    ///     grabbable objects, which remain floating in the air when being released.
    /// </summary>
    public class UxrAlignOnRelease : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]                     private bool                     _onlyLevel    = true;
        [SerializeField] [Range(0.0f, 1.0f)] private float                    _smoothFactor = 0.2f;
        [SerializeField]                     private List<UxrGrabbableObject> _grabbableObjects;

        #endregion

        #region Unity

        /// <summary>
        ///     Caches the transform component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _selfTransform = transform;
        }

        /// <summary>
        ///     Updates the transform while the object is not being grabbed.
        /// </summary>
        private void Update()
        {
            if (!IsBeingGrabbed)
            {
                // Smoothly rotate towards horizontal orientation when not being grabbed

                if (_onlyLevel == false)
                {
                    _selfTransform.rotation = UxrInterpolator.SmoothDampRotation(_selfTransform.rotation, Quaternion.FromToRotation(_selfTransform.up, Vector3.up) * _selfTransform.rotation, _smoothFactor);
                }
                else
                {
                    Vector3    projectedRight = Vector3.ProjectOnPlane(transform.right, Vector3.up);
                    Quaternion targetRotation = Quaternion.FromToRotation(_selfTransform.right, projectedRight) * _selfTransform.rotation;

                    if ((targetRotation * Vector3.up).y < 0.0f)
                    {
                        targetRotation = targetRotation * Quaternion.AngleAxis(180.0f, Vector3.forward);
                    }

                    _selfTransform.rotation = UxrInterpolator.SmoothDampRotation(_selfTransform.rotation, targetRotation, _smoothFactor);
                }
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets whether the object is being grabbed using any of the registered grabbable objects.
        /// </summary>
        private bool IsBeingGrabbed
        {
            get
            {
                foreach (UxrGrabbableObject grabbableObject in _grabbableObjects)
                {
                    if (UxrGrabManager.Instance.IsBeingGrabbed(grabbableObject))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private Transform _selfTransform;

        #endregion
    }
}