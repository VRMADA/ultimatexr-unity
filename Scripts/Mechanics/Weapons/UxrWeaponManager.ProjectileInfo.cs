// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeaponManager.ProjectileInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    public partial class UxrWeaponManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information of a projectile currently travelling through the world.
        /// </summary>
        private class ProjectileInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the <see cref="UxrActor" /> that shot the projectile, or deflected it. It will be used know who to attribute
            ///     the damage to.
            /// </summary>
            public UxrActor WeaponOwner { get; }

            /// <summary>
            ///     Gets the source that shot the projectile.
            /// </summary>
            public UxrProjectileSource ProjectileSource { get; }

            /// <summary>
            ///     Gets the shot descriptor that was used to shoot the projectile.
            /// </summary>
            public UxrShotDescriptor ShotDescriptor { get; }

            /// <summary>
            ///     Gets the layer mask, used to determine which objects the shot can collide with.
            /// </summary>
            public LayerMask ShotLayerMask { get; }

            /// <summary>
            ///     Gets the projectile GameObject instance.
            /// </summary>
            public GameObject Projectile { get; }

            /// <summary>
            ///     Gets the world-space point the projectile came from.
            /// </summary>
            public Vector3 ProjectileOrigin { get; }

            /// <summary>
            ///     Gets the current projectile speed in units/second.
            /// </summary>
            public float ProjectileSpeed { get; }

            /// <summary>
            ///     Gets or sets the projectile's position during the previous frame.
            /// </summary>
            public Vector3 ProjectileLastPosition { get; set; }

            /// <summary>
            ///     Gets or sets the currently travelled distance.
            /// </summary>
            public float ProjectileDistanceTravelled { get; set; }

            /// <summary>
            ///     Gets or sets the deflector that deflected the shot or null if there wasn't any.
            /// </summary>
            public UxrProjectileDeflect ProjectileDeflectSource { get; set; }

            /// <summary>
            ///     Gets or sets whether the current state is the first frame in the shot.
            /// </summary>
            public bool FirstFrame { get; set; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="weaponOwner">Weapon owner, that fired the shot</param>
            /// <param name="projectileSource">Projectile source component</param>
            /// <param name="shotDescriptor">Shot descriptor</param>
            /// <param name="position">World-space position where the shot started</param>
            /// <param name="orientation">
            ///     World space orientation where the shot started. The shot will travel in the z (forward)
            ///     position of these axes
            /// </param>
            public ProjectileInfo(UxrActor weaponOwner, UxrProjectileSource projectileSource, UxrShotDescriptor shotDescriptor, Vector3 position, Quaternion orientation)
            {
                WeaponOwner                 = weaponOwner;
                ProjectileSource            = projectileSource;
                ShotDescriptor              = shotDescriptor;
                ShotLayerMask               = shotDescriptor.CollisionLayerMask;
                Projectile                  = Instantiate(shotDescriptor.ProjectilePrefab, position, orientation);
                Projectile.transform.parent = null;
                ProjectileOrigin            = position;
                ProjectileSpeed             = shotDescriptor.ProjectileSpeed;
                ProjectileLastPosition      = ProjectileOrigin;
                ProjectileDistanceTravelled = 0.0f;
                ProjectileDeflectSource     = null;
                FirstFrame                  = true;
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Adds a value to the layer mask that is used to determine the objects the projectile can collide with.
            /// </summary>
            /// <param name="value">Value to add to the mask</param>
            public void AddShotLayerMask(int value)
            {
                _shotLayerMask |= value;
            }

            #endregion

            #region Private Types & Data

            private LayerMask _shotLayerMask;

            #endregion
        }

        #endregion
    }
}