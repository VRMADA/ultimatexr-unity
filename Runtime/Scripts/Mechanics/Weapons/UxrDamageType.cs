// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDamageType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Enumerates the different damage types that can be taken.
    /// </summary>
    public enum UxrDamageType
    {
        /// <summary>
        ///     Damage due to projectile hit.
        /// </summary>
        ProjectileHit,

        /// <summary>
        ///     Damage due to explosion.
        /// </summary>
        Explosive,

        /// <summary>
        ///     Other types of damage (falls, elements from scenario that generate damage...).
        /// </summary>
        Other
    }
}