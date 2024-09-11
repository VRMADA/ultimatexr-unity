// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Math;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSync;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Networking;
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
    public partial class UxrGrabbableObject : UxrComponent<UxrGrabbableObject>, IUxrGrabbable
    {
        #region Inspector Properties/Serialized Fields

        // Grabbable dependency

        [SerializeField] private bool _isDummyGrabbableParent;
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

        [SerializeField] private bool                     _useParenting = true;
        [SerializeField] private bool                     _autoCreateStartAnchor;
        [SerializeField] private float                    _autoAnchorMaxPlaceDistance = 0.1f;
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
        ///     Event called right before applying the position/rotation constraints to the object.
        ///     This can be used to get the object position/rotation before any user constraints are
        ///     applied.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsApplying;

        /// <summary>
        ///     Event called to apply custom user constraints to the object. Moving the object
        ///     guarantees that the grips on the object will stay in place even if the object
        ///     is not parented to the hands.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsApplied;

        /// <summary>
        ///     Event called right after all <see cref="ConstraintsApplied" /> finished.
        ///     This should not be used to apply any constraints, only to get the final constrained
        ///     position/rotation to, for example, apply manipulation haptic feedback.
        /// </summary>
        public event EventHandler<UxrApplyConstraintsEventArgs> ConstraintsFinished;

        /// <summary>
        ///     <para>
        ///         Gets whether the grabbable object is a dummy grabbable parent. Dummy grabbable parents are objects
        ///         that can only be manipulated through the children, but still have their own translation/rotations constraints.
        ///     </para>
        ///     <para>
        ///         An example of a dummy grabbable parent is a door and handle setup. The door is the parent, but the grabbable
        ///         object is the door handle which can rotate around itself and should also allow rotating the door.
        ///         Using the door handle only, the door cannot be opened or closed, because only the handle will rotate and the
        ///         handle is a child of the door.<br />
        ///         UltimateXR allows grabbable children to control a grabbable parent direction using
        ///         <see cref="ControlParentDirection" />, but sometimes the parent should not really be grabbable. When the
        ///         parent is not grabbable but the <see cref="ControlParentDirection" /> is still desired, it is possible to add a
        ///         <see cref="UxrGrabbableObject" /> component to the parent, set up translation/rotation constraints and enable
        ///         the <see cref="IsDummyGrabbableParent" /> property.
        ///     </para>
        ///     <para>
        ///         Some other examples where dummy grabbable parents come in handy:
        ///         <list type="bullet">
        ///             <item>An aircraft yoke column that moves forward/backward when the child yoke object is being grabbed.</item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool IsDummyGrabbableParent => _isDummyGrabbableParent;

        /// <summary>
        ///     Gets whether a dependent object can control the grabbable parent's direction when being moved.
        /// </summary>
        public bool ControlParentDirection => _controlParentDirection;

        /// <summary>
        ///     Gets whether the grabbable parent dependency is ignored. <see cref="UsesGrabbableParentDependency" />.
        /// </summary>
        public bool IgnoreGrabbableParentDependency => _ignoreGrabbableParentDependency;

        /// <summary>
        ///     Gets whether  the object has constraints and at the same time has a grabbable parent. This means that the object
        ///     can either be considered as another grabbable part of the parent object or a separate grabbable object that is just
        ///     attached to the parent object but has no control over it. The former are movable parts in a composite object while
        ///     the latter are independent grabbable objects that happen to be in the hierarchy.
        /// </summary>
        public bool HasGrabbableParentDependency => IsConstrained && GetGrabbableParent(this) != null;

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
        ///                 optionally also control the parent's direction when moved.
        ///             </item>
        ///             <item>
        ///                 Independent (<see cref="_ignoreGrabbableParentDependency" /> is true): The object is considered as a
        ///                 separate entity where it just happens to be placed under the hierarchy, but it can be moved
        ///                 freely without being constrained by the parent.
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool UsesGrabbableParentDependency => HasGrabbableParentDependency && !_ignoreGrabbableParentDependency;

        /// <summary>
        ///     Gets whether the object can be grabbed with more than one hand.
        /// </summary>
        public bool AllowMultiGrab => _allowMultiGrab;

        /// <summary>
        ///     Gets the first <see cref="UxrGrabbableObject" /> upwards in the hierarchy, and that the object can be dependent on.
        ///     To check whether the dependency is used (meaning, this object controls the parent direction) use
        ///     <see cref="UsesGrabbableParentDependency" />.
        /// </summary>
        public UxrGrabbableObject GrabbableParent => GetGrabbableParent(this);

        /// <summary>
        ///     Gets whether the object has translation/rotation constraints.
        /// </summary>
        public bool IsConstrained => HasTranslationConstraint || HasRotationConstraint || IsLockedInPlace;

        /// <summary>
        ///     Gets whether the object has a translation constraint.
        /// </summary>
        public bool HasTranslationConstraint => TranslationConstraint != UxrTranslationConstraintMode.Free;

        /// <summary>
        ///     Gets whether the object has a rotation constraint.
        /// </summary>
        public bool HasRotationConstraint => RotationConstraint != UxrRotationConstraintMode.Free;

        /// <summary>
        ///     Gets the number of axes that the object can be translated in.
        /// </summary>
        public int RangeOfMotionTranslationAxisCount
        {
            get
            {
                if (TranslationConstraint == UxrTranslationConstraintMode.Free)
                {
                    return 3;
                }

                if (TranslationConstraint == UxrTranslationConstraintMode.Locked)
                {
                    return 0;
                }

                return Vector3Ext.DifferentComponentCount(_translationLimitsMin, _translationLimitsMax);
            }
        }

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
        ///     Gets the local axes that the object can be translated in.
        /// </summary>
        public IEnumerable<UxrAxis> RangeOfMotionTranslationAxes
        {
            get
            {
                if (TranslationConstraint == UxrTranslationConstraintMode.Free)
                {
                    yield return UxrAxis.X;
                    yield return UxrAxis.Y;
                    yield return UxrAxis.Z;
                }

                if (TranslationConstraint == UxrTranslationConstraintMode.Locked)
                {
                    yield break;
                }

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_translationLimitsMin[axisIndex], _translationLimitsMax[axisIndex]))
                    {
                        yield return axisIndex;
                    }
                }
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
        ///     Gets the local axes that the object can be translated in with limited range of motion (not freely, nor locked).
        /// </summary>
        public IEnumerable<UxrAxis> LimitedRangeOfMotionTranslationAxes
        {
            get
            {
                if (TranslationConstraint == UxrTranslationConstraintMode.Free || TranslationConstraint == UxrTranslationConstraintMode.Locked)
                {
                    yield break;
                }

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_translationLimitsMin[axisIndex], _translationLimitsMax[axisIndex]))
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
        ///     Gets the index of the translation axis if the object can only be translated in that single axis.
        ///     Will return any of these values: (x = 0, y = 1, z = 2, none or more than one = -1).
        /// </summary>
        public int SingleTranslationAxisIndex
        {
            get
            {
                if (TranslationConstraint == UxrTranslationConstraintMode.Free || TranslationConstraint == UxrTranslationConstraintMode.Locked)
                {
                    return -1;
                }

                int constrainedAxisIndex = 0;
                int constrainedAxisCount = 0;

                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                {
                    if (!Mathf.Approximately(_translationLimitsMin[axisIndex], _translationLimitsMax[axisIndex]))
                    {
                        constrainedAxisIndex = axisIndex;
                        constrainedAxisCount++;
                    }
                }

                return constrainedAxisCount == 1 ? constrainedAxisIndex : -1;
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
        public int GrabPointCount => IsDummyGrabbableParent        ? 0 :
                                     _additionalGrabPoints != null ? _additionalGrabPoints.Count + 1 : 1;

        /// <summary>
        ///     Gets the <see cref="Transform" /> that needs to align with a <see cref="UxrGrabbableObjectAnchor" /> when placing
        ///     the object on it.
        /// </summary>
        public Transform DropAlignTransform => _dropAlignTransform == null || _dropAlignTransformUseSelf ? transform : _dropAlignTransform;

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
        ///     Gets whether the object's <see cref="RigidBodySource" /> can be made dynamic when the object grip is released,
        ///     checking all conditions.
        /// </summary>
        public bool CanUseRigidBody => !UsesGrabbableParentDependency;

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
        ///     Gets the <see cref="UxrGrabbableObjectAnchor" /> the object started on, regardless of the current anchor.
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
        ///     Gets whether the first grab point in the list is the main grab in objects with more than one grab point.
        ///     When an object is grabbed with both hands, the main grab controls the actual position while the secondary grab
        ///     controls the direction.
        ///     Set it to true in objects like a rifle, where the trigger hand should be the first grab in order to keep the object
        ///     in place, and the front grab will control the aiming direction.
        ///     If false, the grab point order is irrelevant and the hand that grabbed the object first will be considered as the
        ///     main grab.
        /// </summary>
        public bool FirstGrabPointIsMain => _firstGrabPointIsMain;

        /// <summary>
        ///     Gets the rotation provider. The rotation provider is used in objects with constrained position to know
        ///     which element drives the rotation.
        /// </summary>
        public UxrRotationProvider RotationProvider => HasTranslationConstraint && LimitedRangeOfMotionRotationAxes.Any() ? _rotationProvider : UxrRotationProvider.HandOrientation;

        /// <summary>
        ///     Gets which axis is the longitudinal axis (x, y or z) in a rotation with constraints on two or more axes.
        /// </summary>
        public UxrAxis RotationLongitudinalAxis => _rotationLongitudinalAxis;

        /// <summary>
        ///     Gets or sets the rotation angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        /// <remarks>
        ///     Internally it calls <see cref="UxrGrabManager.GetObjectSingleRotationAxisDegrees" /> and
        ///     <see cref="UxrGrabManager.SetObjectSingleRotationAxisDegrees" />.
        /// </remarks>
        public float SingleRotationAxisDegrees
        {
            get => UxrGrabManager.Instance.GetObjectSingleRotationAxisDegrees(this);
            set => UxrGrabManager.Instance.SetObjectSingleRotationAxisDegrees(this, value);
        }

        /// <summary>
        ///     Gets the <see cref="UxrGrabbableObjectAnchor" /> where the object is actually placed or null if it's not placed on
        ///     any.
        /// </summary>
        public UxrGrabbableObjectAnchor CurrentAnchor
        {
            get => _currentAnchor;
            internal set => _currentAnchor = value;
        }

        /// <summary>
        ///     Gets or sets whether the object can be placed on an <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        public bool IsPlaceable
        {
            get => _isPlaceable;
            set => _isPlaceable = value;
        }

        /// <summary>
        ///     Gets or sets whether the object can be moved/rotated. A locked in place object may be grabbed but cannot be moved.
        /// </summary>
        public bool IsLockedInPlace
        {
            get => _isLockedInPlace;
            set
            {
                if (_isLockedInPlace && !value)
                {
                    StartSmoothManipulationTransition();
                }
                _isLockedInPlace = value;
            }
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
        ///     Gets or sets whether to parent the object to the <see cref="UxrGrabbableObjectAnchor" /> being placed. Also whether
        ///     to set the parent to null when grabbing the object from one.
        /// </summary>
        public bool UseParenting
        {
            get => _useParenting;
            set => _useParenting = value;
        }

        /// <summary>
        ///     Gets or sets the string that identifies which <see cref="UxrGrabbableObjectAnchor" /> components are
        ///     compatible for placement. A <see cref="UxrGrabbableObject" /> can be placed on an
        ///     <see cref="UxrGrabbableObjectAnchor" /> only if:
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

        /// <summary>
        ///     Gets or sets the rigidbody component that controls the grabbable object when it is in dynamic
        ///     (physics-enabled) mode.
        /// </summary>
        public Rigidbody RigidBodySource
        {
            get => _rigidBodySource;
            set => _rigidBodySource = value;
        }

        /// <summary>
        ///     Gets or sets how the object will align with a <see cref="UxrGrabbableObjectAnchor" /> when placing it.
        /// </summary>
        public UxrSnapToAnchorMode DropSnapMode
        {
            get => _dropSnapMode;
            set => _dropSnapMode = value;
        }

        /// <summary>
        ///     Gets whether the object is currently in a smooth transition.
        /// </summary>
        public bool IsInSmoothTransition => SmoothManipulationTimer > 0.0f || SmoothPlacementTimer > 0.0f || SmoothConstrainTimer > 0.0f;

        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets the child grabbable objects, grabbed or not, where there is no other grabbable object between the child and
        ///     this grabbable.
        /// </summary>
        internal List<UxrGrabbableObject> AllDirectChildren { get; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets all parent grabbable objects, grabbed or not.
        /// </summary>
        internal List<UxrGrabbableObject> AllParents { get; private set; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets the parent grabbable objects, grabbed or not, whose direction is controlled indirectly by this object (
        ///     <see cref="UsesGrabbableParentDependency" />) and <see cref="ControlParentDirection" />).
        /// </summary>
        internal List<UxrGrabbableObject> ParentLookAts { get; private set; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets all child grabbable objects, grabbed or not.
        /// </summary>
        internal List<UxrGrabbableObject> AllChildren { get; private set; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets all child grabbable objects, grabbed or not, that have  <see cref="UsesGrabbableParentDependency" /> and
        ///     <see cref="ControlParentDirection" />.
        /// </summary>
        internal List<UxrGrabbableObject> AllChildrenLookAts { get; private set; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets the child grabbable objects, grabbed or not, that have <see cref="UsesGrabbableParentDependency" /> and
        ///     <see cref="ControlParentDirection" />, where there is no other grabbable object between the child that controls
        ///     this grabbable.
        /// </summary>
        internal List<UxrGrabbableObject> DirectChildrenLookAts { get; private set; } = new List<UxrGrabbableObject>();

        /// <summary>
        ///     Gets or sets the number of direct children using <see cref="UsesGrabbableParentDependency" /> and (
        ///     <see cref="ControlParentDirection" />) that are being grabbed the current frame. Used by the
        ///     <see cref="UxrGrabManager" />.
        /// </summary>
        internal int DirectLookAtChildGrabbedCount { get; set; }

        /// <summary>
        ///     Gets or sets the number of <see cref="DirectLookAtChildGrabbedCount" /> children processed the current frame. Used
        ///     by the <see cref="UxrGrabManager" />.
        /// </summary>
        internal int DirectLookAtChildProcessedCount { get; set; }

        /// <summary>
        ///     Gets or sets the rotation angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        internal float SingleRotationAngleCumulative
        {
            get => _singleRotationAngleCumulative;
            set => _singleRotationAngleCumulative = value;
        }

        /// <summary>
        ///     Gets or sets the initial position of the grabbable object in local grabbable parent space, if a grabbable parent
        ///     exists.
        /// </summary>
        internal Vector3 InitialPositionInLocalGrabbableParent { get; private set; }

        /// <summary>
        ///     Gets or sets the object's local position before being updated by the grab manager.
        ///     This is only updated if the object or a parent/child dependency is being grabbed.
        /// </summary>
        internal Vector3 LocalPositionBeforeUpdate { get; set; }

        /// <summary>
        ///     Gets or sets the object's local rotation before being updated by the grab manager
        ///     This is only updated if the object or a parent/child dependency is being grabbed.
        /// </summary>
        internal Quaternion LocalRotationBeforeUpdate { get; set; }

        /// <summary>
        ///     Gets the placement options used in the last call that placed the object, for example using
        ///     <see cref="UxrGrabManager.PlaceObject" />.
        /// </summary>
        internal UxrPlacementOptions PlacementOptions
        {
            get => _placementOptions;
            set => _placementOptions = value;
        }

        #endregion

        #region Implicit IUxrGrabbable

        /// <inheritdoc />
        public bool IsBeingGrabbed => UxrGrabManager.Instance.IsBeingGrabbed(this);

        /// <inheritdoc />
        public bool IsGrabbable
        {
            get => _isGrabbable;
            set
            {
                if (value == _isGrabbable)
                {
                    return;
                }
                
                BeginSync();
                _isGrabbable = value;
                EndSyncProperty(value);
            }
        }

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
                        distanceRotationAdd = Mathf.Abs(relativeAngleDegrees) * UxrConstants.DistanceOffsetByAngle;
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

                            if (Vector3.Distance(snapPos, otherGrabber.transform.position) <= UxrConstants.MinHandGrabInterDistance)
                            {
                                // The other hand is grabbing the same shape and is too close. Avoid this by increasing distance.
                                distance += 100000.0f;
                            }
                        }
                        else if (Vector3.Distance(GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint,         grabber.Side).position,
                                                  GetGrabPointGrabAlignTransform(grabber.Avatar, otherGrabbedPoint, otherGrabber.Side).position) <= UxrConstants.MinHandGrabInterDistance)
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

            if (AllChildrenLookAts.Any() && IsDummyGrabbableParent)
            {
                // Dummy grabbable parents cannot be grabbed
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
        ///     object position/orientation.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <param name="grabPoint">Grab point</param>
        /// <param name="grabberPosition">Returns the grabber position</param>
        /// <param name="grabberRotation">Returns the grabber orientation</param>
        /// <param name="includeAlignToController">
        ///     Whether to include the rotation required for AlignToController if the grab point has it.
        /// </param>
        /// <returns>Whether the returned data is meaningful</returns>
        public bool ComputeRequiredGrabberTransform(UxrGrabber grabber, int grabPoint, out Vector3 grabberPosition, out Quaternion grabberRotation, bool includeAlignToController = true)
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

            if (grabPointInfo.AlignToController && includeAlignToController)
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
            return CanBePlacedOnAnchor(anchor, out float _);
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
        /// <remarks>
        ///     Internally it calls <see cref="UxrGrabManager.RemoveObjectFromAnchor" />.
        /// </remarks>
        public void RemoveFromAnchor(bool propagateEvents)
        {
            UxrGrabManager.Instance.RemoveObjectFromAnchor(this, propagateEvents);
        }

        /// <summary>
        ///     <see cref="UxrGrabManager.KeepGripsInPlace" />.
        /// </summary>
        public void KeepGripsInPlace()
        {
            UxrGrabManager.Instance.KeepGripsInPlace(this);
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
            if (IsDummyGrabbableParent)
            {
                // Dummy grabbable parent objects will 99.99% rotate around pivot
                return UxrRotationProvider.HandPositionAroundPivot;
            }

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

        /// <summary>
        ///     Stops a smooth manipulation transition.
        /// </summary>
        public void StopSmoothManipulationTransition()
        {
            SmoothManipulationTimer = -1.0f;
        }

        /// <summary>
        ///     Finishes all smooth transitions (manipulation, placing or constraining).
        ///     Use this when applying custom translation/rotation to a grabbable object without transitions getting in the way.
        /// </summary>
        public void FinishSmoothTransitions()
        {
            StopSmoothManipulationTransition();
            StopSmoothConstrain();
            StopSmoothAnchorPlacement();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Checks whether the given snap mode affects the object position. This only references if the object position is
        ///     going to change in order to snap to the hand, not whether the object itself can be moved while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object position move to the snap position</returns>
        internal static bool GetSnapModeAffectsPosition(UxrSnapToHandMode snapMode)
        {
            return snapMode != UxrSnapToHandMode.DontSnap && snapMode != UxrSnapToHandMode.RotationOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object position. This only references if the object position is
        ///     going to change in order to snap to the hand, not whether the object itself can be moved while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object position move to the snap position</returns>
        internal static bool GetSnapModeAffectsPosition(UxrSnapToAnchorMode snapMode)
        {
            return snapMode != UxrSnapToAnchorMode.DontSnap && snapMode != UxrSnapToAnchorMode.RotationOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object rotation. This only references if the object rotation is
        ///     going to change in order to snap to the hand, not whether the object itself can be rotated while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object rotate to the snap orientation</returns>
        internal static bool GetSnapModeAffectsRotation(UxrSnapToHandMode snapMode)
        {
            return snapMode != UxrSnapToHandMode.DontSnap && snapMode != UxrSnapToHandMode.PositionOnly;
        }

        /// <summary>
        ///     Checks whether the given snap mode affects the object rotation. This only references if the object rotation is
        ///     going to change in order to snap to the hand, not whether the object itself can be rotated while grabbed.
        /// </summary>
        /// <param name="snapMode">Snap mode</param>
        /// <returns>Whether the given snap mode will make the object rotate to the snap orientation</returns>
        internal static bool GetSnapModeAffectsRotation(UxrSnapToAnchorMode snapMode)
        {
            return snapMode != UxrSnapToAnchorMode.DontSnap && snapMode != UxrSnapToAnchorMode.PositionOnly;
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
        ///     Updates parent and children information on this grabbable and all other grabbable objects in same the hierarchy, up
        ///     or down.
        /// </summary>
        internal void UpdateGrabbableDependencies()
        {
            static void UpdateGrabbableDependenciesRecursive(UxrGrabbableObject grabbableObject, ref List<UxrGrabbableObject> processed)
            {
                if (!processed.Contains(grabbableObject))
                {
                    processed.Add(grabbableObject);

                    grabbableObject.AllParents            = GetParents(grabbableObject, false).ToList();
                    grabbableObject.ParentLookAts         = GetParents(grabbableObject, true).ToList();
                    grabbableObject.AllChildren           = GetChildren(grabbableObject, false, false, false).ToList();
                    grabbableObject.AllChildrenLookAts    = GetChildren(grabbableObject, false, true,  true).ToList();
                    grabbableObject.DirectChildrenLookAts = GetChildren(grabbableObject, true,  true,  true).ToList();

                    foreach (UxrGrabbableObject parent in grabbableObject.ParentLookAts)
                    {
                        UpdateGrabbableDependenciesRecursive(parent, ref processed);
                    }

                    foreach (UxrGrabbableObject child in grabbableObject.AllChildren)
                    {
                        UpdateGrabbableDependenciesRecursive(child, ref processed);
                    }
                }
            }

            List<UxrGrabbableObject> processedElements = new List<UxrGrabbableObject>();

            UpdateGrabbableDependenciesRecursive(this, ref processedElements);
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
        ///     Gets the <see cref="Transform" /> that is used to compute the snap to an <see cref="UxrAvatar" />'s hand (
        ///     <see cref="UxrGrabber" />) for a given grab point when it is grabbed.
        /// </summary>
        /// <param name="avatar">Avatar to compute the alignment for</param>
        /// <param name="grabPoint">Grab point to get the alignment of</param>
        /// <param name="handSide">The hand to get the snap transform for</param>
        /// <returns><see cref="Transform" /> that should align to the grabber when it is being grabbed</returns>
        internal Transform GetGrabPointGrabAlignTransform(UxrAvatar avatar, int grabPoint, UxrHandSide handSide)
        {
            UxrGrabPointInfo grabPointInfo = GetGrabPoint(grabPoint);

            if (grabPointInfo == null)
            {
                Debug.LogWarning($"{UxrConstants.ManipulationModule}: Object " + name + $" has no grab point info for index {grabPoint}");
                return transform;
            }

            UxrGripPoseInfo gripPoseInfo = grabPointInfo.GetGripPoseInfo(avatar);

            if (gripPoseInfo == null)
            {
                Debug.LogWarning($"{UxrConstants.ManipulationModule}: Object " + name + $" has no grip pose info for avatar {avatar.name}");
                return transform;
            }

            if (handSide == UxrHandSide.Left)
            {
                if (gripPoseInfo.GripAlignTransformHandLeft == null || grabPointInfo.SnapReference != UxrSnapReference.UseOtherTransform)
                {
                    return transform;
                }

                return gripPoseInfo.GripAlignTransformHandLeft;
            }

            if (gripPoseInfo.GripAlignTransformHandRight == null || grabPointInfo.SnapReference != UxrSnapReference.UseOtherTransform)
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
        ///     Notifies that the object just started to be grabbed.
        /// </summary>
        /// <param name="grabber">Grabber responsible for grabbing the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        /// <param name="snapPosition">The grabber snap position to use</param>
        /// <param name="snapRotation">The grabber snap rotation to use</param>
        internal void NotifyBeginGrab(UxrGrabber grabber, int grabPoint, Vector3 snapPosition, Quaternion snapRotation)
        {
        }

        /// <summary>
        ///     Notifies that the object just stopped being grabbed.
        /// </summary>
        /// <param name="grabber">Grabber responsible for grabbing the object</param>
        /// <param name="grabPoint">Point that was grabbed</param>
        internal void NotifyEndGrab(UxrGrabber grabber, int grabPoint)
        {
        }

        /// <summary>
        ///     Computes, if the object has the AutoRotationProvider set, the rotation provider based on the grab snap position.
        /// </summary>
        /// <param name="snapPosition">Grab snap position</param>
        internal void CheckComputeAutoRotationProvider(Vector3 snapPosition)
        {
            if (_autoRotationProvider)
            {
                _rotationProvider = GetAutoRotationProvider(snapPosition);
            }
        }

        /// <summary>
        ///     Starts a smooth manipulation transition.
        /// </summary>
        internal void StartSmoothManipulationTransition()
        {
            SmoothManipulationTimer              = UxrConstants.SmoothManipulationTransitionSeconds;
            SmoothManipulationLocalPositionStart = transform.localPosition;
            SmoothManipulationLocalRotationStart = transform.localRotation;
            SmoothManipulationReference          = transform.parent;
        }

        /// <summary>
        ///     Updates the smooth manipulation transitions if they exist.
        /// </summary>
        internal void UpdateSmoothManipulationTransition(float deltaTime)
        {
            if (SmoothManipulationTimer < 0.0f)
            {
                return;
            }
            
            SmoothManipulationTimer -= deltaTime;

            if (SmoothManipulationTimer > 0.0f)
            {
                float t = SmoothManipulationTimer <= 0.0f ? 1.0f : 1.0f - Mathf.Clamp01(SmoothManipulationTimer / UxrConstants.SmoothManipulationTransitionSeconds);

                if (transform.parent == SmoothManipulationReference || SmoothManipulationReference == null)
                {
                    transform.localPosition = Vector3.Lerp(SmoothManipulationLocalPositionStart, transform.localPosition, t);
                    transform.localRotation = Quaternion.Slerp(SmoothManipulationLocalRotationStart, transform.localRotation, t);
                }
                else
                {
                    Vector3    posStart = TransformExt.GetWorldPosition(SmoothManipulationReference, SmoothManipulationLocalPositionStart);
                    Vector3    posEnd   = transform.position;
                    Quaternion rotStart = TransformExt.GetWorldRotation(SmoothManipulationReference, SmoothManipulationLocalRotationStart);
                    Quaternion rotEnd   = transform.rotation;

                    transform.position = Vector3.Lerp(posStart, posEnd, t);
                    transform.rotation = Quaternion.Slerp(rotStart, rotEnd, t);
                }
            }
        }

        /// <summary>
        ///     Starts a smooth constraining transition.
        /// </summary>
        internal void StartSmoothConstrain()
        {
            SmoothConstrainTimer              = UxrConstants.SmoothManipulationTransitionSeconds;
            SmoothConstrainLocalPositionStart = transform.localPosition;
            SmoothConstrainLocalRotationStart = transform.localRotation;
            SmoothConstrainReference          = transform.parent;
        }

        /// <summary>
        ///     Stops a smooth constraining transition if there is one.
        /// </summary>
        internal void StopSmoothConstrain()
        {
            SmoothConstrainTimer = -1.0f;
        }

        /// <summary>
        ///     Updates the smooth constraining transition if there is one.
        /// </summary>
        internal void UpdateSmoothConstrainTransition(float deltaTime)
        {
            if (SmoothConstrainTimer < 0.0f)
            {
                return;
            }

            SmoothConstrainTimer -= deltaTime;

            float t = 1.0f - Mathf.Clamp01(SmoothConstrainTimer / UxrConstants.SmoothManipulationTransitionSeconds);

            if (transform.parent == SmoothConstrainReference || SmoothConstrainReference == null)
            {
                transform.localPosition = Vector3.Lerp(SmoothConstrainLocalPositionStart, transform.localPosition, t);
                transform.localRotation = Quaternion.Slerp(SmoothConstrainLocalRotationStart, transform.localRotation, t);
            }
            else
            {
                Vector3    posStart = TransformExt.GetWorldPosition(SmoothConstrainReference, SmoothConstrainLocalPositionStart);
                Vector3    posEnd   = transform.position;
                Quaternion rotStart = TransformExt.GetWorldRotation(SmoothConstrainReference, SmoothConstrainLocalRotationStart);
                Quaternion rotEnd   = transform.rotation;

                transform.position = Vector3.Lerp(posStart, posEnd, t);
                transform.rotation = Quaternion.Slerp(rotStart, rotEnd, t);
            }
        }

        /// <summary>
        ///     Starts a smooth transition to the object placement.
        /// </summary>
        internal void StartSmoothAnchorPlacement()
        {
            if (_dropSnapMode != UxrSnapToAnchorMode.DontSnap && CurrentAnchor != null)
            {
                SmoothPlacementTimer              = UxrConstants.SmoothManipulationTransitionSeconds;
                SmoothPlacementLocalPositionStart = transform.localPosition;
                SmoothPlacementLocalRotationStart = transform.localRotation;
                SmoothPlacementReference          = transform.parent;
            }
        }

        /// <summary>
        ///     Stops the smooth anchor placement transition if there is one.
        /// </summary>
        internal void StopSmoothAnchorPlacement()
        {
            SmoothPlacementTimer = -1.0f;
        }

        /// <summary>
        ///     Updates the smooth anchor placement transition if there is one.
        /// </summary>
        internal void UpdateSmoothAnchorPlacement(float deltaTime)
        {
            if (SmoothPlacementTimer < 0.0f || CurrentAnchor == null)
            {
                return;
            }

            // Smooth placement transitions

            SmoothPlacementTimer -= deltaTime;

            float      t              = 1.0f - Mathf.Clamp01(SmoothPlacementTimer / UxrConstants.SmoothManipulationTransitionSeconds);
            Vector3    targetPosition = transform.position;
            Quaternion targetRotation = transform.rotation;
            TransformExt.ApplyAlignment(ref targetPosition, ref targetRotation, DropAlignTransform.position, DropAlignTransform.rotation, CurrentAnchor.AlignTransform.position, CurrentAnchor.AlignTransform.rotation);

            if (GetSnapModeAffectsPosition(DropSnapMode) || PlacementOptions.HasFlag(UxrPlacementOptions.ForceSnapPosition))
            {
                if (transform.parent == CurrentAnchor.transform)
                {
                    Vector3 targetLocalPosition = TransformExt.GetLocalPosition(transform.parent, targetPosition);
                    transform.localPosition = Vector3.Lerp(SmoothPlacementLocalPositionStart, targetLocalPosition, t);
                }
                else
                {
                    Vector3 posStart = TransformExt.GetWorldPosition(SmoothPlacementReference, SmoothPlacementLocalPositionStart);
                    transform.position = Vector3.Lerp(posStart, targetPosition, t);
                }
            }

            if (GetSnapModeAffectsRotation(DropSnapMode) || PlacementOptions.HasFlag(UxrPlacementOptions.ForceSnapRotation))
            {
                if (transform.parent == CurrentAnchor.transform)
                {
                    Quaternion targetLocalRotation = TransformExt.GetLocalRotation(transform.parent, targetRotation);
                    transform.localRotation = Quaternion.Slerp(SmoothPlacementLocalRotationStart, targetLocalRotation, t);
                }
                else
                {
                    Quaternion rotStart = TransformExt.GetWorldRotation(SmoothPlacementReference, SmoothPlacementLocalRotationStart);
                    transform.rotation = Quaternion.Slerp(rotStart, targetRotation, t);
                }
            }

            if (SmoothPlacementTimer <= 0.0f)
            {
                CurrentAnchor.RaiseSmoothTransitionPlaceEnded();
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

                // Make sure to generate same unique ID for the new anchor to support multiplayer, state save, etc. 
                newAnchor.ChangeUniqueId(GuidExt.Combine(UniqueId, newAnchor.name.GetGuid()));
                newAnchor.MaxPlaceDistance = _autoAnchorMaxPlaceDistance;

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
                    Debug.LogWarning($"{UxrConstants.ManipulationModule}: Object " + name + " has duplicated GrabPointShape for " + UxrGrabPointIndex.GetIndexDisplayName(this, grabPointShape.GrabPoint));
                }
                else
                {
                    _grabPointShapes.Add(grabPointShape.GrabPoint, grabPointShape);
                }
            }

            if (GrabbableParent != null)
            {
                InitialPositionInLocalGrabbableParent = GrabbableParent.transform.InverseTransformPoint(transform.position);
            }

            UpdateGrabbableDependencies();
        }

        /// <summary>
        ///     Called when the object is destroyed. Checks whether it is being grabbed to release it.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsBeingGrabbed)
            {
                UxrGrabManager.Instance.ReleaseGrabs(this, true);
            }
        }

        /// <summary>
        ///     Called when the component is disabled. Stops all transitions.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            SmoothManipulationTimer = -1.0f;
            SmoothPlacementTimer    = -1.0f;
            SmoothConstrainTimer    = -1.0f;
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

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that will keep manually in sync grabbable object rigidbodies without native network synchronization
        ///     components.
        /// </summary>
        /// <param name="grabber">The grabber that released the object</param>
        /// <param name="intervalSeconds">Interval in seconds to send sync messages</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator RegularPhysicsSyncCoroutine(UxrGrabber grabber, float intervalSeconds)
        {
            while (_rigidBodySource != null)
            {
                bool isSleeping = _rigidBodySource.IsSleeping();

                UpdateRigidbody(grabber, isSleeping, transform.position, transform.rotation, _rigidBodySource.velocity, _rigidBodySource.angularVelocity);

                if (isSleeping)
                {
                    break;
                }

                yield return new WaitForSeconds(intervalSeconds);
            }

            _regularPhysicsSyncCoroutine = null;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="ConstraintsApplying" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseConstraintsApplying(UxrApplyConstraintsEventArgs e)
        {
            if (UxrGrabManager.Instance.Features.HasFlag(UxrManipulationFeatures.UserConstraints))
            {
                ConstraintsApplying?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ConstraintsApplied" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseConstraintsApplied(UxrApplyConstraintsEventArgs e)
        {
            if (UxrGrabManager.Instance.Features.HasFlag(UxrManipulationFeatures.UserConstraints))
            {
                ConstraintsApplied?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ConstraintsFinished" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseConstraintsFinished(UxrApplyConstraintsEventArgs e)
        {
            if (UxrGrabManager.Instance.Features.HasFlag(UxrManipulationFeatures.UserConstraints))
            {
                ConstraintsFinished?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="Grabbing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseGrabbingEvent(UxrManipulationEventArgs e)
        {
            if (_regularPhysicsSyncCoroutine != null)
            {
                StopCoroutine(_regularPhysicsSyncCoroutine);
                _regularPhysicsSyncCoroutine = null;
            }

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
            // Check whether the object was released, is in a networking environment and requires manually keep physics in sync through messages

            if (UxrGlobalSettings.Instance.SyncGrabbablePhysics &&
                _rigidBodySource != null &&
                _rigidBodyDynamicOnRelease &&
                !IsBeingGrabbed &&
                e.Grabber != null &&
                e.Grabber.Avatar.GetComponent<IUxrNetworkAvatar>() is IUxrNetworkAvatar networkAvatar &&
                networkAvatar.IsLocal &&
                UxrNetworkManager.HasInstance &&
                !UxrNetworkManager.Instance.NetworkImplementation.HasNetworkTransformSyncComponents(gameObject))
            {
                if (_regularPhysicsSyncCoroutine != null)
                {
                    StopCoroutine(_regularPhysicsSyncCoroutine);
                }

                _regularPhysicsSyncCoroutine = StartCoroutine(RegularPhysicsSyncCoroutine(e.Grabber, UxrGlobalSettings.Instance.GrabbableSyncIntervalSeconds));
            }

            Released?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Placing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaisePlacingEvent(UxrManipulationEventArgs e)
        {
            if (_regularPhysicsSyncCoroutine != null)
            {
                StopCoroutine(_regularPhysicsSyncCoroutine);
                _regularPhysicsSyncCoroutine = null;
            }

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
        ///     Gets a <see cref="UxrGrabbableObject" />'s list of parents whose direction can be controlled
        ///     indirectly by a grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="onlyControlDirection">
        ///     Whether to return only the chain to the first parent that is not controlled by using
        ///     <see cref="UsesGrabbableParentDependency" /> and <see cref="ControlParentDirection" />
        /// </param>
        /// <returns>Bottom-to-top sorted list of parents</returns>
        private static IEnumerable<UxrGrabbableObject> GetParents(UxrGrabbableObject grabbableObject, bool onlyControlDirection)
        {
            if (grabbableObject == null)
            {
                yield break;
            }

            UxrGrabbableObject current = grabbableObject;
            UxrGrabbableObject parent  = current.GrabbableParent;

            while (current != null && parent != null)
            {
                bool valid = !onlyControlDirection || (current.UsesGrabbableParentDependency && current.ControlParentDirection);

                if (valid)
                {
                    yield return parent;
                }
                else
                {
                    yield break;
                }

                current = parent;
                parent  = parent.GrabbableParent;
            }
        }

        /// <summary>
        ///     Gets a <see cref="UxrGrabbableObject" />'s list of children.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="onlyDirect">
        ///     Whether to filter children that have no other grabbable between the child and the grabbable
        ///     object
        /// </param>
        /// <param name="onlyUseDependency">
        ///     Whether to filter children that have <see cref="UsesGrabbableParentDependency" />
        /// </param>
        /// <param name="onlyControlDirection">Whether to filter children that have <see cref="ControlParentDirection" /> </param>
        /// <returns>List of children that control the parent grabbable direction</returns>
        private static IEnumerable<UxrGrabbableObject> GetChildren(UxrGrabbableObject grabbableObject, bool onlyDirect, bool onlyUseDependency, bool onlyControlDirection)
        {
            if (grabbableObject == null)
            {
                yield break;
            }

            UxrGrabbableObject[] children = grabbableObject.GetComponentsInChildren<UxrGrabbableObject>();

            foreach (UxrGrabbableObject child in children)
            {
                bool validControl = !(onlyUseDependency && !child.UsesGrabbableParentDependency);

                if (onlyControlDirection && !child.ControlParentDirection)
                {
                    validControl = false;
                }

                if (child != grabbableObject && validControl)
                {
                    if (onlyDirect)
                    {
                        if (child.GrabbableParent == grabbableObject)
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the rigidbody making sure the messages are synchronized through the network.
        /// </summary>
        /// <param name="grabber">The grabber that released the object</param>
        /// <param name="isSleeping">Whether the rigidbody is sleeping</param>
        /// <param name="transformPosition">The rigidbody position</param>
        /// <param name="transformRotation">The rigidbody rotation</param>
        /// <param name="velocity">The rigidbody velocity</param>
        /// <param name="angularVelocity">The rigidbody angular velocity</param>
        private void UpdateRigidbody(UxrGrabber grabber, bool isSleeping, Vector3 transformPosition, Quaternion transformRotation, Vector3 velocity, Vector3 angularVelocity)
        {
            if (!_rigidBodySource)
            {
                return;
            }

            bool isLocal = grabber != null && grabber.Avatar != null && grabber.Avatar.GetComponent<IUxrNetworkAvatar>() is IUxrNetworkAvatar networkAvatar && networkAvatar.IsLocal;

            // Only sync in network, we don't use it anywhere else.
            BeginSync(UxrStateSyncOptions.Network);

            if (!isLocal)
            {
                _rigidBodySource.transform.position = transformPosition;
                _rigidBodySource.transform.rotation = transformRotation;
                _rigidBodySource.velocity           = velocity;
                _rigidBodySource.angularVelocity    = angularVelocity;

                if (isSleeping)
                {
                    _rigidBodySource.Sleep();
                }
            }

            EndSyncMethod(new object[] { grabber, isSleeping, transformPosition, transformRotation, velocity, angularVelocity });
        }

        /// <summary>
        ///     Gets the first <see cref="UxrGrabbableObject" /> upwards in the hierarchy.
        /// </summary>
        /// <param name="grabbableTransform"><see cref="UxrGrabbableObject" /> to get the grabbable parent of</param>
        /// <returns><see cref="UxrGrabbableObject" /> upwards in the hierarchy</returns>
        private UxrGrabbableObject GetGrabbableParent(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject.transform.parent != null)
            {
                UxrGrabbableObject parentGrabbableObject = grabbableObject.transform.parent.GetComponentInParent<UxrGrabbableObject>();

                if (parentGrabbableObject != null)
                {
                    return parentGrabbableObject;
                }
            }

            return null;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets or sets the grabbable object's local position at the beginning of a smooth transition.
        /// </summary>
        private Vector3 SmoothManipulationLocalPositionStart { get; set; }

        /// <summary>
        ///     Gets or sets the grabbable object's local rotation at the beginning of a smooth transition.
        /// </summary>
        private Quaternion SmoothManipulationLocalRotationStart { get; set; }

        /// <summary>
        ///     Gets or sets the reference for <see cref="SmoothManipulationLocalPositionStart"/> and <see cref="SmoothManipulationLocalRotationStart"/>.
        /// </summary>
        private Transform SmoothManipulationReference { get; set; }

        /// <summary>
        ///     Local position when the smooth object placement started.
        /// </summary>
        private Vector3 SmoothPlacementLocalPositionStart { get; set; }

        /// <summary>
        ///     Local rotation when the smooth object placement started.
        /// </summary>
        private Quaternion SmoothPlacementLocalRotationStart { get; set; }

        /// <summary>
        ///     Gets or sets the reference for <see cref="SmoothPlacementLocalPositionStart"/> and <see cref="SmoothPlacementLocalRotationStart"/>.
        /// </summary>
        private Transform SmoothPlacementReference { get; set; }

        /// <summary>
        ///     Local position at the beginning of a smooth constrain transition.
        /// </summary>
        private Vector3 SmoothConstrainLocalPositionStart { get; set; }

        /// <summary>
        ///     Local rotation at the beginning of a smooth constrain transition.
        /// </summary>
        private Quaternion SmoothConstrainLocalRotationStart { get; set; }

        /// <summary>
        ///     Gets or sets the reference for <see cref="SmoothConstrainLocalPositionStart"/> and <see cref="SmoothConstrainLocalRotationStart"/>.
        /// </summary>
        private Transform SmoothConstrainReference { get; set; }

        /// <summary>
        ///     Gets or sets the decreasing timer that is initialized at
        ///     <see cref="UxrConstants.SmoothManipulationTransitionSeconds" />
        ///     for smooth manipulation transitions.
        /// </summary>
        private float SmoothManipulationTimer { get; set; } = -1.0f;

        /// <summary>
        ///     Gets or sets the decreasing placement timer for smooth placement transitions.
        /// </summary>
        private float SmoothPlacementTimer { get; set; } = -1.0f;

        /// <summary>
        ///     Gets or sets the decreasing constrain timer for smooth constraint transitions.
        /// </summary>
        private float SmoothConstrainTimer { get; set; } = -1.0f;

        // Backing fields for public properties

        private UxrGrabbableObjectAnchor _currentAnchor;
        private bool                     _isPlaceable = true;
        private bool                     _isLockedInPlace;

        // Private vars from internal properties

        private float               _singleRotationAngleCumulative;
        private UxrPlacementOptions _placementOptions;

        // Backing fields for IUxrGrabbable

        private bool _isGrabbable = true;

        // Private vars

        private Dictionary<int, bool>              _grabPointEnabledStates = new Dictionary<int, bool>();
        private bool                               _initialIsKinematic     = true;
        private Dictionary<int, UxrGrabPointShape> _grabPointShapes        = new Dictionary<int, UxrGrabPointShape>();

        // Coroutines

        private Coroutine _regularPhysicsSyncCoroutine;

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