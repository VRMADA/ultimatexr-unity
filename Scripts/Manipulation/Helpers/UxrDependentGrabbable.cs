// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDependentGrabbable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Component that allows an object be grabbed only if another object is being grabbed. For instance, it can
    ///     be added to a grenade pin to make sure the pin is never grabbed unless the grenade is being grabbed too.
    ///     Otherwise the pin could be removed by mistake when trying to grab the grenade.
    /// </summary>
    public class UxrDependentGrabbable : UxrGrabbableObjectComponent<UxrDependentGrabbable>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrGrabbableObject _dependentOn;
        [SerializeField] private bool               _onlyOnce;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the grabbable object the component depends on.
        /// </summary>
        public UxrGrabbableObject DependentFrom
        {
            get => _dependentOn;
            set => _dependentOn = value;
        }

        /// <summary>
        ///     Whether to stop toggling the enabled state once the dependent object was grabbed. For instance, a grenade pin
        ///     should remain grabbable once it has been removed from the grenade, no matter if the grenade is being grabbed or
        ///     not at that point.
        /// </summary>
        public bool OnlyOnce
        {
            get => _onlyOnce;
            set => _onlyOnce = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the grabbable object state.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            GrabbableObject.enabled = false;
        }

        /// <summary>
        ///     Updates the grabbable object state.
        /// </summary>
        private void Update()
        {
            if (GrabbableObject && DependentFrom && _check)
            {
                GrabbableObject.enabled = UxrGrabManager.Instance.IsBeingGrabbed(DependentFrom);

                if (GrabbableObject.enabled && OnlyOnce)
                {
                    _check = false;
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called whenever the object was grabbed.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            _check = false;
        }

        #endregion

        #region Private Types & Data

        private bool _check = true;

        #endregion
    }
}