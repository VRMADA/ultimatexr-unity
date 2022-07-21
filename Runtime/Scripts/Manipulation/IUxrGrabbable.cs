// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrGrabbable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Interface for all objects that can be grabbed/manipulated using the <see cref="UxrGrabManager" />.
    /// </summary>
    public interface IUxrGrabbable
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called when the object is about to be grabbed.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be grabbed.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Target where the object is currently placed. Null
        ///             if it isn't on an anchor.
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
        public event EventHandler<UxrManipulationEventArgs> Grabbing;

        /// <summary>
        ///     Event called right after the object was grabbed. The grab event parameters use the same values as
        ///     <see cref="Grabbing" />.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Grabbed;

        /// <summary>
        ///     Event called when the object is about to be released. An object is released when the last grip is released and
        ///     there is no compatible <see cref="UxrGrabbableObjectAnchor" /> near enough to place it on.
        ///     The following properties from <see cref="UxrManipulationEventArgs" /> will contain meaningful data:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableObject" />: Object that is about to be released.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabbableAnchor" />: Anchor where the object was originally grabbed
        ///             from. Null if it wasn't on a target.
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
        public event EventHandler<UxrManipulationEventArgs> Releasing;

        /// <summary>
        ///     Event called right after the object was released. An object is released when the last grip is released and there is
        ///     no compatible <see cref="UxrGrabbableObjectAnchor" /> near enough to place it on.
        ///     The grab event parameters use the same values as <see cref="Releasing" />.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Released;

        /// <summary>
        ///     Event called when the object is about to be placed. An object is placed when the last grip is released and there is
        ///     a compatible <see cref="UxrGrabbableObjectAnchor" /> near enough to place it on.
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
        ///             This can be null if the object is removed through code using
        ///             <see cref="UxrGrabManager.RemoveObjectFromAnchor" />,
        ///             <see cref="UxrGrabbableObject.RemoveFromAnchor" /> or <see cref="UxrGrabbableObjectAnchor.RemoveObject" />>
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.GrabPointIndex" />: Only if the object is being removed by grabbing it:
        ///             Grab point index of the object that is about to be grabbed by the <see cref="UxrGrabber" />.
        ///         </item>
        ///     </list>
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Placing;

        /// <summary>
        ///     Event called right after the object was placed. An object is placed when the last grip is released and there is a
        ///     compatible <see cref="UxrGrabbableObjectAnchor" /> near enough to place it on.
        ///     The grab event parameters use the same values as <see cref="Placed" />.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Placed;

        /// <summary>
        ///     Gets the associated <see cref="GameObject" />. Since all components that implement the interface will be assigned
        ///     to GameObjects, this allows to access them using the interface.
        ///     It doesn't follow the property PascalCase naming to make it compatible with Unity.
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        ///     Gets the associated <see cref="Transform" /> component. Since all components that implement the interface will be
        ///     assigned to GameObjects, this allows to access their transform using the interface.
        ///     It doesn't follow the property PascalCase naming to make it compatible with Unity.
        /// </summary>
        public Transform transform { get; }

        /// <summary>
        ///     Gets whether the object is being grabbed.
        /// </summary>
        public bool IsBeingGrabbed { get; }

        /// <summary>
        ///     Gets or sets whether the object can be grabbed.
        /// </summary>
        public bool IsGrabbable { get; set; }

        /// <summary>
        ///     Gets or sets whether the rigidbody that drives the object (if any) is kinematic.
        /// </summary>
        public bool IsKinematic { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Resets the object to its initial position/rotation and state. If the object is currently being grabbed, it will be
        ///     released.
        /// </summary>
        /// <param name="propagateEvents">Should <see cref="UxrManipulationEventArgs" /> events be generated?</param>
        public void ResetPositionAndState(bool propagateEvents);

        /// <summary>
        ///     Releases the object from all its grabs if there are any.
        /// </summary>
        /// <param name="propagateEvents">Should <see cref="UxrManipulationEventArgs" /> events be generated?</param>
        public void ReleaseGrabs(bool propagateEvents);

        #endregion
    }
}