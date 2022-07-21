// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrProjectileDeflect.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Audio;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" /> with a collider, will allow to deflect shots coming from
    ///     <see cref="UxrProjectileSource" /> components.
    /// </summary>
    public class UxrProjectileDeflect : UxrGrabbableObjectComponent<UxrProjectileDeflect>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAudioSample _audioDeflect;
        [SerializeField] private UxrImpactDecal _decalOnReflect;
        [SerializeField] private float          _decalLife            = 5.0f;
        [SerializeField] private float          _decalFadeoutDuration = 1.0f;
        [SerializeField] private bool           _twoSidedDecal;
        [SerializeField] private float          _twoSidedDecalThickness    = 0.01f;
        [SerializeField] private LayerMask      _collideLayersAddOnReflect = 0;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called when a projectile got deflected after hitting the object.
        /// </summary>
        public event EventHandler<UxrDeflectEventArgs> ProjectileDeflected;

        /// <summary>
        ///     Gets the sound to play when a projectile was deflected.
        /// </summary>
        public UxrAudioSample AudioDeflect => _audioDeflect;

        /// <summary>
        ///     Gets the decal to instantiate when a projectile was deflected.
        /// </summary>
        public UxrImpactDecal DecalOnReflect => _decalOnReflect;

        /// <summary>
        ///     Gets the decal life in seconds after which it will fade out and be destroyed.
        /// </summary>
        public float DecalLife => _decalLife;

        /// <summary>
        ///     Gets the decal fadeout duration in seconds.
        /// </summary>
        public float DecalFadeoutDuration => _decalFadeoutDuration;

        /// <summary>
        ///     Gets whether the decal requires to generate another copy on the other side.
        /// </summary>
        public bool TwoSidedDecal => _twoSidedDecal;

        /// <summary>
        ///     Gets the object thickness in order to know how far the other side is to generate the copy on the backside of the
        ///     impact.
        /// </summary>
        public float TwoSidedDecalThickness => _twoSidedDecalThickness;

        /// <summary>
        ///     Optional layer mask to add to the collider after a projectile was deflected.
        /// </summary>
        public LayerMask CollideLayersAddOnReflect => _collideLayersAddOnReflect;

        /// <summary>
        ///     Gets the owner in case the deflection object is part of an avatar or can be grabbed.
        /// </summary>
        public UxrActor Owner { get; private set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            Owner = GetComponentInParent<UxrActor>();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the object was grabbed. Will change the <see cref="Owner" />.
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

        /// <summary>
        ///     Called when the object was released. Will reset the <see cref="Owner" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            base.OnObjectReleased(e);

            if (e.IsOwnershipChanged)
            {
                Owner = null;
            }
        }

        /// <summary>
        ///     Called when the object was released. Will reset the <see cref="Owner" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectPlaced(UxrManipulationEventArgs e)
        {
            base.OnObjectPlaced(e);

            if (e.IsOwnershipChanged && UxrGrabManager.Instance.GetGrabbingHand(e.GrabbableObject, e.GrabPointIndex, out UxrGrabber grabber))
            {
                Owner = null;
            }
        }

        /// <summary>
        ///     Raises the <see cref="ProjectileDeflected" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseProjectileDeflected(UxrDeflectEventArgs e)
        {
            ProjectileDeflected?.Invoke(this, e);
        }

        #endregion

        #region Protected Overrides UxrGrabbableObjectComponent<UxrProjectileDeflect>

        /// <summary>
        ///     The grabbable object is not required. When it is present it will be used to assign the <see cref="Owner" /> so that
        ///     the damage will be attributed to the actor instead of the original source.
        /// </summary>
        protected override bool IsGrabbableObjectRequired => false;

        #endregion
    }
}