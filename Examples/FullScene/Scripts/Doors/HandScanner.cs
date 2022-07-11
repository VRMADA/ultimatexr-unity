// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandScanner.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Animation.Materials;
using UltimateXR.Audio;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Doors
{
    /// <summary>
    ///     Component that handles the hand scanning required to open an <see cref="ArmoredDoor" />.
    /// </summary>
    public class HandScanner : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]                     private ArmoredDoor    _armoredDoor;
        [SerializeField]                     private Renderer       _validLight;
        [SerializeField]                     private Renderer       _invalidLight;
        [SerializeField]                     private Renderer       _scannerBeam;
        [SerializeField]                     private Vector3        _scannerBeamTopLocalPos;
        [SerializeField]                     private Vector3        _scannerBeamBottomLocalPos;
        [SerializeField]                     private int            _beamCount      = 5;
        [SerializeField] [Range(0.0f, 1.0f)] private float          _beamTrailDelay = 0.1f;
        [SerializeField]                     private UxrEasing      _beamEeasing    = UxrEasing.EaseInOutQuint;
        [SerializeField]                     private Vector3        _beamMaxScale   = Vector3.one;
        [SerializeField]                     private Renderer       _handRendererLeft;
        [SerializeField]                     private Renderer       _handRendererRight;
        [SerializeField]                     private UxrHandSide    _defaultHandSide = UxrHandSide.Right;
        [SerializeField]                     private BoxCollider    _handBoxValidPos;
        [SerializeField]                     private float          _scanSeconds   = 1.5f;
        [SerializeField]                     private float          _resultSeconds = 2.0f;
        [SerializeField]                     private UxrAudioSample _audioScan;
        [SerializeField]                     private UxrAudioSample _audioError;
        [SerializeField]                     private UxrAudioSample _audioOk;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called right after a hand was scanned. Parameters are the avatar that was scanned and if the scan granted
        ///     access.
        /// </summary>
        public event Action<UxrAvatar, UxrHandSide, bool> HandScanned;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _colorValid   = _validLight.sharedMaterial.GetColor(UxrConstants.Shaders.StandardColorVarName);
            _colorInvalid = _invalidLight.sharedMaterial.GetColor(UxrConstants.Shaders.StandardColorVarName);
            _handSide     = _defaultHandSide;
            _beamScale    = _scannerBeam.transform.localScale;

            _beams.Add(_scannerBeam);

            for (int i = 0; i < _beamCount - 1; ++i)
            {
                _beams.Add(Instantiate(_scannerBeam.gameObject, _scannerBeam.transform.position, _scannerBeam.transform.rotation, _scannerBeam.transform.parent).GetComponent<Renderer>());
            }
        }

        /// <summary>
        ///     Sets the default scanning state.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _scanReady            = true;
            _validLight.enabled   = false;
            _invalidLight.enabled = false;

            _handRendererLeft.enabled         = _defaultHandSide == UxrHandSide.Left;
            _handRendererRight.enabled        = _defaultHandSide == UxrHandSide.Right;
            _handRendererLeft.material.color  = ColorExt.ColorAlpha(_handRendererLeft.material.color,  _defaultHandSide == UxrHandSide.Left ? 1.0f : 0.0f);
            _handRendererRight.material.color = ColorExt.ColorAlpha(_handRendererRight.material.color, _defaultHandSide == UxrHandSide.Right ? 1.0f : 0.0f);

            EnableBeams(false);
        }

        /// <summary>
        ///     Disables the component.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnEnable();

            _scanReady            = true;
            _validLight.enabled   = false;
            _invalidLight.enabled = false;
            EnableBeams(false);
        }

        /// <summary>
        ///     Updates the component. Performs the scanning process.
        /// </summary>
        private void Update()
        {
            if (UxrAvatar.LocalAvatar == null || !_scanReady)
            {
                return;
            }

            // Update scanning & beam

            if (_scanTimer < 0.0f)
            {
                if (_armoredDoor != null && _armoredDoor.IsOpen)
                {
                    // If we are controlling an armored door and it is already open, ignore the hands.
                }
                else
                {
                    // Waiting for hand to be scanned. Look for hand:

                    if (UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Left).position.IsInsideBox(_handBoxValidPos))
                    {
                        _scanTimer = 0.0f;
                        _handSide  = UxrHandSide.Left;
                        _audioScan.Play(transform.position);
                    }
                    else if (UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Right).position.IsInsideBox(_handBoxValidPos))
                    {
                        _scanTimer = 0.0f;
                        _handSide  = UxrHandSide.Right;
                        _audioScan.Play(transform.position);
                    }
                }
            }
            else
            {
                if (UxrAvatar.LocalAvatar.GetHandBone(_handSide).position.IsInsideBox(_handBoxValidPos))
                {
                    if (!UxrAvatar.LocalAvatar.GetHandBone(UxrUtils.GetOppositeSide(_handSide)).position.IsInsideBox(_handBoxValidPos))
                    {
                        // Hand is scanning

                        _scanTimer += Time.deltaTime;
                        EnableBeams(true);

                        for (int i = 0; i < _beams.Count; ++i)
                        {
                            float beamStartTime = i / (_beams.Count == 1 ? 1.0f : _beams.Count - 1.0f) * _beamTrailDelay * _scanSeconds;
                            float beamDuration  = _scanSeconds - _beamTrailDelay;
                            float t             = Mathf.Clamp01((_scanTimer - beamStartTime) / beamDuration);
                            float tScale        = 1.0f - Mathf.Abs(t - 0.5f) * 2.0f;
                            _beams[i].transform.localPosition = Vector3.Lerp(_scannerBeamTopLocalPos, _scannerBeamBottomLocalPos, UxrInterpolator.GetInterpolationFactor(t, _beamEeasing));
                            _beams[i].transform.localScale    = Vector3.Lerp(_beamScale,              _beamMaxScale,              Mathf.Pow(tScale, 8.0f));
                        }

                        if (_scanTimer > _scanSeconds)
                        {
                            ProcessScanResult(UxrAvatar.LocalAvatar, _handSide, true);

                            if (_armoredDoor != null)
                            {
                                _armoredDoor.OpenDoor();
                            }
                        }
                    }
                    else
                    {
                        // Opposite hand got in. Aborting.
                        ProcessScanResult(UxrAvatar.LocalAvatar, _handSide, false);
                    }
                }
                else
                {
                    // Scanning hand got out. Aborting.
                    ProcessScanResult(UxrAvatar.LocalAvatar, _handSide, false);
                }
            }

            // Update scan side

            if (_handSide == UxrHandSide.Left)
            {
                if (_handRendererRight.enabled)
                {
                    float rightAlpha = _handRendererRight.material.color.a - Time.deltaTime * HandAlphaSwitchSpeed;
                    _handRendererRight.material.color = ColorExt.ColorAlpha(_handRendererRight.material.color, Mathf.Clamp01(rightAlpha));

                    if (rightAlpha < 0.0f)
                    {
                        _handRendererRight.enabled = false;
                    }
                }

                if (!_handRendererLeft.enabled)
                {
                    _handRendererLeft.enabled = true;
                }

                _handRendererLeft.material.color = ColorExt.ColorAlpha(_handRendererLeft.material.color, Mathf.Clamp01(_handRendererLeft.material.color.a + Time.deltaTime * HandAlphaSwitchSpeed));
            }
            else
            {
                if (_handRendererLeft.enabled)
                {
                    float leftAlpha = _handRendererLeft.material.color.a - Time.deltaTime * HandAlphaSwitchSpeed;
                    _handRendererLeft.material.color = ColorExt.ColorAlpha(_handRendererLeft.material.color, Mathf.Clamp01(leftAlpha));

                    if (leftAlpha < 0.0f)
                    {
                        _handRendererLeft.enabled = false;
                    }
                }

                if (!_handRendererRight.enabled)
                {
                    _handRendererRight.enabled = true;
                }

                _handRendererRight.material.color = ColorExt.ColorAlpha(_handRendererRight.material.color, Mathf.Clamp01(_handRendererRight.material.color.a + Time.deltaTime * HandAlphaSwitchSpeed));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Enables/disables the scanning beams.
        /// </summary>
        /// <param name="enable">Whether the beams should be enabled</param>
        private void EnableBeams(bool enable)
        {
            _beams.ForEach(r => r.enabled = enable);
        }

        /// <summary>
        ///     Processes a scan result.
        /// </summary>
        /// <param name="avatar">Avatar that was scanned</param>
        /// <param name="handSide">Which hand was scanned</param>
        /// <param name="isValid">Whether the access was granted</param>
        private void ProcessScanResult(UxrAvatar avatar, UxrHandSide handSide, bool isValid)
        {
            EnableBeams(false);

            _scanReady = false;
            _scanTimer = -1.0f;

            if (isValid)
            {
                _audioOk.Play(transform.position);
                _validLight.enabled = true;
                UxrAnimatedMaterial.AnimateBlinkColor(_validLight.gameObject,
                                                      UxrConstants.Shaders.StandardColorVarName,
                                                      _colorValid.WithAlpha(0.0f),
                                                      _colorValid,
                                                      UxrAnimatedMaterial.DefaultBlinkFrequency,
                                                      _resultSeconds,
                                                      UxrMaterialMode.InstanceOnly,
                                                      () =>
                                                      {
                                                          _scanReady          = true;
                                                          _validLight.enabled = false;
                                                      });
            }
            else
            {
                _audioError.Play(transform.position);
                _invalidLight.enabled = true;
                UxrAnimatedMaterial.AnimateBlinkColor(_invalidLight.gameObject,
                                                      UxrConstants.Shaders.StandardColorVarName,
                                                      _colorInvalid.WithAlpha(0.0f),
                                                      _colorInvalid,
                                                      UxrAnimatedMaterial.DefaultBlinkFrequency,
                                                      _resultSeconds,
                                                      UxrMaterialMode.InstanceOnly,
                                                      () =>
                                                      {
                                                          _scanReady            = true;
                                                          _invalidLight.enabled = false;
                                                      });
            }

            HandScanned?.Invoke(avatar, handSide, isValid);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Controls how fast the hand image will switch from one side to the other when a different hand than the currently
        ///     shown is placed on the scanner.
        /// </summary>
        private const float HandAlphaSwitchSpeed = 4.0f;

        private readonly List<Renderer> _beams = new List<Renderer>();

        private bool        _scanReady = true;
        private float       _scanTimer = -1.0f;
        private Vector3     _beamScale;
        private UxrHandSide _handSide;
        private Color       _colorValid;
        private Color       _colorInvalid;

        #endregion
    }
}