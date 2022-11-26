// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrExplodeHierarchy.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.GameObjects;
using UltimateXR.Audio;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that allows to explode a GameObject and all its rigidbody children.
    ///     If the component is attached to a GameObject that also has a <see cref="UxrActor" /> component the explosion will
    ///     be triggered when the actor dies.
    ///     The explosion can also be called explicitly using <see cref="Explode" /> and <see cref="ExplodeNow" />.
    /// </summary>
    public class UxrExplodeHierarchy : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrAudioSample[] _audioExplodePool;
        [SerializeField] private float            _minExplodeSpeed        = 5.0f;
        [SerializeField] private float            _maxExplodeSpeed        = 20.0f;
        [SerializeField] private float            _minExplodeAngularSpeed = 1.0f;
        [SerializeField] private float            _maxExplodeAngularSpeed = 8.0f;
        [SerializeField] private float            _secondsToExplode       = -1.0f;
        [SerializeField] private float            _piecesLifeSeconds      = 5.0f;
        [SerializeField] private float            _piecesFadeoutSeconds   = 1.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the explode timer. A negative value will either indicate that it is not using any timer (if
        ///     <see cref="HasExploded" /> is false) or that it already exploded (if <see cref="HasExploded" /> is true).
        /// </summary>
        public float ExplodeTimer { get; private set; } = -1.0f;

        /// <summary>
        ///     Gets whether the object has exploded.
        /// </summary>
        public bool HasExploded { get; private set; }

        /// <summary>
        ///     Gets or sets the minimum random speed that the chunks will have when the object explodes.
        /// </summary>
        public float MinExplodeSpeed
        {
            get => _minExplodeSpeed;
            set => _minExplodeSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the maximum random speed that the chunks will have when the object explodes.
        /// </summary>
        public float MaxExplodeSpeed
        {
            get => _maxExplodeSpeed;
            set => _maxExplodeSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the minimum random angular speed that the chunks will have when the object explodes.
        /// </summary>
        public float MinExplodeAngularSpeed
        {
            get => _minExplodeAngularSpeed;
            set => _minExplodeAngularSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the maximum random angular speed that the chunks will have when the object explodes.
        /// </summary>
        public float MaxExplodeAngularSpeed
        {
            get => _maxExplodeAngularSpeed;
            set => _maxExplodeAngularSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the seconds it will take for the object to explode after the component is enabled. A negative value
        ///     will disable the user of a timer and will only explode when the object was added to an <see cref="UxrActor" /> that
        ///     dies.
        /// </summary>
        public float SecondsToExplode
        {
            get => _secondsToExplode;
            set
            {
                ExplodeTimer      = value;
                _secondsToExplode = value;
            }
        }

        /// <summary>
        ///     Gets or sets the seconds it will take for the chunks to disappear after the object explodes.
        /// </summary>
        public float PiecesLifeSeconds
        {
            get => _piecesLifeSeconds;
            set => _piecesLifeSeconds = value;
        }

        /// <summary>
        ///     Gets or sets the seconds it will take for the chunks to fade-out when the chunks disappear after
        ///     <see cref="PiecesLifeSeconds" />.
        /// </summary>
        public float PiecesFadeoutSeconds
        {
            get => _piecesFadeoutSeconds;
            set => _piecesFadeoutSeconds = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Explodes an object.
        /// </summary>
        /// <param name="root">Root object to explode</param>
        /// <param name="minExplodeVelocity">Minimum random velocity assigned to the chunks</param>
        /// <param name="maxExplodeVelocity">Maximum random velocity assigned to the chunks</param>
        /// <param name="secondsToExplode">Seconds to wait before exploding</param>
        /// <param name="piecesLifeSeconds">Life in seconds to assign to the chunks, after which they will be destroyed</param>
        public static void Explode(GameObject root, float minExplodeVelocity, float maxExplodeVelocity, float secondsToExplode, float piecesLifeSeconds)
        {
            UxrExplodeHierarchy explodeHierarchy = root.AddComponent<UxrExplodeHierarchy>();

            explodeHierarchy.MinExplodeSpeed   = minExplodeVelocity;
            explodeHierarchy.MaxExplodeSpeed   = maxExplodeVelocity;
            explodeHierarchy.SecondsToExplode  = secondsToExplode;
            explodeHierarchy.PiecesLifeSeconds = piecesLifeSeconds;
        }

        /// <summary>
        ///     Explodes an object immediately using the current parameters.
        /// </summary>
        public void ExplodeNow()
        {
            if (HasExploded)
            {
                return;
            }

            foreach (Collider chunkCollider in _colliders)
            {
                chunkCollider.enabled = true;
                chunkCollider.transform.SetParent(null);
                
                if (!chunkCollider.gameObject.TryGetComponent<Rigidbody>(out var rigidBodyChunk))
                {
                    rigidBodyChunk = chunkCollider.gameObject.AddComponent<Rigidbody>();
                }

                rigidBodyChunk.isKinematic     = false;
                rigidBodyChunk.velocity        = Random.insideUnitSphere * Random.Range(MinExplodeSpeed,        MaxExplodeSpeed);
                rigidBodyChunk.angularVelocity = Random.insideUnitSphere * Random.Range(MinExplodeAngularSpeed, MaxExplodeAngularSpeed);

                float    startAlpha = 1.0f;
                Renderer renderer   = chunkCollider.gameObject.GetComponent<Renderer>();

                if (renderer)
                {
                    startAlpha = renderer.material.color.a;
                }

                UxrObjectFade.Fade(chunkCollider.gameObject, startAlpha, 0.0f, PiecesLifeSeconds - PiecesFadeoutSeconds, PiecesFadeoutSeconds);
                Destroy(rigidBodyChunk.gameObject, PiecesLifeSeconds);
            }

            HasExploded  = true;
            ExplodeTimer = -1.0f;

            if (_audioExplodePool != null)
            {
                int i = Random.Range(0, _audioExplodePool.Length);
                _audioExplodePool[i].Play(transform.position);
            }

            Destroy(gameObject);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _colliders  = gameObject.GetComponentsInChildren<Collider>(true);
            HasExploded = false;
        }

        /// <summary>
        ///     Subscribes to events and initializes the explosion timer.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (!HasExploded)
            {
                ExplodeTimer = SecondsToExplode;
            }

            UxrActor actor = GetCachedComponent<UxrActor>();

            if (actor)
            {
                actor.DamageReceived += Actor_Damaged;
            }
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrActor actor = GetCachedComponent<UxrActor>();

            if (actor)
            {
                actor.DamageReceived -= Actor_Damaged;
            }
        }

        /// <summary>
        ///     Updates the explosion timer and checks if the object needs to explode.
        /// </summary>
        private void Update()
        {
            if (ExplodeTimer >= 0.0f && !HasExploded)
            {
                ExplodeTimer -= Time.deltaTime;

                if (ExplodeTimer < 0.0f)
                {
                    ExplodeNow();
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the component was added to an object with an <see cref="UxrActor" /> component and it took damage.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Actor_Damaged(object sender, UxrDamageEventArgs e)
        {
            if (e.Dies)
            {
                ExplodeNow();
            }
        }

        #endregion

        #region Private Types & Data

        private Collider[] _colliders;

        #endregion
    }
}