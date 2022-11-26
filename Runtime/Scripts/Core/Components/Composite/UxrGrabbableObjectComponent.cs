// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Manipulation;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Components.Composite
{
    /// <summary>
    ///     Generic base class for components belonging to an object that also has a <see cref="UxrGrabbableObject" />
    ///     or in any of its parents. It allows to leverage some of the work related to accessing the
    ///     <see cref="UxrGrabbableObject" /> component and processing the events without the need to subscribe or
    ///     unsubscribe to them. Instead, events can be processed by overriding the different event triggers
    ///     (OnXXX methods).
    ///     <para>
    ///         The component has also all the benefits derived from <see cref="UxrComponent" />.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public abstract class UxrGrabbableObjectComponent<T> : UxrComponent<T> where T : UxrGrabbableObjectComponent<T>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the grabbable object is currently being grabbed.
        /// </summary>
        public bool IsBeingGrabbed => GrabbableObject && GrabbableObject.IsBeingGrabbed;

        /// <summary>
        ///     Gets the grabbable object component.
        /// </summary>
        public UxrGrabbableObject GrabbableObject
        {
            get
            {
                if (_grabbableObject == null)
                {
                    _grabbableObject = this.SafeGetComponentInParent<UxrGrabbableObject>();
                }

                return _grabbableObject;
            }
            private set => _grabbableObject = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Caches the grabbable object component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            GrabbableObject = GetComponentInParent<UxrGrabbableObject>();

            if (GrabbableObject == null && IsGrabbableObjectRequired)
            {
                Debug.LogError($"Component {nameof(T)} requires a {nameof(UxrGrabbableObject)} component in the same object or any of its parents.");
            }
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (GrabbableObject)
            {
                GrabbableObject.Grabbing            += GrabbableObject_Grabbing;
                GrabbableObject.Grabbed             += GrabbableObject_Grabbed;
                GrabbableObject.Releasing           += GrabbableObject_Releasing;
                GrabbableObject.Released            += GrabbableObject_Released;
                GrabbableObject.Placing             += GrabbableObject_Placing;
                GrabbableObject.Placed              += GrabbableObject_Placed;
                GrabbableObject.ConstraintsApplying += GrabbableObject_ConstraintsApplying;
                GrabbableObject.ConstraintsApplied  += GrabbableObject_ConstraintsApplied;
                GrabbableObject.ConstraintsFinished += GrabbableObject_ConstraintsFinished;
            }
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (GrabbableObject)
            {
                GrabbableObject.Grabbing            -= GrabbableObject_Grabbing;
                GrabbableObject.Grabbed             -= GrabbableObject_Grabbed;
                GrabbableObject.Releasing           -= GrabbableObject_Releasing;
                GrabbableObject.Released            -= GrabbableObject_Released;
                GrabbableObject.Placing             -= GrabbableObject_Placing;
                GrabbableObject.Placed              -= GrabbableObject_Placed;
                GrabbableObject.ConstraintsApplying -= GrabbableObject_ConstraintsApplying;
                GrabbableObject.ConstraintsApplied  -= GrabbableObject_ConstraintsApplied;
                GrabbableObject.ConstraintsFinished -= GrabbableObject_ConstraintsFinished;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Grabbing" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Grabbing(object sender, UxrManipulationEventArgs e)
        {
            OnObjectGrabbing(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Grabbed" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Grabbed(object sender, UxrManipulationEventArgs e)
        {
            OnObjectGrabbed(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Releasing" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Releasing(object sender, UxrManipulationEventArgs e)
        {
            OnObjectReleasing(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Released" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Released(object sender, UxrManipulationEventArgs e)
        {
            OnObjectReleased(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Placing" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Placing(object sender, UxrManipulationEventArgs e)
        {
            OnObjectPlacing(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.Placed" /> event. It will call the overridable event
        ///     trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_Placed(object sender, UxrManipulationEventArgs e)
        {
            OnObjectPlaced(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.ConstraintsApplying" /> event. It will call the
        ///     overridable event trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_ConstraintsApplying(object sender, UxrApplyConstraintsEventArgs e)
        {
            OnObjectConstraintsApplying(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.ConstraintsApplied" /> event. It will call the
        ///     overridable event trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_ConstraintsApplied(object sender, UxrApplyConstraintsEventArgs e)
        {
            OnObjectConstraintsApplied(e);
        }

        /// <summary>
        ///     Event handling method for the <see cref="UxrGrabbableObject.ConstraintsFinished" /> event. It will call the
        ///     overridable event trigger so that child classes don't need to subscribe to the event and can override the method instead.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void GrabbableObject_ConstraintsFinished(object sender, UxrApplyConstraintsEventArgs e)
        {
            OnObjectConstraintsFinished(e);
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Grabbing" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectGrabbing(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Grabbed" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Releasing" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectReleasing(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Released" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectReleased(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Placing" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectPlacing(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.Placed" /> event that can be used to
        ///     handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectPlaced(UxrManipulationEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.ConstraintsApplying" /> event that can be
        ///     used to handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectConstraintsApplying(UxrApplyConstraintsEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.ConstraintsApplied" /> event that can be
        ///     used to handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectConstraintsApplied(UxrApplyConstraintsEventArgs e)
        {
        }

        /// <summary>
        ///     Overridable event trigger method for the <see cref="UxrGrabbableObject.ConstraintsFinished" /> event that can be
        ///     used to handle it without requiring to subscribe/unsubscribe.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnObjectConstraintsFinished(UxrApplyConstraintsEventArgs e)
        {
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets whether the grabbable object component is required or it's not. By default it is required but it can be
        ///     overriden in child classes so that it is optional.
        /// </summary>
        protected virtual bool IsGrabbableObjectRequired => true;

        #endregion

        #region Private Types & Data

        private UxrGrabbableObject _grabbableObject;

        #endregion
    }
}