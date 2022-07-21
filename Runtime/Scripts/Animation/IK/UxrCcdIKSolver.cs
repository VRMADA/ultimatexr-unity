// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCcdIKSolver.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Component that we use to solve IK chains using CCD (Cyclic Coordinate Descent). A chain is defined
    ///     by a set of links, an effector and a goal.
    ///     The links are bones that will try to make the effector reach the same exact point, or the closest to, the goal.
    ///     Usually the effector is on the tip of the last bone.
    ///     Each link can have different rotation constraints to simulate different behaviours and systems.
    /// </summary>
    public partial class UxrCcdIKSolver : UxrIKSolver
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int              _maxIterations     = 10;
        [SerializeField] private float            _minDistanceToGoal = 0.001f;
        [SerializeField] private List<UxrCcdLink> _links             = new List<UxrCcdLink>();
        [SerializeField] private Transform        _endEffector;
        [SerializeField] private Transform        _goal;
        [SerializeField] private bool             _constrainGoalToEffector;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the list of links in the CCD.
        /// </summary>
        public IReadOnlyList<UxrCcdLink> Links => _links.AsReadOnly();

        /// <summary>
        ///     Gets the end effector, which is the point that is part of the chain that will try to match the goal position.
        /// </summary>
        public Transform EndEffector => _endEffector;

        /// <summary>
        ///     Gets the goal, which is the goal that the chain will try to match with the <see cref="EndEffector" />.
        /// </summary>
        public Transform Goal => _goal;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes the internal data for the IK chain. This will only need to be called once during Awake(), but inside
        ///     the Unity editor we can call it also for drawing some gizmos that need it.
        /// </summary>
        public void ComputeLinkData()
        {
            if (_links != null && _endEffector != null)
            {
                for (int i = 0; i < _links.Count; ++i)
                {
                    if (_links[i].Bone != null && !(i < _links.Count - 1 && _links[i + 1].Bone == null))
                    {
                        _links[i].MtxToLocalParent = Matrix4x4.identity;

                        if (_links[i].Bone.parent != null)
                        {
                            _links[i].MtxToLocalParent = _links[i].Bone.parent.worldToLocalMatrix;
                        }

                        _links[i].InitialLocalRotation            = _links[i].Bone.localRotation;
                        _links[i].LocalSpaceAxis1ZeroAngleVector  = _links[i].RotationAxis1.GetPerpendicularVector();
                        _links[i].LocalSpaceAxis2ZeroAngleVector  = _links[i].RotationAxis2.GetPerpendicularVector();
                        _links[i].ParentSpaceAxis1                = _links[i].MtxToLocalParent.MultiplyVector(_links[i].Bone.TransformDirection(_links[i].RotationAxis1));
                        _links[i].ParentSpaceAxis2                = _links[i].MtxToLocalParent.MultiplyVector(_links[i].Bone.TransformDirection(_links[i].RotationAxis2));
                        _links[i].ParentSpaceAxis1ZeroAngleVector = _links[i].MtxToLocalParent.MultiplyVector(_links[i].Bone.TransformDirection(_links[i].LocalSpaceAxis1ZeroAngleVector));
                        _links[i].ParentSpaceAxis2ZeroAngleVector = _links[i].MtxToLocalParent.MultiplyVector(_links[i].Bone.TransformDirection(_links[i].LocalSpaceAxis2ZeroAngleVector));
                        _links[i].LinkLength                      = i == _links.Count - 1 ? Vector3.Distance(_links[i].Bone.position, _endEffector.position) : Vector3.Distance(_links[i].Bone.position, _links[i + 1].Bone.position);
                    }
                }
            }
        }

        /// <summary>
        ///     Sets the weight of the given link.
        /// </summary>
        /// <param name="link">Link index</param>
        /// <param name="weight">Link weight [0.0f, 1.0f]</param>
        public void SetLinkWeight(int link, float weight)
        {
            if (link >= 0 && link < _links.Count)
            {
                _links[link].Weight = weight;
            }
        }

        /// <summary>
        ///     Sets the default values for the given link.
        /// </summary>
        /// <param name="link">Link index</param>
        public void SetLinkDefaultValues(int link)
        {
            if (link >= 0 && link < _links.Count)
            {
                _links[link] = new UxrCcdLink();
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the link data.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            ComputeLinkData();
        }

        /// <summary>
        ///     Checks if the goal needs to be parented so that the IK computation doesn't affect the goal itself.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            
            if (_goal.HasParent(_endEffector) || _links.Any(l => _goal.HasParent(l.Bone)))
            {
                _goal.SetParent(transform);
            }
        }

        #endregion

        #region Protected Overrides UxrIKSolver

        /// <summary>
        ///     IK solver implementation. Will try to make the end effector in the link chain to match the goal.
        /// </summary>
        protected override void InternalSolveIK()
        {
            Vector3    goalPosition = _goal.position;
            Vector3    goalForward  = _goal.forward;

            for (int i = 0; i < _maxIterations; ++i)
            {
                IterationResult result = ComputeSingleIterationCcd(_links, _endEffector, goalPosition, goalForward, _minDistanceToGoal);

                if (result != IterationResult.ReachingGoal)
                {
                    break;
                }
            }

            if (_constrainGoalToEffector && Vector3.Distance(goalPosition, _endEffector.position) > _minDistanceToGoal)
            {
                _goal.position = _endEffector.position;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Fixes an angle so that it is always in the -180, 180 degrees range.
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>Angle in the -180, 180 degrees range</returns>
        private static float FixAngle(float angle)
        {
            angle = angle % 360.0f;

            if (angle > 180.0f)
            {
                angle -= 360.0f;
            }
            else if (angle < -180.0f)
            {
                angle += 360.0f;
            }

            return angle;
        }

        /// <summary>
        ///     Computes a single iteration of the CCD algorithm on our link chain.
        /// </summary>
        /// <param name="links">List of links (bones) of the chain</param>
        /// <param name="endEffector">The point on the chain that will try to reach the goal</param>
        /// <param name="goalPosition">The goal that the end effector will try to reach</param>
        /// <param name="goalForward">The goal forward vector that the end effector will try to reach if alignment is enabled</param>
        /// <param name="minDistanceToGoal">Minimum distance to the goal that is considered success</param>
        /// <returns>Result of the iteration</returns>
        private static IterationResult ComputeSingleIterationCcd(List<UxrCcdLink> links, Transform endEffector, Vector3 goalPosition, Vector3 goalForward, float minDistanceToGoal)
        {
            if (Vector3.Distance(goalPosition, endEffector.position) <= minDistanceToGoal)
            {
                return IterationResult.GoalReached;
            }

            // Iterate from tip to base

            bool linksRotated = false;

            foreach (UxrCcdLink link in links)
            {
                if (Vector3.Distance(goalPosition, endEffector.position) <= minDistanceToGoal)
                {
                    return IterationResult.GoalReached;
                }

                // Compute the matrix that transforms from world space to the parent bone's local space

                link.MtxToLocalParent = Matrix4x4.identity;

                if (link.Bone.parent != null)
                {
                    link.MtxToLocalParent = link.Bone.parent.worldToLocalMatrix;
                }

                // Compute the vector that rotates around axis1 corresponding to 0 degrees. It will be computed in local space of the parent link.

                Vector3 parentSpaceAngle1Vector = link.MtxToLocalParent.MultiplyVector(link.Bone.TransformDirection(link.LocalSpaceAxis1ZeroAngleVector));

                if (link.Constraint == UxrCcdConstraintType.TwoAxes)
                {
                    // When dealing with 2 axis constraint mode we need to recompute the rotation axis in parent space
                    link.ParentSpaceAxis1 = link.MtxToLocalParent.MultiplyVector(link.Bone.TransformDirection(link.RotationAxis1));
                }

                // Using the computations above, calculate the angle1 value. This is the value of rotation in degrees corresponding to the first constraint axis

                link.Angle1 = Vector3.SignedAngle(Vector3.ProjectOnPlane(link.ParentSpaceAxis1ZeroAngleVector, link.ParentSpaceAxis1),
                                                  Vector3.ProjectOnPlane(parentSpaceAngle1Vector,              link.ParentSpaceAxis1),
                                                  link.ParentSpaceAxis1);

                // Now let's rotate around axis1 if needed. We will compute the current vector from this node to the effector and also the current vector from this node
                // to the target. Our goal is to make the first vector match the second vector but we may only rotate around axis1. So what we do is project the goal vector
                // onto the plane with axis1 as its normal and this will be the result of our "valid" rotation due to the constraint.

                Vector3 currentDirection = endEffector.position - link.Bone.position;
                Vector3 desiredDirection = goalPosition - link.Bone.position;

                if (link.AlignToGoal)
                {
                    currentDirection = endEffector.forward;
                    desiredDirection = goalForward;
                }

                Vector3 worldAxis1                 = link.Bone.TransformDirection(link.RotationAxis1);
                Vector3 closestVectorAxis1Rotation = Vector3.ProjectOnPlane(desiredDirection, worldAxis1);

                float newAxis1AngleIncrement = link.Weight * Vector3.SignedAngle(Vector3.ProjectOnPlane(currentDirection, worldAxis1), closestVectorAxis1Rotation, worldAxis1);
                float totalAngleAxis1        = FixAngle(link.Angle1 + newAxis1AngleIncrement);

                // Now that we have computed our increment, let's see if we need to clamp it between the limits

                if (link.Axis1HasLimits)
                {
                    if (totalAngleAxis1 > link.Axis1AngleMax)
                    {
                        newAxis1AngleIncrement -= totalAngleAxis1 - link.Axis1AngleMax;
                    }
                    else if (totalAngleAxis1 < link.Axis1AngleMin)
                    {
                        newAxis1AngleIncrement += link.Axis1AngleMin - totalAngleAxis1;
                    }

                    totalAngleAxis1 = FixAngle(link.Angle1 + newAxis1AngleIncrement);
                }

                // Do we need to rotate?

                if (Mathf.Approximately(newAxis1AngleIncrement, 0.0f) == false)
                {
                    link.Angle1             = totalAngleAxis1;
                    link.Bone.localRotation = link.InitialLocalRotation * Quaternion.AngleAxis(link.Angle1, link.RotationAxis1);

                    if (link.Constraint == UxrCcdConstraintType.TwoAxes)
                    {
                        link.Bone.localRotation = link.Bone.localRotation * Quaternion.AngleAxis(link.Angle2, link.RotationAxis2);
                    }

                    linksRotated = true;
                }

                if (link.Constraint == UxrCcdConstraintType.TwoAxes)
                {
                    // Axis 2. Axis 2 works exactly like axis 1 but we operate on another plane

                    Vector3 parentSpaceAngle2Vector = link.MtxToLocalParent.MultiplyVector(link.Bone.TransformDirection(link.LocalSpaceAxis2ZeroAngleVector));

                    link.ParentSpaceAxis2 = link.MtxToLocalParent.MultiplyVector(link.Bone.TransformDirection(link.RotationAxis2));
                    link.Angle2 = Vector3.SignedAngle(Vector3.ProjectOnPlane(link.ParentSpaceAxis2ZeroAngleVector, link.ParentSpaceAxis2),
                                                      Vector3.ProjectOnPlane(parentSpaceAngle2Vector,              link.ParentSpaceAxis2),
                                                      link.ParentSpaceAxis2);

                    currentDirection = endEffector.position - link.Bone.position;
                    desiredDirection = goalPosition - link.Bone.position;

                    if (link.AlignToGoal)
                    {
                        currentDirection = endEffector.forward;
                        desiredDirection = goalForward;
                    }

                    Vector3 worldAxis2                 = link.Bone.TransformDirection(link.RotationAxis2);
                    Vector3 closestVectorAxis2Rotation = Vector3.ProjectOnPlane(desiredDirection, worldAxis2);

                    float newAxis2AngleIncrement = link.Weight * Vector3.SignedAngle(Vector3.ProjectOnPlane(currentDirection, worldAxis2), closestVectorAxis2Rotation, worldAxis2);
                    float totalAngleAxis2        = FixAngle(link.Angle2 + newAxis2AngleIncrement);

                    if (link.Axis2HasLimits)
                    {
                        if (totalAngleAxis2 > link.Axis2AngleMax)
                        {
                            newAxis2AngleIncrement -= totalAngleAxis2 - link.Axis2AngleMax;
                        }
                        else if (totalAngleAxis2 < link.Axis2AngleMin)
                        {
                            newAxis2AngleIncrement += link.Axis2AngleMin - totalAngleAxis2;
                        }

                        totalAngleAxis2 = FixAngle(link.Angle2 + newAxis2AngleIncrement);
                    }

                    if (Mathf.Approximately(newAxis2AngleIncrement, 0.0f) == false)
                    {
                        // Rotation order is first angle2 then angle1 because previously we have rotated in this order already
                        link.Angle2             = totalAngleAxis2;
                        link.Bone.localRotation = link.InitialLocalRotation * Quaternion.AngleAxis(link.Angle1, link.RotationAxis1) * Quaternion.AngleAxis(link.Angle2, link.RotationAxis2);

                        linksRotated = true;
                    }
                }
            }

            return linksRotated ? Vector3.Distance(goalPosition, endEffector.position) <= minDistanceToGoal ? IterationResult.GoalReached : IterationResult.ReachingGoal : IterationResult.Error;
        }

        /// <summary>
        ///     Gets the transform that should be used to restore the goal position every time an IK link
        ///     is reoriented.
        ///     We use this in cases where we manipulate an object that the goal is part of, and the IK chain
        ///     is in a hierarchy above the object/goal. This is needed because when computing the different
        ///     IK steps, the goal and the object may be repositioned as a consequence, being below in the chain.
        ///     As a double measure, what we try to reposition is the topmost parent that is below the IK chain,
        ///     since the goal may be a dummy at the end of the chain and repositioning the goal alone would
        ///     not be enough.
        /// </summary>
        /// <param name="links">List of links (bones) of the chain</param>
        /// <param name="goal">The goal that the end effector will try to reach</param>
        /// <returns>Transform that should be stored</returns>
        private static Transform GetGoalSafeRestoreTransform(List<UxrCcdLink> links, Transform goal)
        {
            Transform current  = goal;
            Transform previous = goal;

            while (current != null)
            {
                for (int i = links.Count - 1; i >= 0; --i)
                {
                    if (current == links[i].Bone && current != previous)
                    {
                        // Found a bone. previous here is the child that we should move/rotate in order to
                        // preserve the original goal position/orientation.
                        return previous;
                    }
                }

                previous = current;
                current  = current.parent;
            }

            return goal;
        }

        #endregion
    }
}