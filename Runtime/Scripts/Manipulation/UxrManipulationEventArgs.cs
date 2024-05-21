// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Serialization;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     <para>
    ///         Event parameters for most manipulation events:
    ///     </para>
    ///     <see cref="UxrGrabManager" />:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="UxrGrabManager.GrabTrying" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectGrabbing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectGrabbed" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectReleasing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectReleased" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectPlacing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectPlaced" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectRemoving" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.ObjectRemoved" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.AnchorRangeEntered" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.AnchorRangeLeft" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.PlacedObjectRangeEntered" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabManager.PlacedObjectRangeLeft" />
    ///         </item>
    ///     </list>
    ///     <see cref="UxrGrabbableObject" />:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Grabbing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Grabbed" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Releasing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Released" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Placing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObject.Placed" />
    ///         </item>
    ///     </list>
    ///     <see cref="UxrGrabbableObjectAnchor" />:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="UxrGrabbableObjectAnchor.Placing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObjectAnchor.Placed" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObjectAnchor.Removing" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObjectAnchor.Removed" />
    ///         </item>
    ///         <item>
    ///             <see cref="UxrGrabbableObjectAnchor.SmoothPlaceTransitionEnded" />
    ///         </item>
    ///     </list>
    /// </summary>
    public class UxrManipulationEventArgs : EventArgs, IUxrSerializable
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the manipulation changed an object from not being grabbed at all to being grabbed or vice-versa.
        ///     This is useful to filter out grabs or releases on an object that is still being grabbed using another hand.<br/>
        ///     <see cref="IsGrabbedStateChanged" /> is true if <see cref="IsMultiHands" /> and <see cref="IsSwitchHands" /> are
        ///     both false.
        /// </summary>
        public bool IsGrabbedStateChanged => !IsMultiHands && !IsSwitchHands;

        /// <summary>
        ///     The type of event.
        /// </summary>
        public UxrManipulationEventType EventType
        {
            get => _eventType;
            private set => _eventType = value;
        }

        /// <summary>
        ///     Gets the grabbable object related to the event. Can be null if the event doesn't use this property. Check the event
        ///     documentation to see how the property is used.
        /// </summary>
        public UxrGrabbableObject GrabbableObject
        {
            get => _grabbableObject;
            private set => _grabbableObject = value;
        }

        /// <summary>
        ///     Gets the grabbable object anchor related to the event. Can be null if the event doesn't use this property. Check
        ///     the event documentation to see how the property is used.
        /// </summary>
        public UxrGrabbableObjectAnchor GrabbableAnchor
        {
            get => _grabbableAnchor;
            private set => _grabbableAnchor = value;
        }

        /// <summary>
        ///     Gets the grabber related to the event. Can be null if the event doesn't use this property. Check the event
        ///     documentation to see how the property is used.
        /// </summary>
        public UxrGrabber Grabber
        {
            get => _grabber;
            private set => _grabber = value;
        }

        /// <summary>
        ///     Gets the grabbable object's grab point index related to the event. Can be meaningless if the event doesn't use this
        ///     property. Check the event documentation to see how the property is used.
        /// </summary>
        public int GrabPointIndex
        {
            get => _grabPointIndex;
            private set => _grabPointIndex = value;
        }

        /// <summary>
        ///     Gets whether the manipulation used more than one hand. Can be meaningless if the event doesn't use this property.
        ///     Check the event documentation to see how the property is used.
        /// </summary>
        public bool IsMultiHands
        {
            get => _isMultiHands;
            private set => _isMultiHands = value;
        }

        /// <summary>
        ///     Gets whether the event was the result of passing the object from one hand to the other. Can be meaningless if the
        ///     event doesn't use this property. Check the event documentation to see how the property is used.
        /// </summary>
        public bool IsSwitchHands
        {
            get => _isSwitchHands;
            private set => _isSwitchHands = value;
        }

        /// <summary>
        ///     Gets the release velocity for release events.
        /// </summary>
        public Vector3 ReleaseVelocity
        {
            get => _releaseVelocity;
            private set => _releaseVelocity = value;
        }

        /// <summary>
        ///     Gets the release angular velocity for release events.
        /// </summary>
        public Vector3 ReleaseAngularVelocity
        {
            get => _releaseAngularVelocity;
            private set => _releaseAngularVelocity = value;
        }

        /// <summary>
        ///     Gets the placement flags in place events.
        /// </summary>
        public UxrPlacementOptions PlacementOptions
        {
            get => _placementOptions;
            private set => _placementOptions = value;
        }

        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets the UxrGrabbableObject position in local UxrGrabber space at the moment of grabbing.
        ///     This is used in multi-player environments to make sure to reproduce the same grab action.
        /// </summary>
        internal Vector3 GrabberLocalObjectPosition
        {
            get => _grabberLocalObjectPosition;
            private set => _grabberLocalObjectPosition = value;
        }

        /// <summary>
        ///     Gets the UxrGrabbableObject rotation in local UxrGrabber space at the moment of grabbing.
        ///     This is used in multi-player environments to make sure to reproduce the same grab action.
        /// </summary>
        internal Quaternion GrabberLocalObjectRotation
        {
            get => _grabberLocalObjectRotation;
            private set => _grabberLocalObjectRotation = value;
        }

        /// <summary>
        ///     Gets the grab snap position in local UxrGrabber space at the moment of grabbing.
        ///     This is used in multi-player environments to make sure to reproduce the same grab action.
        /// </summary>
        internal Vector3 GrabberLocalSnapPosition
        {
            get => _grabberLocalSnapPosition;
            private set => _grabberLocalSnapPosition = value;
        }

        /// <summary>
        ///     Gets the grab snap rotation in local UxrGrabber space at the moment of grabbing.
        ///     This is used in multi-player environments to make sure to reproduce the same grab action.
        /// </summary>
        internal Quaternion GrabberLocalSnapRotation
        {
            get => _grabberLocalSnapRotation;
            private set => _grabberLocalSnapRotation = value;
        }

        /// <summary>
        ///     Gets the release start position for release events.
        /// </summary>
        internal Vector3 ReleaseStartPosition
        {
            get => _releaseStartPosition;
            private set => _releaseStartPosition = value;
        }

        /// <summary>
        ///     Gets the release start rotation for release events.
        /// </summary>
        internal Quaternion ReleaseStartRotation
        {
            get => _releaseStartRotation;
            private set => _releaseStartRotation = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor is private.
        /// </summary>
        private UxrManipulationEventArgs()
        {
        }

        /// <summary>
        ///     Constructor that initializes the event type.
        /// </summary>
        private UxrManipulationEventArgs(UxrManipulationEventType eventType)
        {
            EventType = eventType;
        }

        #endregion

        #region Explicit IUxrSerializable

        /// <inheritdoc />
        int IUxrSerializable.SerializationVersion => 0;

        /// <inheritdoc />
        void IUxrSerializable.Serialize(IUxrSerializer serializer, int serializationVersion)
        {
            serializer.SerializeEnum(ref _eventType);
            serializer.SerializeUniqueComponent(ref _grabbableObject);
            serializer.SerializeUniqueComponent(ref _grabbableAnchor);
            serializer.SerializeUniqueComponent(ref _grabber);
            serializer.Serialize(ref _grabPointIndex);
            serializer.Serialize(ref _isMultiHands);
            serializer.Serialize(ref _isSwitchHands);

            if (EventType == UxrManipulationEventType.Grab)
            {
                serializer.Serialize(ref _grabberLocalSnapPosition);
                serializer.Serialize(ref _grabberLocalSnapRotation);
                serializer.Serialize(ref _grabberLocalObjectPosition);
                serializer.Serialize(ref _grabberLocalObjectRotation);
            }
            else if (EventType == UxrManipulationEventType.Release)
            {
                serializer.Serialize(ref _releaseStartPosition);
                serializer.Serialize(ref _releaseStartRotation);
                serializer.Serialize(ref _releaseVelocity);
                serializer.Serialize(ref _releaseAngularVelocity);
            }
            else if (EventType == UxrManipulationEventType.Place)
            {
                serializer.SerializeEnum(ref _placementOptions);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Constructor for Grab events.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="isMultiHands">Whether the object was already grabbed with one or more hands</param>
        /// <param name="isSwitchHands">Whether the event was a result of passing the grabbable object from one hand to the other</param>
        /// <param name="grabberLocalSnapPosition">Grab snap position in local UxrGrabber space at the moment of grabbing</param>
        /// <param name="grabberLocalSnapRotation">Grab snap rotation in local UxrGrabber space at the moment of grabbing</param>
        public static UxrManipulationEventArgs FromGrab(UxrGrabbableObject       grabbableObject,
                                                        UxrGrabbableObjectAnchor grabbableAnchor,
                                                        UxrGrabber               grabber,
                                                        int                      grabPointIndex,
                                                        bool                     isMultiHands,
                                                        bool                     isSwitchHands,
                                                        Vector3                  grabberLocalSnapPosition,
                                                        Quaternion               grabberLocalSnapRotation)
        {
            UxrManipulationEventArgs eventArgs = new UxrManipulationEventArgs(UxrManipulationEventType.Grab);

            eventArgs.GrabbableObject = grabbableObject;
            eventArgs.GrabbableAnchor = grabbableAnchor;
            eventArgs.Grabber         = grabber;
            eventArgs.GrabPointIndex  = grabPointIndex;
            eventArgs.IsMultiHands    = isMultiHands;
            eventArgs.IsSwitchHands   = isSwitchHands;

            // Internal vars

            if (grabbableObject != null && grabber != null)
            {
                eventArgs.GrabberLocalSnapPosition   = grabberLocalSnapPosition;
                eventArgs.GrabberLocalSnapRotation   = grabberLocalSnapRotation;
                eventArgs.GrabberLocalObjectPosition = grabber.transform.InverseTransformPoint(grabbableObject.transform.position);
                eventArgs.GrabberLocalObjectRotation = Quaternion.Inverse(grabber.transform.rotation) * grabbableObject.transform.rotation;
            }

            return eventArgs;
        }

        /// <summary>
        ///     Constructor for Release events.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="isMultiHands">Whether the object will still be grabbed with one or more hands</param>
        /// <param name="isSwitchHands">Whether the event was a result of passing the grabbable object from one hand to the other</param>
        /// <param name="releaseVelocity">The release velocity</param>
        /// <param name="releaseAngularVelocity">The release angular velocity</param>
        public static UxrManipulationEventArgs FromRelease(UxrGrabbableObject       grabbableObject,
                                                           UxrGrabbableObjectAnchor grabbableAnchor,
                                                           UxrGrabber               grabber,
                                                           int                      grabPointIndex,
                                                           bool                     isMultiHands,
                                                           bool                     isSwitchHands,
                                                           Vector3                  releaseVelocity        = default(Vector3),
                                                           Vector3                  releaseAngularVelocity = default(Vector3))

        {
            UxrManipulationEventArgs eventArgs = new UxrManipulationEventArgs(UxrManipulationEventType.Release);

            eventArgs.GrabbableObject        = grabbableObject;
            eventArgs.GrabbableAnchor        = grabbableAnchor;
            eventArgs.Grabber                = grabber;
            eventArgs.GrabPointIndex         = grabPointIndex;
            eventArgs.IsMultiHands           = isMultiHands;
            eventArgs.IsSwitchHands          = isSwitchHands;
            eventArgs.ReleaseVelocity        = releaseVelocity;
            eventArgs.ReleaseAngularVelocity = releaseAngularVelocity;

            // Internal vars

            if (grabbableObject != null)
            {
                eventArgs.ReleaseStartPosition = grabbableObject.transform.position;
                eventArgs.ReleaseStartRotation = grabbableObject.transform.rotation;
            }

            return eventArgs;
        }

        /// <summary>
        ///     Constructor for Place events.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="placementOptions">The placement options flags</param>
        public static UxrManipulationEventArgs FromPlace(UxrGrabbableObject       grabbableObject,
                                                         UxrGrabbableObjectAnchor grabbableAnchor,
                                                         UxrGrabber               grabber,
                                                         int                      grabPointIndex,
                                                         UxrPlacementOptions      placementOptions)
        {
            UxrManipulationEventArgs eventArgs = new UxrManipulationEventArgs(UxrManipulationEventType.Place);

            eventArgs.GrabbableObject  = grabbableObject;
            eventArgs.GrabbableAnchor  = grabbableAnchor;
            eventArgs.Grabber          = grabber;
            eventArgs.GrabPointIndex   = grabPointIndex;
            eventArgs.IsMultiHands     = false;
            eventArgs.IsSwitchHands    = false;
            eventArgs.PlacementOptions = placementOptions;

            return eventArgs;
        }

        /// <summary>
        ///     Constructor for Release events.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="isMultiHands">Whether the event was a result of a manipulation with more than one hand</param>
        /// <param name="isSwitchHands">Whether the event was a result of passing the grabbable object from one hand to the other</param>
        public static UxrManipulationEventArgs FromRemove(UxrGrabbableObject       grabbableObject,
                                                          UxrGrabbableObjectAnchor grabbableAnchor,
                                                          UxrGrabber               grabber,
                                                          int                      grabPointIndex = 0,
                                                          bool                     isMultiHands   = false,
                                                          bool                     isSwitchHands  = false)
        {
            UxrManipulationEventArgs eventArgs = new UxrManipulationEventArgs(UxrManipulationEventType.Remove);

            eventArgs.GrabbableObject = grabbableObject;
            eventArgs.GrabbableAnchor = grabbableAnchor;
            eventArgs.Grabber         = grabber;
            eventArgs.GrabPointIndex  = grabPointIndex;
            eventArgs.IsMultiHands    = isMultiHands;
            eventArgs.IsSwitchHands   = isSwitchHands;

            return eventArgs;
        }

        /// <summary>
        ///     Constructor for PlacedObjectRangeEntered/Left, AnchorRangeEntered/Left and GrabTrying events.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="isMultiHands">Whether the event was a result of a manipulation with more than one hand</param>
        /// <param name="isSwitchHands">Whether the event was a result of passing the grabbable object from one hand to the other</param>
        public static UxrManipulationEventArgs FromOther(UxrManipulationEventType eventType,
                                                         UxrGrabbableObject       grabbableObject,
                                                         UxrGrabbableObjectAnchor grabbableAnchor,
                                                         UxrGrabber               grabber,
                                                         int                      grabPointIndex = 0,
                                                         bool                     isMultiHands   = false,
                                                         bool                     isSwitchHands  = false)
        {
            UxrManipulationEventArgs eventArgs = new UxrManipulationEventArgs(eventType);

            eventArgs.GrabbableObject = grabbableObject;
            eventArgs.GrabbableAnchor = grabbableAnchor;
            eventArgs.Grabber         = grabber;
            eventArgs.GrabPointIndex  = grabPointIndex;
            eventArgs.IsMultiHands    = isMultiHands;
            eventArgs.IsSwitchHands   = isSwitchHands;

            return eventArgs;
        }

        /// <summary>
        ///     Gets a string that describes the event.
        /// </summary>
        /// <param name="includeIds">Whether to include information of the component unique IDs</param>
        /// <returns>String with a description of the event</returns>
        public string ToString(bool includeIds = false)
        {
            switch (EventType)
            {
                case UxrManipulationEventType.Grab:    return $"Grabbing {GetGrabbableObjectLogInfo(this,  includeIds)}{GetGrabberLogInfo(this,                                           includeIds)}";
                case UxrManipulationEventType.Release: return $"Releasing {GetGrabbableObjectLogInfo(this, includeIds)}{GetGrabberLogInfo(this,                                           includeIds)}";
                case UxrManipulationEventType.Place:   return $"Placing {GetGrabbableObjectLogInfo(this,   includeIds)} on {GetAnchorLogInfo(this,   includeIds)}{GetGrabberLogInfo(this, includeIds)}";
                case UxrManipulationEventType.Remove:  return $"Removing {GetGrabbableObjectLogInfo(this,  includeIds)} from {GetAnchorLogInfo(this, includeIds)}{GetGrabberLogInfo(this, includeIds)}";
            }

            return "Unknown event";
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the log string describing the grabber.
        /// </summary>
        /// <param name="e">Event with the grabber to log</param>
        /// <param name="includeIds">Whether to include information of the component unique IDs</param>
        /// <returns>Log string</returns>
        private static string GetGrabberLogInfo(UxrManipulationEventArgs e, bool includeIds)
        {
            string id = e.Grabber != null && includeIds ? $" (id {e.Grabber.UniqueId})" : string.Empty;
            return e.Grabber != null ? $" using {e.Grabber}{id}" : string.Empty;
        }

        /// <summary>
        ///     Gets the log string describing the grabber.
        /// </summary>
        /// <param name="e">Event with the grabber to log</param>
        /// <param name="includeIds">Whether to include information of the component unique IDs</param>
        /// <returns>Log string</returns>
        private static string GetAnchorLogInfo(UxrManipulationEventArgs e, bool includeIds)
        {
            string id = e.GrabbableAnchor != null && includeIds ? $" (id {e.GrabbableAnchor.UniqueId})" : string.Empty;
            return e.GrabbableAnchor != null ? $"{e.GrabbableAnchor.name}{id}" : string.Empty;
        }

        /// <summary>
        ///     Gets the log string describing the grabbable object.
        /// </summary>
        /// <param name="e">Event with the grabbable object to log</param>
        /// <param name="includeIds">Whether to include information of the component unique IDs</param>
        /// <returns>Log string</returns>
        private static string GetGrabbableObjectLogInfo(UxrManipulationEventArgs e, bool includeIds)
        {
            string id            = e.GrabbableObject != null && includeIds ? $" (id {e.GrabbableObject.UniqueId})" : string.Empty;
            string grabPointInfo = e.GrabbableObject != null && e.GrabbableObject.GrabPointCount > 1 && e.GrabPointIndex >= 0 ? $" (grab point {e.GrabPointIndex})" : string.Empty;
            return e.GrabbableObject != null ? $"{e.GrabbableObject.name}{id}{grabPointInfo}" : string.Empty;
        }

        #endregion

        #region Private Types & Data

        private UxrManipulationEventType _eventType;
        private UxrGrabbableObject       _grabbableObject;
        private UxrGrabbableObjectAnchor _grabbableAnchor;
        private UxrGrabber               _grabber;
        private int                      _grabPointIndex;
        private bool                     _isMultiHands;
        private bool                     _isSwitchHands;
        private Vector3                  _releaseVelocity;
        private Vector3                  _releaseAngularVelocity;
        private UxrPlacementOptions      _placementOptions;
        private Vector3                  _grabberLocalObjectPosition;
        private Quaternion               _grabberLocalObjectRotation;
        private Vector3                  _grabberLocalSnapPosition;
        private Quaternion               _grabberLocalSnapRotation;
        private Vector3                  _releaseStartPosition;
        private Quaternion               _releaseStartRotation;

        #endregion
    }
}