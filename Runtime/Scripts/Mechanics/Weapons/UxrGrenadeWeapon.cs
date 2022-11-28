// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrenadeWeapon.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Audio;
using UltimateXR.Avatar;
using UltimateXR.Core.Caching;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Grenade weapon. A grenade inflicts explosive damage to <see cref="UxrActor" /> components.
    /// </summary>
    public class UxrGrenadeWeapon : UxrWeapon, IUxrPrecacheable
    {
        #region Inspector Properties/Serialized Fields

        // General parameters
        [SerializeField] private UxrGrenadeActivationMode _activationTrigger;
        [SerializeField] private bool                     _explodeOnCollision;

        // Timer
        [SerializeField] private float _timerSeconds = 3.0f;

        // Pin
        [SerializeField] private UxrGrabbableObject _pin;
        [SerializeField] private UxrAudioSample     _audioRemovePin;
        [SerializeField] private UxrHapticClip      _hapticRemovePin = new UxrHapticClip(null, UxrHapticClipType.Click);

        // Impact
        [SerializeField] private LayerMask _impactExplosionCollisionMask = ~0;

        // Explosion        
        [SerializeField] private GameObject[] _explosionPrefabPool;
        [SerializeField] private float        _explosionPrefabLife = 4.0f;

        // Damage
        [SerializeField] private float _damageRadius = 10.0f;
        [SerializeField] private float _damageNear   = 10.0f;
        [SerializeField] private float _damageFar;

        // Physics
        [SerializeField] private bool  _createPhysicsExplosion;
        [SerializeField] private float _physicsExplosionForce = 10.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the grenade has been activated and the detonation timer is running.
        /// </summary>
        public bool IsActivated => _timer >= 0.0f;

        /// <summary>
        ///     Gets the seconds left to explode. If the grenade hasn't been activated yet, it will return
        ///     <see cref="TimerDuration" />.
        /// </summary>
        public float Timer => _timer < 0.0f ? _timerSeconds : _timer;

        /// <summary>
        ///     Gets the seconds it will take for the grenade to explode once it has been activated.
        /// </summary>
        public float TimerDuration => _timerSeconds;

        /// <summary>
        ///     Gets distance from the explosion the grenade will start inflicting damage.
        /// </summary>
        public float DamageRadius => _damageRadius;

        /// <summary>
        ///     Gets the maximum damage, applied at the very point of the explosion.
        /// </summary>
        public float DamageNear => _damageNear;

        /// <summary>
        ///     Gets the minimum damage, applied at <see cref="DamageRadius" /> distance.
        /// </summary>
        public float DamageFar => _damageFar;

        #endregion

        #region Implicit IUxrPrecacheable

        /// <inheritdoc />
        public IEnumerable<GameObject> PrecachedInstances => _explosionPrefabPool;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts the detonation timer.
        /// </summary>
        public void ActivateTimer()
        {
            _timer = _timerSeconds;
        }

        /// <summary>
        ///     Freezes or resumes the detonation timer.
        /// </summary>
        /// <param name="freeze">Whether to freeze or resume the detonation timer</param>
        public void FreezeTimer(bool freeze = true)
        {
            _timerFrozen = freeze;
        }

        /// <summary>
        ///     Restores the detonation timer to the initial time.
        /// </summary>
        public void RestoreTimer()
        {
            _timer = _timerSeconds;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (_pin)
            {
                _pin.Grabbed += Pin_Grabbed;
            }

            if (_activationTrigger == UxrGrenadeActivationMode.OnHandLaunch)
            {
                if (GrabbableObject != null)
                {
                    GrabbableObject.Released += Grenade_Released;
                }
            }
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (_pin)
            {
                _pin.Grabbed -= Pin_Grabbed;
            }

            if (_activationTrigger == UxrGrenadeActivationMode.OnHandLaunch)
            {
                if (GrabbableObject != null)
                {
                    GrabbableObject.Released -= Grenade_Released;
                }
            }
        }

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (_pin)
            {
                // Disable collider. We will enable it once the pin is grabbed.

                Collider pinCollider = _pin.GetCachedComponent<Collider>();

                if (pinCollider)
                {
                    pinCollider.enabled = false;
                }
            }
        }

        /// <summary>
        ///     Updates the component.
        /// </summary>
        private void Update()
        {
            if (_timer > 0.0f && !_timerFrozen)
            {
                _timer -= Time.deltaTime;

                if (_timer <= 0.0f)
                {
                    Explode();
                }
            }
        }

        /// <summary>
        ///     Called by Unity when the physics-driven rigidbody collider hit something.
        /// </summary>
        /// <param name="collision">Collision object</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (_explodeOnCollision && (_impactExplosionCollisionMask.value & 1 << collision.collider.gameObject.layer) != 0)
            {
                Explode();
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the grenade was released.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Grenade_Released(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsOwnershipChanged && _activationTrigger == UxrGrenadeActivationMode.OnHandLaunch)
            {
                _timer = _timerSeconds;
            }
        }

        /// <summary>
        ///     Called when the pin was grabbed. It will remove the pin from the grenade.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Pin_Grabbed(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsOwnershipChanged && _activationTrigger == UxrGrenadeActivationMode.TriggerPin)
            {
                _timer = _timerSeconds;

                Collider pinCollider = _pin.GetCachedComponent<Collider>();

                if (pinCollider)
                {
                    pinCollider.enabled = true;
                }

                _audioRemovePin?.Play(transform.position);

                // Send haptic to the hand that grabbed the pin

                e.Grabber.Avatar.ControllerInput.SendGrabbableHapticFeedback(e.GrabbableObject, _hapticRemovePin);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Explodes the grenade, causing explosion and damage.
        /// </summary>
        private void Explode()
        {
            if (_exploded)
            {
                return;
            }

            _exploded = true;

            if (_explosionPrefabPool.Length > 0)
            {
                GameObject newExplosion = Instantiate(_explosionPrefabPool[Random.Range(0, _explosionPrefabPool.Length)], transform.position, Quaternion.LookRotation(-UxrAvatar.LocalAvatar.CameraForward));

                if (_explosionPrefabLife > 0.0f)
                {
                    Destroy(newExplosion, _explosionPrefabLife);
                }
            }

            if (_createPhysicsExplosion)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, _damageRadius);

                foreach (Collider targetCollider in colliders)
                {
                    if (targetCollider.TryGetComponent<Rigidbody>(out var targetRigidbody))
                    {
                        targetRigidbody.AddExplosionForce(_physicsExplosionForce, transform.position, _damageRadius);
                    }
                }
            }

            UxrWeaponManager.Instance.ApplyRadiusDamage(Owner, transform.position, _damageRadius, _damageNear, _damageFar);

            Destroy(gameObject);
        }

        #endregion

        #region Private Types & Data

        private float _timer = -1.0f;
        private bool  _timerFrozen;
        private bool  _exploded;

        #endregion
    }
}