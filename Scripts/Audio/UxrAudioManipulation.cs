// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAudioManipulation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UnityEngine;

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Audio
{
    /// <summary>
    ///     Component that enables to play audio depending on the grab and manipulation events of the
    ///     <see cref="UxrGrabbableObject" /> in the same <see cref="GameObject" />.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public class UxrAudioManipulation : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [Header("Events:")] [SerializeField] private UxrAudioSample _audioOnGrab    = new UxrAudioSample();
        [SerializeField]                     private UxrAudioSample _audioOnPlace   = new UxrAudioSample();
        [SerializeField]                     private UxrAudioSample _audioOnRelease = new UxrAudioSample();

        [Header("Continuous Manipulation:")] [SerializeField] private bool      _continuousManipulationAudio;
        [SerializeField]                                      private AudioClip _audioLoopClip;
        [SerializeField] [Range(0, 1)]                        private float     _minVolume       = 0.3f;
        [SerializeField] [Range(0, 1)]                        private float     _maxVolume       = 1.0f;
        [SerializeField]                                      private float     _minFrequency    = 1.0f;
        [SerializeField]                                      private float     _maxFrequency    = 1.0f;
        [SerializeField]                                      private float     _minSpeed        = 0.01f;
        [SerializeField]                                      private float     _maxSpeed        = 1.0f;
        [SerializeField]                                      private float     _minAngularSpeed = 1.0f;
        [SerializeField]                                      private float     _maxAngularSpeed = 1800.0f;
        [SerializeField]                                      private bool      _useExternalRigidbody;
        [SerializeField]                                      private Rigidbody _externalRigidbody;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="UxrGrabbableObject" /> component in the same <see cref="GameObject" />.
        /// </summary>
        public UxrGrabbableObject GrabbableObject => GetCachedComponent<UxrGrabbableObject>();

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to manipulation events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            GrabbableObject.Grabbed  += Object_Grabbed;
            GrabbableObject.Placed   += Object_Placed;
            GrabbableObject.Released += Object_Released;
        }

        /// <summary>
        ///     Unsubscribes from manipulation events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            GrabbableObject.Grabbed  -= Object_Grabbed;
            GrabbableObject.Placed   -= Object_Placed;
            GrabbableObject.Released -= Object_Released;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the object was grabbed.
        /// </summary>
        /// <param name="sender">Grabbable object that sent the event</param>
        /// <param name="e">Event arguments</param>
        private void Object_Grabbed(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                _audioOnGrab.Play(transform.position);
            }
        }

        /// <summary>
        ///     Called when the object was placed on an <see cref="UxrGrabbableObjectAnchor" />.
        /// </summary>
        /// <param name="sender">Grabbable object that sent the event</param>
        /// <param name="e">Event arguments</param>
        private void Object_Placed(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                _audioOnPlace.Play(transform.position);
            }
        }

        /// <summary>
        ///     Called when the object was released in the air.
        /// </summary>
        /// <param name="sender">Grabbable object that sent the event</param>
        /// <param name="e">Event arguments</param>
        private void Object_Released(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                _audioOnRelease.Play(transform.position);
            }
        }

        #endregion
    }
}

#pragma warning restore 414