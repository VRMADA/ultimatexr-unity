// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabPointShapeAxisAngle.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Grab shape used to grab cylindrical objects. The cylinder is described by an axis and a length. It is possible to
    ///     specify if the object can be grabbed in both directions or a direction only.
    /// </summary>
    public class UxrGrabPointShapeAxisAngle : UxrGrabPointShape
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _center;
        [SerializeField] private UxrAxis   _centerAxis = UxrAxis.Z;
        [SerializeField] private bool      _bidirectional;
        [SerializeField] private float     _angleMin       = -180.0f;
        [SerializeField] private float     _angleMax       = 180.0f;
        [SerializeField] private float     _angleInterval  = 0.01f;
        [SerializeField] private float     _offsetMin      = -0.1f;
        [SerializeField] private float     _offsetMax      = 0.1f;
        [SerializeField] private float     _offsetInterval = 0.001f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the axis center.
        /// </summary>
        public Transform Center => _center != null ? _center : transform;

        /// <summary>
        ///     Gets one side of the grabbable segment.
        /// </summary>
        public Vector3 SegmentA => Center.position + Center.TransformVector(_centerAxis) * _offsetMin;

        /// <summary>
        ///     Gets the other side of the grabbable segment.
        /// </summary>
        public Vector3 SegmentB => Center.position + Center.TransformVector(_centerAxis) * _offsetMax;

        #endregion

        #region Public Overrides UxrGrabPointShape

        /// <inheritdoc />
        public override float GetDistanceFromGrabber(UxrGrabber grabber, Transform snapTransform, Transform distanceTransform)
        {
            return grabber.transform.position.DistanceToSegment(GetSnapSegmentA(distanceTransform), GetSnapSegmentB(distanceTransform));
        }

        /// <inheritdoc />
        public override void GetClosestSnap(UxrGrabber grabber, Transform snapTransform, Transform distanceTransform, out Vector3 position, out Quaternion rotation)
        {
            // Compute best fitting rotation
            Vector3 worldCenterAxis  = _center.TransformVector(_centerAxis);
            Vector3 localSnapAxis    = snapTransform.InverseTransformVector(worldCenterAxis);
            Vector3 worldGrabberAxis = grabber.transform.TransformVector(localSnapAxis);

            Quaternion projection  = Quaternion.FromToRotation(worldGrabberAxis, worldCenterAxis);
            bool       reverseGrip = false;

            if (_bidirectional && Vector3.Angle(worldGrabberAxis, -worldCenterAxis) < Vector3.Angle(worldGrabberAxis, worldCenterAxis))
            {
                projection  = Quaternion.FromToRotation(worldGrabberAxis, -worldCenterAxis);
                reverseGrip = true;
            }

            rotation = projection * grabber.transform.rotation;

            // Given this rotation, compute position
            Vector3 worldPerpendicular = _center.TransformVector(_centerAxis.Perpendicular);
            Vector3 localPerpendicular = snapTransform.InverseTransformVector(worldPerpendicular);

            Quaternion grabberRotation = grabber.transform.rotation;
            grabber.transform.rotation = rotation;
            float angleDelta = Vector3.SignedAngle(worldPerpendicular, grabber.transform.TransformVector(localPerpendicular), worldCenterAxis);
            grabber.transform.rotation = grabberRotation;

            Vector3 fromAxisToSnap = snapTransform.position - snapTransform.position.GetClosestPointFromSegment(SegmentA, SegmentB);
            Vector3 grabberPos     = grabber.transform.position;

            if (reverseGrip)
            {
                fromAxisToSnap = Quaternion.AngleAxis(180.0f, worldPerpendicular) * fromAxisToSnap;
            }

            position = grabberPos.GetClosestPointFromSegment(SegmentA, SegmentB) + Quaternion.AngleAxis(angleDelta, worldCenterAxis) * fromAxisToSnap;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called when the object is selected, to draw the gizmos in the scene window.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            UxrGrabbableObject grabbableObject = GetComponent<UxrGrabbableObject>();

            if (_grabPointIndex >= 0 && _grabPointIndex < grabbableObject.GrabPointCount)
            {
                Gizmos.DrawLine(SegmentA, SegmentB);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the snap point for one side of the grabbable segment.
        /// </summary>
        /// <param name="pointTransform"><see cref="Transform" /> describing one side of the grabbable segment</param>
        /// <returns>Snap point</returns>
        private Vector3 GetSnapSegmentA(Transform pointTransform)
        {
            return pointTransform.position + Center.TransformVector(_centerAxis) * _offsetMin;
        }

        /// <summary>
        ///     Gets the snap point for the other side of the grabbable segment.
        /// </summary>
        /// <param name="pointTransform"><see cref="Transform" /> describing the other side of the grabbable segment</param>
        /// <returns>Snap point</returns>
        private Vector3 GetSnapSegmentB(Transform pointTransform)
        {
            return pointTransform.position + Center.TransformVector(_centerAxis) * _offsetMax;
        }

        #endregion
    }
}

#pragma warning restore 414