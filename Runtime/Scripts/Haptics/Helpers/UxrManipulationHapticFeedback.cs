// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManipulationHapticFeedback.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Haptics.Helpers
{
    /// <summary>
    ///     Component that, added to a grabbable object (<see cref="UxrGrabbableObject" />), sends haptic feedback to any
    ///     controller that manipulates it.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public class UxrManipulationHapticFeedback : UxrGrabbableObjectComponent<UxrManipulationHapticFeedback>
    {
        #region Inspector Properties/Serialized Fields

        [Header("Continuous Manipulation:")] [SerializeField] private bool          _continuousManipulationHaptics;
        [SerializeField]                                      private UxrHapticMode _hapticMixMode   = UxrHapticMode.Mix;
        [SerializeField] [Range(0, 1)]                        private float         _minAmplitude    = 0.3f;
        [SerializeField] [Range(0, 1)]                        private float         _maxAmplitude    = 1.0f;
        [SerializeField]                                      private float         _minFrequency    = 10.0f;
        [SerializeField]                                      private float         _maxFrequency    = 100.0f;
        [SerializeField]                                      private float         _minSpeed        = 0.01f;
        [SerializeField]                                      private float         _maxSpeed        = 1.0f;
        [SerializeField]                                      private float         _minAngularSpeed = 1.0f;
        [SerializeField]                                      private float         _maxAngularSpeed = 1800.0f;
        [SerializeField]                                      private bool          _useExternalRigidbody;
        [SerializeField]                                      private Rigidbody     _externalRigidbody;

        [Header("Events Haptics:")] [SerializeField] private UxrHapticClip _hapticClipOnGrab    = new UxrHapticClip();
        [SerializeField]                             private UxrHapticClip _hapticClipOnPlace   = new UxrHapticClip();
        [SerializeField]                             private UxrHapticClip _hapticClipOnRelease = new UxrHapticClip();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets whether the component will send haptic feedback continuously while the object is being grabbed.
        /// </summary>
        public bool ContinuousManipulationHaptics
        {
            get => _continuousManipulationHaptics;
            set => _continuousManipulationHaptics = value;
        }

        /// <summary>
        ///     Gets or sets the haptic feedback mix mode.
        /// </summary>
        public UxrHapticMode HapticMixMode
        {
            get => _hapticMixMode;
            set => _hapticMixMode = value;
        }

        /// <summary>
        ///     Gets or sets continuous manipulation haptic feedback's minimum amplitude, which is the haptic amplitude sent when
        ///     the object is moving/rotating at or below <see cref="MinSpeed" />/<see cref="MinAngularSpeed" />.
        /// </summary>
        public float MinAmplitude
        {
            get => _minAmplitude;
            set => _minAmplitude = value;
        }

        /// <summary>
        ///     Gets or sets continuous manipulation haptic feedback's maximum amplitude, which is the haptic amplitude sent when
        ///     the object is moving/rotating at or over <see cref="MaxSpeed" />/<see cref="MaxAngularSpeed" />.
        /// </summary>
        public float MaxAmplitude
        {
            get => _maxAmplitude;
            set => _maxAmplitude = value;
        }

        /// <summary>
        ///     Gets or sets continuous manipulation haptic feedback's minimum frequency, which is the haptic frequency sent when
        ///     the object is moving/rotating at or below <see cref="MinSpeed" />/<see cref="MinAngularSpeed" />.
        /// </summary>
        public float MinFrequency
        {
            get => _minFrequency;
            set => _minFrequency = value;
        }

        /// <summary>
        ///     Gets or sets continuous manipulation haptic feedback's maximum frequency, which is the haptic frequency sent when
        ///     the object is moving/rotating at or over <see cref="MaxSpeed" />/<see cref="MaxAngularSpeed" />.
        /// </summary>
        public float MaxFrequency
        {
            get => _maxFrequency;
            set => _maxFrequency = value;
        }

        /// <summary>
        ///     Gets or sets the minimum manipulation speed, which is the object travel speed while being manipulated below which
        ///     the haptics will be sent with <see cref="MinFrequency" /> and <see cref="MinAmplitude" />.
        ///     Speeds up to <see cref="MaxSpeed" /> will send haptic feedback with frequency and amplitude values linearly
        ///     increasing up to <see cref="MaxFrequency" /> and <see cref="MaxAmplitude" />. This allows to send haptic feedback
        ///     with an intensity/frequency depending on how fast the object is being moved.
        /// </summary>
        public float MinSpeed
        {
            get => _minSpeed;
            set => _minSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the maximum manipulation speed, which is the object travel speed while being manipulated above which
        ///     the haptics will be sent with <see cref="MaxFrequency" /> and <see cref="MaxAmplitude" />.
        ///     Speeds down to <see cref="MinSpeed" /> will send haptic feedback with frequency and amplitude values linearly
        ///     decreasing down to <see cref="MinFrequency" /> and <see cref="MinAmplitude" />. This allows to send haptic feedback
        ///     with an intensity/frequency depending on how fast the object is being moved.
        /// </summary>
        public float MaxSpeed
        {
            get => _maxSpeed;
            set => _maxSpeed = value;
        }

        /// <summary>
        ///     Gets the minimum manipulation angular speed. This is the same as <see cref="MinSpeed" /> but when rotating an
        ///     object.
        /// </summary>
        public float MinAngularSpeed
        {
            get => _minAngularSpeed;
            set => _minAngularSpeed = value;
        }

        /// <summary>
        ///     Gets the maximum manipulation angular speed. This is the same as <see cref="MaxSpeed" /> but when rotating an
        ///     object.
        /// </summary>
        public float MaxAngularSpeed
        {
            get => _maxAngularSpeed;
            set => _maxAngularSpeed = value;
        }

        /// <summary>
        ///     See <see cref="ExternalRigidbody" />.
        /// </summary>
        public bool UseExternalRigidbody
        {
            get => _useExternalRigidbody;
            set => _useExternalRigidbody = value;
        }

        /// <summary>
        ///     In continuous manipulation mode, allows to get the linear/rotational speed from an external rigidbody instead of
        ///     the object being grabbed. This is useful to emulate the tension propagated by a connected physics-driven object.
        ///     For example, in a flail weapon, the grabbable object is the handle which also has the
        ///     <see cref="UxrManipulationHapticFeedback" /> component, but the physics-driven head is the object that should be
        ///     monitored for haptics to generate better results.
        /// </summary>
        public Rigidbody ExternalRigidbody
        {
            get => _externalRigidbody;
            set => _externalRigidbody = value;
        }

        /// <summary>
        ///     Gets or sets the haptic clip played when the object is grabbed.
        /// </summary>
        public UxrHapticClip HapticClipOnGrab
        {
            get => _hapticClipOnGrab;
            set => _hapticClipOnGrab = value;
        }

        /// <summary>
        ///     Gets or sets the haptic clip played when the object is placed.
        /// </summary>
        public UxrHapticClip HapticClipOnPlace
        {
            get => _hapticClipOnPlace;
            set => _hapticClipOnPlace = value;
        }

        /// <summary>
        ///     Gets or sets the haptic clip played when the object is released.
        /// </summary>
        public UxrHapticClip HapticClipOnRelease
        {
            get => _hapticClipOnRelease;
            set => _hapticClipOnRelease = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stops the haptic coroutines.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (_leftHapticsCoroutine != null)
            {
                StopCoroutine(_leftHapticsCoroutine);
                _leftHapticsCoroutine = null;
            }

            if (_rightHapticsCoroutine != null)
            {
                StopCoroutine(_rightHapticsCoroutine);
                _rightHapticsCoroutine = null;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that sends haptic clip to the left controller if the object is being grabbed and continuous manipulation
        ///     haptics are enabled.
        /// </summary>
        /// <param name="grabber">Grabber component that is currently grabbing the object</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator LeftHapticsCoroutine(UxrGrabber grabber)
        {
            while (true)
            {
                if (isActiveAndEnabled && grabber && _continuousManipulationHaptics)
                {
                    SendHapticClip(UxrHandSide.Left);
                }

                yield return new WaitForSeconds(SampleDurationSeconds);
            }
        }

        /// <summary>
        ///     Coroutine that sends haptic clip to the right controller if the object is being grabbed and continuous manipulation
        ///     haptics are enabled.
        /// </summary>
        /// <param name="grabber">Grabber component that is currently grabbing the object</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator RightHapticsCoroutine(UxrGrabber grabber)
        {
            while (true)
            {
                if (isActiveAndEnabled && grabber && _continuousManipulationHaptics)
                {
                    SendHapticClip(UxrHandSide.Right);
                }

                yield return new WaitForSeconds(SampleDurationSeconds);
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the object was grabbed. Sends haptic feedback if it's required.
        /// </summary>
        /// <param name="e">Grab event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            if (!isActiveAndEnabled || !UxrAvatar.LocalAvatar)
            {
                return;
            }

            if (e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                if (e.Grabber.Side == UxrHandSide.Left)
                {
                    _leftHapticsCoroutine = StartCoroutine(LeftHapticsCoroutine(e.Grabber));
                }
                else
                {
                    _rightHapticsCoroutine = StartCoroutine(RightHapticsCoroutine(e.Grabber));
                }

                UxrAvatar.LocalAvatarInput.SendHapticFeedback(e.Grabber.Side, _hapticClipOnGrab);
            }
        }

        /// <summary>
        ///     Called when the object was placed. Sends haptic feedback if it's required.
        /// </summary>
        /// <param name="e">Grab event parameters</param>
        protected override void OnObjectPlaced(UxrManipulationEventArgs e)
        {
            base.OnObjectPlaced(e);

            if (e.Grabber != null && e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                if (e.Grabber.Side == UxrHandSide.Left && _leftHapticsCoroutine != null)
                {
                    StopCoroutine(_leftHapticsCoroutine);
                }
                else if (e.Grabber.Side == UxrHandSide.Right && _rightHapticsCoroutine != null)
                {
                    StopCoroutine(_rightHapticsCoroutine);
                }

                if (isActiveAndEnabled)
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(e.Grabber.Side, _hapticClipOnPlace);
                }
            }
        }

        /// <summary>
        ///     Called when the object was released. Sends haptic feedback if it's required.
        /// </summary>
        /// <param name="e">Grab event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            base.OnObjectReleased(e);

            if (e.Grabber != null && e.Grabber.Avatar == UxrAvatar.LocalAvatar)
            {
                if (e.Grabber.Side == UxrHandSide.Left && _leftHapticsCoroutine != null)
                {
                    StopCoroutine(_leftHapticsCoroutine);
                }
                else if (e.Grabber.Side == UxrHandSide.Right && _rightHapticsCoroutine != null)
                {
                    StopCoroutine(_rightHapticsCoroutine);
                }

                if (isActiveAndEnabled)
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(e.Grabber.Side, _hapticClipOnRelease);
                }
            }
        }

        /// <summary>
        ///     Called after all object manipulation has been processed and potential constraints have been applied.
        ///     It is used to update the speed information.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnObjectConstraintsFinished(UxrApplyConstraintsEventArgs e)
        {
            _linearSpeed  = Vector3.Distance(_previousLocalPosition, e.Grabber.GrabbedObject.transform.localPosition) / Time.deltaTime;
            _angularSpeed = Quaternion.Angle(_previousLocalRotation, e.Grabber.GrabbedObject.transform.localRotation) / Time.deltaTime;

            _previousLocalPosition = e.Grabber.GrabbedObject.transform.localPosition;
            _previousLocalRotation = e.Grabber.GrabbedObject.transform.localRotation;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sends the continuous haptic feedback clip for a short amount of time defined by
        ///     <see cref="SampleDurationSeconds" />.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        private void SendHapticClip(UxrHandSide handSide)
        {
            if (!UxrAvatar.LocalAvatar)
            {
                return;
            }

            float speed        = _useExternalRigidbody && _externalRigidbody ? _externalRigidbody.velocity.magnitude : _linearSpeed;
            float angularSpeed = _useExternalRigidbody && _externalRigidbody ? _externalRigidbody.angularVelocity.magnitude : _angularSpeed;

            float quantityPos = _maxSpeed - _minSpeed <= 0.0f ? 0.0f : (speed - _minSpeed) / (_maxSpeed - _minSpeed);
            float quantityRot = _maxAngularSpeed - _minAngularSpeed <= 0.0f ? 0.0f : (angularSpeed - _minAngularSpeed) / (_maxAngularSpeed - _minAngularSpeed);

            if (quantityPos > 0.0f || quantityRot > 0.0f)
            {
                float frequencyPos = Mathf.Lerp(_minFrequency, _maxFrequency, Mathf.Clamp01(quantityPos));
                float amplitudePos = Mathf.Lerp(_minAmplitude, _maxAmplitude, Mathf.Clamp01(quantityPos));
                float frequencyRot = Mathf.Lerp(_minFrequency, _maxFrequency, Mathf.Clamp01(quantityRot));
                float amplitudeRot = Mathf.Lerp(_minAmplitude, _maxAmplitude, Mathf.Clamp01(quantityRot));

                UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, Mathf.Max(frequencyPos, frequencyRot), Mathf.Max(amplitudePos, amplitudeRot), SampleDurationSeconds, _hapticMixMode);
            }
        }

        #endregion

        #region Private Types & Data

        private const float SampleDurationSeconds = 0.1f;

        private Coroutine  _leftHapticsCoroutine;
        private Coroutine  _rightHapticsCoroutine;
        private Vector3    _previousLocalPosition;
        private Quaternion _previousLocalRotation;
        private float      _linearSpeed;
        private float      _angularSpeed;

        #endregion
    }
}