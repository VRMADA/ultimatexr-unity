// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrShotDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Class describing all the information of a type of projectile that a GameObject having a
    ///     <see cref="UxrProjectileSource" /> component can shoot.
    ///     Normally there will be a <see cref="UxrFirearmWeapon" /> with a <see cref="UxrProjectileSource" /> component
    ///     supporting one or more <see cref="UxrShotDescriptor" />.
    ///     For example, a rifle with a grenade launcher attachment will be able to fire two types of projectiles: bullets and
    ///     explosive grenades.
    ///     <see cref="UxrProjectileSource" /> components, however, do not require to be part of a
    ///     <see cref="UxrFirearmWeapon" /> and can be used on their own.
    /// </summary>
    [Serializable]
    public class UxrShotDescriptor
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform      _shotSource;
        [SerializeField] private Transform      _tip;
        [SerializeField] private bool           _useAutomaticProjectileTrajectory = true;
        [SerializeField] private string         _shotAnimationVarName;
        [SerializeField] private GameObject     _prefabInstantiateOnTipWhenShot;
        [SerializeField] private float          _prefabInstantiateOnTipLife   = 5.0f;
        [SerializeField] private bool           _prefabInstantiateOnTipParent = true;
        [SerializeField] private GameObject     _projectilePrefab;
        [SerializeField] private float          _projectileSpeed                 = 30.0f;
        [SerializeField] private float          _projectileMaxDistance           = 300.0f;
        [SerializeField] private float          _projectileLength                = 0.01f;
        [SerializeField] private float          _projectileDamageNear            = 20.0f;
        [SerializeField] private float          _projectileDamageFar             = 20.0f;
        [SerializeField] private float          _projectileImpactForceMultiplier = 1.0f;
        [SerializeField] private LayerMask      _collisionLayerMask              = -1;
        [SerializeField] private GameObject     _prefabInstantiateOnImpact;
        [SerializeField] private float          _prefabInstantiateOnImpactLife = 5.0f;
        [SerializeField] private UxrImpactDecal _prefabScenarioImpactDecal;
        [SerializeField] private float          _prefabScenarioImpactDecalLife = 10.0f;
        [SerializeField] private float          _decalFadeoutDuration          = 7.0f;
        [SerializeField] private LayerMask      _createDecalLayerMask          = -1;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is used to fire projectiles from, using the forward vector as direction.
        /// </summary>
        public Transform ShotSource => _shotSource;

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is used to instantiate effects on the tip when a shot was fired, using
        ///     <see cref="PrefabInstantiateOnTipWhenShot" />.
        /// </summary>
        public Transform Tip => _tip;

        /// <summary>
        ///     Gets whether the projectiles fired should be updated automatically to compute their trajectory or they will be
        ///     updated manually.
        /// </summary>
        public bool UseAutomaticProjectileTrajectory => _useAutomaticProjectileTrajectory;

        /// <summary>
        ///     Optional <see cref="Animator" /> trigger variable name that will be triggered on the weapon each time a round is
        ///     fired.
        /// </summary>
        public string ShotAnimationVarName => _shotAnimationVarName;

        /// <summary>
        ///     An optional prefab that will be instantiated on the <see cref="Tip" /> each time a round is fired.
        /// </summary>
        public GameObject PrefabInstantiateOnTipWhenShot => _prefabInstantiateOnTipWhenShot;

        /// <summary>
        ///     Life in seconds of <see cref="PrefabInstantiateOnTipWhenShot" /> after which it will be destroyed.
        /// </summary>
        public float PrefabInstantiateOnTipLife => _prefabInstantiateOnTipLife;

        /// <summary>
        ///     Whether <see cref="PrefabInstantiateOnTipWhenShot" /> will be parented to the <see cref="Tip" /> after being
        ///     instantiated or will remain unparented.
        /// </summary>
        public bool PrefabInstantiateOnTipParent => _prefabInstantiateOnTipParent;

        /// <summary>
        ///     Prefab that will be instantiated as the projectile.
        /// </summary>
        public GameObject ProjectilePrefab => _projectilePrefab;

        /// <summary>
        ///     Speed at which the projectile will move.
        /// </summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>
        ///     Maximum reach of the projectile, after which it will be destroyed.
        /// </summary>
        public float ProjectileMaxDistance => _projectileMaxDistance;

        /// <summary>
        ///     The physical length of the projectile, used in ray-casting computations.
        /// </summary>
        public float ProjectileLength => _projectileLength;

        /// <summary>
        ///     The damage a projectile will do if it were to hit at the closest distance. Damage will linearly decrease over
        ///     distance down to <see cref="ProjectileDamageFar" /> until the projectile reaches
        ///     <see cref="_projectileMaxDistance" />.
        /// </summary>
        public float ProjectileDamageNear => _projectileDamageNear;

        /// <summary>
        ///     The damage a projectile will do if it were to hit at the farthest distance. Damage will linearly decrease over
        ///     distance from the start down to <see cref="ProjectileDamageFar" /> until the projectile reaches
        ///     <see cref="_projectileMaxDistance" />.
        /// </summary>
        public float ProjectileDamageFar => _projectileDamageFar;

        /// <summary>
        ///     The force multiplier applied to a rigidbody that was hit by a projectile. The total force applied will be speed *
        ///     ForceMultiplier.
        /// </summary>
        public float ProjectileImpactForceMultiplier => _projectileImpactForceMultiplier;

        /// <summary>
        ///     The layer mask used to determine which objects can be hit.
        /// </summary>
        public LayerMask CollisionLayerMask => _collisionLayerMask;

        /// <summary>
        ///     An optional prefab to instantiate at the point of impact.
        /// </summary>
        public GameObject PrefabInstantiateOnImpact => _prefabInstantiateOnImpact;

        /// <summary>
        ///     Life in seconds after which <see cref="PrefabInstantiateOnImpact" /> will be destroyed.
        /// </summary>
        public float PrefabInstantiateOnImpactLife => _prefabInstantiateOnImpactLife;

        /// <summary>
        ///     Default decal that will be used when the projectile impacted with something. Can be overriden using the
        ///     <see cref="UxrOverrideImpactDecal" /> component.
        /// </summary>
        public UxrImpactDecal PrefabScenarioImpactDecal => _prefabScenarioImpactDecal;

        /// <summary>
        ///     Life in seconds after which <see cref="PrefabScenarioImpactDecal" /> will fadeout and be destroyed.
        /// </summary>
        public float PrefabScenarioImpactDecalLife => _prefabScenarioImpactDecalLife;

        /// <summary>
        ///     Duration of the fadeout effect before a <see cref="PrefabScenarioImpactDecal" /> is destroyed.
        /// </summary>
        public float DecalFadeoutDuration => _decalFadeoutDuration;

        /// <summary>
        ///     The layer mask used to determine if an impact will generate a decal.
        /// </summary>
        public LayerMask CreateDecalLayerMask => _createDecalLayerMask;

        #endregion
    }
}