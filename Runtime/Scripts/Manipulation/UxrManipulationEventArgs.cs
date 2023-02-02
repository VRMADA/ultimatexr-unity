// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
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
    public class UxrManipulationEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the grabbable object related to the event. Can be null if the event doesn't use this property. Check the event
        ///     documentation to see how the property is used.
        /// </summary>
        public UxrGrabbableObject GrabbableObject { get; }

        /// <summary>
        ///     Gets the grabbable object anchor related to the event. Can be null if the event doesn't use this property. Check
        ///     the event documentation to see how the property is used.
        /// </summary>
        public UxrGrabbableObjectAnchor GrabbableAnchor { get; }

        /// <summary>
        ///     Gets the grabber related to the event. Can be null if the event doesn't use this property. Check the event
        ///     documentation to see how the property is used.
        /// </summary>
        public UxrGrabber Grabber { get; }

        /// <summary>
        ///     Gets the grabbable object's grab point index related to the event. Can be meaningless if the event doesn't use this
        ///     property. Check the event documentation to see how the property is used.
        /// </summary>
        public int GrabPointIndex { get; }

        /// <summary>
        ///     Gets whether the manipulation used more than one hand. Can be meaningless if the event doesn't use this property.
        ///     Check the event documentation to see how the property is used.
        /// </summary>
        public bool IsMultiHands { get; }

        /// <summary>
        ///     Gets whether the event was the result of passing the object from one hand to the other. Can be meaningless if the
        ///     event doesn't use this property. Check the event documentation to see how the property is used.
        /// </summary>
        public bool IsSwitchHands { get; }

        /// <summary>
        ///     Gets whether the manipulation changed an object's ownership. This is if <see cref="IsMultiHands" /> and
        ///     <see cref="IsSwitchHands" /> are both false.
        ///     This is useful to filter events that should be processed only if an object switched from belonging to an avatar to
        ///     not belonging anymore or vice-versa, ignoring events where the object was already in the hands of an avatar and is
        ///     just switching hands or being grabbed with more than one hand.
        /// </summary>
        public bool IsOwnershipChanged => !IsMultiHands && !IsSwitchHands;

        /// <summary>
        ///     Gets the release velocity for release events.
        /// </summary>
        public Vector3 ReleaseVelocity { get; internal set; }

        /// <summary>
        ///     Gets the release angular velocity for release events.
        /// </summary>
        public Vector3 ReleaseAngularVelocity { get; internal set; }

        /// <summary>
        ///     Gets the placement flags in place events.
        /// </summary>
        public UxrPlacementOptions PlacementOptions { get; internal set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="grabbableAnchor">Grabbable object anchor</param>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabPointIndex">Grab point index</param>
        /// <param name="isMultiHands">Whether the event was a result of a manipulation with more than one hand</param>
        /// <param name="isSwitchHands">Whether the event was a result of passing the grabbable object from one hand to the other</param>
        public UxrManipulationEventArgs(UxrGrabbableObject       grabbableObject,
                                        UxrGrabbableObjectAnchor grabbableAnchor,
                                        UxrGrabber               grabber,
                                        int                      grabPointIndex = 0,
                                        bool                     isMultiHands   = false,
                                        bool                     isSwitchHands  = false)
        {
            GrabbableObject = grabbableObject;
            GrabbableAnchor = grabbableAnchor;
            Grabber         = grabber;
            GrabPointIndex  = grabPointIndex;
            IsMultiHands    = isMultiHands;
            IsSwitchHands   = isSwitchHands;
        }

        #endregion
    }
}