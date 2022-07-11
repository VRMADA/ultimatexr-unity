// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUniversalLocalAxes.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Core.Math
{
    /// <summary>
    ///     <para>
    ///         Different parts of the framework need to deal with axes. These algorithms like IK solvers or avatar
    ///         components need to know exactly where 'forward' is or which axis points to the right in avatar-space.
    ///         Since modelling packages and artists may rig objects using arbitrary coordinate systems we need a way to
    ///         perform operations in a way that takes this into account. The code also needs to remain readable since many
    ///         math operations may increase complexity. Readability is favoured here over performance.
    ///     </para>
    ///     <para>
    ///         This class allows to transform from arbitrary coordinate systems to a universal one where different rotations
    ///         can then be performed and vice versa.
    ///         One example would be a finger bone curl. We create the convention that forward is the axis from one bone to
    ///         the next, up points upwards and right would be the axis around which the bone should rotate to curl. This is OK
    ///         but now we face the problem that different modelling packages or artists rig fingers in completely different
    ///         ways using all varieties of axis systems. The purpose of this class is to help creating a system where
    ///         operations can be performed in this universal system to follow our conventions and then rotated "back" to any
    ///         kind of coordinate system afterwards.
    ///     </para>
    ///     <para>
    ///         tl;dr A class that helps us operate with rotations and angles of an object no matter which convention the
    ///         3D assets use. We call 'Universal' the coordinate system we use as convention for our computations, we then
    ///         can use <see cref="UniversalToActualAxesRotation" /> to transform the object back to its actual axes.
    ///         This way our computations do not care which coordinate system the assets use, and is essential to simplify
    ///         operations like inverse kinematics or angle computations.
    ///     </para>
    /// </summary>
    [Serializable]
    public class UxrUniversalLocalAxes
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform  _transform;
        [SerializeField] private Vector3    _localRight                             = Vector3.right;
        [SerializeField] private Vector3    _localUp                                = Vector3.up;
        [SerializeField] private Vector3    _localForward                           = Vector3.forward;
        [SerializeField] private Quaternion _universalToActualAxesRotation          = Quaternion.identity;
        [SerializeField] private Quaternion _initialRotation                        = Quaternion.identity;
        [SerializeField] private Quaternion _initialLocalRotation                   = Quaternion.identity;
        [SerializeField] private Quaternion _initialLocalReferenceRotation          = Quaternion.identity;
        [SerializeField] private Quaternion _initialUniversalLocalReferenceRotation = Quaternion.identity;
        [SerializeField] private Vector3    _initialPosition                        = Vector3.zero;
        [SerializeField] private Vector3    _initialLocalPosition                   = Vector3.zero;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the universal 'right' direction in world space.
        /// </summary>
        public Vector3 WorldRight => _transform.TransformDirection(LocalRight);

        /// <summary>
        ///     Gets the universal 'up' direction in world space.
        /// </summary>
        public Vector3 WorldUp => _transform.TransformDirection(LocalUp);

        /// <summary>
        ///     Gets the universal 'forward' direction in world space.
        /// </summary>
        public Vector3 WorldForward => _transform.TransformDirection(LocalForward);

        /// <summary>
        ///     Gets the local object rotation in universal convention
        /// </summary>
        public Quaternion UniversalLocalRotation => _transform.localRotation * Quaternion.Inverse(UniversalToActualAxesRotation);

        /// <summary>
        ///     Gets the object rotation in universal convention
        /// </summary>
        public Quaternion UniversalRotation => _transform.rotation * Quaternion.Inverse(UniversalToActualAxesRotation);

        /// <summary>
        ///     Gets the universal 'right' direction in transform's local space.
        /// </summary>
        public Vector3 LocalRight
        {
            get => _localRight;
            private set => _localRight = value;
        }

        /// <summary>
        ///     Gets the universal 'up' direction in transform's local space.
        /// </summary>
        public Vector3 LocalUp
        {
            get => _localUp;
            private set => _localUp = value;
        }

        /// <summary>
        ///     Gets the universal 'forward' direction in transform's local space.
        /// </summary>
        public Vector3 LocalForward
        {
            get => _localForward;
            private set => _localForward = value;
        }

        /// <summary>
        ///     Gets the rotation that transforms from the universal axes to the convention that the transform follows.
        ///     <example>
        ///         <code>
        ///             // universalRotation may be a rotation around the y axis, where we know
        ///             // exactly that y points upwards in that space.
        ///             // This rotation will rotate an object around the "universal" y axis no
        ///             // matter where his actual axes point to.
        ///             transform.rotation = universalRotation * UniversalToActualAxesRotation;
        ///         </code>
        ///     </example>
        /// </summary>
        public Quaternion UniversalToActualAxesRotation
        {
            get => _universalToActualAxesRotation;
            private set => _universalToActualAxesRotation = value;
        }

        /// <summary>
        ///     Gets the transform's rotation at the time of setting the object up.
        /// </summary>
        public Quaternion InitialRotation
        {
            get => _initialRotation;
            private set => _initialRotation = value;
        }

        /// <summary>
        ///     Gets the transform's local rotation at the time of setting the object up.
        /// </summary>
        public Quaternion InitialLocalRotation
        {
            get => _initialLocalRotation;
            private set => _initialLocalRotation = value;
        }

        /// <summary>
        ///     Gets the transform's rotation with respect to the reference transform at the time of setting the object up.
        ///     This will only contain a rotation when the constructor using a reference transform was used.
        /// </summary>
        public Quaternion InitialLocalReferenceRotation
        {
            get => _initialLocalReferenceRotation;
            private set => _initialLocalReferenceRotation = value;
        }

        /// <summary>
        ///     Gets the transform's rotation (in universal coordinates) with respect to the reference transform at the time of
        ///     setting the object up.
        ///     This will only contain a rotation when the constructor using a reference transform was used.
        /// </summary>
        public Quaternion InitialUniversalLocalReferenceRotation
        {
            get => _initialUniversalLocalReferenceRotation;
            private set => _initialUniversalLocalReferenceRotation = value;
        }

        /// <summary>
        ///     Gets the transform's position at the time of setting the object up.
        /// </summary>
        public Vector3 InitialPosition
        {
            get => _initialPosition;
            private set => _initialPosition = value;
        }

        /// <summary>
        ///     Gets the transform's local position at the time of setting the object up.
        /// </summary>
        public Vector3 InitialLocalPosition
        {
            get => _initialLocalPosition;
            private set => _initialLocalPosition = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        ///     Uses universalReference to check which axes of a transform are actually the ones
        ///     that are right, up and forward. For example, universalReference may be the avatar
        ///     root where we know that right, up and forward point to these actual directions and
        ///     we want to know which axes of an upper body part point to these directions too.
        ///     These may be completely different depending on the modelling package or artist.
        ///     Using this class we can easily check which one points upwards and create a small
        ///     chest torsion by rotating around this axis.
        /// </summary>
        /// <param name="transform">Transform to create the universal axes for</param>
        /// <param name="universalReference">
        ///     The transform to use as a reference for the universal right, up and forward directions.
        /// </param>
        public UxrUniversalLocalAxes(Transform transform, Transform universalReference)
        {
            _transform = transform;

            LocalRight   = transform.InverseTransformDirection(universalReference != null ? universalReference.right : Vector3.right).GetClosestAxis();
            LocalUp      = transform.InverseTransformDirection(universalReference != null ? universalReference.up : Vector3.up).GetClosestAxis();
            LocalForward = transform.InverseTransformDirection(universalReference != null ? universalReference.forward : Vector3.forward).GetClosestAxis();

            UniversalToActualAxesRotation = GetUniversalToActualAxesRotation();

            InitialRotation                        = transform.rotation;
            InitialLocalRotation                   = transform.localRotation;
            InitialLocalReferenceRotation          = Quaternion.Inverse(universalReference.rotation) * transform.rotation;
            InitialUniversalLocalReferenceRotation = InitialLocalReferenceRotation * Quaternion.Inverse(UniversalToActualAxesRotation);
            InitialPosition                        = transform.position;
            InitialLocalPosition                   = transform.localPosition;
        }

        /// <summary>
        ///     Default constructor is private to make it inaccessible.
        /// </summary>
        private UxrUniversalLocalAxes()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a UniversalLocalAxes object describing the universal local axes for the given transform.
        /// </summary>
        /// <param name="transform">The transform the UniversalLocalAxes object is for</param>
        /// <param name="universalLocalRight">
        ///     Which vector in the transform local coordinates points to the 'right' direction in the universal convention
        /// </param>
        /// <param name="universalLocalUp">
        ///     Which vector in the transform local coordinates points to the 'up' direction in the universal convention
        /// </param>
        /// <param name="universalLocalForward">
        ///     Which vector in the transform local coordinates points to the 'forward' direction in the universal convention
        /// </param>
        /// <returns>
        ///     UniversalLocalAxes object that allows us to compute object rotations in a universal
        ///     space and then apply it to a transform that can have any kind of axis convention
        ///     (x may point up, z down...)
        /// </returns>
        public static UxrUniversalLocalAxes FromAxes(Transform transform, Vector3 universalLocalRight, Vector3 universalLocalUp, Vector3 universalLocalForward)
        {
            UxrUniversalLocalAxes localAxes = new UxrUniversalLocalAxes();

            localAxes._transform   = transform;
            localAxes.LocalRight   = universalLocalRight;
            localAxes.LocalUp      = universalLocalUp;
            localAxes.LocalForward = universalLocalForward;

            localAxes.InitialRotation                        = transform.rotation;
            localAxes.InitialLocalRotation                   = transform.localRotation;
            localAxes.InitialLocalReferenceRotation          = Quaternion.identity;
            localAxes.InitialUniversalLocalReferenceRotation = Quaternion.identity;
            localAxes.InitialPosition                        = transform.position;
            localAxes.InitialLocalPosition                   = transform.localPosition;

            localAxes.UniversalToActualAxesRotation = localAxes.GetUniversalToActualAxesRotation();

            return localAxes;
        }

        /// <summary>
        ///     See <see cref="FromAxes" />.
        /// </summary>
        public static UxrUniversalLocalAxes FromRightUp(Transform transform, Vector3 universalLocalRight, Vector3 universalLocalUp)
        {
            return FromAxes(transform, universalLocalRight, universalLocalUp, Vector3.Cross(universalLocalRight, universalLocalUp));
        }

        /// <summary>
        ///     See <see cref="FromAxes" />.
        /// </summary>
        public static UxrUniversalLocalAxes FromRightForward(Transform transform, Vector3 universalLocalRight, Vector3 universalLocalForward)
        {
            return FromAxes(transform, universalLocalRight, Vector3.Cross(universalLocalForward, universalLocalRight), universalLocalForward);
        }

        /// <summary>
        ///     See <see cref="FromAxes" />.
        /// </summary>
        public static UxrUniversalLocalAxes FromUpForward(Transform transform, Vector3 universalLocalUp, Vector3 universalLocalForward)
        {
            return FromAxes(transform, Vector3.Cross(universalLocalUp, universalLocalForward), universalLocalUp, universalLocalForward);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes the rotation that transforms from the universal coordinate system to the convention that the transform
        ///     follows.
        /// </summary>
        private Quaternion GetUniversalToActualAxesRotation()
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetColumn(0, LocalRight);
            matrix.SetColumn(1, LocalUp);
            matrix.SetColumn(2, LocalForward);
            matrix.SetColumn(3, new Vector4(0, 0, 0, 1));

            return Quaternion.Inverse(matrix.rotation);
        }

        #endregion
    }
}