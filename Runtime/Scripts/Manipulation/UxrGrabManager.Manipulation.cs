// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.Manipulation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSync;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System.Math;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Haptics;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the currently grabbed objects.
        /// </summary>
        public IEnumerable<UxrGrabbableObject> CurrentGrabbedObjects => _currentManipulations.Keys;

        /// <summary>
        ///     Gets or sets whether grabbing is allowed.
        /// </summary>
        public bool IsGrabbingAllowed { get; set; } = true;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to grab something. An object will be grabbed if there is any in reach.
        /// </summary>
        /// <param name="avatar">Avatar that tried to grab</param>
        /// <param name="handSide">Whether it is trying to grab using the left hand or right hand</param>
        /// <returns>
        ///     The grabber component that grabbed an object, if an object was grabbed.
        /// </returns>
        public UxrGrabber TryGrab(UxrAvatar avatar, UxrHandSide handSide)
        {
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar).Where(grabber => grabber.Side == handSide))
            {
                bool wasGrabbing = grabber.GrabbedObject != null;
                TryGrab(grabber);

                if (!wasGrabbing && grabber.GrabbedObject != null)
                {
                    return grabber;
                }
            }

            return null;
        }

        /// <summary>
        ///     Tries to release an object being grabbed.
        /// </summary>
        /// <param name="avatar">Avatar that tried to release</param>
        /// <param name="handSide">Whether it is trying to release using the left hand or right hand</param>
        /// <returns>
        ///     The grabber component that released an object, if an object was released.
        /// </returns>
        public UxrGrabber TryRelease(UxrAvatar avatar, UxrHandSide handSide)
        {
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar).Where(grabber => grabber.Side == handSide))
            {
                bool wasGrabbing = grabber.GrabbedObject != null;
                NotifyReleaseGrab(grabber);

                if (wasGrabbing && grabber.GrabbedObject == null)
                {
                    return grabber;
                }
            }

            return null;
        }

        /// <summary>
        ///     Grabs an object.
        /// </summary>
        /// <param name="grabber">Grabber that will grab the object</param>
        /// <param name="grabbableObject">Object to grab</param>
        /// <param name="grabPoint">Grab point to grab the object from</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void GrabObject(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint, bool propagateEvents)
        {
            GrabObject(grabber, grabbableObject, grabPoint, null, propagateEvents);
        }

        /// <summary>
        ///     Releases an object from a hand.
        /// </summary>
        /// <param name="grabber">
        ///     If non-null it will tell the grabber that releases the object. If it is null any grabber that is holding the object
        ///     will release it
        /// </param>
        /// <param name="grabbableObject">Object being released</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void ReleaseObject(UxrGrabber grabber, UxrGrabbableObject grabbableObject, bool propagateEvents)
        {
            ReleaseObject(grabber, grabbableObject, null, null, null, null, propagateEvents);
        }

        /// <summary>
        ///     Releases all grabs on a given <see cref="UxrGrabbableObject" />.
        /// </summary>
        /// <param name="grabbableObject">The object to release</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void ReleaseGrabs(UxrGrabbableObject grabbableObject, bool propagateEvents)
        {
            if (grabbableObject == null)
            {
                return;
            }

            foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabbableObject))
            {
                ReleaseObject(grabInfo.Grabber, grabbableObject, propagateEvents);
            }
        }

        /// <summary>
        ///     Places a <see cref="UxrGrabbableObject" /> on an <see cref="UxrGrabbableObjectAnchor" />.
        ///     <para>
        ///         It can be placed either instantly or smoothly depending on <paramref name="placementOptions" />.
        ///         If the object is currently being grabbed, <paramref name="placementOptions" /> can also decide
        ///         whether the grips are released or not.
        ///     </para>
        /// </summary>
        /// <param name="grabbableObject">The object to place</param>
        /// <param name="anchor">The anchor to place it on</param>
        /// <param name="placementOptions">Placement options</param>
        /// <param name="propagateEvents">Whether to propagate potential Removing/Removed/Placing/Placed events.</param>
        /// <returns>
        ///     Whether the object was placed or not. The placement can fail if there was a null argument or if the anchor has
        ///     already an object on it.
        /// </returns>
        public bool PlaceObject(UxrGrabbableObject grabbableObject, UxrGrabbableObjectAnchor anchor, UxrPlacementOptions placementOptions, bool propagateEvents)
        {
            if (grabbableObject == null)
            {
                return false;
            }

            if (grabbableObject.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(PlaceObject)}, {nameof(UxrGrabbableObject)} component on {grabbableObject.name} is disabled.");
                }
            }

            if (anchor == null || (anchor != null && anchor.CurrentPlacedObject != null))
            {
                return false;
            }

            if (anchor.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(PlaceObject)}, {nameof(UxrGrabbableObjectAnchor)} component on {anchor.name} is disabled.");
                }
            }
            
            // This method will be synchronized. It will generate a new frame when recording a replay to ensure smooth interpolation when re-parenting.
            BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.GenerateNewFrame);

            grabbableObject.PlacementOptions = placementOptions;

            UxrGrabber               grabber      = null;
            int                      grabbedPoint = -1;
            UxrGrabbableObjectAnchor oldAnchor    = grabbableObject.CurrentAnchor;
            bool                     releaseGrip  = !placementOptions.HasFlag(UxrPlacementOptions.DontRelease);

            if (releaseGrip)
            {
                // Release the grips if there are any.

                if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
                {
                    // TODO: Ideally we would send events from all grabbers later, not just the first
                    grabber      = manipulationInfo.Grabs.First().Grabber;
                    grabbedPoint = manipulationInfo.Grabs.First().GrabbedPoint;

                    while (manipulationInfo.Grabs.Any())
                    {
                        // Don't propagate Release events, because Place and Release are mutually exclusive
                        ReleaseObject(manipulationInfo.Grabs.First().Grabber, grabbableObject, false);
                    }
                }
            }

            // Remove and raise events

            if (oldAnchor != null)
            {
                UxrManipulationEventArgs removeEventArgs = UxrManipulationEventArgs.FromRemove(grabbableObject, oldAnchor, grabber, grabbedPoint);

                OnObjectRemoving(removeEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    oldAnchor.RaiseRemovingEvent(removeEventArgs);
                }

                // Activate/deactivate objects
                if (oldAnchor.ActivateOnPlaced)
                {
                    oldAnchor.ActivateOnPlaced.SetActive(true);
                }

                if (oldAnchor.ActivateOnEmpty)
                {
                    oldAnchor.ActivateOnEmpty.SetActive(false);
                }

                if (oldAnchor.ActivateOnCompatibleNear != null)
                {
                    oldAnchor.ActivateOnCompatibleNear.SetActive(false);
                }

                if (oldAnchor.ActivateOnCompatibleNotNear != null)
                {
                    oldAnchor.ActivateOnCompatibleNotNear.SetActive(false);
                }

                oldAnchor.CurrentPlacedObject = null;
                grabbableObject.CurrentAnchor = null;

                OnObjectRemoved(removeEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    oldAnchor.RaiseRemovedEvent(removeEventArgs);
                }
            }

            UxrManipulationEventArgs placeEventArgs = UxrManipulationEventArgs.FromPlace(grabbableObject, anchor, grabber, grabbedPoint, placementOptions);
            OnObjectPlacing(placeEventArgs, propagateEvents);
            if (propagateEvents)
            {
                grabbableObject.RaisePlacingEvent(placeEventArgs);
                anchor.RaisePlacingEvent(placeEventArgs);
            }

            // Setup

            if (grabbableObject.RigidBodySource != null)
            {
                grabbableObject.RigidBodySource.isKinematic = true;
            }

            if (grabbableObject.UseParenting)
            {
                ChangeGrabbableObjectParent(grabbableObject, anchor.AlignTransform);
            }

            grabbableObject.LocalPositionBeforeUpdate = grabbableObject.transform.localPosition;
            grabbableObject.LocalRotationBeforeUpdate = grabbableObject.transform.localRotation;

            // Activate/deactivate objects

            if (anchor.ActivateOnPlaced)
            {
                anchor.ActivateOnPlaced.SetActive(true);
            }

            if (anchor.ActivateOnEmpty)
            {
                anchor.ActivateOnEmpty.SetActive(false);
            }

            if (anchor.ActivateOnCompatibleNear != null)
            {
                anchor.ActivateOnCompatibleNear.SetActive(false);
            }

            if (anchor.ActivateOnCompatibleNotNear != null)
            {
                anchor.ActivateOnCompatibleNotNear.SetActive(false);
            }

            // Update references

            grabbableObject.CurrentAnchor = anchor;
            anchor.CurrentPlacedObject    = grabbableObject;

            if (releaseGrip && grabber != null && grabber.GrabbedObject != null && _currentManipulations.ContainsKey(grabber.GrabbedObject))
            {
                _currentManipulations.Remove(grabbableObject);
            }

            // Start smooth transitions to final position/orientation if necessary

            if (placementOptions.HasFlag(UxrPlacementOptions.Smooth) && Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
            {
                grabbableObject.StartSmoothAnchorPlacement();
            }
            else
            {
                grabbableObject.StopSmoothAnchorPlacement();
                grabbableObject.transform.ApplyAlignment(grabbableObject.DropAlignTransform,
                                                         grabbableObject.CurrentAnchor.AlignTransform,
                                                         UxrUtils.BuildTransformations(UxrGrabbableObject.GetSnapModeAffectsPosition(grabbableObject.DropSnapMode),
                                                                                       UxrGrabbableObject.GetSnapModeAffectsRotation(grabbableObject.DropSnapMode)));
            }

            if (grabbableObject.IsConstrained && Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
            {
                grabbableObject.StartSmoothConstrain();
            }

            // Raise events

            OnObjectPlaced(placeEventArgs, propagateEvents);

            if (propagateEvents)
            {
                grabbableObject.RaisePlacedEvent(placeEventArgs);
                anchor.RaisePlacedEvent(placeEventArgs);
            }
            
            EndSyncMethod(new object[] { grabbableObject, anchor, placementOptions, propagateEvents });

            return true;
        }

        /// <summary>
        ///     Removes a <see cref="UxrGrabbableObject" /> placed on an <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to remove from the anchor</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        public void RemoveObjectFromAnchor(UxrGrabbableObject grabbableObject, bool propagateEvents)
        {
            if (grabbableObject == null || grabbableObject.CurrentAnchor == null)
            {
                return;
            }

            if (grabbableObject.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(RemoveObjectFromAnchor)}, {nameof(UxrGrabbableObject)} component on {nameof(grabbableObject.name)} is disabled.");
                }
            }

            if (grabbableObject.CurrentAnchor.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(RemoveObjectFromAnchor)}, {nameof(UxrGrabbableObjectAnchor)} component on {nameof(grabbableObject.CurrentAnchor.name)} is disabled.");
                }
            }

            // This method will be synchronized. It will generate a new frame when recording a replay to ensure smooth interpolation when re-parenting.
            BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.GenerateNewFrame);

            // Raise events

            UxrGrabbableObjectAnchor sourceAnchor    = grabbableObject.CurrentAnchor;
            UxrManipulationEventArgs removeEventArgs = UxrManipulationEventArgs.FromRemove(grabbableObject, sourceAnchor, null, -1);

            OnObjectRemoving(removeEventArgs, propagateEvents);

            if (propagateEvents)
            {
                sourceAnchor.RaiseRemovingEvent(removeEventArgs);
            }

            // Perform removal

            grabbableObject.StopSmoothConstrain();
            grabbableObject.StopSmoothAnchorPlacement();

            if (grabbableObject.RigidBodySource != null && grabbableObject.CanUseRigidBody && !IsBeingGrabbed(grabbableObject) && !GetDirectChildrenLookAtManipulations(grabbableObject).Any())
            {
                grabbableObject.RigidBodySource.isKinematic = !grabbableObject.RigidBodyDynamicOnRelease;
            }

            if (grabbableObject.UseParenting)
            {
                if (grabbableObject.IsBeingGrabbed)
                {
                    AssignGrabbedObjectParent(grabbableObject);
                }
                else
                {
                    ChangeGrabbableObjectParent(grabbableObject, grabbableObject.CurrentAnchor.transform.parent);
                }
            }

            if (grabbableObject.IsConstrained && Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
            {
                // Smoothly transition from the constrained state to the grabbed state
                grabbableObject.StartSmoothManipulationTransition();
            }

            if (sourceAnchor != null)
            {
                sourceAnchor.CurrentPlacedObject = null;
            }

            grabbableObject.CurrentAnchor = null;

            // Raise events

            OnObjectRemoved(removeEventArgs, propagateEvents);

            if (propagateEvents)
            {
                sourceAnchor.RaiseRemovedEvent(removeEventArgs);
            }
            
            EndSyncMethod(new object[] { grabbableObject, propagateEvents });
        }

        /// <summary>
        ///     Moves the object to a new world position and optionally applies resistance and propagates user-defined constraining
        ///     events.
        /// </summary>
        /// <param name="grabbableObject">The object to move</param>
        /// <param name="position">New world-space position</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new position using <see cref="UxrGrabbableObject.TranslationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetPositionUsingConstraints(UxrGrabbableObject grabbableObject, Vector3 position, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, position, grabbableObject.transform.rotation, Space.World, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Rotates the object to a new world rotation and optionally applies resistance and propagates user-defined
        ///     constraining events.
        /// </summary>
        /// <param name="grabbableObject">The object to rotate</param>
        /// <param name="rotation">New world-space rotation</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new rotation using <see cref="UxrGrabbableObject.RotationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetRotationUsingConstraints(UxrGrabbableObject grabbableObject, Quaternion rotation, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, grabbableObject.transform.position, rotation, Space.World, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Moves and rotates the object to a new world position/rotation and optionally applies resistance and propagates
        ///     user-defined constraining events.
        /// </summary>
        /// <param name="grabbableObject">The object to rotate</param>
        /// <param name="position">New world-space position</param>
        /// <param name="rotation">New world-space rotation</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new position and rotation using
        ///     <see cref="UxrGrabbableObject.TranslationResistance" /> and <see cref="UxrGrabbableObject.RotationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetPositionAndRotationUsingConstraints(UxrGrabbableObject grabbableObject, Vector3 position, Quaternion rotation, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, position, rotation, Space.World, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Moves the object to a new local position and optionally applies resistance and propagates user-defined constraining
        ///     events.
        /// </summary>
        /// <param name="grabbableObject">The object to move</param>
        /// <param name="localPosition">New local-space position</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new position using <see cref="UxrGrabbableObject.TranslationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetLocalPositionUsingConstraints(UxrGrabbableObject grabbableObject, Vector3 localPosition, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, localPosition, grabbableObject.transform.rotation, Space.Self, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Rotates the object to a new local rotation and optionally applies resistance and propagates user-defined
        ///     constraining events.
        /// </summary>
        /// <param name="grabbableObject">The object to rotate</param>
        /// <param name="localRotation">New local-space rotation</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new rotation using <see cref="UxrGrabbableObject.RotationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetLocalRotationUsingConstraints(UxrGrabbableObject grabbableObject, Quaternion localRotation, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, grabbableObject.transform.position, localRotation, Space.Self, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Moves and rotates the object to a new local position/rotation and optionally applies resistance and propagates
        ///     user-defined constraining events.
        /// </summary>
        /// <param name="grabbableObject">The object to rotate</param>
        /// <param name="localPosition">New local-space position</param>
        /// <param name="localRotation">New local-space rotation</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new position and rotation using
        ///     <see cref="UxrGrabbableObject.TranslationResistance" /> and <see cref="UxrGrabbableObject.RotationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        public void SetLocalPositionAndRotationUsingConstraints(UxrGrabbableObject grabbableObject, Vector3 localPosition, Quaternion localRotation, bool useResistance, bool propagateEvents)
        {
            SetPositionAndRotationUsingConstraintsInternal(grabbableObject, localPosition, localRotation, Space.Self, useResistance, propagateEvents);
        }

        /// <summary>
        ///     Places the grips of a grabbed object again in the correct position after the object was moved/rotated.
        ///     This is required because the hands are not parented to the object and if the object is moved or rotated, the hands
        ///     will not keep the correct relative position/rotation anymore.
        ///     For grips that have position/rotation snapping, it makes sure to snap to the correct position.
        ///     For grips that don't have snapping, it will make sure to keep the same grip position/rotation used at the moment
        ///     the object was grabbed.
        /// </summary>
        /// <remarks>
        ///     When using /<see cref="UxrGrabbableObject.ConstraintsApplied" /> there is no need to call this method.
        ///     UltimateX will call it internally to make sure that the grips stay in place after any user-defined constraints are
        ///     applied.
        /// </remarks>
        public void KeepGripsInPlace(UxrGrabbableObject grabbableObject)
        {
            foreach (UxrGrabber grabber in GetGrabbingHands(grabbableObject))
            {
                KeepGripInPlace(grabber);
            }
        }

        /// <summary>
        ///     Like <see cref="KeepGripsInPlace" /> but for a single grip.
        /// </summary>
        public void KeepGripInPlace(UxrGrabber grabber)
        {
            if (grabber.HandBone == null || !Features.HasFlag(UxrManipulationFeatures.KeepGripsInPlace))
            {
                return;
            }

            if (grabber.GrabbedObject != null && _currentManipulations.ContainsKey(grabber.GrabbedObject))
            {
                grabber.HandBone.ApplyAlignment(grabber.transform.position,
                                                grabber.transform.rotation,
                                                GetGrabbedPointGrabAlignPosition(grabber),
                                                GetGrabbedPointGrabAlignRotation(grabber));
            }
        }

        /// <summary>
        ///     Gets or rotation angle in degrees for objects that have a single rotational degree of freedom.
        /// </summary>
        /// <param name="grabbableObject">The object to get the information from</param>
        /// <returns>Angle in degrees</returns>
        public float GetObjectSingleRotationAxisDegrees(UxrGrabbableObject grabbableObject)
        {
            int singleRotationAxisIndex = grabbableObject.SingleRotationAxisIndex;

            if (singleRotationAxisIndex == -1)
            {
                return 0.0f;
            }

            return (grabbableObject.SingleRotationAngleCumulative + GetCurrentSingleRotationAngleContributions(grabbableObject)).Clamped(grabbableObject.RotationAngleLimitsMin[singleRotationAxisIndex], grabbableObject.RotationAngleLimitsMax[singleRotationAxisIndex]);
        }

        /// <summary>
        ///     Sets the rotation angle in degrees for objects that have a single rotational degree of freedom. Use this method to
        ///     change the angle at runtime so that it keeps track of the rotation limits and also works while the object is being
        ///     grabbed.
        /// </summary>
        /// <param name="grabbableObject">The object to apply the new rotation to</param>
        /// <param name="degrees">Angle in degrees</returns>
        public void SetObjectSingleRotationAxisDegrees(UxrGrabbableObject grabbableObject, float degrees)
        {
            int singleRotationAxisIndex = grabbableObject.SingleRotationAxisIndex;

            if (singleRotationAxisIndex == -1)
            {
                return;
            }

            float currentGrabContributions = GetCurrentSingleRotationAngleContributions(grabbableObject);
            float newValue                 = degrees.Clamped(grabbableObject.RotationAngleLimitsMin[singleRotationAxisIndex], grabbableObject.RotationAngleLimitsMax[singleRotationAxisIndex]);

            grabbableObject.SingleRotationAngleCumulative = newValue - currentGrabContributions;
            grabbableObject.transform.localRotation       = grabbableObject.InitialLocalRotation * Quaternion.AngleAxis(newValue, (UxrAxis)singleRotationAxisIndex);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the grabbable objects using manipulation logic.
        /// </summary>
        private void UpdateManipulation()
        {
            ICollection<UxrGrabbableObject> sortedGrabbableObjects = null;

            if (_currentManipulations.Any())
            {
                // Sort objects for manipulation so that parents are always processed before the children.

                sortedGrabbableObjects = _currentManipulations.Keys;

                if (_currentManipulations.Count > 1)
                {
                    List<UxrGrabbableObject> sortedList = new List<UxrGrabbableObject>(sortedGrabbableObjects);
                    sortedList.Sort((a, b) => b.AllChildren.Count.CompareTo(a.AllChildren.Count));
                    sortedGrabbableObjects = sortedList;
                }

                // Compute manipulation update. Check ProcessManipulation's summary for detailed explanation. 

                foreach (UxrGrabbableObject grabbableObject in sortedGrabbableObjects)
                {
                    if (grabbableObject == null || grabbableObject.IsBeingDestroyed)
                    {
                        continue;
                    }

                    ProcessManipulation(grabbableObject);
                }
            }

            // Update smooth object transitions.

            if (Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
            {
                UpdateSmoothObjectTransitions();
            }

            // Apply user-defined constraints

            if (sortedGrabbableObjects != null)
            {
                // Apply user defined manipulation constraints

                foreach (UxrGrabbableObject grabbableObject in sortedGrabbableObjects)
                {
                    if (grabbableObject == null || grabbableObject.IsBeingDestroyed)
                    {
                        continue;
                    }
                    
                    UxrApplyConstraintsEventArgs constrainEventArgs = new UxrApplyConstraintsEventArgs(grabbableObject);
                    grabbableObject.RaiseConstraintsApplying(constrainEventArgs);
                    grabbableObject.RaiseConstraintsApplied(constrainEventArgs);
                    grabbableObject.KeepGripsInPlace();
                    grabbableObject.RaiseConstraintsFinished(constrainEventArgs);
                }
            }

            // Release constrained grips that are too far away from the real hand.
            // Do it in two passes to avoid deleting elements in the collection being iterated.

            List<UxrGrabber> gripsToRelease = null;

            foreach (var manipulation in _currentManipulations)
            {
                foreach (UxrGrabber grabber in manipulation.Value.Grabbers)
                {
                    if (ShouldGrabberReleaseFarAwayGrip(grabber))
                    {
                        if (gripsToRelease == null)
                        {
                            gripsToRelease = new List<UxrGrabber>();
                        }

                        gripsToRelease.Add(grabber);
                    }
                }
            }

            if (gripsToRelease != null)
            {
                foreach (UxrGrabber grabber in gripsToRelease)
                {
                    grabber.Avatar.ControllerInput.SendHapticFeedback(grabber.Side, UxrHapticClipType.Click, 1.0f);
                    ReleaseObject(grabber, grabber.GrabbedObject, true);
                }
            }

            // Update smooth grabber transitions.

            if (Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
            {
                foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
                {
                    grabber.UpdateSmoothManipulationTransition(Time.unscaledDeltaTime);
                }
            }
        }

        /// <summary>
        ///     Grabs an object. Optionally if <paramref name="sourceEventArgs" /> is not null, the grab synchronizes with a grab
        ///     coming from an external event.
        /// </summary>
        /// <param name="grabber">Grabber that will grab the object</param>
        /// <param name="grabbableObject">Object to grab</param>
        /// <param name="grabPoint">Grab point to grab the object from</param>
        /// <param name="sourceEventArgs">
        ///     If not null, it will contain the event args from an external grab that this grab will
        ///     have to mimic. This is useful in multi-player environments to ensure that the grip ends up being the same.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        private void GrabObject(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint, UxrManipulationEventArgs sourceEventArgs, bool propagateEvents)
        {
            if (grabber == null || grabbableObject == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.ManipulationModule} {nameof(GrabObject)}, {nameof(UxrGrabber)} component or {nameof(UxrGrabbableObject)} component is null.");
                }
                
                return;
            }

            if (grabber.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(GrabObject)}, {nameof(UxrGrabber)} component on {grabber.name} is disabled.");
                }
            }

            if (grabbableObject.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(GrabObject)}, {nameof(UxrGrabbableObject)} component on {grabbableObject.name} is disabled.");
                }
            }

            // This method will be synchronized. It will generate a new frame when recording a replay to ensure smooth interpolation when re-parenting.
            BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.GenerateNewFrame);

            UxrGrabbableObjectAnchor sourceAnchor = grabbableObject.CurrentAnchor;

            // Were we swapping hands, are we grabbing with more than one hand or is it a new grab?

            bool                     handSwapSamePoint       = false;
            bool                     handSwapDifferentPoints = false;
            bool                     moreThanOneHand         = false;
            UxrGrabber               releasingGrabber        = null;
            RuntimeManipulationInfo  manipulationInfo        = null;
            UxrManipulationEventArgs releaseEventArgs        = null;

            foreach (UxrGrabber otherGrabberCandidate in UxrGrabber.EnabledComponents)
            {
                if (otherGrabberCandidate != grabber && otherGrabberCandidate.GrabbedObject == grabbableObject)
                {
                    // Other grabber is already grabbing this object. Check if it is the same grabbing point or not.
                    _currentManipulations.TryGetValue(grabbableObject, out manipulationInfo);

                    if (manipulationInfo != null && manipulationInfo.GrabbedPoints.Contains(grabPoint))
                    {
                        // It is the same grabbing point. Now there are two options:
                        // -If the grabbing point has an UxrGrabPointShape component associated, it will be grabbed with the new hand at the same time if MultiGrab is enabled.
                        // -If it does not have an UxrGrabPointShape it will mean that the object will swap from one hand to the other.
                        if (grabbableObject.GetGrabPointShape(grabPoint) != null && grabbableObject.AllowMultiGrab)
                        {
                            moreThanOneHand = true;
                            break;
                        }

                        // We are swapping hands because there is already other hand grabbing this same point
                        // Raise release event for the other hand
                        releasingGrabber  = otherGrabberCandidate;
                        handSwapSamePoint = true;
                        releaseEventArgs  = UxrManipulationEventArgs.FromRelease(releasingGrabber.GrabbedObject, sourceAnchor, releasingGrabber, grabPoint, false, true);
                        break;
                    }

                    if (manipulationInfo != null && !manipulationInfo.GrabbedPoints.Contains(grabPoint))
                    {
                        // Other hand grabbing another point of the same object: both hands will grab the object if MultiGrab is enabled, or the
                        // other hand will be released if not
                        releasingGrabber = otherGrabberCandidate;

                        if (!grabbableObject.AllowMultiGrab)
                        {
                            handSwapDifferentPoints = true;
                            releaseEventArgs        = UxrManipulationEventArgs.FromRelease(releasingGrabber.GrabbedObject, sourceAnchor, releasingGrabber, grabPoint, false, true);
                        }
                        else
                        {
                            moreThanOneHand = true;
                        }

                        break;
                    }
                }
            }

            // Raise release event

            if (releaseEventArgs != null)
            {
                OnObjectReleasing(releaseEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    releaseEventArgs.GrabbableObject.RaiseReleasingEvent(releaseEventArgs);
                }

                manipulationInfo.NotifyEndGrab(releaseEventArgs.Grabber, releaseEventArgs.GrabbableObject, releaseEventArgs.GrabPointIndex);
            }

            // Compute grabber snap position/orientation

            grabbableObject.ComputeRequiredGrabberTransform(grabber, grabPoint, out Vector3 grabberPosition, out Quaternion grabberRotation, false);

            Vector3    grabberLocalSnapPosition = sourceEventArgs?.GrabberLocalSnapPosition ?? grabber.transform.InverseTransformPoint(grabberPosition);
            Quaternion grabberLocalSnapRotation = sourceEventArgs?.GrabberLocalSnapRotation ?? Quaternion.Inverse(grabber.transform.rotation) * grabberRotation;

            // Raise grabbing/removing events

            UxrManipulationEventArgs grabEventArgs   = UxrManipulationEventArgs.FromGrab(grabbableObject, sourceAnchor, grabber, grabPoint, moreThanOneHand, handSwapSamePoint || handSwapDifferentPoints, grabberLocalSnapPosition, grabberLocalSnapRotation);
            UxrManipulationEventArgs removeEventArgs = UxrManipulationEventArgs.FromRemove(grabbableObject, sourceAnchor, grabber, grabPoint, moreThanOneHand, handSwapSamePoint || handSwapDifferentPoints);

            OnObjectGrabbing(grabEventArgs, propagateEvents);

            if (propagateEvents)
            {
                grabbableObject.RaiseGrabbingEvent(grabEventArgs);
            }

            if (sourceAnchor && !grabbableObject.IsConstrained)
            {
                OnObjectRemoving(removeEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    sourceAnchor.RaiseRemovingEvent(removeEventArgs);
                }
            }

            // Link it to the hand

            grabber.GrabbedObject = grabbableObject;

            if (sourceAnchor && !grabbableObject.IsConstrained)
            {
                sourceAnchor.CurrentPlacedObject    = null;
                grabber.GrabbedObject.CurrentAnchor = null;

                if (sourceAnchor.ActivateOnPlaced)
                {
                    sourceAnchor.ActivateOnPlaced.SetActive(false);
                }

                if (sourceAnchor.ActivateOnEmpty)
                {
                    sourceAnchor.ActivateOnEmpty.SetActive(true);
                }

                if (sourceAnchor.ActivateOnCompatibleNear != null)
                {
                    sourceAnchor.ActivateOnCompatibleNear.SetActive(false);
                }

                if (sourceAnchor.ActivateOnCompatibleNotNear != null)
                {
                    sourceAnchor.ActivateOnCompatibleNotNear.SetActive(false);
                }
            }

            // If it is a dynamic object, make it kinematic while it is grabbed. Also check the parent grabbable if it exists.

            Rigidbody rigidBodyToGrab = null;

            if (grabbableObject.RigidBodySource != null && grabbableObject.CanUseRigidBody)
            {
                rigidBodyToGrab = grabbableObject.RigidBodySource;
            }

            UxrGrabbableObject grabbableParentLookAt = grabbableObject.ParentLookAts.FirstOrDefault();

            if (rigidBodyToGrab == null && grabbableParentLookAt != null && grabbableParentLookAt.RigidBodySource != null && grabbableParentLookAt.CanUseRigidBody)
            {
                if (!grabbableParentLookAt.IsBeingGrabbed && !GetDirectChildrenLookAtBeingGrabbed(grabbableParentLookAt).Any())
                {
                    rigidBodyToGrab = grabbableParentLookAt.RigidBodySource;
                }
            }

            if (rigidBodyToGrab)
            {
                rigidBodyToGrab.isKinematic = true;
            }

            if (handSwapSamePoint)
            {
                manipulationInfo.SwapGrabber(manipulationInfo.GetGrabberGrabbingPoint(grabPoint), grabber);
            }
            else if (handSwapDifferentPoints)
            {
                manipulationInfo.SwapGrabber(releasingGrabber, manipulationInfo.GetGrabbedPoint(releasingGrabber), grabber, grabPoint);
            }
            else if (moreThanOneHand)
            {
                manipulationInfo.RegisterNewGrab(grabber, grabPoint);
            }
            else
            {
                manipulationInfo = new RuntimeManipulationInfo(grabber, grabPoint, sourceAnchor);
                _currentManipulations.Add(grabbableObject, manipulationInfo);
            }

            // Re-parent

            if (!grabbableObject.IsConstrained && !moreThanOneHand)
            {
                if (grabbableObject.UseParenting)
                {
                    AssignGrabbedObjectParent(grabbableObject);
                }
            }

            manipulationInfo.NotifyBeginGrab(grabber, grabbableObject, grabPoint, grabberPosition, grabberRotation, sourceEventArgs);

            // Raise events

            if (releaseEventArgs != null)
            {
                OnObjectReleased(releaseEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    releasingGrabber.GrabbedObject.RaiseReleasedEvent(releaseEventArgs);
                }

                releasingGrabber.GrabbedObject = null;
            }

            OnObjectGrabbed(grabEventArgs, propagateEvents);

            if (propagateEvents)
            {
                grabbableObject.RaiseGrabbedEvent(grabEventArgs);
            }

            if (sourceAnchor && !grabbableObject.IsConstrained)
            {
                OnObjectRemoved(removeEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    sourceAnchor.RaiseRemovedEvent(removeEventArgs);
                }
            }

            // Here we use grabEventArgs instead of sourceEventArgs so that all clients are synchronized using the exact same grab position/orientation as
            // computed by the the original grab. This ensures a deterministic grab, otherwise a plain grab would compute the grab position/orientation
            // on each client and the different hand position/orientation would cause different results.
            EndSyncMethod(new object[] { grabber, grabbableObject, grabPoint, grabEventArgs, propagateEvents });
        }

        /// <summary>
        ///     Releases an object from a hand.
        /// </summary>
        /// <param name="grabber">
        ///     If non-null it will tell the grabber that releases the object. If it is null any grabber that is holding the object
        ///     will release it
        /// </param>
        /// <param name="grabbableObject">Object being released</param>
        /// <param name="position">The position to use when releasing, or null to compute it locally</param>
        /// <param name="rotation">The rotation to use when releasing, or null to compute it locally</param>
        /// <param name="velocity">The velocity to use when releasing, or null to compute it locally</param>
        /// <param name="angularVelocity">The angular velocity to use when releasing, or null to compute it locally</param>
        /// <param name="propagateEvents">Whether to propagate events</param>
        private void ReleaseObject(UxrGrabber grabber, UxrGrabbableObject grabbableObject, Vector3? position, Quaternion? rotation, Vector3? velocity, Vector3? angularVelocity, bool propagateEvents)
        {
            int grabbedPoint = GetGrabbedPoint(grabber);

            if (!UxrGrabber.EnabledComponents.Any(grb => grb.GrabbedObject == grabbableObject && (grb == grabber || grabber == null)))
            {
                return;
            }

            if (!_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.ManipulationModule} {nameof(RuntimeManipulationInfo)} not found for object {grabbableObject.name}. This should not be happening.");
                }

                return;
            }

            if (grabbableObject.enabled == false)
            {
                if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.ManipulationModule} {nameof(ReleaseObject)}, {nameof(UxrGrabbableObject)} component on {nameof(grabbableObject.name)} is disabled.");
                }
            }

            // This method will be synchronized. It will generate a new frame when recording a replay to ensure smooth interpolation.
            BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.GenerateNewFrame);

            UxrGrabbableObjectAnchor sourceAnchor           = manipulationInfo.SourceAnchor;
            bool                     isMultiHands           = manipulationInfo.Grabs.Count > 1;
            Vector3                  releasePosition        = position ?? (grabbableObject.RigidBodySource != null ? grabbableObject.RigidBodySource.transform.position : grabbableObject.transform.position);
            Quaternion               releaseRotation        = rotation ?? (grabbableObject.RigidBodySource != null ? grabbableObject.RigidBodySource.transform.rotation : grabbableObject.transform.rotation);
            Vector3                  releaseVelocity        = velocity ?? (grabber != null && !grabber.IsInSmoothManipulationTransition ? grabber.SmoothVelocity : Vector3.zero);
            Vector3                  releaseAngularVelocity = angularVelocity ?? (grabber != null && !grabber.IsInSmoothManipulationTransition ? grabber.SmoothAngularVelocity * Mathf.Deg2Rad : Vector3.zero);
            Vector2                  horizontal             = new Vector2(releaseVelocity.x, releaseVelocity.z);

            // Minimum amount of velocity in units/sec a component (hor/ver) needs to have in order to have the multiplier applied.
            // This will avoid objects being thrown super fast at low velocities.
            float multiplierVelocityThreshold = 2.5f;

            // In order to avoid the sudden change of velocity if an object is thrown just over/under the speed threshold, the
            // multiplier is applied using a gradient measured in units/second. 
            float multiplierVelocityGradient = 2.0f;

            if (grabbableObject.HorizontalReleaseMultiplier > 1.0f)
            {
                // we will apply the multiplier gradually depending on the release velocity starting from multiplierVelocityThreshold.
                // This is measured in units/sec so objects going below multiplierVelocityThreshold units/sec will have the normal
                // release velocity and objects above or equal to (multiplierVelocityThreshold + multiplierVelocityGradient) units/sec will
                // get the maximum multiplier. Velocities in between will have a smooth transition.
                if (horizontal.magnitude > multiplierVelocityThreshold)
                {
                    float lerp = Mathf.Clamp01((horizontal.magnitude - multiplierVelocityThreshold) * (1.0f / multiplierVelocityGradient));
                    horizontal = Vector3.Lerp(horizontal, horizontal * grabbableObject.HorizontalReleaseMultiplier, lerp);
                }
            }
            else
            {
                horizontal *= grabbableObject.HorizontalReleaseMultiplier;
            }

            releaseVelocity.x = horizontal.x;
            releaseVelocity.z = horizontal.y;

            if (grabbableObject.VerticalReleaseMultiplier > 1.0f)
            {
                // Apply multiplier in the same gradual way as the horizontal component.
                if (Mathf.Abs(releaseVelocity.y) > multiplierVelocityThreshold)
                {
                    float lerp = Mathf.Clamp01((Mathf.Abs(releaseVelocity.y) - multiplierVelocityThreshold) * (1.0f / multiplierVelocityGradient));
                    releaseVelocity.y = Mathf.Lerp(releaseVelocity.y, releaseVelocity.y * grabbableObject.VerticalReleaseMultiplier, lerp);
                }
            }
            else
            {
                releaseVelocity.y *= grabbableObject.VerticalReleaseMultiplier;
            }
            
            // Process and raise event(s)

            // Avoid creating list of multiple releases if the release is just a single grabber
            List<(UxrGrabber, int)> multipleReleases = grabber != null ? new List<(UxrGrabber, int)>() : null; 

            foreach (UxrGrabber grb in manipulationInfo.Grabbers)
            {
                if (grb == grabber || grabber == null)
                {
                    int grbPoint = GetGrabbedPoint(grb);

                    UxrManipulationEventArgs releasingEventArgs = UxrManipulationEventArgs.FromRelease(grabbableObject,
                                                                                                       sourceAnchor,
                                                                                                       grb,
                                                                                                       grbPoint,
                                                                                                       isMultiHands && grabber != null,
                                                                                                       isMultiHands && grabber != null,
                                                                                                       releaseVelocity,
                                                                                                       releaseAngularVelocity);

                    OnObjectReleasing(releasingEventArgs, propagateEvents);

                    if (grabbableObject)
                    {
                        if (propagateEvents)
                        {
                            grabbableObject.RaiseReleasingEvent(releasingEventArgs);
                        }

                        manipulationInfo.NotifyEndGrab(grb, grabbableObject, grbPoint);
                    }

                    grb.GrabbedObject = null;

                    multipleReleases?.Add((grb, grbPoint));
                }
            }
            
            // Check if the object's rigidbody needs to be made dynamic

            Rigidbody rigidBodyToRelease = null;

            if (isMultiHands == false || grabber == null)
            {
                _currentManipulations.Remove(grabbableObject);

                if (grabbableObject.RigidBodySource != null && grabbableObject.CanUseRigidBody && grabbableObject.RigidBodyDynamicOnRelease)
                {
                    if (!GetDirectChildrenLookAtBeingGrabbed(grabbableObject).Any())
                    {
                        rigidBodyToRelease = grabbableObject.RigidBodySource;
                    }
                }
            }

            // Check if the object's parent grabbable rigidbody needs to be made dynamic, if this is the last grab that keeps holding it

            UxrGrabbableObject grabbableParentLookAt = grabbableObject.ParentLookAts.FirstOrDefault();

            if (rigidBodyToRelease == null && grabbableParentLookAt != null && grabbableParentLookAt.RigidBodySource != null && grabbableParentLookAt.CanUseRigidBody && grabbableParentLookAt.RigidBodyDynamicOnRelease)
            {
                if (!grabbableParentLookAt.IsBeingGrabbed && !GetDirectChildrenLookAtBeingGrabbed(grabbableParentLookAt).Any())
                {
                    rigidBodyToRelease = grabbableParentLookAt.RigidBodySource;
                }
            }

            // If there is a rigidBody involved, make it dynamic

            if (rigidBodyToRelease != null)
            {
                rigidBodyToRelease.isKinematic = false;
                rigidBodyToRelease.position    = releasePosition;
                rigidBodyToRelease.rotation    = releaseRotation;

                if (releaseVelocity.IsValid())
                {
                    rigidBodyToRelease.AddForce(releaseVelocity, ForceMode.VelocityChange);
                }

                if (releaseAngularVelocity.IsValid())
                {
                    rigidBodyToRelease.AddTorque(releaseAngularVelocity, ForceMode.VelocityChange);
                }
            }
            
            // Remove grabber(s)
            
            if (grabber == null)
            {
                manipulationInfo.RemoveAll();
            }
            else
            {
                manipulationInfo.RemoveGrab(grabber);
            }

            // Raise event(s)

            if (multipleReleases != null)
            {
                foreach ((UxrGrabber, int) releaseInfo in multipleReleases)
                {
                    RaiseReleaseEvent(releaseInfo.Item1, releaseInfo.Item2);    
                }
            }
            else
            {
                RaiseReleaseEvent(grabber, grabbedPoint);
            }

            void RaiseReleaseEvent(UxrGrabber targetGrabber, int targetGrabPoint)
            {
                UxrManipulationEventArgs releasedEventArgs = UxrManipulationEventArgs.FromRelease(grabbableObject,
                                                                                                  sourceAnchor,
                                                                                                  targetGrabber,
                                                                                                  targetGrabPoint,
                                                                                                  isMultiHands && grabber != null,
                                                                                                  isMultiHands && grabber != null,
                                                                                                  releaseVelocity,
                                                                                                  releaseAngularVelocity);

                OnObjectReleased(releasedEventArgs, propagateEvents);

                if (grabbableObject && propagateEvents)
                {
                    grabbableObject.RaiseReleasedEvent(releasedEventArgs);
                }
            }

            EndSyncMethod(new object[] { grabber, grabbableObject, releasePosition, releaseRotation, releaseVelocity, releaseAngularVelocity, propagateEvents });
        }

        /// <summary>
        ///     Assigns the new parent of a grabbable object that was grabbed.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to assign a new parent to</param>
        private void AssignGrabbedObjectParent(UxrGrabbableObject grabbableObject)
        {
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                // Will assign the parent of the first avatar that grabbed the object.
                ChangeGrabbableObjectParent(grabbableObject, manipulationInfo.Grabbers.First().Avatar.transform.parent);
            }
        }

        /// <summary>
        ///     Changes the parent of a grabbable object. We do it using this method to keep track of re-parenting
        ///     so that we can recalculate internal data if necessary.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to change the parent of</param>
        /// <param name="newParent">New parent</param>
        private void ChangeGrabbableObjectParent(UxrGrabbableObject grabbableObject, Transform newParent)
        {
            Vector3    positionBeforeUpdate = TransformExt.GetWorldPosition(grabbableObject.transform.parent, grabbableObject.LocalPositionBeforeUpdate);
            Quaternion rotationBeforeUpdate = TransformExt.GetWorldRotation(grabbableObject.transform.parent, grabbableObject.LocalRotationBeforeUpdate);

            grabbableObject.transform.SetParent(newParent, true);

            grabbableObject.LocalPositionBeforeUpdate = TransformExt.GetLocalPosition(newParent, positionBeforeUpdate);
            grabbableObject.LocalRotationBeforeUpdate = TransformExt.GetLocalRotation(newParent, rotationBeforeUpdate);
        }

        /// <summary>
        ///     Tries to grab something using the given grabber.
        /// </summary>
        /// <param name="grabber">Grabber to try grabbing with</param>
        private void TryGrab(UxrGrabber grabber)
        {
            if (grabber.GrabbedObject != null)
            {
                // We have already something grabbed, this means we have a grab point with toggle grab mode or keep always

                foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabber.GrabbedObject))
                {
                    if (grabInfo.Grabber == grabber && grabber.GrabbedObject.GetGrabPoint(grabInfo.GrabbedPoint).GrabMode == UxrGrabMode.GrabAndKeepAlways)
                    {
                        // Grab is in "Keep always" mode
                        return;
                    }
                }

                // It is in Toggle grab mode
                NotifyReleaseGrab(grabber, true);
                return;
            }

            OnGrabTrying(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.GrabTrying, null, null, grabber), true);

            if (IsGrabbingAllowed == false)
            {
                return;
            }

            // A grab gesture has been made. Try to find possible objects that have been grabbed

            if (GetClosestGrabbableObject(grabber.Avatar, grabber.Side, out UxrGrabbableObject grabbableObject, out int grabPoint))
            {
                // There's a grabbed object!
                GrabObject(grabber, grabbableObject, grabPoint, true);
            }
        }

        /// <summary>
        ///     Notifies that a grip is released and checks if the object needs to be released or placed on an anchor.
        /// </summary>
        /// <param name="grabber">Grabber to release the object from</param>
        /// <param name="fromToggle">Whether the release was from a <see cref="UxrGrabMode.GrabToggle" /></param>
        private void NotifyReleaseGrab(UxrGrabber grabber, bool fromToggle = false)
        {
            // A release gesture has been made. Check for possible object placements / drop
            if (grabber.GrabbedObject != null)
            {
                // First check if the grabbed point has toggle mode or keep always mode. In that case we should not release the object but keep it in the grabbed list

                if (_currentManipulations.TryGetValue(grabber.GrabbedObject, out RuntimeManipulationInfo manipulationInfo) && !fromToggle)
                {
                    int grabbedPoint = manipulationInfo.GetGrabbedPoint(grabber);

                    if (grabbedPoint != -1)
                    {
                        UxrGrabPointInfo grabPointInfo = grabber.GrabbedObject.GetGrabPoint(grabbedPoint);

                        if (grabPointInfo != null && (grabPointInfo.GrabMode == UxrGrabMode.GrabToggle || grabPointInfo.GrabMode == UxrGrabMode.GrabAndKeepAlways))
                        {
                            // Ignore release. We will keep grabbing it until another TryGrab or keep it grabbed always unless another grabber gets it.
                            return;
                        }
                    }
                }

                // If we only have a single hand left grabbing, find the closest compatible anchor candidate within the influence radius
                UxrGrabbableObjectAnchor anchorCandidate = null;

                if (manipulationInfo != null && manipulationInfo.Grabs.Count == 1 && grabber.GrabbedObject.IsPlaceable)
                {
                    float minDistance = float.MaxValue;

                    if (grabber.GrabbedObject.UsesGrabbableParentDependency == false)
                    {
                        foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
                        {
                            if (grabber.GrabbedObject.CanBePlacedOnAnchor(anchorPair.Key, out float distance) && distance < minDistance)
                            {
                                anchorCandidate = anchorPair.Key;
                                minDistance     = distance;
                            }
                        }
                    }
                }

                // Execute

                if (anchorCandidate == null)
                {
                    ReleaseObject(grabber, grabber.GrabbedObject, true);
                }
                else
                {
                    PlaceObject(grabber.GrabbedObject, anchorCandidate, UxrPlacementOptions.Smooth, true);
                }

                grabber.GrabbedObject = null;
            }
        }

        /// <summary>
        ///     <para>
        ///         Handles an object being manipulated. This method is called by <see cref="UpdateManipulation" /> with objects
        ///         sorted from top to bottom grabbable hierarchy, starting with those with the most grabbable children
        ///         in decreasing <see cref="UxrGrabbableObject.AllChildren" /> count order.
        ///     </para>
        ///     <para>
        ///         The algorithm has the following steps:
        ///         <list type="bullet">
        ///             <item>
        ///                 If the object uses <see cref="UxrRotationProvider.HandOrientation" />, it is solved. If it uses
        ///                 <see cref="UxrRotationProvider.HandPositionAroundPivot" />, is is only moved without any rotation. It
        ///                 can't be fully solved at this point without knowing the rotation pivot, which may depend on a parent,
        ///                 and the parent only can be solved after all children. Moving it allows the parent later to know at
        ///                 least where to rotate to.
        ///             </item>
        ///             <item>
        ///                 If there are any parent grabbable objects whose direction when grabbed is controlled by children (
        ///                 <see cref="UxrGrabbableObject.ControlParentDirection" /> used in children grabbable objects), these
        ///                 directions (look-at) in the parent are solved by the last grabbed child's
        ///                 <see cref="ProcessManipulation" />.
        ///             </item>
        ///             <item>
        ///                 Parent look-ats will be:
        ///                 <list>
        ///                     <item>
        ///                         Parent grab to child grabs look-at when the parent is also grabbed and uses
        ///                         <see cref="UxrRotationProvider.HandOrientation" />.
        ///                     </item>
        ///                     <item>
        ///                         Keep position/rotation relative to the children when the parent is not grabbed and uses
        ///                         <see cref="UxrRotationProvider.HandOrientation" />.
        ///                     </item>
        ///                     <item>
        ///                         Parent pivot to child grabs look-at when the parent (grabbed or not) uses
        ///                         <see cref="UxrRotationProvider.HandPositionAroundPivot" />.
        ///                     </item>
        ///                 </list>
        ///             </item>
        ///             <item>
        ///                 A post-process is performed for all objects in this order:
        ///                 <list>
        ///                     <item>
        ///                         The object has <see cref="UxrGrabbableObject.ControlParentDirection" /> grabbed children and
        ///                         the last child was processed.
        ///                     </item>
        ///                     <item>
        ///                         The object controls a parent direction and the parent's last child was processed.
        ///                     </item>
        ///                     <item>
        ///                         The object doesn't control any parent nor has any child that controls it.
        ///                     </item>
        ///                 </list>
        ///             </item>
        ///             <item>
        ///                 Post-processing steps:
        ///                 <list>
        ///                     <item>
        ///                         Solve <see cref="UxrRotationProvider.HandPositionAroundPivot" /> object.
        ///                     </item>
        ///                     <item>
        ///                         Apply object constraints and rotation/position resistance.
        ///                     </item>
        ///                 </list>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="grabbableObject">The object being grabbed</param>
        private void ProcessManipulation(UxrGrabbableObject grabbableObject)
        {
            if (!_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return;
            }

            UxrGrabbableObject grabbableParent         = grabbableObject.GrabbableParent;
            bool               controlsParentDirection = grabbableParent != null && grabbableObject.UsesGrabbableParentDependency && grabbableObject.ControlParentDirection;

            if (controlsParentDirection)
            {
                foreach (RuntimeGrabInfo grabInfo in manipulationInfo.Grabs)
                {
                    grabInfo.ParentLocalGrabPositionBeforeUpdate = grabbableParent.transform.InverseTransformPoint(GetGrabbedPointGrabAlignPosition(grabInfo.Grabber));
                    grabInfo.ChildLocalParentPosition            = grabbableObject.transform.InverseTransformPoint(grabbableParent.transform.position);
                    grabInfo.ChildLocalParentRotation            = Quaternion.Inverse(grabbableObject.transform.rotation) * grabbableParent.transform.rotation;
                }
            }

            switch (grabbableObject.RotationProvider)
            {
                case UxrRotationProvider.HandOrientation:

                    SolveHandOrientationManipulation(grabbableObject, manipulationInfo);
                    break;

                case UxrRotationProvider.HandPositionAroundPivot:

                    SolveSimplePositionManipulation(grabbableObject, manipulationInfo);
                    break;
            }

            if (controlsParentDirection)
            {
                foreach (RuntimeGrabInfo grabInfo in manipulationInfo.Grabs)
                {
                    grabInfo.ParentLocalGrabPositionAfterUpdate = grabbableParent.transform.InverseTransformPoint(GetGrabbedPointGrabAlignPosition(grabInfo.Grabber));
                }

                grabbableParent.DirectLookAtChildProcessedCount++;
            }

            // Check if we need to solve the parent

            if (controlsParentDirection && grabbableParent.DirectLookAtChildProcessedCount == grabbableParent.DirectLookAtChildGrabbedCount)
            {
                // This was the last child processed that controls a grabbable parent direction: Perform look-at.

                // Store currently grabbed child transforms to compute constraints correctly.

                foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableParent))
                {
                    childManipulation.GrabbableObject.PushTransform();
                }

                // Handle the parent grabbable object

                _currentManipulations.TryGetValue(grabbableParent, out RuntimeManipulationInfo parentManipulationInfo);

                if ((parentManipulationInfo != null && grabbableParent.RotationProvider == UxrRotationProvider.HandOrientation) || grabbableParent.RotationProvider == UxrRotationProvider.HandPositionAroundPivot)
                {
                    // Perform lookAt if parent is being grabbed or if not grabbed but rotation provider is HandPositionAroundPivot

                    if (grabbableParent.RotationProvider == UxrRotationProvider.HandPositionAroundPivot)
                    {
                        // In this path we're going to average all individual rotations
                        
                        foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableParent))
                        {
                            foreach (RuntimeGrabInfo grabInfo in childManipulation.Grabs)
                            {
                                grabInfo.ParentGrabbableLeverageContribution = (childManipulation.GrabbableObject.transform.TransformPoint(grabInfo.ParentGrabbableLookAtLocalLeveragePoint) - grabbableParent.transform.position).normalized;
                            }
                        }

                        grabbableParent.transform.localRotation = grabbableParent.InitialLocalRotation;

                        Vector3 singleRotationAxis = grabbableParent.SingleRotationAxisIndex != -1 ? grabbableParent.transform.TransformDirection((UxrAxis)grabbableParent.SingleRotationAxisIndex) : Vector3.zero;

                        foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableParent))
                        {
                            foreach (RuntimeGrabInfo grabInfo in childManipulation.Grabs)
                            {
                                Vector3 originalLookAt = (grabbableParent.transform.TransformPoint(grabInfo.ParentGrabbableLookAtParentLeveragePoint) - grabbableParent.transform.position).normalized;
                                Vector3 desiredLookAt  = grabInfo.ParentGrabbableLeverageContribution;

                                if (grabbableParent.SingleRotationAxisIndex != -1)
                                {
                                    // This makes sure that in parents where there is a single rotation axis, the axis is aligned correctly.

                                    originalLookAt = Vector3.ProjectOnPlane(originalLookAt, singleRotationAxis).normalized;
                                    desiredLookAt  = Vector3.ProjectOnPlane(desiredLookAt,  singleRotationAxis).normalized;

                                    float angle = Vector3.SignedAngle(originalLookAt, desiredLookAt, singleRotationAxis);

                                    grabInfo.ParentGrabbableLookAtRotationContribution = Quaternion.AngleAxis(angle, singleRotationAxis) * grabbableParent.transform.rotation;
                                }
                                else
                                {
                                    Vector3 rotationAxis = Vector3.Cross(originalLookAt, desiredLookAt);                                    
                                    grabInfo.ParentGrabbableLookAtRotationContribution = Quaternion.AngleAxis(Vector3.SignedAngle(originalLookAt, desiredLookAt, singleRotationAxis), rotationAxis);
                                }
                            }
                        }

                        Quaternion averageRotation = QuaternionExt.Average(GetDirectChildrenLookAtManipulations(grabbableParent).SelectMany(m => m.Grabs.Select(g => g.ParentGrabbableLookAtRotationContribution)));
                        grabbableParent.transform.rotation = averageRotation;
                    }
                    else
                    {
                        // Parent is grabbed and rotation provider is HandOrientation: Average all child dependencies for the lookAt
                        // TODO: Ideally we would average all rotation contributions to avoid opposite directions cancelling each other. 

                        Vector3 oldTotal      = Vector3.zero;
                        Vector3 newTotal      = Vector3.zero;
                        int     contributions = 0;

                        foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableParent))
                        {
                            foreach (RuntimeGrabInfo grabInfo in childManipulation.Grabs)
                            {
                                oldTotal += grabInfo.ParentLocalGrabPositionBeforeUpdate;
                                newTotal += grabInfo.ParentLocalGrabPositionAfterUpdate;
                                contributions++;
                            }
                        }

                        // Perform the lookAt.The rotation pivot will be the parent grabs. 

                        if (contributions > 0)
                        {
                            Vector3 worldLookAtPivot = grabbableParent.transform.TransformPoint(parentManipulationInfo.LocalManipulationRotationPivot);
                            Vector3 currentLookAt    = grabbableParent.transform.TransformPoint(oldTotal / contributions) - worldLookAtPivot;
                            Vector3 desiredLookAt    = grabbableParent.transform.TransformPoint(newTotal / contributions) - worldLookAtPivot;

                            Vector3 rotationAxis = Vector3.Cross(currentLookAt, desiredLookAt);
                            grabbableParent.transform.RotateAround(worldLookAtPivot, rotationAxis, Vector3.SignedAngle(currentLookAt, desiredLookAt, rotationAxis));
                        }
                    }
                }
                else
                {
                    // Parent is not being grabbed and rotation provider is not HandPositionAroundPivot: average using look ats and position averaging.
                    
                    IEnumerable<RuntimeGrabInfo> childGrabContributions = GetDirectChildrenLookAtManipulations(grabbableParent).SelectMany(m => m.Grabs);

                    if (childGrabContributions.Any())
                    {
                        SolveUsingLookAtAveraging(grabbableParent, null, childGrabContributions.First().Grabber, childGrabContributions, true);
                    }
                }

                // Finalize parent

                FinalizeManipulation(grabbableParent, parentManipulationInfo);

                // Finalize children

                foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableParent))
                {
                    childManipulation.GrabbableObject.PopTransform();
                    FinalizeManipulation(childManipulation.GrabbableObject, childManipulation);
                }
            }

            // If the object has grabbable parent dependencies, and no child dependencies, the object will be finalized by the last parent's child dependency
            // If the object has grabbable child dependencies the object will be finalized by the last child 
            // If there are no dependencies, finalize now

            if (!controlsParentDirection && grabbableObject.DirectLookAtChildGrabbedCount == 0)
            {
                FinalizeManipulation(grabbableObject, manipulationInfo);
            }
        }

        /// <summary>
        ///     Finalizes a manipulation:
        ///     <list type="bullet">
        ///         <item>Solves <see cref="UxrRotationProvider.HandPositionAroundPivot" /> rotation constraint mode</item>
        ///         <item>Applies constraints</item>
        ///     </list>
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="manipulationInfo">Manipulation info</param>
        private void FinalizeManipulation(UxrGrabbableObject grabbableObject, RuntimeManipulationInfo manipulationInfo)
        {
            if (grabbableObject.RotationProvider == UxrRotationProvider.HandPositionAroundPivot)
            {
                // Constrain translation in a first pass. In grabbed objects, the local position was moved by SolveSimplePositionManipulation to
                // help the parent lookAt if it exists. In parent grabbable objects that are not being grabbed, it clamps translation due to
                // grabbed children.
                // It also is useful if the object position isn't locked since SolveHandAroundPivotManipulation() only takes care of the rotation.
                ConstrainTransform(grabbableObject, UxrTransformations.Translate);
                SolveHandAroundPivotManipulation(grabbableObject, manipulationInfo);
            }
            else
            {
                // Constraints in HandPositionAroundPivot are already computed by SolveHandAroundPivotManipulation().  
                ConstrainTransform(grabbableObject);
            }

            // Apply resistance

            if (Features.HasFlag(UxrManipulationFeatures.ObjectResistance))
            {
                grabbableObject.transform.SetLocalPositionAndRotation(UxrInterpolator.SmoothDampPosition(grabbableObject.LocalPositionBeforeUpdate, grabbableObject.transform.localPosition, grabbableObject.TranslationResistance),
                                                                      UxrInterpolator.SmoothDampRotation(grabbableObject.LocalRotationBeforeUpdate, grabbableObject.transform.localRotation, grabbableObject.RotationResistance));
            }
        }

        /// <summary>
        ///     Solves the manipulation for an object applying simple translation without rotation or snapping. This is used for
        ///     <see cref="UxrRotationProvider.HandPositionAroundPivot" /> in a first pass to place the object approximately in
        ///     space for potential parent objects to know where to rotate to.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to solve</param>
        /// <param name="manipulationInfo">Manipulation information</param>
        /// <remarks>Object is assigned a new position/rotation without considering any constraints</remarks>
        private void SolveSimplePositionManipulation(UxrGrabbableObject grabbableObject, RuntimeManipulationInfo manipulationInfo)
        {
            Vector3 total         = Vector3.zero;
            int     contributions = 0;

            foreach (RuntimeGrabInfo grabInfo in manipulationInfo.Grabs)
            {
                total += grabInfo.Grabber.transform.TransformPoint(GetGrabPointRelativeGrabPosition(grabInfo.Grabber));
                contributions++;
            }

            if (contributions > 0)
            {
                grabbableObject.transform.position = total / contributions;
            }
        }

        /// <summary>
        ///     Solves the manipulation for an object that has <see cref="UxrRotationProvider.HandOrientation" /> as rotation
        ///     provider.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to solve</param>
        /// <param name="manipulationInfo">Manipulation information</param>
        /// <remarks>Object is assigned a new position/rotation without considering any constraints</remarks>
        private void SolveHandOrientationManipulation(UxrGrabbableObject grabbableObject, RuntimeManipulationInfo manipulationInfo)
        {
            UxrGrabber firstGrabber     = manipulationInfo.Grabbers.First();
            UxrGrabber mainGrabber      = grabbableObject.FirstGrabPointIsMain ? manipulationInfo.GetGrabberGrabbingPoint(0) : null;
            bool       recenterEachGrab = true;

            if (mainGrabber != null)
            {
                // If the first grab point is considered as main, the grabber that holds the main grab point will keep its position while the others
                // will only affect the rotation.
                // If the first grab point is not the main, and all grabs have the same importance, then the object will be centered between them.
                firstGrabber     = mainGrabber;
                recenterEachGrab = false;
            }

            SolveUsingLookAtAveraging(grabbableObject, manipulationInfo, firstGrabber, manipulationInfo.Grabs, recenterEachGrab);
        }

        /// <summary>
        ///     Solves an object manipulation so that as more grabs are added, the object will rotate towards those grabs to
        ///     average each contribution.
        ///     To solve the position, <paramref name="averagePosition" /> is used.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to solve</param>
        /// <param name="manipulationInfo">Grabbable object manipulation info. Can be null when solving grabbable parents that are not being grabbed, but whose children are</param>
        /// <param name="firstGrabber">First grabber to be solved in the order</param>
        /// <param name="grabs">Total grabs, which may include or not <paramref name="firstGrabber" /></param>
        /// <param name="averagePosition">
        ///     If true, all contributions will be averaged. If false, the first grab will be used to
        ///     solve the position and all the other grabs will only contribute with the rotation averaging
        /// </param>
        private void SolveUsingLookAtAveraging(UxrGrabbableObject grabbableObject, RuntimeManipulationInfo manipulationInfo, UxrGrabber firstGrabber, IEnumerable<RuntimeGrabInfo> grabs, bool averagePosition)
        {
            if (grabs == null || !grabs.Any())
            {
                return;
            }

            // Place object in first grabber to process

            if (manipulationInfo != null)
            {
                // The object itself is being grabbed
                PlaceObjectInHand(grabbableObject, firstGrabber);    
            }
            else
            {
                // It's a parent object, not being grabbed, with children being grabbed. Use the child relative position to place it in the first hand.
                
                RuntimeGrabInfo firstGrabInfo = grabs.FirstOrDefault(g => g.Grabber == firstGrabber);

                if (firstGrabInfo != null && firstGrabber.GrabbedObject != null && Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                {
                    grabbableObject.transform.position = firstGrabber.GrabbedObject.transform.TransformPoint(firstGrabInfo.ChildLocalParentPosition);
                    grabbableObject.transform.rotation = firstGrabber.GrabbedObject.transform.rotation * firstGrabInfo.ChildLocalParentRotation;
                }
            }

            UxrGrabber previousGrabber = firstGrabber;
            Vector3    pivotSum        = firstGrabber.transform.position;
            int        grabSumCount    = 1;

            // Now iterate over the rest of the grabs, rotating the object towards each grab.
            // The first 3 grabs will place the object on the plane determined by the 3 points.
            // Subsequent grabs will rotate the object less each time.

            foreach (RuntimeGrabInfo grabInfo in grabs)
            {
                if (grabInfo.Grabber == firstGrabber)
                {
                    continue;
                }

                Vector3 rotationPivot  = pivotSum / grabSumCount;
                float   rotationAmount = grabSumCount < 3 ? 1.0f : 1.0f / grabSumCount;

                if (manipulationInfo == null)
                {
                    // The parent is not being grabbed. We need to constrain the child transform early so that RotateObjectTowardsGrab
                    // works using the correct lookAt direction.
                    ConstrainTransform(grabInfo.Grabber.GrabbedObject);
                }

                if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                {
                    RotateObjectTowardsGrab(grabbableObject, pivotSum / grabSumCount, grabInfo.Grabber, rotationAmount);
                }

                if (averagePosition)
                {
                    // Recenter object so that it keeps centered among grabs

                    Vector3 snapPosition    = GetGrabbedPointGrabAlignPosition(grabInfo.Grabber);
                    float   grabberDistance = Vector3.Distance(previousGrabber.transform.position, grabInfo.Grabber.transform.position);
                    float   snapDistance    = Vector3.Distance(snapPosition,                       GetGrabbedPointGrabAlignPosition(previousGrabber));

                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.position += (snapPosition - rotationPivot).normalized * ((grabberDistance - snapDistance) * 0.5f * rotationAmount);
                    }
                }

                pivotSum += grabInfo.Grabber.transform.position;
                grabSumCount++;
                previousGrabber = grabInfo.Grabber;
            }

            // Compute the rotation pivot used when child objects control this object's direction. 

            if (manipulationInfo != null)
            {
                manipulationInfo.LocalManipulationRotationPivot = grabbableObject.transform.InverseTransformPoint(pivotSum / grabSumCount);
            }
        }

        /// <summary>
        ///     Solves the manipulation for an object that has <see cref="UxrRotationProvider.HandPositionAroundPivot" /> as
        ///     rotation provider.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to solve</param>
        /// <param name="manipulationInfo">
        ///     Grabbable object manipulation information. Can be null for parent grabbable objects that
        ///     aren't grabbed but are grabbed by child objects
        /// </param>
        /// <remarks>Object is assigned a new position/rotation without considering any constraints</remarks>
        private void SolveHandAroundPivotManipulation(UxrGrabbableObject grabbableObject, RuntimeManipulationInfo manipulationInfo)
        {
            Vector3 worldPosition = grabbableObject.transform.position;

            int rangeOfMotionAxisCount = grabbableObject.RangeOfMotionRotationAxisCount;

            // Rotation: We use the angle between V1(pivot, initial grab position) and V2 (pivot, current grab position).
            //           This method works better for levers, steering wheels, etc. It won't work well with elements
            //           like knobs or similar because the point where the torque is applied lies in the rotation axis itself.
            //           In this cases we recommend using ManipulationMode.GrabAndMove instead.

            if (rangeOfMotionAxisCount == 1)
            {
                // Object rotation is constrained to a single axis

                UxrAxis rotationAxis = grabbableObject.SingleRotationAxisIndex;

                // We iterate over all current grabbers to compute each contribution

                IEnumerable<RuntimeGrabInfo> grabs             = GetGrabs(grabbableObject);
                int                          contributionCount = grabs.Count();

                if (grabbableObject.NeedsTwoHandsToRotate && contributionCount < 2)
                {
                    return;
                }

                foreach (RuntimeGrabInfo grabInfo in grabs)
                {
                    // Compute values in world coordinates first

                    Vector3 grabDirection        = grabInfo.Grabber.transform.TransformPoint(grabInfo.GrabberLocalLeverageSource) - worldPosition;
                    Vector3 initialGrabDirection = TransformExt.GetWorldPosition(grabbableObject.transform.parent, grabInfo.GrabberLocalParentLeverageSourceOnGrab) - worldPosition;

                    // Transform to local coordinates

                    grabDirection        = Quaternion.Inverse(grabbableObject.transform.GetParentRotation()) * grabDirection;
                    initialGrabDirection = Quaternion.Inverse(grabbableObject.transform.GetParentRotation()) * initialGrabDirection;

                    // When there's a single axis with range of motion, we use additional computations to be able to specify ranges below/above -180/180 degrees

                    Vector3 projectedGrabDirection        = Vector3.ProjectOnPlane(grabDirection,        grabbableObject.LocalRotationBeforeUpdate * rotationAxis);
                    Vector3 projectedInitialGrabDirection = Vector3.ProjectOnPlane(initialGrabDirection, grabbableObject.LocalRotationBeforeUpdate * rotationAxis);

                    float angle      = Vector3.SignedAngle(projectedInitialGrabDirection, projectedGrabDirection, grabbableObject.LocalRotationBeforeUpdate * rotationAxis);
                    float angleDelta = angle - grabInfo.SingleRotationAngleContribution.ToEuler180();

                    // Keep track of turns below/above -360/360 degrees.

                    if (angleDelta > 180.0f)
                    {
                        grabInfo.SingleRotationAngleContribution -= 360.0f - angleDelta;
                    }
                    else if (angleDelta < -180.0f)
                    {
                        grabInfo.SingleRotationAngleContribution += 360.0f + angleDelta;
                    }
                    else
                    {
                        grabInfo.SingleRotationAngleContribution += angleDelta;
                    }
                }

                if (manipulationInfo != null)
                {
                    // Object is grabbed.

                    // Clamp inside valid range.

                    float unclampedAngle = grabbableObject.SingleRotationAngleCumulative + manipulationInfo.CurrentSingleRotationAngleContributions;
                    float clampedAngle   = unclampedAngle.Clamped(grabbableObject.RotationAngleLimitsMin[rotationAxis], grabbableObject.RotationAngleLimitsMax[rotationAxis]);

                    // Rotate using absolute current rotation to preserve precision

                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.localRotation = grabbableObject.InitialLocalRotation * Quaternion.AngleAxis(clampedAngle, rotationAxis);
                    }

                    // Subtract clamping equally among contributors

                    float contributionExcess = unclampedAngle - clampedAngle;

                    if (contributionCount > 1)
                    {
                        contributionExcess /= contributionCount;
                    }

                    foreach (RuntimeGrabInfo grabInfo in grabs)
                    {
                        grabInfo.SingleRotationAngleContribution -= contributionExcess;
                    }
                }
                else
                {
                    // Object is not grabbed directly, but through children that have already been constrained. Keep track of rotation.

                    float singleRotationAngleCumulative = grabbableObject.SingleRotationAngleCumulative;

                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.localRotation = ClampRotation(grabbableObject,
                                                                                grabbableObject.transform.localRotation,
                                                                                grabbableObject.LocalRotationBeforeUpdate,
                                                                                grabbableObject.InitialLocalRotation,
                                                                                grabbableObject.RotationAngleLimitsMin,
                                                                                grabbableObject.RotationAngleLimitsMax,
                                                                                false,
                                                                                ref singleRotationAngleCumulative);
                    }
                    
                    grabbableObject.SingleRotationAngleCumulative = singleRotationAngleCumulative;
                }
            }
            else
            {
                // More than one rotational degree of motion: Compute all grabs and average the result.

                IEnumerable<Quaternion> GetAllLocalRotationContributions()
                {
                    foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabbableObject))
                    {
                        // Here we can potentially have up to 3 rotational ranges of motion but we use the hand position around the pivot to rotate the object, so we need to be
                        // extra careful with not losing any information when computing the rotation and clamping.

                        Vector3 grabDirection        = grabInfo.Grabber.transform.TransformPoint(grabInfo.GrabberLocalLeverageSource) - worldPosition;
                        Vector3 initialGrabDirection = TransformExt.GetWorldPosition(grabbableObject.transform.parent, grabInfo.GrabberLocalParentLeverageSourceOnGrab) - worldPosition;

                        // First compute the rotation of the grabbed object if it were to be controlled by the hand orientation
                        Quaternion rotUsingHandOrientation = grabInfo.Grabber.transform.rotation * GetGrabPointRelativeGrabRotation(grabInfo.Grabber);

                        // Now compute the rotation of the grabbed object if we were to use the hand position around the axis. But we do not use this directly because we
                        // potentially lose the rotation around the longitudinal axis if there is one. We use it instead to know where the longitudinal axis will point,
                        // and correct rotUsingHandOrientation.
                        Quaternion rotationOnGrab            = grabbableObject.transform.GetParentRotation() * grabInfo.LocalRotationOnGrab;
                        Quaternion rotUsingHandPosAroundAxis = Quaternion.FromToRotation(initialGrabDirection.normalized, grabDirection.normalized) * rotationOnGrab;
                        Quaternion rotCorrection             = Quaternion.FromToRotation(rotUsingHandOrientation * grabbableObject.RotationLongitudinalAxis, rotUsingHandPosAroundAxis * grabbableObject.RotationLongitudinalAxis);
                        Quaternion localRotation             = Quaternion.Inverse(grabbableObject.transform.GetParentRotation()) * rotCorrection * rotUsingHandOrientation;

                        yield return localRotation;
                    }
                }

                Quaternion localRotationAverage = QuaternionExt.Average(GetAllLocalRotationContributions(), grabbableObject.transform.localRotation);

                if (grabbableObject.RotationConstraint == UxrRotationConstraintMode.Free)
                {
                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.localRotation = localRotationAverage;
                    }
                }
                else
                {
                    float singleRotationAngleCumulative = grabbableObject.SingleRotationAngleCumulative;

                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.localRotation = ClampRotation(grabbableObject,
                                                                                localRotationAverage,
                                                                                grabbableObject.LocalRotationBeforeUpdate,
                                                                                grabbableObject.InitialLocalRotation,
                                                                                grabbableObject.RotationAngleLimitsMin,
                                                                                grabbableObject.RotationAngleLimitsMax,
                                                                                false,
                                                                                ref singleRotationAngleCumulative);
                    }
                    
                    grabbableObject.SingleRotationAngleCumulative = singleRotationAngleCumulative;
                }
            }
        }

        /// <summary>
        ///     Places the object in a given hand, considering the object snapping parameters.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to place</param>
        /// <param name="grabber">Grabber to place the object in</param>
        /// <remarks>
        ///     Object is assigned a new position/rotation and constraints are applied only if it doesn't control a grabbable
        ///     parent direction.
        ///     The reason constraints are applied in this case is because it's better to do it in two separate steps while
        ///     snapping to the grabber than in a single step afterwards. More info commented in code inside.
        /// </remarks>
        private void PlaceObjectInHand(UxrGrabbableObject grabbableObject, UxrGrabber grabber)
        {
            if (!_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return;
            }

            // Default positioning without any snapping

            if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
            {
                grabbableObject.transform.SetPositionAndRotation(grabber.transform.TransformPoint(GetGrabPointRelativeGrabPosition(grabber)), grabber.transform.rotation * GetGrabPointRelativeGrabRotation(grabber));
            }

            // Now process snapping. We compute the required transformations from the current object snapping transform to the grabber. 

            RuntimeGrabInfo  grabInfo      = manipulationInfo.GetGrabInfo(grabber);
            UxrGrabPointInfo grabPointInfo = grabbableObject.GetGrabPoint(grabInfo.GrabbedPoint);

            Vector3    sourcePosition = TransformExt.GetWorldPosition(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignPosition);
            Quaternion sourceRotation = TransformExt.GetWorldRotation(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignRotation);
            Quaternion targetRotation = grabber.transform.rotation;

            if (grabPointInfo.AlignToController && grabber != null && grabber.Avatar != null && grabber.Avatar.AvatarMode == UxrAvatarMode.Local)
            {
                // Align the object to the controller. Useful for weapons or things that need directional precision.
                // In externally updated avatars (multiplayer, replays...), this doesn't need to be computed because the hand position/rotation is already sampled.

                sourceRotation = grabPointInfo.AlignToControllerAxes != null ? grabPointInfo.AlignToControllerAxes.rotation : grabbableObject.transform.rotation;

                UxrController3DModel controller3DModel = grabber.Avatar.ControllerInput.GetController3DModel(grabber.Side);

                if (controller3DModel != null)
                {
                    targetRotation = controller3DModel.ForwardTrackingRotation;
                }
            }

            // This is a rare place where constraints might be applied early.
            // We do it safely when objects do not have any parent dependencies to avoid breaking any manipulation chain.
            // The reason is constrained objects preferably need to be snapped in steps:
            // -Snap rotation
            // -Constrain rotation
            // -Snap position
            // -Constrain position
            // Otherwise you might get unpleasant manipulations when, for example, twisting your hand while grabbing
            // an object that can't rotate and has constrained translation. If it's performed in a single step the
            // twisting affects the object position, while doing it in steps removes the unwanted translation.
            // It can be seen in the example scene when grabbing a constrained battery and rotating the hand. 

            if (grabbableObject.UsesGrabbableParentDependency && grabbableObject.ControlParentDirection)
            {
                UxrTransformations snapTransformations = UxrUtils.BuildTransformations(grabbableObject.GetGrabPointSnapModeAffectsPosition(grabInfo.GrabbedPoint, UxrHandSnapDirection.ObjectToHand),
                                                                                       grabbableObject.GetGrabPointSnapModeAffectsRotation(grabInfo.GrabbedPoint, UxrHandSnapDirection.ObjectToHand));

                Vector3 targetPosition = grabber.transform.position;

                if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                {
                    grabbableObject.transform.ApplyAlignment(sourcePosition, sourceRotation, targetPosition, targetRotation, snapTransformations);
                }
            }
            else
            {
                if (grabbableObject.GetGrabPointSnapModeAffectsRotation(grabInfo.GrabbedPoint, UxrHandSnapDirection.ObjectToHand))
                {
                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.ApplyAlignment(sourcePosition, sourceRotation, grabber.transform.position, targetRotation, UxrTransformations.Rotate);
                    }
                    
                    ConstrainTransform(grabbableObject, UxrTransformations.Rotate);
                }

                if (grabbableObject.GetGrabPointSnapModeAffectsPosition(grabInfo.GrabbedPoint, UxrHandSnapDirection.ObjectToHand))
                {
                    sourcePosition = TransformExt.GetWorldPosition(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignPosition);
                    sourceRotation = TransformExt.GetWorldRotation(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignRotation);

                    if (Features.HasFlag(UxrManipulationFeatures.ObjectManipulation))
                    {
                        grabbableObject.transform.ApplyAlignment(sourcePosition, sourceRotation, grabber.transform.position, targetRotation, UxrTransformations.Translate);
                    }
                }

                ConstrainTransform(grabbableObject);
            }
        }

        /// <summary>
        ///     Rotates an object towards another grabber.
        /// </summary>
        /// <param name="grabbableObject">Object to rotate</param>
        /// <param name="rotationPivot">Rotation pivot</param>
        /// <param name="grabber">Destination grabber</param>
        /// <param name="t">Interpolation parameter [0.0, 1.0], to control whether to perform the rotation partially or totally</param>
        private void RotateObjectTowardsGrab(UxrGrabbableObject grabbableObject,
                                             Vector3            rotationPivot,
                                             UxrGrabber         grabber,
                                             float              t)
        {
            Vector3 grabPosition              = GetGrabbedPointGrabAlignPosition(grabber);
            Vector3 currentVectorToSecondHand = grabPosition - rotationPivot;
            Vector3 desiredVectorToSecondHand = grabber.transform.position - rotationPivot;
            Vector3 rotationAxis              = Vector3.Cross(currentVectorToSecondHand, desiredVectorToSecondHand);

            grabbableObject.transform.RotateAround(rotationPivot, rotationAxis, Vector3.SignedAngle(currentVectorToSecondHand, desiredVectorToSecondHand, rotationAxis) * t);
        }

        /// <summary>
        ///     Moves/rotates the object and optionally applies resistance and propagates user-defined constraining events.
        /// </summary>
        /// <param name="grabbableObject">The object to move/rotate</param>
        /// <param name="position">New position or local position, depending on <paramref name="space" /></param>
        /// <param name="rotation">New rotation or local rotation, depending on <paramref name="space" /></param>
        /// <param name="space">Whether the new position/rotation is specified in local or world space</param>
        /// <param name="useResistance">
        ///     Whether to apply resistance to the new position/rotation using
        ///     <see cref="UxrGrabbableObject.TranslationResistance" /> and <see cref="UxrGrabbableObject.RotationResistance" />
        /// </param>
        /// <param name="propagateEvents">
        ///     Whether to propagate constraining events (
        ///     <see cref="UxrGrabbableObject.ConstraintsApplying" />/<see cref="UxrGrabbableObject.ConstraintsApplied" />/
        ///     <see cref="UxrGrabbableObject.ConstraintsFinished" />)
        /// </param>
        private void SetPositionAndRotationUsingConstraintsInternal(UxrGrabbableObject grabbableObject, Vector3 position, Quaternion rotation, Space space, bool useResistance, bool propagateEvents)
        {
            UxrApplyConstraintsEventArgs constrainEventArgs = new UxrApplyConstraintsEventArgs(grabbableObject);

            // Store the current position/rotation 

            grabbableObject.LocalPositionBeforeUpdate = grabbableObject.transform.localPosition;
            grabbableObject.LocalRotationBeforeUpdate = grabbableObject.transform.localRotation;

            // Pre-event

            if (propagateEvents)
            {
                grabbableObject.RaiseConstraintsApplying(constrainEventArgs);
            }

            // Move/Rotate

            if (space == Space.Self)
            {
                grabbableObject.transform.SetLocalPositionAndRotation(position, rotation);
            }
            else if (space == Space.World)
            {
                grabbableObject.transform.SetPositionAndRotation(position, rotation);
            }

            // Apply resistance

            if (useResistance && Features.HasFlag(UxrManipulationFeatures.ObjectResistance))
            {
                grabbableObject.transform.SetLocalPositionAndRotation(UxrInterpolator.SmoothDampPosition(grabbableObject.LocalPositionBeforeUpdate, grabbableObject.transform.localPosition, grabbableObject.TranslationResistance),
                                                                      UxrInterpolator.SmoothDampRotation(grabbableObject.LocalRotationBeforeUpdate, grabbableObject.transform.localRotation, grabbableObject.RotationResistance));
            }

            // Constrain

            ConstrainTransform(grabbableObject);

            // Apply user-defined constraints

            if (propagateEvents)
            {
                grabbableObject.RaiseConstraintsApplied(constrainEventArgs);
            }

            // Re-adjust grips to new position/orientation

            grabbableObject.KeepGripsInPlace();

            // Finished event

            if (propagateEvents)
            {
                grabbableObject.RaiseConstraintsFinished(constrainEventArgs);
            }
        }

        /// <summary>
        ///     Applies the constraints to a grabbable object's Transform.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object to apply the constraints to</param>
        /// <param name="transformations">Which constraints to apply</param>
        private void ConstrainTransform(UxrGrabbableObject grabbableObject, UxrTransformations transformations = UxrTransformations.All)
        {
            Vector3    localPosition = grabbableObject.transform.localPosition;
            Quaternion localRotation = grabbableObject.transform.localRotation;

            ConstrainTransform(grabbableObject, ref localPosition, ref localRotation, true, transformations);

            grabbableObject.transform.localPosition = localPosition;
            grabbableObject.transform.localRotation = localRotation;
        }

        /// <summary>
        ///     Applies the constraints to a grabbable object's Transform.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object to apply the constraints to</param>
        /// <param name="localPosition">Current object local position, will return constrained object local position</param>
        /// <param name="localRotation">Current object local rotation, will return constrained object local rotation</param>
        /// <param name="clampSingleAxisRotation">
        ///     Whether to clamp the internal angle that stores the degrees for objects
        ///     constrained to a single rotation axis. Usually this depends on if we want to constrain the actual object transform
        ///     or if we want to compute the constrained transform without applying it. In the second case we don't want to
        ///     modify the internal angle.
        /// </param>
        /// <param name="transformations">Which constraints to apply</param>
        private void ConstrainTransform(UxrGrabbableObject grabbableObject,
                                        ref Vector3        localPosition,
                                        ref Quaternion     localRotation,
                                        bool               clampSingleAxisRotation = true,
                                        UxrTransformations transformations         = UxrTransformations.All)
        {
            if (!Features.HasFlag(UxrManipulationFeatures.ObjectConstraints))
            {
                return;
            }
            
            // Rotation

            if (grabbableObject.IsLockedInPlace)
            {
                localRotation = grabbableObject.LocalRotationBeforeUpdate;
            }
            else if (transformations.HasFlag(UxrTransformations.Rotate) && grabbableObject.RotationConstraint != UxrRotationConstraintMode.Free)
            {
                Quaternion targetLocalRotation = grabbableObject.InitialLocalRotation;

                if (grabbableObject.RotationConstraint == UxrRotationConstraintMode.RestrictLocalRotation)
                {
                    float singleRotationAngleCumulative = grabbableObject.SingleRotationAngleCumulative;

                    targetLocalRotation = ClampRotation(grabbableObject,
                                                        localRotation,
                                                        grabbableObject.LocalRotationBeforeUpdate,
                                                        grabbableObject.InitialLocalRotation,
                                                        grabbableObject.RotationAngleLimitsMin,
                                                        grabbableObject.RotationAngleLimitsMax,
                                                        false,
                                                        ref singleRotationAngleCumulative);

                    if (clampSingleAxisRotation)
                    {
                        grabbableObject.SingleRotationAngleCumulative = singleRotationAngleCumulative;
                    }
                }

                localRotation = targetLocalRotation;
            }

            // Translation

            if (grabbableObject.IsLockedInPlace)
            {
                localPosition = grabbableObject.LocalPositionBeforeUpdate;
            }
            else if (transformations.HasFlag(UxrTransformations.Translate) && grabbableObject.TranslationConstraint != UxrTranslationConstraintMode.Free)
            {
                Vector3 targetLocalPos = grabbableObject.InitialLocalPosition;

                if (grabbableObject.TranslationConstraint == UxrTranslationConstraintMode.RestrictToBox && grabbableObject.RestrictToBox != null)
                {
                    Vector3 clampedWorldPos = grabbableObject.transform.GetParentWorldMatrix().MultiplyPoint(localPosition).ClampToBox(grabbableObject.RestrictToBox);
                    targetLocalPos = grabbableObject.transform.GetParentWorldMatrix().inverse.MultiplyPoint(clampedWorldPos);
                }
                else if (grabbableObject.TranslationConstraint == UxrTranslationConstraintMode.RestrictToSphere && grabbableObject.RestrictToSphere != null)
                {
                    Vector3 clampedWorldPos = grabbableObject.transform.GetParentWorldMatrix().MultiplyPoint(localPosition).ClampToSphere(grabbableObject.RestrictToSphere);
                    targetLocalPos = grabbableObject.transform.GetParentWorldMatrix().inverse.MultiplyPoint(clampedWorldPos);
                }
                else if (grabbableObject.TranslationConstraint == UxrTranslationConstraintMode.RestrictLocalOffset)
                {
                    if (grabbableObject.transform.parent != null || grabbableObject.InitialParent == null)
                    {
                        // Current local space -> Initial local space using the matrix at Awake() 
                        Vector3 localPosOffset = grabbableObject.InitialRelativeMatrix.inverse.MultiplyPoint3x4(localPosition);

                        // Clamp in initial local space, transform to current local space
                        Vector3 reciprocalScale = new Vector3(1.0f / grabbableObject.transform.localScale.x, 1.0f / grabbableObject.transform.localScale.y, 1.0f / grabbableObject.transform.localScale.z);
                        targetLocalPos = grabbableObject.InitialRelativeMatrix.MultiplyPoint(localPosOffset.Clamp(Vector3.Scale(grabbableObject.TranslationLimitsMin, reciprocalScale),
                                                                                                                  Vector3.Scale(grabbableObject.TranslationLimitsMax, reciprocalScale)));
                    }
                    else
                    {
                        targetLocalPos = localPosition;
                    }
                }

                localPosition = targetLocalPos;
            }
        }

        /// <summary>
        ///     Tries to clamp a rotation for a <see cref="UxrGrabbableObject" />.
        /// </summary>
        /// <param name="grabbableObject">GrabbableObject whose rotation to clamp</param>
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
        private Quaternion ClampRotation(UxrGrabbableObject grabbableObject, Quaternion rot, Quaternion rotBeforeUpdate, Quaternion initialRot, Vector3 eulerMin, Vector3 eulerMax, bool invertRotation, ref float singleRotationAngle)
        {
            if (!Features.HasFlag(UxrManipulationFeatures.ObjectConstraints))
            {
                return rot;
            }
            
            int rangeOfMotionAxisCount = grabbableObject.RangeOfMotionRotationAxisCount;

            if (grabbableObject.RangeOfMotionRotationAxisCount == 0)
            {
                return initialRot;
            }

            if (rangeOfMotionAxisCount > 1)
            {
                bool invertPitchYaw    = false;
                int  ignorePitchYaw    = -1;
                bool clampLongitudinal = false;

                // Use classic yaw/pitch clamping when more than one axis has constrained range of motion.

                UxrAxis axis1            = grabbableObject.RangeOfMotionRotationAxes.First();
                UxrAxis axis2            = grabbableObject.RangeOfMotionRotationAxes.Last();
                UxrAxis longitudinalAxis = UxrAxis.OtherThan(axis1, axis2);

                if (rangeOfMotionAxisCount == 3)
                {
                    // Pitch/yaw clamping will be on the other-than-longitudinal axes, when all 3 axes have constrained range of motion.

                    axis1             = grabbableObject.RangeOfMotionRotationAxes.First(a => a != grabbableObject.RotationLongitudinalAxis);
                    axis2             = grabbableObject.RangeOfMotionRotationAxes.Last(a => a != grabbableObject.RotationLongitudinalAxis);
                    longitudinalAxis  = grabbableObject.RotationLongitudinalAxis;
                    clampLongitudinal = true;
                }
                else
                {
                    // If there are only two rotation axes constrained, check if one of the constrained axes is actually the longitudinal axis.
                    // In this case, we will zero either the pitch or yaw and perform longitudinal clamping later.
                    if (!longitudinalAxis.Equals(grabbableObject.RotationLongitudinalAxis))
                    {
                        // Ignore the incorrectly computed longitudinal axis, which in reality is either the pitch or yaw
                        ignorePitchYaw = longitudinalAxis;

                        // Assign the longitudinal axis correctly based on what the user selected
                        longitudinalAxis = grabbableObject.RotationLongitudinalAxis;

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

            int     singleAxisIndex            = grabbableObject.SingleRotationAxisIndex;
            Vector3 rotationAxis               = (UxrAxis)singleAxisIndex;
            Vector3 perpendicularAxis          = ((UxrAxis)singleAxisIndex).Perpendicular;
            Vector3 initialPerpendicularVector = initialRot * perpendicularAxis;
            Vector3 currentPerpendicularVector = Vector3.ProjectOnPlane(rot * perpendicularAxis, initialRot * rotationAxis);
            float   angle                      = Vector3.SignedAngle(initialPerpendicularVector, currentPerpendicularVector, initialRot * rotationAxis) * (invertRotation ? -1.0f : 1.0f);
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

            singleRotationAngle.Clamp(grabbableObject.RotationAngleLimitsMin[singleAxisIndex], grabbableObject.RotationAngleLimitsMax[singleAxisIndex]);
            return initialRot * Quaternion.AngleAxis(singleRotationAngle, rotationAxis);
        }

        /// <summary>
        ///     Gets the current rotation angle, in objects constrained to a single rotation axis, contributed by all the grabbers
        ///     manipulating the object.
        /// </summary>
        /// <param name="grabbableObject">The object to get the information from</param>
        private float GetCurrentSingleRotationAngleContributions(UxrGrabbableObject grabbableObject)
        {
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return manipulationInfo.CurrentSingleRotationAngleContributions;
            }

            return 0.0f;
        }

        /// <summary>
        ///     If an object is constrained, sometimes the hand that grabs it can be at a different distance than the real hand.
        ///     For example, if an object whose position is constrained is grabbed and the hand is pulled far away, there is a
        ///     point where the real hand might be far enough to the hand that is grabbing the grip is automatically released.
        ///     This method does the check. Only the local avatar is processed to keep consistency, the releases will be
        ///     be requested from each client.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <returns>Whether the grip should be released</returns>
        private bool ShouldGrabberReleaseFarAwayGrip(UxrGrabber grabber)
        {
            if (grabber.Avatar == UxrAvatar.LocalAvatar && grabber != null && grabber.GrabbedObject != null)
            {
                // Check if the user separated the hands too much to drop it from the hand that went too far
                if (Vector3.Distance(grabber.UnprocessedGrabberPosition, grabber.transform.position) > grabber.GrabbedObject.LockedGrabReleaseDistance)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Updates the smooth grabbable object position/rotation transitions due to manipulation.
        /// </summary>
        private void UpdateSmoothObjectTransitions()
        {
            foreach (UxrGrabbableObject grabbableObject in UxrGrabbableObject.EnabledComponents)
            {
                grabbableObject.UpdateSmoothManipulationTransition(Time.unscaledDeltaTime);
                grabbableObject.UpdateSmoothAnchorPlacement(Time.unscaledDeltaTime);
                grabbableObject.UpdateSmoothConstrainTransition(Time.unscaledDeltaTime);
            }
        }

        #endregion
    }
}