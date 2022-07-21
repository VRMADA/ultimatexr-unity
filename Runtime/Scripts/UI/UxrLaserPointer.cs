// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLaserPointer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Devices;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.UI.UnityInputModule;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateXR.UI
{
    /// <summary>
    ///     Component that, added to an object in an <see cref="UxrAvatar" /> , allows it to interact with user interfaces
    ///     using a laser pointer. It is normally added to the hand, so that it points in a forward direction from the hand,
    ///     but can also be added to inanimate objects.
    /// </summary>
    public class UxrLaserPointer : UxrAvatarComponent<UxrLaserPointer>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] protected UxrHandSide        _handSide             = UxrHandSide.Left;
        [SerializeField] protected UxrInputButtons    _clickInput           = UxrInputButtons.Trigger;
        [SerializeField] protected UxrInputButtons    _showLaserInput       = UxrInputButtons.Joystick;
        [SerializeField] protected UxrButtonEventType _showLaserButtonEvent = UxrButtonEventType.Touching;
        [SerializeField] protected GameObject         _optionalEnableWhenLaserOn;
        [SerializeField] protected bool               _useControllerForward = true;
        [SerializeField] protected bool               _invisible;
        [SerializeField] protected float              _rayLength              = 100.0f;
        [SerializeField] protected float              _rayWidth               = 0.003f;
        [SerializeField] protected Color              _rayColorInteractive    = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        [SerializeField] protected Color              _rayColorNonInteractive = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        [SerializeField] protected Material           _rayHitMaterial;
        [SerializeField] protected float              _rayHitSize = 0.004f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the laser is currently enabled.
        /// </summary>
        public bool IsLaserEnabled => _isAutoEnabled || (Avatar.ControllerInput.IsControllerEnabled(_handSide) && Avatar.ControllerInput.GetButtonsEvent(_handSide, _showLaserInput, _showLaserButtonEvent));

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is used to compute the direction in which the laser points. The laser will
        ///     point in the <see cref="Transform.forward" /> direction.
        /// </summary>
        public Transform LaserTransform
        {
            get
            {
                if (_useControllerForward && !Avatar.HasDummyControllerInput)
                {
                    UxrController3DModel model = Avatar.ControllerInput.GetController3DModel(_handSide);

                    if (model && model.gameObject.activeInHierarchy)
                    {
                        return model.Forward != null ? model.Forward : transform;
                    }
                }

                return transform;
            }
        }

        /// <summary>
        ///     Gets the laser origin position.
        /// </summary>
        public Vector3 LaserPos => LaserTransform.position;

        /// <summary>
        ///     Gets the laser direction.
        /// </summary>
        public Vector3 LaserDir => LaserTransform.forward;

        /// <summary>
        ///     Gets the hand the laser pointer belongs to.
        /// </summary>
        public UxrHandSide HandSide
        {
            get => _handSide;
            set => _handSide = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the user performed a click this frame (released the input button after pressing).
        /// </summary>
        /// <returns>Whether the user performed a click action</returns>
        public bool IsClickedThisFrame()
        {
            return Avatar.ControllerInput.GetButtonsEvent(_handSide, _clickInput, UxrButtonEventType.PressDown);
        }

        /// <summary>
        ///     Checks whether the user performed a press this frame (pressed the input button).
        /// </summary>
        /// <returns>Whether the user performed a press action</returns>
        public bool IsReleasedThisFrame()
        {
            return Avatar.ControllerInput.GetButtonsEvent(_handSide, _clickInput, UxrButtonEventType.PressUp);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Avatar == null)
            {
                UxrManager.LogMissingAvatarInHierarchyError(this);
            }

            // Set up line renderer

            if (_invisible == false)
            {
                _lineRenderer               = gameObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;

                SetLineRendererMesh(_rayLength);

                _lineRenderer.material             = new Material(ShaderExt.UnlitTransparentColor);
                _lineRenderer.material.renderQueue = (int)RenderQueue.Overlay + 1;
            }

            // Set up raycast hit quad

            if (_invisible == false)
            {
                _hitQuad                  = new GameObject("Laser Hit");
                _hitQuad.transform.parent = transform;

                MeshFilter laserHitMeshFilter = _hitQuad.AddComponent<MeshFilter>();
                laserHitMeshFilter.sharedMesh = MeshExt.CreateQuad(1.0f);

                _laserHitRenderer                   = _hitQuad.AddComponent<MeshRenderer>();
                _laserHitRenderer.receiveShadows    = false;
                _laserHitRenderer.shadowCastingMode = ShadowCastingMode.Off;
                _laserHitRenderer.sharedMaterial    = _rayHitMaterial;

                _hitQuad.SetActive(false);
            }

            _isAutoEnabled = false;
        }

        /// <summary>
        ///     Updates the laser pointer.
        /// </summary>
        private void LateUpdate()
        {
            _isAutoEnabled = UxrPointerInputModule.Instance && UxrPointerInputModule.Instance.CheckRaycastAutoEnable(this);

            if (_optionalEnableWhenLaserOn != null)
            {
                _optionalEnableWhenLaserOn.SetActive(IsLaserEnabled);
            }

            // TODO: In order to use UxrLaserPointer for other than Unity UI, the following part should be extracted. 

            UxrPointerEventData laserPointerEventData = UxrPointerInputModule.Instance != null ? UxrPointerInputModule.Instance.GetPointerEventData(this) : null;

            if (_lineRenderer && laserPointerEventData != null)
            {
                _lineRenderer.enabled        = IsLaserEnabled;
                _lineRenderer.material.color = UxrPointerInputModule.IsInteractive(laserPointerEventData.pointerEnter) ? _rayColorInteractive : _rayColorNonInteractive;

                if (_laserHitRenderer)
                {
                    _laserHitRenderer.material.color = _lineRenderer.material.color;
                }
            }

            float currentRayLength = _rayLength;

            if (laserPointerEventData != null && laserPointerEventData.pointerCurrentRaycast.isValid && IsLaserEnabled)
            {
                currentRayLength = laserPointerEventData.pointerCurrentRaycast.distance;

                if (Avatar.CameraComponent && _hitQuad)
                {
                    _hitQuad.SetActive(true);
                    _hitQuad.transform.localPosition = Vector3.forward * currentRayLength;
                    _hitQuad.transform.LookAt(Avatar.CameraPosition);

                    Plane plane = new Plane(Avatar.CameraForward, Avatar.CameraPosition);
                    float dist  = plane.GetDistanceToPoint(_hitQuad.transform.position);
                    _hitQuad.transform.localScale = Vector3.one * _rayHitSize * Mathf.Max(2.0f, dist);
                }
            }
            else
            {
                // TODO: currentRayLength should come somehow from UxrLaserPointerRaycaster.Raycast() because the actual computation
                // with the correct blocking objects and blocking mask is there

                if (IsLaserEnabled && Physics.Raycast(LaserPos, LaserDir, out RaycastHit hitInfo, currentRayLength, -1, QueryTriggerInteraction.Ignore))
                {
                    currentRayLength = hitInfo.distance;

                    if (Avatar.CameraComponent && _hitQuad)
                    {
                        _hitQuad.SetActive(true);
                        _hitQuad.transform.localPosition = Vector3.forward * currentRayLength;
                        _hitQuad.transform.LookAt(Avatar.CameraPosition);

                        Plane plane = new Plane(Avatar.CameraForward, Avatar.CameraPosition);
                        float dist  = plane.GetDistanceToPoint(_hitQuad.transform.position);
                        _hitQuad.transform.localScale = Vector3.one * _rayHitSize * Mathf.Max(2.0f, dist);
                    }
                }
                else
                {
                    if (_hitQuad)
                    {
                        _hitQuad.SetActive(false);
                    }
                }
            }

            if (_lineRenderer && _lineRenderer.enabled)
            {
                SetLineRendererMesh(currentRayLength);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the line renderer mesh.
        /// </summary>
        /// <param name="rayLength">New ray length</param>
        private void SetLineRendererMesh(float rayLength)
        {
            _lineRenderer.startWidth = _rayWidth;
            _lineRenderer.endWidth   = _rayWidth;

            Vector3[] positions =
            {
                        new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f,                                                                               0.0f, rayLength > GradientLength ? GradientLength : rayLength * 0.33f),
                        new Vector3(0.0f, 0.0f, rayLength < GradientLength * 2.0f ? rayLength * 0.66f : rayLength - GradientLength), new Vector3(0.0f, 0.0f, rayLength)
            };

            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = _lineRenderer.transform.InverseTransformPoint(LaserTransform.TransformPoint(positions[i]));
            }

            _lineRenderer.SetPositions(positions);

            Gradient colorGradient = new Gradient();
            colorGradient.colorKeys = new[]
                                      {
                                                  new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white,                                                                          rayLength > GradientLength ? GradientLength / rayLength : 0.33f),
                                                  new GradientColorKey(Color.white, rayLength < GradientLength * 2.0f ? 0.66f : 1.0f - GradientLength / rayLength), new GradientColorKey(Color.white, 1.0f)
                                      };
            colorGradient.alphaKeys = new[]
                                      {
                                                  new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f,                                                                          rayLength > GradientLength ? GradientLength / rayLength : 0.3f),
                                                  new GradientAlphaKey(1.0f, rayLength < GradientLength * 2.0f ? 0.66f : 1.0f - GradientLength / rayLength), new GradientAlphaKey(0.0f, 1.0f)
                                      };
            _lineRenderer.colorGradient = colorGradient;

            _lineRenderer.positionCount = 4;
        }

        #endregion

        #region Private Types & Data

        private const float GradientLength = 0.4f;

        private LineRenderer _lineRenderer;
        private Renderer     _laserHitRenderer;
        private bool         _isAutoEnabled;
        private GameObject   _hitQuad;

        #endregion
    }
}