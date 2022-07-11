// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFaceGestures.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation.Avatars
{
    /// <summary>
    ///     Allows to simulate facial gestures like eyes movement/blinking and mouth using
    ///     the microphone input.
    /// </summary>
    public class UxrFaceGestures : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [Header("Blinking")] [SerializeField] private bool      _blinkEyes = true;
        [SerializeField]                      private Transform _eyeLidTopLeft;
        [SerializeField]                      private Transform _eyeLidTopRight;
        [SerializeField]                      private Transform _eyeLidBottomLeft;
        [SerializeField]                      private Transform _eyeLidBottomRight;
        [SerializeField]                      private Vector3   _eyeBlinkTopLocalAxis    = Vector3.right;
        [SerializeField]                      private float     _eyeBlinkTopClosedAngle  = 30.0f;
        [SerializeField]                      private Vector3   _eyeBlinkBottomLocalAxis = Vector3.right;
        [SerializeField]                      private float     _eyeBlinkBottomClosedAngle;
        [SerializeField]                      private float     _eyeBlinkDurationMin = 0.05f;
        [SerializeField]                      private float     _eyeBlinkDurationMax = 0.1f;
        [SerializeField]                      private float     _eyeBlinkIntervalMin = 1.0f;
        [SerializeField]                      private float     _eyeBlinkIntervalMax = 5.0f;

        [Header("Eye movement")] [SerializeField] private bool      _moveEyes = true;
        [SerializeField]                          private Transform _eyeLeft;
        [SerializeField]                          private Transform _eyeRight;
        [SerializeField] [Range(0.0f, 90.0f)]     private float     _eyeLookStraightAngleRange = 3.0f;
        [SerializeField] [Range(0.0f, 90.0f)]     private float     _eyeLookEdgeAngleMin       = 15.0f;
        [SerializeField] [Range(0.0f, 90.0f)]     private float     _eyeLookEdgeAngleMax       = 45.0f;
        [SerializeField] [Range(0.0f, 1.0f)]      private float     _eyeLookEdgeProbability    = 0.3f;
        [SerializeField]                          private float     _eyeSwitchLookDurationMin  = 0.05f;
        [SerializeField]                          private float     _eyeSwitchLookDurationMax  = 0.1f;
        [SerializeField]                          private float     _eyeSwitchLookIntervalMin  = 0.25f;
        [SerializeField]                          private float     _eyeSwitchLookIntervalMax  = 2.0f;

        [Header("Mouth movement")] [SerializeField] private bool      _moveMouthUsingMic       = true;
        [SerializeField]                            private float     _microphoneAmplification = 1.0f;
        [SerializeField]                            private Transform _mouthOpenTransform;
        [SerializeField]                            private Vector3   _mouthOpenLocalAxis = Vector3.right;
        [SerializeField]                            private float     _mouthClosedAngle;
        [SerializeField]                            private float     _mouthMaxOpenAngle = 5.0f;
        [SerializeField] [Range(0.0f, 0.2f)]        private float     _mouthRotationDamp = 0.05f;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            StartCoroutine(BlinkCoroutine());
            StartCoroutine(SwitchLookCoroutine());

            if (_moveMouthUsingMic && Microphone.devices.Length > 0)
            {
                _microphoneClipRecord = Microphone.Start(null, true, 10, 44100);
            }
        }

        /// <summary>
        ///     Releases the microphone resource if it's in use.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (Microphone.IsRecording(null) && _moveMouthUsingMic)
            {
                Microphone.End(null);
            }
        }

        /// <summary>
        ///     Additional initialization.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (_eyeLidTopLeft)
            {
                _localRotEyeLidTopLeft = _eyeLidTopLeft.localRotation;
            }
            if (_eyeLidTopRight)
            {
                _localRotEyeLidTopRight = _eyeLidTopRight.localRotation;
            }
            if (_eyeLidBottomLeft)
            {
                _localRotEyeLidBottomLeft = _eyeLidBottomLeft.localRotation;
            }
            if (_eyeLidBottomLeft)
            {
                _localRotEyeLidBottomRight = _eyeLidBottomRight.localRotation;
            }

            if (_eyeLeft)
            {
                _localRotEyeLeft = _eyeLeft.localRotation;
            }
            if (_eyeRight)
            {
                _localRotEyeRight = _eyeRight.localRotation;
            }

            if (_mouthOpenTransform)
            {
                _localRotMouth = _mouthOpenTransform.localRotation;
            }
        }

        /// <summary>
        ///     Updates the mouth if the microphone is being used.
        /// </summary>
        private void Update()
        {
            if (Microphone.IsRecording(null) && _mouthOpenTransform && _moveMouthUsingMic)
            {
                _mouthAngle                       = Mathf.SmoothDampAngle(_mouthAngle, Mathf.LerpAngle(_mouthClosedAngle, _mouthMaxOpenAngle, GetMicrophoneMaxLevel()), ref _mouthAngleDampSpeed, _mouthRotationDamp);
                _mouthOpenTransform.localRotation = _localRotMouth * Quaternion.AngleAxis(_mouthAngle, _mouthOpenLocalAxis);
            }
            else if (_mouthOpenTransform)
            {
                _mouthOpenTransform.localRotation = _localRotMouth;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Blinking coroutine.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator BlinkCoroutine()
        {
            while (true)
            {
                // Wait until next blink

                yield return new WaitForSeconds(Random.Range(_eyeBlinkIntervalMin, _eyeBlinkIntervalMax));

                if (_blinkEyes == false)
                {
                    continue;
                }

                // Start blink with random duration

                float blinkDuration = Random.Range(_eyeBlinkDurationMin, _eyeBlinkDurationMax);
                float startTime     = Time.time;

                // Close

                while (Time.time - startTime < blinkDuration * 0.5f)
                {
                    float t = Mathf.Clamp01((Time.time - startTime) / (blinkDuration * 0.5f));

                    Quaternion rotationTop    = Quaternion.AngleAxis(_eyeBlinkTopClosedAngle * t,    _eyeBlinkTopLocalAxis);
                    Quaternion rotationBottom = Quaternion.AngleAxis(_eyeBlinkBottomClosedAngle * t, _eyeBlinkBottomLocalAxis);

                    if (_eyeLidTopLeft)
                    {
                        _eyeLidTopLeft.localRotation = _localRotEyeLidTopLeft * rotationTop;
                    }
                    if (_eyeLidTopRight)
                    {
                        _eyeLidTopRight.localRotation = _localRotEyeLidTopRight * rotationTop;
                    }
                    if (_eyeLidBottomLeft)
                    {
                        _eyeLidBottomLeft.localRotation = _localRotEyeLidBottomLeft * rotationBottom;
                    }
                    if (_eyeLidBottomRight)
                    {
                        _eyeLidBottomRight.localRotation = _localRotEyeLidBottomRight * rotationBottom;
                    }

                    yield return null;
                }

                Quaternion rotationClosedTop    = Quaternion.AngleAxis(_eyeBlinkTopClosedAngle,    _eyeBlinkTopLocalAxis);
                Quaternion rotationClosedBottom = Quaternion.AngleAxis(_eyeBlinkBottomClosedAngle, _eyeBlinkBottomLocalAxis);

                if (_eyeLidTopLeft)
                {
                    _eyeLidTopLeft.localRotation = _localRotEyeLidTopLeft * rotationClosedTop;
                }
                if (_eyeLidTopRight)
                {
                    _eyeLidTopRight.localRotation = _localRotEyeLidTopRight * rotationClosedTop;
                }
                if (_eyeLidBottomLeft)
                {
                    _eyeLidBottomLeft.localRotation = _localRotEyeLidBottomLeft * rotationClosedBottom;
                }
                if (_eyeLidBottomRight)
                {
                    _eyeLidBottomRight.localRotation = _localRotEyeLidBottomRight * rotationClosedBottom;
                }

                yield return null;

                // Open

                startTime = Time.time;

                while (Time.time - startTime < blinkDuration * 0.5f)
                {
                    float t = 1.0f - Mathf.Clamp01((Time.time - startTime) / (blinkDuration * 0.5f));

                    Quaternion rotationTop    = Quaternion.AngleAxis(_eyeBlinkTopClosedAngle * t,    _eyeBlinkTopLocalAxis);
                    Quaternion rotationBottom = Quaternion.AngleAxis(_eyeBlinkBottomClosedAngle * t, _eyeBlinkBottomLocalAxis);

                    if (_eyeLidTopLeft)
                    {
                        _eyeLidTopLeft.localRotation = _localRotEyeLidTopLeft * rotationTop;
                    }
                    if (_eyeLidTopRight)
                    {
                        _eyeLidTopRight.localRotation = _localRotEyeLidTopRight * rotationTop;
                    }
                    if (_eyeLidBottomLeft)
                    {
                        _eyeLidBottomLeft.localRotation = _localRotEyeLidBottomLeft * rotationBottom;
                    }
                    if (_eyeLidBottomRight)
                    {
                        _eyeLidBottomRight.localRotation = _localRotEyeLidBottomRight * rotationBottom;
                    }

                    yield return null;
                }

                if (_eyeLidTopLeft)
                {
                    _eyeLidTopLeft.localRotation = _localRotEyeLidTopLeft;
                }
                if (_eyeLidTopRight)
                {
                    _eyeLidTopRight.localRotation = _localRotEyeLidTopRight;
                }
                if (_eyeLidBottomLeft)
                {
                    _eyeLidBottomLeft.localRotation = _localRotEyeLidBottomLeft;
                }
                if (_eyeLidBottomRight)
                {
                    _eyeLidBottomRight.localRotation = _localRotEyeLidBottomRight;
                }
            }
        }

        /// <summary>
        ///     Coroutine that randomly switches where the eyes are looking.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator SwitchLookCoroutine()
        {
            while (true)
            {
                // Wait until next switch

                yield return new WaitForSeconds(Random.Range(_eyeSwitchLookIntervalMin, _eyeSwitchLookIntervalMax));

                if (_moveEyes == false)
                {
                    if (_eyeLeft)
                    {
                        _eyeLeft.localRotation = _localRotEyeLeft;
                    }
                    if (_eyeRight)
                    {
                        _eyeRight.localRotation = _localRotEyeRight;
                    }
                    continue;
                }

                // Start switch with random duration

                float switchDuration = Random.Range(_eyeSwitchLookDurationMin, _eyeSwitchLookDurationMax);
                float startTime      = Time.time;

                // Rotate

                Quaternion rotation = Quaternion.identity;

                if (Random.value < _eyeLookEdgeProbability)
                {
                    rotation = Quaternion.RotateTowards(Quaternion.identity, Random.rotation, Random.Range(_eyeLookEdgeAngleMin, _eyeLookEdgeAngleMax));
                }
                else
                {
                    rotation = Quaternion.RotateTowards(Quaternion.identity, Random.rotation, Random.Range(0.0f, _eyeLookStraightAngleRange));
                }

                Quaternion rotLeft  = _eyeLeft.localRotation;
                Quaternion rotRight = _eyeRight.localRotation;

                while (Time.time - startTime < switchDuration)
                {
                    float t = Mathf.Clamp01((Time.time - startTime) / switchDuration);

                    if (_eyeLeft)
                    {
                        _eyeLeft.localRotation = Quaternion.Slerp(rotLeft, _localRotEyeLeft * rotation, t);
                    }
                    if (_eyeRight)
                    {
                        _eyeRight.localRotation = Quaternion.Slerp(rotRight, _localRotEyeRight * rotation, t);
                    }

                    yield return null;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to get the current microphone output level.
        /// </summary>
        /// <returns>
        ///     Microphone output level, approximately in the [0.0, 1.0] range but it's not clamped and the actual range is
        ///     undefined.
        /// </returns>
        private float GetMicrophoneMaxLevel()
        {
            float   maxLevel    = 0;
            float[] waveData    = new float[MicrophoneSampleWindow];
            int     micPosition = Microphone.GetPosition(null) - (MicrophoneSampleWindow + 1);

            if (micPosition < 0)
            {
                return 0.0f;
            }

            _microphoneClipRecord.GetData(waveData, micPosition);

            for (int i = 0; i < MicrophoneSampleWindow; ++i)
            {
                float wavePeak = waveData[i] * waveData[i];

                if (maxLevel < wavePeak)
                {
                    maxLevel = wavePeak;
                }
            }

            return maxLevel * 1024.0f * _microphoneAmplification;
        }

        #endregion

        #region Private Types & Data

        private const int MicrophoneSampleWindow = 128;

        private Quaternion _localRotEyeLidTopLeft;
        private Quaternion _localRotEyeLidTopRight;
        private Quaternion _localRotEyeLidBottomLeft;
        private Quaternion _localRotEyeLidBottomRight;

        private Quaternion _localRotEyeLeft;
        private Quaternion _localRotEyeRight;

        private Quaternion _localRotMouth;
        private float      _mouthAngle;
        private float      _mouthAngleDampSpeed;
        private AudioClip  _microphoneClipRecord;

        #endregion
    }
}