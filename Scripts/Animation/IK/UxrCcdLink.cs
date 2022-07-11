// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCcdLink.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Defines a link -bone- in an IK chain solved using CCD.
    /// </summary>
    /// <seealso cref="UxrCcdIKSolver" />
    [Serializable]
    public class UxrCcdLink
    {
        #region Inspector Properties/Serialized Fields

        // Setup in the editor
        [SerializeField] private Transform            _bone;
        [SerializeField] private float                _weight;
        [SerializeField] private UxrCcdConstraintType _constraint;
        [SerializeField] private Vector3              _rotationAxis1;
        [SerializeField] private Vector3              _rotationAxis2;
        [SerializeField] private bool                 _axis1HasLimits;
        [SerializeField] private float                _axis1AngleMin;
        [SerializeField] private float                _axis1AngleMax;
        [SerializeField] private bool                 _axis2HasLimits;
        [SerializeField] private float                _axis2AngleMin;
        [SerializeField] private float                _axis2AngleMax;
        [SerializeField] private bool                 _alignToGoal;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the link transform.
        /// </summary>
        public Transform Bone => _bone;

        /// <summary>
        ///     Gets the link constraint type.
        /// </summary>
        public UxrCcdConstraintType Constraint => _constraint;

        /// <summary>
        ///     Gets the first rotation axis.
        /// </summary>
        public Vector3 RotationAxis1 => _rotationAxis1;

        /// <summary>
        ///     Gets the second rotation axis when there are two constraints.
        /// </summary>
        public Vector3 RotationAxis2 => _rotationAxis2;

        /// <summary>
        ///     Gets whether the first axis has rotational limits.
        /// </summary>
        public bool Axis1HasLimits => _axis1HasLimits;

        /// <summary>
        ///     Gets the lower angle limits of the first axis.
        /// </summary>
        public float Axis1AngleMin => _axis1AngleMin;

        /// <summary>
        ///     Gets the higher angle limits of the first axis.
        /// </summary>
        public float Axis1AngleMax => _axis1AngleMax;

        /// <summary>
        ///     Gets whether the second axis has rotational limits when there are two constraints.
        /// </summary>
        public bool Axis2HasLimits => _axis2HasLimits;

        /// <summary>
        ///     Gets the lower angle limits of the second axis when there are two constraints.
        /// </summary>
        public float Axis2AngleMin => _axis2AngleMin;

        /// <summary>
        ///     Gets the higher angle limits of the second axis when there are two constraints.
        /// </summary>
        public float Axis2AngleMax => _axis2AngleMax;

        /// <summary>
        ///     Gets whether the effector should not only try to position itself on the goal but also use the same orientation.
        /// </summary>
        public bool AlignToGoal => _alignToGoal;

        /// <summary>
        ///     The weight among all the CCD links in the chain.
        /// </summary>
        public float Weight
        {
            get => _weight;
            set => _weight = value;
        }

        /// <summary>
        ///     Gets the local rotation at the beginning.
        /// </summary>
        public Quaternion InitialLocalRotation { get; internal set; }

        /// <summary>
        ///     Gets a reference perpendicular to axis1 that is considered as the reference of having 0 degrees around axis1.
        /// </summary>
        public Vector3 LocalSpaceAxis1ZeroAngleVector { get; internal set; }

        /// <summary>
        ///     Gets a reference perpendicular to axis2 that is considered as the reference of having 0 degrees around axis2.
        /// </summary>
        public Vector3 LocalSpaceAxis2ZeroAngleVector { get; internal set; }

        /// <summary>
        ///     Gets the length of the link.
        /// </summary>
        public float LinkLength { get; internal set; }

        /// <summary>
        ///     Gets <see cref="RotationAxis1" /> in local space of the parent object.
        /// </summary>
        public Vector3 ParentSpaceAxis1 { get; internal set; }

        /// <summary>
        ///     Gets <see cref="RotationAxis2" /> in local space of the parent object.
        /// </summary>
        public Vector3 ParentSpaceAxis2 { get; internal set; }

        /// <summary>
        ///     Gets <see cref="LocalSpaceAxis1ZeroAngleVector" /> in local space of the parent object.
        /// </summary>
        public Vector3 ParentSpaceAxis1ZeroAngleVector { get; internal set; }

        /// <summary>
        ///     Gets <see cref="LocalSpaceAxis2ZeroAngleVector" /> in local space of the parent object.
        /// </summary>
        public Vector3 ParentSpaceAxis2ZeroAngleVector { get; internal set; }

        /// <summary>
        ///     Gets the transformation matrix that gets from world-space to local space in the parent transform.
        /// </summary>
        public Matrix4x4 MtxToLocalParent { get; internal set; }

        /// <summary>
        ///     Gets rotation degrees around <see cref="RotationAxis1" />.
        /// </summary>
        public float Angle1 { get; internal set; }

        /// <summary>
        ///     Gets rotation degrees around <see cref="RotationAxis2" />.
        /// </summary>
        public float Angle2 { get; internal set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrCcdLink()
        {
            _weight         = 1.0f;
            _constraint     = UxrCcdConstraintType.SingleAxis;
            _rotationAxis1  = Vector3.right;
            _rotationAxis2  = Vector3.up;
            _axis1HasLimits = true;
            _axis1AngleMin  = -45.0f;
            _axis1AngleMax  = 45.0f;
            _axis2HasLimits = false;
            _axis2AngleMin  = -45.0f;
            _axis2AngleMax  = 45.0f;
        }

        #endregion
    }
}