// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDamageEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Damage event parameters.
    /// </summary>
    public class UxrDamageEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the type of damage.
        /// </summary>
        public UxrDamageType DamageType { get; }

        /// <summary>
        ///     Gets the actor that inflicted the damage, or null if the damage didn't come from any specific actor.
        /// </summary>
        public UxrActor ActorSource { get; }

        /// <summary>
        ///     Gets the actor that received the damage.
        /// </summary>
        public UxrActor ActorTarget { get; }

        /// <summary>
        ///     Gets the raycast information for projectile hits. Only valid if <see cref="DamageType" /> is
        ///     <see cref="UxrDamageType.ProjectileHit" />.
        /// </summary>
        public RaycastHit RaycastHit { get; }

        /// <summary>
        ///     Gets the source position for explosive damage. Only valid if <see cref="DamageType" /> is
        ///     <see cref="UxrDamageType.Explosive" />
        /// </summary>
        public Vector3 ExplosionPosition { get; }

        /// <summary>
        ///     Gets the amount of damage taken/inflicted.
        /// </summary>
        public float Damage { get; }

        /// <summary>
        ///     Gets whether the damage will result in the death of the receiving actor.
        /// </summary>
        public bool Dies { get; }

        /// <summary>
        ///     Gets if the damage was canceled for damage pre-events. Damage post-events cannot be canceled since the damage was
        ///     already inflicted.
        /// </summary>
        public bool IsCanceled { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor for projectile damage.
        /// </summary>
        /// <param name="source">Source actor</param>
        /// <param name="target">Target actor</param>
        /// <param name="raycastHit">Raycast hit</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="dies">Whether the damage results in death</param>
        public UxrDamageEventArgs(UxrActor source, UxrActor target, RaycastHit raycastHit, float damage, bool dies)
        {
            DamageType  = UxrDamageType.ProjectileHit;
            ActorSource = source;
            ActorTarget = target;
            RaycastHit  = raycastHit;
            Damage      = damage;
            Dies        = dies;
        }

        /// <summary>
        ///     Constructor for explosive damage.
        /// </summary>
        /// <param name="source">Source actor or null if the damage didn't come from another actor</param>
        /// <param name="target">Target actor</param>
        /// <param name="explosionPosition">Explosion world position</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="dies">Whether the damage results in death</param>
        public UxrDamageEventArgs(UxrActor source, UxrActor target, Vector3 explosionPosition, float damage, bool dies)
        {
            DamageType        = UxrDamageType.Explosive;
            ActorSource       = source;
            ActorTarget       = target;
            ExplosionPosition = explosionPosition;
            Damage            = damage;
            Dies              = dies;
        }

        /// <summary>
        ///     Constructor for generic damage.
        /// </summary>
        /// <param name="damage">Damage amount</param>
        /// <param name="dies">Whether the damage results in death</param>
        public UxrDamageEventArgs(float damage, bool dies)
        {
            DamageType = UxrDamageType.Other;
            Damage     = damage;
            Dies       = dies;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Allows pre-events to cancel the damage. post-events can not be cancelled since the damage was already taken.
        /// </summary>
        public void Cancel()
        {
            IsCanceled = true;
        }

        #endregion
    }
}