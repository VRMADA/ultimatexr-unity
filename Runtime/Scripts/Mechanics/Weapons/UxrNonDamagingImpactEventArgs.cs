// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNonDamagingImpactEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Event parameters for projectile impacts that do not cause any damage to actors, such as impacts on the
    ///     scenario or other elements.
    /// </summary>
    public class UxrNonDamagingImpactEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the actor that fired the shot.
        /// </summary>
        public UxrActor WeaponOwner { get; }

        /// <summary>
        ///     The projectile source.
        /// </summary>
        public UxrProjectileSource ProjectileSource { get; }

        /// <summary>
        ///     The raycast that detected the hit.
        /// </summary>
        public RaycastHit RaycastHit { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="weaponOwner">Owner of the weapon that fired the shot</param>
        /// <param name="projectileSource">Projectile source</param>
        /// <param name="raycastHit">Raycast that detected the hit</param>
        public UxrNonDamagingImpactEventArgs(UxrActor weaponOwner, UxrProjectileSource projectileSource, RaycastHit raycastHit)
        {
            WeaponOwner      = weaponOwner;
            ProjectileSource = projectileSource;
            RaycastHit       = raycastHit;
        }

        #endregion
    }
}