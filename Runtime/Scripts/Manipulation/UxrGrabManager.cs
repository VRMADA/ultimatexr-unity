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
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
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
    public partial class UxrGrabManager : UxrSingleton<UxrGrabManager>
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
        ///             <see cref="UxrManipulationEventArgs.IsMultiHands" />: true if it is already being grabbed and
        ///             will be grabbed with one more hand after. False if no hand is currently grabbing it.
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
        ///             that will keep holding it. False if no other hand is currently grabbing it.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrManipulationEventArgs.IsSwitchHands" />: True if it was released because another
        ///             <see cref="UxrGrabber" /> grabbed it, false otherwise. if
        ///             <see cref="UxrManipulationEventArgs.IsMultiHands" /> is
        ///             true then <see cref="UxrManipulationEventArgs.IsSwitchHands" /> will tell if it was released by all hands
        ///             (false) or if it was just released by one hand and the other(s) still keep the grab (true).
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
        ///     Gets or sets the manipulation features that are used when the manager is updated.
        /// </summary>
        public UxrManipulationFeatures Features { get; set; } = UxrManipulationFeatures.All;

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Updates the grab manager to the current frame.
        /// </summary>
        internal void UpdateManager()
        {
            // Initializes the variables for a manipulation frame update computation.
            InitializeManipulationFrame();

            // Updates the grabbable objects based on manipulation logic
            UpdateManipulation();

            if (Features.HasFlag(UxrManipulationFeatures.Affordances))
            {
                // Updates visual feedback states (objects that can be grabbed, anchors where a grabbed object can be placed on, etc.)
                UpdateAffordances();
            }

            // Perform operations that need to be done at the end of the updating process.
            FinalizeManipulationFrame();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the manager and subscribes to global events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

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

            UxrAvatar.GlobalAvatarMoved += UxrAvatar_GlobalAvatarMoved;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrAvatar.GlobalAvatarMoved -= UxrAvatar_GlobalAvatarMoved;
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
            if (_currentManipulations.ContainsKey(grabbableObject))
            {
                _currentManipulations.Remove(grabbableObject);
            }
        }

        /// <summary>
        ///     Called when an avatar was moved due to regular movement or teleportation. It is used to process the objects that
        ///     are being grabbed to the avatar to keep it in the same relative position/orientation.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_GlobalAvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            UxrAvatar avatar = sender as UxrAvatar;

            if (avatar == null || avatar.AvatarMode == UxrAvatarMode.UpdateExternally)
            {
                return;
            }

            // Create anonymous pairs of grabbable objects and their grabs that are affected by the avatar position change
            var dependencies = _currentManipulations.Where(pair => pair.Value.Grabbers.Any(g => g.Avatar == avatar)).Select(pair => new { GrabbableObject = pair.Key, Grabs = pair.Value.Grabs.Where(g => g.Grabber.Avatar == avatar) });

            foreach (var dependency in dependencies)
            {
                UxrGrabbableObject grabbableObject = dependency.GrabbableObject;

                // Move grabbed objects without being parented to avatar to new position/orientation to avoid rubber-band effects
                if (!grabbableObject.transform.HasParent(avatar.transform))
                {
                    UxrGrabbableObject grabbableRoot = grabbableObject.AllParents.LastOrDefault() ?? grabbableObject;

                    // Use this handy method to make the grabbable object keep the relative positioning to the avatar 
                    e.ReorientRelativeToAvatar(grabbableRoot.transform);

                    grabbableRoot.LocalPositionBeforeUpdate = grabbableRoot.transform.localPosition;
                    grabbableRoot.LocalRotationBeforeUpdate = grabbableRoot.transform.localRotation;
                    ConstrainTransform(grabbableRoot);
                    KeepGripsInPlace(grabbableRoot);

                    foreach (UxrGrabbableObject grabbableChild in grabbableRoot.AllChildren)
                    {
                        grabbableChild.LocalPositionBeforeUpdate = grabbableChild.transform.localPosition;
                        grabbableChild.LocalRotationBeforeUpdate = grabbableChild.transform.localRotation;
                        ConstrainTransform(grabbableChild);
                        KeepGripsInPlace(grabbableChild);
                    }
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
            if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.ManipulationModule} Trying to grab using {e.Grabber}.");
            }

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
            if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.ManipulationModule} {e.ToString(UxrGlobalSettings.Instance.LogLevelManipulation == UxrLogLevel.Verbose)}");
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
            }

            if (e.GrabbableObject)
            {
                e.GrabbableObject.UpdateGrabbableDependencies();
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectReleasing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectReleasing(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.ManipulationModule} {e.ToString(UxrGlobalSettings.Instance.LogLevelManipulation == UxrLogLevel.Verbose)}");
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
            }

            if (e.GrabbableObject)
            {
                e.GrabbableObject.UpdateGrabbableDependencies();
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectPlacing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectPlacing(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.ManipulationModule} {e.ToString(UxrGlobalSettings.Instance.LogLevelManipulation == UxrLogLevel.Verbose)}");
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
            }

            if (e.GrabbableObject)
            {
                e.GrabbableObject.UpdateGrabbableDependencies();
            }
        }

        /// <summary>
        ///     Event trigger for <see cref="ObjectRemoving" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <param name="propagateEvent">Whether to propagate the event</param>
        private void OnObjectRemoving(UxrManipulationEventArgs e, bool propagateEvent)
        {
            if (UxrGlobalSettings.Instance.LogLevelManipulation >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.ManipulationModule} {e.ToString(UxrGlobalSettings.Instance.LogLevelManipulation == UxrLogLevel.Verbose)}");
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
        ///     Initializes the variables for a manipulation frame update computation.
        /// </summary>
        private void InitializeManipulationFrame()
        {
            // Store the unprocessed grabber positions for this update.

            foreach (UxrGrabber grabber in UxrGrabber.AllComponents)
            {
                grabber.UnprocessedGrabberPosition = grabber.transform.position;
                grabber.UnprocessedGrabberRotation = grabber.transform.rotation;
            }

            // Update grabbable object information

            void InitializeGrabbableData(UxrGrabbableObject grabbableObject)
            {
                grabbableObject.DirectLookAtChildProcessedCount = 0;
                grabbableObject.DirectLookAtChildGrabbedCount   = grabbableObject.DirectChildrenLookAts.Count(IsBeingGrabbed);
                grabbableObject.LocalPositionBeforeUpdate       = grabbableObject.transform.localPosition;
                grabbableObject.LocalRotationBeforeUpdate       = grabbableObject.transform.localRotation;
            }

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeManipulationInfo> manipulationInfoPair in _currentManipulations)
            {
                if (manipulationInfoPair.Key == null)
                {
                    continue;
                }
                
                UxrGrabbableObject grabbableParent = manipulationInfoPair.Key.GrabbableParent;

                InitializeGrabbableData(manipulationInfoPair.Key);

                if (grabbableParent != null)
                {
                    InitializeGrabbableData(grabbableParent);
                }

                foreach (UxrGrabbableObject child in manipulationInfoPair.Key.DirectChildrenLookAts)
                {
                    InitializeGrabbableData(child);
                }

                manipulationInfoPair.Value.LocalManipulationRotationPivot = Vector3.zero;
            }

            // Initialize some anchor variables for later

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
        }

        /// <summary>
        ///     Performs operations that require to be done at the end of the manipulation update pipeline.
        /// </summary>
        private void FinalizeManipulationFrame()
        {
            // Update grabbers

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                grabber.UpdateThrowPhysicsInfo();
                grabber.UpdateHandGrabberRenderer();
            }
        }

        /// <summary>
        ///     Updates visual feedback states (objects that can be grabbed, anchors where a grabbed object can be placed on,
        ///     etc.).
        /// </summary>
        private void UpdateAffordances()
        {
            // Look for grabbed objects that can be placed on anchors

            foreach (KeyValuePair<UxrGrabbableObject, RuntimeManipulationInfo> manipulationInfoPair in _currentManipulations)
            {
                UxrGrabbableObjectAnchor anchorTargetCandidate = null;
                float                    minDistance           = float.MaxValue;

                if (manipulationInfoPair.Key.UsesGrabbableParentDependency == false && manipulationInfoPair.Key.IsPlaceable)
                {
                    foreach (KeyValuePair<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> anchorPair in _grabbableObjectAnchors)
                    {
                        if (manipulationInfoPair.Key.CanBePlacedOnAnchor(anchorPair.Key, out float distance) && distance < minDistance)
                        {
                            anchorTargetCandidate = anchorPair.Key;
                            minDistance           = distance;
                        }
                    }
                }

                // Is there a compatible anchor if we would release it? store the grabber for later

                if (anchorTargetCandidate != null)
                {
                    _grabbableObjectAnchors[anchorTargetCandidate].HasCompatibleObjectNear = true;
                    _grabbableObjectAnchors[anchorTargetCandidate].FullGrabberNear         = manipulationInfoPair.Value.Grabs[0].Grabber;
                    _grabbableObjectAnchors[anchorTargetCandidate].LastFullGrabberNear     = manipulationInfoPair.Value.Grabs[0].Grabber;
                }
            }

            // Look for objects that can be grabbed to update feedback objects (blinks, labels...).
            // First pass: get closest candidate for each grabber.

            Dictionary<UxrGrabbableObject, List<int>> possibleGrabs = null;

            foreach (UxrGrabber grabber in UxrGrabber.EnabledComponents)
            {
                if (grabber.GrabbedObject == null)
                {
                    if (GetClosestGrabbableObject(grabber, out UxrGrabbableObject grabbableCandidate, out int grabPointCandidate) && !IsBeingGrabbed(grabbableCandidate, grabPointCandidate))
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
                        OnPlacedObjectRangeEntered(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.PlacedObjectRangeEntered, anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.LastValidGrabberNear, anchorPair.Value.LastValidGrabPointNear), true);

                        anchorPair.Value.LastValidGrabberNear   = null;
                        anchorPair.Value.LastValidGrabPointNear = -1;
                    }

                    if (anchorPair.Value.HasCompatibleObjectNear && !anchorPair.Value.HadCompatibleObjectNearLastFrame)
                    {
                        OnAnchorRangeEntered(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.AnchorRangeEntered, anchorPair.Value.FullGrabberNear.GrabbedObject, anchorPair.Key, anchorPair.Value.FullGrabberNear), true);
                    }

                    if (!anchorPair.Value.HasCompatibleObjectNear && anchorPair.Value.HadCompatibleObjectNearLastFrame)
                    {
                        OnAnchorRangeLeft(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.AnchorRangeLeft, anchorPair.Value.LastFullGrabberNear.GrabbedObject, anchorPair.Key, anchorPair.Value.LastFullGrabberNear), true);
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
                            OnPlacedObjectRangeEntered(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.PlacedObjectRangeEntered, anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.GrabberNear, anchorPair.Value.GrabPointNear), true);
                        }
                        else if (anchorPair.Value.LastValidGrabberNear != null)
                        {
                            OnPlacedObjectRangeLeft(UxrManipulationEventArgs.FromOther(UxrManipulationEventType.PlacedObjectRangeLeft, anchorPair.Key.CurrentPlacedObject, anchorPair.Key, anchorPair.Value.LastValidGrabberNear, anchorPair.Value.GrabPointNear), true);
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
        }

        #endregion

        #region Private Types & Data

        private          Dictionary<UxrGrabbableObject, RuntimeManipulationInfo>         _currentManipulations   = new Dictionary<UxrGrabbableObject, RuntimeManipulationInfo>();
        private readonly Dictionary<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo> _grabbableObjectAnchors = new Dictionary<UxrGrabbableObjectAnchor, GrabbableObjectAnchorInfo>();

        #endregion
    }
}