// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotion.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Devices;
using UnityEngine;

#pragma warning disable 0414

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Standard locomotion using an arc projected from the controllers.
    /// </summary>
    public class UxrTeleportLocomotion : UxrTeleportLocomotionBase
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

            EnableArc(false, false);
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
                if (joystickValue != Vector2.zero)
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
                        teleportArcActive    = true;
                        _arcCancelledByAngle = false;
                    }
                }
            }
            else if (Avatar.ControllerInput.GetButtonsPressDown(HandSide, UxrInputButtons.JoystickUp))
            {
                teleportArcActive = true;
            }

            // If teleport is active update arc & target

            if (teleportArcActive)
            {
                // Disable others if this one just activated

                if (_previousFrameHadArc == false)
                {
                    CancelOtherTeleportTargets();
                }

                // Compute trajectory

                bool    isValidTeleport  = false;
                Vector3 right            = Vector3.Cross(ControllerForward, Vector3.up).normalized;
                Vector3 projectedForward = Vector3.ProjectOnPlane(ControllerForward, Vector3.up).normalized;
                float   angle            = -Vector3.SignedAngle(ControllerForward, projectedForward, right);

                if (Mathf.Abs(angle) < AbsoluteMaxArcAngleThreshold && _arcWidth > 0.0f)
                {
                    float parabolicSpeed           = Mathf.Sqrt(ArcGravity * MaxAllowedDistance);
                    float timeToTravelHorizontally = Mathf.Max(2.0f, 2.0f * parabolicSpeed * Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)) / ArcGravity);

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
                            isValidTeleport = NotifyDestinationRaycast(hit, false);
                            break;
                        }
                    }

                    if (hitSomething == false)
                    {
                        NotifyNoDestinationRaycast();
                    }

                    _previousFrameHadArc = true;
                    GenerateArcMesh(isValidTeleport, right, parabolicSpeed, endTime);
                    EnableArc(true, isValidTeleport);
                }
                else
                {
                    _arcCancelledByAngle = true;
                    EnableArc(false, false);
                    NotifyNoDestinationRaycast();
                }
            }
            else
            {
                if (_previousFrameHadArc)
                {
                    TryTeleportUsingCurrentTarget();
                }

                _previousFrameHadArc = false;
                EnableArc(false, false);
                NotifyNoDestinationRaycast();
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

            _arcGameObject.transform.SetPositionAndRotation(ControllerStart, Quaternion.LookRotation(ControllerForward));

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
            _arcMesh.bounds = new Bounds(Vector3.zero, Vector3.one * UxrAvatar.LocalAvatarCamera.farClipPlane);
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
            return origin + speed * time * forward - 0.5f * 9.8f * time * time * Vector3.up;
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

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets whether the teleport arc is currently visible.
        /// </summary>
        private bool IsArcVisible => _arcGameObject.activeSelf;

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

        #endregion
    }
}

#pragma warning restore 0414