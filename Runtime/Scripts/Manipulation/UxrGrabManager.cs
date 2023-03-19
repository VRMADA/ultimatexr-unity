// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Haptics;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Manager that takes care of updating all the manipulation mechanics. The manipulation system handles three main
    ///     types of entities:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="UxrGrabber" />: Components usually assigned to each hand of an <see cref="UxrAvatar" /> and
    ///             that are able to grab objects
    ///         </item>
    ///         <item><see cref="UxrGrabbableObject" />: Objects that can be grabbed</item>
    ///         <item><see cref="UxrGrabbableObjectAnchor" />: Anchors where grabbable objects can be placed</item>
    ///     </list>
    /// </summary>
    public partial class UxrGrabManager : UxrSingleton<UxrGrabManager>, IUxrStateSync, IUxrLogger
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabber" /> component is about to try to grab something (a hand is beginning
        ///     to close). If it ends up grabbing something will depend on whether there is a <see cref="UxrGrabbableObject" /> in
        ///     reach.
        ///     Properties available:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that tried to grab.
        ///         </item>
        ///     </list>
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> GrabTrying;

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabber" /> component is about to grab a <see cref="UxrGrabbableObject" />.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be grabbed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object is currently placed. Null
        ///             if it isn't placed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that is about to grab the object.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Grab point index of the object that is about to be
        ///             grabbed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.IsMultiHands" />: true if it is already being grabbed with one hand and
        ///             it will be grabbed with both hands after. False if no hand is currently grabbing it.
        ///         </item>
        ///     </list>
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectGrabbing;

        /// <summary>
        ///     Same as <see cref="ObjectGrabbing" /> but called right after the object was grabbed.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectGrabbed;

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabber" /> component is about to release the
        ///     <see cref="UxrGrabbableObject" /> that it is holding and there is no <see cref="UxrGrabbableObjectAnchor" /> nearby
        ///     to place it on.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be released.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object was originally grabbed
        ///             from. Null if it wasn't grabbed from an anchor.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that is about to release the object.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Grab point index of the object that is being
        ///             grabbed by the <see cref="UxrGrabber" />.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.IsMultiHands" />: true if it is already being grabbed with another hand
        ///             that will keep it holding. False if no other hand is currently grabbing it.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.IsSwitchHands" />: True if it was released because another
        ///             <see cref="UxrGrabber" /> grabbed it, false otherwise. if
        ///             <see cref="UxrManipulationEventArgs.IsMultiHands" /> is
        ///             true then <see cref="UxrManipulationEventArgs.IsSwitchHands" /> will tell if it was released by both hands
        ///             (false) or if it was just released by one hand and the other one still keeps it grabbed (true).
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.ReleaseVelocity" />: Velocity the object is being released with.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.ReleaseAngularVelocity" />: Angular velocity the object is being
        ///             released with.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     If the object is being released on a <see cref="UxrGrabbableObjectAnchor" /> that can hold it, it will
        ///     generate a <see cref="ObjectPlacing" /> event instead. Whenever an object is released it will either generate
        ///     either a Place or Release event, but not both.
        /// </remarks>
        public event EventHandler<UxrManipulationEventArgs> ObjectReleasing;

        /// <summary>
        ///     Same as <see cref="ObjectReleasing" /> but called right after the object was released.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectReleased;

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabbableObject" /> is about to be placed on an
        ///     <see cref="UxrGrabbableObjectAnchor" />.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be placed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object is about to be placed on.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that is placing the object.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Grab point index of the object that is being
        ///             grabbed by the <see cref="UxrGrabber" />.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     If the object is being placed it will not generate a <see cref="ObjectReleasing" /> event. Whenever an object is
        ///     released it will either generate either a Place or Release event, but not both.
        /// </remarks>
        public event EventHandler<UxrManipulationEventArgs> ObjectPlacing;

        /// <summary>
        ///     Same as <see cref="ObjectPlacing" /> but called right after the object was placed.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectPlaced;

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabbableObject" /> is about to be removed from an
        ///     <see cref="UxrGrabbableObjectAnchor" />.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be removed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object is currently placed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that is about to remove the object by grabbing it.
        ///             This can be null if the object is removed through code using <see cref="RemoveObjectFromAnchor" />,
        ///             <see cref="UxrGrabbableObject.RemoveFromAnchor" /> or <see cref="UxrGrabbableObjectAnchor.RemoveObject" />>
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Only if the object is being removed by grabbing it:
        ///             Grab point index of the object that is about to be grabbed by the <see cref="UxrGrabber" />.
        ///         </item>
        ///     </list>
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectRemoving;

        /// <summary>
        ///     Same as <see cref="ObjectRemoving" /> but called right after the object was removed.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> ObjectRemoved;

        /// <summary>
        ///     Event called whenever an <see cref="UxrGrabbableObject" /> being grabbed by a <see cref="UxrGrabber" /> entered the
        ///     valid placement range (distance) of a compatible <see cref="UxrGrabbableObjectAnchor" />.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that entered the valid placement range.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object can potentially be placed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that is holding the object. If more than one
        ///             grabber is holding it, it will indicate the first one to grab it.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     Only enter/leave events will be generated. To check if an object can be placed on an anchor use
        ///     <see cref="UxrGrabbableObject.CanBePlacedOnAnchor" />.
        /// </remarks>
        /// <seealso cref="AnchorRangeLeft" />
        public event EventHandler<UxrManipulationEventArgs> AnchorRangeEntered;

        /// <summary>
        ///     Same as <see cref="AnchorRangeEntered" /> but when leaving the valid range.
        /// </summary>
        /// <remarks>
        ///     Only enter/leave events will be generated. To check if an object can be placed on an anchor use
        ///     <see cref="UxrGrabbableObject.CanBePlacedOnAnchor" />.
        /// </remarks>
        /// <seealso cref="AnchorRangeEntered" />
        public event EventHandler<UxrManipulationEventArgs> AnchorRangeLeft;

        /// <summary>
        ///     Event called whenever a <see cref="UxrGrabber" /> enters the valid grab range (distance) of a
        ///     <see cref="UxrGrabbableObject" /> placed on an <see cref="UxrGrabbableObjectAnchor" />.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is within reach.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object is placed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.Grabber" />: Grabber that entered the valid grab range.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Grab point index that is within reach.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     Only enter/leave events will be generated. To check if an object can be grabbed use
        ///     <see cref="UxrGrabbableObject.CanBeGrabbedByGrabber" />.
        /// </remarks>
        /// <seealso cref="PlacedObjectRangeLeft" />
        public event EventHandler<UxrManipulationEventArgs> PlacedObjectRangeEntered;

        /// <summary>
        ///     Same as <see cref="PlacedObjectRangeEntered" /> but when leaving the valid range.
        /// </summary>
        /// <remarks>
        ///     Only enter/leave events will be generated. To check if an object can be grabbed use
        ///     <see cref="UxrGrabbableObject.CanBeGrabbedByGrabber" />.
        /// </remarks>
        /// <seealso cref="PlacedObjectRangeEntered" />
        public event EventHandler<UxrManipulationEventArgs> PlacedObjectRangeLeft;

        /// <summary>
        ///     Gets the currently grabbed objects.
        /// </summary>
        public IEnumerable<UxrGrabbableObject> CurrentGrabbedObjects => _currentGrabs.Keys;

        /// <summary>
        ///     Gets or sets whether grabbing is allowed.
        /// </summary>
        public bool IsGrabbingAllowed { get; set; } = true;

        #endregion

        #region Implicit IUxrLogger

        /// <inheritdoc />
        public UxrLogLevel LogLevel { get; set; } = UxrLogLevel.Relevant;

        #endregion

        #region Implicit IUxrStateSync

        /// <inheritdoc />
        public event EventHandler<UxrStateSyncEventArgs> StateChanged;

        /// <inheritdoc />
        public void SyncState(UxrStateSyncEventArgs e, bool propagateEvents)
        {
            if (!(e is UxrManipulationSyncEventArgs syncArgs))
            {
                return;
            }

            switch (syncArgs.EventType)
            {
                case UxrManipulationSyncEventType.Grab:

                    GrabObject(syncArgs.EventArgs.Grabber, syncArgs.EventArgs.GrabbableObject, syncArgs.EventArgs.GrabPointIndex, propagateEvents);
                    break;

                case UxrManipulationSyncEventType.Release:

                    ReleaseObject(syncArgs.EventArgs.Grabber, syncArgs.EventArgs.GrabbableObject, propagateEvents);
                    break;

                case UxrManipulationSyncEventType.Place:

                    PlaceObject(syncArgs.EventArgs.GrabbableObject, syncArgs.EventArgs.GrabbableAnchor, syncArgs.EventArgs.PlacementOptions, propagateEvents);
                    break;

                case UxrManipulationSyncEventType.Remove:

                    RemoveObjectFromAnchor(syncArgs.EventArgs.GrabbableObject, propagateEvents);
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether an <see cref="UxrAvatar" /> can grab something using the given hand.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="handSide">Whether to check the left or right hand</param>
        /// <returns>Whether something can be grabbed</returns>
        public bool CanGrabSomething(UxrAvatar avatar, UxrHandSide handSide)
        {
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide)
                {
                    return CanGrabSomething(grabber);
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks whether a <see cref="UxrGrabber" /> can grab something using the given grabber.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <returns>Whether something can be grabbed</returns>
        public bool CanGrabSomething(UxrGrabber grabber)
        {
            foreach (UxrGrabbableObject grabbableObject in UxrGrabbableObject.EnabledComponents)
            {
                if (grabbableObject.IsGrabbable)
                {
                    for (int point = 0; point < grabbableObject.GrabPointCount; ++point)
                    {
                        if (grabbableObject.CanBeGrabbedByGrabber(grabber, point))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the closest grabbable object that can be grabbed by an <see cref="UxrAvatar" /> using the given hand.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="handSide">Whether to check the left hand or right hand</param>
        /// <param name="grabbableObject">Returns the closest grabbable object or null if none was found</param>
        /// <param name="grabPoint">Returns the grab point that can be grabbed</param>
        /// <param name="candidates">List of grabbable objects to process or null to process all current enabled grabbable objects</param>
        /// <returns>Whether a grabbable object was found</returns>
        public bool GetClosestGrabbableObject(UxrAvatar avatar, UxrHandSide handSide, out UxrGrabbableObject grabbableObject, out int grabPoint, IEnumerable<UxrGrabbableObject> candidates = null)
        {
            grabbableObject = null;
            grabPoint       = 0;

            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide)
                {
                    return GetClosestGrabbableObject(grabber, out grabbableObject, out grabPoint, candidates);
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the closest grabbable object that can be grabbed by a <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <param name="grabbableObject">Returns the closest grabbable object or null if none was found</param>
        /// <param name="grabPoint">Returns the grab point that can be grabbed</param>
        /// <param name="candidates">List of grabbable objects to process or null to process all current enabled grabbable objects</param>
        /// <returns>Whether a grabbable object was found</returns>
        public bool GetClosestGrabbableObject(UxrGrabber grabber, out UxrGrabbableObject grabbableObject, out int grabPoint, IEnumerable<UxrGrabbableObject> candidates = null)
        {
            int   maxPriority                = int.MinValue;
            float minDistanceWithoutRotation = float.MaxValue; // Between different objects we don't take orientations into account

            grabbableObject = null;
            grabPoint       = 0;

            // Iterate over objects

            foreach (UxrGrabbableObject candidate in candidates ?? UxrGrabbableObject.EnabledComponents)
            {
                float minDistance = float.MaxValue; // For the same object we will not just consider the distance but also how close the grabber is to the grip orientation

                // Iterate over grab points
                for (int point = 0; point < candidate.GrabPointCount; ++point)
                {
                    if (candidate.CanBeGrabbedByGrabber(grabber, point))
                    {
                        candidate.GetDistanceFromGrabber(grabber, point, out float distance, out float distanceWithoutRotation);

                        if (candidate.Priority > maxPriority)
                        {
                            grabbableObject            = candidate;
                            grabPoint                  = point;
                            minDistance                = distance;
                            minDistanceWithoutRotation = distanceWithoutRotation;
                            maxPriority                = candidate.Priority;
                        }
                        else
                        {
                            if ((grabbableObject == candidate && distance < minDistance) || (grabbableObject != candidate && distanceWithoutRotation < minDistanceWithoutRotation))
                            {
                                grabbableObject            = candidate;
                                grabPoint                  = point;
                                minDistance                = distance;
                                minDistanceWithoutRotation = distanceWithoutRotation;
                            }
                        }
                    }
                }
            }

            return grabbableObject != null;
        }

        /// <summary>
        ///     Tries to grab something.
        /// </summary>
        /// <param name="avatar">Avatar that tried to grab something</param>
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
        ///     Tries to release something.
        /// </summary>
        /// <param name="avatar">Avatar that tried to release something</param>
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

            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                List<UxrGrabber> grabbersToRelease = new List<UxrGrabber>(grabInfo.Grabbers);
                grabbersToRelease.ForEach(g => ReleaseObject(g, grabbableObject, propagateEvents));
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

            if (anchor != null && anchor.CurrentPlacedObject != null)
            {
                return false;
            }

            UxrGrabber               grabber      = null;
            int                      grabbedPoint = -1;
            UxrGrabbableObjectAnchor oldAnchor    = grabbableObject.CurrentAnchor;
            bool                     releaseGrip  = !placementOptions.HasFlag(UxrPlacementOptions.DontRelease);

            if (releaseGrip)
            {
                // Release the grips if there are any.

                if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
                {
                    // TODO: Ideally we would send events from all grabbers later, not just the first
                    grabber      = grabInfo.Grabbers[0];
                    grabbedPoint = grabInfo.GrabbedPoints[0];

                    // Don't propagate Release events, because Place and Release are mutually exclusive
                    List<UxrGrabber> grabbersToRelease = new List<UxrGrabber>(grabInfo.Grabbers);
                    grabbersToRelease.ForEach(g => ReleaseObject(g, grabbableObject, false));
                }
            }

            if (grabbableObject.IsConstrained)
            {
                grabbableObject.StartSmoothConstrain();
            }

            if (anchor == null)
            {
                return true;
            }

            // Remove and raise events

            if (oldAnchor != null)
            {
                UxrManipulationEventArgs manipulationEventArgs = new UxrManipulationEventArgs(grabbableObject, oldAnchor, grabber, grabbedPoint);

                OnObjectRemoving(manipulationEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    oldAnchor.RaiseRemovingEvent(manipulationEventArgs);
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

                OnObjectRemoved(manipulationEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    oldAnchor.RaiseRemovedEvent(manipulationEventArgs);
                }
            }

            UxrManipulationEventArgs placingEventArgs = new UxrManipulationEventArgs(grabbableObject, anchor, grabber, grabbedPoint);
            OnObjectPlacing(placingEventArgs, propagateEvents);
            if (propagateEvents)
            {
                grabbableObject.RaisePlacingEvent(placingEventArgs);
                anchor.RaisePlacingEvent(placingEventArgs);
            }

            // Setup

            if (grabbableObject.RigidBodySource != null)
            {
                grabbableObject.RigidBodySource.isKinematic = true;
            }

            if (grabbableObject.UseParenting)
            {
                grabbableObject.transform.SetParent(anchor.AlignTransform, true);
            }

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

            if (releaseGrip && grabber != null && grabber.GrabbedObject != null && _currentGrabs.ContainsKey(grabber.GrabbedObject))
            {
                _currentGrabs.Remove(grabbableObject);
            }

            // Start smooth transition to final position/orientation if necessary
            if (placementOptions == UxrPlacementOptions.Smooth)
            {
                grabbableObject.StartSmoothAnchorPlacement();
            }
            else
            {
                grabbableObject.PlaceOnAnchor();
            }

            // Raise events

            UxrManipulationEventArgs placedEventArgs = new UxrManipulationEventArgs(grabbableObject, anchor, grabber, grabbedPoint);
            placedEventArgs.PlacementOptions = placementOptions;
            OnObjectPlaced(placedEventArgs, propagateEvents);

            if (placedEventArgs.Grabber)
            {
                grabbableObject.NotifyEndGrab(placedEventArgs.Grabber, placedEventArgs.GrabPointIndex);
            }

            if (propagateEvents)
            {
                grabbableObject.RaisePlacedEvent(placedEventArgs);
                anchor.RaisePlacedEvent(placedEventArgs);
            }

            return true;
        }

        /// <summary>
        ///     Gets whether grabbing a given <see cref="UxrGrabbableObject" /> using a certain <see cref="UxrGrabber" /> will make
        ///     the grabber's renderer show up as hidden due to the parameters set in the inspector.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <param name="grabbableObject">Grabbable object to check</param>
        /// <returns>Whether the renderer would be hidden when grabbed</returns>
        public bool ShouldHideHandRenderer(UxrGrabber grabber, UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                for (int point = 0; point < grabInfo.Grabbers.Count; ++point)
                {
                    if (grabInfo.Grabbers[point] == grabber)
                    {
                        return grabbableObject.GetGrabPoint(grabInfo.GrabbedPoints[point]).HideHandGrabberRenderer;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the grab pose name required when grabbing the given <see cref="UxrGrabbableObject" /> using the
        ///     <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Grab pose name or null to use the default grab pose specified in the avatar belonging to the grabber</returns>
        public string GetOverrideGrabPoseName(UxrGrabber grabber, UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                for (int point = 0; point < grabInfo.Grabbers.Count; ++point)
                {
                    if (grabInfo.Grabbers[point] == grabber)
                    {
                        UxrHandPoseAsset handPoseAsset = grabbableObject.GetGrabPoint(grabInfo.GrabbedPoints[point]).GetGripPoseInfo(grabber.Avatar).HandPose;
                        return handPoseAsset != null ? handPoseAsset.name : null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the blend value for the <see cref="UxrHandPoseType.Blend" /> pose used when grabbing the given
        ///     <see cref="UxrGrabbableObject" /> using the <see cref="UxrGrabber" />.
        ///     Blending is used to transition between different states such as open/closed or similar.
        /// </summary>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Blending value [0.0, 1.0]</returns>
        public float GetOverrideGrabPoseBlendValue(UxrGrabber grabber, UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                for (int point = 0; point < grabInfo.Grabbers.Count; ++point)
                {
                    if (grabInfo.Grabbers[point] == grabber)
                    {
                        return grabbableObject.GetGrabPoint(grabInfo.GrabbedPoints[point]).GetGripPoseInfo(grabber.Avatar).PoseBlendValue;
                    }
                }
            }

            return 0.0f;
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
            UxrGrabbableObjectAnchor anchorFrom = grabbableObject.CurrentAnchor;

            // Were we swapping hands, are we grabbing with both hands or is it a new grab?

            bool                     handSwapSamePoint            = false;
            bool                     handSwapDifferentPoints      = false;
            bool                     bothHands                    = false;
            UxrGrabber               releasingGrabber             = null;
            RuntimeGrabInfo          grabInfo                     = null;
            UxrManipulationEventArgs manipulationReleaseEventArgs = null;

            foreach (UxrGrabber otherGrabberCandidate in UxrGrabber.EnabledComponents)
            {
                if (otherGrabberCandidate != grabber && otherGrabberCandidate.GrabbedObject == grabbableObject)
                {
                    // Other grabber is already grabbing this object. Check if it is the same grabbing point or not.
                    _currentGrabs.TryGetValue(grabbableObject, out grabInfo);

                    if (grabInfo != null && grabInfo.GrabbedPoints.Contains(grabPoint))
                    {
                        // It is the same grabbing point. Now there are two options:
                        // -If the grabbing point has an UxrGrabPointShape component associated, it will be grabbed with both hands at the same time if MultiGrab is enabled.
                        // -If it does not have an UxrGrabPointShape it will mean that the object will swap from one hand to the other.
                        if (grabbableObject.GetGrabPointShape(grabPoint) != null && grabbableObject.AllowMultiGrab)
                        {
                            bothHands = true;
                            break;
                        }

                        // We are swapping hands because there is already other hand grabbing this same point
                        // Raise release event for the other hand
                        releasingGrabber             = otherGrabberCandidate;
                        handSwapSamePoint            = true;
                        manipulationReleaseEventArgs = new UxrManipulationEventArgs(releasingGrabber.GrabbedObject, anchorFrom, releasingGrabber, grabPoint, false, true);
                        break;
                    }

                    if (grabInfo != null && !grabInfo.GrabbedPoints.Contains(grabPoint))
                    {
                        // Other hand grabbing another point of the same object: both hands will grab the object if MultiGrab is enabled, or the
                        // other hand will be released if not
                        releasingGrabber = otherGrabberCandidate;

                        if (!grabbableObject.AllowMultiGrab)
                        {
                            handSwapDifferentPoints      = true;
                            manipulationReleaseEventArgs = new UxrManipulationEventArgs(releasingGrabber.GrabbedObject, anchorFrom, releasingGrabber, grabPoint, false, true);
                        }
                        else
                        {
                            bothHands = true;
                        }

                        break;
                    }
                }
            }

            // Raise events

            if (manipulationReleaseEventArgs != null)
            {
                OnObjectReleasing(manipulationReleaseEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    releasingGrabber.GrabbedObject.RaiseReleasingEvent(manipulationReleaseEventArgs);
                }
            }

            UxrManipulationEventArgs manipulationEventArgs = new UxrManipulationEventArgs(grabbableObject, anchorFrom, grabber, grabPoint, bothHands, handSwapSamePoint || handSwapDifferentPoints);

            OnObjectGrabbing(manipulationEventArgs, propagateEvents);

            if (propagateEvents)
            {
                grabbableObject.RaiseGrabbingEvent(manipulationEventArgs);
            }

            if (anchorFrom && !grabbableObject.IsConstrained)
            {
                OnObjectRemoving(manipulationEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    anchorFrom.RaiseRemovingEvent(manipulationEventArgs);
                }
            }

            // Link it to the hand

            grabber.GrabbedObject = grabbableObject;

            if (anchorFrom && !grabbableObject.IsConstrained)
            {
                anchorFrom.CurrentPlacedObject      = null;
                grabber.GrabbedObject.CurrentAnchor = null;

                if (anchorFrom.ActivateOnPlaced)
                {
                    anchorFrom.ActivateOnPlaced.SetActive(false);
                }

                if (anchorFrom.ActivateOnEmpty)
                {
                    anchorFrom.ActivateOnEmpty.SetActive(true);
                }

                if (anchorFrom.ActivateOnCompatibleNear != null)
                {
                    anchorFrom.ActivateOnCompatibleNear.SetActive(false);
                }

                if (anchorFrom.ActivateOnCompatibleNotNear != null)
                {
                    anchorFrom.ActivateOnCompatibleNotNear.SetActive(false);
                }
            }

            // If it is a dynamic object, make it kinematic while it is grabbed

            if (grabber.GrabbedObject.RigidBodySource != null)
            {
                grabber.GrabbedObject.RigidBodySource.isKinematic = true;
            }

            if (!grabber.GrabbedObject.IsConstrained)
            {
                if (grabber.GrabbedObject.UseParenting)
                {
                    grabber.GrabbedObject.transform.SetParent(null, true);
                }
            }

            if (handSwapSamePoint)
            {
                grabInfo.SwapGrabber(grabInfo.Grabbers[grabInfo.GrabbedPoints.IndexOf(grabPoint)], grabber);
            }
            else if (handSwapDifferentPoints)
            {
                grabInfo.SwapGrabber(releasingGrabber, grabInfo.Grabbers.IndexOf(releasingGrabber), grabber, grabPoint);
            }
            else if (bothHands)
            {
                if (grabPoint != 0 || grabbableObject.GetGrabPointShape(0) != null)
                {
                    // We started a new grab with a hand that is not the main grabbing point. Since only the main grabbing point triggers a smooth transition
                    // due to the smooth hand lock we need to force a smooth lookAt to the new grab
                    grabInfo.LookAtTimer = UxrGrabbableObject.HandLockSeconds;
                }

                grabInfo.AddGrabber(grabber, grabPoint);
            }
            else
            {
                if (grabber.GrabbedObject.UsesGrabbableParentDependency && grabber.GrabbedObject.ControlParentDirection && IsBeingGrabbed(grabber.GrabbedObject.GrabbableParentDependency.GetComponent<UxrGrabbableObject>()))
                {
                    // We started a new grab with a hand whose parent is already being grabbed but this object controls the parents direction. Since only the main grabbing point triggers a smooth transition
                    // due to the smooth hand lock we need to force a smooth lookAt to the new grab.

                    if (_currentGrabs.TryGetValue(grabber.GrabbedObject.GrabbableParentDependency.GetComponent<UxrGrabbableObject>(), out RuntimeGrabInfo mainGrabInfo))
                    {
                        mainGrabInfo.LookAtTimer = UxrGrabbableObject.HandLockSeconds;
                    }
                }

                _currentGrabs.Add(grabbableObject, new RuntimeGrabInfo(grabber, grabPoint, anchorFrom));
            }

            grabber.GrabbedObject.NotifyBeginGrab(grabber, grabPoint);
            StopSmoothHandTransition(grabber);

            // Raise events

            if (manipulationReleaseEventArgs != null)
            {
                OnObjectReleased(manipulationReleaseEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    releasingGrabber.GrabbedObject.RaiseReleasedEvent(manipulationReleaseEventArgs);
                }

                releasingGrabber.GrabbedObject.NotifyEndGrab(manipulationReleaseEventArgs.Grabber, manipulationReleaseEventArgs.GrabPointIndex);
                releasingGrabber.GrabbedObject = null;
            }

            OnObjectGrabbed(manipulationEventArgs, propagateEvents);

            if (propagateEvents)
            {
                grabbableObject.RaiseGrabbedEvent(manipulationEventArgs);
            }

            if (anchorFrom)
            {
                OnObjectRemoved(manipulationEventArgs, propagateEvents);

                if (propagateEvents)
                {
                    anchorFrom.RaiseRemovedEvent(manipulationEventArgs);
                }
            }
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
            bool found        = false;
            int  grabbedPoint = GetGrabbedPoint(grabber);

            foreach (UxrGrabber grb in UxrGrabber.EnabledComponents.Where(grb => grb.GrabbedObject == grabbableObject && (grb == grabber || grabber == null)))
            {
                grb.GrabbedObject = null;
                found             = true;
            }

            if (found == false)
            {
                return;
            }

            if (!_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                if (LogLevel >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.ManipulationModule}: GrabInfo not found for object {grabbableObject.name}. This should not be happening.");
                }

                return;
            }

            UxrGrabbableObjectAnchor anchorFrom             = grabInfo.AnchorFrom;
            bool                     isMultiHands           = grabInfo.GrabbedPoints.Count > 1;
            Vector3                  releaseVelocity        = grabber.SmoothVelocity;
            Vector3                  releaseAngularVelocity = grabber.SmoothAngularVelocity * Mathf.Deg2Rad;
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

            // Raise event(s)

            foreach (UxrGrabber grb in grabInfo.Grabbers)
            {
                if (grb == grabber || grabber == null)
                {
                    UxrManipulationEventArgs releasingEventArgs = new UxrManipulationEventArgs(grabbableObject, anchorFrom, grb, grabbedPoint, isMultiHands, isMultiHands && grabber != null);

                    releasingEventArgs.ReleaseVelocity        = releaseVelocity;
                    releasingEventArgs.ReleaseAngularVelocity = releaseAngularVelocity;

                    OnObjectReleasing(releasingEventArgs, propagateEvents);

                    if (grabbableObject && propagateEvents)
                    {
                        grabbableObject.RaiseReleasingEvent(releasingEventArgs);
                    }
                }
            }

            // Check if it is still grabbed with other hand

            if (grabber != null && isMultiHands)
            {
                // Grabbing with both hands so the hand that released may be not in its actual position where the sensor is.
                // Start a smooth transition to the actual position.
                StartSmoothHandTransition(grabber, grabbableObject, grabbedPoint);

                if (grabbableObject.RotationProvider != UxrRotationProvider.HandPositionAroundPivot)
                {
                    // We released one hand but we keep grabbing with the other. Unless the grabber that keeps grabbing has a snap to
                    // pivot and align rotation point currently grabbed, we change the grabbing parameters so that it keeps
                    // the current orientation to avoid the sudden "pop" that otherwise would happen.
                    int remainingPoint = -1;
                    int remainingIndex = -1;

                    for (int i = 0; i < grabInfo.Grabbers.Count; ++i)
                    {
                        if (grabInfo.Grabbers[i] != grabber)
                        {
                            remainingIndex = i;
                            remainingPoint = grabInfo.GrabbedPoints[remainingIndex];
                            break;
                        }
                    }

                    UxrGrabber otherGrabber = remainingIndex != -1 ? grabInfo.Grabbers[remainingIndex] : null;

                    if (remainingIndex != -1 && grabbableObject.GetGrabPoint(remainingPoint).SnapMode == UxrSnapToHandMode.DontSnap)
                    {
                        grabbableObject.ComputeGrabTransforms(otherGrabber, remainingPoint);
                    }
                    else if (otherGrabber != null)
                    {
                        StartSmoothHandTransition(otherGrabber, grabbableObject, remainingPoint);
                    }
                }
            }
            else
            {
                // Grabber is controlling the direction of a parent. Force the parent look-at and then start a smooth transition to the single grab orientation.
                UxrGrabbableObject grabbableParentBeingGrabbed = GetParentBeingGrabbed(grabbableObject.transform);

                if (grabbableParentBeingGrabbed != null && grabbableObject.ControlParentDirection && grabbableObject.UsesGrabbableParentDependency)
                {
                    if (_currentGrabs.TryGetValue(grabbableParentBeingGrabbed, out RuntimeGrabInfo mainGrabInfo))
                    {
                        StartSmoothHandTransition(mainGrabInfo.Grabbers[0], grabbableParentBeingGrabbed, mainGrabInfo.GrabbedPoints[0]);
                    }
                }
            }

            if (isMultiHands == false || grabber == null)
            {
                _currentGrabs.Remove(grabbableObject);

                if (grabbableObject.RigidBodySource != null)
                {
                    grabbableObject.RigidBodySource.isKinematic = !grabbableObject.RigidBodyDynamicOnRelease;

                    if (releaseVelocity.IsValid())
                    {
                        grabbableObject.RigidBodySource.AddForce(releaseVelocity, ForceMode.VelocityChange);
                        grabbableObject.RigidBodySource.position += releaseVelocity * Time.deltaTime;
                    }

                    if (releaseAngularVelocity.IsValid())
                    {
                        grabbableObject.RigidBodySource.AddTorque(releaseAngularVelocity, ForceMode.VelocityChange);
                    }
                }

                if (grabbableObject.IsConstrained)
                {
                    if (grabber != null && grabbedPoint != -1)
                    {
                        StartSmoothHandTransition(grabber, grabbableObject, grabbedPoint);
                    }
                }
            }

            // Raise event(s)

            foreach (UxrGrabber grb in grabInfo.Grabbers)
            {
                if (grb == grabber || grabber == null)
                {
                    UxrManipulationEventArgs releasedEventArgs = new UxrManipulationEventArgs(grabbableObject, anchorFrom, grb, grabbedPoint, isMultiHands, isMultiHands && grabber != null);

                    releasedEventArgs.ReleaseVelocity        = releaseVelocity;
                    releasedEventArgs.ReleaseAngularVelocity = releaseAngularVelocity;

                    OnObjectReleased(releasedEventArgs, propagateEvents);

                    if (grabbableObject && propagateEvents)
                    {
                        grabbableObject.RaiseReleasedEvent(releasedEventArgs);
                        grabbableObject.NotifyEndGrab(releasedEventArgs.Grabber, releasedEventArgs.GrabPointIndex);
                    }
                }
            }

            // Remove grabber(s)
            if (grabber == null)
            {
                grabInfo.RemoveAll();
            }
            else
            {
                grabInfo.RemoveGrabber(grabber);
            }
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

            // Raise events

            UxrGrabbableObjectAnchor anchorFrom            = grabbableObject.CurrentAnchor;
            UxrManipulationEventArgs manipulationEventArgs = new UxrManipulationEventArgs(grabbableObject, anchorFrom, null, -1);

            OnObjectRemoving(manipulationEventArgs, propagateEvents);

            if (propagateEvents)
            {
                anchorFrom.RaiseRemovingEvent(manipulationEventArgs);
            }

            // Perform removal

            if (grabbableObject.RigidBodySource != null && !IsBeingGrabbed(grabbableObject))
            {
                grabbableObject.RigidBodySource.isKinematic = !grabbableObject.RigidBodyDynamicOnRelease;
            }

            if (grabbableObject.UseParenting)
            {
                grabbableObject.transform.SetParent(null, true);
            }

            if (grabbableObject.IsConstrained)
            {
                grabbableObject.StartSmoothConstrainExit();
            }

            if (anchorFrom != null)
            {
                anchorFrom.CurrentPlacedObject = null;
            }

            grabbableObject.CurrentAnchor = null;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == grabbableObject)
                {
                    grabbableObject.CheckAndApplyConstraints(grabber, GetGrabbedPoint(grabber), grabbableObject.transform.localPosition, grabbableObject.transform.localRotation, propagateEvents);
                    grabbableObject.CheckAndApplyLockHand(grabber, GetGrabbedPoint(grabber));
                }
            }

            // Raise events

            OnObjectRemoved(manipulationEventArgs, propagateEvents);

            if (propagateEvents)
            {
                anchorFrom.RaiseRemovedEvent(manipulationEventArgs);
            }
        }

        /// <summary>
        ///     Checks whether the given <see cref="UxrAvatar" /> hand is currently grabbing something.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="handSide">Whether to check the left hand or right hand</param>
        /// <returns>Whether it is currently grabbing something</returns>
        public bool IsHandGrabbing(UxrAvatar avatar, UxrHandSide handSide)
        {
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide && grabber.GrabbedObject != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the object being grabbed by an avatar.
        /// </summary>
        /// <param name="avatar">Avatar to get the grabbed object of</param>
        /// <param name="handSide">Whether to check the left hand or right hand</param>
        /// <param name="grabbableObject">Returns the object being grabbed, or null if not found</param>
        /// <returns>Whether there is an object being grabbed by the avatar using the given hand</returns>
        public bool GetObjectBeingGrabbed(UxrAvatar avatar, UxrHandSide handSide, out UxrGrabbableObject grabbableObject)
        {
            grabbableObject = null;

            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide && grabber.GrabbedObject != null)
                {
                    grabbableObject = grabber.GrabbedObject;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if an avatar's hand is grabbing a grabbable object.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="grabbableObject">Object to check if it is being grabbed</param>
        /// <param name="handSide">Whether to check the left hand or right hand</param>
        /// <param name="alsoCheckDependentGrab">
        ///     Whether to also check for any parent <see cref="UxrGrabbableObject" /> that
        ///     controls its direction and is physically connected.
        /// </param>
        /// <returns>Whether the object is being grabbed by the avatar using the given hand</returns>
        public bool IsHandGrabbing(UxrAvatar avatar, UxrGrabbableObject grabbableObject, UxrHandSide handSide, bool alsoCheckDependentGrab = true)
        {
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide && grabber.GrabbedObject != null)
                {
                    // Grabbing directly with the hand
                    if (grabber.GrabbedObject == grabbableObject)
                    {
                        return true;
                    }

                    if (alsoCheckDependentGrab)
                    {
                        // Grabbing a dependent grabbable?
                        UxrGrabbableObject grabbableParentBeingGrabbed = GetParentBeingGrabbed(grabber.GrabbedObject.transform);

                        if (grabbableParentBeingGrabbed == grabbableObject && grabber.GrabbedObject.ControlParentDirection && grabber.GrabbedObject.UsesGrabbableParentDependency)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the number of hands currently grabbing an object.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="alsoCheckDependentGrabs">
        ///     Also checks for hands that are grabbing other child objects that also control the direction of the given parent
        ///     object.
        /// </param>
        /// <returns>Number of hands grabbing the object</returns>
        public int GetHandsGrabbingCount(UxrGrabbableObject grabbableObject, bool alsoCheckDependentGrabs = true)
        {
            int result = 0;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject != null)
                {
                    // Grabbing directly with the hand
                    if (grabber.GrabbedObject == grabbableObject)
                    {
                        result++;
                    }
                    else if (alsoCheckDependentGrabs)
                    {
                        // Grabbing a dependent grabbable?
                        UxrGrabbableObject grabbableParentBeingGrabbed = GetParentBeingGrabbed(grabber.GrabbedObject.transform);

                        if (grabbableParentBeingGrabbed == grabbableObject && grabber.GrabbedObject.ControlParentDirection && grabber.GrabbedObject.UsesGrabbableParentDependency)
                        {
                            result++;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <returns>Whether it is being grabbed</returns>
        public bool IsBeingGrabbed(UxrGrabbableObject grabbableObject)
        {
            return _currentGrabs != null && _currentGrabs.ContainsKey(grabbableObject);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed using the given grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Grab point of the grabbable object to check</param>
        /// <returns>Whether the grab point is being grabbed using the given grab point</returns>
        public bool IsBeingGrabbed(UxrGrabbableObject grabbableObject, int point)
        {
            if (_currentGrabs == null)
            {
                return false;
            }

            return _currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo) && grabInfo.GrabbedPoints.Contains(point);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed by an avatar.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="avatar">The avatar to check</param>
        /// <returns>Whether it is being grabbed by the avatar</returns>
        public bool IsBeingGrabbedBy(UxrGrabbableObject grabbableObject, UxrAvatar avatar)
        {
            if (_currentGrabs == null)
            {
                return false;
            }

            return _currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo) && grabInfo.Grabbers.Any(grabber => grabber.Avatar == avatar);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed by a specific grabber.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="grabber">The grabber to check</param>
        /// <returns>Whether it is being grabbed by the given grabber</returns>
        public bool IsBeingGrabbedBy(UxrGrabbableObject grabbableObject, UxrGrabber grabber)
        {
            if (_currentGrabs == null)
            {
                return false;
            }

            return _currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo) && grabInfo.Grabbers.Any(grb => grabber == grb);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed using any other grab point than the specified.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Grab point of the grabbable object not to check</param>
        /// <returns>Whether any other grab point is being grabbed</returns>
        public bool IsBeingGrabbedByOtherThan(UxrGrabbableObject grabbableObject, int point)
        {
            if (_currentGrabs == null)
            {
                return false;
            }

            return _currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo) && grabInfo.GrabbedPoints.Any(grabPoint => point != grabPoint);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed using any other grab point and any other grabber than
        ///     the specified.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Point should be any other than this</param>
        /// <param name="grabber">Grabber should be any other than this</param>
        /// <returns>Whether the object is being grabbed with the specified conditions</returns>
        public bool IsBeingGrabbedByOtherThan(UxrGrabbableObject grabbableObject, int point, UxrGrabber grabber)
        {
            if (_currentGrabs == null)
            {
                return false;
            }

            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                for (int i = 0; i < grabInfo.GrabbedPoints.Count; ++i)
                {
                    if (grabInfo.GrabbedPoints[i] != point || grabInfo.Grabbers[i] != grabber)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the hands that are grabbing an object.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="isLeft">Whether it is being grabbed using the left hand</param>
        /// <param name="isRight">Whether it is being grabbed using the right hand</param>
        /// <returns>Whether it is being grabbed</returns>
        public bool GetGrabbingHand(UxrGrabbableObject grabbableObject, out bool isLeft, out bool isRight)
        {
            isLeft  = false;
            isRight = false;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == grabbableObject)
                {
                    if (grabber.Side == UxrHandSide.Left)
                    {
                        isLeft = true;
                    }
                    else
                    {
                        isRight = true;
                    }
                }
            }

            return isLeft || isRight;
        }

        /// <summary>
        ///     Gets the grabber that is grabbing an object using a specific grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Grab point to check</param>
        /// <param name="grabber">Grabber to check</param>
        /// <returns>Whether it is being grabbed with the specified conditions</returns>
        public bool GetGrabbingHand(UxrGrabbableObject grabbableObject, int point, out UxrGrabber grabber)
        {
            grabber = null;

            foreach (UxrGrabber grabberCandidate in UxrGrabber.EnabledComponents)
            {
                if (grabberCandidate.GrabbedObject == grabbableObject && _currentGrabs.TryGetValue(grabberCandidate.GrabbedObject, out RuntimeGrabInfo grabInfo))
                {
                    for (int grabEntry = 0; grabEntry < grabInfo.Grabbers.Count; ++grabEntry)
                    {
                        if (grabInfo.GrabbedPoints[grabEntry] == point)
                        {
                            grabber = grabInfo.Grabbers[grabEntry];
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the grabbers that are grabbing the object using a specific grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">The grab point or -1 to get all grabbed points</param>
        /// <param name="grabbers">
        ///     Returns the list of grabbers. If the list is null a new list is created, otherwise the grabbers
        ///     are added to the list.
        /// </param>
        /// <returns>Whether one or more grabbers were found</returns>
        public bool GetGrabbingHands(UxrGrabbableObject grabbableObject, int point, out List<UxrGrabber> grabbers)
        {
            grabbers = null;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == grabbableObject && _currentGrabs.TryGetValue(grabber.GrabbedObject, out RuntimeGrabInfo grabInfo))
                {
                    for (int grabEntry = 0; grabEntry < grabInfo.Grabbers.Count; ++grabEntry)
                    {
                        if (grabInfo.GrabbedPoints[grabEntry] == point || point == -1)
                        {
                            grabbers ??= new List<UxrGrabber>();
                            grabbers.Add(grabInfo.Grabbers[grabEntry]);
                        }
                    }
                }
            }

            return grabbers != null && grabbers.Count > 0;
        }

        /// <summary>
        ///     Gets the hand grabbing the given object using a given grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">The grab point used to grab the object</param>
        /// <param name="handSide">Returns the hand that is used to grab the object</param>
        /// <returns>Whether there is a hand grabbing the object</returns>
        public bool GetGrabbingHand(UxrGrabbableObject grabbableObject, int point, out UxrHandSide handSide)
        {
            handSide = UxrHandSide.Left;

            if (GetGrabbingHand(grabbableObject, point, out UxrGrabber grabber))
            {
                handSide = grabber.Side;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the grab point that the <see cref="UxrGrabber" /> is currently grabbing on a <see cref="UxrGrabbableObject" />
        ///     .
        /// </summary>
        /// <param name="grabber">Grabber to get the grabbed point from</param>
        /// <returns>Grab point index that is being grabbed or -1 if there is no object currently being grabbed</returns>
        public int GetGrabbedPoint(UxrGrabber grabber)
        {
            if (grabber && grabber.GrabbedObject != null && _currentGrabs.TryGetValue(grabber.GrabbedObject, out RuntimeGrabInfo grabInfo))
            {
                for (int i = 0; i < grabInfo.Grabbers.Count; ++i)
                {
                    if (grabInfo.Grabbers[i] == grabber)
                    {
                        return grabInfo.GrabbedPoints[i];
                    }
                }
            }

            return -1;
        }

        /// <summary>
        ///     Gets the number of grab points that are currently being grabbed from a <see cref="UxrGrabbableObject" />.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Number of grab points being grabbed</returns>
        public int GetGrabbedPointCount(UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.Grabbers.Count;
            }

            return 0;
        }

        /// <summary>
        ///     Gets the number of hands that are grabbing the given object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Number of hands grabbing the object</returns>
        public int GetGrabbingHandCount(UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.Grabbers.Count + grabInfo.ChildDependentGrabCount;
            }

            return 0;
        }

        /// <summary>
        ///     Gets the <see cref="UxrGrabbableObjectAnchor" /> where the given <see cref="UxrGrabbableObject" /> was grabbed
        ///     from.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Anchor the grabbable object was grabbed from</returns>
        public UxrGrabbableObjectAnchor GetGrabbedObjectAnchorFrom(UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.AnchorFrom;
            }

            return null;
        }

        /// <summary>
        ///     Gets the current world-space velocity, in units per second, of an object that is being grabbed.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="smooth">Whether to smooth the velocity using a previous frame data window for improved behavior</param>
        /// <returns>Velocity in world-space units per second</returns>
        public Vector3 GetGrabbedObjectVelocity(UxrGrabbableObject grabbableObject, bool smooth = true)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                return smooth ? grabInfo.Grabbers[0].SmoothVelocity : grabInfo.Grabbers[0].Velocity;
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Gets the current world-space angular velocity, in degrees per second, of an object that is being grabbed.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="smooth">Whether to smooth the velocity using a previous frame data window for improved behavior</param>
        /// <returns>Angular velocity in world-space euler angle degrees per second</returns>
        public Vector3 GetGrabbedObjectAngularVelocity(UxrGrabbableObject grabbableObject, bool smooth = true)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                return smooth ? grabInfo.Grabbers[0].SmoothAngularVelocity : grabInfo.Grabbers[0].AngularVelocity;
            }

            return Vector3.zero;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Updates the grab manager to the current frame.
        /// </summary>
        internal void UpdateManager()
        {
            // Store the unprocessed grabber positions for this update.

            foreach (UxrGrabber grabber in UxrGrabber.AllComponents)
            {
                grabber.UnprocessedGrabberPosition = grabber.transform.position;
                grabber.UnprocessedGrabberRotation = grabber.transform.rotation;
            }

            // Update look-at timers and store pre-update positions/orientations

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeGrabInfo> grabInfoPair in _currentGrabs)
            {
                if (grabInfoPair.Value.LookAtTimer > 0.0f)
                {
                    grabInfoPair.Value.LookAtTimer -= Time.deltaTime;
                }

                grabInfoPair.Value.LocalPositionBeforeUpdate = grabInfoPair.Key.transform.localPosition;
                grabInfoPair.Value.LocalRotationBeforeUpdate = grabInfoPair.Key.transform.localRotation;
            }

            // Initialize some variables for later

            foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
            {
                if (anchorPair.Key.CurrentPlacedObject == null)
                {
                    anchorPair.Value.HadCompatibleObjectNearLastFrame = anchorPair.Value.HasCompatibleObjectNear;
                    anchorPair.Value.HasCompatibleObjectNear          = false;
                }
                else
                {
                    anchorPair.Value.GrabberNear = null;
                }

                anchorPair.Value.FullGrabberNear = null;
                anchorPair.Value.GrabPointNear   = -1;
            }

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeGrabInfo> grabInfoPair in _currentGrabs)
            {
                grabInfoPair.Value.ChildDependentGrabCount     = 0;
                grabInfoPair.Value.ChildDependentGrabProcessed = 0;
            }

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeGrabInfo> grabInfoPair in _currentGrabs)
            {
                grabInfoPair.Value.GrabbableParentBeingGrabbed = GetParentBeingGrabbed(grabInfoPair.Key.transform);

                if (grabInfoPair.Value.GrabbableParentBeingGrabbed != null)
                {
                    if (_currentGrabs.TryGetValue(grabInfoPair.Value.GrabbableParentBeingGrabbed, out RuntimeGrabInfo grabInfoParent))
                    {
                        grabInfoParent.ChildDependentGrabCount++;
                    }
                }
            }

            // First force smooth hand transition orientations if there are any.

            UpdateSmoothHandTransitions(Time.unscaledDeltaTime);

            // Iterate over grabbed objects. Process objects that are being grabbed while other parent object is being grabbed too in a second pass.

            // First pass (objects without parents being grabbed)
            List<UxrGrabber> listToRelease = new List<UxrGrabber>();

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeGrabInfo> grabInfoPair in _currentGrabs)
            {
                if (grabInfoPair.Value.GrabbableParentBeingGrabbed == null)
                {
                    ProcessGrab(grabInfoPair.Key, grabInfoPair.Value, ref listToRelease);
                }
            }

            foreach (UxrGrabber grabber in listToRelease)
            {
                if (grabber.GrabbedObject != null)
                {
                    ReleaseObject(grabber, grabber.GrabbedObject, true);
                }
            }

            // Second pass (child objects with a parent being grabbed)
            listToRelease.Clear();

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeGrabInfo> grabInfoPair in _currentGrabs)
            {
                if (grabInfoPair.Value.GrabbableParentBeingGrabbed != null)
                {
                    ProcessGrab(grabInfoPair.Key, grabInfoPair.Value, ref listToRelease);

                    if (_currentGrabs.TryGetValue(grabInfoPair.Value.GrabbableParentBeingGrabbed, out RuntimeGrabInfo grabInfoParent))
                    {
                        grabInfoParent.ChildDependentGrabProcessed++;
                    }
                }
            }

            foreach (UxrGrabber grabber in listToRelease)
            {
                if (grabber.GrabbedObject != null)
                {
                    ReleaseObject(grabber, grabber.GrabbedObject, true);
                }
            }

            listToRelease.Clear();

            // Look for objects that can be grabbed to update feedback objects (blinks, labels...). First pass: get closest candidate for each grabber.
            Dictionary<UxrGrabbableObject, List<int>> possibleGrabs = null;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == null)
                {
                    if (GetClosestGrabbableObject(grabber, out UxrGrabbableObject grabbableCandidate, out int grabPointCandidate) &&
                        !IsBeingGrabbed(grabbableCandidate, grabPointCandidate))
                    {
                        if (possibleGrabs == null)
                        {
                            possibleGrabs = new Dictionary<UxrGrabbableObject, List<int>>();
                        }

                        if (possibleGrabs.ContainsKey(grabbableCandidate))
                        {
                            possibleGrabs[grabbableCandidate].Add(grabPointCandidate);
                        }
                        else
                        {
                            possibleGrabs.Add(grabbableCandidate, new List<int> { grabPointCandidate });
                        }
                    }
                }
            }

            // Second pass: update visual feedback objects for grabbable objects.
            foreach (UxrGrabbableObject grabbable in UxrGrabbableObject.EnabledComponents)
            {
                // First disable all needed, then enable them in another pass because some points may share the same object
                for (int point = 0; point < grabbable.GrabPointCount; ++point)
                {
                    GameObject enableOnHandNear = grabbable.GetGrabPoint(point).EnableOnHandNear;

                    if (enableOnHandNear)
                    {
                        bool      enableObject = false;
                        List<int> grabPoints   = null;

                        if (possibleGrabs != null && possibleGrabs.TryGetValue(grabbable, out grabPoints))
                        {
                            enableObject = grabPoints.Contains(point);
                        }

                        if (!enableObject && enableOnHandNear.activeSelf)
                        {
                            // Try to find first if other point needs to enable it
                            bool foundEnable = false;

                            for (int pointOther = 0; pointOther < grabbable.GrabPointCount; ++pointOther)
                            {
                                GameObject enableOnHandNearOther = grabbable.GetGrabPoint(pointOther).EnableOnHandNear;

                                if (enableOnHandNear == enableOnHandNearOther)
                                {
                                    if (possibleGrabs != null && possibleGrabs.TryGetValue(grabbable, out List<int> grabPointsOther))
                                    {
                                        foundEnable = grabPoints.Contains(pointOther);

                                        if (foundEnable)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!foundEnable)
                            {
                                enableOnHandNear.SetActive(false);
                                break;
                            }
                        }
                    }
                }

                for (int point = 0; point < grabbable.GrabPointCount; ++point)
                {
                    GameObject enableOnHandNear = grabbable.GetGrabPoint(point).EnableOnHandNear;

                    if (enableOnHandNear)
                    {
                        bool enableObject = false;

                        if (possibleGrabs != null && possibleGrabs.TryGetValue(grabbable, out List<int> grabPoints))
                        {
                            enableObject = grabPoints.Contains(point);
                        }

                        if (enableObject && !enableOnHandNear.activeSelf)
                        {
                            enableOnHandNear.SetActive(true);
                            break;
                        }
                    }
                }
            }

            // Look for empty hand being able to grab something from an anchor to update anchor visual feedback objects later and also raise events. First pass: gather info.
            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == null)
                {
                    UxrGrabbableObjectAnchor anchorCandidate            = null;
                    int                      grabPointCandidate         = 0;
                    int                      maxPriority                = int.MinValue;
                    float                    minDistanceWithoutRotation = float.MaxValue; // Between different objects we don't take orientations into account

                    foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
                    {
                        UxrGrabbableObjectAnchor grabbableAnchor = anchorPair.Key;

                        if (grabbableAnchor.CurrentPlacedObject != null)
                        {
                            // For the same object we will not just consider the distance but also how close the grabber is to the grip orientation
                            float minDistance = float.MaxValue;

                            for (int point = 0; point < grabbableAnchor.CurrentPlacedObject.GrabPointCount; ++point)
                            {
                                if (grabbableAnchor.CurrentPlacedObject.CanBeGrabbedByGrabber(grabber, point))
                                {
                                    grabbableAnchor.CurrentPlacedObject.GetDistanceFromGrabber(grabber, point, out float distance, out float distanceWithoutRotation);

                                    if (grabbableAnchor.CurrentPlacedObject.Priority > maxPriority)
                                    {
                                        anchorCandidate            = grabbableAnchor;
                                        grabPointCandidate         = point;
                                        minDistance                = distance;
                                        minDistanceWithoutRotation = distanceWithoutRotation;
                                        maxPriority                = grabbableAnchor.CurrentPlacedObject.Priority;
                                    }
                                    else
                                    {
                                        if ((anchorCandidate == grabbableAnchor && distance < minDistance) || (anchorCandidate != grabbableAnchor && distanceWithoutRotation < minDistanceWithoutRotation))
                                        {
                                            anchorCandidate            = grabbableAnchor;
                                            grabPointCandidate         = point;
                                            minDistance                = distance;
                                            minDistanceWithoutRotation = distanceWithoutRotation;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (anchorCandidate != null)
                    {
                        _grabbableObjectAnchors[anchorCandidate].GrabberNear   = null;
                        _grabbableObjectAnchors[anchorCandidate].GrabPointNear = grabPointCandidate;
                    }
                }
            }

            // Second pass: update object states and raise events.
            foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
            {
                if (anchorPair.Key.CurrentPlacedObject == null)
                {
                    if (anchorPair.Value.LastValidGrabberNear != null)
                    {
                        PlacedObjectRangeEntered?.Invoke(this, new UxrManipulationEventArgs(anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.LastValidGrabberNear, anchorPair.Value.LastValidGrabPointNear));

                        anchorPair.Value.LastValidGrabberNear   = null;
                        anchorPair.Value.LastValidGrabPointNear = -1;
                    }

                    if (anchorPair.Value.HasCompatibleObjectNear && !anchorPair.Value.HadCompatibleObjectNearLastFrame)
                    {
                        OnAnchorRangeEntered(new UxrManipulationEventArgs(anchorPair.Value.FullGrabberNear.GrabbedObject, anchorPair.Key, anchorPair.Value.FullGrabberNear), true);
                    }

                    if (!anchorPair.Value.HasCompatibleObjectNear && anchorPair.Value.HadCompatibleObjectNearLastFrame)
                    {
                        OnAnchorRangeLeft(new UxrManipulationEventArgs(anchorPair.Value.LastFullGrabberNear.GrabbedObject, anchorPair.Key, anchorPair.Value.LastFullGrabberNear), true);
                    }

                    if (anchorPair.Key.ActivateOnCompatibleNear)
                    {
                        anchorPair.Key.ActivateOnCompatibleNear.SetActive(anchorPair.Value.HasCompatibleObjectNear);
                    }

                    if (anchorPair.Key.ActivateOnCompatibleNotNear)
                    {
                        anchorPair.Key.ActivateOnCompatibleNotNear.SetActive(!anchorPair.Value.HasCompatibleObjectNear);
                    }

                    if (anchorPair.Key.ActivateOnHandNearAndGrabbable)
                    {
                        anchorPair.Key.ActivateOnHandNearAndGrabbable.SetActive(false);
                    }
                }
                else
                {
                    if (anchorPair.Value.GrabberNear != anchorPair.Value.LastValidGrabberNear)
                    {
                        if (anchorPair.Value.GrabberNear != null)
                        {
                            OnPlacedObjectRangeEntered(new UxrManipulationEventArgs(anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.GrabberNear, anchorPair.Value.GrabPointNear), true);
                        }
                        else if (anchorPair.Value.LastValidGrabberNear != null)
                        {
                            OnPlacedObjectRangeLeft(new UxrManipulationEventArgs(anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.LastValidGrabberNear, anchorPair.Value.GrabPointNear), true);
                        }

                        anchorPair.Value.LastValidGrabberNear   = anchorPair.Value.GrabberNear;
                        anchorPair.Value.LastValidGrabPointNear = anchorPair.Value.GrabPointNear;
                    }

                    if (anchorPair.Key.ActivateOnHandNearAndGrabbable)
                    {
                        anchorPair.Key.ActivateOnHandNearAndGrabbable.SetActive(anchorPair.Value.GrabberNear != null);
                    }

                    if (anchorPair.Key.ActivateOnPlaced)
                    {
                        anchorPair.Key.ActivateOnPlaced.SetActive(true);
                    }

                    if (anchorPair.Key.ActivateOnEmpty)
                    {
                        anchorPair.Key.ActivateOnEmpty.SetActive(false);
                    }
                }
            }

            // Update grabbers
            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                grabber.UpdateThrowPhysicsInfo();
                grabber.UpdateHandGrabberRenderer();
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the manager and subscribes to global events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _grabbableObjectAnchors = new Dictionary<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo>();
            _currentGrabs           = new Dictionary<UxrGrabbableObject, RuntimeGrabInfo>();
            _handTransitions        = new Dictionary<UxrGrabber, HandTransitionInfo>();

            UxrGrabbableObjectAnchor.GlobalEnabled  += GrabbableObjectAnchor_Enabled;
            UxrGrabbableObjectAnchor.GlobalDisabled += GrabbableObjectAnchor_Disabled;
            UxrGrabbableObject.GlobalDisabled       += GrabbableObject_Disabled;
        }

        /// <summary>
        ///     Unsubscribes from global events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            UxrGrabbableObjectAnchor.GlobalEnabled  -= GrabbableObjectAnchor_Enabled;
            UxrGrabbableObjectAnchor.GlobalDisabled -= GrabbableObjectAnchor_Disabled;
            UxrGrabbableObject.GlobalDisabled       += GrabbableObject_Disabled;
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarMoved += UxrManager_AvatarMoved;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarMoved -= UxrManager_AvatarMoved;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when a grabbable object anchor was enabled. Adds it to the internal list.
        /// </summary>
        /// <param name="anchor">Anchor that was enabled</param>
        private void GrabbableObjectAnchor_Enabled(UxrGrabbableObjectAnchor anchor)
        {
            _grabbableObjectAnchors.Add(anchor, new GrabbableObjectAnchorInfo());
        }

        /// <summary>
        ///     Called when a grabbable object anchor was disabled. Removes it from the internal list.
        /// </summary>
        /// <param name="anchor">Anchor that was disabled</param>
        private void GrabbableObjectAnchor_Disabled(UxrGrabbableObjectAnchor anchor)
        {
            _grabbableObjectAnchors.Remove(anchor);
        }

        /// <summary>
        ///     Called when a grabbable object was disabled. Removes it from current grabs if present.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object that was disabled</param>
        private void GrabbableObject_Disabled(UxrGrabbableObject grabbableObject)
        {
            if (_currentGrabs.ContainsKey(grabbableObject))
            {
                _currentGrabs.Remove(grabbableObject);
            }
        }

        /// <summary>
        ///     Called when an avatar was moved due to regular movement or teleportation. It is used to process the objects that
        ///     are being grabbed to the avatar to keep it in the same relative position/orientation.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrManager_AvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            // Create anonymous pairs of grabbable objects and their grabbers that are affected by the avatar position change
            var dependencies = _currentGrabs.Where(pair => pair.Value.Grabbers.Any(g => g.Avatar == e.Avatar)).Select(pair => new { GrabbableObject = pair.Key, Grabbers = pair.Value.Grabbers.Where(g => g.Avatar == e.Avatar) });

            foreach (var dependency in dependencies)
            {
                UxrGrabbableObject grabbableObject = dependency.GrabbableObject;

                // Move grabbed objects without being parented to avatar to new position/orientation to avoid rubber-band effects
                if (!grabbableObject.transform.HasParent(e.Avatar.transform))
                {
                    Vector3    oldLocalPosition = grabbableObject.transform.localPosition;
                    Quaternion oldLocalRotation = grabbableObject.transform.localRotation;

                    // Use this handy method to make the grabbable object keep the relative positioning to the avatar 
                    e.ReorientRelativeToAvatar(grabbableObject.transform);

                    float translationResistance = grabbableObject.TranslationResistance;
                    float rotationResistance    = grabbableObject.RotationResistance;
                    grabbableObject.TranslationResistance = 0.0f;
                    grabbableObject.RotationResistance    = 0.0f;

                    // Apply constraints making sure the resistance doesn't get in the way of resistance interpolations
                    foreach (UxrGrabber grabber in dependency.Grabbers)
                    {
                        // Also make sure align to controller doesn't get in the way
                        int              grabbedPoint      = GetGrabbedPoint(grabber);
                        UxrGrabPointInfo grabPointInfo     = grabbableObject.GetGrabPoint(grabbedPoint);
                        bool             alignToController = grabPointInfo.AlignToController;
                        grabPointInfo.AlignToController = false;

                        grabbableObject.CheckAndApplyConstraints(grabber, GetGrabbedPoint(grabber), oldLocalPosition, oldLocalRotation, true);
                        grabbableObject.CheckAndApplyLockHand(grabber, GetGrabbedPoint(grabber));

                        grabPointInfo.AlignToController = alignToController;
                    }

                    grabbableObject.TranslationResistance = translationResistance;
                    grabbableObject.RotationResistance    = rotationResistance;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="GrabTrying" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnGrabTrying(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                GrabTrying?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectGrabbing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectGrabbing(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (LogLevel >= UxrLogLevel.Relevant)
            {
                string handInfo = e.Grabber != null ? $" using {e.Grabber.Side.ToString().ToLower()} hand" : string.Empty;
                Debug.Log($"{UxrConstants.ManipulationModule}: Grabbing {e.GrabbableObject.name}{handInfo}");
            }

            if (propagateEvent)
            {
                ObjectGrabbing?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectGrabbed" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectGrabbed(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                ObjectGrabbed?.Invoke(this, e);
                StateChanged?.Invoke(this, new UxrManipulationSyncEventArgs(UxrManipulationSyncEventType.Grab, e));
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectReleasing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectReleasing(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (LogLevel >= UxrLogLevel.Relevant)
            {
                string handInfo = e.Grabber != null ? $" using {e.Grabber.Side.ToString().ToLower()} hand" : string.Empty;
                Debug.Log($"{UxrConstants.ManipulationModule}: Releasing {e.GrabbableObject.name}{handInfo}");
            }

            if (propagateEvent)
            {
                ObjectReleasing?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectReleased" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectReleased(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                ObjectReleased?.Invoke(this, e);
                StateChanged?.Invoke(this, new UxrManipulationSyncEventArgs(UxrManipulationSyncEventType.Release, e));
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectPlacing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectPlacing(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (LogLevel >= UxrLogLevel.Relevant)
            {
                string handInfo = e.Grabber != null ? $" using {e.Grabber.Side.ToString().ToLower()} hand" : string.Empty;
                Debug.Log($"{UxrConstants.ManipulationModule}: Placing {e.GrabbableObject.name} on {e.GrabbableAnchor.name}{handInfo}");
            }

            if (propagateEvent)
            {
                ObjectPlacing?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectPlaced" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectPlaced(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                ObjectPlaced?.Invoke(this, e);
                StateChanged?.Invoke(this, new UxrManipulationSyncEventArgs(UxrManipulationSyncEventType.Place, e));
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectRemoving" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectRemoving(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (LogLevel >= UxrLogLevel.Relevant)
            {
                string handInfo = e.Grabber != null ? $" using {e.Grabber.Side.ToString().ToLower()} hand" : string.Empty;
                Debug.Log($"{UxrConstants.ManipulationModule}: Removing {e.GrabbableObject.name} from {e.GrabbableAnchor.name}{handInfo}");
            }

            if (propagateEvent)
            {
                ObjectRemoving?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectRemoved" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectRemoved(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                ObjectRemoved?.Invoke(this, e);
                StateChanged?.Invoke(this, new UxrManipulationSyncEventArgs(UxrManipulationSyncEventType.Remove, e));
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="AnchorRangeEntered" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnAnchorRangeEntered(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                AnchorRangeEntered?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="AnchorRangeLeft" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnAnchorRangeLeft(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                AnchorRangeLeft?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="PlacedObjectRangeEntered" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnPlacedObjectRangeEntered(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                PlacedObjectRangeEntered?.Invoke(this, e);
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="PlacedObjectRangeLeft" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnPlacedObjectRangeLeft(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (propagateEvent)
            {
                PlacedObjectRangeLeft?.Invoke(this, e);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Moves the hand so that the grabber moves back to the last unprocessed grabber position/orientation.
        /// </summary>
        /// <param name="grabber">Grabber to revert</param>
        private void RevertToUnprocessedGrabberTransform(UxrGrabber grabber)
        {
            if (grabber && grabber.HandBone)
            {
                Matrix4x4 grabberMatrix = Matrix4x4.TRS(grabber.UnprocessedGrabberPosition, grabber.UnprocessedGrabberRotation, Vector3.one);
                grabber.HandBone.SetPositionAndRotation(grabberMatrix.MultiplyPoint(grabber.HandBoneRelativePos), grabberMatrix.rotation * grabber.HandBoneRelativeRot);
            }
        }

        /// <summary>
        ///     Gets the parent of a given <see cref="UxrGrabbableObject" /> that is being grabbed.
        /// </summary>
        /// <param name="grabbableTransform"><see cref="Transform" /> of a grabbable object</param>
        /// <returns>Parent <see cref="UxrGrabbableObject" /> that is being grabbed or null if there isn't any</returns>
        private UxrGrabbableObject GetParentBeingGrabbed(Transform grabbableTransform)
        {
            if (grabbableTransform.parent != null)
            {
                UxrGrabbableObject parentGrabbableObject = grabbableTransform.parent.GetComponent<UxrGrabbableObject>();

                if (parentGrabbableObject != null && IsBeingGrabbed(parentGrabbableObject))
                {
                    return parentGrabbableObject;
                }

                return GetParentBeingGrabbed(grabbableTransform.parent);
            }

            return null;
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

                if (_currentGrabs.TryGetValue(grabber.GrabbedObject, out RuntimeGrabInfo grabInfo))
                {
                    for (int i = 0; i < grabInfo.Grabbers.Count; ++i)
                    {
                        if (grabInfo.Grabbers[i] == grabber)
                        {
                            if (grabber.GrabbedObject.GetGrabPoint(grabInfo.GrabbedPoints[i]).GrabMode == UxrGrabMode.GrabAndKeepAlways)
                            {
                                // Grab is in "Keep always" mode
                                return;
                            }
                        }
                    }
                }

                // Toggle grab mode
                NotifyReleaseGrab(grabber, true);
                return;
            }

            OnGrabTrying(new UxrManipulationEventArgs(null, null, grabber), true);

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
        ///     Releases an object if the given grabber is grabbing any.
        /// </summary>
        /// <param name="grabber">Grabber to release the object from</param>
        /// <param name="fromToggle">Whether the release was from a <see cref="UxrGrabMode.GrabToggle"/></param>
        private void NotifyReleaseGrab(UxrGrabber grabber, bool fromToggle = false)
        {
            // A release gesture has been made. Check for possible object placements / drop
            if (grabber.GrabbedObject != null)
            {
                // First check if the grabbed point has toggle mode or keep always mode. In that case we should not release the object but keep it in the grabbed list

                RuntimeGrabInfo grabInfo = null;
                
                if (_currentGrabs.TryGetValue(grabber.GrabbedObject, out grabInfo) && !fromToggle)
                {
                    for (int i = 0; i < grabInfo.Grabbers.Count; ++i)
                    {
                        if ((grabInfo.Grabbers[i] == grabber && grabber.GrabbedObject.GetGrabPoint(grabInfo.GrabbedPoints[i]).GrabMode == UxrGrabMode.GrabToggle) ||
                            grabber.GrabbedObject.GetGrabPoint(grabInfo.GrabbedPoints[i]).GrabMode == UxrGrabMode.GrabAndKeepAlways)
                        {
                            // Ignore release. We will keep grabbing it until another TryGrab or keep it grabbed always unless another grabber gets it.
                            return;
                        }
                    }
                }

                // If we only have a single hand left grabbing, find the closest compatible anchor candidate within the influence radius
                UxrGrabbableObjectAnchor anchorCandidate = null;

                if (grabInfo != null && grabInfo.GrabbedPoints.Count == 1 && grabber.GrabbedObject.IsPlaceable)
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
        ///     Handles an object being grabbed.
        /// </summary>
        /// <param name="grabbableObject">The object being grabbed</param>
        /// <param name="grabInfo">Grab information structure</param>
        /// <param name="listToRelease">
        ///     If somewhere in the process we detect that a hand needs to forcefully release an object, it will be added to this
        ///     list. The main reason will be a hand grabbing a constrained object and pulling too far away from the actual
        ///     grabbing point.
        /// </param>
        private void ProcessGrab(UxrGrabbableObject grabbableObject, RuntimeGrabInfo grabInfo, ref List<UxrGrabber> listToRelease)
        {
            if (grabbableObject.RotationProvider != UxrRotationProvider.HandPositionAroundPivot && grabInfo.GrabbedPoints.Count > 1)
            {
                // Position multi-grab object
                HandleMultiGrab(grabbableObject, ref listToRelease, grabInfo.LookAtT);
            }
            else
            {
                for (int point = 0; point < grabInfo.GrabbedPoints.Count; ++point)
                {
                    UxrGrabber grabber                       = grabInfo.Grabbers[point];
                    int        pointIndex                    = grabInfo.GrabbedPoints[point];
                    bool       triggerApplyConstraintsEvents = grabInfo.ChildDependentGrabCount == 0;

                    // Check if there is a parent being grabbed and this object tells it to look at it. This object controls the direction of the parent if grabbed.
                    if (grabInfo.GrabbableParentBeingGrabbed != null)
                    {
                        bool performedLookAt = false;

                        if (grabbableObject.ControlParentDirection && grabbableObject.UsesGrabbableParentDependency)
                        {
                            if (_currentGrabs.TryGetValue(grabInfo.GrabbableParentBeingGrabbed, out RuntimeGrabInfo mainGrabInfo))
                            {
                                // Revert grabber processed in previous pass
                                RevertToUnprocessedGrabberTransform(mainGrabInfo.Grabbers[0]);

                                // Do look-at
                                PerformPrimaryLookAtSecondaryGrab(mainGrabInfo, mainGrabInfo.Grabbers[0], grabInfo.GrabbableParentBeingGrabbed, mainGrabInfo.GrabbedPoints[0], grabber, grabbableObject, pointIndex, mainGrabInfo.LookAtT);
                                performedLookAt = true;
                            }
                        }

                        if (!performedLookAt && grabInfo.ChildDependentGrabCount > 0 && grabInfo.ChildDependentGrabProcessed == grabInfo.ChildDependentGrabCount - 1)
                        {
                            triggerApplyConstraintsEvents = true;
                        }
                    }

                    if (grabbableObject.RotationProvider != UxrRotationProvider.HandPositionAroundPivot)
                    {
                        // Place
                        Vector3    finalPosition = grabber.transform.position;
                        Quaternion finalRotation = grabber.transform.rotation;

                        finalRotation *= grabbableObject.GetGrabPointRelativeGrabRotation(grabber, pointIndex);
                        finalPosition =  grabber.transform.TransformPoint(grabbableObject.GetGrabPointRelativeGrabPosition(grabber, pointIndex));

                        grabbableObject.transform.SetPositionAndRotation(finalPosition, finalRotation);
                    }

                    // Restrict movement?
                    if (point == 0 && !(grabbableObject.RotationProvider == UxrRotationProvider.HandPositionAroundPivot && grabbableObject.NeedsTwoHandsToRotate && grabbableObject.GrabPointCount > 1 && grabInfo.GrabbedPoints.Count < 2))
                    {
                        grabbableObject.CheckAndApplyConstraints(grabber, pointIndex, grabInfo.LocalPositionBeforeUpdate, grabInfo.LocalRotationBeforeUpdate, triggerApplyConstraintsEvents);
                    }

                    // Check if the user separated the hands too much to drop it from the hand that went beyond the distance threshold
                    if (Vector3.Distance(grabbableObject.GetGrabbedPointGrabProximityPosition(grabber, pointIndex), grabber.transform.position) > grabbableObject.LockedGrabReleaseDistance)
                    {
                        if (grabbableObject.IsConstrained)
                        {
                            StartSmoothHandTransition(grabber, grabbableObject, pointIndex);
                            listToRelease.Add(grabber);
                            grabber.Avatar.ControllerInput.SendHapticFeedback(grabber.Side, UxrHapticClipType.Click, 1.0f);
                        }
                    }

                    // Lock hand always, since we have the TriggerApplyConstraintsEvent() earlier and the hand needs to be locked to the object
                    grabbableObject.CheckAndApplyLockHand(grabber, pointIndex);
                }
            }

            // Activate/deactivate anchor elements based on proximity to potential drop anchors
            UxrGrabbableObjectAnchor anchorCandidate = null;
            float                    minDistance     = float.MaxValue;

            if (grabbableObject.UsesGrabbableParentDependency == false && grabbableObject.IsPlaceable)
            {
                foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
                {
                    if (grabbableObject.CanBePlacedOnAnchor(anchorPair.Key, out float distance) && distance < minDistance)
                    {
                        anchorCandidate = anchorPair.Key;
                        minDistance     = distance;
                    }
                }
            }

            // Is there a compatible anchor if we would release it? store the grabber for later
            if (anchorCandidate != null)
            {
                _grabbableObjectAnchors[anchorCandidate].HasCompatibleObjectNear = true;
                _grabbableObjectAnchors[anchorCandidate].FullGrabberNear         = grabInfo.Grabbers[0];
                _grabbableObjectAnchors[anchorCandidate].LastFullGrabberNear     = grabInfo.Grabbers[0];
            }
        }

        /// <summary>
        ///     Makes the primary grab look at the secondary grab.
        /// </summary>
        /// <param name="mainGrabInfo">Information of the main grab</param>
        /// <param name="grabberMain">Grabber that is grabbing the main grab point</param>
        /// <param name="grabbableObjectMain">
        ///     Grabbable object that has the main grab. It may be different of the secondary
        ///     grabbable object when processing dependent grabs
        /// </param>
        /// <param name="grabPointMain">Main grab point</param>
        /// <param name="grabberSecondary">Grabber that is grabbing the secondary grab point</param>
        /// <param name="grabbableObjectSecondary">
        ///     Grabbable object that has the secondary grab. It may be different of the primary
        ///     grabbable object when processing dependent grabs
        /// </param>
        /// <param name="grabPointSecondary">Secondary grab point index</param>
        /// <param name="lookAtT">Interpolation value [0.0, 1.0] so that the look-at can be performed in a smooth transition</param>
        private void PerformPrimaryLookAtSecondaryGrab(RuntimeGrabInfo    mainGrabInfo,
                                                       UxrGrabber         grabberMain,
                                                       UxrGrabbableObject grabbableObjectMain,
                                                       int                grabPointMain,
                                                       UxrGrabber         grabberSecondary,
                                                       UxrGrabbableObject grabbableObjectSecondary,
                                                       int                grabPointSecondary,
                                                       float              lookAtT)
        {
            // First place in main hand

            Vector3    finalPosition = grabberMain.transform.position;
            Quaternion finalRotation = grabberMain.transform.rotation;

            if (grabbableObjectMain.GetGrabPointSnapModeAffectsRotation(grabPointMain, UxrHandSnapDirection.ObjectToHand) == false)
            {
                finalRotation *= grabbableObjectMain.GetGrabPointRelativeGrabRotation(grabberMain, grabPointMain);
            }

            if (grabbableObjectMain.GetGrabPointSnapModeAffectsPosition(grabPointMain, UxrHandSnapDirection.ObjectToHand) == false)
            {
                finalPosition = grabberMain.transform.TransformPoint(grabbableObjectMain.GetGrabPointRelativeGrabPosition(grabberMain, grabPointMain));
            }

            grabbableObjectMain.transform.SetPositionAndRotation(finalPosition, finalRotation);

            // Constrain using main hand

            bool raiseApplyConstraintsEvents = mainGrabInfo.ChildDependentGrabCount == 0 || mainGrabInfo.ChildDependentGrabProcessed == mainGrabInfo.ChildDependentGrabCount - 1;
            bool isRotationConstrained       = grabbableObjectMain.RotationConstraint != UxrRotationConstraintMode.Free;

            grabbableObjectMain.CheckAndApplyConstraints(grabberMain, grabPointMain, mainGrabInfo.LocalPositionBeforeUpdate, mainGrabInfo.LocalRotationBeforeUpdate, raiseApplyConstraintsEvents && !isRotationConstrained);

            // Now just rotate towards the second hand
            Vector3 otherHandPoint = grabbableObjectSecondary.GetGrabPointSnapModeAffectsPosition(grabPointSecondary, UxrHandSnapDirection.ObjectToHand)
                                                 ? grabbableObjectSecondary.GetGrabbedPointGrabAlignPosition(grabberSecondary, grabPointSecondary)
                                                 : grabbableObjectSecondary.transform.TransformPoint(grabbableObjectSecondary.GetGrabPointRelativeGrabberPosition(grabberSecondary, grabPointSecondary));

            Vector3 currentVectorToSecondHand = otherHandPoint - grabberMain.transform.position;
            Vector3 desiredVectorToSecondHand = grabberSecondary.transform.position - grabberMain.transform.position;

            Vector3 rotationAxis = Vector3.Cross(currentVectorToSecondHand, desiredVectorToSecondHand);

            grabbableObjectMain.transform.RotateAround(grabberMain.transform.position, rotationAxis, Vector3.SignedAngle(currentVectorToSecondHand, desiredVectorToSecondHand, rotationAxis) * lookAtT);

            // Second pass to handle possible rotation constraints due to the look-at
            if (isRotationConstrained)
            {
                grabbableObjectMain.CheckAndApplyConstraints(grabberMain, grabPointMain, mainGrabInfo.LocalPositionBeforeUpdate, mainGrabInfo.LocalRotationBeforeUpdate, raiseApplyConstraintsEvents);
            }

            if (grabbableObjectMain.GetGrabPointSnapModeAffectsRotation(grabPointMain, UxrHandSnapDirection.ObjectToHand))
            {
                // Keep grabber locked to new grabbable object orientation
                grabbableObjectMain.CheckAndApplyLockHand(grabberMain, grabPointMain);
            }
        }

        /// <summary>
        ///     Handles a grabbable object being grabbed by multiple grabbers.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="listToRelease">If any grabbers release the object they will be added to this list</param>
        /// <param name="lookAtT">Interpolation value [0.0, 1.0] so that the look-at can be performed in a smooth transition</param>
        private void HandleMultiGrab(UxrGrabbableObject grabbableObject, ref List<UxrGrabber> listToRelease, float lookAtT)
        {
            if (_currentGrabs.TryGetValue(grabbableObject, out RuntimeGrabInfo grabInfo))
            {
                int indexMain      = grabbableObject.FirstGrabPointIsMain ? grabInfo.MainPointIndex : 0;
                int indexSecondary = indexMain == 0 ? 1 : 0;

                UxrGrabber grabberMain      = grabInfo.Grabbers[indexMain];
                UxrGrabber grabberSecondary = grabInfo.Grabbers[indexSecondary];

                int pointIndexMain      = grabInfo.GrabbedPoints[indexMain];
                int pointIndexSecondary = grabInfo.GrabbedPoints[indexSecondary];

                // Multi-grab LookAt
                PerformPrimaryLookAtSecondaryGrab(grabInfo, grabberMain, grabbableObject, pointIndexMain, grabberSecondary, grabbableObject, pointIndexSecondary, lookAtT);

                // Check if the user separated the hands too much to drop it from the hand that went too far
                if (Vector3.Distance(grabbableObject.GetGrabbedPointGrabProximityPosition(grabberSecondary, pointIndexSecondary), grabberSecondary.transform.position) > grabbableObject.LockedGrabReleaseDistance)
                {
                    StartSmoothHandTransition(grabberSecondary, grabbableObject, pointIndexSecondary);
                    listToRelease.Add(grabberSecondary);
                    grabberSecondary.Avatar.ControllerInput.SendHapticFeedback(grabberSecondary.Side, UxrHapticClipType.Click, 1.0f);
                }

                // Now lock the hands if necessary
                grabbableObject.CheckAndApplyLockHand(grabberMain,      pointIndexMain);
                grabbableObject.CheckAndApplyLockHand(grabberSecondary, pointIndexSecondary);
            }
        }

        /// <summary>
        ///     Starts a smooth transition where a hand releases an object and moves/rotates to the actual position/orientation
        ///     defined by the tracking sensor.
        ///     For objects that have constraints, such as objects anchored to the world, it may be possible for hands to move to
        ///     positions that have an offset to the hand position in real life.
        /// </summary>
        /// <param name="grabber">Grabber whose hand will start the transition</param>
        /// <param name="grabbableObject">Object being grabbed</param>
        /// <param name="grabPoint">Grab point being grabbed</param>
        private void StartSmoothHandTransition(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint)
        {
            if (_handTransitions.ContainsKey(grabber) == false)
            {
                if (grabbableObject.ComputeRequiredGrabberTransform(grabber, grabPoint, out Vector3 grabberPosition, out Quaternion grabberRotation))
                {
                    _handTransitions.Add(grabber, new HandTransitionInfo(grabber, grabberPosition, grabberRotation));
                }
            }
        }

        /// <summary>
        ///     Stops a transition started by <see cref="StartSmoothHandTransition" />.
        /// </summary>
        /// <param name="grabber">Grabber in transition</param>
        private void StopSmoothHandTransition(UxrGrabber grabber)
        {
            if (_handTransitions.ContainsKey(grabber))
            {
                _handTransitions.Remove(grabber);
            }
        }

        /// <summary>
        ///     Updates the smooth transitions started by <see cref="StartSmoothHandTransition" />.
        /// </summary>
        /// <param name="deltaTime">Update delta time</param>
        private void UpdateSmoothHandTransitions(float deltaTime)
        {
            List<UxrGrabber> keysToRemove = null;

            // Perform smooth interpolation
            foreach (KeyValuePair<UxrGrabber, HandTransitionInfo> handTransitionPair in _handTransitions)
            {
                if (deltaTime > 0.0f)
                {
                    handTransitionPair.Value.Timer -= deltaTime;
                }

                float t = 1.0f - Mathf.Clamp01(handTransitionPair.Value.Timer / UxrGrabbableObject.HandLockSeconds);

                handTransitionPair.Key.HandBone.SetPositionAndRotation(Vector3.Lerp(handTransitionPair.Key.Avatar.transform.TransformPoint(handTransitionPair.Value.StartLocalAvatarPosition), handTransitionPair.Key.transform.TransformPoint(handTransitionPair.Key.HandBoneRelativePos), t),
                                                                       Quaternion.Slerp(handTransitionPair.Key.Avatar.transform.rotation * handTransitionPair.Value.StartLocalAvatarRotation, handTransitionPair.Key.transform.rotation * handTransitionPair.Key.HandBoneRelativeRot, t));

                if (deltaTime > 0.0f && handTransitionPair.Value.Timer < 0.0f && keysToRemove == null)
                {
                    keysToRemove = new List<UxrGrabber>();
                    keysToRemove.Add(handTransitionPair.Key);
                }
            }

            // Remove finished transitions
            if (keysToRemove != null)
            {
                foreach (UxrGrabber grabber in keysToRemove)
                {
                    _handTransitions.Remove(grabber);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private Dictionary<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> _grabbableObjectAnchors;
        private Dictionary<UxrGrabbableObject, RuntimeGrabInfo>                 _currentGrabs;
        private Dictionary<UxrGrabber, HandTransitionInfo>                      _handTransitions;

        #endregion
    }
}