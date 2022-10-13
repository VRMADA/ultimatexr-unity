// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmTrigger.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Audio;
using UltimateXR.Core.Math;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Stores all the information related to a trigger in a <see cref="UxrFirearmWeapon" />. The projectile that will be
    ///     shot is described by a separate <see cref="UxrShotDescriptor" />. <see cref="ProjectileShotIndex" /> determines
    ///     which <see cref="UxrShotDescriptor" /> in a <see cref="UxrProjectileSource" /> will be shot. It usually is 0, but
    ///     can be higher if the projectile source supports multiple shot types.
    /// </summary>
    [Serializable]
    internal class UxrFirearmTrigger
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int                      _projectileShotIndex;
        [SerializeField] private UxrShotCycle             _cycleType;
        [SerializeField] private int                      _maxShotFrequency;
        [SerializeField] private UxrAudioSample           _shotAudio;
        [SerializeField] private UxrAudioSample           _shotAudioNoAmmo;
        [SerializeField] private UxrHapticClip            _shotHapticClip = new UxrHapticClip(null, UxrHapticClipType.Shot);
        [SerializeField] private UxrGrabbableObject       _triggerGrabbable;
        [SerializeField] private int                      _grabbableGrabPointIndex;
        [SerializeField] private Transform                _triggerTransform;
        [SerializeField] private UxrAxis                  _triggerRotationAxis    = UxrAxis.X;
        [SerializeField] private float                    _triggerRotationDegrees = 40.0f;
        [SerializeField] private UxrGrabbableObjectAnchor _ammunitionMagAnchor;
        [SerializeField] private float                    _recoilAngleOneHand   = 0.5f;
        [SerializeField] private float                    _recoilAngleTwoHands  = 2.0f;
        [SerializeField] private Vector3                  _recoilOffsetOneHand  = -Vector3.forward * 0.03f;
        [SerializeField] private Vector3                  _recoilOffsetTwoHands = -Vector3.forward * 0.01f;
        [SerializeField] private float                    _recoilDurationSeconds;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the index in the <see cref="UxrProjectileSource" /> component of the shot fired whenever the triggers is
        ///     pressed.
        /// </summary>
        public int ProjectileShotIndex => _projectileShotIndex;

        /// <summary>
        ///     Gets the shot cycle type.
        /// </summary>
        public UxrShotCycle CycleType => _cycleType;

        /// <summary>
        ///     Gets the maximum shooting frequency.
        /// </summary>
        public int MaxShotFrequency => _maxShotFrequency;

        /// <summary>
        ///     Gets the audio played when the user pulls the trigger and the weapon shoots.
        /// </summary>
        public UxrAudioSample ShotAudio => _shotAudio;

        /// <summary>
        ///     Gets the audio played when the user pulls the trigger and the weapon isn't loaded.
        /// </summary>
        public UxrAudioSample ShotAudioNoAmmo => _shotAudioNoAmmo;

        /// <summary>
        ///     Gets the haptic feedback sent whenever the weapon shoots.
        /// </summary>
        public UxrHapticClip ShotHapticClip => _shotHapticClip;

        /// <summary>
        ///     Gets the object that is required to grab in order to access the trigger.
        /// </summary>
        public UxrGrabbableObject TriggerGrabbable => _triggerGrabbable;

        /// <summary>
        ///     Gets the index point for <see cref="TriggerGrabbable" />.
        /// </summary>
        public int GrabbableGrabPointIndex => _grabbableGrabPointIndex;

        /// <summary>
        ///     Gets the transform that will rotate when the trigger is pressed.
        /// </summary>
        public Transform TriggerTransform => _triggerTransform;

        /// <summary>
        ///     Gets the trigger rotation axis.
        /// </summary>
        public UxrAxis TriggerRotationAxis => _triggerRotationAxis;

        /// <summary>
        ///     Gets the amount of degrees that the trigger will rotate when it is fully pressed.
        /// </summary>
        public float TriggerRotationDegrees => _triggerRotationDegrees;

        /// <summary>
        ///     Gets the anchor where mags for ammo that will be shot using the trigger will be attached to.
        /// </summary>
        public UxrGrabbableObjectAnchor AmmunitionMagAnchor => _ammunitionMagAnchor;

        /// <summary>
        ///     Recoil rotation in degrees when a single hand is grabbing the weapon.
        /// </summary>
        public float RecoilAngleOneHand => _recoilAngleOneHand;

        /// <summary>
        ///     Recoil rotation in degrees when two  hands are grabbing the weapon.
        /// </summary>
        public float RecoilAngleTwoHands => _recoilAngleTwoHands;

        /// <summary>
        ///     Recoil offset when a single hand is grabbing the weapon.
        /// </summary>
        public Vector3 RecoilOffsetOneHand => _recoilOffsetOneHand;

        /// <summary>
        ///     Recoil offset when two hands are grabbing the weapon.
        /// </summary>
        public Vector3 RecoilOffsetTwoHands => _recoilOffsetTwoHands;

        /// <summary>
        ///     Recoil animation duration in seconds. The animation will be procedurally applied to the weapon.
        /// </summary>
        public float RecoilDurationSeconds => _recoilDurationSeconds;

        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets or sets the decreasing timer in seconds that will reach zero when the firearm is ready to shoot again.
        /// </summary>
        internal float LastShotTimer { get; set; }

        /// <summary>
        ///     Gets or sets whether the weapon is currently loaded.
        /// </summary>
        internal bool HasReloaded { get; set; }

        /// <summary>
        ///     Gets or sets the trigger's initial local rotation.
        /// </summary>
        internal Quaternion TriggerInitialLocalRotation { get; set; }

        /// <summary>
        ///     Gets or sets the decreasing timer in seconds that will reach zero when the recoil animation finished.
        /// </summary>
        internal float RecoilTimer { get; set; }

        #endregion
    }
}