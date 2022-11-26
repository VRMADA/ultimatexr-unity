// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraFade.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Extensions.System.Threading;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.CameraUtils
{
    /// <summary>
    ///     Component added to a camera that allows to fade the rendered content to and from a color
    ///     by using a fullscreen quad.
    /// </summary>
    public class UxrCameraFade : UxrAvatarComponent<UxrCameraFade>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the component is currently fading.
        /// </summary>
        public bool IsFading => DrawFade;

        /// <summary>
        ///     Gets or sets the fade color used. The alpha is determined by the fade itself.
        /// </summary>
        public Color FadeColor
        {
            get => _fadeColor;
            set => _fadeColor = value;
        }

        /// <summary>
        ///     Gets or sets the layer value of the quad that is used to render the fade.
        /// </summary>
        public int QuadLayer
        {
            get => _quadLayer;
            set
            {
                _quadLayer = value;

                if (_quadObject != null)
                {
                    _quadObject.layer = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the given camera has a <see cref="UxrCameraFade" /> component. If not it is added to the camera.
        /// </summary>
        /// <param name="camera">Camera to check</param>
        /// <returns>The <see cref="UxrCameraFade" /> component which may have been added or was already present</returns>
        public static UxrCameraFade CheckAddToCamera(Camera camera)
        {
            UxrCameraFade cameraFade = camera.gameObject.GetOrAddComponent<UxrCameraFade>();

            cameraFade._fadeColor = Color.black;

            return cameraFade;
        }

        /// <summary>
        ///     Checks if the given camera has a <see cref="UxrCameraFade" /> component and a fade is currently active.
        /// </summary>
        /// <param name="camera">Camera to check</param>
        /// <returns>
        ///     True if the camera has a <see cref="UxrCameraFade" /> component attached AND a fade
        ///     currently running, false otherwise
        /// </returns>
        public static bool HasCameraFadeActive(Camera camera)
        {
            UxrCameraFade cameraFade = camera.gameObject.GetComponent<UxrCameraFade>();

            return cameraFade != null && cameraFade.DrawFade;
        }

        /// <summary>
        ///     Starts a fade over time on the given camera. The camera will fade out to a given color and
        ///     then fade in from that color again.
        ///     This is the static helper method that can be used to perform everything in just a single static call.
        /// </summary>
        /// <param name="camera">The camera to perform the fade on</param>
        /// <param name="fadeOutDurationSeconds">Number of seconds of the initial fade-out</param>
        /// <param name="fadeInDurationSeconds">Number of seconds of the fade-in</param>
        /// <param name="fadeColor">The color the component fades out to and fades in from</param>
        /// <param name="fadeOutFinishedCallback">Optional callback executed right after the fade out finished</param>
        /// <param name="fadeInFinishedCallback">Optional callback executed right after the fade in finished</param>
        public static void StartFade(Camera camera,
                                     float  fadeOutDurationSeconds,
                                     float  fadeInDurationSeconds,
                                     Color  fadeColor,
                                     Action fadeOutFinishedCallback = null,
                                     Action fadeInFinishedCallback  = null)
        {
            UxrCameraFade cameraFade = CheckAddToCamera(camera);
            cameraFade.StartFade(fadeOutDurationSeconds, fadeInDurationSeconds, fadeColor, fadeOutFinishedCallback, fadeInFinishedCallback);
        }

        /// <summary>
        ///     Starts a fade over time on the camera that has this component. The camera will fade out to a given color and
        ///     then fade in from that color again.
        ///     For a coroutine-friendly way of fading check StartFadeCoroutine().
        /// </summary>
        /// <param name="fadeOutDurationSeconds">Number of seconds of the initial fade-out</param>
        /// <param name="fadeInDurationSeconds">Number of seconds of the fade-in</param>
        /// <param name="fadeColor">The color the component fades out to and fades in from</param>
        /// <param name="fadeOutFinishedCallback">Optional callback that is called just after the fade out finished</param>
        /// <param name="fadeInFinishedCallback">Optional callback that is called just after the fade in finished</param>
        public void StartFade(float  fadeOutDurationSeconds,
                              float  fadeInDurationSeconds,
                              Color  fadeColor,
                              Action fadeOutFinishedCallback = null,
                              Action fadeInFinishedCallback  = null)
        {
            if (DrawFade)
            {
                Debug.LogWarning("A fade was requested while one already being active. Some callbacks may not be called correctly. ");
            }

            _fadeColor = fadeColor;

            DrawFade         = true;
            _fadeOutFinished = false;
            _fadeTimer       = fadeOutDurationSeconds + fadeInDurationSeconds;
            _fadeOutDuration = fadeOutDurationSeconds;
            _fadeInDuration  = fadeInDurationSeconds;

            _fadeOutFinishedCallback = fadeOutFinishedCallback;
            _fadeInFinishedCallback  = fadeInFinishedCallback;

            _fadeCurrentColor   = _fadeColor;
            _fadeCurrentColor.a = 0.0f;

            FadeMaterial.color = _fadeCurrentColor;
        }

        /// <summary>
        ///     Enables the camera fade color. It will draw an overlay with the given color until <see cref="DisableFadeColor" />
        ///     is called.
        /// </summary>
        /// <param name="color">The color to draw the overlay with</param>
        /// <param name="quantity">The quantity [0.0, 1.0] of the fade</param>
        public void EnableFadeColor(Color color, float quantity)
        {
            DrawFade   = true;
            _fadeTimer = -1.0f;

            _fadeCurrentColor   =  color;
            _fadeCurrentColor.a *= quantity;

            FadeMaterial.color = _fadeCurrentColor;
        }

        /// <summary>
        ///     Disables the camera fade rendering.
        /// </summary>
        public void DisableFadeColor()
        {
            DrawFade   = false;
            _fadeTimer = -1.0f;
        }

        /// <summary>
        ///     Starts a fade over time using an async operation.
        /// </summary>
        /// <param name="ct">The cancellation token to cancel the async operation</param>
        /// <param name="fadeSeconds">The fade duration in seconds</param>
        /// <param name="startColor">The fade start color</param>
        /// <param name="endColor">The fade end color</param>
        public async Task FadeAsync(CancellationToken ct, float fadeSeconds, Color startColor, Color endColor)
        {
            await TaskExt.Loop(ct,
                               fadeSeconds,
                               t =>
                               {
                                   DrawFade           = true;
                                   _fadeCurrentColor  = Color.Lerp(startColor, endColor, t);
                                   FadeMaterial.color = _fadeCurrentColor;
                               },
                               UxrEasing.Linear,
                               true);

            if (Mathf.Approximately(endColor.a, 0.0f))
            {
                DrawFade = false;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes all internal data.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CheckInitialize();
        }

        /// <summary>
        ///     Updates the fade (StartFade version) and calls all callbacks when they need to be triggered.
        /// </summary>
        private void Update()
        {
            if (_fadeTimer > 0.0f)
            {
                _fadeTimer -= Time.deltaTime;

                if (_fadeTimer <= 0.0f)
                {
                    DrawFade = false;

                    _fadeInFinishedCallback?.Invoke();
                }
                else if (_fadeTimer < _fadeInDuration && _fadeOutFinished == false)
                {
                    _fadeOutFinished    = true;
                    _fadeCurrentColor.a = 1.0f * _fadeColor.a;

                    _fadeOutFinishedCallback?.Invoke();
                }
                else
                {
                    if (_fadeTimer < _fadeInDuration)
                    {
                        _fadeCurrentColor.a = Mathf.Clamp01(_fadeTimer / _fadeInDuration) * _fadeColor.a;
                    }
                    else
                    {
                        _fadeCurrentColor.a = Mathf.Clamp01(1.0f - (_fadeTimer - _fadeInDuration) / _fadeOutDuration) * _fadeColor.a;
                    }
                }

                FadeMaterial.color = _fadeCurrentColor;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that fades the screen over time. It can be used to be yielded externally from another coroutine.
        ///     <see cref="FadeAsync" /> is provided as the async alternative.
        /// </summary>
        /// <param name="fadeSeconds">Seconds it will take to execute the fade</param>
        /// <param name="startColor">Start color value</param>
        /// <param name="endColor">End color value</param>
        /// <returns>Coroutine IEnumerator</returns>
        public IEnumerator StartFadeCoroutine(float fadeSeconds, Color startColor, Color endColor)
        {
            if (DrawFade)
            {
                // Debug.LogWarning("A fade coroutine was requested while a fade already being active");
            }

            yield return this.LoopCoroutine(fadeSeconds,
                                            t =>
                                            {
                                                DrawFade           = true;
                                                _fadeCurrentColor  = Color.Lerp(startColor, endColor, t);
                                                FadeMaterial.color = _fadeCurrentColor;
                                            },
                                            UxrEasing.Linear,
                                            true);

            if (Mathf.Approximately(endColor.a, 0.0f))
            {
                DrawFade = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the component if necessary.
        /// </summary>
        private void CheckInitialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                _camera      = Avatar.CameraComponent;
                _fadeTimer   = -1.0f;

                DrawFade = false;
                CreateCameraQuad();
            }
        }

        /// <summary>
        ///     Creates the quad to render in front of the camera.
        /// </summary>
        private void CreateCameraQuad()
        {
            _quadObject = new GameObject("Fade");
            _quadObject.transform.SetParent(transform);
            _quadObject.transform.localPosition    = Vector3.forward * (_camera.nearClipPlane + 0.01f);
            _quadObject.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            _quadObject.layer                      = _quadLayer;

            MeshFilter   meshFilter   = _quadObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = _quadObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = MeshExt.CreateQuad(2.0f);
            _fadeMaterial   = new Material(ShaderExt.UnlitOverlayFade);

            meshRenderer.sharedMaterial = _fadeMaterial;

            _quadObject.SetActive(_drawFade);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the material used to draw the fade.
        /// </summary>
        private Material FadeMaterial
        {
            get
            {
                if (_fadeMaterial == null)
                {
                    _fadeMaterial = new Material(ShaderExt.UnlitTransparentColorNoDepthTest);
                }

                return _fadeMaterial;
            }
        }

        /// <summary>
        ///     Gets or sets whether to draw the fade.
        /// </summary>
        private bool DrawFade
        {
            get => _drawFade;
            set
            {
                _drawFade = value;

                if (_quadObject != null)
                {
                    _quadObject.SetActive(_drawFade);
                }
            }
        }

        private Color      _fadeColor = Color.black;
        private bool       _initialized;
        private Camera     _camera;
        private Material   _fadeMaterial;
        private bool       _drawFade;
        private bool       _fadeOutFinished;
        private float      _fadeTimer;
        private float      _fadeInDuration;
        private float      _fadeOutDuration;
        private Color      _fadeCurrentColor;
        private GameObject _quadObject;
        private int        _quadLayer = 1;

        private Action _fadeOutFinishedCallback;
        private Action _fadeInFinishedCallback;

        #endregion
    }
}