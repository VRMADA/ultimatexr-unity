// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

#pragma warning disable 0414

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" />, will enable the object to be grabbed by the
    ///     <see cref="UxrGrabber" /> components found in the hands of an <see cref="UxrAvatar" />.
    ///     Some of the main features of grabbable objects are:
    ///     <list type="bullet">
    ///         <item>
    ///             Manipulation is handled automatically by the <see cref="UxrGrabManager" />. There is no special
    ///             requirement to enable it in a scene, the grab manager will be available as soon as it is invoked.
    ///         </item>
    ///         <item>
    ///             Grabbable objects can be grabbed, released and placed. Releasing an object means dropping it mid-air,
    ///             while placing it is releasing an object close enough to a compatible
    ///             <see cref="UxrGrabbableObjectAnchor" />.
    ///         </item>
    ///         <item>Objects can be grabbed from different grab points.</item>
    ///         <item>
    ///             Additionally, grab points can be expanded using <see cref="UxrGrabPointShape" /> components opening up
    ///             more complex manipulation by describing grab points as composite shapes.
    ///         </item>
    ///         <item>
    ///             Although all avatars that have <see cref="UxrGrabber" /> components are able to interact with
    ///             <see cref="UxrGrabbableObject" /> objects, it is possible to register the way specific avatars will
    ///             interact with it. This allows to specify snap points and poses for different avatars and make sure
    ///             that all have precise and realistic manipulation.
    ///         </item>
    ///         <item>
    ///             The Hand Pose Editor can create poses that are used by <see cref="UxrGrabbableObject" /> in order to tell
    ///             how objects are grabbed. The inspector window will preview grab poses and enable editing them.
    ///         </item>
    ///         <item>
    ///             Events such as <see cref="Grabbed" />, <see cref="Released" /> and <see cref="Placed" /> allow to write
    ///             logic when a user interacts with the object. Each has pre and post events.
    ///         </item>
    ///         <item>
    ///             <see cref="ConstraintsApplying" /> and <see cref="ConstraintsApplied" /> allow to program more complex
    ///             logic when grabbing objects.
    ///         </item>
    ///     </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class UxrGrabbableObject : UxrComponent<UxrGrabbableObject>, IUxrGrabbable
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrManipulationMode _manipulationMode       = UxrManipulationMode.GrabAndMove;
        [SerializeField] private bool                _controlParentDirection = true;
        [SerializeField] private bool                _ignoreGrabbableParentDependency;

        [SerializeField] private int       _priority;
        [SerializeField] private Rigidbody _rigidBodySource;
        [SerializeField] private bool      _rigidBodyDynamicOnRelease   = true;
        [SerializeField] private float     _verticalReleaseMultiplier   = 1.0f;
        [SerializeField] private float     _horizontalReleaseMultiplier = 1.0f;
        [SerializeField] private bool      _needsTwoHandsToRotate;
        [SerializeField] private bool      _allowMultiGrab = true;

        [SerializeField] private UxrPreviewGrabPoses    _previewGrabPosesMode = UxrPreviewGrabPoses.ShowBothHands;
        [SerializeField] private int                    _previewPosesRegenerationType;
        [SerializeField] private int                    _previewPosesRegenerationIndex = -1;
        [SerializeField] private GameObject             _selectedAvatarForGrips;
        [SerializeField] private bool                   _firstGrabPointIsMain = true;
        [SerializeField] private UxrGrabPointInfo       _grabPoint;
        [SerializeField] private List<UxrGrabPointInfo> _additionalGrabPoints;

        [SerializeField] private bool                     _useParenting;
        [SerializeField] private bool                     _autoCreateStartAnchor;
        [SerializeField] private UxrGrabbableObjectAnchor _startAnchor;
        [SerializeField] private string                   _tag                       = "";
        [SerializeField] private bool                     _dropAlignTransformUseSelf = true;
        [SerializeField] private Transform                _dropAlignTransform;
        [SerializeField] private UxrSnapToAnchorMode      _dropSnapMode                  = UxrSnapToAnchorMode.PositionAndRotation;
        [SerializeField] private bool                     _dropProximityTransformUseSelf = true;
        [SerializeField] private Transform                _dropProximityTransform;

        [SerializeField] private UxrTranslationConstraintMode _translationConstraintMode = UxrTranslationConstraintMode.Free;
        [SerializeField] private BoxCollider                  _restrictToBox;
        [SerializeField] private SphereCollider               _restrictToSphere;
        [SerializeField] private Vector3                      _translationLimitsMin               = Vector3.zero;
        [SerializeField] private Vector3                      _translationLimitsMax               = Vector3.zero;
        [SerializeField] private bool                         _translationLimitsReferenceIsParent = true;
        [SerializeField] private Transform                    _translationLimitsParent;
        [SerializeField] private UxrRotationConstraintMode    _rotationConstraintMode          = UxrRotationConstraintMode.Free;
        [SerializeField] private Vector3                      _rotationAngleLimitsMin          = Vector3.zero;
        [SerializeField] private Vector3                      _rotationAngleLimitsMax          = Vector3.zero;
        [SerializeField] private bool                         _rotationLimitsReferenceIsParent = true;
        [SerializeField] private Transform                    _rotationLimitsParent;
        [SerializeField] private float                        _lockedGrabReleaseDistance = 0.4f;
        [SerializeField] private float                        _translationResistance;
        [SerializeField] private float                        _rotationResistance;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Seconds it takes to smoothly transition an object from/to the hand.
        /// </summary>
        public const float ObjectAlignmentSeconds = 0.1f;

        /// <summary>
        ///     Seconds it takes to smoothly transition a hand to or from a locked grab (a grab that forces to move the hand bone
        ///     out of its natural position because the object is constrained in space).
        /// </summary>
        public const float HandLockSeconds = 0.1f;

        /// <summary>
        ///     Seconds it takes to smoothly transition an object to or from its space constraints.
        /// </summary>
        public const float ConstrainSeconds = 0.1f;

        /// <summary>
        ///     How much a difference in angle will offset the distance at which we compute a grabbable object from a grabber.
        ///     Objects that are not so well aligned with the grabber will be considered slightly farther away by(angle*
        ///     DistanceOffsetByAngle) units, this means in the [0, 0.05] range.
        ///     This will favor grabbing objects that are better aligned when there are two or more at similar distances.
        /// </summary>
        public const float DistanceOffsetByAngle = 1.0f / 3600.0f;

        /// <summary>
        ///     Used by the editor to identify the default avatar when no avatars have been registered for grips.
        /// </summary>
        public const string DefaultAvatarName = "[Default]";

        /// <summary>
        ///     Used by the editor to prefix the left grab pose mesh.
        /// </summary>
        public const string LeftGrabPoseMeshSuffix = " left";

        /// <summary>
        ///     Used by the editor to prefix the right grab pose mesh.
        /// </summary>
        public const string RightGrabPoseMeshSuffix = " right";

        /// <summary>
        ///     Event called right before applying the position/rotation constraints to the object.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsApplying;

        /// <summary>
        ///     Event called right after applying the position/rotation constraints to the object.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsApplied;

        /// <summary>
        ///     Gets if the object has constraints and at the same time has a grabbable parent. This means that the object can
        ///     either be considered as another grabbable part of the parent object or a separate grabbable object that is just
        ///     attached to the parent object but has no control over it. The former are movable parts in a composite object while
        ///     the latter are independent grabbable objects that happen to be in the hierarchy.
        /// </summary>
        public bool HasGrabbableParentDependency => IsConstrained && GetGrabbableParentDependency(transform) != null;

        /// <summary>
        ///     <para>
        ///         Gets whether the object has a parent dependency (<see cref="HasGrabbableParentDependency" /> is true) and is
        ///         using it through <see cref="_ignoreGrabbableParentDependency" /> in the inspector.
        ///     </para>
        ///     <para>
        ///         When a grabbable object that has position/rotation constraints hangs from a hierarchy where another grabbable
        ///         object is somewhere above, the child grabbable object can either be:
        ///         <list type="bullet">
        ///             <item>
        ///                 Dependent (<see cref="_ignoreGrabbableParentDependency" /> is false): The object is considered as
        ///                 another part of the parent grabbable object. It will be constrained by its parent object and can
        ///                 optionally also control the parent's direction when both are being grabbed.
        ///             </item>
        ///             <item>
        ///                 Independent (<see cref="_ignoreGrabbableParentDependency" /> is true): The object is considered as a
        ///                 separate entity where it just happens to be placed under the hierarchy.
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool UsesGrabbableParentDependency => HasGrabbableParentDependency && !_ignoreGrabbableParentDependency;

        /// <summary>
        ///     Gets the Transform that the object is dependent on, when the grabbable object has another above in its hierarchy.
        /// </summary>
        public Transform GrabbableParentDependency => GetGrabbableParentDependency(transform);

        /// <summary>
        ///     Gets whether the object has position/rotation constraints.
        /// </summary>
        public bool IsConstrained => TranslationConstraint != UxrTranslationConstraintMode.Free ||
                                     RotationConstraint != UxrRotationConstraintMode.Free ||
                                     Manipulation == UxrManipulationMode.RotateAroundAxis ||
                                     IsLockedInPlace ||
                                     _constraintExitTimer > 0.0f;

        /// <summary>
        ///     Gets the total number of grab points.
        /// </summary>
        public int GrabPointCount => _additionalGrabPoints != null ? _additionalGrabPoints.Count + 1 : 1;

        /// <summary>
        ///     Gets the <see cref="Transform" /> that needs to align with a <see cref="UxrGrabbableObjectAnchor" /> when placing
        ///     the object on it.
        /// </summary>
        public Transform DropAlignTransform => _dropAlignTransform == null || _dropAlignTransformUseSelf ? transform : _dropAlignTransform;

        /// <summary>
        ///     Gets how the object will align with a <see cref="UxrGrabbableObjectAnchor" /> when placing it.
        /// </summary>
        public UxrSnapToAnchorMode DropSnapMode => _dropSnapMode;

        /// <summary>
        ///     Gets the <see cref="Transform" /> that will be used to compute the distance to
        ///     <see cref="UxrGrabbableObjectAnchor" /> components when looking for the closest available to place it.
        /// </summary>
        public Transform DropProximityTransform => _dropProximityTransform == null || _dropProximityTransformUseSelf ? transform : _dropProximityTransform;

        /// <summary>
        ///     Gets the distance that the real hand needs to have to the virtual hand in order for the object grip to be released
        ///     automatically. This happens when a grabbed object has a range of movement and the grip is pulled too far from a
        ///     valid position.
        /// </summary>
        public float LockedGrabReleaseDistance => _lockedGrabReleaseDistance;

        /// <summary>
        ///     Gets whether the object's <see cref="RigidBodySource" /> will be made dynamic when the object grip is released.
        /// </summary>
        public bool RigidBodyDynamicOnRelease => _rigidBodyDynamicOnRelease;

        /// <summary>
        ///     Gets the vertical velocity factor that will be applied to the object when being thrown.
        /// </summary>
        public float VerticalReleaseMultiplier => _verticalReleaseMultiplier;

        /// <summary>
        ///     Gets the horizontal velocity factor that will be applied to the object when being thrown.
        /// </summary>
        public float HorizontalReleaseMultiplier => _horizontalReleaseMultiplier;

        /// <summary>
        ///     Gets whether the object requires both hands grabbing it in order to rotate it.
        /// </summary>
        public bool NeedsTwoHandsToRotate => _needsTwoHandsToRotate;

        /// <summary>
        ///     Gets whether the object can be grabbed with more than one hand.
        /// </summary>
        public bool AllowMultiGrab => _allowMultiGrab;

        /// <summary>
        ///     Gets the starting <see cref="UxrGrabbableObjectAnchor" /> the object is placed on.
        /// </summary>
        public UxrGrabbableObjectAnchor StartAnchor => _startAnchor;

        /// <summary>
        ///     Gets the <see cref="UxrGrabbableObjectAnchor" /> where the object is actually placed or null if it's not placed on
        ///     any.
        /// </summary>
        public UxrGrabbableObjectAnchor CurrentAnchor { get; internal set; }

        /// <summary>
        ///     Gets or sets whether the object can be placed on an <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        public bool IsPlaceable { get; set; } = true;

        /// <summary>
        ///     Gets or sets whether the object can be moved/rotated. A locked in place object may be grabbed but cannot be moved.
        /// </summary>
        public bool IsLockedInPlace { get; set; } = false;

        /// <summary>
        ///     Gets or sets the manipulation mode used by the grabbable object.
        /// </summary>
        public UxrManipulationMode Manipulation
        {
            get => _manipulationMode;
            set => _manipulationMode = value;
        }

        /// <summary>
        ///     Gets or sets whether a dependent object can control the grabbable parent's direction when both are being grabbed at
        ///     the same time.
        /// </summary>
        public bool ControlParentDirection
        {
            get => _controlParentDirection;
            set => _controlParentDirection = value;
        }

        /// <summary>
        ///     Gets or sets whether to ignore the grabbable parent dependency. <see cref="UsesGrabbableParentDependency" />.
        /// </summary>
        public bool IgnoreGrabbableParentDependency
        {
            get => _ignoreGrabbableParentDependency;
            set => _ignoreGrabbableParentDependency = value;
        }

        /// <summary>
        ///     Gets or sets the object priority. The priority is used to control which object will be grabbed when multiple
        ///     objects are in reach and the user performs the grab gesture.
        ///     The default behaviour is to use the distance and orientation to the objects in reach to select the one with the
        ///     closest grip. The priority can override this behaviour
        ///     by selecting the one with the highest priority value. By default all objects have priority 0.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        /// <summary>
        ///     Specifies the rigidbody component that controls the grabbable object when it is in dynamic (physics-enabled) mode.
        /// </summary>
        public Rigidbody RigidBodySource
        {
            get => _rigidBodySource;
            set => _rigidBodySource = value;
        }

        /// <summary>
        ///     Gets or sets whether the first grab point in the list is the main grab in objects with more than one grab point.
        ///     When an object is grabbed with both hands, the main grab controls the actual position while the secondary grab
        ///     controls the direction.
        ///     Set it to true in objects like a rifle, where the trigger hand should be the first grab in order to keep the object
        ///     in place, and the front grab will control the aiming direction.
        ///     If false, the grab point order is irrelevant and the hand that grabbed the object first will be considered as the
        ///     main grab.
        /// </summary>
        public bool FirstGrabPointIsMain
        {
            get => _firstGrabPointIsMain;
            set => _firstGrabPointIsMain = value;
        }

        /// <summary>
        ///     Gets or sets whether to parent the object to the <see cref="UxrGrabbableObjectAnchor" /> being placed. Also whether
        ///     to set the parent to null when grabbing the object from one.
        /// </summary>
        public bool UseParenting
        {
            get => _useParenting;
            set => _useParenting = value;
        }

        /// <summary>
        ///     String that identifies which <see cref="UxrGrabbableObjectAnchor" /> components are compatible for placement. A
        ///     <see cref="UxrGrabbableObject" /> can be placed on an <see cref="UxrGrabbableObjectAnchor" /> only if:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="Tag" /> is null or empty and <see cref="UxrGrabbableObjectAnchor" /> has no compatible tags
        ///             set
        ///         </item>
        ///         <item>
        ///             <see cref="Tag" /> has a value that is in one of the compatible tag entries in
        ///             <see cref="UxrGrabbableObjectAnchor" />
        ///         </item>
        ///     </list>
        /// </summary>
        public string Tag
        {
            get => _tag;
            set => _tag = value;
        }

        /// <summary>
        ///     Gets or sets the translation constraint.
        /// </summary>
        public UxrTranslationConstraintMode TranslationConstraint
        {
            get => _translationConstraintMode;
            set => _translationConstraintMode = value;
        }

        /// <summary>
        ///     Gets or sets the box collider used When <see cref="TranslationConstraint" /> is
        ///     <see cref="UxrTranslationConstraintMode.RestrictToBox" />.
        /// </summary>
        public BoxCollider RestrictToBox
        {
            get => _restrictToBox;
            set => _restrictToBox = value;
        }

        /// <summary>
        ///     Gets or sets the sphere collider used When <see cref="TranslationConstraint" /> is
        ///     <see cref="UxrTranslationConstraintMode.RestrictToSphere" />.
        /// </summary>
        public SphereCollider RestrictToSphere
        {
            get => _restrictToSphere;
            set => _restrictToSphere = value;
        }

        /// <summary>
        ///     Gets or sets the translation minimum limits in local space when <see cref="TranslationConstraint" /> is
        ///     <see cref="UxrTranslationConstraintMode.RestrictLocalOffset" />.
        /// </summary>
        public Vector3 TranslationLimitsMin
        {
            get => _translationLimitsMin;
            set => _translationLimitsMin = value;
        }

        /// <summary>
        ///     Gets or sets the translation maximum limits in local space when <see cref="TranslationConstraint" /> is
        ///     <see cref="UxrTranslationConstraintMode.RestrictLocalOffset" />.
        /// </summary>
        public Vector3 TranslationLimitsMax
        {
            get => _translationLimitsMax;
            set => _translationLimitsMax = value;
        }

        /// <summary>
        ///     Gets or sets whether the reference for the <see cref="TranslationLimitsMin" /> and
        ///     <see cref="TranslationLimitsMax" /> is the object's parent. A different object can be specified using
        ///     <see cref="TranslationLimitsParent" />.
        /// </summary>
        public bool IsParentTranslationLimitsReference
        {
            get => _translationLimitsReferenceIsParent;
            set => _translationLimitsReferenceIsParent = value;
        }

        /// <summary>
        ///     Gets or sets the reference for <see cref="TranslationLimitsMin" /> and <see cref="TranslationLimitsMax" /> when
        ///     <see cref="IsParentTranslationLimitsReference" /> is used.
        /// </summary>
        public Transform TranslationLimitsParent
        {
            get => _translationLimitsParent;
            set => _translationLimitsParent = value;
        }

        /// <summary>
        ///     Gets or sets the initial local position with respect to the translation constraint reference.
        /// </summary>
        public Vector3 InitialLocalPositionToReference { get; set; } = Vector3.zero;

        /// <summary>
        ///     Gets or sets the rotation constraint type.
        /// </summary>
        public UxrRotationConstraintMode RotationConstraint
        {
            get => _rotationConstraintMode;
            set => _rotationConstraintMode = value;
        }

        /// <summary>
        ///     Gets or sets the rotational minimum limits in local space when <see cref="RotationConstraint" /> is
        ///     <see cref="UxrRotationConstraintMode.RestrictLocalRotation" />.
        /// </summary>
        public Vector3 RotationAngleLimitsMin
        {
            get => _rotationAngleLimitsMin;
            set => _rotationAngleLimitsMin = value;
        }

        /// <summary>
        ///     Gets or sets the rotational maximum limits in local space when <see cref="RotationConstraint" /> is
        ///     <see cref="UxrRotationConstraintMode.RestrictLocalRotation" />.
        /// </summary>
        public Vector3 RotationAngleLimitsMax
        {
            get => _rotationAngleLimitsMax;
            set => _rotationAngleLimitsMax = value;
        }

        /// <summary>
        ///     Gets or sets whether the reference for the <see cref="RotationAngleLimitsMin" /> and
        ///     <see cref="RotationAngleLimitsMax" /> is the object's parent. A different object can be specified using
        ///     <see cref="RotationLimitsParent" />.
        /// </summary>
        public bool IsParentRotationLimitsReference
        {
            get => _rotationLimitsReferenceIsParent;
            set => _rotationLimitsReferenceIsParent = value;
        }

        /// <summary>
        ///     Gets or sets the reference for <see cref="RotationAngleLimitsMin" /> and <see cref="RotationAngleLimitsMax" /> when
        ///     <see cref="IsParentRotationLimitsReference" /> is used.
        /// </summary>
        public Transform RotationLimitsParent
        {
            get => _rotationLimitsParent;
            set => _rotationLimitsParent = value;
        }

        /// <summary>
        ///     Gets or sets the initial local rotation angles with respect to the rotation constraint reference.
        /// </summary>
        public Vector3 InitialLocalEulerAnglesToReference { get; set; } = Vector3.zero;

        /// <summary>
        ///     Gets or sets the resistance to the object being moved around. This can be used to smooth out the position but also
        ///     to simulate heavy objects.
        /// </summary>
        public float TranslationResistance
        {
            get => _translationResistance;
            set => _translationResistance = value;
        }

        /// <summary>
        ///     Gets or sets the resistance to the object being rotated. This can be used to smooth out the rotation but also to
        ///     simulate heavy objects.
        /// </summary>
        public float RotationResistance
        {
            get => _rotationResistance;
            set => _rotationResistance = value;
        }

        #endregion

        #region Implicit IUxrGrabbable

        /// <inheritdoc />
        public bool IsBeingGrabbed => UxrGrabManager.Instance.IsBeingGrabbed(this);

        /// <inheritdoc />
        public bool IsGrabbable { get; set; } = true;

        /// <inheritdoc />
        public bool IsKinematic
        {
            get => _rigidBodySource == null || _rigidBodySource.isKinematic;
            set
            {
                if (_rigidBodySource != null)
                {
                    _rigidBodySource.isKinematic = value;
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Grabbing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Grabbed;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Releasing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Released;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Placing;

        /// <inheritdoc />
        public event EventHandler<UxrManipulationEventArgs> Placed;

        /// <inheritdoc />
        public void ResetPositionAndState(bool propagateEvents)
        {
            transform.localPosition = InitialLocalPosition;
            transform.localRotation = InitialLocalRotation;
            IsKinematic             = _initialIsKinematic;

            if (_startAnchor)
            {
                UxrGrabManager.Instance.PlaceObject(this, _startAnchor, UxrPlacementType.Immediate, propagateEvents);
            }
        }

        /// <inheritdoc />
        public void ReleaseGrabs(bool propagateEvents)
        {
            UxrGrabManager.Instance.ReleaseGrabs(this, propagateEvents);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets a given grab point information.
        /// </summary>
        /// <param name="index">Grab point index to get the information of</param>
        /// <returns>Grab point information</returns>
        public UxrGrabPointInfo GetGrabPoint(int index)
        {
            if (index == 0)
            {
                return _grabPoint;
            }

            if (index >= 1 && index <= _additionalGrabPoints.Count)
            {
                return _additionalGrabPoints[index - 1];
            }

            return null;
        }

        /// <summary>
        ///     Enables or disables the possibility to use the given grab point.
        /// </summary>
        /// <param name="grabPoint">Grab point index</param>
        /// <param name="grabPointEnabled">Whether to enable or disable interaction</param>
        public void SetGrabPointEnabled(int grabPoint, bool grabPointEnabled)
        {
            if (_grabPointEnabledStates.ContainsKey(grabPoint))
            {
                if (grabPointEnabled)
                {
                    _grabPointEnabledStates.Remove(grabPoint);
                }
                else
                {
                    _grabPointEnabledStates[grabPoint] = false; // This is redundant
                }
            }
            else if (!grabPointEnabled)
            {
                _grabPointEnabledStates.Add(grabPoint, false);
            }
        }

        /// <summary>
        ///     Re-enables all disabled grab points by <see cref="SetGrabPointEnabled" />.
        /// </summary>
        public void EnableAllGrabPoints()
        {
            _grabPointEnabledStates.Clear();
        }

        /// <summary>
        ///     Computes the distance from the object to a <see cref="UxrGrabber" />, which is the component found in
        ///     <see cref="UxrAvatar" /> hands that are able to grab objects.
        /// </summary>
        /// <param name="grabber">Grabber component</param>
        /// <param name="grabPoint">Grab point index to compute the distance to</param>
        /// <param name="distance">
        ///     Returns the distance, which is not be actual euclidean distance but a value that also takes into
        ///     account the relative rotation between the grabber and the grab point. This helps favoring grabs that have a more
        ///     convenient orientation to the grabber and are just a little farther away
        /// </param>
        /// <param name="distanceWithoutRotation">Returns the euclidean distance, without factoring in any relative rotation</param>
        public void GetDistanceFromGrabber(UxrGrabber grabber, int grabPoint, out float distance, out float distanceWithoutRotation)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            distance = Vector3.Distance(grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex).position, GetGrabPointGrabProximityTransform(grabber, grabPoint).position);

            // distanceRotationAdd will store the distance added to count for the rotation and favor those grips closer in orientation to the grabber

            float distanceRotationAdd = 0.0f;

            // First check if there is an UxrGrabPointShape based component that describes this 

            UxrGrabPointShape grabPointShape = GetGrabPointShape(grabPoint);

            if (grabPointShape != null)
            {
                distance = grabPointShape.GetDistanceFromGrabber(grabber,
                                                                 GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side),
                                                                 GetGrabPointGrabProximityTransform(grabber, grabPoint));
            }
            else
            {
                // If there is no UxrGrabPointShape for this grabPoint just compute the distance normally

                if (grabPointInfo.GrabProximityMode == UxrGrabProximityMode.UseProximity)
                {
                    // Offset distance slightly based on relative orientation of grabber and grabbableObject to favor objects whose grab axes are more closely aligned to the grabber.
                    // This way we can put 2 grabs for different hand orientations in the same position and the one with a closer alignment will be favoured.

                    if (GetGrabPoint(grabPoint).SnapMode == UxrSnapToHandMode.RotationOnly || GetGrabPoint(grabPoint).SnapMode == UxrSnapToHandMode.PositionAndRotation)
                    {
                        float relativeAngleDegrees = Quaternion.Angle(grabber.transform.rotation, GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side).rotation);
                        distanceRotationAdd = Mathf.Abs(relativeAngleDegrees) * DistanceOffsetByAngle;
                    }
                }
                else if (grabPointInfo.GrabProximityMode == UxrGrabProximityMode.BoxConstrained)
                {
                    if (grabPointInfo.GrabProximityBox)
                    {
                        distance = grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex).position.IsInsideBox(grabPointInfo.GrabProximityBox) ? distance : float.MaxValue;
                    }
                }
            }

            // Do not allow to grab if there is a hand grabbing another grabPoint nearby

            for (int otherGrabbedPoint = 0; otherGrabbedPoint < GrabPointCount; ++otherGrabbedPoint)
            {
                if (otherGrabbedPoint == grabPoint && grabPointShape == null)
                {
                    continue;
                }

                if (UxrGrabManager.Instance.GetGrabbingHand(this, otherGrabbedPoint, out UxrGrabber otherGrabber))
                {
                    if (GetGrabPoint(grabPoint).SnapMode == UxrSnapToHandMode.PositionAndRotation && GetGrabPoint(otherGrabbedPoint).SnapMode == UxrSnapToHandMode.PositionAndRotation)
                    {
                        // Other hand nearby and both have full snap mode? Check if there is room for the new hand

                        if (grabPointShape && otherGrabbedPoint == grabPoint)
                        {
                            // Grabbing same shape. Check if proposed snap point is too close to the other hand or not.

                            grabPointShape.GetClosestSnap(grabber,
                                                          GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side),
                                                          GetGrabPointGrabProximityTransform(grabber, grabPoint),
                                                          out Vector3 snapPos,
                                                          out Quaternion snapRot);

                            if (Vector3.Distance(snapPos, otherGrabber.transform.position) <= MinHandGrabInterDistance)
                            {
                                // The other hand is grabbing the same shape and is too close. Avoid this by increasing distance.
                                distance += 100000.0f;
                            }
                        }
                        else if (Vector3.Distance(GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint,         grabber.Side).position,
                                                  GetGrabPointGrabAlignTransform(grabber.Avatar, otherGrabbedPoint, otherGrabber.Side).position) <= MinHandGrabInterDistance)
                        {
                            // Grabbing other point whose snapping point is too close. Avoid this by increasing distance.
                            distance += 100000.0f;
                        }
                    }
                }
            }

            // Factor in the orientation

            distanceWithoutRotation =  distance;
            distance                += distanceRotationAdd;
        }

        /// <summary>
        ///     Checks whether the object can be grabbed by a <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPoint">Grab point index to check</param>
        /// <returns>Whether the object can be grabbed by the grabber using the given grab point</returns>
        public bool CanBeGrabbedByGrabber(UxrGrabber grabber, int grabPoint)
        {
            if (_grabPointEnabledStates.ContainsKey(grabPoint))
            {
                // It always is false when it exists. This has manually been set up by SetGrabPointEnabled()
                return false;
            }

            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            if (grabber == null || IsGrabbable == false || isActiveAndEnabled == false || grabPointInfo == null)
            {
                return false;
            }

            if (grabPointInfo.BothHandsCompatible == false && grabPointInfo.HandSide != grabber.Side)
            {
                // Invalid hand
                return false;
            }

            bool isBeingGrabbedByOtherPoint = GrabPointCount > 1 && UxrGrabManager.Instance.IsBeingGrabbed(this) && !UxrGrabManager.Instance.IsBeingGrabbed(this, grabPoint);
            bool isBeingGrabbedBySameShape  = _grabPointShapes.ContainsKey(grabPoint) && UxrGrabManager.Instance.IsBeingGrabbed(this, grabPoint);

            if (!AllowMultiGrab && (isBeingGrabbedByOtherPoint || isBeingGrabbedBySameShape))
            {
                // Object does not allow to be grabbed with more than one hand

                // We skip this check because we want to be able to switch from one hand to the other.
                // @TODO: Check if we really need this. Maybe add a flag to check for it or not.
                //return false;
            }

            GetDistanceFromGrabber(grabber, grabPoint, out float distance, out float distanceWithoutRotation);

            if (grabPointInfo.GrabProximityMode == UxrGrabProximityMode.BoxConstrained)
            {
                if (grabPointInfo.GrabProximityBox)
                {
                    return grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex).position.IsInsideBox(grabPointInfo.GrabProximityBox);
                }
            }
            else if (distanceWithoutRotation <= Mathf.Max(0.0f, GetGrabPoint(grabPoint).MaxDistanceGrab))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Computes the position/rotation that a <see cref="UxrGrabber" /> would have to hold the object using the current
        ///     position/orientation.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <param name="grabPoint">Grab point</param>
        /// <param name="grabberPosition">Returns the grabber position</param>
        /// <param name="grabberRotation">Returns the grabber orientation</param>
        /// <returns>Whether the returned data is meaningful</returns>
        public bool ComputeRequiredGrabberTransform(UxrGrabber grabber, int grabPoint, out Vector3 grabberPosition, out Quaternion grabberRotation)
        {
            grabberPosition = Vector3.zero;
            grabberRotation = Quaternion.identity;

            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            Transform        snapTransform = GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side);

            if (snapTransform == null || grabPointInfo == null)
            {
                return false;
            }

            grabberPosition = snapTransform.position;
            grabberRotation = snapTransform.rotation;

            UxrGrabPointShape grabPointShape = GetGrabPointShape(grabPoint);

            if (grabPointShape != null)
            {
                grabPointShape.GetClosestSnap(grabber, snapTransform, GetGrabPointGrabProximityTransform(grabber, grabPoint), out grabberPosition, out grabberRotation);
            }

            if (grabPointInfo.AlignToController)
            {
                UxrController3DModel controller3DModel = grabber != null && grabber.Avatar != null ? grabber.Avatar.ControllerInput.GetController3DModel(grabber.Side) : null;

                if (controller3DModel != null)
                {
                    Quaternion relativeTrackerRotation = Quaternion.Inverse(grabber.transform.rotation) * controller3DModel.ForwardTrackingRotation;
                    Quaternion trackerRotation         = grabberRotation * relativeTrackerRotation;
                    Quaternion sourceAlignAxes         = grabPointInfo.AlignToControllerAxes != null ? grabPointInfo.AlignToControllerAxes.rotation : transform.rotation;

                    grabberRotation = sourceAlignAxes * Quaternion.Inverse(trackerRotation) * grabberRotation;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks whether the object is near enough to be placed on the given <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        /// <param name="anchor">Anchor to check</param>
        /// <returns>Whether it is near enough to be placed</returns>
        public bool CanBePlacedOnAnchor(UxrGrabbableObjectAnchor anchor)
        {
            return CanBePlacedOnAnchor(anchor, out float distance);
        }

        /// <summary>
        ///     Checks whether the object is near enough to be placed on the given <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        /// <param name="anchor">Anchor to check</param>
        /// <param name="distance">Returns the euclidean distance to the anchor</param>
        /// <returns>Whether it is near enough to be placed</returns>
        public bool CanBePlacedOnAnchor(UxrGrabbableObjectAnchor anchor, out float distance)
        {
            if (anchor.enabled && anchor.gameObject.activeInHierarchy && anchor.CurrentPlacedObject == null && anchor.IsCompatibleObject(this))
            {
                distance = Vector3.Distance(DropProximityTransform.position, anchor.DropProximityTransform.position);

                if (distance <= anchor.MaxPlaceDistance)
                {
                    return true;
                }
            }

            distance = Mathf.Infinity;
            return false;
        }

        /// <summary>
        ///     Removes the object from the anchor it is placed on, if any.
        /// </summary>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void RemoveFromAnchor(bool propagateEvents)
        {
            UxrGrabManager.Instance.RemoveObjectFromAnchor(this, propagateEvents);
            _constraintTimer     = -1.0f;
            _constraintExitTimer = -1.0f;
            _placementTimer      = -1.0f;
        }

        /// <summary>
        ///     Places the object on the currently specified anchor by <see cref="CurrentAnchor" />.
        /// </summary>
        public void PlaceOnAnchor()
        {
            _placementTimer = -1.0f;
            transform.ApplyAlignment(DropAlignTransform.position,
                                     DropAlignTransform.rotation,
                                     CurrentAnchor.AlignTransform.position,
                                     CurrentAnchor.AlignTransform.rotation,
                                     GetSnapModeAffectsRotation(_dropSnapMode),
                                     GetSnapModeAffectsPosition(_dropSnapMode));
        }

        /// <summary>
        ///     Locks all hands that are currently grabbing the object if necessary. This is used to keep the grips in place after
        ///     the object has been moved or constrained.
        /// </summary>
        public void CheckAndApplyLockHands()
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                if (UxrGrabManager.Instance.GetGrabbingHands(this, i, out List<UxrGrabber> grabbers))
                {
                    foreach (UxrGrabber grabber in grabbers)
                    {
                        CheckAndApplyLockHand(grabber, i);
                    }
                }
            }
        }

        /// <summary>
        ///     Locks all hands that are currently grabbing the given object using a grab point if necessary. This is used to keep
        ///     the grips in place after the object has been moved or constrained.
        /// </summary>
        public void CheckAndApplyLockHand(UxrGrabber grabber, int grabPoint)
        {
            if (grabber.HandBone && grabPoint >= 0)
            {
                if (!GetGrabPoint(grabPoint).RuntimeGrabs.TryGetValue(grabber, out UxrRuntimeGripInfo grabInfo))
                {
                    return;
                }

                bool  lockHand     = false;
                bool  inTransition = false;
                float t            = 1.0f;

                if (grabInfo.LockHandInTransition)
                {
                    // Lock in transition
                    lockHand     = true;
                    inTransition = grabInfo.HandLockTimer > 0.0f;
                    t            = Mathf.Clamp01(1.0f - grabInfo.HandLockTimer / HandLockSeconds);
                }
                else
                {
                    // Lock hand to the object always while it is being grabbed. But only when
                    // the object is already in the hand and not during the transition
                    lockHand = grabInfo.GrabTimer < 0.0f;
                }

                if (lockHand)
                {
                    grabber.HandBone.ApplyAlignment(grabber.transform.position,
                                                    grabber.transform.rotation,
                                                    GetGrabbedPointGrabAlignPosition(grabber, grabPoint),
                                                    GetGrabbedPointGrabAlignRotation(grabber, grabPoint),
                                                    GetGrabPointSnapModeAffectsRotation(grabPoint),
                                                    false);

                    grabber.HandBone.ApplyAlignment(grabber.transform.position,
                                                    grabber.transform.rotation,
                                                    GetGrabbedPointGrabAlignPosition(grabber, grabPoint),
                                                    GetGrabbedPointGrabAlignRotation(grabber, grabPoint),
                                                    false,
                                                    GetGrabPointSnapModeAffectsPosition(grabPoint));

                    if (inTransition)
                    {
                        if (GetGrabPointSnapModeAffectsPosition(grabPoint))
                        {
                            grabber.HandBone.position = Vector3.Lerp(grabInfo.HandBonePositionOnGrab, grabber.HandBone.position, t);
                        }
                        if (GetGrabPointSnapModeAffectsRotation(grabPoint))
                        {
                            grabber.HandBone.rotation = Quaternion.Slerp(grabInfo.HandBoneRotationOnGrab, grabber.HandBone.rotation, t);
                        }
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Starts a smooth constrain transition.
        /// </summary>
        internal void StartSmoothConstrain()
        {
            _constraintTimer     = ConstrainSeconds;
            _constraintExitTimer = -1.0f;
        }

        /// <summary>
        ///     Exits the constrain smoothly to the current grab.
        /// </summary>
        internal void StartSmoothConstrainExit()
        {
            _constraintTimer     = -1.0f;
            _constraintExitTimer = ConstrainSeconds;
            _constraintExitPos   = transform.position;
            _constraintExitRot   = transform.rotation;
        }

        /// <summary>
        ///     Starts a smooth transition to the object placement.
        /// </summary>
        internal void StartSmoothAnchorPlacement()
        {
            _placementTimer = ObjectAlignmentSeconds;
        }

        /// <summary>
        ///     Checks whether the given grab point's snap mode affects the object position. This only references if the object
        ///     position is going to change in order to snap to the hand, not whether the object itself can be moved while grabbed.
        /// </summary>
        /// <param name="grabPoint">Grab point index</param>
        /// <returns>Whether the given grab point's snap mode will make the object position move to the snap position</returns>
        internal bool GetGrabPointSnapModeAffectsPosition(int grabPoint)
        {
            return GetSnapModeAffectsPosition(GetGrabPoint(grabPoint).SnapMode);
        }

        /// <summary>
        ///     Same as <see cref="GetGrabPointSnapModeAffectsPosition(int)" /> but also checks if the snap direction is in the
        ///     direction specified.
        /// </summary>
        /// <param name="grabPoint">Grab point index</param>
        /// <param name="snapDirection">Direction in which the snap should occur</param>
        /// <returns>
        ///     Whether the given grab point's snap mode will make the object position move to the snap position and in the
        ///     direction specified
        /// </returns>
        internal bool GetGrabPointSnapModeAffectsPosition(int grabPoint, UxrHandSnapDirection snapDirection)
        {
            return snapDirection == GetGrabPoint(grabPoint).SnapDirection && GetSnapModeAffectsPosition(GetGrabPoint(grabPoint).SnapMode);
        }

        /// <summary>
        ///     Checks whether the given grab point's snap mode affects the object rotation. This only references if the object
        ///     rotation is going to change in order to snap to the hand, not whether the object itself can be rotated while
        ///     grabbed.
        /// </summary>
        /// <param name="grabPoint">Grab point index</param>
        /// <returns>Whether the given grab point's snap mode will make the object rotate to the snap orientation</returns>
        internal bool GetGrabPointSnapModeAffectsRotation(int grabPoint)
        {
            return GetSnapModeAffectsRotation(GetGrabPoint(grabPoint).SnapMode);
        }

        /// <summary>
        ///     Same as <see cref="GetGrabPointSnapModeAffectsRotation(int)" /> but also checks if the snap direction is in the
        ///     direction specified.
        /// </summary>
        /// <param name="grabPoint">Grab point index</param>
        /// <param name="snapDirection">Direction in which the snap should occur</param>
        /// <returns>
        ///     Whether the given grab point's snap mode will make the object rotate to the snap orientation and in the
        ///     direction specified
        /// </returns>
        internal bool GetGrabPointSnapModeAffectsRotation(int grabPoint, UxrHandSnapDirection snapDirection)
        {
            return snapDirection == GetGrabPoint(grabPoint).SnapDirection && GetSnapModeAffectsRotation(GetGrabPoint(grabPoint).SnapMode);
        }

        /// <summary>
        ///     Gets the <see cref="UxrGrabPointShape" /> component for the given grab point.
        /// </summary>
        /// <param name="grabPoint">Grab point index to get the shape for</param>
        /// <returns><see cref="UxrGrabPointShape" /> component or null if there is none</returns>
        internal UxrGrabPointShape GetGrabPointShape(int grabPoint)
        {
            if (_grabPointShapes.TryGetValue(grabPoint, out UxrGrabPointShape shape))
            {
                return shape;
            }

            return null;
        }

        /// <summary>
        ///     Gets the snap position of a given grab point for a specific <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber to get the snap position for</param>
        /// <param name="grabPoint">Grab point to get the snap position of</param>
        /// <returns>Snap position</returns>
        internal Vector3 GetGrabbedPointGrabAlignPosition(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            return transform.TransformPoint(grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignPosition);
        }

        /// <summary>
        ///     Gets the snap orientation of a given grab point for a specific <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber to get the snap rotation for</param>
        /// <param name="grabPoint">Grab point to get the snap rotation of</param>
        /// <returns>Snap rotation</returns>
        internal Quaternion GetGrabbedPointGrabAlignRotation(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            return transform.rotation * grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignRotation;
        }

        /// <summary>
        ///     Gets the <see cref="Transform" /> component that is used to compute the distance to a <see cref="UxrGrabber" /> for
        ///     a given grab point.
        /// </summary>
        /// <param name="grabber">Grabber to get the distance from</param>
        /// <param name="grabPoint">Grab point to get the distance to</param>
        /// <returns><see cref="Transform" /> to compute the distance with</returns>
        internal Transform GetGrabPointGrabProximityTransform(UxrGrabber grabber, int grabPoint)
        {
            Transform        proximityTransform = null;
            UxrGrabPointInfo grabPointInfo      = GetGrabPoint(grabPoint);

            if (grabPointInfo.GrabProximityTransformUseSelf)
            {
                proximityTransform = grabber.Side == UxrHandSide.Left ? grabPointInfo.GetGripPoseInfo(grabber.Avatar).GripAlignTransformHandLeft : grabPointInfo.GetGripPoseInfo(grabber.Avatar).GripAlignTransformHandRight;
            }
            else
            {
                proximityTransform = grabPointInfo.GrabProximityTransform;
            }

            return proximityTransform != null ? proximityTransform : transform;
        }

        /// <summary>
        ///     Gets the position that is used to compute proximity from a <see cref="UxrGrabber" /> to a given grab point.
        /// </summary>
        /// <param name="grabber">Grabber to get the distance from</param>
        /// <param name="grabPoint">Grab point to get the distance to</param>
        /// <returns>Position to compute the proximity with</returns>
        internal Vector3 GetGrabbedPointGrabProximityPosition(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            return transform.TransformPoint(grabPointInfo.RuntimeGrabs[grabber].RelativeProximityPosition);
        }

        /// <summary>
        ///     Gets the relative rotation of the object to the grabber at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber to get the relative rotation to</param>
        /// <param name="grabPoint">Grab point that was grabbed</param>
        /// <returns>
        ///     Relative rotation of the object to the <see cref="UxrGrabber" /> at the time it was grabbed using the given
        ///     grab point
        /// </returns>
        internal Quaternion GetGrabPointRelativeGrabRotation(UxrGrabber grabber, int grabPoint)
        {
            return GetGrabPoint(grabPoint).RuntimeGrabs[grabber].RelativeGrabRotation;
        }

        /// <summary>
        ///     Gets the relative position of the object to the grabber at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber to get the relative position to</param>
        /// <param name="grabPoint">Grab point that was grabbed</param>
        /// <returns>
        ///     Relative position of the object to the <see cref="UxrGrabber" /> at the time it was grabbed using the given
        ///     grab point
        /// </returns>
        internal Vector3 GetGrabPointRelativeGrabPosition(UxrGrabber grabber, int grabPoint)
        {
            return GetGrabPoint(grabPoint).RuntimeGrabs[grabber].RelativeGrabPosition;
        }

        /// <summary>
        ///     Gets the relative grabber position to the object at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber to get the relative position of</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        /// <returns>Relative grabber position to the object at the time it was grabbed</returns>
        internal Vector3 GetGrabPointRelativeGrabberPosition(UxrGrabber grabber, int grabPoint)
        {
            return GetGrabPoint(grabPoint).RuntimeGrabs[grabber].RelativeGrabberPosition;
        }

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is used to compute the snap to an <see cref="UxrAvatar" />'s hand (
        ///     <see cref="UxrGrabber" />) for a given grab point when it is grabbed.
        /// </summary>
        /// <param name="avatar">Avatar to compute the alignment for</param>
        /// <param name="grabPoint">Grab point to get the alignment of</param>
        /// <param name="handSide">The hand to get the snap transform for</param>
        /// <returns><see cref="Transform" /> that should align to the grabber when it is being grabbed</returns>
        internal Transform GetGrabPointGrabAlignTransform(UxrAvatar avatar, int grabPoint, UxrHandSide handSide)
        {
            UxrGripPoseInfo gripPoseInfo = GetGrabPoint(grabPoint).GetGripPoseInfo(avatar);

            if (handSide == UxrHandSide.Left)
            {
                if (gripPoseInfo.GripAlignTransformHandLeft == null || GetGrabPoint(grabPoint).SnapReference != UxrSnapReference.UseOtherTransform)
                {
                    return transform;
                }

                return gripPoseInfo.GripAlignTransformHandLeft;
            }

            if (gripPoseInfo.GripAlignTransformHandRight == null || GetGrabPoint(grabPoint).SnapReference != UxrSnapReference.UseOtherTransform)
            {
                return transform;
            }

            return gripPoseInfo.GripAlignTransformHandRight;
        }

        /// <summary>
        ///     Gets the additional rotation required to apply to a grabber in order to align it with the controller.
        /// </summary>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPoint">Grab point</param>
        /// <returns>Additional rotation to apply to the object using the current grabber to align it with the controller</returns>
        internal Quaternion GetGrabberControllerAlignmentRotation(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            Transform        snapTransform = GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side);

            if (snapTransform == null || grabPointInfo == null || !grabPointInfo.AlignToController)
            {
                return Quaternion.identity;
            }

            UxrController3DModel controller3DModel = grabber != null && grabber.Avatar != null ? grabber.Avatar.ControllerInput.GetController3DModel(grabber.Side) : null;

            if (controller3DModel != null)
            {
                Quaternion relativeTrackerRotation = Quaternion.Inverse(grabber.transform.rotation) * controller3DModel.ForwardTrackingRotation;
                Quaternion trackerRotation         = grabber.transform.rotation * relativeTrackerRotation;
                Quaternion sourceAlignAxes         = grabPointInfo.AlignToControllerAxes != null ? grabPointInfo.AlignToControllerAxes.rotation : transform.rotation;
                Quaternion grabberTargetRotation   = sourceAlignAxes * Quaternion.Inverse(trackerRotation) * grabber.transform.rotation;

                return Quaternion.Inverse(grabber.transform.rotation) * grabberTargetRotation;
            }

            return Quaternion.identity;
        }

        /// <summary>
        ///     Computes internal transform data at the time of a grab.
        /// </summary>
        /// <param name="grabber">Grabber that grabbed the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        internal void ComputeGrabTransforms(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            if (grabPointInfo != null && grabPointInfo.RuntimeGrabs.TryGetValue(grabber, out UxrRuntimeGripInfo grabInfo))
            {
                grabInfo.RelativeGrabRotation    = Quaternion.Inverse(grabber.transform.rotation) * transform.rotation;
                grabInfo.RelativeGrabPosition    = grabber.transform.InverseTransformPoint(transform.position);
                grabInfo.RelativeGrabberRotation = Quaternion.Inverse(transform.rotation) * grabber.transform.rotation;
                grabInfo.RelativeGrabberPosition = transform.InverseTransformPoint(grabber.transform.position);
            }
        }

        /// <summary>
        ///     Applies, if needed, the different constraints to the grabbable object.
        /// </summary>
        /// <param name="grabber">Grabber that is currently grabbing the object</param>
        /// <param name="grabPoint">Point that is being grabbed</param>
        /// <param name="positionBeforeUpdate">Object position before being updated</param>
        /// <param name="rotationBeforeUpdate">Object rotation before being updated</param>
        /// <param name="propagateEvents">
        ///     Whether to propagate <see cref="ConstraintsApplying" /> and
        ///     <see cref="ConstraintsApplied" /> events
        /// </param>
        internal void CheckAndApplyConstraints(UxrGrabber grabber, int grabPoint, Vector3 positionBeforeUpdate, Quaternion rotationBeforeUpdate, bool propagateEvents)
        {
            if (propagateEvents)
            {
                OnConstraintsApplying(new UxrApplyConstraintsEventArgs(this));
            }

            if (IsLockedInPlace)
            {
                transform.position = positionBeforeUpdate;
                transform.rotation = rotationBeforeUpdate;
                return;
            }

            if (_manipulationMode == UxrManipulationMode.GrabAndMove)
            {
                // Alignment

                if (grabPoint >= 0 && UsesGrabbableParentDependency == false)
                {
                    UxrGrabPointInfo   grabPointInfo = GetGrabPoint(grabPoint);
                    UxrRuntimeGripInfo gripInfo      = grabPointInfo.RuntimeGrabs[grabber];

                    Quaternion sourceRotation = transform.rotation * gripInfo.RelativeGrabAlignRotation;
                    Quaternion targetRotation = grabber.transform.rotation;

                    if (grabPointInfo.AlignToController)
                    {
                        // Align the object to the controller. Useful for weapons or things that need directional precision.

                        sourceRotation = grabPointInfo.AlignToControllerAxes != null ? grabPointInfo.AlignToControllerAxes.rotation : transform.rotation;

                        UxrController3DModel controller3DModel = grabber != null && grabber.Avatar != null ? grabber.Avatar.ControllerInput.GetController3DModel(grabber.Side) : null;

                        if (controller3DModel != null)
                        {
                            targetRotation = controller3DModel.ForwardTrackingRotation;
                        }
                    }

                    transform.ApplyAlignment(transform.TransformPoint(gripInfo.RelativeGrabAlignPosition),
                                             sourceRotation,
                                             grabber.transform.position,
                                             targetRotation,
                                             GetGrabPointSnapModeAffectsRotation(grabPoint, UxrHandSnapDirection.ObjectToHand),
                                             false);
                    ConstrainTransform(false, true, positionBeforeUpdate, rotationBeforeUpdate); // Constrain rotation before snapping pivot
                    transform.ApplyAlignment(transform.TransformPoint(gripInfo.RelativeGrabAlignPosition),
                                             sourceRotation,
                                             grabber.transform.position,
                                             targetRotation,
                                             false,
                                             GetGrabPointSnapModeAffectsPosition(grabPoint, UxrHandSnapDirection.ObjectToHand));

                    if (gripInfo.GrabTimer > 0.0f)
                    {
                        // Smooth transition to the hand
                        float t = 1.0f - Mathf.Clamp01(gripInfo.GrabTimer / ObjectAlignmentSeconds);
                        transform.localPosition = Vector3.Lerp(gripInfo.LocalPositionOnGrab, transform.localPosition, t);
                        transform.localRotation = Quaternion.Slerp(gripInfo.LocalRotationOnGrab, transform.localRotation, t);
                    }
                }

                // Translation & Rotation

                ConstrainTransform(true, true, positionBeforeUpdate, rotationBeforeUpdate);

                // Smoothly exit from a constraint if we had manually created the transition using StartSmoothConstraintExit()

                if (_constraintExitTimer > 0.0f)
                {
                    float constraintExitT = 1.0f - _constraintExitTimer / ConstrainSeconds;
                    transform.position = Vector3.Lerp(_constraintExitPos, transform.position, constraintExitT);
                    transform.rotation = Quaternion.Lerp(_constraintExitRot, transform.rotation, constraintExitT);
                }
            }
            else if (_manipulationMode == UxrManipulationMode.RotateAroundAxis && grabPoint >= 0)
            {
                UxrGrabPointInfo   grabPointInfo = GetGrabPoint(grabPoint);
                UxrRuntimeGripInfo gripInfo      = grabPointInfo.RuntimeGrabs[grabber];

                // Position

                transform.position = grabber.transform.TransformPoint(GetGrabPointRelativeGrabPosition(grabber, grabPoint));
                ConstrainTransform(true, false, positionBeforeUpdate, rotationBeforeUpdate);

                // Rotation: We use the angle between V1(pivot, initial grab position) and V2 (pivot, current grab position).
                //           This method works better for levers, steering wheels, etc. It won't work well with elements
                //           like knobs or similar because the point where the torque is applied lies in the rotation axis itself.
                //           In this cases we recommend using ManipulationMode.GrabAndMove instead.

                Vector3    grabDirection        = grabber.transform.position - transform.position;
                Vector3    initialGrabDirection = grabber.Avatar.transform.TransformPoint(gripInfo.GrabberLocalAvatarPositionOnGrab) - transform.position;
                Quaternion rotationOnGrab       = transform.parent != null ? transform.parent.rotation * gripInfo.LocalRotationOnGrab : gripInfo.LocalRotationOnGrab;

                Quaternion rotation              = Quaternion.FromToRotation(initialGrabDirection.normalized, grabDirection.normalized) * rotationOnGrab;
                Quaternion inverseParentRotation = Quaternion.Inverse(RotationLimitsParentRotation);
                Quaternion localRotation         = inverseParentRotation * rotation;
                Quaternion localRotationPrevious = inverseParentRotation * rotationBeforeUpdate;
                Quaternion clampedLocalRotation  = ClampRotation(localRotation, localRotationPrevious, Quaternion.Euler(InitialLocalEulerAnglesToReference), _rotationAngleLimitsMin, _rotationAngleLimitsMax);
                transform.rotation = RotationLimitsParentRotation * clampedLocalRotation;
            }

            if (UxrGrabManager.Instance.GetHandsGrabbingCount(this) == 1)
            {
                transform.position = UxrInterpolator.SmoothDampPosition(positionBeforeUpdate, transform.position, _translationResistance);
                transform.rotation = UxrInterpolator.SmoothDampRotation(rotationBeforeUpdate, transform.rotation, _rotationResistance);
            }

            if (propagateEvents)
            {
                OnConstraintsApplied(new UxrApplyConstraintsEventArgs(this));
            }
        }

        /// <summary>
        ///     Notifies that the object just started to be grabbed.
        /// </summary>
        /// <param name="grabber">Grabber responsible for grabbing the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        internal void NotifyBeginGrab(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo      = GetGrabPoint(grabPoint);
            Transform        grabAlignTransform = GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side);

            if (grabPointInfo.RuntimeGrabs == null)
            {
                grabPointInfo.RuntimeGrabs = new Dictionary<UxrGrabber, UxrRuntimeGripInfo>();
            }

            if (grabPointInfo.RuntimeGrabs.TryGetValue(grabber, out UxrRuntimeGripInfo grabInfo) == false)
            {
                grabInfo = new UxrRuntimeGripInfo();
                grabPointInfo.RuntimeGrabs.Add(grabber, grabInfo);
            }

            Vector3    snapPosition = grabAlignTransform.position;
            Quaternion snapRotation = grabAlignTransform.rotation;

            UxrGrabPointShape grabPointShape = GetGrabPointShape(grabPoint);

            if (grabPointShape != null)
            {
                grabPointShape.GetClosestSnap(grabber, grabAlignTransform, GetGrabPointGrabProximityTransform(grabber, grabPoint), out snapPosition, out snapRotation);
            }

            Matrix4x4 snapMatrix     = Matrix4x4.TRS(snapPosition, snapRotation, grabAlignTransform.lossyScale);
            Vector3   localProximity = grabAlignTransform.InverseTransformPoint(GetGrabPointGrabProximityTransform(grabber, grabPoint).position);

            ComputeGrabTransforms(grabber, grabPoint);

            grabInfo.RelativeGrabAlignRotation = Quaternion.Inverse(transform.rotation) * snapRotation;
            grabInfo.RelativeGrabAlignPosition = transform.InverseTransformPoint(snapPosition);
            grabInfo.RelativeProximityPosition = transform.InverseTransformPoint(snapMatrix.MultiplyPoint(localProximity));

            grabInfo.LocalPositionOnGrab              = transform.localPosition;
            grabInfo.LocalRotationOnGrab              = transform.localRotation;
            grabInfo.AlignPositionOnGrab              = snapPosition;
            grabInfo.AlignRotationOnGrab              = snapRotation;
            grabInfo.GrabberLocalAvatarPositionOnGrab = grabber.Avatar.transform.InverseTransformPoint(grabber.transform.position);
            grabInfo.GrabberLocalAvatarRotationOnGrab = Quaternion.Inverse(grabber.Avatar.transform.rotation) * grabber.transform.rotation;
            grabInfo.HandBonePositionOnGrab           = grabber.HandBone.position;
            grabInfo.HandBoneRotationOnGrab           = grabber.HandBone.rotation;
            grabInfo.GrabTimer                        = ObjectAlignmentSeconds;

            if (IsConstrained || UxrGrabManager.Instance.IsBeingGrabbedByOtherThan(this, grabPoint, grabber))
            {
                // Smoothly lock the hand to the grab point
                grabInfo.HandLockTimer        = HandLockSeconds;
                grabInfo.LockHandInTransition = true;
            }
            else
            {
                // Do not lock the hand to the grab point while in transition, only when the object is already in the hand
                grabInfo.LockHandInTransition = false;
            }
        }

        /// <summary>
        ///     Notifies that the object just stopped being grabbed.
        /// </summary>
        /// <param name="grabber">Grabber responsible for grabbing the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        internal void NotifyEndGrab(UxrGrabber grabber, int grabPoint)
        {
            // Don't remove from grabPointInfo.RuntimeGrabs because we may have transitions that need the GrabInfo

            /*
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            grabPointInfo.RuntimeGrabs.Remove(grabber);
            */
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the object and creates the <see cref="UxrGrabbableObjectAnchor" /> the object is placed on initially if
        ///     <see cref="_autoCreateStartAnchor" /> is set.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrGrabManager.Instance.Poke();

            if (_autoCreateStartAnchor)
            {
                UxrGrabbableObjectAnchor newAnchor = new GameObject($"{name} Auto Anchor", typeof(UxrGrabbableObjectAnchor)).GetComponent<UxrGrabbableObjectAnchor>();
                newAnchor.transform.SetPositionAndRotation(DropAlignTransform);
                newAnchor.transform.SetParent(transform.parent);
                transform.SetParent(newAnchor.transform);

                if (!string.IsNullOrEmpty(Tag))
                {
                    newAnchor.AddCompatibleTags(Tag);
                }

                _startAnchor = newAnchor;
            }

            CurrentAnchor = _startAnchor;

            if (CurrentAnchor != null)
            {
                CurrentAnchor.CurrentPlacedObject = this;

                if (_rigidBodySource)
                {
                    _rigidBodySource.isKinematic = true;
                }
            }

            InitialLocalPositionToReference    = TranslationLimitsParentMatrix.inverse.MultiplyPoint(transform.position);
            InitialLocalEulerAnglesToReference = (Quaternion.Inverse(RotationLimitsParentRotation) * transform.rotation).eulerAngles;
            _initialIsKinematic                = IsKinematic;

            _grabPointShapes = new Dictionary<int, UxrGrabPointShape>();
            UxrGrabPointShape[] grabPointShapes = GetComponents<UxrGrabPointShape>();

            foreach (UxrGrabPointShape grabPointShape in grabPointShapes)
            {
                if (_grabPointShapes.ContainsKey(grabPointShape.GrabPoint))
                {
                    Debug.LogWarning("Object " + name + " has duplicated GrabPointShape for " + UxrGrabPointIndex.GetIndexDisplayName(this, grabPointShape.GrabPoint));
                }
                else
                {
                    _grabPointShapes.Add(grabPointShape.GrabPoint, grabPointShape);
                }
            }
        }

        /// <summary>
        ///     Resets the component.
        /// </summary>
        private void Reset()
        {
            _grabPoint = new UxrGrabPointInfo();
        }

        /// <summary>
        ///     Performs additional initialization.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                if (grabPointInfo.RuntimeGrabs != null)
                {
                    foreach (KeyValuePair<UxrGrabber, UxrRuntimeGripInfo> grabPairInfo in grabPointInfo.RuntimeGrabs)
                    {
                        grabPairInfo.Value.GrabTimer = -1.0f;
                    }
                }
            }

            _placementTimer      = -1.0f;
            _constraintTimer     = -1.0f;
            _constraintExitTimer = -1.0f;
        }

        /// <summary>
        ///     Updates some of the transitions made to have smooth object interaction.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                if (grabPointInfo.RuntimeGrabs != null)
                {
                    foreach (KeyValuePair<UxrGrabber, UxrRuntimeGripInfo> grabPairInfo in grabPointInfo.RuntimeGrabs)
                    {
                        if (grabPairInfo.Value.GrabTimer > 0.0f)
                        {
                            grabPairInfo.Value.GrabTimer -= Time.unscaledDeltaTime;
                        }

                        if (grabPairInfo.Value.HandLockTimer > 0.0f)
                        {
                            grabPairInfo.Value.HandLockTimer -= Time.unscaledDeltaTime;
                        }
                    }
                }
            }

            if (_placementTimer > 0.0f && CurrentAnchor)
            {
                _placementTimer -= Time.unscaledDeltaTime;

                transform.ApplyAlignment(DropAlignTransform.position,
                                         DropAlignTransform.rotation,
                                         CurrentAnchor.AlignTransform.position,
                                         CurrentAnchor.AlignTransform.rotation,
                                         GetSnapModeAffectsRotation(_dropSnapMode),
                                         GetSnapModeAffectsPosition(_dropSnapMode),
                                         1.0f - Mathf.Clamp01(_placementTimer / ObjectAlignmentSeconds));

                if (_placementTimer <= 0.0f)
                {
                    CurrentAnchor.RaiseSmoothTransitionPlaceEnded();
                }
            }

            if (_constraintTimer > 0.0f)
            {
                _constraintTimer -= Time.unscaledDeltaTime;

                if (UxrGrabManager.Instance.IsBeingGrabbed(this) == false)
                {
                    // This object was released into the constrained zone before the full transition finished. Finish manually.
                    ConstrainTransform(true, true, transform.position, transform.rotation);
                }
            }

            if (_constraintExitTimer > 0.0f)
            {
                _constraintExitTimer -= Time.unscaledDeltaTime;
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="ConstraintsApplying" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnConstraintsApplying(UxrApplyConstraintsEventArgs e)
        {
            ConstraintsApplying?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="ConstraintsApplied" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnConstraintsApplied(UxrApplyConstraintsEventArgs e)
        {
            ConstraintsApplied?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Grabbing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseGrabbingEvent(UxrManipulationEventArgs e)
        {
            Grabbing?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Grabbed" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseGrabbedEvent(UxrManipulationEventArgs e)
        {
            Grabbed?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Releasing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseReleasingEvent(UxrManipulationEventArgs e)
        {
            Releasing?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Released" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseReleasedEvent(UxrManipulationEventArgs e)
        {
            Released?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Placing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaisePlacingEvent(UxrManipulationEventArgs e)
        {
            Placing?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Placed" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaisePlacedEvent(UxrManipulationEventArgs e)
        {
            Placed?.Invoke(this, e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the given snap mode affects the object position. This only references if the object position is
        ///     going to change in order to snap to the hand, not whether the object itself can be moved while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object position move to the snap position</returns>
        private static bool GetSnapModeAffectsPosition(UxrSnapToHandMode snapMode)
        {
            return snapMode != UxrSnapToHandMode.DontSnap && snapMode != UxrSnapToHandMode.RotationOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object position. This only references if the object position is
        ///     going to change in order to snap to the hand, not whether the object itself can be moved while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object position move to the snap position</returns>
        private static bool GetSnapModeAffectsPosition(UxrSnapToAnchorMode snapMode)
        {
            return snapMode != UxrSnapToAnchorMode.DontSnap && snapMode != UxrSnapToAnchorMode.RotationOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object rotation. This only references if the object rotation is
        ///     going to change in order to snap to the hand, not whether the object itself can be rotated while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object rotate to the snap orientation</returns>
        private static bool GetSnapModeAffectsRotation(UxrSnapToHandMode snapMode)
        {
            return snapMode != UxrSnapToHandMode.DontSnap && snapMode != UxrSnapToHandMode.PositionOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object rotation. This only references if the object rotation is
        ///     going to change in order to snap to the hand, not whether the object itself can be rotated while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object rotate to the snap orientation</returns>
        private static bool GetSnapModeAffectsRotation(UxrSnapToAnchorMode snapMode)
        {
            return snapMode != UxrSnapToAnchorMode.DontSnap && snapMode != UxrSnapToAnchorMode.PositionOnly;
        }

        /// <summary>
        ///     Gets the <see cref="Transform" /> of a <see cref="UxrGrabbableObject" /> upwards in the hierarchy. This parent has
        ///     the potential to be a dependency of the child grabbable object, meaning that the parent constraints the child.
        /// </summary>
        /// <param name="grabbableTransform">Transform of the <see cref="UxrGrabbableObject" /> to get the potential dependency of</param>
        /// <returns>Transform of the <see cref="UxrGrabbableObject" /> upwards in the hierarchy</returns>
        private Transform GetGrabbableParentDependency(Transform grabbableTransform)
        {
            if (grabbableTransform.parent != null)
            {
                UxrGrabbableObject parentGrabbableObject = grabbableTransform.parent.GetComponentInParent<UxrGrabbableObject>();

                if (parentGrabbableObject != null)
                {
                    return parentGrabbableObject.transform;
                }
            }

            return null;
        }

        /// <summary>
        ///     Applies constraints to the object.
        /// </summary>
        /// <param name="processPosition">Whether to apply the position constraints if any</param>
        /// <param name="processRotation">Whether to apply the rotation constraints if any</param>
        /// <param name="lastPosition">The object position on the last frame</param>
        /// <param name="lastRotation">The object rotation on the last frame</param>
        private void ConstrainTransform(bool processPosition, bool processRotation, Vector3 lastPosition, Quaternion lastRotation)
        {
            // Rotation

            if (_rotationConstraintMode == UxrRotationConstraintMode.RestrictLocalRotation && processRotation)
            {
                Quaternion inverseParentRot  = Quaternion.Inverse(RotationLimitsParentRotation);
                Quaternion localQuat         = inverseParentRot * transform.rotation;
                Quaternion localQuatPrevious = inverseParentRot * lastRotation;
                Quaternion clampedQuat       = ClampRotation(localQuat, localQuatPrevious, Quaternion.Euler(InitialLocalEulerAnglesToReference), _rotationAngleLimitsMin, _rotationAngleLimitsMax);
                Quaternion targetQuat        = RotationLimitsParentRotation * clampedQuat;

                transform.rotation = _constraintTimer < 0.0f ? targetQuat : Quaternion.Slerp(transform.rotation, targetQuat, 1.0f - _constraintTimer / ConstrainSeconds);
            }

            // Translation

            if (processPosition)
            {
                if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictToBox && _restrictToBox != null)
                {
                    Vector3 targetPos = transform.position.ClampToBox(_restrictToBox);
                    transform.position = _constraintTimer < 0.0f ? targetPos : Vector3.Lerp(transform.position, targetPos, 1.0f - _constraintTimer / ConstrainSeconds);
                }
                else if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictToSphere && _restrictToSphere != null)
                {
                    Vector3 targetPos = transform.position.ClampToSphere(_restrictToSphere);
                    transform.position = _constraintTimer < 0.0f ? targetPos : Vector3.Lerp(transform.position, targetPos, 1.0f - _constraintTimer / ConstrainSeconds);
                }
                else if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictLocalOffset || Manipulation == UxrManipulationMode.RotateAroundAxis)
                {
                    Vector3 localPos  = TranslationLimitsParentMatrix.inverse.MultiplyPoint(transform.position);
                    Vector3 targetPos = TranslationLimitsParentMatrix.MultiplyPoint(localPos.Clamp(InitialLocalPositionToReference + _translationLimitsMin, InitialLocalPositionToReference + _translationLimitsMax));
                    transform.position = _constraintTimer < 0.0f ? targetPos : Vector3.Lerp(transform.position, targetPos, 1.0f - _constraintTimer / ConstrainSeconds);
                }
            }
        }

        /// <summary>
        ///     Tries to clamp a rotation.
        /// </summary>
        /// <param name="rot">Rotation to clamp</param>
        /// <param name="previousRot">Rotation the previous frame</param>
        /// <param name="initialRot">Initial rotation</param>
        /// <param name="eulerMin">Minimum euler values</param>
        /// <param name="eulerMax">Maximum euler values</param>
        /// <returns></returns>
        private Quaternion ClampRotation(Quaternion rot, Quaternion previousRot, Quaternion initialRot, Vector3 eulerMin, Vector3 eulerMax)
        {
            Vector3    axis;
            Vector3    currentAxis;
            Quaternion quatMin;
            Quaternion quatMax;
            float      angleMin;
            float      angleMax;

            int axisIndex = 0;
            int axisCount = 0;

            // First pass: Check for components that have a fixed value. Sometimes x, y or z needs to be -90 for instance because of the object axis system

            for (axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                if (Mathf.Approximately(eulerMin[axisIndex], eulerMax[axisIndex]) && !Mathf.Approximately(eulerMin[axisIndex], 0.0f))
                {
                    Vector3 fixedEuler = rot.eulerAngles;
                    fixedEuler[axisIndex] = eulerMin[axisIndex];
                    rot                   = Quaternion.Euler(fixedEuler);
                }
                else if(!Mathf.Approximately(eulerMin[axisIndex], eulerMax[axisIndex]))
                {
                    axisCount++;
                }
            }

            // If we have more than one axis constraint we probably have something like a socket-ball joint. We solve this easier.
            
            if (axisCount > 1)
            {
                Vector3 initialEuler = initialRot.eulerAngles;
                return Quaternion.Euler(rot.eulerAngles.ToEuler180().Clamp(initialEuler + eulerMin, initialEuler + eulerMax));
            }

            // Second pass: Try to fix around axis that has a range of degrees

            if ((!Mathf.Approximately(eulerMin.x, 0.0f) || !Mathf.Approximately(eulerMax.x, 0.0f)) && !Mathf.Approximately(eulerMin.x, eulerMax.x))
            {
                axisIndex   = 0;
                axis        = initialRot * Vector3.right;
                currentAxis = rot * Vector3.right;
                angleMin    = eulerMin.x;
                angleMax    = eulerMax.x;
            }
            else if ((!Mathf.Approximately(eulerMin.y, 0.0f) || !Mathf.Approximately(eulerMax.y, 0.0f)) && !Mathf.Approximately(eulerMin.y, eulerMax.y))
            {
                axisIndex   = 1;
                axis        = initialRot * Vector3.up;
                currentAxis = rot * Vector3.up;
                angleMin    = eulerMin.y;
                angleMax    = eulerMax.y;
            }
            else if ((!Mathf.Approximately(eulerMin.z, 0.0f) || !Mathf.Approximately(eulerMax.z, 0.0f)) && !Mathf.Approximately(eulerMin.z, eulerMax.z))
            {
                axisIndex   = 2;
                axis        = initialRot * Vector3.forward;
                currentAxis = rot * Vector3.forward;
                angleMin    = eulerMin.z;
                angleMax    = eulerMax.z;
            }
            else
            {
                return initialRot;
            }

            quatMin = Quaternion.AngleAxis(angleMin, axis) * initialRot;
            quatMax = Quaternion.AngleAxis(angleMax, axis) * initialRot;

            // First fix rotation so that it is constrained to the correct plane

            Quaternion fixedQuat = Quaternion.FromToRotation(currentAxis, axis) * rot;

            // Now make sure it is clamped between the given angles

            float currentAngle = 0.0f;

            if (axisIndex == 0)
            {
                currentAngle = Vector3.SignedAngle(initialRot * Vector3.forward, fixedQuat * Vector3.forward, axis);
            }
            else if (axisIndex == 1)
            {
                currentAngle = Vector3.SignedAngle(initialRot * Vector3.right, fixedQuat * Vector3.right, axis);
            }
            else if (axisIndex == 2)
            {
                currentAngle = Vector3.SignedAngle(initialRot * Vector3.up, fixedQuat * Vector3.up, axis);
            }

            bool clampAngle = false;

            if (angleMin < -180.0f)
            {
                if (angleMax - angleMin < 360.0f)
                {
                    if (currentAngle > angleMax && currentAngle < angleMin + 360.0f)
                    {
                        clampAngle = true;
                    }
                }
            }
            else if (angleMax > 180.0f)
            {
                if (angleMax - angleMin < 360.0f)
                {
                    if (currentAngle < angleMin && currentAngle > angleMax - 360.0f)
                    {
                        clampAngle = true;
                    }
                }
            }
            else if (currentAngle < angleMin)
            {
                if (angleMax - angleMin > 240.0f)
                {
                    // This way of clamping will behave nicer if the valid arc is big
                    clampAngle = true;
                }
                else
                {
                    return quatMin;
                }
            }
            else if (currentAngle > angleMax)
            {
                if (angleMax - angleMin > 240.0f)
                {
                    // This way of clamping will behave nicer if the valid arc is big
                    clampAngle = true;
                }
                else
                {
                    return quatMax;
                }
            }

            if (clampAngle)
            {
                // The current angle is inside the invalid arc. Instead of clamping the current angle
                // between (min, max) we will clamp the last value to the closest between (min, max).
                // This should be more correct because the last value should always be a correctly
                // clamped angle, and would avoid traversing the invalid arc or switching between
                // min and max due to overshooting.

                float angleFromPreviousToMin = Quaternion.Angle(previousRot, quatMin);
                float angleFromPreviousToMax = Quaternion.Angle(previousRot, quatMax);

                return angleFromPreviousToMin < angleFromPreviousToMax ? quatMin : quatMax;
            }

            return fixedQuat;
        }

        #endregion

        #region Private Types & Data

        // Private properties

        /// <summary>
        ///     Gets the parent matrix where the min/max translation constraints are applied.
        /// </summary>
        private Matrix4x4 TranslationLimitsParentMatrix
        {
            get
            {
                if (!_translationLimitsReferenceIsParent && _translationLimitsParent)
                {
                    return _translationLimitsParent.localToWorldMatrix;
                }

                return transform.parent != null ? transform.parent.localToWorldMatrix : Matrix4x4.identity;
            }
        }

        /// <summary>
        ///     Gets the parent matrix where the min/max rotation constraints are applied.
        /// </summary>
        private Quaternion RotationLimitsParentRotation
        {
            get
            {
                if (!_rotationLimitsReferenceIsParent && _rotationLimitsParent)
                {
                    return _rotationLimitsParent.rotation;
                }

                return transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            }
        }

        /// <summary>
        ///     Minimum distance allowed between two grabbable points that can be grabbed at the same time. Avoid hand overlapping.
        /// </summary>
        private const float MinHandGrabInterDistance = 0.05f;

        private readonly Dictionary<int, bool> _grabPointEnabledStates = new Dictionary<int, bool>();

        // Private vars

        private bool                               _initialIsKinematic    = true;
        private float                              _placementTimer        = -1.0f;
        private float                              _constraintTimer       = -1.0f;
        private float                              _constraintExitTimer   = -1.0f;
        private Vector3                            _constraintExitPos     = Vector3.zero;
        private Quaternion                         _constraintExitRot     = Quaternion.identity;
        private Dictionary<int, UxrGrabPointShape> _grabPointShapes       = new Dictionary<int, UxrGrabPointShape>();

        #endregion

#if UNITY_EDITOR

        /// <summary>
        ///     Registers a new avatar to have grips on this object. If the avatar is an instance it will register its source
        ///     prefab, otherwise it will register the avatar prefab itself. The reason to register the prefab is so that child
        ///     prefabs/instances will be able to use the same poses.
        /// </summary>
        /// <param name="avatar">Avatar to register</param>
        public void Editor_RegisterAvatarForGrips(UxrAvatar avatar)
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                GetGrabPoint(i).CheckAddGripPoseInfo(avatar.PrefabGuid);
            }
        }

        /// <summary>
        ///     Removes an avatar that was registered for grip poses. If the avatar is an instance it will unregister its source
        ///     prefab, otherwise it will unregister the avatar prefab itself.
        /// </summary>
        /// <param name="avatar">The avatar to remove</param>
        public void Editor_RemoveAvatarFromGrips(UxrAvatar avatar)
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);
                grabPointInfo.RemoveGripPoseInfo(avatar.PrefabGuid);
            }
        }

        /// <summary>
        ///     Gets the editor currently selected avatar prefab GUID whose grip parameters are being edited.
        /// </summary>
        /// <returns>Avatar prefab GUID or null if there isn't any avatar prefab selected</returns>
        public string Editor_GetSelectedAvatarPrefabGuidForGrips()
        {
            if (_selectedAvatarForGrips)
            {
                UxrAvatar avatar = _selectedAvatarForGrips.GetComponent<UxrAvatar>();

                if (avatar)
                {
                    return avatar.PrefabGuid;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the editor currently selected avatar prefab whose grip parameters are being edited.
        /// </summary>
        /// <returns>Avatar prefab</returns>
        public GameObject Editor_GetSelectedAvatarPrefabForGrips()
        {
            return _selectedAvatarForGrips;
        }

        /// <summary>
        ///     Sets the editor currently selected avatar prefab whose grip parameters will be edited.
        /// </summary>
        public void Editor_SetSelectedAvatarForGrips(GameObject avatarPrefab)
        {
            _selectedAvatarForGrips = avatarPrefab;
        }

        /// <summary>
        ///     Gets the avatar prefab GUIDs that have been registered to have dedicated grip parameters to grab the object.
        /// </summary>
        /// <returns>Registered avatar prefab GUIDs</returns>
        public List<string> Editor_GetRegisteredAvatarsGuids()
        {
            List<string> registeredAvatars = new List<string>();

            // All grab points have the same amount of avatar grip entries. Use default grab point.

            UxrGrabPointInfo grabPointInfo = GetGrabPoint(0);

            foreach (UxrGripPoseInfo poseInfo in grabPointInfo.AvatarGripPoseEntries)
            {
                if (!registeredAvatars.Contains(poseInfo.AvatarPrefabGuid))
                {
                    registeredAvatars.Add(poseInfo.AvatarPrefabGuid);
                }
            }

            return registeredAvatars;
        }

        /// <summary>
        ///     If we add new grab points from the editor, we want to make sure that all avatars are registered in those new
        ///     entries.
        ///     This method makes sure that the avatars are registered in all grab points.
        /// </summary>
        public void Editor_CheckRegisterAvatarsInNewGrabPoints()
        {
            List<string> registeredAvatars = new List<string>();

            // Look for all registered avatars

            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                foreach (UxrGripPoseInfo gripPoseInfo in grabPointInfo.AvatarGripPoseEntries)
                {
                    if (!registeredAvatars.Contains(gripPoseInfo.AvatarPrefabGuid))
                    {
                        registeredAvatars.Add(gripPoseInfo.AvatarPrefabGuid);
                    }
                }
            }

            // Fill missing avatars in grab points

            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                foreach (string prefabGuid in registeredAvatars)
                {
                    grabPointInfo.CheckAddGripPoseInfo(prefabGuid);
                }
            }
        }

        /// <summary>
        ///     Gets the prefab avatar that is used for the given grip snap transform. If there is more than one the first is
        ///     returned.
        /// </summary>
        /// <param name="snapTransform">The snap transform to get the avatar for</param>
        /// <returns>Avatar prefab GUID</returns>
        public string Editor_GetGrabAlignTransformAvatar(Transform snapTransform)
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                if (grabPointInfo.SnapMode != UxrSnapToHandMode.DontSnap)
                {
                    foreach (UxrGripPoseInfo gripPose in grabPointInfo.AvatarGripPoseEntries)
                    {
                        if (gripPose.GripAlignTransformHandLeft == snapTransform || gripPose.GripAlignTransformHandRight == snapTransform)
                        {
                            return gripPose.AvatarPrefabGuid;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets all the transforms used for alignment with the registered avatars.
        /// </summary>
        /// <returns>List of align transforms</returns>
        public IEnumerable<Transform> Editor_GetAllRegisteredAvatarGrabAlignTransforms()
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                if (grabPointInfo.SnapMode != UxrSnapToHandMode.DontSnap)
                {
                    foreach (UxrGripPoseInfo gripPose in grabPointInfo.AvatarGripPoseEntries)
                    {
                        if (gripPose.GripAlignTransformHandLeft != null)
                        {
                            yield return gripPose.GripAlignTransformHandLeft;
                        }

                        if (gripPose.GripAlignTransformHandRight != null)
                        {
                            yield return gripPose.GripAlignTransformHandRight;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all the align transforms of a given registered avatar and hand.
        /// </summary>
        /// <param name="avatar">The avatar instance or prefab</param>
        /// <param name="includeLeft">Whether to include snap transforms for the left hand</param>
        /// <param name="includeRight">Whether to include snap transforms for the right hand</param>
        /// <param name="usePrefabInheritance">Whether to get the transforms for </param>
        /// <returns>List of align transforms</returns>
        public IEnumerable<Transform> Editor_GetAllRegisteredAvatarGrabAlignTransforms(UxrAvatar avatar, bool includeLeft, bool includeRight, bool usePrefabInheritance)
        {
            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);

                if (grabPointInfo.SnapMode != UxrSnapToHandMode.DontSnap)
                {
                    foreach (UxrGripPoseInfo gripPoseInfo in grabPointInfo.GetCompatibleGripPoseInfos(avatar, usePrefabInheritance))
                    {
                        if (includeLeft && gripPoseInfo.GripAlignTransformHandLeft != null)
                        {
                            yield return gripPoseInfo.GripAlignTransformHandLeft;
                        }

                        if (includeRight && gripPoseInfo.GripAlignTransformHandRight != null)
                        {
                            yield return gripPoseInfo.GripAlignTransformHandRight;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the grab point indices that use the given <see cref="Transform" /> for alignment when interacting with an
        ///     avatar.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        /// <param name="snapTransform">Alignment transform</param>
        /// <returns>List of align transforms</returns>
        public List<int> Editor_GetGrabPointsForGrabAlignTransform(UxrAvatar avatar, Transform snapTransform)
        {
            List<int> grabPointIndices = new List<int>();

            for (int i = 0; i < GrabPointCount; ++i)
            {
                UxrGrabPointInfo grabPointInfo = GetGrabPoint(i);
                UxrGripPoseInfo  gripPoseInfo  = grabPointInfo.GetGripPoseInfo(avatar);

                if (gripPoseInfo.GripAlignTransformHandLeft == snapTransform || gripPoseInfo.GripAlignTransformHandRight == snapTransform)
                {
                    grabPointIndices.Add(i);
                }
            }

            return grabPointIndices;
        }

        /// <summary>
        ///     Gets the <see cref="Transform" /> used for alignment when the given avatar grabs a point using a given hand.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        /// <param name="grabPoint">Grab point</param>
        /// <param name="handSide">Hand to get the snap transform for</param>
        /// <returns>Alignment transform</returns>
        public Transform Editor_GetGrabPointGrabAlignTransform(UxrAvatar avatar, int grabPoint, UxrHandSide handSide)
        {
            UxrGripPoseInfo gripPoseInfo = GetGrabPoint(grabPoint).GetGripPoseInfo(avatar);

            if (handSide == UxrHandSide.Left)
            {
                if (gripPoseInfo.GripAlignTransformHandLeft == null || GetGrabPoint(grabPoint).SnapReference != UxrSnapReference.UseOtherTransform)
                {
                    return transform;
                }

                return gripPoseInfo.GripAlignTransformHandLeft;
            }

            if (gripPoseInfo.GripAlignTransformHandRight == null || GetGrabPoint(grabPoint).SnapReference != UxrSnapReference.UseOtherTransform)
            {
                return transform;
            }

            return gripPoseInfo.GripAlignTransformHandRight;
        }

        /// <summary>
        ///     Checks whether the object has a grab point with the given <see cref="Transform" /> for alignment.
        /// </summary>
        /// <param name="snapTransform">Transform to check</param>
        /// <returns>Whether the given <see cref="Transform" /> is present in any of the grab point alignments</returns>
        public bool Editor_HasGrabPointWithGrabAlignTransform(Transform snapTransform)
        {
            for (int grabPoint = 0; grabPoint < GrabPointCount; ++grabPoint)
            {
                for (int i = 0; i < GetGrabPoint(grabPoint).GripPoseInfoCount; ++i)
                {
                    UxrGripPoseInfo gripPoseInfo = GetGrabPoint(grabPoint).GetGripPoseInfo(i);

                    if (gripPoseInfo.GripAlignTransformHandLeft == snapTransform || gripPoseInfo.GripAlignTransformHandRight == snapTransform)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks whether the object has a grab point with the given <see cref="Transform" /> for alignment registered using
        ///     the given prefab.
        /// </summary>
        /// <param name="snapTransform">Transform to check</param>
        /// <param name="avatarPrefab">Avatar prefab</param>
        /// <returns>
        ///     Whether the given <see cref="Transform" /> is present in any of the grab point alignments registered using the
        ///     given avatar prefab
        /// </returns>
        public bool Editor_HasGrabPointWithGrabAlignTransform(Transform snapTransform, GameObject avatarPrefab)
        {
            for (int grabPoint = 0; grabPoint < GrabPointCount; ++grabPoint)
            {
                UxrGripPoseInfo gripPoseInfo = GetGrabPoint(grabPoint).GetGripPoseInfo(avatarPrefab.GetComponent<UxrAvatar>(), false);

                if (gripPoseInfo != null && (gripPoseInfo.GripAlignTransformHandLeft == snapTransform || gripPoseInfo.GripAlignTransformHandRight == snapTransform))
                {
                    return true;
                }
            }

            return false;
        }
#endif
    }
}

#pragma warning restore 0414