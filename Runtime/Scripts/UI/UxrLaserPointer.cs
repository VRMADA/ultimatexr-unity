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

        // General
        
        [SerializeField] protected UxrHandSide _handSide             = UxrHandSide.Left;
        [SerializeField] protected bool        _useControllerForward = true;
        
        // Interaction
        
        [SerializeField] protected UxrLaserPointerTargetTypes _targetTypes                 = UxrLaserPointerTargetTypes.UI | UxrLaserPointerTargetTypes.Colliders2D | UxrLaserPointerTargetTypes.Colliders3D;
        [SerializeField] private   QueryTriggerInteraction    _triggerCollidersInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField] private   LayerMask                  _blockingMask                = ~0;
        
        // Input
        
        [SerializeField] protected UxrInputButtons    _clickInput                = UxrInputButtons.Trigger;
        [SerializeField] protected UxrInputButtons    _showLaserInput            = UxrInputButtons.Joystick;
        [SerializeField] protected UxrButtonEventType _showLaserButtonEvent      = UxrButtonEventType.Touching;
        
        // Appearance
        
        [SerializeField] protected bool       _invisible                 = false;
        [SerializeField] protected float      _rayLength                 = 100.0f;
        [SerializeField] protected float      _rayWidth                  = 0.003f;
        [SerializeField] protected Color      _rayColorInteractive       = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        [SerializeField] protected Color      _rayColorNonInteractive    = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        [SerializeField] protected Material   _rayHitMaterial            = null;
        [SerializeField] protected float      _rayHitSize                = 0.004f;
        [SerializeField] protected GameObject _optionalEnableWhenLaserOn = null;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the laser is currently enabled.
        /// </summary>
        public bool IsLaserEnabled => gameObject.activeInHierarchy && enabled &&
                                      (ForceLaserEnabled ||
                                       IsAutoEnabled ||
                                       (Avatar.ControllerInput.IsControllerEnabled(_handSide) && Avatar.ControllerInput.GetButtonsEvent(_handSide, ShowLaserInput, ShowLaserButtonEvent)));

        /// <summary>
        ///     Gets the <see cref="Transform" /> that is used to compute the direction in which the laser points. The laser will
        ///     point in the <see cref="Transform.forward" /> direction.
        /// </summary>
        public Transform LaserTransform
        {
            get
            {
                if (UseControllerForward && !Avatar.HasDummyControllerInput)
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
        public UxrHandSide HandSide => _handSide;

        /// <summary>
        ///     Gets or sets whether the laser should be forcefully enabled. This is useful when
        ///     <see cref="UxrCanvas.AutoEnableLaserPointer" /> is used or a controller input is required to enable the laser
        ///     pointer.
        /// </summary>
        public bool ForceLaserEnabled { get; set; }

        /// <summary>
        ///     Gets or sets whether the laser should ignore the <see cref="UxrCanvas.AutoEnableLaserPointer" />
        ///     property in canvases.
        /// </summary>
        public bool IgnoreAutoEnable { get; set; }

        /// <summary>
        ///     Gets or sets whether to use the real controller forward instead of the component's forward.
        /// </summary>
        public bool UseControllerForward
        {
            get => _useControllerForward;
            set => _useControllerForward = value;
        }

        /// <summary>
        /// Gets or sets the elements the laser can interact with.
        /// </summary>
        public UxrLaserPointerTargetTypes TargetTypes
        {
            get => _targetTypes;
            set => _targetTypes = value;
        }

        /// <summary>
        ///     Gets or sets how to treat collisions against trigger volumes.
        ///     By default the laser doesn't collide against trigger volumes.
        /// </summary>
        public QueryTriggerInteraction TriggerCollidersInteraction
        {
            get => _triggerCollidersInteraction;
            set => _triggerCollidersInteraction = value;
        }

        /// <summary>
        ///     Gets or sets the which layers will block the laser for 3D objects.
        /// </summary>
        public LayerMask BlockingMask
        {
            get => _blockingMask;
            set => _blockingMask = value;
        }

        /// <summary>
        ///     Gets or sets the input button(s) required for a click.
        /// </summary>
        public UxrInputButtons ClickInput
        {
            get => _clickInput;
            set => _clickInput = value;
        }

        /// <summary>
        ///     Gets or sets the input button(s) required to show the laser. Use <see cref="UxrInputButtons.None" /> to have the
        ///     laser always enabled or <see cref="UxrInputButtons.Everything" /> to have it always disabled and let
        ///     <see cref="UxrCanvas.AutoEnableLaserPointer" /> handle the enabling/disabling.
        /// </summary>
        public UxrInputButtons ShowLaserInput
        {
            get => _showLaserInput;
            set => _showLaserInput = value;
        }

        /// <summary>
        ///     Gets or sets the button event type required for <see cref="ShowLaserInput" />.
        /// </summary>
        public UxrButtonEventType ShowLaserButtonEvent
        {
            get => _showLaserButtonEvent;
            set => _showLaserButtonEvent = value;
        }

        /// <summary>
        ///     Gets or sets whether to use an invisible laser ray.
        /// </summary>
        public bool IsInvisible
        {
            get => _invisible;
            set => _invisible = value;
        }

        /// <summary>
        ///     Gets or sets the maximum laser length. This is the distance that the ray will travel if not occluded.
        /// </summary>
        public float MaxRayLength
        {
            get => _rayLength;
            set => _rayLength = value;
        }

        /// <summary>
        ///     Gets the current laser ray length.
        /// </summary>
        public float CurrentRayLength { get; private set; }

        /// <summary>
        ///     Gets or sets the laser ray width.
        /// </summary>
        public float RayWidth
        {
            get => _rayWidth;
            set => _rayWidth = value;
        }

        /// <summary>
        ///     Gets or sets the ray color when it's pointing to an interactive element.
        /// </summary>
        public Color RayColorInteractive
        {
            get => _rayColorInteractive;
            set => _rayColorInteractive = value;
        }

        /// <summary>
        ///     Gets or sets the ray color when it's not pointing to an interactive element.
        /// </summary>
        public Color RayColorNonInteractive
        {
            get => _rayColorNonInteractive;
            set => _rayColorNonInteractive = value;
        }

        /// <summary>
        ///     Gets or sets the size of the ray hit quad..
        /// </summary>
        public float RayHitSize
        {
            get => _rayHitSize;
            set => _rayHitSize = value;
        }

        /// <summary>
        ///     Gets or sets an optional GameObject that will be enabled or disabled along with the laser.
        /// </summary>
        public GameObject OptionalEnableWhenLaserOn
        {
            get => _optionalEnableWhenLaserOn;
            set => _optionalEnableWhenLaserOn = value;
        }
        
        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets or sets whether the laser is enabled automatically due to pointing at a UI.
        /// </summary>
        internal bool IsAutoEnabled { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the user performed a click this frame (released the input button after pressing).
        /// </summary>
        /// <returns>Whether the user performed a click action</returns>
        public bool IsClickedThisFrame()
        {
            return Avatar.ControllerInput.GetButtonsEvent(_handSide, ClickInput, UxrButtonEventType.PressDown);
        }

        /// <summary>
        ///     Checks whether the user performed a press this frame (pressed the input button).
        /// </summary>
        /// <returns>Whether the user performed a press action</returns>
        public bool IsReleasedThisFrame()
        {
            return Avatar.ControllerInput.GetButtonsEvent(_handSide, ClickInput, UxrButtonEventType.PressUp);
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

            _lineRenderer               = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;

            SetLineRendererMesh(MaxRayLength);

            _lineRenderer.material             = new Material(ShaderExt.UnlitTransparentColor);
            _lineRenderer.material.renderQueue = (int)RenderQueue.Overlay + 1;

            // Set up raycast hit quad

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

        /// <summary>
        ///     Updates the laser pointer.
        /// </summary>
        private void LateUpdate()
        {
            if (OptionalEnableWhenLaserOn != null)
            {
                OptionalEnableWhenLaserOn.SetActive(IsLaserEnabled);
            }

            // TODO: In order to use UxrLaserPointer for other than Unity UI, the following part should be extracted. 

            UxrPointerEventData laserPointerEventData = UxrPointerInputModule.Instance != null ? UxrPointerInputModule.Instance.GetPointerEventData(this) : null;

            if (_lineRenderer)
            {
                _lineRenderer.enabled        = IsLaserEnabled && !IsInvisible;
                _lineRenderer.material.color = laserPointerEventData != null && laserPointerEventData.IsInteractive ? RayColorInteractive : RayColorNonInteractive;

                if (_laserHitRenderer)
                {
                    _laserHitRenderer.enabled        = !IsInvisible;
                    _laserHitRenderer.material.color = _lineRenderer.material.color;
                }
            }

            CurrentRayLength = MaxRayLength;

            if (laserPointerEventData != null && laserPointerEventData.HasData && IsLaserEnabled)
            {
                CurrentRayLength = laserPointerEventData.pointerCurrentRaycast.distance;

                if (Avatar.CameraComponent && _hitQuad)
                {
                    _hitQuad.SetActive(true);
                    _hitQuad.transform.localPosition = Vector3.forward * CurrentRayLength;
                    _hitQuad.transform.LookAt(Avatar.CameraPosition);

                    Plane plane = new Plane(Avatar.CameraForward, Avatar.CameraPosition);
                    float dist  = plane.GetDistanceToPoint(_hitQuad.transform.position);
                    _hitQuad.transform.localScale = RayHitSize * Mathf.Max(2.0f, dist) * Vector3.one;
                }
            }
            else
            {
                _hitQuad.SetActive(false);
            }

            if (_lineRenderer && _lineRenderer.enabled)
            {
                SetLineRendererMesh(CurrentRayLength);
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
            _lineRenderer.startWidth = RayWidth;
            _lineRenderer.endWidth   = RayWidth;

            float t1 = Mathf.Min(rayLength * 0.33f, GradientLength);
            float t2 = Mathf.Max(rayLength * 0.66f, rayLength - GradientLength);

            Vector3[] positions =
            {
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, t1),
                        new Vector3(0.0f, 0.0f, t2),
                        new Vector3(0.0f, 0.0f, rayLength)
            };

            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = _lineRenderer.transform.InverseTransformPoint(LaserTransform.TransformPoint(positions[i]));
            }

            _lineRenderer.SetPositions(positions);

            Gradient colorGradient = new Gradient();
            colorGradient.colorKeys = new[]
                                      {
                                                  new GradientColorKey(Color.white, 0.0f),
                                                  new GradientColorKey(Color.white, t1 / rayLength),
                                                  new GradientColorKey(Color.white, t2 / rayLength),
                                                  new GradientColorKey(Color.white, 1.0f)
                                      };
            colorGradient.alphaKeys = new[]
                                      {
                                                  new GradientAlphaKey(0.0f, 0.0f),
                                                  new GradientAlphaKey(1.0f, t1 / rayLength),
                                                  new GradientAlphaKey(1.0f, t2 / rayLength),
                                                  new GradientAlphaKey(0.0f, 1.0f)
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