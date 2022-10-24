// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrProjectileSource.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core.Caching;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that has the ability to fire shots.
    /// </summary>
    public class UxrProjectileSource : UxrComponent<UxrProjectileSource>, IUxrPrecacheable
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Animator                _weaponAnimator;
        [SerializeField] private List<UxrShotDescriptor> _shotTypes;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     The different shots that can be fired using the component.
        /// </summary>
        public IReadOnlyList<UxrShotDescriptor> ShotTypes => _shotTypes;

        #endregion

        #region Implicit IUxrPrecacheable

        /// <inheritdoc />
        public IEnumerable<GameObject> PrecachedInstances
        {
            get
            {
                foreach (UxrShotDescriptor shotType in _shotTypes)
                {
                    if (shotType.PrefabInstantiateOnImpact)
                    {
                        yield return shotType.PrefabInstantiateOnImpact;
                    }

                    if (shotType.PrefabInstantiateOnTipWhenShot)
                    {
                        yield return shotType.PrefabInstantiateOnTipWhenShot;
                    }

                    if (shotType.PrefabScenarioImpactDecal && shotType.PrefabScenarioImpactDecal.gameObject)
                    {
                        yield return shotType.PrefabScenarioImpactDecal.gameObject;
                    }

                    if (shotType.ProjectilePrefab)
                    {
                        yield return shotType.ProjectilePrefab;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to get the <see cref="UxrActor" /> that holds the <see cref="UxrWeapon" /> that has the
        ///     <see cref="UxrProjectileSource" /> component.
        /// </summary>
        /// <returns>Actor component or null if it wasn't found</returns>
        public UxrActor TryGetWeaponOwner()
        {
            UxrWeapon weapon = GetComponentInParent<UxrWeapon>();

            if (weapon)
            {
                return weapon.Owner;
            }

            return GetComponentInParent<UxrActor>();
        }

        /// <summary>
        ///     Shoots a round.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to fire</param>
        public void Shoot(int shotTypeIndex)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                Shoot(shotTypeIndex, _shotTypes[shotTypeIndex].ShotSource.position, _shotTypes[shotTypeIndex].ShotSource.rotation);
            }
        }

        /// <summary>
        ///     Shoots a round, overriding the source position and orientation.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to fire</param>
        /// <param name="projectileSource">Source shot position</param>
        /// <param name="projectileOrientation">Shot source orientation. The shot will be fired in the z (forward) direction</param>
        public void Shoot(int shotTypeIndex, Vector3 projectileSource, Quaternion projectileOrientation)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                if (_shotTypes[shotTypeIndex].PrefabInstantiateOnTipWhenShot)
                {
                    GameObject newInstance = Instantiate(_shotTypes[shotTypeIndex].PrefabInstantiateOnTipWhenShot, _shotTypes[shotTypeIndex].Tip.position, _shotTypes[shotTypeIndex].Tip.rotation);

                    if (_shotTypes[shotTypeIndex].PrefabInstantiateOnTipParent)
                    {
                        newInstance.transform.parent = transform;
                    }
                    else
                    {
                        newInstance.transform.parent = null;
                    }

                    if (_shotTypes[shotTypeIndex].PrefabInstantiateOnTipLife >= 0.0f)
                    {
                        Destroy(newInstance, _shotTypes[shotTypeIndex].PrefabInstantiateOnTipLife);
                    }
                }

                UxrWeaponManager.Instance.RegisterNewProjectileShot(this, _shotTypes[shotTypeIndex], projectileSource, projectileOrientation);

                if (_weaponAnimator != null && string.IsNullOrEmpty(_shotTypes[shotTypeIndex].ShotAnimationVarName) == false)
                {
                    _weaponAnimator.SetTrigger(_shotTypes[shotTypeIndex].ShotAnimationVarName);
                }
            }
        }

        /// <summary>
        ///     Shoots a round pointing to the given target.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to fire</param>
        /// <param name="target">Position where the shot will be going towards</param>
        public void ShootTo(int shotTypeIndex, Vector3 target)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                Vector3 direction = (target - _shotTypes[shotTypeIndex].ShotSource.position).normalized;
                Shoot(shotTypeIndex, _shotTypes[shotTypeIndex].ShotSource.position, Quaternion.LookRotation(direction));
            }
        }

        /// <summary>
        ///     Gets the distance where a shot using the current position and orientation will impact.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to use</param>
        /// <returns>Shot distance or a negative value telling the current target is out of range</returns>
        public float ShotRaycastDistance(int shotTypeIndex)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                if (Physics.Raycast(_shotTypes[shotTypeIndex].ShotSource.position,
                                    _shotTypes[shotTypeIndex].ShotSource.forward,
                                    out RaycastHit raycastHit,
                                    _shotTypes[shotTypeIndex].ProjectileMaxDistance,
                                    _shotTypes[shotTypeIndex].CollisionLayerMask,
                                    QueryTriggerInteraction.Ignore))
                {
                    return raycastHit.distance;
                }
            }

            return -1.0f;
        }

        /// <summary>
        ///     Gets the current world-space origin of projectiles fired using the given shot type.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to use</param>
        /// <returns>Projectile world-space source</returns>
        public Vector3 GetShotOrigin(int shotTypeIndex)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                return _shotTypes[shotTypeIndex].ShotSource.position;
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Gets the current world-space direction of projectiles fired using the given shot type.
        /// </summary>
        /// <param name="shotTypeIndex">Index in <see cref="ShotTypes" />, telling which shot type to use</param>
        /// <returns>Projectile world-space direction</returns>
        public Vector3 GetShotDirection(int shotTypeIndex)
        {
            if (shotTypeIndex >= 0 && shotTypeIndex < _shotTypes.Count)
            {
                return _shotTypes[shotTypeIndex].ShotSource.forward;
            }

            return Vector3.zero;
        }

        #endregion
    }
}