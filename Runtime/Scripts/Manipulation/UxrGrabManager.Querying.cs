// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.Querying.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
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
        /// <returns>Whether a grabbable object was found</returns>
        public bool GetClosestGrabbableObject(UxrAvatar avatar, UxrHandSide handSide, out UxrGrabbableObject grabbableObject)
        {
            grabbableObject = null;
            
            foreach (UxrGrabber grabber in UxrGrabber.GetComponents(avatar))
            {
                if (grabber.Side == handSide)
                {
                    return GetClosestGrabbableObject(avatar, handSide, out grabbableObject, out int _);
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
                        else if (candidate.Priority == maxPriority)
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
        ///     Gets whether grabbing a given <see cref="UxrGrabbableObject" /> using a certain <see cref="UxrGrabber" /> will make
        ///     the grabber's renderer show up as hidden due to the parameters set in the inspector.
        /// </summary>
        /// <param name="grabber">Grabber to check</param>
        /// <param name="grabbableObject">Grabbable object to check</param>
        /// <returns>Whether the renderer would be hidden when grabbed</returns>
        public bool ShouldHideHandRenderer(UxrGrabber grabber, UxrGrabbableObject grabbableObject)
        {
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                int grabPoint = manipulationInfo.GetGrabbedPoint(grabber);

                if (grabPoint != -1)
                {
                    return grabbableObject.GetGrabPoint(grabPoint).HideHandGrabberRenderer;
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                int grabPoint = manipulationInfo.GetGrabbedPoint(grabber);

                if (grabPoint != -1)
                {
                    UxrHandPoseAsset handPoseAsset = grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(grabber.Avatar).HandPose;
                    return handPoseAsset != null ? handPoseAsset.name : null;
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                int grabPoint = manipulationInfo.GetGrabbedPoint(grabber);

                if (grabPoint != -1)
                {
                    return grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(grabber.Avatar).PoseBlendValue;
                }
            }

            return 0.0f;
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
        ///     Checks if an avatar's hand is grabbing a grabbable object.
        /// </summary>
        /// <param name="avatar">Avatar to check</param>
        /// <param name="grabbableObject">Object to check if it is being grabbed</param>
        /// <param name="handSide">Whether to check the left hand or right hand</param>
        /// <param name="alsoCheckDependentGrab">
        ///     Whether to also check for any parent or child <see cref="UxrGrabbableObject" /> that
        ///     is physically connected.
        /// </param>
        /// <returns>Whether the object is being grabbed by the avatar using the given hand</returns>
        public bool IsHandGrabbing(UxrAvatar avatar, UxrGrabbableObject grabbableObject, UxrHandSide handSide, bool alsoCheckDependentGrab)
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
                        // Grabbing a parent/child?

                        if (GetParentsBeingGrabbedChain(grabber.GrabbedObject).Contains(grabbableObject) || GetChildrenBeingGrabbed(grabber.GrabbedObject).Contains(grabbableObject))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets all the objects currently being grabbed.
        /// </summary>
        /// <returns>Objects being grabbed</returns>
        public IEnumerator<UxrGrabbableObject> GetObjectsBeingGrabbed()
        {
            foreach (var manipulation in _currentManipulations)
            {
                yield return manipulation.Key;
            }
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

                        if (GetParentsBeingGrabbedChain(grabber.GrabbedObject).Contains(grabbableObject) || GetChildrenBeingGrabbed(grabber.GrabbedObject).Contains(grabbableObject))
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
            return _currentManipulations != null && grabbableObject != null && _currentManipulations.ContainsKey(grabbableObject);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed using the given grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Grab point of the grabbable object to check</param>
        /// <returns>Whether the grab point is being grabbed using the given grab point</returns>
        public bool IsBeingGrabbed(UxrGrabbableObject grabbableObject, int point)
        {
            if (_currentManipulations == null || grabbableObject == null)
            {
                return false;
            }

            return _currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.GrabbedPoints.Contains(point);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed by an avatar.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="avatar">The avatar to check</param>
        /// <returns>Whether it is being grabbed by the avatar</returns>
        public bool IsBeingGrabbedBy(UxrGrabbableObject grabbableObject, UxrAvatar avatar)
        {
            if (_currentManipulations == null || grabbableObject == null)
            {
                return false;
            }

            return _currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.Grabbers.Any(grabber => grabber.Avatar == avatar);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed by a specific grabber.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="grabber">The grabber to check</param>
        /// <returns>Whether it is being grabbed by the given grabber</returns>
        public bool IsBeingGrabbedBy(UxrGrabbableObject grabbableObject, UxrGrabber grabber)
        {
            if (_currentManipulations == null || grabbableObject == null)
            {
                return false;
            }

            return _currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.Grabbers.Any(grb => grabber == grb);
        }

        /// <summary>
        ///     Checks whether the given grabbable object is being grabbed using any other grab point than the specified.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">Grab point of the grabbable object not to check</param>
        /// <returns>Whether any other grab point is being grabbed</returns>
        public bool IsBeingGrabbedByOtherThan(UxrGrabbableObject grabbableObject, int point)
        {
            if (_currentManipulations == null || grabbableObject == null)
            {
                return false;
            }

            return _currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.GrabbedPoints.Any(grabPoint => point != grabPoint);
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
            if (_currentManipulations == null || grabbableObject == null)
            {
                return false;
            }

            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                foreach (RuntimeGrabInfo grabInfo in manipulationInfo.Grabs)
                {
                    if (grabInfo.Grabber != grabber || grabInfo.GrabbedPoint != point)
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
                if (grabberCandidate.GrabbedObject == grabbableObject)
                {
                    foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabbableObject))
                    {
                        if (grabInfo.GrabbedPoint == point)
                        {
                            grabber = grabInfo.Grabber;
                            return true;
                        }
                    }
                }
            }

            return false;
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

            foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabbableObject))
            {
                if (grabInfo.GrabbedPoint == point || point == -1)
                {
                    grabbers ??= new List<UxrGrabber>();

                    if (!grabbers.Contains(grabInfo.Grabber))
                    {
                        grabbers.Add(grabInfo.Grabber);
                    }
                }
            }

            return grabbers != null && grabbers.Count > 0;
        }

        /// <summary>
        ///     Gets the grabbers that are grabbing the object using a specific grab point.
        /// </summary>
        /// <param name="grabbableObject">The grabbable object</param>
        /// <param name="point">The grab point or -1 to get all grabbed points</param>
        /// <returns>List of grabbers</returns>
        public IEnumerable<UxrGrabber> GetGrabbingHands(UxrGrabbableObject grabbableObject, int point = -1)
        {
            foreach (RuntimeGrabInfo grabInfo in GetGrabs(grabbableObject))
            {
                if (grabInfo.GrabbedPoint == point || point == -1)
                {
                    yield return grabInfo.Grabber;
                }
            }
        }

        /// <summary>
        ///     Gets the grab point that the <see cref="UxrGrabber" /> is currently grabbing on a <see cref="UxrGrabbableObject" />
        ///     .
        /// </summary>
        /// <param name="grabber">Grabber to get the grabbed point from</param>
        /// <returns>Grab point index that is being grabbed or -1 if there is no object currently being grabbed</returns>
        public int GetGrabbedPoint(UxrGrabber grabber)
        {
            if (grabber && grabber.GrabbedObject != null && _currentManipulations.TryGetValue(grabber.GrabbedObject, out RuntimeManipulationInfo manipulationInfo))
            {
                foreach (RuntimeGrabInfo grabInfo in manipulationInfo.Grabs)
                {
                    if (grabInfo.Grabber == grabber)
                    {
                        return grabInfo.GrabbedPoint;
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return manipulationInfo.Grabs.Count;
            }

            return 0;
        }

        /// <summary>
        ///     Gets the number of hands that are grabbing the given object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="includeChildGrabs">Whether to also count dependent child grabbable objects that are being grabbed</param>
        /// <returns>Number of hands grabbing the object</returns>
        public int GetGrabbingHandCount(UxrGrabbableObject grabbableObject, bool includeChildGrabs = true)
        {
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return manipulationInfo.Grabs.Count + (includeChildGrabs ? GetChildrenBeingGrabbed(grabbableObject).Count() : 0);
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return manipulationInfo.SourceAnchor;
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.Grabs.Any())
            {
                return smooth ? manipulationInfo.Grabs[0].Grabber.SmoothVelocity : manipulationInfo.Grabs[0].Grabber.Velocity;
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
            if (_currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo) && manipulationInfo.Grabs.Any())
            {
                return smooth ? manipulationInfo.Grabs[0].Grabber.SmoothAngularVelocity : manipulationInfo.Grabs[0].Grabber.AngularVelocity;
            }

            return Vector3.zero;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to get the grab information of a specific <see cref="UxrGrabber" />.
        /// </summary>
        /// <param name="grabber">Grabber to get the grab information from</param>
        /// <param name="grabInfo">Returns the grab information</param>
        /// <returns>True if successful, false if the grabber is not valid or isn't grabbing any object</returns>
        private bool TryGetGrabInfo(UxrGrabber grabber, out RuntimeGrabInfo grabInfo)
        {
            grabInfo = null;

            if (grabber && grabber.GrabbedObject && _currentManipulations.TryGetValue(grabber.GrabbedObject, out RuntimeManipulationInfo manipulationInfo))
            {
                grabInfo = manipulationInfo.GetGrabInfo(grabber);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns all grabs on the given grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to get the grab information from</param>
        /// <returns>List of grabs</returns>
        private List<RuntimeGrabInfo> GetGrabs(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject != null && _currentManipulations.TryGetValue(grabbableObject, out RuntimeManipulationInfo manipulationInfo))
            {
                return new List<RuntimeGrabInfo>(manipulationInfo.Grabs);
            }

            return new List<RuntimeGrabInfo>();
        }

        /// <summary>
        ///     Gets the parent being grabbed of a given <see cref="UxrGrabbableObject" />.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Parent <see cref="UxrGrabbableObject" /> that is being grabbed or null if there isn't any</returns>
        private UxrGrabbableObject GetParentBeingGrabbed(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject != null && grabbableObject.transform.parent != null)
            {
                UxrGrabbableObject parentGrabbableObject = grabbableObject.transform.parent.GetComponentInParent<UxrGrabbableObject>();

                if (parentGrabbableObject != null)
                {
                    if (IsBeingGrabbed(parentGrabbableObject))
                    {
                        return parentGrabbableObject;
                    }

                    return GetParentBeingGrabbed(parentGrabbableObject);
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the chain of parents of a given <see cref="UxrGrabbableObject" /> that are being grabbed in bottom to top
        ///     hierarchical order.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>Chain of parents being grabbed in bottom to top hierarchical order</returns>
        private IEnumerable<UxrGrabbableObject> GetParentsBeingGrabbedChain(UxrGrabbableObject grabbableObject)
        {
            UxrGrabbableObject current = grabbableObject;

            while (current != null)
            {
                current = GetParentBeingGrabbed(current);

                if (current != null)
                {
                    yield return current;
                }
            }
        }

        /// <summary>
        ///     Gets a <see cref="UxrGrabbableObject" />'s list of children being grabbed.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>List of children that are being grabbed</returns>
        private IEnumerable<UxrGrabbableObject> GetChildrenBeingGrabbed(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject == null)
            {
                yield break;
            }

            foreach (UxrGrabbableObject child in grabbableObject.AllChildren)
            {
                if (child != grabbableObject && _currentManipulations.ContainsKey(child))
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        ///     Gets a <see cref="UxrGrabbableObject" />'s list of direct grabbable children that are being grabbed
        ///     and control the direction of the grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>List of direct grabbable children that are being grabbed and control the grabbable object direction</returns>
        private IEnumerable<UxrGrabbableObject> GetDirectChildrenLookAtBeingGrabbed(UxrGrabbableObject grabbableObject)
        {
            foreach (RuntimeManipulationInfo childManipulation in GetDirectChildrenLookAtManipulations(grabbableObject))
            {
                yield return childManipulation.GrabbableObject;
            }
        }

        /// <summary>
        ///     Gets a <see cref="UxrGrabbableObject" />'s list of direct grabbable children manipulations that
        ///     control the direction of the grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <returns>
        ///     List of manipulations of direct grabbable children that are being grabbed and control the grabbable object
        ///     direction
        /// </returns>
        private IEnumerable<RuntimeManipulationInfo> GetDirectChildrenLookAtManipulations(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject == null)
            {
                yield break;
            }

            foreach (UxrGrabbableObject child in grabbableObject.DirectChildrenLookAts)
            {
                if (_currentManipulations.TryGetValue(child, out RuntimeManipulationInfo childManipulationInfo))
                {
                    yield return childManipulationInfo;
                }
            }
        }

        /// <summary>
        ///     Gets the world-space snap position of a given <see cref="UxrGrabber" /> grabbing the object.
        ///     Depending on the object's snap settings, this will be the grab point snap position (either
        ///     object to hand or hand to object) or the same grip position relative to the object when the
        ///     grab was performed.
        /// </summary>
        /// <param name="grabber">Grabber to get the snap position of</param>
        /// <returns>Snap position or <see cref="Vector3.zero" /> if the grabber isn't currently grabbing an object</returns>
        private Vector3 GetGrabbedPointGrabAlignPosition(UxrGrabber grabber)
        {
            if (grabber.GrabbedObject == null || !_currentManipulations.TryGetValue(grabber.GrabbedObject, out RuntimeManipulationInfo manipulationInfo) || !TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return Vector3.zero;
            }

            if (grabber.GrabbedObject.GetGrabPointSnapModeAffectsPosition(manipulationInfo.GetGrabbedPoint(grabber), UxrHandSnapDirection.ObjectToHand))
            {
                // Snap to grab point so that object goes to hand
                return TransformExt.GetWorldPosition(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignPosition);
            }

            if (grabber.GrabbedObject.GetGrabPointSnapModeAffectsPosition(manipulationInfo.GetGrabbedPoint(grabber), UxrHandSnapDirection.HandToObject))
            {
                // Snap to grab point so that hand goes to object
                Transform snapTransform = grabber.GrabbedObject.GetGrabPointGrabAlignTransform(grabber.Avatar, manipulationInfo.GetGrabbedPoint(grabber), grabber.Side);
                return snapTransform.TransformPoint(grabInfo.RelativeUsedGrabAlignPosition);
            }

            // Keep same grip position relative to the object as when it was grabbed
            return grabber.GrabbedObject.transform.TransformPoint(GetGrabPointRelativeGrabberPosition(grabber));
        }

        /// <summary>
        ///     Gets the world-space snap rotation of a given <see cref="UxrGrabber" /> grabbing the object.
        ///     Depending on the object's snap settings, this will be the grab point snap rotation (either
        ///     object to hand or hand to object) or the same grip rotation relative to the object when the
        ///     grab was performed.
        /// </summary>
        /// <param name="grabber">Grabber to get the snap rotation of</param>
        /// <returns>Snap rotation or <see cref="Quaternion.identity" /> if the grabber isn't currently grabbing an object</returns>
        private Quaternion GetGrabbedPointGrabAlignRotation(UxrGrabber grabber)
        {
            if (grabber.GrabbedObject == null || !_currentManipulations.TryGetValue(grabber.GrabbedObject, out RuntimeManipulationInfo manipulationInfo) || !TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return Quaternion.identity;
            }

            if (grabber.GrabbedObject.GetGrabPointSnapModeAffectsRotation(manipulationInfo.GetGrabbedPoint(grabber), UxrHandSnapDirection.ObjectToHand))
            {
                // Snap to grab point so that object rotates to hand
                return TransformExt.GetWorldRotation(grabInfo.GrabAlignParentTransformUsed, grabInfo.RelativeGrabAlignRotation);
            }

            if (grabber.GrabbedObject.GetGrabPointSnapModeAffectsRotation(manipulationInfo.GetGrabbedPoint(grabber), UxrHandSnapDirection.HandToObject))
            {
                // Snap to grab point so that hand rotates to object
                Transform snapTransform = grabber.GrabbedObject.GetGrabPointGrabAlignTransform(grabber.Avatar, manipulationInfo.GetGrabbedPoint(grabber), grabber.Side);
                return snapTransform.rotation * grabInfo.RelativeUsedGrabAlignRotation;
            }

            // Keep same grip rotation relative to the object as when it was grabbed
            return grabber.GrabbedObject.transform.rotation * GetGrabPointRelativeGrabberRotation(grabber);
        }

        /// <summary>
        ///     Gets the position that is used to compute proximity from a <see cref="UxrGrabber" /> to the grabbed point.
        /// </summary>
        /// <param name="grabber">Grabber to get the proximity point for</param>
        /// <returns>
        ///     Position required to compute the proximity to or <see cref="Vector3.zero" /> if the grabber isn't currently
        ///     grabbing an object
        /// </returns>
        private Vector3 GetGrabbedPointGrabProximityPosition(UxrGrabber grabber)
        {
            if (TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return grabber.GrabbedObject.transform.TransformPoint(grabInfo.RelativeProximityPosition);
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Gets the relative position of the object to the grabber at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber with the object being grabbed</param>
        /// <returns>
        ///     Relative position or <see cref="Vector3.zero" /> if the grabber isn't currently grabbing an object.
        /// </returns>
        private Vector3 GetGrabPointRelativeGrabPosition(UxrGrabber grabber)
        {
            if (TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.RelativeGrabPosition;
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Gets the relative rotation of the object to the grabber at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber with the object being grabbed</param>
        /// <returns>
        ///     Relative rotation or <see cref="Quaternion.identity" /> if the grabber isn't currently grabbing an object.
        /// </returns>
        private Quaternion GetGrabPointRelativeGrabRotation(UxrGrabber grabber)
        {
            if (TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.RelativeGrabRotation;
            }

            return Quaternion.identity;
        }

        /// <summary>
        ///     Gets the relative position of the grabber to the object it is grabbing at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber with the object being grabbed</param>
        /// <returns>
        ///     Relative position or <see cref="Vector3.zero" /> if the grabber isn't currently grabbing an object.
        /// </returns>
        private Vector3 GetGrabPointRelativeGrabberPosition(UxrGrabber grabber)
        {
            if (TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.RelativeGrabberPosition;
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Gets the relative rotation of the grabber to the object it is grabbing at the time it was grabbed.
        /// </summary>
        /// <param name="grabber">Grabber with the object being grabbed</param>
        /// <returns>
        ///     Relative rotation or <see cref="Quaternion.identity" /> if the grabber isn't currently grabbing an object.
        /// </returns>
        private Quaternion GetGrabPointRelativeGrabberRotation(UxrGrabber grabber)
        {
            if (TryGetGrabInfo(grabber, out RuntimeGrabInfo grabInfo))
            {
                return grabInfo.RelativeGrabberRotation;
            }

            return Quaternion.identity;
        }

        #endregion
    }
}