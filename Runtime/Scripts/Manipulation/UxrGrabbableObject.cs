// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Math;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System.Math;
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
    ///             <see cref="ConstraintsApplying" />, <see cref="ConstraintsApplied" /> and
    ///             <see cref="ConstraintsFinished" /> allow to program more complex logic when grabbing objects.
    ///         </item>
    ///     </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class UxrGrabbableObject : UxrComponent<UxrGrabbableObject>, IUxrGrabbable
    {
        #region Inspector Properties/Serialized Fields

        // Grabbable dependency

        [SerializeField] private bool _controlParentDirection = true;
        [SerializeField] private bool _ignoreGrabbableParentDependency;

        // General parameters

        [SerializeField] private int  _priority;
        [SerializeField] private bool _allowMultiGrab = true;

        // Constraints

        [SerializeField] private UxrTranslationConstraintMode _translationConstraintMode = UxrTranslationConstraintMode.Free;
        [SerializeField] private BoxCollider                  _restrictToBox;
        [SerializeField] private SphereCollider               _restrictToSphere;
        [SerializeField] private Vector3                      _translationLimitsMin     = Vector3.zero;
        [SerializeField] private Vector3                      _translationLimitsMax     = Vector3.zero;
        [SerializeField] private UxrRotationConstraintMode    _rotationConstraintMode   = UxrRotationConstraintMode.Free;
        [SerializeField] private Vector3                      _rotationAngleLimitsMin   = Vector3.zero;
        [SerializeField] private Vector3                      _rotationAngleLimitsMax   = Vector3.zero;
        [SerializeField] private bool                         _autoRotationProvider     = true;
        [SerializeField] private UxrRotationProvider          _rotationProvider         = UxrRotationProvider.HandOrientation;
        [SerializeField] private UxrAxis                      _rotationLongitudinalAxis = UxrAxis.Z;
        [SerializeField] private bool                         _needsTwoHandsToRotate;
        [SerializeField] private float                        _lockedGrabReleaseDistance = 0.4f;
        [SerializeField] private float                        _translationResistance;
        [SerializeField] private float                        _rotationResistance;

        // Physics

        [SerializeField] private Rigidbody _rigidBodySource;
        [SerializeField] private bool      _rigidBodyDynamicOnRelease   = true;
        [SerializeField] private float     _verticalReleaseMultiplier   = 1.0f;
        [SerializeField] private float     _horizontalReleaseMultiplier = 1.0f;

        // Avatar grips

        [SerializeField] private UxrPreviewGrabPoses _previewGrabPosesMode = UxrPreviewGrabPoses.ShowBothHands;
        [SerializeField] private int                 _previewPosesRegenerationType;
        [SerializeField] private int                 _previewPosesRegenerationIndex = -1;
        [SerializeField] private GameObject          _selectedAvatarForGrips;

        // Grab points

        [SerializeField] private bool                   _firstGrabPointIsMain = true;
        [SerializeField] private UxrGrabPointInfo       _grabPoint            = new UxrGrabPointInfo();
        [SerializeField] private List<UxrGrabPointInfo> _additionalGrabPoints;

        // Placement

        [SerializeField] private bool                     _useParenting;
        [SerializeField] private bool                     _autoCreateStartAnchor;
        [SerializeField] private UxrGrabbableObjectAnchor _startAnchor;
        [SerializeField] private string                   _tag                       = "";
        [SerializeField] private bool                     _dropAlignTransformUseSelf = true;
        [SerializeField] private Transform                _dropAlignTransform;
        [SerializeField] private UxrSnapToAnchorMode      _dropSnapMode                  = UxrSnapToAnchorMode.PositionAndRotation;
        [SerializeField] private bool                     _dropProximityTransformUseSelf = true;
        [SerializeField] private Transform                _dropProximityTransform;

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
        ///     This can be used to apply custom constraints to the object.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsApplied;

        /// <summary>
        ///     Event called right after all <see cref="ConstraintsApplied" /> finished.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsFinished;

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
        ///     Gets whether the object has translation/rotation constraints.
        /// </summary>
        public bool IsConstrained => HasTranslationConstraint || HasRotationConstraint || IsLockedInPlace || _constraintExitTimer > 0.0f;

        /// <summary>
        ///     Gets whether the object has a translation constraint.
        /// </summary>
        public bool HasTranslationConstraint => TranslationConstraint != UxrTranslationConstraintMode.Free;

        /// <summary>
        ///     Gets whether the object has a rotation constraint.
        /// </summary>
        public bool HasRotationConstraint => RotationConstraint != UxrRotationConstraintMode.Free;

        /// <summary>
        ///     Gets the number of axes that the object can rotate around.
        /// </summary>
        public int RangeOfMotionRotationAxisCount
        {
            get
            {
                if (RotationConstraint == UxrRotationConstraintMode.Free)
                {
                    return 3;
                }

                if (RotationConstraint == UxrRotationConstraintMode.Locked)
                {
                    return 0;
                }

                return Vector3Ext.DifferentComponentCount(_rotationAngleLimitsMin, _rotationAngleLimitsMax);
            }
        }

        /// <summary>
        ///     Gets the local axes that the object can rotate around.
        /// </summary>
        public IEnumerable<UxrAxis> RangeOfMotionRotationAxes
        {
            get
            {
                if (RotationConstraint == UxrRotationConstraintMode.Free)
                {
                    yield return UxrAxis.X;
                    yield return UxrAxis.Y;
                    yield return UxrAxis.Z;
                }

                if (RotationConstraint == UxrRotationConstraintMode.Locked)
                {
                    yield break;
                }

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_rotationAngleLimitsMin[axisIndex], _rotationAngleLimitsMax[axisIndex]))
                    {
                        yield return axisIndex;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the local axes that the object can rotate around with limited range of motion (not freely, nor locked).
        /// </summary>
        public IEnumerable<UxrAxis> LimitedRangeOfMotionRotationAxes
        {
            get
            {
                if (RotationConstraint == UxrRotationConstraintMode.Free || RotationConstraint == UxrRotationConstraintMode.Locked)
                {
                    yield break;
                }

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_rotationAngleLimitsMin[axisIndex], _rotationAngleLimitsMax[axisIndex]))
                    {
                        yield return axisIndex;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the index of the rotation axis if the object can only rotate around that single axis.
        ///     Will return any of these values: (x = 0, y = 1, z = 2, none or more than one = -1).
        /// </summary>
        public int SingleRotationAxisIndex
        {
            get
            {
                if (RotationConstraint == UxrRotationConstraintMode.Free || RotationConstraint == UxrRotationConstraintMode.Locked)
                {
                    return -1;
                }

                int constrainedAxisIndex = 0;
                int constrainedAxisCount = 0;

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_rotationAngleLimitsMin[axisIndex], _rotationAngleLimitsMax[axisIndex]))
                    {
                        constrainedAxisIndex = axisIndex;
                        constrainedAxisCount++;
                    }
                }

                return constrainedAxisCount == 1 ? constrainedAxisIndex : -1;
            }
        }

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
        ///     Gets the minimum allowed angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        public float MinSingleRotationDegrees
        {
            get
            {
                int singleRotationAxisIndex = SingleRotationAxisIndex;
                return singleRotationAxisIndex == -1 ? 0.0f : _rotationAngleLimitsMin[singleRotationAxisIndex];
            }
        }

        /// <summary>
        ///     Gets the maximum allowed angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        public float MaxSingleRotationDegrees
        {
            get
            {
                int singleRotationAxisIndex = SingleRotationAxisIndex;
                return singleRotationAxisIndex == -1 ? 0.0f : _rotationAngleLimitsMax[singleRotationAxisIndex];
            }
        }

        /// <summary>
        ///     Gets or sets the rotation angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        public float SingleRotationAxisDegrees
        {
            get
            {
                int singleRotationAxisIndex = SingleRotationAxisIndex;

                if (singleRotationAxisIndex == -1)
                {
                    return 0.0f;
                }

                return (_singleRotationAngleCumulative + _singleRotationAngleGrab).Clamped(_rotationAngleLimitsMin[singleRotationAxisIndex], _rotationAngleLimitsMax[singleRotationAxisIndex]);
            }
            set
            {
                if (!IsBeingGrabbed)
                {
                    int singleRotationAxisIndex = SingleRotationAxisIndex;

                    if (singleRotationAxisIndex == -1)
                    {
                        return;
                    }

                    _singleRotationAngleCumulative = value.Clamped(_rotationAngleLimitsMin[singleRotationAxisIndex], _rotationAngleLimitsMax[singleRotationAxisIndex]);
                    transform.localRotation        = InitialLocalRotation * Quaternion.AngleAxis(_singleRotationAngleCumulative, (UxrAxis)singleRotationAxisIndex);
                }
            }
        }

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
        ///     Gets or sets the rotation provider. The rotation provider is used in objects with constrained position to know
        ///     which element drives the rotation.
        /// </summary>
        public UxrRotationProvider RotationProvider
        {
            get => HasTranslationConstraint && LimitedRangeOfMotionRotationAxes.Any() ? _rotationProvider : UxrRotationProvider.HandOrientation;
            set => _rotationProvider = value;
        }

        /// <summary>
        ///     Gets or sets which one is the longitudinal axis (x, y or z) in a rotation with constraints on two or more axes.
        /// </summary>
        public UxrAxis RotationLongitudinalAxis
        {
            get => _rotationLongitudinalAxis;
            set => _rotationLongitudinalAxis = value;
        }

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
                UxrGrabManager.Instance.PlaceObject(this, _startAnchor, UxrPlacementOptions.None, propagateEvents);
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
            else if (!grabPointEnabled && !grabPointEnabled)
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
            UxrGrabPointInfo grabPointInfo             = GetGrabPoint(grabPoint);
            Transform        grabberProximityTransform = grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex);
            
            distance = Vector3.Distance(grabberProximityTransform.position, GetGrabPointGrabProximityTransform(grabber, grabPoint).position);

            // distanceRotationAdd will store the distance added to count for the rotation and favor those grips closer in orientation to the grabber

            float distanceRotationAdd = 0.0f;

            // First check if there is an UxrGrabPointShape based component that describes this 

            UxrGrabPointShape grabPointShape = GetGrabPointShape(grabPoint);

            if (grabPointShape != null)
            {
                distance = grabPointShape.GetDistanceFromGrabber(grabber,
                                                                 GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side),
                                                                 GetGrabPointGrabProximityTransform(grabber, grabPoint),
                                                                 grabberProximityTransform);
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
                                                          grabberProximityTransform,
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
            grabberPosition = grabber.transform.position;
            grabberRotation = grabber.transform.rotation;

            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);
            Transform        snapTransform = GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side);

            if (snapTransform == null || grabPointInfo == null)
            {
                return false;
            }

            if (GetSnapModeAffectsPosition(grabPointInfo.SnapMode))
            {
                grabberPosition = snapTransform.position;
            }

            if (GetSnapModeAffectsRotation(grabPointInfo.SnapMode))
            {
                grabberRotation = snapTransform.rotation;
            }

            UxrGrabPointShape grabPointShape = GetGrabPointShape(grabPoint);

            if (grabPointShape != null)
            {
                Transform grabberProximityTransform = grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex);
                grabPointShape.GetClosestSnap(grabber, snapTransform, GetGrabPointGrabProximityTransform(grabber, grabPoint), grabberProximityTransform, out grabberPosition, out grabberRotation);
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
                            grabber.HandBone.position = Vector3.Lerp(grabber.Avatar.transform.TransformPoint(grabInfo.HandBoneLocalAvatarPositionOnGrab), grabber.HandBone.position, t);
                        }
                        if (GetGrabPointSnapModeAffectsRotation(grabPoint))
                        {
                            grabber.HandBone.rotation = Quaternion.Slerp(grabber.Avatar.transform.rotation * grabInfo.HandBoneLocalAvatarRotationOnGrab, grabber.HandBone.rotation, t);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Tries to get the longitudinal rotation axis of the grabbable object. If it hasn't been defined by the user (on
        ///     objects where <see cref="RangeOfMotionRotationAxisCount" /> is less than 2.
        /// </summary>
        /// <returns>Longitudinal rotation axis</returns>
        public UxrAxis GetMostProbableLongitudinalRotationAxis()
        {
            if (RangeOfMotionRotationAxisCount > 1)
            {
                // Longitudinal axis is user defined for constrained rotation with more than one axis
                return _rotationLongitudinalAxis;
            }

            // We have an object with a single rotation axis. First compute bounds and see if the rotation pivot is not centered.

            Bounds localBounds            = gameObject.GetLocalBounds(true);
            int    maxUncenteredComponent = -1;
            float  maxUncenteredDistance  = 0.0f;

            for (int i = 0; i < 3; ++i)
            {
                float centerOffset = Mathf.Abs(localBounds.center[i]);

                if (centerOffset > localBounds.size[i] * 0.25f && centerOffset > maxUncenteredDistance)
                {
                    maxUncenteredComponent = i;
                    maxUncenteredDistance  = centerOffset;
                }
            }

            // Found an axis that is significantly larger than others?

            if (maxUncenteredComponent != -1)
            {
                return maxUncenteredComponent;
            }

            // At this point the best bet is the single rotation axis

            int singleRotationAxisIndex = SingleRotationAxisIndex;

            if (singleRotationAxisIndex != -1)
            {
                return singleRotationAxisIndex;
            }

            return UxrAxis.Z;
        }

        /// <summary>
        ///     Tries to infer the most appropriate <see cref="UxrRotationProvider" /> to rotate the object based on the shape and
        ///     size of the object, and the grip.
        /// </summary>
        /// <param name="gripPos">The grip snap position</param>
        /// <returns>Most appropriate <see cref="UxrRotationProvider" /></returns>
        public UxrRotationProvider GetAutoRotationProvider(Vector3 gripPos)
        {
            if (!(HasTranslationConstraint && LimitedRangeOfMotionRotationAxes.Any()))
            {
                // No constraint
                return UxrRotationProvider.HandOrientation;
            }

            UxrAxis longitudinalAxis        = GetMostProbableLongitudinalRotationAxis();
            int     singleRotationAxisIndex = SingleRotationAxisIndex;
            float   leverageDistance        = 0.0f;

            if (singleRotationAxisIndex != -1)
            {
                // Object with a single rotation axis

                if (longitudinalAxis != singleRotationAxisIndex)
                {
                    // Lever action
                    return UxrRotationProvider.HandPositionAroundPivot;
                }

                // Lever action will depend on grabber distance to rotation axis. Smaller than a hand distance will use rotation while larger will use leverage.
                leverageDistance = Vector3.ProjectOnPlane(gripPos - transform.position, transform.TransformDirection(longitudinalAxis)).magnitude;
                return leverageDistance > UxrConstants.Hand.HandWidth ? UxrRotationProvider.HandPositionAroundPivot : UxrRotationProvider.HandOrientation;
            }

            // Object with more than one rotation axis

            leverageDistance = Mathf.Abs(gripPos.DistanceToPlane(transform.position, transform.TransformDirection(longitudinalAxis)));
            return leverageDistance > UxrConstants.Hand.HandWidth ? UxrRotationProvider.HandPositionAroundPivot : UxrRotationProvider.HandOrientation;
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
            _constraintTimer        = -1.0f;
            _constraintExitTimer    = ConstrainSeconds;
            _constraintLocalExitPos = transform.localPosition;
            _constraintLocalExitRot = transform.localRotation;
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
            return TransformExt.GetWorldPosition(grabPointInfo.RuntimeGrabs[grabber].GrabAlignParentTransformUsed, grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignPosition);
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
            return TransformExt.GetWorldRotation(grabPointInfo.RuntimeGrabs[grabber].GrabAlignParentTransformUsed, grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignRotation);
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
        /// <param name="localPositionBeforeUpdate">Object local position before being updated</param>
        /// <param name="localRotationBeforeUpdate">Object local rotation before being updated</param>
        /// <param name="propagateEvents">
        ///     Whether to propagate <see cref="ConstraintsApplying" />, <see cref="ConstraintsApplied" />
        ///     and <see cref="ConstraintsFinished" /> events
        /// </param>
        internal void CheckAndApplyConstraints(UxrGrabber grabber, int grabPoint, Vector3 localPositionBeforeUpdate, Quaternion localRotationBeforeUpdate, bool propagateEvents)
        {
            if (propagateEvents)
            {
                OnConstraintsApplying(new UxrApplyConstraintsEventArgs(grabber, grabPoint));
            }

            if (IsLockedInPlace)
            {
                transform.SetLocalPositionAndRotation(localPositionBeforeUpdate, localRotationBeforeUpdate);

                if (propagateEvents)
                {
                    OnConstraintsApplied(new UxrApplyConstraintsEventArgs(grabber, grabPoint));
                }

                return;
            }

            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            if (grabPointInfo == null)
            {
                if (propagateEvents)
                {
                    OnConstraintsApplied(new UxrApplyConstraintsEventArgs(grabber, grabPoint));
                }

                return;
            }

            UxrRuntimeGripInfo gripInfo = grabPointInfo.RuntimeGrabs[grabber];

            if (RotationProvider == UxrRotationProvider.HandPositionAroundPivot)
            {
                // Position

                transform.position = grabber.transform.TransformPoint(GetGrabPointRelativeGrabPosition(grabber, grabPoint));
                ConstrainTransform(true, false, localPositionBeforeUpdate, localRotationBeforeUpdate);

                int rangeOfMotionAxisCount = RangeOfMotionRotationAxisCount;

                // Rotation: We use the angle between V1(pivot, initial grab position) and V2 (pivot, current grab position).
                //           This method works better for levers, steering wheels, etc. It won't work well with elements
                //           like knobs or similar because the point where the torque is applied lies in the rotation axis itself.
                //           In this cases we recommend using ManipulationMode.GrabAndMove instead.

                Vector3 grabDirection        = grabber.transform.TransformPoint(gripInfo.GrabberLocalLeverageSource) - transform.position;
                Vector3 initialGrabDirection = TransformExt.GetWorldPosition(transform.parent, gripInfo.GrabberLocalParentLeverageSourceOnGrab) - transform.position;

                if (rangeOfMotionAxisCount == 1)
                {
                    // Compute in local coordinates

                    grabDirection        = Quaternion.Inverse(transform.GetParentRotation()) * grabDirection;
                    initialGrabDirection = Quaternion.Inverse(transform.GetParentRotation()) * initialGrabDirection;
                    
                    // When there's a single axis with range of motion, we use additional computations to be able to specify ranges below/above -180/180 degrees

                    UxrAxis rotationAxis                  = SingleRotationAxisIndex;
                    Vector3 projectedGrabDirection        = Vector3.ProjectOnPlane(grabDirection,        localRotationBeforeUpdate * rotationAxis);
                    Vector3 projectedInitialGrabDirection = Vector3.ProjectOnPlane(initialGrabDirection, localRotationBeforeUpdate * rotationAxis);

                    float angle      = Vector3.SignedAngle(projectedInitialGrabDirection, projectedGrabDirection, localRotationBeforeUpdate * rotationAxis);
                    float angleDelta = angle - _singleRotationAngleGrab.ToEuler180();

                    // Keep track of turns below/above -360/360 degrees.

                    if (angleDelta > 180.0f)
                    {
                        _singleRotationAngleGrab -= 360.0f - angleDelta;
                    }
                    else if (angleDelta < -180.0f)
                    {
                        _singleRotationAngleGrab += 360.0f + angleDelta;
                    }
                    else
                    {
                        _singleRotationAngleGrab += angleDelta;
                    }

                    // Clamp inside valid range 

                    float rotationAngle = (_singleRotationAngleCumulative + _singleRotationAngleGrab).Clamped(_rotationAngleLimitsMin[rotationAxis], _rotationAngleLimitsMax[rotationAxis]);
                    _singleRotationAngleGrab = rotationAngle - _singleRotationAngleCumulative;

                    // Rotate using absolute current rotation to preserve precision

                    transform.localRotation = InitialLocalRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);
                }
                else
                {
                    // Here we can potentially have up to 3 rotational ranges of motion but we use the hand position around the pivot to rotate the object, so we need to be
                    // extra careful with not losing any information when computing the rotation and clamping.

                    // First compute the rotation of the grabbed object if it were to be controlled by the hand orientation
                    Quaternion rotUsingHandOrientation = grabber.transform.rotation * GetGrabPointRelativeGrabRotation(grabber, grabPoint);

                    // Now compute the rotation of the grabbed object if we were to use the hand position around the axis. But we do not use this directly because we
                    // potentially lose the rotation around the longitudinal axis if there is one. We use it instead to know where the longitudinal axis will point,
                    // and correct rotUsingHandOrientation.
                    Quaternion rotationOnGrab            = transform.GetParentRotation() * gripInfo.LocalRotationOnGrab;
                    Quaternion rotUsingHandPosAroundAxis = Quaternion.FromToRotation(initialGrabDirection.normalized, grabDirection.normalized) * rotationOnGrab;
                    Quaternion rotCorrection             = Quaternion.FromToRotation(rotUsingHandOrientation * _rotationLongitudinalAxis, rotUsingHandPosAroundAxis * _rotationLongitudinalAxis);
                    Quaternion localRotation             = Quaternion.Inverse(transform.GetParentRotation()) * rotCorrection * rotUsingHandOrientation;

                    if (RotationConstraint == UxrRotationConstraintMode.Free)
                    {
                        transform.localRotation = localRotation;
                    }
                    else
                    {
                        transform.localRotation = ClampRotation(localRotation, localRotationBeforeUpdate, InitialLocalRotation, _rotationAngleLimitsMin, _rotationAngleLimitsMax, false, ref _singleRotationAngleCumulative);
                    }
                }
            }
            else
            {
                // Alignment

                if (UsesGrabbableParentDependency == false)
                {
                    Vector3    sourcePosition = TransformExt.GetWorldPosition(grabPointInfo.RuntimeGrabs[grabber].GrabAlignParentTransformUsed, grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignPosition);
                    Quaternion sourceRotation = TransformExt.GetWorldRotation(grabPointInfo.RuntimeGrabs[grabber].GrabAlignParentTransformUsed, grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignRotation);
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

                    transform.ApplyAlignment(sourcePosition, sourceRotation, grabber.transform.position, targetRotation, GetGrabPointSnapModeAffectsRotation(grabPoint, UxrHandSnapDirection.ObjectToHand), false);

                    // Constrain rotation before snapping pivot
                    ConstrainTransform(false, true, localPositionBeforeUpdate, localRotationBeforeUpdate);

                    sourcePosition = TransformExt.GetWorldPosition(grabPointInfo.RuntimeGrabs[grabber].GrabAlignParentTransformUsed, grabPointInfo.RuntimeGrabs[grabber].RelativeGrabAlignPosition);

                    transform.ApplyAlignment(sourcePosition, sourceRotation, grabber.transform.position, targetRotation, false, GetGrabPointSnapModeAffectsPosition(grabPoint, UxrHandSnapDirection.ObjectToHand));

                    if (gripInfo.GrabTimer > 0.0f)
                    {
                        // Smooth transition to the hand
                        float t = 1.0f - Mathf.Clamp01(gripInfo.GrabTimer / ObjectAlignmentSeconds);
                        transform.localPosition = Vector3.Lerp(gripInfo.LocalPositionOnGrab, transform.localPosition, t);
                        transform.localRotation = Quaternion.Slerp(gripInfo.LocalRotationOnGrab, transform.localRotation, t);
                    }
                }

                // Translation & Rotation

                ConstrainTransform(true, true, localPositionBeforeUpdate, localRotationBeforeUpdate);

                // Smoothly exit from a constraint if we had manually created the transition using StartSmoothConstraintExit()

                if (_constraintExitTimer > 0.0f)
                {
                    float constraintExitT = 1.0f - _constraintExitTimer / ConstrainSeconds;

                    transform.SetLocalPositionAndRotation(Vector3.Lerp(_constraintLocalExitPos, transform.localPosition, constraintExitT),
                                                          Quaternion.Lerp(_constraintLocalExitRot, transform.localRotation, constraintExitT));
                }
            }

            if (UxrGrabManager.Instance.GetHandsGrabbingCount(this) == 1 && _constraintTimer <= 0.0f && _constraintExitTimer <= 0.0f && _placementTimer <= 0.0f && gripInfo.HandLockTimer <= 0.0f && gripInfo.GrabTimer <= 0.0f)
            {
                // Only apply smoothing when grabbing with a single hand and no transitions are being executed
                transform.SetLocalPositionAndRotation(UxrInterpolator.SmoothDampPosition(localPositionBeforeUpdate, transform.localPosition, _translationResistance),
                                                      UxrInterpolator.SmoothDampRotation(localRotationBeforeUpdate, transform.localRotation, _rotationResistance));
            }

            if (propagateEvents)
            {
                OnConstraintsApplied(new UxrApplyConstraintsEventArgs(grabber, grabPoint));
            }

            if (propagateEvents)
            {
                OnConstraintsFinished(new UxrApplyConstraintsEventArgs(grabber, grabPoint));
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
                Transform grabberProximityTransform = grabber.GetProximityTransform(grabPointInfo.GrabberProximityTransformIndex);
                grabPointShape.GetClosestSnap(grabber, grabAlignTransform, GetGrabPointGrabProximityTransform(grabber, grabPoint), grabberProximityTransform, out snapPosition, out snapRotation);
            }

            Matrix4x4 snapMatrix     = Matrix4x4.TRS(snapPosition, snapRotation, grabAlignTransform.lossyScale);
            Vector3   localProximity = grabAlignTransform.InverseTransformPoint(GetGrabPointGrabProximityTransform(grabber, grabPoint).position);

            ComputeGrabTransforms(grabber, grabPoint);

            grabInfo.GrabAlignParentTransformUsed = grabAlignTransform.parent;
            grabInfo.RelativeGrabAlignPosition    = TransformExt.GetLocalPosition(grabInfo.GrabAlignParentTransformUsed, snapPosition);
            grabInfo.RelativeGrabAlignRotation    = TransformExt.GetLocalRotation(grabInfo.GrabAlignParentTransformUsed, snapRotation);
            grabInfo.RelativeProximityPosition    = transform.InverseTransformPoint(snapMatrix.MultiplyPoint(localProximity));
            grabInfo.GrabberLocalLeverageSource   = Vector3.zero;

            if (_autoRotationProvider)
            {
                _rotationProvider = GetAutoRotationProvider(snapPosition);
            }

            if (RotationProvider == UxrRotationProvider.HandPositionAroundPivot && GetGrabPointSnapModeAffectsRotation(grabPoint))
            {
                // Check if the leverage is provided by the inner side of the palm (where the thumb is) or the outer side.
                // We do that by checking the difference in distance of both to the rotation pivot. If it is above a threshold, it is provided by either one of the two.
                // If it is below a threshold it is provide by the grabber itself.

                float separation    = UxrConstants.Hand.HandWidth;
                float distanceInner = Vector3.Distance(transform.position, snapPosition + snapRotation * grabber.LocalPalmThumbDirection * (separation * 0.5f));
                float distanceOuter = Vector3.Distance(transform.position, snapPosition - snapRotation * grabber.LocalPalmThumbDirection * (separation * 0.5f));

                if (Mathf.Abs(distanceInner - distanceOuter) > separation * 0.5f)
                {
                    grabInfo.GrabberLocalLeverageSource = grabber.LocalPalmThumbDirection * separation * 0.5f * (distanceInner > distanceOuter ? 1.0f : -1.0f);
                }
            }

            grabInfo.GrabberLocalParentLeverageSourceOnGrab = TransformExt.GetLocalPosition(transform.parent, grabber.transform.TransformPoint(grabInfo.GrabberLocalLeverageSource));

            grabInfo.LocalPositionOnGrab                    = transform.localPosition;
            grabInfo.LocalRotationOnGrab                    = transform.localRotation;
            grabInfo.AlignPositionOnGrab                    = snapPosition;
            grabInfo.AlignRotationOnGrab                    = snapRotation;
            grabInfo.HandBoneLocalAvatarPositionOnGrab      = grabber.Avatar.transform.InverseTransformPoint(grabber.HandBone.position);
            grabInfo.HandBoneLocalAvatarRotationOnGrab      = Quaternion.Inverse(grabber.Avatar.transform.rotation) * grabber.HandBone.rotation;
            grabInfo.GrabTimer                              = ObjectAlignmentSeconds;

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

            if (!UxrGrabManager.Instance.IsBeingGrabbedByOtherThan(this, grabPoint, grabber))
            {
                _singleRotationAngleGrab = 0.0f;
            }
        }

        /// <summary>
        ///     Notifies that the object just stopped being grabbed.
        /// </summary>
        /// <param name="grabber">Grabber responsible for grabbing the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        internal void NotifyEndGrab(UxrGrabber grabber, int grabPoint)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            if (grabPointInfo != null && grabPointInfo.RuntimeGrabs != null && grabPointInfo.RuntimeGrabs.ContainsKey(grabber))
            {
                grabPointInfo.RuntimeGrabs.Remove(grabber);

                _singleRotationAngleCumulative += _singleRotationAngleGrab;
                _singleRotationAngleGrab       =  0.0f;
            }
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

            // Make sure singleton is created so that grabbable objects get registered
            UxrGrabManager.Instance.Poke();

            // Fix some common mistakes just in case
            Vector3 fixedMin = Vector3.Min(_translationLimitsMin, _translationLimitsMax);
            Vector3 fixedMax = Vector3.Max(_translationLimitsMin, _translationLimitsMax);
            _translationLimitsMin   = fixedMin;
            _translationLimitsMax   = fixedMax;
            fixedMin                = Vector3.Min(_rotationAngleLimitsMin, _rotationAngleLimitsMax);
            fixedMax                = Vector3.Max(_rotationAngleLimitsMin, _rotationAngleLimitsMax);
            _rotationAngleLimitsMin = fixedMin;
            _rotationAngleLimitsMax = fixedMax;

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

            _initialIsKinematic = IsKinematic;

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
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            UxrManager.StageUpdated += UxrManager_StageUpdated;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.StageUpdated -= UxrManager_StageUpdated;
        }

        /// <summary>
        ///     Resets the component.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            
            _grabPoint                = new UxrGrabPointInfo();
            _rotationLongitudinalAxis = GetMostProbableLongitudinalRotationAxis();
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

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the <see cref="UxrManager" /> finished updating a stage.
        /// </summary>
        /// <param name="stage">Stage that finished updating</param>
        private void UxrManager_StageUpdated(UxrUpdateStage stage)
        {
            if (stage == UxrUpdateStage.Manipulation)
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
        ///     Event trigger for <see cref="ConstraintsFinished" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnConstraintsFinished(UxrApplyConstraintsEventArgs e)
        {
            ConstraintsFinished?.Invoke(this, e);
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
        /// <param name="unprocessedLocalPosition">The object local position before the manipulation update during this frame</param>
        /// <param name="unprocessedLocalRotation">The object local rotation before the manipulation update during this frame</param>
        private void ConstrainTransform(bool processPosition, bool processRotation, Vector3 unprocessedLocalPosition, Quaternion unprocessedLocalRotation)
        {
            // Rotation

            if (processRotation && _rotationConstraintMode != UxrRotationConstraintMode.Free)
            {
                Quaternion targetLocalRotation = InitialLocalRotation;

                if (_rotationConstraintMode == UxrRotationConstraintMode.RestrictLocalRotation)
                {
                    targetLocalRotation = ClampRotation(transform.localRotation, unprocessedLocalRotation, InitialLocalRotation, _rotationAngleLimitsMin, _rotationAngleLimitsMax, false, ref _singleRotationAngleCumulative);
                }

                transform.localRotation = _constraintTimer < 0.0f ? targetLocalRotation : Quaternion.Slerp(unprocessedLocalRotation, targetLocalRotation, 1.0f - _constraintTimer / ConstrainSeconds);
            }

            // Translation

            if (processPosition && _translationConstraintMode != UxrTranslationConstraintMode.Free)
            {
                Vector3 localPos       = unprocessedLocalPosition;
                Vector3 targetLocalPos = InitialLocalPosition;

                if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictToBox && _restrictToBox != null)
                {
                    targetLocalPos = transform.GetParentWorldMatrix().inverse.MultiplyPoint(transform.position.ClampToBox(_restrictToBox));
                }
                else if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictToSphere && _restrictToSphere != null)
                {
                    targetLocalPos = transform.GetParentWorldMatrix().inverse.MultiplyPoint(transform.position.ClampToSphere(_restrictToSphere));
                }
                else if (_translationConstraintMode == UxrTranslationConstraintMode.RestrictLocalOffset)
                {
                    if (transform.parent != null || InitialParent == null)
                    {
                        // Current local space -> Initial local space using the matrix at Awake() 
                        Vector3 localPosOffset = InitialRelativeMatrix.inverse.MultiplyPoint3x4(transform.localPosition);

                        // Clamp in initial local space, transform to current local space
                        targetLocalPos = InitialRelativeMatrix.MultiplyPoint(localPosOffset.Clamp(_translationLimitsMin, _translationLimitsMax));
                    }
                    else
                    {
                        targetLocalPos = transform.localPosition;
                    }
                }

                transform.localPosition = _constraintTimer < 0.0f ? targetLocalPos : Vector3.Lerp(localPos, targetLocalPos, 1.0f - _constraintTimer / ConstrainSeconds);
            }
        }

        /// <summary>
        ///     Tries to clamp a rotation.
        /// </summary>
        /// <param name="rot">Rotation to clamp</param>
        /// <param name="rotBeforeUpdate">Rotation before the manipulation update this frame</param>
        /// <param name="initialRot">Initial rotation</param>
        /// <param name="eulerMin">Minimum euler values</param>
        /// <param name="eulerMax">Maximum euler values</param>
        /// <param name="invertRotation">Whether to invert the rotation angles</param>
        /// <param name="singleRotationAngle">
        ///     The rotation angle if rotation is being constrained to a single axis. This improves
        ///     constraining by allowing ranges over +-360 degrees.
        /// </param>
        /// <returns>Clamped rotation</returns>
        private Quaternion ClampRotation(Quaternion rot, Quaternion rotBeforeUpdate, Quaternion initialRot, Vector3 eulerMin, Vector3 eulerMax, bool invertRotation, ref float singleRotationAngle)
        {
            int rangeOfMotionAxisCount = RangeOfMotionRotationAxisCount;

            if (RangeOfMotionRotationAxisCount == 0)
            {
                return initialRot;
            }

            if (rangeOfMotionAxisCount > 1)
            {
                bool invertPitchYaw    = false;
                int  ignorePitchYaw    = -1;
                bool clampLongitudinal = false;

                // Use classic yaw/pitch clamping when more than one axis has constrained range of motion.

                UxrAxis axis1            = RangeOfMotionRotationAxes.First();
                UxrAxis axis2            = RangeOfMotionRotationAxes.Last();
                UxrAxis longitudinalAxis = UxrAxis.OtherThan(axis1, axis2);

                if (rangeOfMotionAxisCount == 3)
                {
                    // Pitch/yaw clamping will be on the other-than-longitudinal axes, when all 3 axes have constrained range of motion.

                    axis1             = RangeOfMotionRotationAxes.First(a => a != _rotationLongitudinalAxis);
                    axis2             = RangeOfMotionRotationAxes.Last(a => a != _rotationLongitudinalAxis);
                    longitudinalAxis  = _rotationLongitudinalAxis;
                    clampLongitudinal = true;
                }
                else
                {
                    // If there are only two rotation axes constrained, check if one of the constrained axes is actually the longitudinal axis.
                    // In this case, we will zero either the pitch or yaw and perform longitudinal clamping later.
                    if (!longitudinalAxis.Equals(_rotationLongitudinalAxis))
                    {
                        // Ignore the incorrectly computed longitudinal axis, which in reality is either the pitch or yaw
                        ignorePitchYaw = longitudinalAxis;

                        // Assign the longitudinal axis correctly based on what the user selected
                        longitudinalAxis = _rotationLongitudinalAxis;

                        if (axis1 == longitudinalAxis)
                        {
                            axis1 = UxrAxis.OtherThan(longitudinalAxis, axis2);
                        }
                        else if (axis2 == longitudinalAxis)
                        {
                            axis2 = UxrAxis.OtherThan(longitudinalAxis, axis1);
                        }

                        // Clamp the rotation around longitudinal axis later
                        clampLongitudinal = true;

                        // Check need to invert
                        invertPitchYaw = UxrAxis.OtherThan(ignorePitchYaw, longitudinalAxis) == UxrAxis.Y;
                    }
                }

                Quaternion relativeRot     = Quaternion.Inverse(initialRot) * rot;
                Vector3    targetDirection = relativeRot * longitudinalAxis;

                float pitch = -Mathf.Asin(targetDirection[axis2]) * Mathf.Rad2Deg;
                float yaw   = Mathf.Atan2(targetDirection[axis1], targetDirection[longitudinalAxis]) * Mathf.Rad2Deg;

                pitch = longitudinalAxis == UxrAxis.Y ? Mathf.Clamp(pitch, -eulerMax[axis1], -eulerMin[axis1]) : Mathf.Clamp(pitch, eulerMin[axis1], eulerMax[axis1]);
                yaw   = longitudinalAxis == UxrAxis.Y ? Mathf.Clamp(yaw,   -eulerMax[axis2], -eulerMin[axis2]) : Mathf.Clamp(yaw,   eulerMin[axis2], eulerMax[axis2]);

                // Detect cases where we need to invert angles due to math

                if (invertPitchYaw || longitudinalAxis == UxrAxis.Y)
                {
                    pitch = -pitch;
                    yaw   = -yaw;
                }

                // Now invert if it was requested

                if (invertRotation)
                {
                    pitch = -pitch;
                    yaw   = -yaw;
                }

                // Create clamped rotation using pitch/yaw

                Vector3 clampedEuler = Vector3.zero;

                clampedEuler[axis1] = pitch;
                clampedEuler[axis2] = yaw;

                if (ignorePitchYaw != -1)
                {
                    clampedEuler[ignorePitchYaw] = 0.0f;
                }

                Quaternion clampedRot = Quaternion.Euler(clampedEuler);

                // Clamp rotation around the longitudinal axis if necessary

                if (clampLongitudinal)
                {
                    Vector3    fixedLongitudinal = clampedRot * longitudinalAxis;
                    Quaternion correctionRot     = Quaternion.FromToRotation(targetDirection, fixedLongitudinal);
                    Quaternion withRoll          = correctionRot * relativeRot;
                    float      roll              = Vector3.SignedAngle(clampedRot * axis1, withRoll * axis1, fixedLongitudinal) * (invertRotation ? -1.0f : 1.0f);
                    float      clampedRoll       = roll.Clamped(eulerMin[longitudinalAxis], eulerMax[longitudinalAxis]);

                    return initialRot * Quaternion.AngleAxis(clampedRoll, fixedLongitudinal) * clampedRot;
                }

                return initialRot * clampedRot;
            }

            // At this point we have a rotation constrained to a single axis. We will allow limits beyond +- 360 degrees by keeping track of the rotation angle.

            // Get a perpendicular vector to the rotation axis, compute the projection on the rotation plane and then get the angle increment.  

            int     singleAxisIndex            = SingleRotationAxisIndex;
            Vector3 rotationAxis               = (UxrAxis)singleAxisIndex;
            Vector3 perpendicularAxis          = ((UxrAxis)singleAxisIndex).Perpendicular;
            Vector3 initialPerpendicularVector = initialRot * perpendicularAxis;
            Vector3 currentPerpendicularVector = Vector3.ProjectOnPlane(rot * perpendicularAxis, rotationAxis);
            float   angle                      = Vector3.SignedAngle(initialPerpendicularVector, currentPerpendicularVector, rotationAxis) * (invertRotation ? -1.0f : 1.0f);
            float   angleDelta                 = angle - singleRotationAngle.ToEuler180();

            // Keep track of turns below/above -360/360 degrees.

            if (angleDelta > 180.0f)
            {
                singleRotationAngle -= 360.0f - angleDelta;
            }
            else if (angleDelta < -180.0f)
            {
                singleRotationAngle += 360.0f + angleDelta;
            }
            else
            {
                singleRotationAngle += angleDelta;
            }

            // Clamp inside valid range

            singleRotationAngle.Clamp(_rotationAngleLimitsMin[singleAxisIndex], _rotationAngleLimitsMax[singleAxisIndex]);
            return initialRot * Quaternion.AngleAxis(singleRotationAngle, rotationAxis);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Minimum distance allowed between two grabbable points that can be grabbed at the same time. Avoid hand overlapping.
        /// </summary>
        private const float MinHandGrabInterDistance = UxrConstants.Hand.HandWidth * 0.5f + 0.01f;

        // Private vars

        private readonly Dictionary<int, bool> _grabPointEnabledStates = new Dictionary<int, bool>();

        private bool                               _initialIsKinematic = true;
        private float                              _singleRotationAngleGrab;
        private float                              _singleRotationAngleCumulative;
        private float                              _placementTimer         = -1.0f;
        private float                              _constraintTimer        = -1.0f;
        private float                              _constraintExitTimer    = -1.0f;
        private Vector3                            _constraintLocalExitPos = Vector3.zero;
        private Quaternion                         _constraintLocalExitRot = Quaternion.identity;
        private Dictionary<int, UxrGrabPointShape> _grabPointShapes        = new Dictionary<int, UxrGrabPointShape>();

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