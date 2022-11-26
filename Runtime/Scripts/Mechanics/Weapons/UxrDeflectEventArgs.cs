// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDeflectEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Projectile deflection event parameters.
    /// </summary>
    public class UxrDeflectEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the projectile source.
        /// </summary>
        public UxrProjectileSource ProjectileSource { get; }

        /// <summary>
        ///     Gets the raycast that was used to detect the collision.
        /// </summary>
        public RaycastHit RaycastHit { get; }

        /// <summary>
        ///     Gets the new projectile direction after being deflected.
        /// </summary>
        public Vector3 NewDirection { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="projectileSource">The projectile source</param>
        /// <param name="raycastHit">The raycast used to detect projectile collision</param>
        /// <param name="newDirection">The new, deflected, projectile direction</param>
        public UxrDeflectEventArgs(UxrProjectileSource projectileSource, RaycastHit raycastHit, Vector3 newDirection)
        {
            ProjectileSource = projectileSource;
            RaycastHit       = raycastHit;
            NewDirection     = newDirection;
        }

        #endregion
    }
}