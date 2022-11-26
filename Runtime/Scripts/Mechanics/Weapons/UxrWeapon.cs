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
    public abstract class UxrWeapon : UxrGrabbableObjectComponent<UxrWeapon>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets who is in possession of the weapon, to attribute the inflicted damage to.
        /// </summary>
        public UxrActor Owner { get; protected set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            Owner = GetComponentInParent<UxrActor>();
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

            if (e.IsOwnershipChanged && UxrGrabManager.Instance.GetGrabbingHand(e.GrabbableObject, e.GrabPointIndex, out UxrGrabber grabber))
            {
                Owner = grabber.Avatar.GetComponentInChildren<UxrActor>();
            }
        }

        #endregion

        #region Protected Overrides UxrGrabbableObjectComponent<UxrWeapon>

        /// <inheritdoc />
        protected override bool IsGrabbableObjectRequired => false;

        #endregion
    }
}