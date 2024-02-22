// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeapon.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Base class for weapons. Weapons are used by <see cref="UxrActor" /> components to inflict damage to other actor
    ///     components.
    /// </summary>
    public abstract partial class UxrWeapon : UxrGrabbableObjectComponent<UxrWeapon>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets who is in possession of the weapon, to attribute the inflicted damage to.
        /// </summary>
        public UxrActor Owner
        {
            get => _owner;
            protected set => _owner = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrActor.GlobalUnregistering += UxrActor_GlobalUnregistering;

            Owner = GetComponentInParent<UxrActor>();
        }

        /// <summary>
        ///     Called when it's going to be destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            UxrActor.GlobalUnregistering -= UxrActor_GlobalUnregistering;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever an actor is about to be destroyed.
        /// </summary>
        /// <param name="actor"></param>
        private void UxrActor_GlobalUnregistering(UxrActor actor)
        {
            if (Owner == actor)
            {
                Owner = null;
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the object was grabbed. It is used to set the weapon owner.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            if (e.IsGrabbedStateChanged && UxrGrabManager.Instance.GetGrabbingHand(e.GrabbableObject, e.GrabPointIndex, out UxrGrabber grabber))
            {
                Owner = grabber.Avatar.GetComponentInChildren<UxrActor>();
            }
        }

        #endregion

        #region Protected Overrides UxrGrabbableObjectComponent<UxrWeapon>

        /// <inheritdoc />
        protected override bool IsGrabbableObjectRequired => false;

        #endregion

        #region Private Types & Data

        private UxrActor _owner;

        #endregion
    }
}