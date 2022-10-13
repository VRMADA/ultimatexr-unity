// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrActor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     An actor in the Weapons module is an entity that can inflict and/or take damage.
    /// </summary>
    public class UxrActor : UxrComponent<UxrActor>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float     _life;
        [SerializeField] private Animator  _animator;
        [SerializeField] private string    _takeDamageAnimationTriggerVarName;
        [SerializeField] private string    _dieAnimationTriggerVarName;
        [SerializeField] private AudioClip _takeDamageAudioClip;
        [SerializeField] private AudioClip _dieAudioClip;
        [SerializeField] private float     _destroyAfterDeadSeconds = -1.0f;
        [SerializeField] private bool      _automaticDamageHandling = true;
        [SerializeField] private bool      _automaticDeadHandling   = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event triggered right before the actor is about to receive damage.
        ///     Setting <see cref="UxrDamageEventArgs.Cancel" /> will allow not to take the damage.
        /// </summary>
        public event EventHandler<UxrDamageEventArgs> DamageReceiving;

        /// <summary>
        ///     Event triggered right after the actor received damage.
        ///     Setting <see cref="UxrDamageEventArgs.Cancel" /> is not supported, since the damage was already taken.
        /// </summary>
        public event EventHandler<UxrDamageEventArgs> DamageReceived;

        /// <summary>
        ///     Gets or sets whether damage should be handled automatically. Automatic damage handling will take care of computing
        ///     the new life value when receiving damage.
        /// </summary>
        public bool AutomaticDamageHandling
        {
            get => _automaticDamageHandling;
            set => _automaticDamageHandling = value;
        }

        /// <summary>
        ///     Gets or sets whether to handle death automatically when the actor's life reaches zero.
        /// </summary>
        public bool AutomaticDeadHandling
        {
            get => _automaticDeadHandling;
            set => _automaticDeadHandling = value;
        }

        /// <summary>
        ///     Gets or sets the actor's life value.
        /// </summary>
        public float Life
        {
            get => _life;
            set => _life = value;
        }

        /// <summary>
        ///     Gets whether the actor is dead.
        /// </summary>
        public bool IsDead { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Makes the actor receive a damaging projectile impact.
        /// </summary>
        /// <param name="actorSource">Actor source of the projectile</param>
        /// <param name="raycastHit">Raycast that hit the actor</param>
        /// <param name="damage">Damage to be taken</param>
        public void ReceiveImpact(UxrActor actorSource, RaycastHit raycastHit, float damage)
        {
            OnReceiveDamage(new UxrDamageEventArgs(actorSource, this, raycastHit, damage, damage >= Life));
        }

        /// <summary>
        ///     Makes the actor receive explosive damage.
        /// </summary>
        /// <param name="actorSource">Actor source of the projectile</param>
        /// <param name="position">Explosion source</param>
        /// <param name="damage">Damage to be taken</param>
        public void ReceiveExplosion(UxrActor actorSource, Vector3 position, float damage)
        {
            OnReceiveDamage(new UxrDamageEventArgs(actorSource, this, position, damage, damage >= Life));
        }

        /// <summary>
        ///     Makes the actor receive generic damage.
        /// </summary>
        /// <param name="damage">Damage to be taken</param>
        public void ReceiveDamage(float damage)
        {
            OnReceiveDamage(new UxrDamageEventArgs(damage, damage >= Life));
        }

        /// <summary>
        ///     Forces the actor to die after a certain amount of seconds.
        /// </summary>
        /// <param name="delaySeconds">Seconds to wait for the actor to die</param>
        public void Die(float delaySeconds)
        {
            Invoke(nameof(DieInternal), delaySeconds);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Makes sure the <see cref="UxrWeaponManager" /> singleton instance is available so that actors are registered."/>
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrWeaponManager.Instance.Poke();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Handles receiving damage and calls the appropriate events.
        /// </summary>
        /// <param name="e">Damage event parameters</param>
        private void OnReceiveDamage(UxrDamageEventArgs e)
        {
            if (IsDead)
            {
                return;
            }

            DamageReceiving?.Invoke(this, e);

            if (!e.IsCanceled)
            {
                bool destroy = false;
                
                if (_automaticDamageHandling)
                {
                    _life -= e.Damage;
                }

                if (_life <= 0.0f)
                {
                    // Deadly damage

                    if (_automaticDeadHandling)
                    {
                        destroy = true;
                    }
                    else
                    {
                        IsDead = true;
                    }
                }
                else
                {
                    // Non-deadly damage

                    if (_animator != null && string.IsNullOrEmpty(_takeDamageAnimationTriggerVarName) == false)
                    {
                        _animator.SetTrigger(_takeDamageAnimationTriggerVarName);
                    }

                    if (_takeDamageAudioClip)
                    {
                        AudioSource.PlayClipAtPoint(_takeDamageAudioClip, transform.position);
                    }
                }

                DamageReceived?.Invoke(this, e);

                if (destroy)
                {
                    DieInternal();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Makes the actor die.
        /// </summary>
        private void DieInternal()
        {
            Life   = 0.0f;
            IsDead = true;

            if (_animator != null && string.IsNullOrEmpty(_dieAnimationTriggerVarName) == false)
            {
                _animator.SetTrigger(_dieAnimationTriggerVarName);
            }

            if (_dieAudioClip)
            {
                AudioSource.PlayClipAtPoint(_dieAudioClip, transform.position);
            }

            Destroy(gameObject, _destroyAfterDeadSeconds > 0.0f ? _destroyAfterDeadSeconds : 0.0f);
        }

        #endregion
    }
}