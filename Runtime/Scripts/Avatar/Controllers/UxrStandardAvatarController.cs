// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStandardAvatarController.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.IK;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UltimateXR.UI;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     <see cref="UxrAvatarController" /> default implementation provided by UltimateXR to update user-controlled avatars.
    ///     It is in charge of updating the avatar in the correct order, granting access to all features in the framework.
    ///     By adding it to a GameObject with an <see cref="UxrAvatar" /> component, it will automatically update the avatar
    ///     each frame.
    /// </summary>
    [RequireComponent(typeof(UxrAvatar))]
    public sealed partial class UxrStandardAvatarController : UxrAvatarController
    {
        #region Inspector Properties/Serialized Fields

        // add a small margin -in Unity units- to be added to the box when checking if a finger tip gets out of it.

        [SerializeField] private bool                           _useArmIK             = true;
        [SerializeField] private float                          _armIKElbowAperture   = UxrArmIKSolver.DefaultElbowAperture;
        [SerializeField] private UxrArmOverExtendMode           _armIKOverExtendMode  = UxrArmOverExtendMode.LimitHandReach;
        [SerializeField] private bool                           _useBodyIK            = true;
        [SerializeField] private UxrBodyIKSettings              _bodyIKSettings       = new UxrBodyIKSettings();
        [SerializeField] private bool                           _useLegIK             = true;
        [SerializeField] private List<UxrAvatarControllerEvent> _listControllerEvents = new List<UxrAvatarControllerEvent>();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the list of controller events.
        /// </summary>
        public IReadOnlyList<UxrAvatarControllerEvent> ControllerEvents => _listControllerEvents;

        /// <summary>
        ///     Gets the list of controller events that belong to the left hand.
        /// </summary>
        public IEnumerable<UxrAvatarControllerEvent> LeftControllerEvents
        {
            get
            {
                foreach (UxrAvatarControllerEvent controllerEvent in _listControllerEvents)
                {
                    if (IsLeftSideAnimation(controllerEvent.TypeOfAnimation))
                    {
                        yield return controllerEvent;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the list of controller events that belong to the right hand.
        /// </summary>
        public IEnumerable<UxrAvatarControllerEvent> RightControllerEvents
        {
            get
            {
                foreach (UxrAvatarControllerEvent controllerEvent in _listControllerEvents)
                {
                    if (!IsLeftSideAnimation(controllerEvent.TypeOfAnimation))
                    {
                        yield return controllerEvent;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the pose name that is used for the default left hand animation. That is, the animation that is played
        ///     when no other animation is played.
        /// </summary>
        public string LeftHandDefaultPoseName => Avatar.DefaultHandPoseName;

        /// <summary>
        ///     Gets the pose name that is used for the default right hand animation. That is, the animation that is played
        ///     when no other animation is played.
        /// </summary>
        public string RightHandDefaultPoseName => Avatar.DefaultHandPoseName;

        /// <summary>
        ///     Gets the pose name that is used for the left hand grab animation. That is, the animation that is played for
        ///     the event that has the <see cref="UxrAnimationType.LeftHandGrab" /> animation.
        /// </summary>
        public string LeftHandGrabPoseName => _leftHandInfo.InitialHandGrabPoseName;

        /// <summary>
        ///     Gets the pose name that is used for the right hand grab animation. That is, the animation that is played
        ///     for the event that has the <see cref="UxrAnimationType.RightHandGrab" /> animation.
        /// </summary>
        public string RightHandGrabPoseName => _rightHandInfo.InitialHandGrabPoseName;

        /// <summary>
        ///     Gets the button(s) that is/are required to play the left hand grab animation. That is, the animation that is played
        ///     for the event that has the <see cref="UxrAnimationType.LeftHandGrab" /> animation.
        /// </summary>
        public UxrInputButtons LeftHandGrabButtons => _leftHandInfo.InitialHandGrabButtons;

        /// <summary>
        ///     Gets the button(s) that is/are used to play the right hand grab animation. That is, the animation that is played
        ///     for the event that has the <see cref="UxrAnimationType.RightHandGrab" /> animation.
        /// </summary>
        public UxrInputButtons RightHandGrabButtons => _rightHandInfo.InitialHandGrabButtons;

        /// <summary>
        ///     Gets whether the left hand is inside an <see cref="UxrFingerPointingVolume" />.
        /// </summary>
        public bool IsLeftHandInsideFingerPointingVolume => _leftHandInfo.IsInsideFingerPointingVolume;

        /// <summary>
        ///     Gets whether the right hand is inside an <see cref="UxrFingerPointingVolume" />.
        /// </summary>
        public bool IsRightHandInsideFingerPointingVolume => _rightHandInfo.IsInsideFingerPointingVolume;

        /// <summary>
        ///     Gets or sets whether the avatar controller is using IK for the arms.
        /// </summary>
        public bool UseArmIK
        {
            get => _useArmIK;
            set
            {
                _useArmIK = value;

                if (_leftArmIK)
                {
                    _leftArmIK.enabled = value;
                }

                if (_rightArmIK)
                {
                    _rightArmIK.enabled = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the arm IK relaxed elbow aperture. The value between range [0.0, 1.0] that determines how tight
        ///     the elbows are to the body.
        /// </summary>
        public float ArmIKElbowAperture
        {
            get => _armIKElbowAperture;
            set
            {
                _armIKElbowAperture = value;

                if (_leftArmIK)
                {
                    _leftArmIK.RelaxedElbowAperture = value;
                }

                if (_rightArmIK)
                {
                    _rightArmIK.RelaxedElbowAperture = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether body IK are used.
        /// </summary>
        public bool UseBodyIK
        {
            get => _useBodyIK;
            set => _useBodyIK = value;
        }

        /// <summary>
        ///     Gets or sets the pose name that overrides <see cref="LeftHandDefaultPoseName" /> if it is other than
        ///     null. If it is set to null, <see cref="LeftHandDefaultPoseName" /> is back to being used.
        /// </summary>
        public string LeftHandDefaultPoseNameOverride { get; set; } = null;

        /// <summary>
        ///     Gets or sets the pose name that overrides <see cref="RightHandDefaultPoseName" /> if it is other
        ///     than null. If it is set to null, <see cref="RightHandDefaultPoseName" /> is back to being used.
        /// </summary>
        public string RightHandDefaultPoseNameOverride { get; set; } = null;

        /// <summary>
        ///     Gets or sets the pose name that overrides <see cref="LeftHandGrabPoseName" /> if it is other than
        ///     null. If it is set to null, <see cref="LeftHandGrabPoseName" /> is back to being used.
        /// </summary>
        public string LeftHandGrabPoseNameOverride
        {
            get => GetEventPoseNameOverride(_leftHandInfo.GrabEventIndex, LeftHandGrabPoseName);
            set => OverrideEventPoseName(_leftHandInfo.GrabEventIndex, value, LeftHandGrabPoseName);
        }

        /// <summary>
        ///     Gets or sets the pose name that overrides <see cref="RightHandGrabPoseName" /> if it is other than
        ///     null. If it is set to null, <see cref="RightHandGrabPoseName" /> is back to being used.
        /// </summary>
        public string RightHandGrabPoseNameOverride
        {
            get => GetEventPoseNameOverride(_rightHandInfo.GrabEventIndex, RightHandGrabPoseName);
            set => OverrideEventPoseName(_rightHandInfo.GrabEventIndex, value, RightHandGrabPoseName);
        }

        /// <summary>
        ///     Gets or sets the button(s) that override <see cref="LeftHandGrabButtons" /> if it is other than
        ///     <see cref="UxrInputButtons.Everything" />. If it is set to <see cref="UxrInputButtons.Everything" />,
        ///     <see cref="LeftHandGrabButtons" /> is back to being used.
        /// </summary>
        public UxrInputButtons LeftHandGrabButtonsOverride
        {
            get => GetEventButtonsOverride(_leftHandInfo.GrabEventIndex, LeftHandGrabButtons);
            set => OverrideEventButtons(_leftHandInfo.GrabEventIndex, value, LeftHandGrabButtons);
        }

        /// <summary>
        ///     Gets or sets the button(s) that override <see cref="RightHandGrabButtons" /> if it is other than
        ///     <see cref="UxrInputButtons.Everything" />. If it is set to <see cref="UxrInputButtons.Everything" />,
        ///     <see cref="RightHandGrabButtons" /> is back to being used.
        /// </summary>
        public UxrInputButtons RightHandGrabButtonsOverride
        {
            get => GetEventButtonsOverride(_rightHandInfo.GrabEventIndex, RightHandGrabButtons);
            set => OverrideEventButtons(_rightHandInfo.GrabEventIndex, value, RightHandGrabButtons);
        }

        /// <summary>
        ///     Gets or sets whether to also process ignored input for events. Input can be ignored by using
        ///     <see cref="UxrControllerInput.SetIgnoreControllerInput" /> and <see cref="ProcessIgnoredInput" /> tells whether to
        ///     actually ignore it or not. By default the ignored input is also processed, which may seem counterintuitive but most
        ///     of the time the input is ignored when specific objects are being grabbed. Since the avatar controller is
        ///     responsible for triggering the grab events depending on user input, it was chosen that the default behaviour would
        ///     be to also process ignored input.
        /// </summary>
        public bool ProcessIgnoredInput { get; set; } = true;

        #endregion

        #region Public Overrides UxrAvatarController

        /// <inheritdoc />
        public override bool CanHandInteractWithUI(UxrHandSide handSide)
        {
            if (Avatar.ControllerInput.GetControllerCapabilities(handSide).HasFlag(UxrControllerInputCapabilities.TrackedHandPose))
            {
                // With tracked hand pose controllers (for example Valve Index) we cannot use the IsGrabbing property. The pointing
                // gesture reports grabbing due to the middle finger pressure. We will rely on the index finger direction with
                // respect to the wrist instead, to check if it is curled.

                Transform     forearm      = Avatar.GetArm(handSide).Forearm;
                UxrAvatarHand hand         = Avatar.GetHand(handSide);
                Transform     wrist        = hand.Wrist;
                Transform     intermediate = hand.GetFinger(UxrFingerType.Index).Intermediate;
                Transform     distal       = hand.GetFinger(UxrFingerType.Index).Distal;

                if (forearm != null && wrist != null && intermediate != null && distal != null)
                {
                    Vector3 wristDir  = wrist.position - forearm.position;
                    Vector3 fingerDir = distal.position - intermediate.position;

                    return Vector3.Angle(wristDir, fingerDir) < 90.0f;
                }
            }

            return handSide == UxrHandSide.Left ? !_leftHandInfo.IsGrabbing : !_rightHandInfo.IsGrabbing;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Solves the body IK using the current headset and controller positions.
        /// </summary>
        public void SolveBodyIK()
        {
            if (_bodyIK != null && _useBodyIK)
            {
                _bodyIK.PreSolveAvatarIK();
            }

            // Update arms without clavicles to check how much tension is applied on the shoulders

            IEnumerable<UxrArmIKSolver> autoUpdateArmSolvers = UxrIKSolver.GetComponents(Avatar).OfType<UxrArmIKSolver>().Where(s => s.NeedsAutoUpdate);

            autoUpdateArmSolvers.ForEach(s => s.SolveIKPass(UxrArmSolveOptions.None, UxrArmOverExtendMode.ExtendForearm));

            // Update torso rotation

            if (_bodyIK != null && _useBodyIK)
            {
                _bodyIK.PostSolveAvatarIK();
            }

            // Update arms normally

            autoUpdateArmSolvers.ForEach(s => s.SolveIK());

            // Update non-arm IKs

            UxrIKSolver.GetComponents(Avatar).Where(s => s.GetType() != typeof(UxrArmIKSolver) && s.NeedsAutoUpdate).ForEach(s => s.SolveIK());
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _leftHandInfo.GrabEventIndex  = -1;
            _rightHandInfo.GrabEventIndex = -1;

            _leftHandInfo.InitialHandGrabPoseName  = string.Empty;
            _rightHandInfo.InitialHandGrabPoseName = string.Empty;

            _leftHandInfo.InitialHandGrabButtons  = UxrInputButtons.Everything;
            _rightHandInfo.InitialHandGrabButtons = UxrInputButtons.Everything;

            if (Avatar != null)
            {
                if (TryFindTypeOfAnimationEventIndex(_listControllerEvents, UxrAnimationType.LeftHandGrab, out int leftGrabEventIndex))
                {
                    _leftHandInfo.GrabEventIndex = leftGrabEventIndex;
                }

                if (TryFindTypeOfAnimationEventIndex(_listControllerEvents, UxrAnimationType.RightHandGrab, out int rightGrabEventIndex))
                {
                    _rightHandInfo.GrabEventIndex = rightGrabEventIndex;
                }

                _leftHandInfo.InitialHandGrabPoseName  = _leftHandInfo.GrabEventIndex != -1 ? _listControllerEvents[_leftHandInfo.GrabEventIndex].PoseName : string.Empty;
                _rightHandInfo.InitialHandGrabPoseName = _rightHandInfo.GrabEventIndex != -1 ? _listControllerEvents[_rightHandInfo.GrabEventIndex].PoseName : string.Empty;

                _leftHandInfo.InitialHandGrabButtons  = _leftHandInfo.GrabEventIndex != -1 ? _listControllerEvents[_leftHandInfo.GrabEventIndex].Buttons : UxrInputButtons.Everything;
                _rightHandInfo.InitialHandGrabButtons = _rightHandInfo.GrabEventIndex != -1 ? _listControllerEvents[_rightHandInfo.GrabEventIndex].Buttons : UxrInputButtons.Everything;

                if (_useArmIK && Avatar.AvatarRigType == UxrAvatarRigType.HalfOrFullBody)
                {
                    _leftArmIK  = SetupArmIK(Avatar.AvatarRig.LeftArm,  UxrHandSide.Left);
                    _rightArmIK = SetupArmIK(Avatar.AvatarRig.RightArm, UxrHandSide.Right);
                }
            }

            _leftHandInfo.LetGrabAgain  = true;
            _rightHandInfo.LetGrabAgain = true;
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarMoved                 += UxrManager_AvatarMoved;
            UxrGrabManager.Instance.ObjectGrabbed  += UxrGrabManager_ObjectGrabbed;
            UxrGrabManager.Instance.ObjectPlaced   += UxrGrabManager_ObjectPlacedOrReleased;
            UxrGrabManager.Instance.ObjectReleased += UxrGrabManager_ObjectPlacedOrReleased;
        }

        /// <summary>
        ///     Subscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarMoved                 -= UxrManager_AvatarMoved;
            UxrGrabManager.Instance.ObjectGrabbed  -= UxrGrabManager_ObjectGrabbed;
            UxrGrabManager.Instance.ObjectPlaced   -= UxrGrabManager_ObjectPlacedOrReleased;
            UxrGrabManager.Instance.ObjectReleased -= UxrGrabManager_ObjectPlacedOrReleased;
        }

        /// <summary>
        ///     Initializes the body IK component if required.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (Avatar != null && Avatar.AvatarRigType == UxrAvatarRigType.HalfOrFullBody && _useBodyIK)
            {
                _bodyIK = new UxrBodyIK();
                _bodyIK.Initialize(Avatar, _bodyIKSettings, _useArmIK, _useLegIK);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever the avatar has moved. Use it to notify the body IK component.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UxrManager_AvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            if (_bodyIK != null && e.Avatar == Avatar)
            {
                _bodyIK.NotifyAvatarMoved(e);
            }
        }

        /// <summary>
        ///     Event handling method called when an object was grabbed.
        ///     It's used to check if the avatar hand grab pose needs to be changed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrGrabManager_ObjectGrabbed(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && Avatar == e.Grabber.Avatar)
            {
                UpdateGrabPoseInfo(e.Grabber, e.GrabbableObject);
            }
        }

        /// <summary>
        ///     Event handling method called when an object was released.
        ///     It's used to check if the avatar hand grab pose needs to be changed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrGrabManager_ObjectPlacedOrReleased(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && Avatar == e.Grabber.Avatar)
            {
                ClearGrabButtonsOverride(e.Grabber.Side);
                GetHandInfo(e.Grabber.Side).GrabBlendValue = -1.0f;

                if (e.Grabber.Side == UxrHandSide.Left)
                {
                    LeftHandGrabPoseNameOverride = null;
                }

                if (e.Grabber.Side == UxrHandSide.Right)
                {
                    RightHandGrabPoseNameOverride = null;
                }
            }
        }

        #endregion

        #region Protected Overrides UxrAvatarController

        /// <inheritdoc />
        protected override void UpdateAvatar()
        {
            base.UpdateAvatar();

            // Update controller inputs first, then locomotion (which need inputs and may affect the avatar position) and then the tracking (which may use the avatar position) associated to this avatar

            UpdateInputDevice();
            UpdateLocomotion();
            UpdateTrackingDevices();

            // Initialize some internal vars

            CheckFingerTipInsideFingerPointingVolume(Avatar.FingerTips, out bool hasLeftFingerTipInside, out bool hasRightFingerTipInside, _leftHandInfo.IsInsideFingerPointingVolume, _rightHandInfo.IsInsideFingerPointingVolume);

            _leftHandInfo.IsInsideFingerPointingVolume  = hasLeftFingerTipInside;
            _rightHandInfo.IsInsideFingerPointingVolume = hasRightFingerTipInside;

            _leftHandInfo.WasGrabbingLastFrame  = _leftHandInfo.IsGrabbing;
            _rightHandInfo.WasGrabbingLastFrame = _rightHandInfo.IsGrabbing;
            _leftHandInfo.WasPointingLastFrame  = _leftHandInfo.IsPointing;
            _rightHandInfo.WasPointingLastFrame = _rightHandInfo.IsPointing;

            _leftHandInfo.IsGrabbing  = false;
            _rightHandInfo.IsGrabbing = false;
            _leftHandInfo.IsPointing  = false;
            _rightHandInfo.IsPointing = false;

            // Check with the help of the grab manager if we need to override the grab buttons based on proximity to a grabbable object that used non-default grab buttons.

            UxrInputButtons GetRequiredGrabButtonsOverride(UxrHandSide handSide)
            {
                if( UxrGrabManager.Instance.GetClosestGrabbableObject(Avatar, handSide, out UxrGrabbableObject grabbableObject, out int grabPoint) &&
                    !grabbableObject.GetGrabPoint(grabPoint).UseDefaultGrabButtons &&
                    (Avatar.ControllerInput.GetButtonsEvent(handSide, grabbableObject.GetGrabPoint(grabPoint).InputButtons, UxrButtonEventType.PressDown, ProcessIgnoredInput) ||
                     Avatar.ControllerInput.GetButtonsEvent(handSide, handSide == UxrHandSide.Left ? LeftHandGrabButtons : RightHandGrabButtons, UxrButtonEventType.PressDown, ProcessIgnoredInput)))
                {
                    return grabbableObject.GetGrabPoint(grabPoint).InputButtons;
                }

                return UxrInputButtons.Everything;
            }

            if (!UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Left))
            {
                LeftHandGrabButtonsOverride = GetRequiredGrabButtonsOverride(UxrHandSide.Left);
            }

            if (!UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Right))
            {
                RightHandGrabButtonsOverride = GetRequiredGrabButtonsOverride(UxrHandSide.Right);
            }

            // Update only internal vars (are we grabbing and/or pointing?) but don't execute the actions. We might change things like the grab pose name at runtime
            // after the UxrGrabManager grab event and then it's when we want to execute the var actions.
            // Also, we want to do it in a priority order where grab has priority over point and point has priority over the rest of the animations. The rest of
            // the animations have top to bottom priority.

            ProcessControllerEvents(_listControllerEvents, ControllerEventTypes.Grab,  EventProcessing.InternalVars);
            ProcessControllerEvents(_listControllerEvents, ControllerEventTypes.Point, EventProcessing.InternalVars);
            ProcessControllerEvents(_listControllerEvents, ControllerEventTypes.Other, EventProcessing.InternalVars);
        }

        /// <inheritdoc />
        protected override void UpdateAvatarAnimation()
        {
            base.UpdateAvatarAnimation();

            // Execute the event actions that potentially change the current poses

            ProcessControllerEvents(_listControllerEvents, ControllerEventTypes.All, EventProcessing.ExecuteActions);

            // Update animation
            // We update the avatar animation in a different method so that it can be called at a different stage.
            // When the avatar has Animator components that update the bone transforms, pose animations and IK components need to be processed after.

            Avatar.UpdateHandPoseTransforms();
        }

        /// <inheritdoc />
        protected override void UpdateAvatarManipulation()
        {
            ProcessHandManipulation(_leftHandInfo,  UxrHandSide.Left);
            ProcessHandManipulation(_rightHandInfo, UxrHandSide.Right);
        }

        /// <inheritdoc />
        protected override void UpdateAvatarPostProcess()
        {
            base.UpdateAvatarPostProcess();

            // Needs to be done after managers since the grab manager may force hands to be in certain positions

            SolveBodyIK();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the pose name that overrides the event with the given index.
        /// </summary>
        /// <param name="eventIndex">The event index to get the override from</param>
        /// <param name="defaultPoseName">The initial pose name, required to check if the pose name has been overriden or not</param>
        /// <returns>
        ///     Pose name that overrides the given event, or null if the pose name is currently the one from the beginning.
        /// </returns>
        private string GetEventPoseNameOverride(int eventIndex, string defaultPoseName)
        {
            if (Avatar == null || eventIndex == -1)
            {
                return string.Empty;
            }

            UxrAvatarControllerEvent controllerEvent = _listControllerEvents[eventIndex];
            return controllerEvent.PoseName == defaultPoseName ? null : controllerEvent.PoseName;
        }

        /// <summary>
        ///     Sets the pose name that overrides the event with the given index.
        /// </summary>
        /// <param name="eventIndex">The event index to override</param>
        /// <param name="poseName">The pose name that will be used to override</param>
        /// <param name="defaultPoseName">The initial pose name, required to check if the pose name is being overriden or not</param>
        private void OverrideEventPoseName(int eventIndex, string poseName, string defaultPoseName)
        {
            if (Avatar == null || eventIndex == -1)
            {
                return;
            }

            // Change the pose name if necessary

            UxrAvatarControllerEvent controllerEvent = _listControllerEvents[eventIndex];

            if (string.IsNullOrEmpty(poseName) || poseName == defaultPoseName)
            {
                controllerEvent.PoseNameOverride = null;
            }
            else
            {
                controllerEvent.PoseNameOverride = poseName;
            }
        }

        /// <summary>
        ///     Gets the button(s) that override the event with the given index.
        /// </summary>
        /// <param name="eventIndex">The event index to get the override from</param>
        /// <param name="defaultButtons">The initial buttons value, required to check if they has been overriden or not</param>
        /// <returns>
        ///     Button(s) that override the given event, or <see cref="UxrInputButtons.Everything" /> if the button(s)
        ///     currently in use is/are the default.
        /// </returns>
        private UxrInputButtons GetEventButtonsOverride(int eventIndex, UxrInputButtons defaultButtons)
        {
            if (Avatar == null || eventIndex == -1)
            {
                return UxrInputButtons.Everything;
            }

            UxrAvatarControllerEvent controllerEvent = _listControllerEvents[eventIndex];
            return controllerEvent.Buttons == defaultButtons ? UxrInputButtons.Everything : controllerEvent.Buttons;
        }

        /// <summary>
        ///     Sets the button(s) that overrides the event with the given index.
        /// </summary>
        /// <param name="eventIndex">The event index to override</param>
        /// <param name="buttons">Button(s) value to override</param>
        /// <param name="defaultButtons">The initial buttons value, required to check if the value is being overriden or not.</param>
        private void OverrideEventButtons(int eventIndex, UxrInputButtons buttons, UxrInputButtons defaultButtons)
        {
            if (Avatar == null || eventIndex == -1)
            {
                return;
            }

            UxrAvatarControllerEvent controllerEvent = _listControllerEvents[eventIndex];
            controllerEvent.Buttons = buttons == UxrInputButtons.Everything ? defaultButtons : buttons;
        }

        /// <summary>
        ///     Processes a list of controller events.
        /// </summary>
        /// <param name="controllerEvents">The list of events to process</param>
        /// <param name="eventTypes">The type of events to process</param>
        /// <param name="eventProcessing">The way the events are going to be processed</param>
        private void ProcessControllerEvents(List<UxrAvatarControllerEvent> controllerEvents, ControllerEventTypes eventTypes, EventProcessing eventProcessing)
        {
            bool eventProcessedLeft  = false;
            bool eventProcessedRight = false;

            foreach (UxrAvatarControllerEvent e in controllerEvents)
            {
                // Filter events

                if (IsGrabAnimation(e.TypeOfAnimation) && !eventTypes.HasFlag(ControllerEventTypes.Grab))
                {
                    continue;
                }

                if (IsPointAnimation(e.TypeOfAnimation) && !eventTypes.HasFlag(ControllerEventTypes.Point))
                {
                    continue;
                }

                if (IsOtherAnimation(e.TypeOfAnimation) && !eventTypes.HasFlag(eventTypes & ControllerEventTypes.Other))
                {
                    continue;
                }

                // Get hand side

                bool        isLeftSide = IsLeftSideAnimation(e.TypeOfAnimation);
                UxrHandSide handSide   = isLeftSide ? UxrHandSide.Left : UxrHandSide.Right;
                HandInfo    handInfo   = GetHandInfo(handSide);

                // Prepare additional grab vars that we may need

                bool eventProcessed = isLeftSide ? eventProcessedLeft : eventProcessedRight;

                // Input is true?

                UxrInputButtons inputButtons = e.Buttons;

                if (IsGrabAnimation(e.TypeOfAnimation) && GetGrabButtonsOverride(handSide) != UxrInputButtons.Everything)
                {
                    inputButtons = GetGrabButtonsOverride(handSide);
                }

                if (Avatar.ControllerInput.GetButtonsEvent(handSide, inputButtons, UxrButtonEventType.Pressing, ProcessIgnoredInput) && !eventProcessed)
                {
                    // First check the special case of finger pointing and grabbing. Inside FingerPointVolumes you can't try to grab (to force the finger to be always pointing for UI elements f.e),
                    // unless there is something that can be grabbed.

                    bool allowAnimation = true;

                    switch (e.TypeOfAnimation)
                    {
                        case UxrAnimationType.LeftHandGrab:
                        case UxrAnimationType.RightHandGrab:

                            if (eventProcessing.HasFlag(EventProcessing.InternalVars))
                            {
                                handInfo.IsGrabbing = !handInfo.IsInsideFingerPointingVolume ||
                                                      (handInfo.IsInsideFingerPointingVolume && (UxrGrabManager.Instance.IsHandGrabbing(Avatar, handSide) || UxrGrabManager.Instance.CanGrabSomething(Avatar, handSide)));

                                handInfo.LetGrabAgain = Avatar.ControllerInput.GetButtonsEvent(handSide, inputButtons, UxrButtonEventType.PressDown, ProcessIgnoredInput);
                            }

                            allowAnimation = handInfo.IsGrabbing;
                            break;

                        case UxrAnimationType.LeftFingerPoint:
                        case UxrAnimationType.RightFingerPoint:

                            if (eventProcessing.HasFlag(EventProcessing.InternalVars))
                            {
                                handInfo.IsPointing = !handInfo.IsGrabbing && !UxrGrabManager.Instance.IsHandGrabbing(Avatar, handSide);
                            }

                            allowAnimation = handInfo.IsPointing;
                            break;

                        case UxrAnimationType.LeftHandOther:
                        case UxrAnimationType.RightHandOther:

                            allowAnimation = !handInfo.IsPointing && !handInfo.IsGrabbing && !UxrGrabManager.Instance.IsHandGrabbing(Avatar, handSide);
                            break;
                    }

                    // Execute the action

                    if (eventProcessing.HasFlag(EventProcessing.ExecuteActions) && allowAnimation)
                    {
                        ExecuteEventAction(handSide, e);
                    }

                    // Ignore all other events from the side due to priority

                    if (isLeftSide && !eventProcessedLeft)
                    {
                        eventProcessedLeft = allowAnimation;
                    }

                    if (!isLeftSide && !eventProcessedRight)
                    {
                        eventProcessedRight = allowAnimation;
                    }
                }
                else
                {
                    bool forceAnimation = false;

                    switch (e.TypeOfAnimation)
                    {
                        case UxrAnimationType.LeftFingerPoint:
                        case UxrAnimationType.RightFingerPoint:

                            if (eventProcessing.HasFlag(EventProcessing.InternalVars))
                            {
                                if (handInfo.IsInsideFingerPointingVolume)
                                {
                                    if (UxrGrabManager.Instance.IsHandGrabbing(Avatar, handSide) || (UxrGrabManager.Instance.CanGrabSomething(Avatar, handSide) && handInfo.IsGrabbing && handInfo.LetGrabAgain))
                                    {
                                        handInfo.IsPointing = false;
                                    }
                                    else
                                    {
                                        handInfo.IsPointing = true;
                                        handInfo.IsGrabbing = false;
                                    }
                                }
                            }

                            forceAnimation = handInfo.IsPointing;
                            break;
                    }

                    // Execute the action

                    if (eventProcessing.HasFlag(EventProcessing.ExecuteActions) && forceAnimation)
                    {
                        ExecuteEventAction(handSide, e);
                    }

                    // Ignore all other events from the side due to priority

                    if (isLeftSide && !eventProcessedLeft)
                    {
                        eventProcessedLeft = forceAnimation;
                    }

                    if (!isLeftSide && !eventProcessedRight)
                    {
                        eventProcessedRight = forceAnimation;
                    }
                }
            }

            // If no events were found, set the default or grab pose depending on the current state

            if (eventProcessing.HasFlag(EventProcessing.ExecuteActions))
            {
                if (!eventProcessedLeft)
                {
                    if (UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Left))
                    {
                        Avatar.SetCurrentHandPose(UxrHandSide.Left, LeftHandGrabPoseNameOverride ?? LeftHandGrabPoseName, _leftHandInfo.GrabBlendValue);
                    }
                    else
                    {
                        Avatar.SetCurrentHandPose(UxrHandSide.Left, LeftHandDefaultPoseNameOverride ?? Avatar.DefaultHandPoseName);
                    }
                }

                if (!eventProcessedRight)
                {
                    if (UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Right))
                    {
                        Avatar.SetCurrentHandPose(UxrHandSide.Right, RightHandGrabPoseNameOverride ?? RightHandGrabPoseName, _rightHandInfo.GrabBlendValue);
                    }
                    else
                    {
                        Avatar.SetCurrentHandPose(UxrHandSide.Right, RightHandDefaultPoseNameOverride ?? Avatar.DefaultHandPoseName);
                    }
                }
            }
        }

        /// <summary>
        ///     Executes an event action.
        /// </summary>
        /// <param name="handSide">Which hand to process</param>
        /// <param name="controllerEvent">The event to process</param>
        private void ExecuteEventAction(UxrHandSide handSide, UxrAvatarControllerEvent controllerEvent)
        {
            HandInfo handInfo = GetHandInfo(handSide);
            Avatar.SetCurrentHandPose(handSide, controllerEvent.PoseName, IsGrabAnimation(controllerEvent.TypeOfAnimation) && handInfo.GrabBlendValue >= 0.0f ? handInfo.GrabBlendValue : controllerEvent.PoseBlendValue);
        }

        /// <summary>
        ///     Processes an avatar hand, updating the internal state.
        /// </summary>
        /// <param name="handInfo">Hand information</param>
        /// <param name="handSide">Which hand is being processed</param>
        private void ProcessHandManipulation(HandInfo handInfo, UxrHandSide handSide)
        {
            UxrGrabber grabber = Avatar.GetGrabber(handSide);

            if (!handInfo.WasGrabbingLastFrame && handInfo.IsGrabbing && handInfo.LetGrabAgain)
            {
                // Pressed grab action.
                // We get the grabber instead of the return value of TryGrab() because we may already be grabbing an object with a special UxrGrabMode.

                UxrGrabManager.Instance.TryGrab(Avatar, handSide);
            }
            else if (handInfo.WasGrabbingLastFrame && !handInfo.IsGrabbing)
            {
                // Released grab action.
                UxrGrabManager.Instance.TryRelease(Avatar, handSide);
            }
            else if (UxrGrabManager.Instance.IsHandGrabbing(Avatar, handSide) && grabber.GrabbedObject)
            {
                // Update pose info to refresh potential blend value mainly
                UpdateGrabPoseInfo(grabber, grabber.GrabbedObject);
            }
        }

        /// <summary>
        ///     Updates the internal grab information for the given grabber and grabbed object.
        /// </summary>
        /// <param name="grabber">Grabber</param>
        /// <param name="grabbableObject">Grabbed object</param>
        private void UpdateGrabPoseInfo(UxrGrabber grabber, UxrGrabbableObject grabbableObject)
        {
            // Change the avatar's grab pose

            string             overrideGrabPoseName = UxrGrabManager.Instance.GetOverrideGrabPoseName(grabber, grabbableObject);
            UxrRuntimeHandPose overrideGrabPose     = !string.IsNullOrEmpty(overrideGrabPoseName) ? Avatar.GetRuntimeHandPose(overrideGrabPoseName) : null;

            if (grabber.Side == UxrHandSide.Left)
            {
                LeftHandGrabPoseNameOverride = overrideGrabPose != null ? overrideGrabPoseName : null;
            }
            else
            {
                RightHandGrabPoseNameOverride = overrideGrabPose != null ? overrideGrabPoseName : null;
            }

            // Is it a blend pose?

            if (overrideGrabPose != null && overrideGrabPose.PoseType == UxrHandPoseType.Blend)
            {
                if (grabber.Side == UxrHandSide.Left)
                {
                    _leftHandInfo.GrabBlendValue = UxrGrabManager.Instance.GetOverrideGrabPoseBlendValue(grabber, grabbableObject);
                }
                else
                {
                    _rightHandInfo.GrabBlendValue = UxrGrabManager.Instance.GetOverrideGrabPoseBlendValue(grabber, grabbableObject);
                }
            }
            else
            {
                if (grabber.Side == UxrHandSide.Left)
                {
                    if (overrideGrabPose == null)
                    {
                        LeftHandGrabPoseNameOverride = null;
                    }

                    _leftHandInfo.GrabBlendValue = -1.0f;
                }
                else
                {
                    if (overrideGrabPose == null)
                    {
                        RightHandGrabPoseNameOverride = null;
                    }

                    _rightHandInfo.GrabBlendValue = -1.0f;
                }
            }
        }

        /// <summary>
        ///     Checks if finger tips from a list are inside a <see cref="UxrFingerPointingVolume" />, which are components that
        ///     define a volume inside of which a hand will adopt a pointing pose using the index finger.
        /// </summary>
        /// <param name="fingerTips">List of finger tips to check</param>
        /// <param name="hasLeftFingerTipInside">Returns whether the left hand has any finger tip inside the volume</param>
        /// <param name="hasRightFingerTipInside">Returns whether the right hand has any finger tip inside the volume</param>
        /// <param name="lastLeftInsideFingerPointingVolume">Whether the left hand had any finger tip inside last frame</param>
        /// <param name="lastRightInsideFingerPointingVolume">Whether the right hand had any finger tip inside last frame</param>
        private void CheckFingerTipInsideFingerPointingVolume(IEnumerable<UxrFingerTip> fingerTips,
                                                              out bool                  hasLeftFingerTipInside,
                                                              out bool                  hasRightFingerTipInside,
                                                              bool                      lastLeftInsideFingerPointingVolume,
                                                              bool                      lastRightInsideFingerPointingVolume)
        {
            hasLeftFingerTipInside  = false;
            hasRightFingerTipInside = false;

            if (fingerTips != null)
            {
                foreach (UxrFingerTip fingerTip in fingerTips)
                {
                    if (fingerTip.Side == UxrHandSide.Left)
                    {
                        if (IsInsideFingerPointingVolume(fingerTip, lastLeftInsideFingerPointingVolume))
                        {
                            hasLeftFingerTipInside = true;
                        }
                    }
                    else if (fingerTip.Side == UxrHandSide.Right)
                    {
                        if (IsInsideFingerPointingVolume(fingerTip, lastRightInsideFingerPointingVolume))
                        {
                            hasRightFingerTipInside = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a finger tip is inside a given <see cref="UxrFingerPointingVolume" />.
        /// </summary>
        /// <param name="fingerTip">Finger tip to check</param>
        /// <param name="lastHandWasInsideFingerPointingVolume">Whether the hand had a finger tip inside last frame</param>
        /// <returns>Whether the finger tip is inside a given <see cref="UxrFingerPointingVolume" />.</returns>
        private bool IsInsideFingerPointingVolume(UxrFingerTip fingerTip, bool lastHandWasInsideFingerPointingVolume)
        {
            foreach (UxrFingerPointingVolume volume in UxrFingerPointingVolume.AllComponents)
            {
                if (volume != null && volume.isActiveAndEnabled && volume.IsCompatible(fingerTip.Side) &&
                    volume.IsPointInside(fingerTip.transform.position, lastHandWasInsideFingerPointingVolume ? FlexibleFingerVolumeMargin : 0.0f))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Try to find the index of a certain event animation type.
        /// </summary>
        /// <param name="listControllerEvents">The list of events where to look</param>
        /// <param name="animationType">The animation type to look for</param>
        /// <param name="indexOut">If found, returns the index where it is. Otherwise -1.</param>
        /// <returns>Whether the given animation type was found</returns>
        private bool TryFindTypeOfAnimationEventIndex(List<UxrAvatarControllerEvent> listControllerEvents, UxrAnimationType animationType, out int indexOut)
        {
            indexOut = -1;

            for (int i = 0; i < listControllerEvents.Count; ++i)
            {
                if (listControllerEvents[i].TypeOfAnimation == animationType)
                {
                    indexOut = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Sets up Inverse Kinematics for a given arm.
        /// </summary>
        /// <param name="arm">The arm to set Inverse Kinematics for</param>
        /// <param name="side">Whether it is the left arm or right arm</param>
        /// <returns>The Inverse Kinematics solver</returns>
        private UxrArmIKSolver SetupArmIK(UxrAvatarArm arm, UxrHandSide side)
        {
            if (arm.UpperArm && arm.Forearm && arm.Hand.Wrist)
            {
                UxrArmIKSolver armIK = arm.UpperArm.gameObject.GetOrAddComponent<UxrArmIKSolver>();
                armIK.Side                 = side;
                armIK.RelaxedElbowAperture = _armIKElbowAperture;
                armIK.OverExtendMode       = _armIKOverExtendMode;

                return armIK;
            }

            return null;
        }

        /// <summary>
        ///     Returns whether the given animation type is for the left hand. Otherwise it's for the right hand.
        /// </summary>
        /// <param name="animationType">Animation type</param>
        /// <returns>Whether the given animation is for the left hand. If false, it's for the right hand</returns>
        private bool IsLeftSideAnimation(UxrAnimationType animationType)
        {
            return animationType == UxrAnimationType.LeftHandOther || animationType == UxrAnimationType.LeftHandGrab || animationType == UxrAnimationType.LeftFingerPoint;
        }

        /// <summary>
        ///     Returns whether the given animation type is for grabbing.
        /// </summary>
        /// <param name="animationType">Animation type</param>
        /// <returns>Whether the given animation is for grabbing</returns>
        private bool IsGrabAnimation(UxrAnimationType animationType)
        {
            return animationType == UxrAnimationType.LeftHandGrab || animationType == UxrAnimationType.RightHandGrab;
        }

        /// <summary>
        ///     Returns whether the given animation type is for pointing with the index finger.
        /// </summary>
        /// <param name="animationType">Animation type</param>
        /// <returns>Whether the given animation is for pointing with the index finger</returns>
        private bool IsPointAnimation(UxrAnimationType animationType)
        {
            return animationType == UxrAnimationType.LeftFingerPoint || animationType == UxrAnimationType.RightFingerPoint;
        }

        /// <summary>
        ///     Returns whether the given animation type is of other type, not grabbing nor pointing.
        /// </summary>
        /// <param name="animationType">Animation type</param>
        /// <returns>Whether the given animation is of other type, not grabbing nor pointing</returns>
        private bool IsOtherAnimation(UxrAnimationType animationType)
        {
            return animationType == UxrAnimationType.LeftHandOther || animationType == UxrAnimationType.RightHandOther;
        }

        /// <summary>
        ///     Gets the hand information given the side.
        /// </summary>
        /// <param name="handSide">Which side to retrieve</param>
        /// <returns>Hand information</returns>
        private HandInfo GetHandInfo(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? _leftHandInfo : _rightHandInfo;
        }

        /// <summary>
        ///     Returns the current override button that requires to be pressed to execute the grab action. It will return
        ///     <see cref="UxrInputButtons.Everything" /> if no override is active.
        /// </summary>
        /// <param name="handSide">Which side to check</param>
        /// <returns>The override grab button or <see cref="UxrInputButtons.Everything" /> if the default grab button is required</returns>
        private UxrInputButtons GetGrabButtonsOverride(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? LeftHandGrabButtonsOverride : RightHandGrabButtonsOverride;
        }

        /// <summary>
        ///     Clears the current override button that requires to be pressed to execute the grab action.
        /// </summary>
        /// <param name="handSide">Which side to reset</param>
        private void ClearGrabButtonsOverride(UxrHandSide handSide)
        {
            if (handSide == UxrHandSide.Left)
            {
                LeftHandGrabButtonsOverride = UxrInputButtons.Everything;
            }
            else
            {
                RightHandGrabButtonsOverride = UxrInputButtons.Everything;
            }
        }

        #endregion

        #region Private Types & Data

        private const float FlexibleFingerVolumeMargin = 0.1f;

        private readonly HandInfo       _leftHandInfo  = new HandInfo();
        private readonly HandInfo       _rightHandInfo = new HandInfo();
        private          UxrArmIKSolver _leftArmIK;
        private          UxrArmIKSolver _rightArmIK;
        private          UxrBodyIK      _bodyIK;

        #endregion
    }
}