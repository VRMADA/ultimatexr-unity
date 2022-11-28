// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCompass.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.Guides
{
    /// <summary>
    ///     Compass component that assists the user by giving visual hints to know where to look or the action to perform.
    ///     It will show an arrow in front of the view that will help getting the target into sight.
    ///     When the target gets into sight it can optionally show an action icon:
    ///     <list type="bullet">
    ///         <item>Location: To let the user know where to move next</item>
    ///         <item>Grab: To let the user know an object should be grabbed</item>
    ///         <item>Look: To focus attention on an object</item>
    ///         <item>Use: To let the user know an operation should be performed on an object</item>
    ///     </list>
    /// </summary>
    /// <remarks>
    ///     Since the compass is a <see cref="UxrSingleton{T}" />, it is unique and can be invoked from any point using
    ///     UxrCompass.Instance.
    /// </remarks>
    public class UxrCompass : UxrSingleton<UxrCompass>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float        _distanceToCamera = 1.0f;
        [SerializeField] private Transform    _focusedObjectTarget;
        [SerializeField] private Transform    _compassArrowPivot;
        [SerializeField] private Renderer     _compassArrowRenderer;
        [SerializeField] private Transform    _transitionArrow;
        [SerializeField] private GameObject   _rootOnScreenIcons;
        [SerializeField] private Transform    _iconLocationPivot;
        [SerializeField] private Transform    _iconLocationBottom;
        [SerializeField] private MeshRenderer _iconLocationRenderer;
        [SerializeField] private Transform    _iconLookPivot;
        [SerializeField] private MeshRenderer _iconLookRenderer;
        [SerializeField] private Transform    _iconGrabPivot;
        [SerializeField] private MeshRenderer _iconGrabRenderer;
        [SerializeField] private Transform    _iconUsePivot;
        [SerializeField] private MeshRenderer _iconUseRenderer;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the compass is currently focused on an object.
        /// </summary>
        public bool HasTarget => _focusedObjectTarget != null || _targetIsRawPos;

        /// <summary>
        ///     Gets the target's <see cref="Transform" />.
        /// </summary>
        public Transform TargetTransform => _targetHint != null ? _targetHint.GetTransform(this) : _focusedObjectTarget;

        /// <summary>
        ///     Gets the target's position.
        /// </summary>
        public Vector3 TargetPosition
        {
            get
            {
                if (_targetIsRawPos)
                {
                    return _rawTargetPos;
                }

                return TargetTransform != null ? TargetTransform.position : Vector3.zero;
            }
        }

        /// <summary>
        ///     Gets or sets the current display mode.
        /// </summary>
        public UxrCompassDisplayMode DisplayMode { get; set; } = UxrCompassDisplayMode.Location;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sets the current target.
        /// </summary>
        /// <param name="target">New target or null to stop</param>
        /// <param name="displayMode">The display mode</param>
        /// <param name="iconScale">The icon size multiplier</param>
        public void SetTarget(Transform target, UxrCompassDisplayMode displayMode = UxrCompassDisplayMode.OnlyCompass, float iconScale = 1.0f)
        {
            DisplayMode          = displayMode;
            _focusedObjectTarget = target;
            _targetStartTime     = Time.unscaledTime;
            _onScreenStartTime   = Time.unscaledTime;
            _targetIsRawPos      = false;
            _targetHint          = target != null ? target.gameObject.GetComponent<UxrCompassTargetHint>() : null;
            _iconScale           = iconScale;
            _isTemporary         = false;
        }

        /// <summary>
        ///     Sets the current target. When the object gets into sight it will show the icon described by
        ///     <paramref name="displayMode" /> during a limited amount of time (<see cref="TemporaryDurationSeconds" />). The
        ///     timer is reset each time the object gets out of sight.
        /// </summary>
        /// <param name="target">New target or null to stop</param>
        /// <param name="displayMode">The display mode</param>
        /// <param name="iconScale">The icon size multiplier</param>
        public void SetTargetTemporary(Transform target, UxrCompassDisplayMode displayMode = UxrCompassDisplayMode.OnlyCompass, float iconScale = 1.0f)
        {
            SetTarget(target, displayMode, iconScale);
            _isTemporary = true;
        }

        /// <summary>
        ///     Sets the current target.
        /// </summary>
        /// <param name="position">The target position</param>
        /// <param name="displayMode">The display mode</param>
        /// <param name="iconScale">The icon size multiplier</param>
        public void SetTarget(Vector3 position, UxrCompassDisplayMode displayMode = UxrCompassDisplayMode.OnlyCompass, float iconScale = 1.0f)
        {
            DisplayMode          = displayMode;
            _focusedObjectTarget = null;
            _targetStartTime     = Time.unscaledTime;
            _onScreenStartTime   = Time.unscaledTime;
            _targetIsRawPos      = true;
            _rawTargetPos        = position;
            _targetHint          = null;
            _iconScale           = iconScale;
            _isTemporary         = false;
        }

        /// <summary>
        ///     Sets the current target. When the object gets into sight it will show the icon described by
        ///     <paramref name="displayMode" /> during a limited amount of time (<see cref="TemporaryDurationSeconds" />). The
        ///     timer is reset each time the object gets out of sight.
        /// </summary>
        /// <param name="position">The target position</param>
        /// <param name="displayMode">The display mode</param>
        /// <param name="iconScale">The icon size multiplier</param>
        public void SetTargetTemporary(Vector3 position, UxrCompassDisplayMode displayMode = UxrCompassDisplayMode.OnlyCompass, float iconScale = 1.0f)
        {
            SetTarget(position, displayMode, iconScale);
            _isTemporary = true;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the compass.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _compassArrowPivot.gameObject.SetActive(false);
            _transitionArrow.gameObject.SetActive(false);
            _rootOnScreenIcons.SetActive(false);

            _initialIconScales = new Dictionary<MeshRenderer, Vector3>();

            foreach (MeshRenderer iconRenderer in IconRenderers)
            {
                _initialIconScales.Add(iconRenderer, iconRenderer.transform.localScale);
            }
        }

        /// <summary>
        ///     Updates the compass.
        /// </summary>
        private void Update()
        {
            if (!HasTarget)
            {
                // No object focused anymore
                if (_targetFocused)
                {
                    _targetFocused = false;

                    if (_coroutineArrowTransition != null)
                    {
                        StopCoroutine(_coroutineArrowTransition);
                    }

                    _compassArrowPivot.gameObject.SetActive(false);
                    _transitionArrow.gameObject.SetActive(false);
                    _rootOnScreenIcons.SetActive(false);
                }
            }
            else
            {
                // Object focused. Check if the object is onscreen or offscreen to show compass or bouncing arrow.
                // Also check if we need to trigger the transition arrow when going from offscreen to onscreen.
                if (!_targetFocused)
                {
                    _targetFocused = true;
                }

                if (_isTemporary && Time.unscaledTime - _targetStartTime > TemporaryDurationSeconds)
                {
                    SetTarget(null);
                    return;
                }

                Camera  avatarCamera      = UxrAvatar.LocalAvatarCamera;
                Vector3 targetInCameraPos = avatarCamera.WorldToScreenPoint(TargetPosition);
                float   percentMargin     = 0.20f;
                float   marginWidth       = avatarCamera.pixelWidth * percentMargin;
                float   marginHeight      = avatarCamera.pixelHeight * percentMargin;

                if (targetInCameraPos.x >= marginWidth &&
                    targetInCameraPos.x <= avatarCamera.pixelWidth - marginWidth &&
                    targetInCameraPos.y >= marginHeight &&
                    targetInCameraPos.y <= avatarCamera.pixelHeight - marginHeight &&
                    targetInCameraPos.z > 0.0f)
                {
                    // Object onscreen
                    if (!_rootOnScreenIcons.activeSelf && !_transitionArrow.gameObject.activeSelf)
                    {
                        // Transition offscreen -> onscreen
                        _transitionArrow.gameObject.SetActive(true);

                        if (_coroutineArrowTransition != null)
                        {
                            StopCoroutine(_coroutineArrowTransition);
                        }

                        _coroutineArrowTransition = StartCoroutine(ArrowTransitionCoroutine(_compassArrowRenderer.transform.position, TargetPosition));
                    }

                    _rootOnScreenIcons.transform.position = TargetPosition;
                    _compassArrowPivot.gameObject.SetActive(false);
                    UpdateOnScreenIcon(Time.unscaledTime);
                }
                else
                {
                    // Object offscreen -> show compass
                    _rootOnScreenIcons.gameObject.SetActive(false);
                    _compassArrowPivot.gameObject.SetActive(true);

                    Vector3 direction = avatarCamera.transform.InverseTransformPoint(TargetPosition);
                    direction.z = 0.0f;
                    direction.Normalize();
                    direction = new Vector3(targetInCameraPos.x - avatarCamera.pixelWidth * 0.5f, targetInCameraPos.y - avatarCamera.pixelHeight * 0.5f, 0.0f).normalized;

                    if (targetInCameraPos.z < 0.0f)
                    {
                        direction = -direction;
                    }

                    _compassArrowPivot.transform.SetPositionAndRotation(avatarCamera.transform.position + avatarCamera.transform.forward * _distanceToCamera, Quaternion.LookRotation(avatarCamera.transform.forward, avatarCamera.transform.TransformDirection(direction)));
                }
            }

            Color color = Color.white;
            color.a                              = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2.0f * 5.0f) + 1.0f) * 0.5f;
            _compassArrowRenderer.material.color = color;
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that transitions between the compass arrow to the arrow that moves to the target when it comes into
        ///     sight.
        /// </summary>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <returns></returns>
        private IEnumerator ArrowTransitionCoroutine(Vector3 posStart, Vector3 posEnd)
        {
            _transitionArrow.rotation = Quaternion.LookRotation(posEnd - posStart);

            float duration  = 0.2f;
            float startTime = Time.unscaledTime;

            while (Time.unscaledTime - startTime < duration)
            {
                float t = (Time.unscaledTime - startTime) / duration;
                _transitionArrow.transform.position = Vector3.Lerp(posStart, posEnd, t);
                yield return null;
            }

            _transitionArrow.gameObject.SetActive(false);

            // _onScreenStartTime will ensure that the effects will align in a cool way when the transition arrow disappears. The animation curve will always start correctly.
            _onScreenStartTime = Time.unscaledTime;
            _rootOnScreenIcons.SetActive(true);
            UpdateOnScreenIcon(Time.unscaledTime);

            _coroutineArrowTransition = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the icon.
        /// </summary>
        /// <param name="time">Time in seconds the icon has been on screen</param>
        private void UpdateOnScreenIcon(float time)
        {
            if (UxrAvatar.LocalAvatarCamera == null)
            {
                return;
            }

            float frequency         = 2.0f;
            float timeSinceOnScreen = time - _onScreenStartTime;
            float interpolationTime = timeSinceOnScreen * frequency;
            float effectBounceT     = UxrInterpolator.GetInterpolationFactor(interpolationTime, UxrEasing.EaseOutQuad,   UxrLoopMode.PingPong);
            float effectSineT       = UxrInterpolator.GetInterpolationFactor(interpolationTime, UxrEasing.EaseInOutSine, UxrLoopMode.PingPong);

            _rootOnScreenIcons.transform.position = TargetPosition;

            _iconLocationPivot.gameObject.SetActive(DisplayMode == UxrCompassDisplayMode.Location);
            _iconLookPivot.gameObject.SetActive(DisplayMode == UxrCompassDisplayMode.Look && timeSinceOnScreen < TemporaryDurationSeconds);
            _iconGrabPivot.gameObject.SetActive(DisplayMode == UxrCompassDisplayMode.Grab);
            _iconUsePivot.gameObject.SetActive(DisplayMode == UxrCompassDisplayMode.Use);

            if (DisplayMode == UxrCompassDisplayMode.Location)
            {
                _iconLocationBottom.transform.localPosition = Vector3.up * (effectBounceT * 0.4f);
            }
            else if (DisplayMode == UxrCompassDisplayMode.Grab)
            {
                _iconGrabRenderer.material.color = ColorExt.ColorAlpha(Color.white, effectSineT);
            }
            else if (DisplayMode == UxrCompassDisplayMode.Look)
            {
                _iconLookRenderer.material.color = ColorExt.ColorAlpha(Color.white, effectSineT);
            }
            else if (DisplayMode == UxrCompassDisplayMode.Use)
            {
                _iconUseRenderer.material.color = ColorExt.ColorAlpha(Color.white, effectSineT);
            }

            // Scale visible icon based on size

            foreach (KeyValuePair<MeshRenderer, Vector3> iconScale in _initialIconScales)
            {
                if (iconScale.Key.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(iconScale.Key.transform.position, UxrAvatar.LocalAvatar.CameraPosition);
                    iconScale.Key.transform.localScale = Vector3.Max(iconScale.Value * _iconScale, (distance * 0.3f) * _iconScale * iconScale.Value);
                }
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the icon renderer components.
        /// </summary>
        private IEnumerable<MeshRenderer> IconRenderers
        {
            get
            {
                yield return _iconLocationRenderer;
                yield return _iconLookRenderer;
                yield return _iconGrabRenderer;
                yield return _iconUseRenderer;
            }
        }

        /// <summary>
        ///     Duration in seconds to show the look icon while the target is in view. After that, do not show the look icon unless
        ///     it comes into sight again. It is also used by <see cref="SetTargetTemporary" />.
        /// </summary>
        private const float TemporaryDurationSeconds = 3.0f;

        private bool                              _targetFocused;
        private bool                              _targetIsRawPos;
        private Vector3                           _rawTargetPos;
        private UxrCompassTargetHint              _targetHint;
        private Coroutine                         _coroutineArrowTransition;
        private float                             _targetStartTime;
        private float                             _onScreenStartTime;
        private Dictionary<MeshRenderer, Vector3> _initialIconScales;
        private float                             _iconScale;
        private bool                              _isTemporary;

        #endregion
    }
}