// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.RuntimeGrabInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
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
        private sealed class RuntimeGrabInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the grabber grabbing the <see cref="UxrGrabbableObject" />.
            /// </summary>
            public UxrGrabber Grabber { get; set; }

            /// <summary>
            ///     Gets the <see cref="UxrGrabbableObject" /> grabbed point.
            /// </summary>
            public int GrabbedPoint { get; set; }

            // *************************************************************************************************************************
            // Transform information about the grip.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> rotation relative to the <see cref="UxrGrabber" /> at the moment
            ///     it was grabbed.
            /// </summary>
            public Quaternion RelativeGrabRotation { get; private set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> position in local <see cref="UxrGrabber" /> space at the moment
            ///     it was grabbed.
            /// </summary>
            public Vector3 RelativeGrabPosition { get; private set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> rotation relative to the <see cref="UxrGrabbableObject" /> at the moment
            ///     it was grabbed.
            /// </summary>
            public Quaternion RelativeGrabberRotation { get; private set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> position in local <see cref="UxrGrabbableObject" /> space at the moment
            ///     it was grabbed.
            /// </summary>
            public Vector3 RelativeGrabberPosition { get; private set; }

            /// <summary>
            ///     Gets or sets the transform relative to which <see cref="RelativeGrabAlignPosition" /> and
            ///     <see cref="RelativeGrabAlignRotation" /> are specified.
            /// </summary>
            public Transform GrabAlignParentTransformUsed { get; private set; }

            /// <summary>
            ///     Gets or sets the snap rotation relative to the <see cref="GrabAlignParentTransformUsed" /> at the moment it was
            ///     grabbed.
            /// </summary>
            public Quaternion RelativeGrabAlignRotation { get; private set; }

            /// <summary>
            ///     Gets or sets the snap position in local <see cref="GrabAlignParentTransformUsed" /> space at the moment it was
            ///     grabbed.
            /// </summary>
            public Vector3 RelativeGrabAlignPosition { get; private set; }

            /// <summary>
            ///     Gets or sets the computed snap rotation relative to the object's snap rotation, which might be different if
            ///     the computed snap rotation came from an <see cref="UxrGrabPointShape"/>.
            /// </summary>
            public Quaternion RelativeUsedGrabAlignRotation { get; private set; }

            /// <summary>
            ///     Gets or sets the computed snap position relative to the object's snap position, which might be different if
            ///     the computed snap position came from an <see cref="UxrGrabPointShape"/>.
            /// </summary>
            public Vector3 RelativeUsedGrabAlignPosition { get; private set; }

            /// <summary>
            ///     Gets or sets the proximity rotation relative to the <see cref="UxrGrabbableObject" /> at the moment it was grabbed.
            /// </summary>
            public Vector3 RelativeProximityPosition { get; private set; }

            /// <summary>
            ///     Gets or sets the source in local <see cref="UxrGrabber" /> coordinates where the source of leverage will be
            ///     computed for <see cref="UxrRotationProvider.HandPositionAroundPivot" /> manipulation. This will improve rotation
            ///     behaviour when the hands are rotated because otherwise the source of leverage is the grabber itself and rotating
            ///     the hand will keep the grabber more or less stationary.
            /// </summary>
            public Vector3 GrabberLocalLeverageSource { get; private set; }

            /// <summary>
            ///     Gets or sets the leverage source <see cref="GrabberLocalLeverageSource" /> in local coordinates of the parent
            ///     transform of the grabbable at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Vector3 GrabberLocalParentLeverageSourceOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the leverage point in local coordinates that this child grabbable will use when the parent
            ///     grabbable rotation provider is HandPositionAroundPivot. 
            /// </summary>
            public Vector3 ParentGrabbableLookAtLocalLeveragePoint { get; set; }

            /// <summary>
            ///     Gets or sets the leverage point in local grabbable parent coordinates that this child grabbable will use
            ///     when the parent grabbable rotation provider is HandPositionAroundPivot. 
            /// </summary>
            public Vector3 ParentGrabbableLookAtParentLeveragePoint { get; set; }

            /// <summary>
            ///     Gets or sets the look-at contribution in world coordinates of this child grabbable object to the parent
            ///     grabbable look-at algorithm for the current frame. Only for HandPositionAroundPivot in parent grabbable objects.
            /// </summary>
            public Vector3 ParentGrabbableLeverageContribution { get; set; }

            /// <summary>
            ///     Gets or sets the rotation contribution of this object to the parent grabbable look-at algorithm for the
            ///     current frame. Only for HandPositionAroundPivot in parent grabbable objects.
            /// </summary>
            public Quaternion ParentGrabbableLookAtRotationContribution { get; set; }

            /// <summary>
            ///     Gets or sets the rotation angle contribution, in objects constrained to a single axis rotation, during the current
            ///     grab.
            /// </summary>
            public float SingleRotationAngleContribution { get; set; }

            /// <summary>
            ///     Gets or sets the <see cref="SingleRotationAngleContribution" /> value the last time it was accumulated into
            ///     the object internal angle. This allows angle contributions to work using absolute values instead of delta values
            ///     to have better precision.
            /// </summary>
            public float LastAccumulatedAngle { get; set; }

            // *************************************************************************************************************************
            // Parent dependency information.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets the grab position in grabbable parent space before updating this object being grabbed. It is used to compute
            ///     the lookAt contribution of this grab on the parent, when the parent is being grabbed. The grab position
            ///     is computed before constraints are applied to the object to compute the contribution correctly.
            /// </summary>
            public Vector3 ParentLocalGrabPositionBeforeUpdate { get; set; }

            /// <summary>
            ///     Gets the grab position in grabbable parent space after updating this object being grabbed. See
            ///     <see cref="ParentLocalGrabPositionBeforeUpdate" />.
            /// </summary>
            public Vector3 ParentLocalGrabPositionAfterUpdate { get; set; }

            /// <summary>
            ///     Gets the grabbable parent position in local grabbable child space before updating the child being grabbed.
            ///     It is used to compute the contribution of a child on a parent when the parent is not being grabbed.
            /// </summary>
            public Vector3 ChildLocalParentPosition { get; set; }

            /// <summary>
            ///     Gets the grabbable parent rotation in local grabbable child space before updating the child being grabbed.
            ///     It is used to compute the contribution of a child on a parent when the parent is not being grabbed.
            /// </summary>
            public Quaternion ChildLocalParentRotation { get; set; }

            // *************************************************************************************************************************
            // For smooth transitions from object to hand or object to target or hand to object where we want to avoid instant snapping.
            // *************************************************************************************************************************

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> local position at the moment it was grabbed.
            /// </summary>
            public Vector3 LocalPositionOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabbableObject" /> local rotation at the moment it was grabbed.
            /// </summary>
            public Quaternion LocalRotationOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the world-space snap position at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Vector3 AlignPositionOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the world-space snap rotation at the moment the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public Quaternion AlignRotationOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the hand bone position in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
            ///     was grabbed.
            /// </summary>
            public Vector3 HandBoneLocalAvatarPositionOnGrab { get; private set; }

            /// <summary>
            ///     Gets or sets the hand bone rotation in local avatar coordinates at the moment the <see cref="UxrGrabbableObject" />
            ///     was grabbed.
            /// </summary>
            public Quaternion HandBoneLocalAvatarRotationOnGrab { get; private set; }

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

                RelativeGrabRotation           = Quaternion.Inverse(grabber.transform.rotation) * grabbableObject.transform.rotation;
                RelativeGrabPosition           = grabber.transform.InverseTransformPoint(grabbableObject.transform.position);
                RelativeGrabberRotation        = Quaternion.Inverse(grabbableObject.transform.rotation) * grabber.transform.rotation;
                RelativeGrabberPosition        = grabbableObject.transform.InverseTransformPoint(grabber.transform.position);
                GrabAlignParentTransformUsed   = grabAlignTransform == grabbableObject.transform ? grabbableObject.transform : grabAlignTransform.parent;
                RelativeGrabAlignPosition      = TransformExt.GetLocalPosition(GrabAlignParentTransformUsed, snapPosition);
                RelativeGrabAlignRotation      = TransformExt.GetLocalRotation(GrabAlignParentTransformUsed, snapRotation);
                RelativeUsedGrabAlignRotation  = Quaternion.Inverse(grabAlignTransform.rotation) * snapRotation;
                RelativeUsedGrabAlignPosition  = grabAlignTransform.InverseTransformPoint(snapPosition);
                RelativeProximityPosition      = grabbableObject.transform.InverseTransformPoint(snapMatrix.MultiplyPoint(localProximity));
                GrabberLocalLeverageSource     = Vector3.zero;

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
            }

            #endregion
        }

        #endregion
    }
}