// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotion.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices;
using UnityEngine;

#pragma warning disable 0414

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Standard locomotion using an arc projected from the controllers.
    /// </summary>
    public partial class UxrTeleportLocomotion : UxrTeleportLocomotionBase
    {
        #region Inspector Properties/Serialized Fields

        // Arc

        [SerializeField] [Range(2,     1000)] private int                    _arcSegments           = 100;
        [SerializeField] [Range(0.01f, 0.4f)] private float                  _arcWidth              = 0.1f;
        [SerializeField]                      private float                  _arcScrollSpeedValid   = 1.0f;
        [SerializeField]                      private float                  _arcScrollSpeedInvalid = 0.5f;
        [SerializeField]                      private Material               _arcMaterialValid;
        [SerializeField]                      private Material               _arcMaterialInvalid;
        [SerializeField]                      private float                  _arcFadeLength       = 2.0f;
        [SerializeField]                      private UxrRaycastStepsQuality _raycastStepsQuality = UxrRaycastStepsQuality.HighQuality;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets whether the arc can be used.
        /// </summary>
        public bool IsArcAllowed { get; set; } = true;

        #endregion

        #region Public Overrides UxrLocomotion

        /// <inheritdoc />
        public override bool IsSmoothLocomotion => false;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Create arc GameObject

            _arcGameObject = new GameObject("Arc");
            _arcGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            _arcGameObject.transform.parent = Avatar.transform;

            _arcMeshFilter = _arcGameObject.AddComponent<MeshFilter>();
            _arcRenderer   = _arcGameObject.AddComponent<MeshRenderer>();

            _arcMesh              = new Mesh();
            _vertices             = new Vector3[(_arcSegments + 1) * 2];
            _vertexColors         = new Color32[(_arcSegments + 1) * 2];
            _vertexMapping        = new Vector2[(_arcSegments + 1) * 2];
            _accumulatedArcLength = new float [(_arcSegments + 1) * 2];

            _indices = new int[_arcSegments * 4];

            for (int i = 0; i < _arcSegments; i++)
            {
                int baseIndex = (_arcSegments - 1 - i) * 2;
                _indices[i * 4 + 0] = baseIndex;
                _indices[i * 4 + 1] = baseIndex + 2;
                _indices[i * 4 + 2] = baseIndex + 3;
                _indices[i * 4 + 3] = baseIndex + 1;
            }

            _arcMesh.vertices = _vertices;
            _arcMesh.colors32 = _vertexColors;
            _arcMesh.uv       = _vertexMapping;
            _arcMesh.SetIndices(_indices, MeshTopology.Quads, 0);

            _arcMeshFilter.mesh         = _arcMesh;
            _arcRenderer.sharedMaterial = _arcMaterialValid;
        }

        /// <summary>
        ///     Resets the component when enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            // Set initial state

            _previousFrameHadArc = false;
            _arcCancelled        = false;
            _arcCancelledByAngle = false;
            _scroll              = 0.0f;

            _lastSyncIsArcEnabled        = false;
            _lastSyncIsTargetEnabled     = false;
            _lastSyncIsValidTeleport     = false;
            _lastSyncTargetArrowLocalRot = Quaternion.identity;

            EnableArc(false, false);
        }

        /// <summary>
        ///     Disables the teleport graphics.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            UpdateTeleportState(false, false, false, Quaternion.identity);
        }

        #endregion

        #region Protected Overrides UxrTeleportLocomotionBase

        /// <inheritdoc />
        protected override bool CanBackStep => !IsArcVisible;

        /// <inheritdoc />
        protected override bool CanRotate => !IsArcVisible;

        /// <inheritdoc />
        protected override void UpdateTeleportLocomotion()
        {
            Vector2 joystickValue = Avatar.ControllerInput.GetInput2D(HandSide, UxrInput2D.Joystick);

            if (joystickValue == Vector2.zero)
            {
                _arcCancelled        = false;
                _arcCancelledByAngle = false;
            }
            else
            {
                if (_arcCancelled)
                {
                    joystickValue = Vector2.zero;
                }
            }

            bool teleportArcActive = false;

            // Check if the arc is active.

            if (IsArcVisible || _arcCancelledByAngle)
            {
                // To support both touchpads and joysticks, we need to check in the case of touchpads that it is also pressed.

                if (Avatar.ControllerInput.MainJoystickIsTouchpad)
                {
                    if (Avatar.ControllerInput.GetButtonsPress(HandSide, UxrInputButtons.Joystick))
                    {
                        teleportArcActive    = true;
                        _arcCancelledByAngle = false;
                    }
                }
                else
                {
                    if (joystickValue != Vector2.zero)
                    {
                        teleportArcActive    = true;
                        _arcCancelledByAngle = false;
                    }
                }
            }
            else if (Avatar.ControllerInput.GetButtonsPressDown(HandSide, UxrInputButtons.JoystickUp))
            {
                teleportArcActive = true;
            }

            if (!IsArcAllowed)
            {
                teleportArcActive = false;
            }

            // If teleport is active update arc & target

            bool isArcEnabled;
            bool isTargetEnabled;
            bool isValidTeleport;

            if (teleportArcActive)
            {
                // Disable others if this one just activated

                if (_previousFrameHadArc == false)
                {
                    CancelOtherTeleportTargets();
                }

                // Compute trajectory

                ComputeCurrentArcTrajectory(out isArcEnabled, out isTargetEnabled, out isValidTeleport);
            }
            else
            {
                if (_previousFrameHadArc && IsArcAllowed)
                {
                    TryTeleportUsingCurrentTarget();
                }

                _previousFrameHadArc = false;
                isArcEnabled         = false;
                isTargetEnabled      = false;
                isValidTeleport      = false;

                EnableArc(isArcEnabled, isValidTeleport);
                NotifyNoDestinationRaycast();
            }

            // Notify state changes in a simpler way to avoid unnecessary traffic

            if (_lastSyncIsArcEnabled != isArcEnabled ||
                _lastSyncIsTargetEnabled != isTargetEnabled ||
                _lastSyncIsValidTeleport != isValidTeleport ||
                Quaternion.Angle(_lastSyncTargetArrowLocalRot, TeleportArrowLocalRotation) > ArrowAngleChangeThreshold)
            {
                UpdateTeleportState(isArcEnabled, isTargetEnabled, isValidTeleport, TeleportArrowLocalRotation);
            }
        }

        /// <inheritdoc />
        protected override void CancelTarget()
        {
            base.CancelTarget();

            EnableArc(false, false);

            _previousFrameHadArc = false;
            _arcCancelled        = true;
            _arcCancelledByAngle = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the state of elements in the teleport. This is mainly to synchronize the state on a networking environment.
        ///     It is performed in a separate method in order to have better control over the amount of traffic that is being
        ///     generated because of the arrow rotation.
        /// </summary>
        /// <param name="isArcEnabled">Is the arc enabled?</param>
        /// <param name="isTargetEnabled">Is the target enabled?</param>
        /// <param name="isValidTeleport">Is the teleport destination valid?</param>
        /// <param name="teleportArrowLocalRotation">The teleport arrow's local rotation</param>
        private void UpdateTeleportState(bool isArcEnabled, bool isTargetEnabled, bool isValidTeleport, Quaternion teleportArrowLocalRotation)
        {
            // This method will be synchronized through network
            BeginSync();

            _lastSyncIsArcEnabled        = isArcEnabled;
            _lastSyncIsTargetEnabled     = isTargetEnabled;
            _lastSyncIsValidTeleport     = isValidTeleport;
            _lastSyncTargetArrowLocalRot = teleportArrowLocalRotation;
            
            if (isArcEnabled)
            {
                // TODO: The target enabled and valid teleport state are computed using the current
                // hand transform, which might be slightly different in a network environment. 
                ComputeCurrentArcTrajectory(out bool _, out bool _, out bool _);
                TeleportArrowLocalRotation = teleportArrowLocalRotation;
            }
            else
            {
                EnableArc(isArcEnabled, isValidTeleport);
                NotifyNoDestinationRaycast();
            }

            EndSyncMethod(new object[] { isArcEnabled, isTargetEnabled, isValidTeleport, teleportArrowLocalRotation });
        }

        /// <summary>
        ///     Enables or disables the teleportation arc.
        /// </summary>
        /// <param name="enable">Whether the arc is visible</param>
        /// <param name="isValidTeleport">Whether the current teleport destination is valid</param>
        private void EnableArc(bool enable, bool isValidTeleport)
        {
            _arcGameObject.SetActive(enable);
            _arcRenderer.sharedMaterial = isValidTeleport ? _arcMaterialValid : _arcMaterialInvalid;
        }

        /// <summary>
        ///     Computes the current teleport arc trajectory.
        /// </summary>
        private void ComputeCurrentArcTrajectory(out bool isArcEnabled, out bool isTargetEnabled, out bool isValidTeleport)
        {
            Vector3 right                    = Vector3.Cross(ControllerForward, Vector3.up).normalized;
            float   angle                    = GetCurrentParabolicAngle();
            float   timeToTravelHorizontally = GetTimeToTravelHorizontally(angle);
            float   parabolicSpeed           = GetCurrentParabolicSpeed();

            if (Mathf.Abs(angle) < AbsoluteMaxArcAngleThreshold && _arcWidth > 0.0f)
            {
                isTargetEnabled = false;
                isValidTeleport = false;

                float endTime   = timeToTravelHorizontally * 2;
                float deltaTime = endTime / BlockingRaycastStepsQualityToSteps(_raycastStepsQuality);

                bool hitSomething = false;

                for (float time = 0.0f; time < endTime; time += deltaTime)
                {
                    Vector3 point1                = EvaluateArc(ControllerStart, ControllerForward, parabolicSpeed, time);
                    Vector3 point2                = EvaluateArc(ControllerStart, ControllerForward, parabolicSpeed, time + deltaTime);
                    float   distanceBetweenPoints = Vector3.Distance(point1, point2);
                    Vector3 direction             = (point2 - point1) / distanceBetweenPoints;

                    // Process blocking hit.
                    // Use RaycastAll to avoid putting "permitted" objects in between "blocking" objects to teleport through walls or any other cheats.

                    if (HasBlockingRaycastHit(point1, direction, distanceBetweenPoints, out RaycastHit hit))
                    {
                        endTime         = time + deltaTime * (hit.distance / distanceBetweenPoints);
                        hitSomething    = true;
                        isValidTeleport = NotifyDestinationRaycast(hit, false, out isTargetEnabled);
                        break;
                    }
                }

                if (hitSomething == false)
                {
                    NotifyNoDestinationRaycast();
                }

                _previousFrameHadArc = true;
                isArcEnabled         = true;
                GenerateArcMesh(isValidTeleport, right, parabolicSpeed, endTime);
                EnableArc(isArcEnabled, isValidTeleport);
            }
            else
            {
                _arcCancelledByAngle = true;
                isArcEnabled         = false;
                isTargetEnabled      = false;
                isValidTeleport      = false;
                EnableArc(isArcEnabled, isValidTeleport);
                NotifyNoDestinationRaycast();
            }
        }

        /// <summary>
        ///     Generates the arc mesh.
        /// </summary>
        /// <param name="isValidTeleport">Whether the current teleport destination is valid</param>
        /// <param name="right">Arc world-space right vector</param>
        /// <param name="parabolicSpeed">The start speed used for parabolic computation</param>
        /// <param name="endTime">The time in the parabolic equation where the arc intersects with the first blocking element</param>
        private void GenerateArcMesh(bool isValidTeleport, Vector3 right, float parabolicSpeed, float endTime)
        {
            Vector3 previousPoint = Vector3.zero;

            float currentLength = 0.0f;
            float totalLength   = 0.0f;

            _arcGameObject.transform.SetPositionAndRotation(ControllerStart, Quaternion.LookRotation(ControllerForward, UpVector));

            _scroll += Time.deltaTime * (isValidTeleport ? _arcScrollSpeedValid : _arcScrollSpeedInvalid);

            for (int i = 0; i <= _arcSegments; ++i)
            {
                float time = endTime * ((float)i / _arcSegments);

                Vector3 point = EvaluateArc(ControllerStart, ControllerForward, parabolicSpeed, time);

                _vertices[i * 2 + 0] = _arcGameObject.transform.InverseTransformPoint(point - _arcWidth * 0.5f * right);
                _vertices[i * 2 + 1] = _arcGameObject.transform.InverseTransformPoint(point + _arcWidth * 0.5f * right);

                float pointDistance = i == 0 ? 0.0f : Vector3.Distance(previousPoint, point);

                if (i > 0)
                {
                    currentLength += pointDistance;
                }

                _accumulatedArcLength[i] = currentLength;
                totalLength              = currentLength;

                float v = currentLength / _arcWidth - _scroll;

                _vertexMapping[i * 2 + 0] = new Vector2(0.0f, v);
                _vertexMapping[i * 2 + 1] = new Vector2(1.0f, v);

                previousPoint = point;
            }

            // After creating the vertices, assign the colors after because knowing the exact arc length we can fade it nicely at both ends

            for (int i = 0; i <= _arcSegments; ++i)
            {
                byte alpha = 255;

                if (_arcFadeLength > 0.0f)
                {
                    float alphaFloat = 1.0f;

                    if (_accumulatedArcLength[i] < _arcFadeLength)
                    {
                        alphaFloat = _accumulatedArcLength[i] / _arcFadeLength;
                        alpha      = (byte)(alphaFloat * 255);
                    }

                    if (totalLength - _accumulatedArcLength[i] < _arcFadeLength)
                    {
                        alphaFloat = alphaFloat * ((totalLength - _accumulatedArcLength[i]) / _arcFadeLength);
                        alpha      = (byte)(alphaFloat * 255);
                    }
                }

                _vertexColors[i * 2 + 0] = new Color32(255, 255, 255, alpha);
                _vertexColors[i * 2 + 1] = new Color32(255, 255, 255, alpha);
            }

            // Assign mesh

            _arcMesh.vertices = _vertices;
            _arcMesh.colors32 = _vertexColors;
            _arcMesh.uv       = _vertexMapping;
            _arcMesh.SetIndices(_indices, MeshTopology.Quads, 0);
            _arcMesh.bounds = new Bounds(Vector3.zero, Vector3.one * Avatar.CameraComponent.farClipPlane);
        }

        /// <summary>
        ///     Computes the arc parabola.
        /// </summary>
        /// <param name="origin">World-space arc start position</param>
        /// <param name="forward">World-space arc start direction</param>
        /// <param name="speed">Start speed</param>
        /// <param name="time">Time value to get the position for</param>
        /// <returns>Position in the arc corresponding to the given time value</returns>
        private Vector3 EvaluateArc(Vector3 origin, Vector3 forward, float speed, float time)
        {
            return origin + speed * time * forward - 0.5f * 9.8f * time * time * UpVector;
        }

        /// <summary>
        ///     Maps quality to steps.
        /// </summary>
        /// <param name="stepsQuality">Quality</param>
        /// <returns>Step count used for ray-casting</returns>
        private int BlockingRaycastStepsQualityToSteps(UxrRaycastStepsQuality stepsQuality)
        {
            switch (stepsQuality)
            {
                case UxrRaycastStepsQuality.LowQuality:      return BlockingRaycastStepsQualityLow;
                case UxrRaycastStepsQuality.MediumQuality:   return BlockingRaycastStepsQualityMedium;
                case UxrRaycastStepsQuality.HighQuality:     return BlockingRaycastStepsQualityHigh;
                case UxrRaycastStepsQuality.VeryHighQuality: return BlockingRaycastStepsQualityVeryHigh;
                default:                                     return BlockingRaycastStepsQualityHigh;
            }
        }

        /// <summary>
        ///     Gets the parabolic speed to compute in the arc equation.
        /// </summary>
        /// <returns>Parabolic speed</returns>
        private float GetCurrentParabolicSpeed()
        {
            return Mathf.Sqrt(ArcGravity * MaxAllowedDistance);
        }

        /// <summary>
        ///     Gets the parabolic angle to compute in the arc equation.
        /// </summary>
        /// <returns>Parabolic angle</returns>
        private float GetCurrentParabolicAngle()
        {
            Vector3 right            = Vector3.Cross(ControllerForward, UpVector).normalized;
            Vector3 projectedForward = Vector3.ProjectOnPlane(ControllerForward, UpVector).normalized;
            return -Vector3.SignedAngle(ControllerForward, projectedForward, right);
        }

        /// <summary>
        ///     Gets the time in seconds it would take a parabolic trajectory to travel up and down again.
        /// </summary>
        /// <param name="angle">Parabolic angle</param>
        /// <returns>Time in seconds to go up and get back down again</returns>
        private float GetTimeToTravelHorizontally(float angle)
        {
            return Mathf.Max(2.0f, 2.0f * GetCurrentParabolicSpeed() * Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)) / ArcGravity);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets whether the teleport arc is currently visible.
        /// </summary>
        private bool IsArcVisible => _arcGameObject.activeSelf;

        /// <summary>
        ///     The change in degrees of the arrow direction to consider it a state change and raise the event. It is used to avoid
        ///     sending repeated data.
        /// </summary>
        private const float ArrowAngleChangeThreshold = 2.0f;

        private const int   BlockingRaycastStepsQualityLow      = 10;
        private const int   BlockingRaycastStepsQualityMedium   = 20;
        private const int   BlockingRaycastStepsQualityHigh     = 40;
        private const int   BlockingRaycastStepsQualityVeryHigh = 80;
        private const float AbsoluteMaxArcAngleThreshold        = 75.0f;
        private const float ArcGravity                          = 9.8f;

        private bool _previousFrameHadArc;
        private bool _arcCancelled;
        private bool _arcCancelledByAngle;

        private GameObject   _arcGameObject;
        private MeshFilter   _arcMeshFilter;
        private MeshRenderer _arcRenderer;
        private Mesh         _arcMesh;
        private Vector3[]    _vertices;
        private Color32[]    _vertexColors;
        private Vector2[]    _vertexMapping;
        private int[]        _indices;
        private float[]      _accumulatedArcLength;
        private float        _scroll;

        private bool       _lastSyncIsArcEnabled;
        private bool       _lastSyncIsTargetEnabled;
        private bool       _lastSyncIsValidTeleport;
        private Quaternion _lastSyncTargetArrowLocalRot;

        #endregion
    }
}

#pragma warning restore 0414