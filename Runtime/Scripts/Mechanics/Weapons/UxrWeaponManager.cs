// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeaponManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Components.Singleton;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Singleton manager in charge of updating projectiles, computing hits against entities and damage done on
    ///     <see cref="UxrActor" /> components.
    /// </summary>
    public partial class UxrWeaponManager : UxrSingleton<UxrWeaponManager>, IUxrLogger
    {
        #region Public Types & Data

        /// <summary>
        ///     Event triggered right before an <see cref="UxrActor" /> is about to receive damage.
        ///     Setting <see cref="UxrDamageEventArgs.Cancel" /> will allow not to take the damage.
        /// </summary>
        public event EventHandler<UxrDamageEventArgs> DamageReceiving;

        /// <summary>
        ///     Event triggered right after the actor received damage.
        ///     Setting <see cref="UxrDamageEventArgs.Cancel" /> is not supported, since the damage was already taken.
        /// </summary>
        public event EventHandler<UxrDamageEventArgs> DamageReceived;

        /// <summary>
        ///     Event called whenever there was a projectile impact but no <see cref="UxrActor" /> was involved. Mostly hits
        ///     against the scenario that still generate decals, FX, etc.
        /// </summary>
        public event EventHandler<UxrNonDamagingImpactEventArgs> NonActorImpacted;

        #endregion

        #region Implicit IUxrLogger

        /// <inheritdoc />
        public UxrLogLevel LogLevel { get; set; } = UxrLogLevel.Relevant;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Updates the manager.
        /// </summary>
        public void UpdateManager()
        {
            UpdateProjectiles();
        }

        /// <summary>
        ///     Registers a new projectile shot so that it gets automatically update by the manager from that moment until it hits
        ///     something or gets destroyed.
        /// </summary>
        /// <param name="projectileSource">Projectile source</param>
        /// <param name="shotDescriptor">Shot descriptor</param>
        /// <param name="position">World position</param>
        /// <param name="orientation">World orientation. The projectile will travel in the forward (z) direction</param>
        public void RegisterNewProjectileShot(UxrProjectileSource projectileSource, UxrShotDescriptor shotDescriptor, Vector3 position, Quaternion orientation)
        {
            _projectiles.Add(new ProjectileInfo(projectileSource.TryGetWeaponOwner(), projectileSource, shotDescriptor, position, orientation));
        }

        /// <summary>
        ///     Applies radius damage to all elements around a source position.
        /// </summary>
        /// <param name="actorSource">The actor that was responsible for the damage or null if there wasn't any</param>
        /// <param name="position">Explosion world position</param>
        /// <param name="radius">Radius</param>
        /// <param name="nearDamage">Damage at the very same point of the explosion</param>
        /// <param name="farDamage">Damage at the distance set by <paramref name="radius" /></param>
        public void ApplyRadiusDamage(UxrActor actorSource, Vector3 position, float radius, float nearDamage, float farDamage)
        {
            foreach (KeyValuePair<UxrActor, ActorInfo> damageActorPair in _damageActors)
            {
                float distance = Vector3.Distance(damageActorPair.Value.Transform.position, position);

                if (distance < radius)
                {
                    float damage = Mathf.Lerp(nearDamage, farDamage, distance / radius);
                    damageActorPair.Key.ReceiveExplosion(actorSource, position, damage);
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the manager. Subscribes to actor enable/disable events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrActor.GlobalEnabled  += Actor_Enabled;
            UxrActor.GlobalDisabled += Actor_Disabled;
        }

        /// <summary>
        ///     Initializes the manager. Unsubscribes from actor enable/disable events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            UxrActor.GlobalEnabled  -= Actor_Enabled;
            UxrActor.GlobalDisabled -= Actor_Disabled;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever an <see cref="UxrActor" /> was enabled. Create a new entry in the internal dictionary.
        /// </summary>
        /// <param name="actor">Actor that was enabled</param>
        private void Actor_Enabled(UxrActor actor)
        {
            if (_damageActors.ContainsKey(actor) == false)
            {
                _damageActors.Add(actor, new ActorInfo(actor));
                actor.DamageReceiving += Actor_DamageReceiving;
                actor.DamageReceived  += Actor_DamageReceived;
            }
        }

        /// <summary>
        ///     Called whenever an <see cref="UxrActor" /> was disabled. Remove the entry from the internal dictionary.
        /// </summary>
        /// <param name="actor">Actor that was disabled</param>
        private void Actor_Disabled(UxrActor actor)
        {
            if (_damageActors.ContainsKey(actor))
            {
                _damageActors.Remove(actor);
                actor.DamageReceiving -= Actor_DamageReceiving;
                actor.DamageReceived  -= Actor_DamageReceived;
            }
        }

        /// <summary>
        ///     Called whenever an actor is about to receive any damage. The damage can be canceled using
        ///     <see cref="UxrDamageEventArgs.Cancel" />.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Actor_DamageReceiving(object sender, UxrDamageEventArgs e)
        {
            OnDamageReceiving(e);
        }

        /// <summary>
        ///     Called right after an actor received any damage. Damage can not be canceled using
        ///     <see cref="UxrDamageEventArgs.Cancel" /> because it was already taken.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Actor_DamageReceived(object sender, UxrDamageEventArgs e)
        {
            OnDamageReceived(e);
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="DamageReceiving" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnDamageReceiving(UxrDamageEventArgs e)
        {
            DamageReceiving?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="DamageReceived" />.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnDamageReceived(UxrDamageEventArgs e)
        {
            if (LogLevel >= UxrLogLevel.Relevant)
            {
                string sourceInfo = e.ActorSource != null ? $" from actor {e.ActorSource.name}." : string.Empty;
                Debug.Log($"{UxrConstants.WeaponsModule}: Actor {e.ActorTarget.name} received {e.Damage} damage of type {e.DamageType}{sourceInfo}. Did die? {e.Dies}.");
            }

            DamageReceived?.Invoke(this, e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates all current projectiles.
        /// </summary>
        private void UpdateProjectiles()
        {
            for (int i = 0; i < _projectiles.Count; ++i)
            {
                if (_projectiles[i].Projectile == null)
                {
                    // Dynamic instance removed somewhere
                    _projectiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (_projectiles[i].FirstFrame)
                {
                    // Render first frame where the weapon is
                    _projectiles[i].FirstFrame = false;
                    continue;
                }

                Vector3 oldPos = _projectiles[i].ProjectileLastPosition;
                Vector3 newPos = _projectiles[i].Projectile.transform.position;

                Vector3 projectileForward = _projectiles[i].Projectile.transform.forward;

                if (_projectiles[i].ShotDescriptor.UseAutomaticProjectileTrajectory)
                {
                    newPos                                        = oldPos + (_projectiles[i].ProjectileSpeed * Time.deltaTime * projectileForward);
                    _projectiles[i].Projectile.transform.position = newPos;
                }

                _projectiles[i].ProjectileLastPosition      =  newPos;
                _projectiles[i].ProjectileDistanceTravelled += Vector3.Distance(oldPos, newPos);

                if (_projectiles[i].ProjectileDistanceTravelled >= _projectiles[i].ShotDescriptor.ProjectileMaxDistance)
                {
                    // Max distance travelled
                    Destroy(_projectiles[i].Projectile);
                    _projectiles.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_projectiles[i].ShotDescriptor.UseAutomaticProjectileTrajectory)
                {
                    float rayLength = Vector3.Distance(oldPos, newPos + projectileForward * _projectiles[i].ShotDescriptor.ProjectileLength);

                    if (Physics.Raycast(oldPos, projectileForward, out RaycastHit raycastHit, rayLength, _projectiles[i].ShotLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        UxrProjectileDeflect projectileDeflect = raycastHit.collider.GetComponentInParent<UxrProjectileDeflect>();

                        // Has UxrProjectileDeflect component?

                        if (projectileDeflect != null)
                        {
                            Vector3 newForward = Vector3.Reflect(projectileForward, raycastHit.normal).normalized;
                            _projectiles[i].Projectile.transform.SetPositionAndRotation(raycastHit.point + newForward * 0.05f, Quaternion.LookRotation(newForward)); // + rayLength - raycastHit.distance));
                            _projectiles[i].ProjectileDeflectSource = projectileDeflect;

                            projectileDeflect.AudioDeflect?.Play(raycastHit.point);

                            if (projectileDeflect.DecalOnReflect != null)
                            {
                                // Generate decal?
                                UxrImpactDecal.CheckCreateDecal(raycastHit,
                                                                -1,
                                                                projectileDeflect.DecalOnReflect,
                                                                projectileDeflect.DecalLife,
                                                                projectileDeflect.DecalFadeoutDuration,
                                                                projectileDeflect.TwoSidedDecal,
                                                                projectileDeflect.TwoSidedDecalThickness);
                            }

                            _projectiles[i].AddShotLayerMask(projectileDeflect.CollideLayersAddOnReflect.value);
                            _projectiles[i].ProjectileLastPosition = _projectiles[i].Projectile.transform.position;

                            projectileDeflect.RaiseProjectileDeflected(new UxrDeflectEventArgs(_projectiles[i].ProjectileSource, raycastHit, newForward));
                        }
                        else
                        {
                            UxrActor targetActor = raycastHit.collider.GetComponentInParent<UxrActor>();

                            if (targetActor != null)
                            {
                                // Impact with an actor.

                                float normalizedDistance = _projectiles[i].ProjectileDistanceTravelled / _projectiles[i].ShotDescriptor.ProjectileMaxDistance;
                                float damage             = Mathf.Lerp(_projectiles[i].ShotDescriptor.ProjectileDamageNear, _projectiles[i].ShotDescriptor.ProjectileDamageFar, normalizedDistance);

                                if (_projectiles[i].ProjectileDeflectSource != null)
                                {
                                    // Came from a shot deflected by a UxrProjectileDeflect
                                    targetActor.ReceiveImpact(_projectiles[i].ProjectileDeflectSource.Owner, raycastHit, damage);
                                }
                                else
                                {
                                    // Direct hit from a projectile
                                    targetActor.ReceiveImpact(_projectiles[i].ProjectileSource.TryGetWeaponOwner(), raycastHit, damage);
                                }
                            }
                            else
                            {
                                // Impact with something that isn't an actor, probably the scenario.

                                if (_projectiles[i].ShotDescriptor.PrefabInstantiateOnImpact)
                                {
                                    Vector3    instantiateForward = Vector3.Reflect(projectileForward, raycastHit.normal).normalized;
                                    GameObject newInstance        = Instantiate(_projectiles[i].ShotDescriptor.PrefabInstantiateOnImpact, raycastHit.point, Quaternion.LookRotation(instantiateForward));

                                    if (_projectiles[i].ShotDescriptor.PrefabInstantiateOnImpactLife >= 0.0f)
                                    {
                                        Destroy(newInstance, _projectiles[i].ShotDescriptor.PrefabInstantiateOnImpactLife);
                                    }
                                }

                                Rigidbody rigidbody = raycastHit.collider.GetComponentInParent<Rigidbody>();

                                if (rigidbody != null)
                                {
                                    rigidbody.AddForceAtPosition(_projectiles[i].ProjectileSpeed * _projectiles[i].ShotDescriptor.ProjectileImpactForceMultiplier * projectileForward, raycastHit.transform.position);
                                }

                                UxrOverrideImpactDecal overrideDecal = raycastHit.collider.GetComponentInParent<UxrOverrideImpactDecal>();

                                // Generate decal?
                                UxrImpactDecal.CheckCreateDecal(raycastHit,
                                                                _projectiles[i].ShotDescriptor.CreateDecalLayerMask,
                                                                overrideDecal != null ? overrideDecal.DecalToUse : _projectiles[i].ShotDescriptor.PrefabScenarioImpactDecal,
                                                                _projectiles[i].ShotDescriptor.PrefabScenarioImpactDecalLife,
                                                                _projectiles[i].ShotDescriptor.DecalFadeoutDuration);

                                NonActorImpacted?.Invoke(_projectiles[i].ProjectileSource, new UxrNonDamagingImpactEventArgs(_projectiles[i].ProjectileSource.TryGetWeaponOwner(), _projectiles[i].ProjectileSource, raycastHit));
                            }

                            Destroy(_projectiles[i].Projectile);
                            _projectiles.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Types & Data

        private readonly Dictionary<UxrActor, ActorInfo> _damageActors = new Dictionary<UxrActor, ActorInfo>();
        private readonly List<ProjectileInfo>            _projectiles  = new List<ProjectileInfo>();

        #endregion
    }
}