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

        #endregion

        #region Public Overrides UxrGrabPointShape

        /// <inheritdoc />
        public override float GetDistanceFromGrabber(UxrGrabber grabber, Transform snapTransform, Transform objectDistanceTransform, Transform grabberDistanceTransform)
        {
            // TODO: Consider rotation difference
            return grabberDistanceTransform.position.DistanceToSegment(GetSegmentA(objectDistanceTransform.position), GetSegmentB(objectDistanceTransform.position));
        }

        /// <inheritdoc />
        public override void GetClosestSnap(UxrGrabber grabber, Transform snapTransform, Transform distanceTransform, Transform grabberDistanceTransform, out Vector3 position, out Quaternion rotation)
        {
            // Compute best fitting rotation

            Vector3 worldAxis        = Center.TransformDirection(_centerAxis);
            Vector3 localSnapAxis    = snapTransform.InverseTransformDirection(worldAxis);
            Vector3 worldGrabberAxis = grabber.transform.TransformDirection(localSnapAxis);
            bool    reverseGrip      = _bidirectional && Vector3.Angle(worldGrabberAxis, -worldAxis) < Vector3.Angle(worldGrabberAxis, worldAxis);

            // worldGrabberAxis contains the axis in world coordinates if it was being grabbed with the current grabber orientation
            // projection contains the rotation that the grabber would need to rotate to align to the axis using the closest angle

            Quaternion projection = Quaternion.FromToRotation(worldGrabberAxis, reverseGrip ? -worldAxis : worldAxis);
/*
            if (reverseGrip)
            {
                Vector3 right   = projection * Vector3.right;
                Vector3 up      = projection * Vector3.up;
                Vector3 forward = projection * Vector3.forward;
            }*/

            // Compute the rotation required to rotate the grabber to the best suited grip on the axis with the given properties 

            rotation = projection * grabber.transform.rotation;
            
            // Compute perpendicular vectors to the axis to get the angle from snap rotation to projected snap rotation.

            Vector3 worldPerpendicular = Center.TransformDirection(_centerAxis.Perpendicular);
            Vector3 localPerpendicular = snapTransform.InverseTransformDirection(worldPerpendicular);

            Quaternion grabberRotation = grabber.transform.rotation;
            grabber.transform.rotation = rotation;
            
            // Compute angle and clamp it.
            
            float angle        = Vector3.SignedAngle(worldPerpendicular, grabber.transform.TransformDirection(localPerpendicular), worldAxis);
            float clampedAngle = Mathf.Clamp(angle, _angleMin, _angleMax);
            rotation = Quaternion.AngleAxis(clampedAngle - angle, worldAxis) * rotation;
            
            // TODO: use _angleInterval
            grabber.transform.rotation = grabberRotation;

            // Compute grabber position by rotating the snap position around the axis

            Vector3 projectedSnap  = snapTransform.position.ProjectOnLine(Center.position, worldAxis);
            Vector3 fromAxisToSnap = snapTransform.position - projectedSnap;
            Vector3 grabberPos     = grabber.transform.position;

            if (reverseGrip)
            {
                fromAxisToSnap = Quaternion.AngleAxis(180.0f, worldPerpendicular) * fromAxisToSnap;
            }

            position = grabberPos.ProjectOnSegment(GetSegmentA(projectedSnap), GetSegmentB(projectedSnap)) + fromAxisToSnap.GetRotationAround(worldAxis, clampedAngle);
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
                Gizmos.DrawLine(GetSegmentA(transform.position), GetSegmentB(transform.position));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets one side of the grabbable segment in world space if it started in <paramref name="center"/>.
        /// </summary>
        /// <param name="center">Center in world space to consider</param>
        private Vector3 GetSegmentA(Vector3 center)
        {
            return center + Center.TransformDirection(_centerAxis) * _offsetMin;
        }

        /// <summary>
        ///     Gets the other side of the grabbable segment in world space if it started in <paramref name="center"/>.
        /// </summary>
        /// <param name="center">Center in world space to consider</param>
        private Vector3 GetSegmentB(Vector3 center)
        {
            return center + Center.TransformDirection(_centerAxis) * _offsetMax;
        }

        #endregion
    }
}

#pragma warning restore 414