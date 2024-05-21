// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.RuntimeGrabInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.StateSave;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information of a grab performed on an object.
        /// </summary>
        /// <remarks>
        ///     Implements <see cref="IUxrSerializable" /> to help <see cref="UxrGrabManager" />'s implementation of the
        ///     <see cref="IUxrStateSave" /> interface (<see cref="UxrGrabManager.SerializeState" />).
        /// </remarks>
        private sealed class RuntimeGrabInfo : IUxrSerializable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the grabber grabbing the <see cref="UxrGrabbableObject" />.
            /// </summary>
            public UxrGrabber Grabber
            {
                get => _grabber;
                set => _grabber = value;
            }

            /// <summary>
            ///     Gets the <see cref="UxrGrabbableObject" /> grabbed point.
            /// </summary>
            public int GrabbedPoint
            {
                get => _grabbedPoint;
                set => _grabbedPoint = value;
            }

            // *************************************************************************************************************************
            // Transform information about the grip.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> rotation relative to the <see cref="UxrGrabber" /> at the moment
            ///     it was grabbed.
            /// </summary>
            public Quaternion RelativeGrabRotation
            {
                get => _relativeGrabRotation;
                private set => _relativeGrabRotation = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> position in local <see cref="UxrGrabber" /> space at the moment
            ///     it was grabbed.
            /// </summary>
            public Vector3 RelativeGrabPosition
            {
                get => _relativeGrabPosition;
                private set => _relativeGrabPosition = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> rotation relative to the <see cref="UxrGrabbableObject" /> at the moment
            ///     it was grabbed.
            /// </summary>
            public Quaternion RelativeGrabberRotation
            {
                get => _relativeGrabberRotation;
                private set => _relativeGrabberRotation = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> position in local <see cref="UxrGrabbableObject" /> space at the moment
            ///     it was grabbed.
            /// </summary>
            public Vector3 RelativeGrabberPosition
            {
                get => _relativeGrabberPosition;
                private set => _relativeGrabberPosition = value;
            }

            /// <summary>
            ///     Gets or sets the transform relative to which <see cref="RelativeGrabAlignPosition" /> and
            ///     <see cref="RelativeGrabAlignRotation" /> are specified.
            /// </summary>
            public Transform GrabAlignParentTransformUsed
            {
                get => _grabAlignParentTransformUsed;
                private set => _grabAlignParentTransformUsed = value;
            }

            /// <summary>
            ///     Gets or sets the snap rotation relative to the <see cref="GrabAlignParentTransformUsed" /> at the moment it was
            ///     grabbed.
            /// </summary>
            public Quaternion RelativeGrabAlignRotation
            {
                get => _relativeGrabAlignRotation;
                private set => _relativeGrabAlignRotation = value;
            }

            /// <summary>
            ///     Gets or sets the snap position in local <see cref="GrabAlignParentTransformUsed" /> space at the moment it was
            ///     grabbed.
            /// </summary>
            public Vector3 RelativeGrabAlignPosition
            {
                get => _relativeGrabAlignPosition;
                private set => _relativeGrabAlignPosition = value;
            }

            /// <summary>
            ///     Gets or sets the computed snap rotation relative to the object's snap rotation, which might be different if
            ///     the computed snap rotation came from an <see cref="UxrGrabPointShape" />.
            /// </summary>
            public Quaternion RelativeUsedGrabAlignRotation
            {
                get => _relativeUsedGrabAlignRotation;
                private set => _relativeUsedGrabAlignRotation = value;
            }

            /// <summary>
            ///     Gets or sets the computed snap position relative to the object's snap position, which might be different if
            ///     the computed snap position came from an <see cref="UxrGrabPointShape" />.
            /// </summary>
            public Vector3 RelativeUsedGrabAlignPosition
            {
                get => _relativeUsedGrabAlignPosition;
                private set => _relativeUsedGrabAlignPosition = value;
            }

            /// <summary>
            ///     Gets or sets the proximity rotation relative to the <see cref="UxrGrabbableObject" /> at the moment it was grabbed.
            /// </summary>
            public Vector3 RelativeProximityPosition
            {
                get => _relativeProximityPosition;
                private set => _relativeProximityPosition = value;
            }

            /// <summary>
            ///     Gets or sets the source in local <see cref="UxrGrabber" /> coordinates where the source of leverage will be
            ///     computed for <see cref="UxrRotationProvider.HandPositionAroundPivot" /> manipulation. This will improve rotation
            ///     behaviour when the hands are rotated because otherwise the source of leverage is the grabber itself and rotating
            ///     the hand will keep the grabber more or less stationary.
            /// </summary>
            public Vector3 GrabberLocalLeverageSource
            {
                get => _grabberLocalLeverageSource;
                private set => _grabberLocalLeverageSource = value;
            }

            /// <summary>
            ///     Gets or sets the leverage source <see cref="GrabberLocalLeverageSource" /> in local coordinates of the parent
            ///     transform of the grabbable at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Vector3 GrabberLocalParentLeverageSourceOnGrab
            {
                get => _grabberLocalParentLeverageSourceOnGrab;
                private set => _grabberLocalParentLeverageSourceOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the leverage point in local coordinates that this child grabbable will use when the parent
            ///     grabbable rotation provider is HandPositionAroundPivot.
            /// </summary>
            public Vector3 ParentGrabbableLookAtLocalLeveragePoint
            {
                get => _parentGrabbableLookAtLocalLeveragePoint;
                set => _parentGrabbableLookAtLocalLeveragePoint = value;
            }

            /// <summary>
            ///     Gets or sets the leverage point in local grabbable parent coordinates that this child grabbable will use
            ///     when the parent grabbable rotation provider is HandPositionAroundPivot.
            /// </summary>
            public Vector3 ParentGrabbableLookAtParentLeveragePoint
            {
                get => _parentGrabbableLookAtParentLeveragePoint;
                set => _parentGrabbableLookAtParentLeveragePoint = value;
            }

            /// <summary>
            ///     Gets or sets the look-at contribution in world coordinates of this child grabbable object to the parent
            ///     grabbable look-at algorithm for the current frame. Only for HandPositionAroundPivot in parent grabbable objects.
            /// </summary>
            public Vector3 ParentGrabbableLeverageContribution
            {
                get => _parentGrabbableLeverageContribution;
                set => _parentGrabbableLeverageContribution = value;
            }

            /// <summary>
            ///     Gets or sets the rotation contribution of this object to the parent grabbable look-at algorithm for the
            ///     current frame. Only for HandPositionAroundPivot in parent grabbable objects.
            /// </summary>
            public Quaternion ParentGrabbableLookAtRotationContribution
            {
                get => _parentGrabbableLookAtRotationContribution;
                set => _parentGrabbableLookAtRotationContribution = value;
            }

            /// <summary>
            ///     Gets or sets the rotation angle contribution, in objects constrained to a single axis rotation, during the current
            ///     grab.
            /// </summary>
            public float SingleRotationAngleContribution
            {
                get => _singleRotationAngleContribution;
                set => _singleRotationAngleContribution = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="SingleRotationAngleContribution" /> value the last time it was accumulated into
            ///     the object internal angle. This allows angle contributions to work using absolute values instead of delta values
            ///     to have better precision.
            /// </summary>
            public float LastAccumulatedAngle
            {
                get => _lastAccumulatedAngle;
                set => _lastAccumulatedAngle = value;
            }

            // *************************************************************************************************************************
            // Parent dependency information.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets the grab position in grabbable parent space before updating this object being grabbed. It is used to compute
            ///     the lookAt contribution of this grab on the parent, when the parent is being grabbed. The grab position
            ///     is computed before constraints are applied to the object to compute the contribution correctly.
            /// </summary>
            public Vector3 ParentLocalGrabPositionBeforeUpdate
            {
                get => _parentLocalGrabPositionBeforeUpdate;
                set => _parentLocalGrabPositionBeforeUpdate = value;
            }

            /// <summary>
            ///     Gets the grab position in grabbable parent space after updating this object being grabbed. See
            ///     <see cref="ParentLocalGrabPositionBeforeUpdate" />.
            /// </summary>
            public Vector3 ParentLocalGrabPositionAfterUpdate
            {
                get => _parentLocalGrabPositionAfterUpdate;
                set => _parentLocalGrabPositionAfterUpdate = value;
            }

            /// <summary>
            ///     Gets the grabbable parent position in local grabbable child space before updating the child being grabbed.
            ///     It is used to compute the contribution of a child on a parent when the parent is not being grabbed.
            /// </summary>
            public Vector3 ChildLocalParentPosition
            {
                get => _childLocalParentPosition;
                set => _childLocalParentPosition = value;
            }

            /// <summary>
            ///     Gets the grabbable parent rotation in local grabbable child space before updating the child being grabbed.
            ///     It is used to compute the contribution of a child on a parent when the parent is not being grabbed.
            /// </summary>
            public Quaternion ChildLocalParentRotation
            {
                get => _childLocalParentRotation;
                set => _childLocalParentRotation = value;
            }

            // *************************************************************************************************************************
            // For smooth transitions from object to hand or object to target or hand to object where we want to avoid instant snapping.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> local position at the moment it was grabbed.
            /// </summary>
            public Vector3 LocalPositionOnGrab
            {
                get => _localPositionOnGrab;
                private set => _localPositionOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> local rotation at the moment it was grabbed.
            /// </summary>
            public Quaternion LocalRotationOnGrab
            {
                get => _localRotationOnGrab;
                private set => _localRotationOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the world-space snap position at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Vector3 AlignPositionOnGrab
            {
                get => _alignPositionOnGrab;
                private set => _alignPositionOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the world-space snap rotation at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Quaternion AlignRotationOnGrab
            {
                get => _alignRotationOnGrab;
                private set => _alignRotationOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the hand bone position in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
            ///     was grabbed.
            /// </summary>
            public Vector3 HandBoneLocalAvatarPositionOnGrab
            {
                get => _handBoneLocalAvatarPositionOnGrab;
                private set => _handBoneLocalAvatarPositionOnGrab = value;
            }

            /// <summary>
            ///     Gets or sets the hand bone rotation in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
            ///     was grabbed.
            /// </summary>
            public Quaternion HandBoneLocalAvatarRotationOnGrab
            {
                get => _handBoneLocalAvatarRotationOnGrab;
                private set => _handBoneLocalAvatarRotationOnGrab = value;
            }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="grabber">The grabber</param>
            /// <param name="grabbedPoint">The grabbed point</param>
            public RuntimeGrabInfo(UxrGrabber grabber, int grabbedPoint)
            {
                Grabber      = grabber;
                GrabbedPoint = grabbedPoint;
            }

            /// <summary>
            ///     Default constructor required for serialization.
            /// </summary>
            private RuntimeGrabInfo()
            {
                
            }

            #endregion

            #region Implicit IUxrSerializable

            /// <inheritdoc />
            public int SerializationVersion => 0;

            /// <inheritdoc />
            public void Serialize(IUxrSerializer serializer, int serializationVersion)
            {
                serializer.SerializeUniqueComponent(ref _grabber);
                serializer.Serialize(ref _grabbedPoint);
                serializer.Serialize(ref _relativeGrabRotation);
                serializer.Serialize(ref _relativeGrabPosition);
                serializer.Serialize(ref _relativeGrabberRotation);
                serializer.Serialize(ref _relativeGrabberPosition);

                // Trick to be able to restore _grabAlignParentTransformUsed because we can't serialize a reference without IUxrUniqueID
                {
                    serializer.SerializeUniqueComponent(ref _grabbableObject);

                    if (serializer.IsReading && _grabbableObject)
                    {
                        Transform grabAlignTransform = _grabbableObject.GetGrabPointGrabAlignTransform(_grabber.Avatar, _grabbedPoint, _grabber.Side);
                        GrabAlignParentTransformUsed = grabAlignTransform == _grabbableObject.transform ? _grabbableObject.transform : grabAlignTransform.parent;
                    }
                }

                serializer.Serialize(ref _relativeGrabAlignRotation);
                serializer.Serialize(ref _relativeGrabAlignPosition);
                serializer.Serialize(ref _relativeUsedGrabAlignRotation);
                serializer.Serialize(ref _relativeUsedGrabAlignPosition);
                serializer.Serialize(ref _relativeProximityPosition);
                serializer.Serialize(ref _grabberLocalLeverageSource);
                serializer.Serialize(ref _grabberLocalParentLeverageSourceOnGrab);
                serializer.Serialize(ref _parentGrabbableLookAtLocalLeveragePoint);
                serializer.Serialize(ref _parentGrabbableLookAtParentLeveragePoint);
                serializer.Serialize(ref _parentGrabbableLeverageContribution);
                serializer.Serialize(ref _parentGrabbableLookAtRotationContribution);
                serializer.Serialize(ref _singleRotationAngleContribution);
                serializer.Serialize(ref _lastAccumulatedAngle);
                serializer.Serialize(ref _parentLocalGrabPositionBeforeUpdate);
                serializer.Serialize(ref _parentLocalGrabPositionAfterUpdate);
                serializer.Serialize(ref _childLocalParentPosition);
                serializer.Serialize(ref _childLocalParentRotation);
                serializer.Serialize(ref _localPositionOnGrab);
                serializer.Serialize(ref _localRotationOnGrab);
                serializer.Serialize(ref _alignPositionOnGrab);
                serializer.Serialize(ref _alignRotationOnGrab);
                serializer.Serialize(ref _handBoneLocalAvatarPositionOnGrab);
                serializer.Serialize(ref _handBoneLocalAvatarRotationOnGrab);
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Computes the grab information.
            /// </summary>
            /// <param name="grabber">Grabber responsible for grabbing the object</param>
            /// <param name="grabbableObject">The object being grabbed</param>
            /// <param name="grabPoint">Point that was grabbed</param>
            /// <param name="snapPosition">The grabber snap position to use</param>
            /// <param name="snapRotation">The grabber snap rotation to use</param>
            /// <param name="sourceGrabEventArgs">
            ///     If non-null, the grab will use the information on the event to ensure that
            ///     it is performed in exactly the same way. This is used in multi-player environments.
            /// </param>
            public void Compute(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint, Vector3 snapPosition, Quaternion snapRotation, UxrManipulationEventArgs sourceGrabEventArgs = null)
            {
                Transform  grabAlignTransform = grabbableObject.GetGrabPointGrabAlignTransform(grabber.Avatar, grabPoint, grabber.Side);
                Vector3    originalPosition   = grabbableObject.transform.position;
                Quaternion originalRotation   = grabbableObject.transform.rotation;

                if (sourceGrabEventArgs != null)
                {
                    // Grab is synchronizing with external grab. Position object momentarily in the exact same relative position with the grabber as the source external data. 
                    grabbableObject.transform.position = grabber.transform.TransformPoint(sourceGrabEventArgs.GrabberLocalObjectPosition);
                    grabbableObject.transform.rotation = grabber.transform.rotation * sourceGrabEventArgs.GrabberLocalObjectRotation;
                }

                // Update snap position/orientation if it's an external grab, to keep it in sync with exactly the same grip 

                if (sourceGrabEventArgs != null)
                {
                    snapPosition = grabber.transform.TransformPoint(sourceGrabEventArgs.GrabberLocalSnapPosition);
                    snapRotation = grabber.transform.rotation * sourceGrabEventArgs.GrabberLocalSnapRotation;
                }

                Matrix4x4 snapMatrix     = Matrix4x4.TRS(snapPosition, snapRotation, grabAlignTransform.lossyScale);
                Vector3   localProximity = grabAlignTransform.InverseTransformPoint(grabbableObject.GetGrabPointGrabProximityTransform(grabber, grabPoint).position);

                RelativeGrabRotation          = Quaternion.Inverse(grabber.transform.rotation) * grabbableObject.transform.rotation;
                RelativeGrabPosition          = grabber.transform.InverseTransformPoint(grabbableObject.transform.position);
                RelativeGrabberRotation       = Quaternion.Inverse(grabbableObject.transform.rotation) * grabber.transform.rotation;
                RelativeGrabberPosition       = grabbableObject.transform.InverseTransformPoint(grabber.transform.position);
                GrabAlignParentTransformUsed  = grabAlignTransform == grabbableObject.transform ? grabbableObject.transform : grabAlignTransform.parent;
                RelativeGrabAlignPosition     = TransformExt.GetLocalPosition(GrabAlignParentTransformUsed, snapPosition);
                RelativeGrabAlignRotation     = TransformExt.GetLocalRotation(GrabAlignParentTransformUsed, snapRotation);
                RelativeUsedGrabAlignRotation = Quaternion.Inverse(grabAlignTransform.rotation) * snapRotation;
                RelativeUsedGrabAlignPosition = grabAlignTransform.InverseTransformPoint(snapPosition);
                RelativeProximityPosition     = grabbableObject.transform.InverseTransformPoint(snapMatrix.MultiplyPoint(localProximity));
                GrabberLocalLeverageSource    = Vector3.zero;

                grabbableObject.CheckComputeAutoRotationProvider(snapPosition);

                if (grabbableObject.RotationProvider == UxrRotationProvider.HandPositionAroundPivot && grabbableObject.GetGrabPointSnapModeAffectsRotation(grabPoint))
                {
                    // Check if the leverage is provided by the inner side of the palm (where the thumb is) or the outer side.
                    // We do that by checking the difference in distance of both to the rotation pivot. If it is above a threshold, it is provided by either one of the two.
                    // If it is below a threshold it is provide by the grabber itself.

                    float separation    = UxrConstants.Hand.HandWidth;
                    float distanceInner = Vector3.Distance(grabbableObject.transform.position, snapPosition + snapRotation * grabber.LocalPalmThumbDirection * (separation * 0.5f));
                    float distanceOuter = Vector3.Distance(grabbableObject.transform.position, snapPosition - snapRotation * grabber.LocalPalmThumbDirection * (separation * 0.5f));

                    if (Mathf.Abs(distanceInner - distanceOuter) > separation * 0.5f)
                    {
                        GrabberLocalLeverageSource = grabber.LocalPalmThumbDirection * (separation * 0.5f * (distanceInner > distanceOuter ? 1.0f : -1.0f));
                    }
                }

                GrabberLocalParentLeverageSourceOnGrab = TransformExt.GetLocalPosition(grabbableObject.transform.parent, grabber.transform.TransformPoint(GrabberLocalLeverageSource));
                LocalPositionOnGrab                    = grabbableObject.transform.localPosition;
                LocalRotationOnGrab                    = grabbableObject.transform.localRotation;
                AlignPositionOnGrab                    = snapPosition;
                AlignRotationOnGrab                    = snapRotation;
                HandBoneLocalAvatarPositionOnGrab      = grabber.Avatar.transform.InverseTransformPoint(grabber.HandBone.position);
                HandBoneLocalAvatarRotationOnGrab      = Quaternion.Inverse(grabber.Avatar.transform.rotation) * grabber.HandBone.rotation;

                if (grabbableObject.UsesGrabbableParentDependency && grabbableObject.ControlParentDirection)
                {
                    // Compute leverage point in local grabbable parent coordinates when parent is rotated using the children.
                    // We will use the largest vector of these two: (leverage point, local child position).

                    Vector3 localParentLeveragePosition = grabbableObject.GrabbableParent.transform.InverseTransformPoint(grabber.transform.TransformPoint(GrabberLocalLeverageSource));
                    Vector3 localParentChildPosition    = grabbableObject.GrabbableParent.transform.InverseTransformPoint(grabbableObject.transform.position);

                    bool useLeveragePosition = localParentLeveragePosition.magnitude > localParentChildPosition.magnitude;

                    ParentGrabbableLookAtParentLeveragePoint = useLeveragePosition ? localParentLeveragePosition : localParentChildPosition;
                    ParentGrabbableLookAtLocalLeveragePoint  = useLeveragePosition ? grabbableObject.transform.InverseTransformPoint(grabber.transform.TransformPoint(GrabberLocalLeverageSource)) : Vector3.zero;
                }

                SingleRotationAngleContribution = 0.0f;
                LastAccumulatedAngle            = 0.0f;

                if (sourceGrabEventArgs != null)
                {
                    // Place back again. 
                    grabbableObject.transform.position = originalPosition;
                    grabbableObject.transform.rotation = originalRotation;
                }

                // Additional help for serialization
                _grabbableObject = grabbableObject;
            }

            #endregion

            #region Private Types & Data

            private UxrGrabber _grabber;
            private int        _grabbedPoint;
            private Quaternion _relativeGrabRotation;
            private Vector3    _relativeGrabPosition;
            private Quaternion _relativeGrabberRotation;
            private Vector3    _relativeGrabberPosition;
            private Transform  _grabAlignParentTransformUsed;
            private Quaternion _relativeGrabAlignRotation;
            private Vector3    _relativeGrabAlignPosition;
            private Quaternion _relativeUsedGrabAlignRotation;
            private Vector3    _relativeUsedGrabAlignPosition;
            private Vector3    _relativeProximityPosition;
            private Vector3    _grabberLocalLeverageSource;
            private Vector3    _grabberLocalParentLeverageSourceOnGrab;
            private Vector3    _parentGrabbableLookAtLocalLeveragePoint;
            private Vector3    _parentGrabbableLookAtParentLeveragePoint;
            private Vector3    _parentGrabbableLeverageContribution;
            private Quaternion _parentGrabbableLookAtRotationContribution;
            private float      _singleRotationAngleContribution;
            private float      _lastAccumulatedAngle;
            private Vector3    _parentLocalGrabPositionBeforeUpdate;
            private Vector3    _parentLocalGrabPositionAfterUpdate;
            private Vector3    _childLocalParentPosition;
            private Quaternion _childLocalParentRotation;
            private Vector3    _localPositionOnGrab;
            private Quaternion _localRotationOnGrab;
            private Vector3    _alignPositionOnGrab;
            private Quaternion _alignRotationOnGrab;
            private Vector3    _handBoneLocalAvatarPositionOnGrab;
            private Quaternion _handBoneLocalAvatarRotationOnGrab;

            // To be able to retrieve _grabAlignParentTransformUsed when serializing, because it doesn't have any way to serialize the reference:
            private UxrGrabbableObject _grabbableObject;

            #endregion
        }

        #endregion
    }
}