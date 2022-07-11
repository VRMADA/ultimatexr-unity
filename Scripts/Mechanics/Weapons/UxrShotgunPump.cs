// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrShotgunPump.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Audio;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that, added to a GameObject with a <see cref="UxrFirearmWeapon" /> component, allows to communicate
    ///     whenever the shotgun is reloaded using a pump action using a <see cref="UxrGrabbableObject" />. The shot cycle in
    ///     the firearm should be set to <see cref="UxrShotCycle.ManualReload" />.
    /// </summary>
    [RequireComponent(typeof(UxrFirearmWeapon))]
    public partial class UxrShotgunPump : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]               private int                _triggerIndex;
        [SerializeField]               private UxrGrabbableObject _pump;
        [SerializeField]               private Vector3            _localPumpDirection               = Vector3.forward;
        [SerializeField]               private Vector3            _localPumpOffset                  = Vector3.forward * 0.2f;
        [SerializeField] [Range(0, 1)] private float              _slideThreshold                   = 0.7f;
        [SerializeField]               private UxrAudioSample     _audioSlide                       = new UxrAudioSample();
        [SerializeField]               private UxrAudioSample     _audioSlideBack                   = new UxrAudioSample();
        [SerializeField]               private UxrAudioSample     _audioSlideAlreadyLoaded          = new UxrAudioSample();
        [SerializeField]               private UxrAudioSample     _audioSlideBackAlreadyLoaded      = new UxrAudioSample();
        [SerializeField]               private UxrHapticClip      _hapticClipSlide                  = new UxrHapticClip(null, UxrHapticClipType.Click);
        [SerializeField]               private UxrHapticClip      _hapticClipSlideBack              = new UxrHapticClip(null, UxrHapticClipType.Click);
        [SerializeField]               private UxrHapticClip      _hapticClipSlideAlreadyLoaded     = new UxrHapticClip(null, UxrHapticClipType.Slide);
        [SerializeField]               private UxrHapticClip      _hapticClipSlideBackAlreadyLoaded = new UxrHapticClip(null, UxrHapticClipType.Slide);

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _state      = State.WaitPump;
            _localStart = _pump != null ? _pump.transform.localPosition : Vector3.zero;
            _firearm    = GetComponent<UxrFirearmWeapon>();
        }

        /// <summary>
        ///     Updates the component, looking for the pump action if necessary.
        /// </summary>
        private void Update()
        {
            if (_pump == null)
            {
                return;
            }

            float currentSlide = Vector3.Scale(_pump.transform.localPosition - _localStart, _localPumpDirection).magnitude / _localPumpOffset.magnitude;

            if (_state == State.WaitPump && currentSlide > _slideThreshold)
            {
                _state = State.WaitPumpBack;

                if (_firearm.IsLoaded(_triggerIndex))
                {
                    _audioSlideAlreadyLoaded.Play(_pump.transform.position);
                }
                else
                {
                    _audioSlide.Play(_pump.transform.position);
                }

                if (UxrGrabManager.Instance.GetGrabbingHand(_pump, 0, out UxrHandSide handSide))
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _firearm.IsLoaded(_triggerIndex) ? _hapticClipSlideAlreadyLoaded : _hapticClipSlide);
                }
            }
            else if (_state == State.WaitPumpBack && currentSlide < _slideThreshold * 0.9f)
            {
                if (_firearm.IsLoaded(_triggerIndex))
                {
                    _audioSlideBackAlreadyLoaded.Play(_pump.transform.position);
                }
                else
                {
                    _audioSlideBack.Play(_pump.transform.position);
                }

                if (UxrGrabManager.Instance.GetGrabbingHand(_pump, 0, out UxrHandSide handSide))
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _firearm.IsLoaded(_triggerIndex) ? _hapticClipSlideBackAlreadyLoaded : _hapticClipSlideBack);
                }

                _firearm.Reload(_triggerIndex);
                _state = State.WaitPump;
            }
        }

        #endregion

        #region Private Types & Data

        private UxrFirearmWeapon _firearm;
        private Vector3          _localStart;
        private State            _state;

        #endregion
    }
}