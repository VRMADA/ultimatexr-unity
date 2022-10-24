// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectAnchor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" />, will enable <see cref="UxrGrabbableObject" /> objects to be
    ///     placed on it.
    ///     Some of the main features of grabbable object anchors are:
    ///     <list type="bullet">
    ///         <item>
    ///             Placement mechanics are handled automatically by the <see cref="UxrGrabManager" />. There is no special
    ///             requirement to set it up in a scene, the grab manager will be available as soon as it is required.
    ///         </item>
    ///         <item>
    ///             Compatible tags allow to model which objects can be placed on the anchor. If the list is empty, the
    ///             anchor is compatible with all other <see cref="UxrGrabbableObject" /> that do not have a tag.
    ///         </item>
    ///         <item>
    ///             Events such as <see cref="Placed" /> and <see cref="Removed" /> allow to write logic when a user interacts
    ///             with the anchor. Each one has pre and post events.
    ///         </item>
    ///         <item>
    ///             <see cref="ActivateOnCompatibleNear" />, <see cref="ActivateOnCompatibleNotNear" />,
    ///             <see cref="ActivateOnHandNearAndGrabbable" />, <see cref="ActivateOnPlaced" /> and
    ///             <see cref="ActivateOnEmpty" /> can be used to activate/deactivate objects on manipulation events. They can
    ///             be assigned during edit-time using the inspector and also at runtime.
    ///         </item>
    ///         <item>
    ///             <see cref="AddPlacingValidator" /> and <see cref="RemovePlacingValidator" /> can be used to model complex
    ///             compatibility behaviour that changes at runtime.
    ///         </item>
    ///     </list>
    /// </summary>
    public class UxrGrabbableObjectAnchor : UxrComponent<UxrGrabbableObjectAnchor>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private List<string> _compatibleTags        = new List<string>();
        [SerializeField] private float        _maxPlaceDistance      = 0.1f;
        [SerializeField] private bool         _alignTransformUseSelf = true;
        [SerializeField] private Transform    _alignTransform;
        [SerializeField] private bool         _dropProximityTransformUseSelf = true;
        [SerializeField] private Transform    _dropProximityTransform;
        [SerializeField] private GameObject   _activateOnCompatibleNear;
        [SerializeField] private GameObject   _activateOnCompatibleNotNear;
        [SerializeField] private GameObject   _activateOnHandNearAndGrabbable;
        [SerializeField] private GameObject   _activateOnPlaced;
        [SerializeField] private GameObject   _activateOnEmpty;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called right before an object is placed on the anchor.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Placing;

        /// <summary>
        ///     Event called right after an object was placed on the anchor.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Placed;

        /// <summary>
        ///     Event called right before the currently placed object is removed from the anchor.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Removing;

        /// <summary>
        ///     Event called right after the currently placed object is removed from the anchor.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> Removed;

        /// <summary>
        ///     Event called right after an object that was placed on the anchor ended its smooth placing transition.
        /// </summary>
        public event EventHandler<UxrManipulationEventArgs> SmoothPlaceTransitionEnded;

        /// <summary>
        ///     Gets the <see cref="Transform" /> that will be used to snap the <see cref="UxrGrabbableObject" /> placed on it.
        /// </summary>
        public Transform AlignTransform
        {
            get
            {
                if (_alignTransform == null || _alignTransformUseSelf)
                {
                    return transform;
                }

                return _alignTransform;
            }
        }

        /// <summary>
        ///     Gets the <see cref="Transform" /> that will be used to compute the distance to <see cref="UxrGrabbableObject" />
        ///     that can be placed on it, in order to determine if they are close enough.
        /// </summary>
        public Transform DropProximityTransform
        {
            get
            {
                if (_dropProximityTransform == null || _dropProximityTransformUseSelf)
                {
                    return transform;
                }

                return _dropProximityTransform;
            }
        }

        /// <summary>
        ///     Gets the <see cref="UxrGrabbableObject" /> that is currently placed on the anchor.
        /// </summary>
        public UxrGrabbableObject CurrentPlacedObject { get; internal set; }

        /// <summary>
        ///     Gets or sets the maximum distance from which an object that is released will be placed on the anchor.
        /// </summary>
        public float MaxPlaceDistance
        {
            get => _maxPlaceDistance;
            set => _maxPlaceDistance = value;
        }

        /// <summary>
        ///     Gets or sets the object that will be enabled or disabled depending on if there is a grabbed compatible
        ///     <see cref="UxrGrabbableObject" /> close enough to be placed on it.
        /// </summary>
        public GameObject ActivateOnCompatibleNear
        {
            get => _activateOnCompatibleNear;
            set => _activateOnCompatibleNear = value;
        }

        /// <summary>
        ///     Gets or sets the object that will be enabled or disabled depending on if there isn't a grabbed compatible
        ///     <see cref="UxrGrabbableObject" /> close enough to be placed on it.
        /// </summary>
        public GameObject ActivateOnCompatibleNotNear
        {
            get => _activateOnCompatibleNotNear;
            set => _activateOnCompatibleNotNear = value;
        }

        /// <summary>
        ///     Gets or sets the object that will be enabled or disabled depending on if there is a
        ///     <see cref="UxrGrabbableObject" /> currently placed on the anchor and a <see cref="UxrGrabber" /> close enough to
        ///     grab it.
        /// </summary>
        public GameObject ActivateOnHandNearAndGrabbable
        {
            get => _activateOnHandNearAndGrabbable;
            set => _activateOnHandNearAndGrabbable = value;
        }

        /// <summary>
        ///     Gets or sets the object that will be enabled or disabled depending on if there is a
        ///     <see cref="UxrGrabbableObject" /> currently placed on the anchor.
        /// </summary>
        public GameObject ActivateOnPlaced
        {
            get => _activateOnPlaced;
            set => _activateOnPlaced = value;
        }

        /// <summary>
        ///     Gets or sets the object that will be enabled or disabled depending on if there isn't a
        ///     <see cref="UxrGrabbableObject" /> currently placed on the anchor.
        /// </summary>
        public GameObject ActivateOnEmpty
        {
            get => _activateOnEmpty;
            set => _activateOnEmpty = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds compatible tags to the list of compatible tags that control which objects can be placed on the anchor.
        /// </summary>
        /// <param name="tags">Tags to add</param>
        /// <exception cref="ArgumentNullException">tags is null</exception>
        public void AddCompatibleTags(params string[] tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            _compatibleTags.AddRange(tags);
        }

        /// <summary>
        ///     Removes compatible tags from the list of compatible tags that control which objects can be placed on the anchor.
        /// </summary>
        /// <param name="tags">Tags to remove</param>
        /// <exception cref="ArgumentNullException">tags is null</exception>
        public void RemoveCompatibleTags(params string[] tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            foreach (string tag in tags)
            {
                _compatibleTags.Remove(tag);
            }
        }

        /// <summary>
        ///     Removes the currently placed object, if there is any, from the anchor.
        /// </summary>
        /// <param name="propagateEvents">Whether the call should generate any events or not</param>
        public void RemoveObject(bool propagateEvents)
        {
            if (CurrentPlacedObject != null)
            {
                UxrGrabManager.Instance.RemoveObjectFromAnchor(CurrentPlacedObject, propagateEvents);
            }
        }

        /// <summary>
        ///     Adds a placing validator to the internal list of validators. Placing validators are functions that are used in
        ///     addition to compatibility tags in order to determine if a <see cref="UxrGrabbableObject" /> can be placed on the
        ///     anchor.
        ///     An object can be placed on an anchor if the tag is compatible and if it is allowed by all of the placing
        ///     validators.
        /// </summary>
        /// <param name="validator">
        ///     New placing validator function to add. It takes an <see cref="UxrGrabbableObject" /> as input
        ///     and returns a boolean telling whether it can be placed or not
        /// </param>
        /// <exception cref="ArgumentNullException">The validator function is null</exception>
        public void AddPlacingValidator(Func<UxrGrabbableObject, bool> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _placingValidators.Add(validator);
        }

        /// <summary>
        ///     Removes a placing validator added using <see cref="AddPlacingValidator" />.
        /// </summary>
        /// <param name="validator">Validator to remove</param>
        /// <exception cref="ArgumentNullException">the validator function is null</exception>
        public void RemovePlacingValidator(Func<UxrGrabbableObject, bool> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _placingValidators.Remove(validator);
        }

        /// <summary>
        ///     Checks whether the given <see cref="UxrGrabbableObject" /> is compatible with the anchor, which means that it can
        ///     potentially be placed on it if there is no other object placed.
        /// </summary>
        /// <param name="grabbableObject">Object to check</param>
        /// <returns>Whether the object is compatible with the anchor</returns>
        public bool IsCompatibleObject(UxrGrabbableObject grabbableObject)
        {
            return grabbableObject != null && _placingValidators.All(v => v(grabbableObject)) && IsCompatibleObjectTag(grabbableObject.Tag);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrGrabManager.Instance.Poke();

            if (_activateOnCompatibleNear != null)
            {
                _activateOnCompatibleNear.SetActive(false);
            }

            if (_activateOnCompatibleNotNear != null)
            {
                _activateOnCompatibleNotNear.SetActive(false);
            }

            if (_activateOnHandNearAndGrabbable != null)
            {
                _activateOnHandNearAndGrabbable.SetActive(false);
            }
        }

        /// <summary>
        ///     Removes the validators.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _placingValidators.Clear();
        }

        /// <summary>
        ///     Performs additional initialization.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (_activateOnPlaced != null)
            {
                _activateOnPlaced.SetActive(CurrentPlacedObject != null);
            }

            if (_activateOnEmpty != null)
            {
                _activateOnEmpty.SetActive(CurrentPlacedObject == null);
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="Placing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaisePlacingEvent(UxrManipulationEventArgs e)
        {
            Placing?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Placed" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaisePlacedEvent(UxrManipulationEventArgs e)
        {
            Placed?.Invoke(this, e);
            _smoothPlaceEventArgs = e;
        }

        /// <summary>
        ///     Event trigger for <see cref="Removing" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseRemovingEvent(UxrManipulationEventArgs e)
        {
            Removing?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="Removed" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseRemovedEvent(UxrManipulationEventArgs e)
        {
            Removed?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="SmoothPlaceTransitionEnded" />.
        /// </summary>
        internal void RaiseSmoothTransitionPlaceEnded()
        {
            SmoothPlaceTransitionEnded?.Invoke(this, _smoothPlaceEventArgs);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checking whether the given tag is compatible with the anchor.
        /// </summary>
        /// <param name="otherTag">Tag to check whether it is compatible</param>
        /// <returns>Whether the tag is compatible</returns>
        private bool IsCompatibleObjectTag(string otherTag)
        {
            if (_compatibleTags == null || _compatibleTags.Count == 0)
            {
                return string.IsNullOrEmpty(otherTag);
            }

            return _compatibleTags.Contains(otherTag);
        }

        #endregion

        #region Private Types & Data

        private readonly List<Func<UxrGrabbableObject, bool>> _placingValidators = new List<Func<UxrGrabbableObject, bool>>();
        private          UxrManipulationEventArgs             _smoothPlaceEventArgs;

        #endregion
    }
}