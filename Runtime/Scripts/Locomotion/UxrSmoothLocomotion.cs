// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSmoothLocomotion.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.CameraUtils;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Type of locomotion where the user moves across the scenario in a similar way to FPS video-games.
    /// </summary>
    public class UxrSmoothLocomotion : UxrLocomotion
    {
        #region Inspector Properties/Serialized Fields

        [Header("General parameters")] [SerializeField] private bool             _parentToDestination;
        [SerializeField]                                private float            _metersPerSecondNormal          = 2.0f;
        [SerializeField]                                private float            _metersPerSecondSprint          = 4.0f;
        [SerializeField]                                private UxrWalkDirection _walkDirection                  = UxrWalkDirection.ControllerForward;
        [SerializeField]                                private float            _rotationDegreesPerSecondNormal = 120.0f;
        [SerializeField]                                private float            _rotationDegreesPerSecondSprint = 120.0f;
        [SerializeField]                                private float            _gravity                        = -9.81f;

        [Header("Input parameters")] [SerializeField] private UxrHandSide     _sprintButtonHand = UxrHandSide.Left;
        [SerializeField]                              private UxrInputButtons _sprintButton     = UxrInputButtons.Joystick;

        [Header("Constraints")] [SerializeField] private QueryTriggerInteraction _triggerCollidersInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField]                         private LayerMask               _collisionLayerMask          = ~0;
        [SerializeField]                         private float                   _capsuleRadius               = 0.25f;
        [SerializeField]                         private float                   _maxStepHeight               = 0.2f;
        [SerializeField] [Range(0.0f, 80.0f)]    private float                   _maxSlopeDegrees             = 35.0f;
        [SerializeField]                         private float                   _stepDistanceCheck           = 0.2f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Meters per second the user will move when walking normally and the joystick is at peak amplitude.
        /// </summary>
        public float MetersPerSecondNormal
        {
            get => _metersPerSecondNormal;
            set => _metersPerSecondNormal = value;
        }

        /// <summary>
        ///     Meters per second the user will move when sprinting and the joystick is at peak amplitude.
        /// </summary>
        public float MetersPerSecondSprint
        {
            get => _metersPerSecondSprint;
            set => _metersPerSecondSprint = value;
        }

        /// <summary>
        ///     Degrees per second the user will rotate when walking normally and the joystick is at peak amplitude.
        /// </summary>
        public float RotationDegreesPerSecondNormal
        {
            get => _rotationDegreesPerSecondNormal;
            set => _rotationDegreesPerSecondNormal = value;
        }

        /// <summary>
        ///     Degrees per second the user will rotate when sprinting and the joystick is at peak amplitude.
        /// </summary>
        public float RotationDegreesPerSecondSprint
        {
            get => _rotationDegreesPerSecondSprint;
            set => _rotationDegreesPerSecondSprint = value;
        }

        /// <summary>
        ///     Gravity when falling.
        /// </summary>
        public float Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        #endregion

        #region Public Overrides UxrLocomotion

        /// <inheritdoc />
        public override bool IsSmoothLocomotion => true;

        #endregion

        #region Unity

        /// <summary>
        ///     Tries to place the user on the ground.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            TryGround();
        }

        /// <summary>
        ///     Checks if the user needs to be placed on the ground.
        /// </summary>
        private void Update()
        {
            if (_initialized == false)
            {
                TryGround();
                _initialized = true;
            }
        }

        #endregion

        #region Protected Overrides UxrLocomotion

        /// <summary>
        ///     Gathers input and updates the physics parameters.
        /// </summary>
        protected override void UpdateLocomotion()
        {
            if (Avatar)
            {
                // Get input

                Vector2 joystickLeft  = Vector2.zero;
                Vector2 joystickRight = Vector2.zero;

                if (Avatar.ControllerInput.SetupType == UxrControllerSetupType.Dual)
                {
                    // Two controllers with joystick
                    joystickLeft  = Avatar.ControllerInput.GetInput2D(UxrHandSide.Left,  UxrInput2D.Joystick);
                    joystickRight = Avatar.ControllerInput.GetInput2D(UxrHandSide.Right, UxrInput2D.Joystick);
                }
                else if (Avatar.ControllerInput.SetupType == UxrControllerSetupType.Single)
                {
                    // Single controller with 2 joysticks (gamepad?)
                    joystickLeft  = Avatar.ControllerInput.GetInput2D(UxrHandSide.Left, UxrInput2D.Joystick);
                    joystickRight = Avatar.ControllerInput.GetInput2D(UxrHandSide.Left, UxrInput2D.Joystick2);
                }

                Vector3 offset = Vector3.zero;

                if (_walkDirection == UxrWalkDirection.ControllerForward)
                {
                    Transform forwardTransform = Avatar.GetControllerInputForward(UxrHandSide.Left);

                    if (forwardTransform != null)
                    {
                        offset = Vector3.ProjectOnPlane(forwardTransform.forward, Vector3.up).normalized * joystickLeft.y +
                                 Vector3.ProjectOnPlane(forwardTransform.right,   Vector3.up).normalized * joystickLeft.x;
                    }
                }
                else if (_walkDirection == UxrWalkDirection.AvatarForward)
                {
                    offset = Avatar.transform.forward * joystickLeft.y + Avatar.transform.right * joystickLeft.x;
                }
                else if (_walkDirection == UxrWalkDirection.LookDirection)
                {
                    offset = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.forward, Vector3.up).normalized * joystickLeft.y +
                             Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.right,   Vector3.up).normalized * joystickLeft.x;
                }

                if (offset.magnitude > 1.0f)
                {
                    offset.Normalize();
                }

                // Compute translation speed for UpdateLocomotionPhysics()

                bool isSprinting = Avatar.ControllerInput.GetButtonsPress(_sprintButtonHand, _sprintButton);

                float speed = isSprinting ? _metersPerSecondSprint : _metersPerSecondNormal;
                _translationSpeed = offset * speed;

                // Rotation. We perform it here since it doesn't require any collision checks.

                if (!Mathf.Approximately(joystickRight.x, 0.0f))
                {
                    float rotationSpeed = isSprinting ? _rotationDegreesPerSecondSprint : _rotationDegreesPerSecondNormal;
                    UxrManager.Instance.RotateAvatar(Avatar, joystickRight.x * rotationSpeed * Time.deltaTime);
                }

                UpdateLocomotionPhysics(Time.deltaTime);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the locomotion physics.
        /// </summary>
        /// <param name="deltaTime">The delta time in seconds</param>
        private void UpdateLocomotionPhysics(float deltaTime)
        {
            Vector3 avatarPos = Avatar.transform.position;
            Vector3 cameraPos = Avatar.CameraPosition;

            // Translation based on input

            if (_translationSpeed.magnitude > 0.0f && !UxrCameraWallFade.IsAvatarPeekingThroughGeometry(Avatar))
            {
                float   cameraHeight  = cameraPos.y - avatarPos.y;
                Vector3 capsuleTop    = cameraPos;
                Vector3 capsuleBottom = Avatar.CameraFloorPosition + Vector3.up * (_maxStepHeight * 3.0f + SafeFloorDistance);

                Vector3 newRequestedCameraPos = cameraPos + _translationSpeed * deltaTime;

                if (!HasBlockingCapsuleCastHit(Avatar, capsuleTop, capsuleBottom, _capsuleRadius, _translationSpeed.normalized, (newRequestedCameraPos - capsuleTop).magnitude, _collisionLayerMask, _triggerCollidersInteraction, out RaycastHit _))
                {
                    // Nothing in front. Now check for slope and maximum step height
                    if (HasBlockingRaycastHit(Avatar, cameraPos + _translationSpeed.normalized * _stepDistanceCheck, -Vector3.up, cameraHeight + _maxStepHeight, _collisionLayerMask, _triggerCollidersInteraction, out RaycastHit hitInfo))
                    {
                        float heightIncrement = hitInfo.point.y - avatarPos.y;
                        float slopeDegrees    = Mathf.Atan(heightIncrement / _stepDistanceCheck) * Mathf.Rad2Deg;

                        if (heightIncrement <= _maxStepHeight && slopeDegrees < _maxSlopeDegrees)
                        {
                            Vector3 cameraFloor = Avatar.CameraFloorPosition;
                            Vector3 translation = Vector3.Lerp(cameraFloor, hitInfo.point, _translationSpeed.magnitude * deltaTime / _stepDistanceCheck) - cameraFloor;

                            UxrManager.Instance.TranslateAvatar(Avatar, translation);
                        }

                        CheckSetAvatarParent(hitInfo);
                    }
                    else
                    {
                        // No collisions found, just keep walking. Probably to a fall.
                        UxrManager.Instance.TranslateAvatar(Avatar, _translationSpeed * deltaTime);
                    }
                }
            }

            // Check if needs to fall

            if (_isFalling)
            {
                // Falling

                if (HasBlockingRaycastHit(Avatar, avatarPos + Vector3.up * SafeFloorDistance, -Vector3.up, Mathf.Abs(_fallSpeed * deltaTime) + SafeFloorDistance * 2.0f, _collisionLayerMask, _triggerCollidersInteraction, out RaycastHit hitInfo))
                {
                    // Hit ground
                    _isFalling = false;
                    _fallSpeed = 0.0f;

                    UxrManager.Instance.MoveAvatarTo(Avatar, hitInfo.point.y);
                    CheckSetAvatarParent(hitInfo);
                }
                else
                {
                    // Keep falling
                    _fallSpeed += deltaTime * _gravity;

                    UxrManager.Instance.MoveAvatarTo(Avatar, Avatar.transform.position.y + _fallSpeed * deltaTime);
                }
            }
            else if (!_isFalling && !HasBlockingRaycastHit(Avatar, cameraPos, -Vector3.up, cameraPos.y - avatarPos.y + SafeFloorDistance, _collisionLayerMask, _triggerCollidersInteraction, out RaycastHit _))
            {
                // Start falling
                _isFalling = true;
                _fallSpeed = 0.0f;
            }
            else
            {
                _isFalling = false;
                _fallSpeed = 0.0f;
            }
        }

        /// <summary>
        ///     Checks whether to parent the avatar to a new transform.
        /// </summary>
        /// <param name="hitInfo">Raycast hit information with the potential parent collider</param>
        private void CheckSetAvatarParent(RaycastHit hitInfo)
        {
            if (_parentToDestination && hitInfo.collider.transform != null && Avatar.transform.parent != hitInfo.collider.transform)
            {
                Avatar.transform.SetParent(hitInfo.collider.transform);
            }
        }

        /// <summary>
        ///     Tries to place the user on the ground.
        /// </summary>
        private void TryGround()
        {
            if (Avatar)
            {
                _translationSpeed = Vector3.zero;
                _fallSpeed        = 0.0f;

                if (HasBlockingRaycastHit(Avatar, Avatar.transform.position + Vector3.up, -Vector3.up, 2.0f, _collisionLayerMask, _triggerCollidersInteraction, out RaycastHit hitInfo))
                {
                    UxrManager.Instance.MoveAvatarTo(Avatar, hitInfo.point);
                    CheckSetAvatarParent(hitInfo);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private const float SafeFloorDistance = 0.01f;

        private bool    _initialized;
        private Vector3 _translationSpeed;
        private bool    _isFalling;
        private float   _fallSpeed;

        #endregion
    }
}