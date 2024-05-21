// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotionBase.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.CameraUtils;
using UltimateXR.Core;
using UltimateXR.Core.Caching;
using UltimateXR.Devices;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Base component for teleport locomotion.
    /// </summary>
    public abstract partial class UxrTeleportLocomotionBase : UxrLocomotion, IUxrPrecacheable
    {
        #region Inspector Properties/Serialized Fields

        // General parameters

        [SerializeField] private UxrHandSide          _controllerHand       = UxrHandSide.Left;
        [SerializeField] private bool                 _useControllerForward = true;
        [SerializeField] private bool                 _parentToDestination;
        [SerializeField] private float                _shakeFilter              = 0.4f;
        [SerializeField] private UxrTranslationType   _translationType          = UxrTranslationType.Fade;
        [SerializeField] private Color                _fadeTranslationColor     = Color.black;
        [SerializeField] private float                _fadeTranslationSeconds   = UxrConstants.TeleportTranslationSeconds;
        [SerializeField] private float                _smoothTranslationSeconds = UxrConstants.TeleportTranslationSeconds;
        [SerializeField] private bool                 _allowJoystickBackStep    = true;
        [SerializeField] private float                _backStepDistance         = 2.0f;
        [SerializeField] private UxrRotationType      _rotationType             = UxrRotationType.Immediate;
        [SerializeField] private float                _rotationStepDegrees      = 45.0f;
        [SerializeField] private Color                _fadeRotationColor        = Color.black;
        [SerializeField] private float                _fadeRotationSeconds      = UxrConstants.TeleportRotationSeconds;
        [SerializeField] private float                _smoothRotationSeconds    = UxrConstants.TeleportRotationSeconds;
        [SerializeField] private UxrReorientationType _reorientationType        = UxrReorientationType.AllowUserJoystickRedirect;

        // Target

        [SerializeField] private UxrTeleportTarget _target;
        [SerializeField] private float             _targetPlacementAboveHit = 0.01f;
        [SerializeField] private bool              _showTargetAlsoWhenInvalid;
        [SerializeField] private Color             _validMaterialColorTargets   = Color.white;
        [SerializeField] private Color             _invalidMaterialColorTargets = Color.red;

        // Constraints

        [SerializeField] private QueryTriggerInteraction _triggerCollidersInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField] private float                   _maxAllowedDistance          = 20.0f;
        [SerializeField] private float                   _maxAllowedHeightDifference  = 10.0f;
        [SerializeField] private float                   _maxAllowedSlopeDegrees      = 30.0f;
        [SerializeField] private float                   _destinationValidationRadius = 0.25f;
        [SerializeField] private LayerMask               _validTargetLayers           = ~0;
        [SerializeField] private LayerMask               _blockingTargetLayers        = ~0;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when a destination validator using <see cref="AddDestinationValidator" /> with
        ///     <see cref="UxrDestinationValidatorMode.EveryFrame" /> invalidated a destination. If there is more than one
        ///     validator, it will be raised only by the first validator that returns false during the frame.
        /// </summary>
        public event Action<UxrTeleportDestination> ValidatorInvalidated;

        /// <summary>
        ///     Called when a destination validator using <see cref="AddDestinationValidator" /> with
        ///     <see cref="UxrDestinationValidatorMode.OnConfirmationOnly" /> canceled a destination. IF there is more than one
        ///     validator, it will be raised only by the first validator that returns false.
        /// </summary>
        public event Action<UxrTeleportDestination> ValidatorCanceled;

        /// <summary>
        ///     Gets the hand used to control the teleport component.
        /// </summary>
        public UxrHandSide HandSide => _controllerHand;

        /// <summary>
        ///     Gets or sets the teleport translation type.
        /// </summary>
        public UxrTranslationType TranslationType
        {
            get => _translationType;
            set => _translationType = value;
        }

        /// <summary>
        ///     Gets or sets the fade color when using <see cref="UxrTranslationType.Fade" /> translation teleporting.
        /// </summary>
        public Color FadeTranslationColor
        {
            get => _fadeTranslationColor;
            set => _fadeTranslationColor = value;
        }

        /// <summary>
        ///     Gets or sets the transition duration in seconds for the <see cref="UxrTranslationType.Fade" /> translation type.
        /// </summary>
        public float FadeTranslationSeconds
        {
            get => _fadeTranslationSeconds;
            set => _fadeTranslationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets the transition duration in seconds for the <see cref="UxrTranslationType.Smooth" /> translation type.
        /// </summary>
        public float SmoothTranslationSeconds
        {
            get => _smoothTranslationSeconds;
            set => _smoothTranslationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets whether the back-step is permitted.
        /// </summary>
        public bool AllowJoystickBackStep
        {
            get => _allowJoystickBackStep;
            set => _allowJoystickBackStep = value;
        }

        /// <summary>
        ///     Gets or sets the back-step distance.
        /// </summary>
        public float BackStepDistance
        {
            get => _backStepDistance;
            set => _backStepDistance = value;
        }

        /// <summary>
        ///     Gets or sets the teleport rotation type.
        /// </summary>
        public UxrRotationType RotationType
        {
            get => _rotationType;
            set => _rotationType = value;
        }

        /// <summary>
        ///     Gets or sets the amount of degrees rotated around the avatar axis when the user presses the left or right joystick
        ///     buttons.
        /// </summary>
        public float RotationStepDegrees
        {
            get => _rotationStepDegrees;
            set => _rotationStepDegrees = value;
        }

        /// <summary>
        ///     Gets or sets the fade color when using <see cref="UxrRotationType.Fade" /> translation teleporting.
        /// </summary>
        public Color FadeRotationColor
        {
            get => _fadeRotationColor;
            set => _fadeRotationColor = value;
        }

        /// <summary>
        ///     Gets or sets the transition duration in seconds for the <see cref="UxrRotationType.Fade" /> rotation type.
        /// </summary>
        public float FadeRotationSeconds
        {
            get => _fadeRotationSeconds;
            set => _fadeRotationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets the transition duration in seconds for the <see cref="UxrRotationType.Smooth" /> rotation type.
        /// </summary>
        public float SmoothRotationSeconds
        {
            get => _smoothRotationSeconds;
            set => _smoothRotationSeconds = value;
        }

        /// <summary>
        ///     Gets or sets how the teleport target direction is set.
        /// </summary>
        public UxrReorientationType ReorientationType
        {
            get => _reorientationType;
            set => _reorientationType = value;
        }

        /// <summary>
        ///     Gets or sets the target object.
        /// </summary>
        public UxrTeleportTarget Target
        {
            get => _teleportTarget;
            set => _teleportTarget = value;
        }

        /// <summary>
        ///     Gets or sets the distance above the ground the target is positioned.
        /// </summary>
        public float TargetPlacementAboveHit
        {
            get => _targetPlacementAboveHit;
            set => _targetPlacementAboveHit = value;
        }

        /// <summary>
        ///     Gets or sets whether the target should also be visible when the teleport destination is not valid.
        /// </summary>
        public bool ShowTargetAlsoWhenInvalid
        {
            get => _showTargetAlsoWhenInvalid;
            set => _showTargetAlsoWhenInvalid = value;
        }

        /// <summary>
        ///     When <see cref="ShowTargetAlsoWhenInvalid" /> is true, sets the teleport target color used when the destination is
        ///     valid.
        /// </summary>
        public Color ValidMaterialColorTargets
        {
            get => _validMaterialColorTargets;
            set => _validMaterialColorTargets = value;
        }

        /// <summary>
        ///     When <see cref="ShowTargetAlsoWhenInvalid" /> is true, sets the teleport target color used when the destination is
        ///     invalid.
        /// </summary>
        public Color InvalidMaterialColorTargets
        {
            get => _invalidMaterialColorTargets;
            set => _invalidMaterialColorTargets = value;
        }

        /// <summary>
        ///     Gets or sets the behaviour for raycasts against trigger volumes.
        /// </summary>
        public QueryTriggerInteraction TriggerCollidersInteraction
        {
            get => _triggerCollidersInteraction;
            set => _triggerCollidersInteraction = value;
        }

        /// <summary>
        ///     Gets or sets the maximum teleport distance.
        /// </summary>
        public float MaxAllowedDistance
        {
            get => _maxAllowedDistance;
            set => _maxAllowedDistance = value;
        }

        /// <summary>
        ///     Gets or sets the maximum height difference allowed from the current position to a destination.
        /// </summary>
        public float MaxAllowedHeightDifference
        {
            get => _maxAllowedHeightDifference;
            set => _maxAllowedHeightDifference = value;
        }

        /// <summary>
        ///     Gets or sets the maximum slop for a destination to be considered valid.
        /// </summary>
        public float MaxAllowedSlopeDegrees
        {
            get => _maxAllowedSlopeDegrees;
            set => _maxAllowedSlopeDegrees = value;
        }

        /// <summary>
        ///     Gets or sets the radius of a cylinder used when validating if a teleport destination is valid.
        /// </summary>
        public float DestinationValidationRadius
        {
            get => _destinationValidationRadius;
            set => _destinationValidationRadius = value;
        }

        /// <summary>
        ///     Gets or sets the layers over which teleportation is allowed.
        /// </summary>
        public LayerMask ValidTargetLayers
        {
            get => _validTargetLayers;
            set => _validTargetLayers = value;
        }

        /// <summary>
        ///     Gets or sets the layers which should be considered when ray-casting looking for either valid or invalid
        ///     teleportation surfaces.
        /// </summary>
        public LayerMask BlockingTargetLayers
        {
            get => _blockingTargetLayers;
            set => _blockingTargetLayers = value;
        }

        #endregion

        #region Implicit IUxrPrecacheable

        /// <inheritdoc />
        public IEnumerable<GameObject> PrecachedInstances
        {
            get
            {
                if (Target != null)
                {
                    yield return Target.gameObject;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds a destination validator, which can cancel a teleport based on custom conditions.
        /// </summary>
        /// <param name="validator">
        ///     The destination validator, a function that receives a <see cref="UxrTeleportDestination" /> and
        ///     returns a boolean telling whether the teleport can be executed or not
        /// </param>
        /// <param name="mode">The validator execution mode</param>
        /// <exception cref="ArgumentNullException">The validator is null</exception>
        public void AddDestinationValidator(Func<UxrTeleportDestination, bool> validator, UxrDestinationValidatorMode mode)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _destinationValidators.Add(new Validator(validator, mode));
        }

        /// <summary>
        ///     Removes a destination validator added using <see cref="AddDestinationValidator" />.
        /// </summary>
        /// <param name="validator">Validator to remove</param>
        /// <returns>Whether the validator was removed, or false if the validator was not found</returns>
        /// <exception cref="ArgumentNullException">the validator function is null</exception>
        public bool RemoveDestinationValidator(Func<UxrTeleportDestination, bool> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            for (int i = 0; i < _destinationValidators.Count; ++i)
            {
                if (_destinationValidators[i].ValidatorFunc == validator)
                {
                    _destinationValidators.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Removes all destination validators added using <see cref="AddDestinationValidator" />.
        /// </summary>
        public void RemoveAllDestinationValidators()
        {
            _destinationValidators.Clear();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component. Should also be called in child classes.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Look for other avatar teleports

            _otherAvatarTeleports = new List<UxrTeleportLocomotionBase>();

            UxrTeleportLocomotionBase[] allAvatarTeleports = Avatar.GetComponentsInChildren<UxrTeleportLocomotionBase>();

            _otherAvatarTeleports.AddRange(allAvatarTeleports.Where(teleport => teleport != this));

            // If the teleport target is a prefab, instantiate. Otherwise just reference the object in the scene

            if (_target.IsInPrefab())
            {
                _teleportTarget = Instantiate(_target, Avatar.transform);
            }
            else
            {
                _teleportTarget = _target;

                if (_teleportTarget != null)
                {
                    _teleportTarget.transform.parent = Avatar.transform;
                }
            }

            // Set initial state

            if (_teleportTarget != null)
            {
                _teleportTarget.transform.rotation = Avatar.transform.rotation;
                TeleportReference                  = null;
                TeleportLocalDirection             = Avatar.ProjectedCameraForward;
                TeleportLocalPosition              = Avatar.transform.position;
            }

            _layerMaskRaycast.value = BlockingTargetLayers.value | ValidTargetLayers.value;
        }

        /// <summary>
        ///     Resets the component and subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrAvatar.GlobalAvatarMoved += UxrAvatar_GlobalAvatarMoved;
            UxrManager.AvatarsUpdated   += UxrManager_AvatarsUpdated;

            EnableTeleportObjects(false, false);

            _isBackStepAvailable = true;
            _isValidTeleport     = false;
            IsTeleporting        = false;
            ControllerStart      = RawControllerStart;
            ControllerForward    = RawControllerForward;
        }

        /// <summary>
        ///     Clear some states and unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrAvatar.GlobalAvatarMoved -= UxrAvatar_GlobalAvatarMoved;
            UxrManager.AvatarsUpdated   -= UxrManager_AvatarsUpdated;

            NotifyTeleportSpawnCollider(null);

            EnableTeleportObjects(false, false);
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the avatar moved.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_GlobalAvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            if (ReferenceEquals(sender, Avatar))
            {
                ControllerStart   = RawControllerStart;
                ControllerForward = RawControllerForward;
            }
        }

        /// <summary>
        ///     When the avatar is in UpdateExternally mode, still smooth the transforms to support multiplayer and replays.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (Avatar.AvatarMode != UxrAvatarMode.Local)
            {
                UpdateSmoothTransforms();
            }
        }

        #endregion

        #region Protected Overrides UxrLocomotion

        /// <inheritdoc />
        protected override void UpdateLocomotion()
        {
            if (Avatar == null)
            {
                return;
            }

            // Check for back-step and rotations

            if (Avatar.ControllerInput.GetButtonsPressUp(_controllerHand, UxrInputButtons.JoystickDown))
            {
                _isBackStepAvailable = true;
            }

            bool backStepInput = false;

            if (IsAllowedToTeleport)
            {
                backStepInput = _allowJoystickBackStep && CanBackStep && Avatar.ControllerInput.GetButtonsPress(_controllerHand, UxrInputButtons.JoystickDown);

                if (RotationType != UxrRotationType.NotAllowed)
                {
                    if (Avatar.ControllerInput.GetButtonsPressDown(_controllerHand, UxrInputButtons.JoystickLeft) && CanRotate)
                    {
                        Rotate(-RotationStepDegrees);
                        return;
                    }

                    if (Avatar.ControllerInput.GetButtonsPressDown(_controllerHand, UxrInputButtons.JoystickRight) && CanRotate)
                    {
                        Rotate(RotationStepDegrees);
                        return;
                    }
                }
            }

            // Back step?

            if (backStepInput && _isBackStepAvailable && IsAllowedToTeleport)
            {
                Vector3 newPosition = Avatar.CameraFloorPosition - Avatar.ProjectedCameraForward * _backStepDistance;

                if (HasBlockingRaycastHit(Avatar,
                                          newPosition + UpVector * RaycastAboveGround,
                                          -UpVector,
                                          _maxAllowedHeightDifference > 0.0f ? _maxAllowedHeightDifference : RaycastLongDistance,
                                          BlockingTargetLayers,
                                          TriggerCollidersInteraction,
                                          out RaycastHit backStepRaycast))
                {
                    if (NotifyDestinationRaycast(backStepRaycast, true, out bool _))
                    {
                        _isBackStepAvailable   = false;
                        TeleportLocalDirection = TransformExt.GetLocalDirection(TeleportReference, Avatar.ProjectedCameraForward);
                        TryTeleportUsingCurrentTarget();
                        return;
                    }
                }
            }

            // Update smoothing of transforms that track the hands

            UpdateSmoothTransforms();

            // Update locomotion in child classes

            UpdateTeleportLocomotion();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Can be overriden in child classes to execute the additional per-frame teleport locomotion logic.
        /// </summary>
        protected virtual void UpdateTeleportLocomotion()
        {
        }

        /// <summary>
        ///     Cancels the current teleport target. When overriden in child classes the base class should be called too.
        /// </summary>
        protected virtual void CancelTarget()
        {
            EnableTeleportObjects(false, false);
            _isValidTeleport = false;
        }

        /// <summary>
        ///     Checks if the teleport position is valid.
        /// </summary>
        /// <param name="checkBlockingInBetween">
        ///     Should it check for blocking elements in a straight line from the current position to the new position?
        /// </param>
        /// <param name="newPosition">
        ///     Teleport position. If should passed as reference because it may have slight corrections
        /// </param>
        /// <param name="hitNormal">The hit normal that generated the teleport position candidate</param>
        /// <param name="isValidSlope">Returns a boolean telling if the slope at the destination is valid or not</param>
        /// <returns>Boolean telling whether the new position is a valid teleport destination or not</returns>
        protected bool IsValidTeleport(bool checkBlockingInBetween, ref Vector3 newPosition, Vector3 hitNormal, out bool isValidSlope)
        {
            isValidSlope = true;

            if (!IsAllowedToTeleport)
            {
                return false;
            }

            Vector3 localNewPosition = Avatar.transform.InverseTransformPoint(newPosition);

            if (Mathf.Abs(localNewPosition.y) > MaxAllowedHeightDifference)
            {
                return false;
            }

            float   eyeHeight   = Avatar.transform.InverseTransformPoint(Avatar.CameraPosition).y;
            Vector3 eyePosStart = Avatar.CameraPosition;
            Vector3 eyePosEnd   = newPosition + UpVector * eyeHeight;

            // Check if there is something blocking in a straight line if requested, used in a back step

            if (checkBlockingInBetween)
            {
                Vector3 direction = eyePosEnd - eyePosStart;

                if (HasBlockingRaycastHit(eyePosStart, direction.normalized, direction.magnitude, out RaycastHit _))
                {
                    // There is something blocking in between
                    return false;
                }
            }
            else
            {
                // First perform a sphere test on the place where the head would be teleported to see if we can have an early negative.
                if (Physics.CheckSphere(eyePosEnd, HeadRadius, BlockingTargetLayers, TriggerCollidersInteraction))
                {
                    return false;
                }

                if (MaxAllowedHeightDifference > 0.0f && Vector3.Angle(hitNormal, UpVector) > MaxAllowedSlopeDegrees)
                {
                    // Check if we are hitting a tall enough wall to see if we can have an early negative. This avoids the filtering
                    // below to allow climbing up the first portion of the wall.
                    // What we do is raycast in an inclined upwards direction to see if the wall is significantly enough above the raycast.

                    Vector3 rayStart = newPosition + hitNormal * 0.1f;
                    Vector3 rayEnd   = newPosition + UpVector * MaxAllowedHeightDifference;

                    if (HasBlockingRaycastHit(Avatar,
                                              rayStart,
                                              (rayEnd - rayStart).normalized,
                                              Vector3.Distance(rayStart, rayEnd) + 0.01f,
                                              BlockingTargetLayers,
                                              TriggerCollidersInteraction,
                                              out RaycastHit _))
                    {
                        // We are hitting the base of a tall wall
                        isValidSlope = false;
                        return false;
                    }
                }
            }

            // If not, we want to check also a radius around the destination pos. If a certain number of positions within
            // this radius are valid we consider it also a valid destination. This removes some unwanted false negatives due
            // to small height or slope differences on the ground.

            int positives   = 0;
            int validSlopes = 0;

            for (int i = 0; i < DestinationValidationSubdivisions; ++i)
            {
                float offsetT = 1.0f / DestinationValidationSubdivisions * 0.5f;
                float radians = Mathf.PI * 2.0f * (i * (1.0f / DestinationValidationSubdivisions) + offsetT);

                Vector3 offset = new Vector3(Mathf.Cos(radians), 0.0f, Mathf.Sin(radians));

                if (IsValidDestination(newPosition, eyePosEnd + offset.normalized * DestinationValidationRadius, out bool isValidSlopeSubdivision))
                {
                    positives++;
                }

                validSlopes += isValidSlopeSubdivision ? 1 : 0;
            }

            isValidSlope = validSlopes >= DestinationValidationPositivesNeeded;

            return positives >= DestinationValidationPositivesNeeded && isValidSlope;
        }

        /// <summary>
        ///     Cancels all other current teleport targets. When overriden in child classes the base class should be called too.
        /// </summary>
        protected void CancelOtherTeleportTargets()
        {
            foreach (UxrTeleportLocomotionBase otherTeleport in _otherAvatarTeleports)
            {
                otherTeleport.CancelTarget();
            }
        }

        /// <summary>
        ///     Checks whether the given raycast hits have any that are blocking. A blocking raycast can either be a valid or
        ///     invalid teleport destination depending on many factors. Use <see cref="IsValidDestination" /> to check whether the
        ///     given position is valid.
        ///     This method filters out invalid raycasts such as against anything part of an avatar or a grabbed object.
        /// </summary>
        /// <param name="origin">Ray origin</param>
        /// <param name="direction">Ray direction</param>
        /// <param name="maxDistance">Raycast maximum distance</param>
        /// <param name="outputHit">Result blocking raycast</param>
        /// <returns>Whether there is a blocking raycast returned in <paramref name="outputHit" /></returns>
        protected bool HasBlockingRaycastHit(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit outputHit)
        {
            return HasBlockingRaycastHit(Avatar, origin, direction, maxDistance, LayerMaskRaycast, TriggerCollidersInteraction, out outputHit);
        }

        /// <summary>
        ///     Notifies a raycast was selected to be a potential destination. Computes whether the destination is valid. If it is,
        ///     sets the appropriate internal state that can later be executed using <see cref="TryTeleportUsingCurrentTarget" />.
        /// </summary>
        /// <param name="hit">Raycast that will be processed as a potential teleport destination</param>
        /// <param name="checkBlockingInBetween">
        ///     Should it check for blocking elements in a straight line from the current position to the new position?
        /// </param>
        /// <param name="isTargetEnabled">Will return whether the target was enabled</param>
        /// <returns>Whether the destination is a valid teleport location</returns>
        protected bool NotifyDestinationRaycast(RaycastHit hit, bool checkBlockingInBetween, out bool isTargetEnabled)
        {
            _isValidTeleport = true;
            isTargetEnabled  = true;

            UxrIgnoreTeleportDestination ignoreDestinationComponent = hit.collider.GetComponentInParent<UxrIgnoreTeleportDestination>();

            bool ignoreDestination = ignoreDestinationComponent != null && ignoreDestinationComponent.enabled;
            TeleportReference = hit.collider != null ? hit.collider.transform : null;

            _hitInfo = hit;

            // Check for UxrTeleportSpawnCollider component

            UxrTeleportSpawnCollider teleportSpawnCollider = hit.collider.GetComponentInParent<UxrTeleportSpawnCollider>();

            NotifyTeleportSpawnCollider(teleportSpawnCollider);

            if (teleportSpawnCollider != null && teleportSpawnCollider.enabled && !ignoreDestination)
            {
                Transform spawnPos = teleportSpawnCollider.GetSpawnPos(Avatar, out Vector3 _);

                if (spawnPos != null)
                {
                    TeleportReference      = spawnPos;
                    TeleportLocalPosition  = TransformExt.GetLocalPosition(TeleportReference, spawnPos.position);
                    TeleportLocalDirection = TransformExt.GetLocalDirection(TeleportReference, Vector3.ProjectOnPlane(spawnPos.forward, spawnPos.up));

                    _isValidTeleport = true;
                    isTargetEnabled  = true;
                    EnableTeleportObjects(isTargetEnabled, _isValidTeleport);
                }
            }
            else
            {
                Vector3 teleportPos            = hit.point;
                Vector3 teleportLocalDirection = Vector3.zero;

                isTargetEnabled = true;

                // Compute the new local avatar direction

                if (ReorientationType == UxrReorientationType.KeepOrientation)
                {
                    teleportLocalDirection = TransformExt.GetLocalDirection(TeleportReference, Avatar.ProjectedCameraForward);
                }
                else if (ReorientationType == UxrReorientationType.UseTeleportFromToDirection)
                {
                    teleportLocalDirection = TransformExt.GetLocalDirection(TeleportReference, Vector3.ProjectOnPlane(teleportPos - Avatar.CameraPosition, UpVector));
                }
                else if (ReorientationType == UxrReorientationType.AllowUserJoystickRedirect)
                {
                    Vector2 joystickValue     = Avatar.ControllerInput.GetInput2D(HandSide, UxrInput2D.Joystick);
                    Vector3 projectedForward  = Vector3.ProjectOnPlane(ControllerForward, UpVector).normalized;
                    Vector3 joystickDirection = new Vector3(joystickValue.x, 0.0f, joystickValue.y).normalized;

                    teleportLocalDirection = TransformExt.GetLocalDirection(TeleportReference, Quaternion.LookRotation(projectedForward, UpVector) * Quaternion.LookRotation(joystickDirection, UpVector) * Vector3.forward);
                }

                // Run "EveryFrame" validators if there are any

                UxrTeleportDestination destination = null;

                foreach (Validator validator in _destinationValidators)
                {
                    if (validator.Mode == UxrDestinationValidatorMode.EveryFrame)
                    {
                        if (destination == null)
                        {
                            destination = new UxrTeleportDestination(hit, teleportPos, Quaternion.LookRotation(TransformExt.GetWorldDirection(TeleportReference, teleportLocalDirection), UpVector));
                        }

                        if (!validator.ValidatorFunc.Invoke(destination))
                        {
                            _isValidTeleport = false;
                            isTargetEnabled  = ShowTargetAlsoWhenInvalid;
                            ValidatorInvalidated?.Invoke(destination);
                            break;
                        }
                    }
                }

                if (_isValidTeleport)
                {
                    // Run internal validation

                    _isValidTeleport = IsValidTeleport(checkBlockingInBetween, ref teleportPos, hit.normal, out bool validSlope) && !ignoreDestination;

                    if (_isValidTeleport && validSlope)
                    {
                        // Hit against valid target
                        EnableTeleportObjects(true, _isValidTeleport);
                    }
                    else
                    {
                        // Hit against blocking object or invalid slope
                        _isValidTeleport = false;
                        isTargetEnabled  = ShowTargetAlsoWhenInvalid && validSlope;
                        EnableTeleportObjects(isTargetEnabled, _isValidTeleport);
                    }
                }

                if (isTargetEnabled)
                {
                    // Place target

                    TeleportLocalPosition = TransformExt.GetLocalPosition(TeleportReference, teleportPos);

                    if (Avatar.AvatarMode == UxrAvatarMode.Local)
                    {
                        TeleportLocalDirection = teleportLocalDirection;
                    }
                }
            }

            return _isValidTeleport;
        }

        /// <summary>
        ///     Notifies that no raycast were found to be processed as a  potential teleport destination.
        /// </summary>
        protected void NotifyNoDestinationRaycast()
        {
            _isValidTeleport = false;
            EnableTeleportObjects(false, false);
            NotifyTeleportSpawnCollider(null);
        }

        /// <summary>
        ///     Tries to teleport the avatar using the current <see cref="TeleportPosition" /> and <see cref="TeleportDirection" />
        ///     values, only if the current destination is valid and the avatar isn't currently being teleported.
        /// </summary>
        protected void TryTeleportUsingCurrentTarget()
        {
            // Teleport if we can!

            if (_isValidTeleport && !IsTeleporting && !IsOtherComponentTeleporting && IsAllowedToTeleport)
            {
                Transform  avatarTransform = Avatar.transform;
                Vector3    avatarPos       = avatarTransform.position;
                Vector3    avatarUp        = avatarTransform.up;
                Quaternion avatarRot       = avatarTransform.rotation;

                if (TranslationType == UxrTranslationType.Fade)
                {
                    UxrManager.Instance.TeleportFadeColor = FadeTranslationColor;
                }

                bool parentToDestination = ParentToDestination;

                if (TeleportReference != null && TeleportReference.TryGetComponent(out UxrParentAvatarDestination parentAvatarDestination))
                {
                    parentToDestination = parentAvatarDestination.ParentAvatar;
                }

                UxrTeleportSpawnCollider spawnCollider = _lastSpawnCollider;

                Vector3    teleportPos = TransformExt.GetWorldPosition(TeleportReference, TeleportLocalPosition);
                Quaternion teleportRot = Quaternion.LookRotation(TransformExt.GetWorldDirection(TeleportReference, TeleportLocalDirection), UpVector);

                // Run validators if there are any

                bool isValid = true;

                if (spawnCollider == null)
                {
                    UxrTeleportDestination destination = null;

                    foreach (Validator validator in _destinationValidators)
                    {
                        if (validator.Mode == UxrDestinationValidatorMode.OnConfirmationOnly)
                        {
                            if (destination == null)
                            {
                                destination = new UxrTeleportDestination(_hitInfo, teleportPos, teleportRot);
                            }

                            if (!validator.ValidatorFunc.Invoke(destination))
                            {
                                ValidatorCanceled?.Invoke(destination);
                                isValid = false;
                                break;
                            }
                        }
                    }
                }

                if (isValid)
                {
                    IsTeleporting = true;

                    UxrManager.Instance.TeleportLocalAvatarRelative(TeleportReference,
                                                                    parentToDestination,
                                                                    teleportPos,
                                                                    teleportRot,
                                                                    _translationType,
                                                                    TranslationSeconds,
                                                                    () =>
                                                                    {
                                                                        if (spawnCollider != null)
                                                                        {
                                                                            spawnCollider.RaiseTeleported(Avatar, new UxrAvatarMoveEventArgs(avatarPos, avatarRot, Avatar.CameraFloorPosition, Quaternion.LookRotation(Avatar.ProjectedCameraForward, avatarUp)));
                                                                        }
                                                                    },
                                                                    finished =>
                                                                    {
                                                                        _isValidTeleport  = false;
                                                                        IsTeleporting     = false;
                                                                        ControllerStart   = RawControllerStart;
                                                                        ControllerForward = RawControllerForward;
                                                                    });
                }
            }

            NotifyTeleportSpawnCollider(null);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Applies smoothing to the source transforms to avoid too much jitter.
        /// </summary>
        private void UpdateSmoothTransforms()
        {
            if (_shakeFilter > 0.0f && IsTeleporting && IsOtherComponentTeleporting == false)
            {
                float deltaTimeMultiplier = Mathf.Lerp(DeltaTimeMultiplierFilterMin, DeltaTimeMultiplierFilterMax, Mathf.Clamp01(_shakeFilter));

                ControllerStart   = Vector3.Lerp(ControllerStart,   RawControllerStart,   Time.deltaTime * deltaTimeMultiplier);
                ControllerForward = Vector3.Lerp(ControllerForward, RawControllerForward, Time.deltaTime * deltaTimeMultiplier);
            }
            else
            {
                ControllerStart   = RawControllerStart;
                ControllerForward = RawControllerForward;
            }
        }

        /// <summary>
        ///     Checks if a given teleport position is valid. We use the eye position of the teleport destination as
        ///     a reference to be able to raycast to the ground and check for valid layers and slope angle and if there
        ///     is a discrepancy between the raycast and the expected floor height.
        /// </summary>
        /// <param name="teleportPos">The floor level position of the teleport</param>
        /// <param name="newEyePos">The eye position that will be used as reference for the teleport destination</param>
        /// <param name="isValidSlope">Will return if it is a valid slope</param>
        /// <returns>Boolean telling whether newEyePos is a valid teleport position or not</returns>
        private bool IsValidDestination(Vector3 teleportPos, Vector3 newEyePos, out bool isValidSlope)
        {
            isValidSlope = false;

            Vector3 localNewEyePos   = Avatar.transform.InverseTransformPoint(newEyePos);
            Vector3 localTeleportPos = Avatar.transform.InverseTransformPoint(teleportPos);
            float   eyeHeight        = localNewEyePos.y - localTeleportPos.y;

            if (HasBlockingRaycastHit(Avatar, newEyePos, -UpVector, eyeHeight * 1.2f, LayerMaskRaycast, TriggerCollidersInteraction, out RaycastHit hit))
            {
                float slopeDegrees = Mathf.Abs(Vector3.Angle(hit.normal, UpVector));

                isValidSlope = slopeDegrees < MaxAllowedSlopeDegrees;
                bool valid = isValidSlope && (ValidTargetLayers.value & 1 << hit.collider.gameObject.layer) != 0;

                Vector3 localHitPoint = Avatar.transform.InverseTransformPoint(hit.point);
                valid = valid && Mathf.Abs(localHitPoint.y - localTeleportPos.y) < MaxVerticalHeightDisparity;

                UxrIgnoreTeleportDestination ignoreDestinationComponent = hit.collider.GetComponentInParent<UxrIgnoreTeleportDestination>();

                if (ignoreDestinationComponent != null && ignoreDestinationComponent.enabled)
                {
                    return false;
                }

                if (valid)
                {
                    // Raycast upwards to see if there is something between the ground and eye level. Since we can be teleported inside a box
                    // at eye level for instance, the previous raycast will not handle that case. We need to raycast from outside as well

                    return !HasBlockingRaycastHit(Avatar, teleportPos, UpVector, eyeHeight, LayerMaskRaycast, TriggerCollidersInteraction, out RaycastHit _);
                }
            }

            return false;
        }

        /// <summary>
        ///     Notifies a change in the currently targeted <see cref="UxrTeleportSpawnCollider" /> component.
        /// </summary>
        /// <param name="teleportSpawnCollider">New currently targeted component or null if none is selected</param>
        private void NotifyTeleportSpawnCollider(UxrTeleportSpawnCollider teleportSpawnCollider)
        {
            if (teleportSpawnCollider && teleportSpawnCollider.enabled)
            {
                if (_lastSpawnCollider != null && teleportSpawnCollider != _lastSpawnCollider && _lastSpawnCollider.EnableWhenSelected)
                {
                    _lastSpawnCollider.EnableWhenSelected.SetActive(false);
                }
                else if (_lastSpawnCollider != teleportSpawnCollider && teleportSpawnCollider.EnableWhenSelected)
                {
                    if (teleportSpawnCollider.EnableWhenSelected.activeSelf == false)
                    {
                        teleportSpawnCollider.EnableWhenSelected.SetActive(true);
                    }
                }

                _lastSpawnCollider = teleportSpawnCollider;
            }
            else
            {
                if (_lastSpawnCollider != null && _lastSpawnCollider.EnableWhenSelected)
                {
                    _lastSpawnCollider.EnableWhenSelected.SetActive(false);
                }

                _lastSpawnCollider = null;
            }
        }

        /// <summary>
        ///     Enables or disables teleport graphical components.
        /// </summary>
        /// <param name="enableTarget">Whether to enable the teleport target object</param>
        /// <param name="validTeleport">Whether the current teleport destination is valid</param>
        private void EnableTeleportObjects(bool enableTarget, bool validTeleport)
        {
            _teleportTargetEnabled = enableTarget;
            _teleportTargetValid   = validTeleport;
            
            // Enable / disable

            if (_teleportTarget != null)
            {
                _teleportTarget.gameObject.SetActive(enableTarget);
            }

            // Set materials

            if (ShowTargetAlsoWhenInvalid)
            {
                _teleportTarget.SetMaterialColor(validTeleport ? ValidMaterialColorTargets : InvalidMaterialColorTargets);
            }
        }

        /// <summary>
        ///     Rotates the avatar around its vertical axis, where a positive angle turns it to the right and a negative angle to
        ///     the left.
        /// </summary>
        /// <param name="degrees">Degrees to rotate</param>
        private void Rotate(float degrees)
        {
            if (!IsTeleporting && !IsOtherComponentTeleporting)
            {
                IsTeleporting = true;

                Transform avatarTransform = Avatar.transform;

                if (RotationType == UxrRotationType.Fade)
                {
                    UxrManager.Instance.TeleportFadeColor = FadeRotationColor;
                }

                UxrManager.Instance.RotateLocalAvatar(degrees,
                                                      RotationType,
                                                      RotationSeconds,
                                                      null,
                                                      finished =>
                                                      {
                                                          IsTeleporting     = false;
                                                          ControllerStart   = RawControllerStart;
                                                          ControllerForward = RawControllerForward;
                                                      });
            }

            NotifyTeleportSpawnCollider(null);
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets whether the avatar can currently receive input to step backwards.
        /// </summary>
        protected virtual bool CanBackStep => true;

        /// <summary>
        ///     Gets whether the avatar can currently receive input to rotate around.
        /// </summary>
        protected virtual bool CanRotate => true;

        /// <summary>
        ///     Gets whether other teleport component is currently teleporting the avatar.
        /// </summary>
        protected bool IsOtherComponentTeleporting => _otherAvatarTeleports != null && _otherAvatarTeleports.Any(otherTeleport => otherTeleport.IsTeleporting);

        /// <summary>
        ///     Gets whether the component is currently allowed to teleport the avatar.
        /// </summary>
        protected bool IsAllowedToTeleport
        {
            get
            {
                if (IsTeleporting)
                {
                    // Component is currently teleporting
                    return false;
                }

                if (UxrCameraWallFade.IsAvatarPeekingThroughGeometry(Avatar))
                {
                    // Head is currently inside a wall. Avoid teleportation for "cheating".
                    return false;
                }

                Vector3 cameraPos          = Avatar.CameraPosition;
                Vector3 cameraToController = ControllerStart - cameraPos;

                return !HasBlockingRaycastHit(cameraPos, cameraToController.normalized, cameraToController.magnitude, out RaycastHit hit);
            }
        }

        /// <summary>
        ///     Gets the <see cref="LayerMask" /> used for ray-casting either valid or invalid teleport destinations.
        /// </summary>
        protected LayerMask LayerMaskRaycast => _layerMaskRaycast;

        /// <summary>
        ///     Gets the up vector used to compute rotations so that it is always computed in the correct space.
        /// </summary>
        protected Vector3 UpVector => Avatar.transform.up;

        /// <summary>
        ///     Gets or sets whether to parent the avatar to the destination object (<see cref="TeleportReference" />) after
        ///     teleporting.
        ///     This can also be overriden using a <see cref="UxrParentAvatarDestination" /> component.
        /// </summary>
        protected bool ParentToDestination
        {
            get => _parentToDestination;
            set => _parentToDestination = value;
        }

        /// <summary>
        ///     Gets or sets whether the component is currently teleporting the avatar.
        /// </summary>
        protected bool IsTeleporting { get; private set; }

        /// <summary>
        ///     Gets the smoothed source of ray-casting when it starts on the controller.
        /// </summary>
        protected Vector3 ControllerStart { get; private set; }

        /// <summary>
        ///     Gets the smoothed direction of ray-casting when starts on the controller.
        /// </summary>
        protected Vector3 ControllerForward { get; private set; }

        /// <summary>
        ///     Gets or sets the transform that will be used as reference for <see cref="TeleportPosition" /> and
        ///     <see cref="TeleportDirection" /> to keep the relative positioning/orientation to while performing potential
        ///     transitions, such as fades, before the actual teleporting. It is usually assigned the transform of the object that
        ///     was hit with the destination raycast.
        ///     The reference transform is used to make teleport transitions work correctly when the avatar is on a moving object.
        ///     Without it, using absolute position and rotation only, would spawn the avatar with an incorrect offset due to the
        ///     delay the transition introduces before the teleport.
        /// </summary>
        protected Transform TeleportReference { get; private set; }

        /// <summary>
        ///     Gets or sets the current teleport destination in <see cref="TeleportReference" /> space. If
        ///     <see cref="TeleportReference" /> is null, it will be considered as world-space position.
        /// </summary>
        protected Vector3 TeleportLocalPosition
        {
            get => _teleportLocalPosition;
            set
            {
                _teleportLocalPosition = value;

                if (_teleportTarget != null)
                {
                    _teleportTarget.transform.position = TransformExt.GetWorldPosition(TeleportReference, value) + UpVector * TargetPlacementAboveHit;
                    _teleportTarget.OrientArrow(Quaternion.LookRotation(TransformExt.GetWorldDirection(TeleportReference, TeleportLocalDirection), UpVector));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the current teleport direction in in <see cref="TeleportReference" /> space. If
        ///     <see cref="TeleportReference" /> is null, it will be considered as world-space rotation.
        /// </summary>
        protected Vector3 TeleportLocalDirection
        {
            get => _teleportLocalDirection;
            set
            {
                _teleportLocalDirection = value;

                if (_teleportTarget != null)
                {
                    _teleportTarget.OrientArrow(Quaternion.LookRotation(TeleportReference != null ? TeleportReference.rotation * value : value, UpVector));
                }
            }
        }

        /// <summary>
        ///     Gets or sets teleport arrow's local rotation.
        /// </summary>
        protected Quaternion TeleportArrowLocalRotation
        {
            get => _teleportTarget.ReorientArrowLocalRotation;
            set => _teleportTarget.ReorientArrowLocalRotation = value;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the raw unprocessed world position on the controller where the ray-casting starts.
        /// </summary>
        private Vector3 RawControllerStart
        {
            get
            {
                if (_useControllerForward && Avatar != null)
                {
                    Transform forwardTransform = Avatar.GetControllerInputForward(_controllerHand);
                    return forwardTransform ? forwardTransform.position : transform.position;
                }

                return transform.position;
            }
        }

        /// <summary>
        ///     Gets the raw unprocessed world direction on the controller used for ray-casting.
        /// </summary>
        private Vector3 RawControllerForward
        {
            get
            {
                if (_useControllerForward && Avatar != null)
                {
                    Transform forwardTransform = Avatar.GetControllerInputForward(_controllerHand);
                    return forwardTransform ? forwardTransform.forward : transform.forward;
                }

                return transform.forward;
            }
        }

        /// <summary>
        ///     Gets the translation transition in seconds depending on <see cref="TranslationType" />.
        /// </summary>
        private float TranslationSeconds
        {
            get
            {
                return _translationType switch
                       {
                                   UxrTranslationType.Fade   => _fadeTranslationSeconds,
                                   UxrTranslationType.Smooth => _smoothTranslationSeconds,
                                   _                         => 0.0f
                       };
            }
        }

        /// <summary>
        ///     Gets the rotation transition in seconds depending on <see cref="RotationType" />.
        /// </summary>
        private float RotationSeconds
        {
            get
            {
                return _rotationType switch
                       {
                                   UxrRotationType.Fade   => _fadeRotationSeconds,
                                   UxrRotationType.Smooth => _smoothRotationSeconds,
                                   _                      => 0.0f
                       };
            }
        }

        private const    float           RaycastAboveGround                   = 0.05f;
        private const    float           RaycastLongDistance                  = 1000.0f;
        private const    float           HeadRadius                           = 0.2f;
        private const    float           DeltaTimeMultiplierFilterMin         = 25.0f;
        private const    float           DeltaTimeMultiplierFilterMax         = 5.0f;
        private const    float           MaxVerticalHeightDisparity           = 0.2f;
        private const    float           DestinationValidationSubdivisions    = 8;
        private const    float           DestinationValidationPositivesNeeded = 5;
        private readonly List<Validator> _destinationValidators               = new List<Validator>();

        private List<UxrTeleportLocomotionBase> _otherAvatarTeleports;

        private bool                     _isBackStepAvailable;
        private bool                     _isValidTeleport;
        private Vector3                  _teleportLocalPosition;
        private Vector3                  _teleportLocalDirection;
        private bool                     _teleportTargetEnabled;
        private bool                     _teleportTargetValid;
        private LayerMask                _layerMaskRaycast = 0;
        private UxrTeleportTarget        _teleportTarget;
        private UxrTeleportSpawnCollider _lastSpawnCollider;
        private RaycastHit               _hitInfo;

        #endregion
    }
}