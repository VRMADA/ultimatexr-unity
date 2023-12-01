// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Avatar.Rig;
using UltimateXR.CameraUtils;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.StateSync;
using UltimateXR.Devices;
using UltimateXR.Devices.Integrations;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Locomotion;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UltimateXR.UI;
using UnityEngine;
#if ULTIMATEXR_UNITY_XR_MANAGEMENT
using UnityEngine.SpatialTracking;
#endif

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     <para>
    ///         Main class for avatars in the scene. An avatar is the visual representation of a user in the application.
    ///         The minimal representation requires a single camera that allows the user to look around. More elements
    ///         can be added such as hands to interact with the environment or a half/full body to have a more complete
    ///         representation.
    ///     </para>
    ///     <para>
    ///         A special avatar is the local avatar, which is the avatar controlled by the user using the headset and input
    ///         controllers. Non-local avatars are other avatars instantiated in the scene but not controlled by the user,
    ///         either other users through the network or special cases such as automated replays. A quick way to access the
    ///         local avatar is by using the <see cref="LocalAvatar">UxrAvatar.LocalAvatar</see> static property.
    ///     </para>
    ///     <para>
    ///         Although avatars may contain significant information and components, the avatar is not in charge of updating
    ///         itself.
    ///         Local avatars are updated by the <see cref="UxrAvatarController" /> added to the same GameObject where the
    ///         <see cref="UxrAvatar" /> is. This allows to separate the update logic and functionality from the avatar
    ///         representation.
    ///         The standard avatar controller provided by UltimateXR is the <see cref="UxrStandardAvatarController" />
    ///         component. It provides high-level functionality to interact with the virtual world.
    ///         Different update logic can be created by programming a new <see cref="UxrAvatarController" /> component if
    ///         required.
    ///     </para>
    ///     <para>
    ///         Non-local avatars (Avatars whose <see cref="UxrAvatarMode">UxrAvatar.AvatarMode</see> is
    ///         <see cref="UxrAvatarMode.UpdateExternally" />) can be updated by accessing their rig using
    ///         <see cref="AvatarRig" />.
    ///     </para>
    ///     <para>
    ///         Local avatar update logic is handled automatically by the <see cref="UxrManager" /> singleton without requiring
    ///         any user intervention.
    ///     </para>
    /// </summary>
    [DisallowMultipleComponent]
    public partial class UxrAvatar : UxrComponent<UxrAvatar>, IUxrStateSync
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private string                 _prefabGuid;
        [SerializeField] private GameObject             _parentPrefab;
        [SerializeField] private UxrAvatarMode          _avatarMode          = UxrAvatarMode.Local;
        [SerializeField] private UxrAvatarRenderModes   _renderMode          = UxrAvatarRenderModes.Avatar;
        [SerializeField] private bool                   _showControllerHands = true;
        [SerializeField] private List<Renderer>         _avatarRenderers     = new List<Renderer>();
        [SerializeField] private UxrAvatarRigType       _rigType             = UxrAvatarRigType.HandsOnly;
        [SerializeField] private bool                   _rigExpandedInitialized;
        [SerializeField] private bool                   _rigFoldout       = true;
        [SerializeField] private UxrAvatarRig           _rig              = new UxrAvatarRig();
        [SerializeField] private UxrAvatarRigInfo       _rigInfo          = new UxrAvatarRigInfo();
        [SerializeField] private bool                   _handPosesFoldout = true;
        [SerializeField] private List<UxrHandPoseAsset> _handPoses;
        [SerializeField] private UxrHandPoseAsset       _defaultHandPose;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called right after after the local avatar called its Start(). This is useful when the avatar is instantiated
        ///     in a deferred way, such as a networking environment, and the avatar isn't ready during Awake()/OnEnable()/Start().
        /// </summary>
        public static event EventHandler<UxrAvatarStartedEventArgs> LocalAvatarStarted;

        /// <summary>
        ///     Event called right before the avatar is about to change a hand's pose.
        /// </summary>
        public event EventHandler<UxrAvatarHandPoseChangeEventArgs> HandPoseChanging;

        /// <summary>
        ///     Event called right after the avatar changed a hand's pose.
        /// </summary>
        public event EventHandler<UxrAvatarHandPoseChangeEventArgs> HandPoseChanged;

        /// <summary>
        ///     Event called right before before the avatar goes through an <see cref="UxrUpdateStage" /> of the updating process.
        /// </summary>
        public event EventHandler<UxrAvatarUpdateEventArgs> AvatarUpdating;

        /// <summary>
        ///     Event called right after the avatar goes through an <see cref="UxrUpdateStage" /> of the updating process.
        /// </summary>
        public event EventHandler<UxrAvatarUpdateEventArgs> AvatarUpdated;

        /// <summary>
        ///     Gets the local avatar or null if there is none.
        /// </summary>
        public static UxrAvatar LocalAvatar => AllComponents.FirstOrDefault(c => c.AvatarMode == UxrAvatarMode.Local);

        /// <summary>
        ///     Gets the camera component of the local avatar. If there is no local avatar it will return null.
        /// </summary>
        public static Camera LocalAvatarCamera
        {
            get
            {
                UxrAvatar localAvatar = LocalAvatar;
                return localAvatar != null ? localAvatar.CameraComponent : null;
            }
        }

        /// <summary>
        ///     Gets the controller input of the local avatar. If there is a local avatar present, the controller input is
        ///     guaranteed to be non-null. If there is no input available, the controller input will simply return a dummy object
        ///     that will generate no input events. It will return null if there is no local avatar currently in the scene.
        /// </summary>
        public static UxrControllerInput LocalAvatarInput => LocalAvatar != null ? LocalAvatar.ControllerInput : null;

        /// <summary>
        ///     Gets the standard avatar controller component if it exists.
        /// </summary>
        public static UxrStandardAvatarController LocalStandardAvatarController => LocalAvatar != null ? LocalAvatar.GetCachedComponent<UxrStandardAvatarController>() : null;

        /// <summary>
        ///     Gets the avatar rig.
        /// </summary>
        public UxrAvatarRig AvatarRig => _rig;

        /// <summary>
        ///     Gets the avatar rig information.
        /// </summary>
        public UxrAvatarRigInfo AvatarRigInfo
        {
            get
            {
                if (_rigInfo.SerializedVersion != UxrAvatarRigInfo.CurrentVersion)
                {
                    // We have new serialization data from a newer version, so compute it.
                    CreateRigInfo();
                }

                return _rigInfo;
            }
        }

        /// <summary>
        ///     Gets the first enabled controller input belonging to the avatar. If there is no enabled
        ///     controller input available, to avoid null checks, it returns a dummy component that sends no
        ///     input events.
        /// </summary>
        public UxrControllerInput ControllerInput
        {
            get
            {
                // First look for a controller that is not dummy nor gamepad:
                UxrControllerInput controllerInput = UxrControllerInput.GetComponents(this).FirstOrDefault(i => i.GetType() != typeof(UxrDummyControllerInput) && i.GetType() != typeof(UxrGamepadInput));

                // No controllers found? Try gamepad
                if (controllerInput == null)
                {
                    controllerInput = UxrControllerInput.GetComponents(this).FirstOrDefault(i => i.GetType() == typeof(UxrGamepadInput));    
                }

                // No controllers found? Return dummy to avoid null reference exceptions.
                if (controllerInput == null)
                {
                    UxrDummyControllerInput inputDummy = gameObject.GetOrAddComponent<UxrDummyControllerInput>();
                    return inputDummy;
                }

                return controllerInput;
            }
        }

        /// <summary>
        ///     Gets whether the current controller input is a dummy controller input component. Dummy controller input is used
        ///     when there is no VR input device available in order to avoid null checking.
        /// </summary>
        public bool HasDummyControllerInput => ControllerInput.GetType() == typeof(UxrDummyControllerInput);

        /// <summary>
        ///     Gets the avatar's camera position.
        /// </summary>
        public Vector3 CameraPosition => CameraComponent != null ? CameraComponent.transform.position : Vector3.zero;

        /// <summary>
        ///     Gets the avatar's camera floor position: the camera position projected on the floor. This is useful
        ///     when testing if the avatar is near a well known place or to trigger certain events.
        /// </summary>
        public Vector3 CameraFloorPosition
        {
            get
            {
                Vector3 cameraPosition = CameraPosition;
                return new Vector3(cameraPosition.x, transform.position.y, cameraPosition.z);
            }
        }

        /// <summary>
        ///     Gets the avatar's camera forward vector.
        /// </summary>
        public Vector3 CameraForward => CameraComponent != null ? CameraComponent.transform.forward : Vector3.zero;

        /// <summary>
        ///     Gets the avatar's camera forward vector projected onto the XZ plane, the floor.
        /// </summary>
        public Vector3 ProjectedCameraForward => CameraComponent != null ? Vector3.ProjectOnPlane(CameraComponent.transform.forward, transform.up).normalized : Vector3.zero;

        /// <summary>
        ///     Gets the currently enabled controller inputs belonging to the avatar, except for any
        ///     <see cref="UxrDummyControllerInput" /> component.
        /// </summary>
        public IEnumerable<UxrControllerInput> EnabledControllerInputs => UxrControllerInput.GetComponents(this).Where(i => i.GetType() != typeof(UxrDummyControllerInput));

        /// <summary>
        ///     Gets all (enabled or disabled) controller inputs belonging to the avatar, except for any
        ///     <see cref="UxrDummyControllerInput" /> component.
        /// </summary>
        public IEnumerable<UxrControllerInput> AllControllerInputs => UxrControllerInput.GetComponents(this, true).Where(i => i.GetType() != typeof(UxrDummyControllerInput));

        /// <summary>
        ///     Returns all available enabled tracking devices.
        /// </summary>
        public IEnumerable<UxrTrackingDevice> TrackingDevices => UxrTrackingDevice.GetComponents(this);

        /// <summary>
        ///     Gets the first enabled tracking device that inherits from <see cref="UxrControllerTracking" />,
        ///     meaning a standard left+right controller setup.
        /// </summary>
        public IUxrControllerTracking FirstControllerTracking => (IUxrControllerTracking)TrackingDevices.FirstOrDefault(i => i is IUxrControllerTracking);

        /// <summary>
        ///     Gets all the enabled <see cref="UxrFingerTip" /> components in the avatar.
        /// </summary>
        public IEnumerable<UxrFingerTip> FingerTips => UxrFingerTip.GetComponents(this);

        /// <summary>
        ///     Gets all the enabled <see cref="UxrLaserPointer" /> components in the avatar.
        /// </summary>
        public IEnumerable<UxrLaserPointer> LaserPointers => UxrLaserPointer.GetComponents(this);

        /// <summary>
        ///     Gets the avatar prefab Guid. It is stored instead of the GameObject because storing the GameObject would make an
        ///     instantiated avatar reference itself, instead of the source prefab.
        /// </summary>
        public string PrefabGuid => _prefabGuid;

        /// <summary>
        ///     Gets the parent prefab GameObject, if it exists.
        /// </summary>
        public GameObject ParentPrefab => _parentPrefab;

        /// <summary>
        ///     Gets the <see cref="ParentPrefab" />'s <see cref="UxrAvatar" /> component.
        /// </summary>
        public UxrAvatar ParentAvatarPrefab => ParentPrefab != null ? ParentPrefab.GetComponent<UxrAvatar>() : null;

        /// <summary>
        ///     Gets whether the avatar's prefab has a parent avatar prefab.
        /// </summary>
        public bool IsPrefabVariant => ParentPrefab != null;

        /// <summary>
        ///     Gets the avatar rig type.
        /// </summary>
        public UxrAvatarRigType AvatarRigType => _rigType;

        /// <summary>
        ///     Gets the avatar's camera component.
        /// </summary>
        public Camera CameraComponent
        {
            get
            {
                if (_camera == null || _camera.enabled == false)
                {
                    Camera newCamera = GetComponentInChildren<Camera>();

                    if (newCamera != null)
                    {
                        _camera = newCamera;
                    }
                }

                return _camera;
            }
        }

        /// <summary>
        ///     Gets the avatar's camera transform.
        /// </summary>
        public Transform CameraTransform => CameraComponent ? CameraComponent.transform : null;

        /// <summary>
        ///     Gets the avatar's camera fade component. If the camera doesn't currently have one, it will be added.
        /// </summary>
        public UxrCameraFade CameraFade => CameraComponent.gameObject.GetOrAddComponent<UxrCameraFade>();

        /// <summary>
        ///     Gets the renderers used by the avatar, excluding controllers and everything related to them.
        /// </summary>
        public IEnumerable<Renderer> AvatarRenderers => _avatarRenderers;

        /// <summary>
        ///     Gets the left hand.
        /// </summary>
        public UxrAvatarHand LeftHand => AvatarRig.LeftArm.Hand;

        /// <summary>
        ///     Gets the right hand.
        /// </summary>
        public UxrAvatarHand RightHand => AvatarRig.RightArm.Hand;

        /// <summary>
        ///     Gets the left hand bone.
        /// </summary>
        public Transform LeftHandBone => AvatarRig.LeftArm.Hand.Wrist;

        /// <summary>
        ///     Gets the right hand bone.
        /// </summary>
        public Transform RightHandBone => AvatarRig.RightArm.Hand.Wrist;

        /// <summary>
        ///     Gets the left hand's grabber component. Null if no left <see cref="UxrGrabber" /> component was found.
        /// </summary>
        public UxrGrabber LeftGrabber
        {
            get
            {
#if UNITY_EDITOR

                if (Application.isEditor && !Application.isPlaying)
                {
                    return GetComponentsInChildren<UxrGrabber>().FirstOrDefault(g => g.Side == UxrHandSide.Left);
                }

#endif
                return UxrGrabber.GetComponents(this).FirstOrDefault(g => g.Side == UxrHandSide.Left);
            }
        }

        /// <summary>
        ///     Gets the right hand's grabber component. Null if no right <see cref="UxrGrabber" /> component was found.
        /// </summary>
        public UxrGrabber RightGrabber
        {
            get
            {
#if UNITY_EDITOR

                if (Application.isEditor && !Application.isPlaying)
                {
                    return GetComponentsInChildren<UxrGrabber>().FirstOrDefault(g => g.Side == UxrHandSide.Right);
                }

#endif
                return UxrGrabber.GetComponents(this).FirstOrDefault(g => g.Side == UxrHandSide.Right);
            }
        }

        /// <summary>
        ///     Gets the default hand pose name or null if there isn't any default hand pose set.
        /// </summary>
        public string DefaultHandPoseName => DefaultHandPose != null ? DefaultHandPose.name : null;

        /// <summary>
        ///     Gets the <see cref="Transform" /> of the camera controller (the parent transform of the avatar's camera).
        /// </summary>
        public Transform CameraController { get; private set; }

        /// <summary>
        ///     Gets the avatar controller, responsible for updating the avatar.
        /// </summary>
        public UxrAvatarController AvatarController { get; private set; }

        /// <summary>
        ///     Gets or sets the avatar operating mode.
        /// </summary>
        public UxrAvatarMode AvatarMode
        {
            get => _avatarMode;
            set
            {
                _avatarMode = value;

                if (CameraController)
                {
                    CameraController.gameObject.SetActive(value == UxrAvatarMode.Local);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the avatar render mode.
        /// </summary>
        public UxrAvatarRenderModes RenderMode
        {
            get => _renderMode;
            set
            {
                _renderMode = value;

                // Enable or disable avatar renderers

                _avatarRenderers?.ForEach(r => r.enabled = value.HasFlag(UxrAvatarRenderModes.Avatar));

                // Enable/disable controller 3d models (and controller hands) depending on if their input component is active

                IEnumerable<UxrControllerInput> controllerInputs = UxrControllerInput.GetComponents(this, true);

                foreach (UxrControllerInput controllerInput in controllerInputs)
                {
                    // Here we do some additional checks in case two components reference the same 3D model(s):

                    bool leftControllerEnabled  = controllerInputs.Any(c => c.LeftController3DModel == controllerInput.LeftController3DModel && c.enabled);
                    bool rightControllerEnabled = controllerInputs.Any(c => c.RightController3DModel == controllerInput.RightController3DModel && c.enabled);
                    bool showAvatar             = value.HasFlag(UxrAvatarRenderModes.Avatar);
                    bool showControllerLeft     = value.HasFlag(UxrAvatarRenderModes.LeftController);
                    bool showControllerRight    = value.HasFlag(UxrAvatarRenderModes.RightController);

                    if (controllerInput.SetupType == UxrControllerSetupType.Single)
                    {
                        // In single setups there is only one device for both hands.

                        if (controllerInput.LeftController3DModel)
                        {
                            controllerInput.LeftController3DModel.IsControllerVisible = (leftControllerEnabled && showControllerLeft) || (rightControllerEnabled && showControllerRight);
                            controllerInput.LeftController3DModel.IsHandVisible       = _showControllerHands;
                        }

                        controllerInput.EnableObjectListSingle((leftControllerEnabled || rightControllerEnabled) && showAvatar);
                    }
                    else if (controllerInput.SetupType == UxrControllerSetupType.Dual)
                    {
                        if (controllerInput.LeftController3DModel)
                        {
                            controllerInput.LeftController3DModel.IsControllerVisible = leftControllerEnabled && showControllerLeft;
                            controllerInput.LeftController3DModel.IsHandVisible       = _showControllerHands;
                        }

                        if (controllerInput.RightController3DModel)
                        {
                            controllerInput.RightController3DModel.IsControllerVisible = rightControllerEnabled && showControllerRight;
                            controllerInput.RightController3DModel.IsHandVisible       = _showControllerHands;
                        }

                        controllerInput.EnableObjectListLeft(leftControllerEnabled && showAvatar);
                        controllerInput.EnableObjectListRight(rightControllerEnabled && showAvatar);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether the hands will be shown when the controllers are being rendered.
        /// </summary>
        public bool ShowControllerHands
        {
            get => _showControllerHands;
            set
            {
                _showControllerHands = value;

                foreach (UxrControllerInput controllerInput in UxrControllerInput.GetComponents(this))
                {
                    if (controllerInput.LeftController3DModel)
                    {
                        controllerInput.LeftController3DModel.IsHandVisible = _showControllerHands;
                    }

                    if (controllerInput.RightController3DModel)
                    {
                        controllerInput.RightController3DModel.IsHandVisible = _showControllerHands;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the default hand pose. The default hand pose is the pose a hand will have when no other event that
        ///     changes the pose is happening. Usually it is a neutral, relaxed pose.
        /// </summary>
        public UxrHandPoseAsset DefaultHandPose
        {
            get => GetDefaultPoseInHierarchy();
            set => _defaultHandPose = value;
        }

        #endregion

        #region Implicit IUxrStateSync

        /// <inheritdoc />
        public event EventHandler<UxrStateSyncEventArgs> StateChanged;

        /// <inheritdoc />
        public void SyncState(UxrStateSyncEventArgs e, bool propagateEvents)
        {
            if (!(e is UxrAvatarSyncEventArgs syncArgs))
            {
                return;
            }

            switch (syncArgs.EventType)
            {
                case UxrAvatarSyncEventType.AvatarMove:
                    UxrManager.Instance.MoveAvatarTo(this, syncArgs.AvatarMoveEventArgs.NewPosition, syncArgs.AvatarMoveEventArgs.NewForward, propagateEvents);
                    break;

                case UxrAvatarSyncEventType.HandPose:
                    SetCurrentHandPose(syncArgs.HandPoseChangeEventArgs.HandSide, syncArgs.HandPoseChangeEventArgs.PoseName, syncArgs.HandPoseChangeEventArgs.BlendValue, propagateEvents);
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Enables or disables the <see cref="UxrFingerTip" /> components in the avatar.
        /// </summary>
        /// <param name="enable">Whether to enable or disable the components.</param>
        public void EnableFingerTips(bool enable)
        {
            UxrFingerTip.GetComponents(this, true).ForEach(t => t.enabled = enable);
        }

        /// <summary>
        ///     Enables or disables the <see cref="UxrLaserPointer" /> components in the avatar.
        /// </summary>
        /// <param name="enable">Whether to enable or disable the components.</param>
        public void EnableLaserPointers(bool enable)
        {
            UxrLaserPointer.GetComponents(this, true).ForEach(t => t.gameObject.SetActive(enable));
        }

        /// <summary>
        ///     Enables or disables the <see cref="UxrLocomotion" /> components in the avatar.
        /// </summary>
        /// <param name="enable">Whether to enable or disable the components.</param>
        public void EnableLocomotion(bool enable)
        {
            UxrLocomotion.GetComponents(this, true).ForEach(t => t.enabled = enable);
        }

        /// <summary>
        ///     Gets the transform in the given hand controller that points forward. The controller needs to have a 3D model
        ///     assigned to it, which all controllers in the framework have.
        /// </summary>
        /// <param name="handSide">Hand to get the forward of</param>
        /// <returns>
        ///     The transform describing the controller forward direction (use <see cref="Transform.forward" /> to get the
        ///     actual vector). Null if there is no input controller available or if it doesn't have a 3D model assigned.
        /// </returns>
        public Transform GetControllerInputForward(UxrHandSide handSide)
        {
            if (!HasDummyControllerInput)
            {
                UxrController3DModel model = ControllerInput.GetController3DModel(handSide);

                if (model != null)
                {
                    return model.Forward;
                }
            }

            return null;
        }

        /// <summary>
        ///     Checks if the avatar is looking at a point in space.
        /// </summary>
        /// <param name="worldPosition">World position to check.</param>
        /// <param name="radiusFromViewportCenterAllowed">
        ///     Radius threshold in viewport coordinates [0.0, 1.0] above which it isn't
        ///     considered looking at anymore.
        /// </param>
        /// <returns>Whether the avatar is looking at the specific point in space.</returns>
        public bool IsLookingAt(Vector3 worldPosition, float radiusFromViewportCenterAllowed)
        {
            Vector3 viewPortPoint = CameraComponent.WorldToViewportPoint(worldPosition);
            return Vector3.Distance(new Vector3(0.5f, 0.5f, viewPortPoint.z), viewPortPoint) <= radiusFromViewportCenterAllowed;
        }

        /// <summary>
        ///     Gets the initial local position of the given avatar bone.
        /// </summary>
        /// <param name="bone">Bone to get the initial local position of.</param>
        /// <param name="position">Gets the initial local position.</param>
        /// <returns>
        ///     Boolean telling whether the position was successfully retrieved or if the given bone was not found in the
        ///     avatar.
        /// </returns>
        public bool GetInitialBoneLocalPosition(Transform bone, out Vector3 position)
        {
            return _initialBoneLocalPositions.TryGetValue(bone, out position);
        }

        /// <summary>
        ///     Gets the initial local rotation of the given avatar bone.
        /// </summary>
        /// <param name="bone">Bone to get the initial local rotation of.</param>
        /// <param name="rotation">Gets the initial local rotation.</param>
        /// <returns>
        ///     Boolean telling whether the rotation was successfully retrieved or if the given bone was not found in the
        ///     avatar.
        /// </returns>
        public bool GetInitialBoneLocalRotation(Transform bone, out Quaternion rotation)
        {
            return _initialBoneLocalRotations.TryGetValue(bone, out rotation);
        }

        /// <summary>
        ///     Places the camera at floor level.
        /// </summary>
        public void SetCameraAtFloorLevel()
        {
            if (_camera && CameraController)
            {
                Transform cameraTransform     = CameraController.transform;
                Vector3   cameraControllerPos = cameraTransform.position;
                cameraControllerPos.y    = transform.position.y + _startCameraControllerHeight - _startCameraHeight;
                cameraTransform.position = cameraControllerPos;
            }
        }

        /// <summary>
        ///     Gets the <see cref="UxrAvatarArm" /> rig information for the given arm.
        /// </summary>
        /// <param name="handSide">Which arm to get</param>
        /// <returns><see cref="UxrAvatarArm" /> rig information</returns>
        public UxrAvatarArm GetArm(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? AvatarRig.LeftArm : AvatarRig.RightArm;
        }

        /// <summary>
        ///     Gets the <see cref="UxrAvatarHand" /> rig information for the given hand.
        /// </summary>
        /// <param name="handSide">Which hand to get</param>
        /// <returns><see cref="UxrAvatarHand" /> rig information</returns>
        public UxrAvatarHand GetHand(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? LeftHand : RightHand;
        }

        /// <summary>
        ///     Gets the given hand bone.
        /// </summary>
        /// <param name="handSide">Which hand bone to get</param>
        /// <returns>Bone transform</returns>
        public Transform GetHandBone(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? LeftHandBone : RightHandBone;
        }

        /// <summary>
        ///     Gets the <see cref="UxrGrabber" /> component for the given hand.
        /// </summary>
        /// <param name="handSide">Which hand to get the component for</param>
        /// <returns><see cref="UxrGrabber" /> component or null if no component was found for the given side</returns>
        public UxrGrabber GetGrabber(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? LeftGrabber : RightGrabber;
        }

        /// <summary>
        ///     Sets up the <see cref="AvatarRig" /> using information from the <see cref="Animator" /> if it describes a humanoid
        ///     avatar.
        /// </summary>
        /// <returns>Whether any humanoid data was found and assigned</returns>
        public bool SetupRigElementsFromAnimator()
        {
            Animator[] animators = GetComponentsInChildren<Animator>();

            foreach (Animator animator in animators)
            {
                if (UxrAvatarRig.SetupRigElementsFromAnimator(_rig, animator))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Clears all the <see cref="AvatarRig" /> element references.
        /// </summary>
        public void ClearRigElements()
        {
            _rig?.ClearRigElements();
        }

        /// <summary>
        ///     Tries to infer rig elements by doing some checks on names and bone hierarchy.
        ///     This is useful when we have a rig that has no full humanoid avatar set up on its animator .
        /// </summary>
        public void TryToInferMissingRigElements()
        {
            UxrAvatarRig.TryToInferMissingRigElements(_rig, GetAllAvatarRendererComponents());
        }

        /// <summary>
        ///     Gets all <see cref="Renderer" /> components except those hanging from a <see cref="UxrHandIntegration" /> object,
        ///     which are renderers that are not part of the avatar itself but part of the supported input controllers / controller
        ///     hands that can be rendered too.
        /// </summary>
        public IEnumerable<SkinnedMeshRenderer> GetAllAvatarRendererComponents()
        {
            SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            return skins.Where(s => s.SafeGetComponentInParent<UxrHandIntegration>() == null);
        }

        /// <summary>
        ///     Gets the <see cref="UxrAvatar" /> component chain. This is the avatar's own <see cref="UxrAvatar" /> component
        ///     followed by all parent prefab <see cref="UxrAvatar" /> components up to the root parent prefab.
        ///     If the avatar is an instance the first component will reference its own component instantiated in the scene,
        ///     not the avatar component in the source prefab.
        ///     <see cref="GetPrefabGuidChain()" /> can be used to traverse the prefab chain information including the source
        ///     prefab.
        /// </summary>
        /// <returns>
        ///     Upwards <see cref="UxrAvatar" /> component chain. If the avatar is a prefab itself, it will be the first component
        ///     in the list. If the avatar is an instance, the first component will be the one from the instance instead of the
        ///     source prefab. Components cannot store references to their own prefab because on instantiation they will
        ///     automatically point to the instantiated component. <see cref="GetPrefabGuidChain()" /> can be used to traverse the
        ///     prefab information chain, including the source prefab.
        /// </returns>
        /// <seealso cref="GetPrefabGuidChain" />
        public IEnumerable<UxrAvatar> GetAvatarChain()
        {
            yield return this;

            UxrAvatar current = ParentAvatarPrefab;

            while (current != null && current != this)
            {
                yield return current;

                current = current.ParentAvatarPrefab;
            }
        }

        /// <summary>
        ///     Gets the prefab GUID chain. This is the source prefab Guid followed by all parent prefab GUIDs up to the root
        ///     parent prefab.
        /// </summary>
        /// <returns>
        ///     Upwards prefab GUID chain. If the avatar is a prefab itself, it will be the first GUID in the list.
        /// </returns>
        /// <seealso cref="GetAvatarChain" />
        public IEnumerable<string> GetPrefabGuidChain()
        {
            if (!string.IsNullOrEmpty(_prefabGuid))
            {
                yield return _prefabGuid;

                UxrAvatar current = ParentAvatarPrefab;

                while (current != null && current != this)
                {
                    yield return current.PrefabGuid;

                    current = current.ParentAvatarPrefab;
                }
            }
        }

        /// <summary>
        ///     Gets the parent prefab chain. These are all parent prefabs up to the root parent prefab.
        /// </summary>
        /// <returns>
        ///     Upwards prefab chain starting by the parent prefab.
        /// </returns>
        public IEnumerable<UxrAvatar> GetParentPrefabChain()
        {
            UxrAvatar current = ParentAvatarPrefab;

            while (current != null && current != this)
            {
                yield return current;

                current = current.ParentAvatarPrefab;
            }
        }

        /// <summary>
        ///     Gets the parent prefab that stores a given hand pose.
        /// </summary>
        /// <param name="handPoseAsset">Hand pose to look for</param>
        /// <returns>Parent prefab that stores the given hand pose or null if the pose was not found</returns>
        public UxrAvatar GetParentPrefab(UxrHandPoseAsset handPoseAsset)
        {
            return GetParentPrefabChain().FirstOrDefault(avatar => avatar._handPoses.Contains(handPoseAsset));
        }

        /// <summary>
        ///     Gets the parent prefab that stores a given hand pose.
        /// </summary>
        /// <param name="poseName">Hand pose name to look for</param>
        /// <returns>Parent prefab that stores the given hand pose or null if the pose was not found</returns>
        public UxrAvatar GetParentPrefab(string poseName)
        {
            return GetParentPrefabChain().FirstOrDefault(avatar => avatar._handPoses.Any(p => p != null && p.name == poseName));
        }

        /// <summary>
        ///     Checks whether the avatar shares the same prefab or is a prefab variant of another avatar.
        /// </summary>
        /// <param name="avatar">Avatar instance or prefab. If it is an instance, it will be checked against the instance's prefab</param>
        /// <returns>Whether the avatar shares the same prefab or is a prefab variant</returns>
        public bool IsPrefabCompatibleWith(UxrAvatar avatar)
        {
            foreach (string guid in GetPrefabGuidChain())
            {
                if (_prefabGuid == guid)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets all hand poses in the avatar, including those inherited from parent prefabs. If more than one pose exists
        ///     with the same name, only the overriden will be in the results.
        /// </summary>
        /// <returns>List of all poses in the avatar and any of its parent prefabs</returns>
        public IEnumerable<UxrHandPoseAsset> GetAllHandPoses()
        {
            Dictionary<string, UxrHandPoseAsset> poses = new Dictionary<string, UxrHandPoseAsset>();

            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                foreach (UxrHandPoseAsset handPoseAsset in avatar._handPoses)
                {
                    if (handPoseAsset != null && !poses.ContainsKey(handPoseAsset.name))
                    {
                        poses.Add(handPoseAsset.name, handPoseAsset);
                    }
                }
            }

            return poses.Values.OrderBy(p => p.name);
        }

        /// <summary>
        ///     Gets the hand poses in the avatar, not counting those inherited from parent prefabs.
        /// </summary>
        /// <returns>Pose names</returns>
        public IEnumerable<UxrHandPoseAsset> GetHandPoses()
        {
            return _handPoses.Where(p => p != null).OrderBy(p => p.name);
        }

        /// <summary>
        ///     Gets a given hand pose. It can happen that the pose name is present in a prefab/instance and at the same time also
        ///     in any of the parent prefabs upwards in the hierarchy. In this case the hand pose in the child will always have
        ///     priority so that child prefabs can override poses that they inherit from parent prefabs.
        /// </summary>
        /// <param name="poseName">Pose to get</param>
        /// <param name="usePrefabInheritance">Whether to look for the pose in any prefab above the first prefab in the hierarchy.</param>
        /// <returns>Hand pose object</returns>
        public UxrHandPoseAsset GetHandPose(string poseName, bool usePrefabInheritance = true)
        {
            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                foreach (UxrHandPoseAsset handPoseAsset in avatar.GetHandPoses())
                {
                    if (handPoseAsset.name == poseName)
                    {
                        return handPoseAsset;
                    }
                }

                if (!usePrefabInheritance)
                {
                    break;
                }
            }

            // Not found

            return null;
        }

        /// <summary>
        ///     Checks if a given pose has been overriden in any point in the prefab hierarchy.
        /// </summary>
        /// <param name="poseName">Pose name</param>
        /// <param name="avatarPrefab">Returns the avatar prefab that has the original pose</param>
        /// <returns>Whether the given pose has been overriden at any point in the prefab hierarchy</returns>
        public bool IsHandPoseOverriden(string poseName, out UxrAvatar avatarPrefab)
        {
            avatarPrefab = null;
            int count = 0;

            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                if (avatar._handPoses.Any(p => p != null && p.name == poseName))
                {
                    count++;

                    if (count == 2)
                    {
                        avatarPrefab = avatar;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a given pose has been overriden in the prefab hierarchy.
        /// </summary>
        /// <param name="handPoseAsset">Hand pose</param>
        /// <returns>Whether the given pose has been overriden in the prefab hierarchy</returns>
        public bool IsHandPoseOverriden(UxrHandPoseAsset handPoseAsset)
        {
            // Traverse upwards in the prefab hierarchy

            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                if (avatar.GetHandPoses().Contains(handPoseAsset))
                {
                    // Found pose itself
                    return false;
                }

                if (avatar._handPoses.Any(p => p != null && p.name == handPoseAsset.name))
                {
                    // Found pose name, which is an override
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets a given hand pose. This method is intended for use at runtime to animate the avatars. While
        ///     <see cref="GetHandPose" /> uses <see cref="UxrHandPoseAsset" />, <see cref="GetRuntimeHandPose" /> uses
        ///     <see cref="UxrRuntimeHandPose" />.
        ///     The main differences are:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrRuntimeHandPose" /> objects contain transforms that require less operations and are valid
        ///             only for this avatar. They are high-performant and cached at the beginning, making them only available at
        ///             runtime. They are used to animate the avatar hands at runtime.
        ///         </item>
        ///         <item>
        ///             <see cref="UxrHandPoseAsset" /> objects contain transforms that are independent of the coordinate system
        ///             used.
        ///             They require more operations but can be applied to any avatar. They are mainly used in editor mode.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="poseName"></param>
        /// <returns>Hand pose</returns>
        public UxrRuntimeHandPose GetRuntimeHandPose(string poseName)
        {
            if (_cachedRuntimeHandPoses != null && !string.IsNullOrEmpty(poseName) && _cachedRuntimeHandPoses.TryGetValue(poseName, out UxrRuntimeHandPose handPose))
            {
                return handPose;
            }

            return null;
        }

        /// <summary>
        ///     Gets the current runtime hand pose.
        /// </summary>
        /// <param name="handSide">Which hand to retrieve</param>
        /// <returns>Runtime hand pose or null if there isn't any</returns>
        public UxrRuntimeHandPose GetCurrentRuntimeHandPose(UxrHandSide handSide)
        {
            HandState handState = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;
            return handState.CurrentHandPose;
        }

        /// <summary>
        ///     Sets the currently active pose for a given hand in the avatar at runtime. Blending is used to transition between
        ///     poses smoothly, which means the pose is not immediately adopted. To adopt a pose immediately at a given instant use
        ///     <see cref="SetCurrentHandPoseImmediately" /> instead.
        /// </summary>
        /// <param name="handSide">Which hand the new pose will be applied to</param>
        /// <param name="poseName">The new pose name</param>
        /// <param name="blendValue">The blend value if the pose is a blend pose</param>
        /// <param name="propagateEvents">
        ///     Whether the change should generate events (<see cref="HandPoseChanging" />,
        ///     <see cref="HandPoseChanged" />, <see cref="GlobalHandPoseChanging" />, <see cref="GlobalHandPoseChanged" />).
        /// </param>
        /// <returns>Whether the pose was found</returns>
        public bool SetCurrentHandPose(UxrHandSide handSide, string poseName, float blendValue = 0.0f, bool propagateEvents = true)
        {
            if (string.IsNullOrEmpty(poseName))
            {
                return false;
            }

            UxrRuntimeHandPose handPose = GetRuntimeHandPose(poseName);

            if (handPose == null)
            {
                Debug.LogWarning($"Pose {poseName} was not found in avatar {name}");
                return false;
            }

            HandState                        handState                = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;
            UxrAvatarHandPoseChangeEventArgs avatarHandPoseChangeArgs = new UxrAvatarHandPoseChangeEventArgs(this, handSide, poseName, blendValue);

            if (!handState.IsChange(avatarHandPoseChangeArgs))
            {
                return true;
            }

            if (propagateEvents)
            {
                OnHandPoseChanging(avatarHandPoseChangeArgs);
            }

            handState.SetPose(handPose, blendValue);

            if (propagateEvents)
            {
                OnHandPoseChanged(avatarHandPoseChangeArgs);
            }

            return true;
        }

        /// <summary>
        ///     Sets the currently active pose blend value, if the current pose is <see cref="UxrHandPoseType.Blend" />.
        /// </summary>
        /// <param name="handSide">Which hand the new pose will be applied to</param>
        /// <param name="blendValue">The blend value if the pose is a blend pose</param>
        /// <param name="propagateEvents">
        ///     Whether the change should generate events (<see cref="HandPoseChanging" />,
        ///     <see cref="HandPoseChanged" />, <see cref="GlobalHandPoseChanging" />, <see cref="GlobalHandPoseChanged" />).
        /// </param>
        public void SetCurrentHandPoseBlendValue(UxrHandSide handSide, float blendValue, bool propagateEvents = true)
        {
            HandState                        handState                = handSide == UxrHandSide.Left ? _leftHandState : _rightHandState;
            UxrAvatarHandPoseChangeEventArgs avatarHandPoseChangeArgs = new UxrAvatarHandPoseChangeEventArgs(this, handSide, handState.CurrentHandPoseName, blendValue);

            if (!handState.IsChange(avatarHandPoseChangeArgs))
            {
                return;
            }

            if (propagateEvents)
            {
                OnHandPoseChanging(avatarHandPoseChangeArgs);
            }

            handState.SetPose(handState.CurrentHandPose, blendValue);

            if (handSide == UxrHandSide.Left)
            {
                _leftHandState.Update(this, UxrHandSide.Left, Time.deltaTime);
            }

            if (handSide == UxrHandSide.Right)
            {
                _rightHandState.Update(this, UxrHandSide.Right, Time.deltaTime);
            }

            if (propagateEvents)
            {
                OnHandPoseChanged(avatarHandPoseChangeArgs);
            }
        }

        /// <summary>
        ///     Adopts a given hand pose by changing the transforms immediately. The bones may be overriden at any other point or
        ///     the next frame if there is a hand pose currently enabled using <see cref="SetCurrentHandPose" />.
        /// </summary>
        /// <param name="handSide">Which hand to apply the pose to</param>
        /// <param name="poseName">Hand pose name</param>
        /// <param name="blendPoseType">Blend pose to use if the hand pose is <see cref="UxrHandPoseType.Blend" /></param>
        public void SetCurrentHandPoseImmediately(UxrHandSide handSide, string poseName, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            if (string.IsNullOrEmpty(poseName))
            {
                return;
            }

            UxrHandPoseAsset handPoseAsset = GetHandPose(poseName);

            if (handPoseAsset != null)
            {
                SetCurrentHandPoseImmediately(handSide, handPoseAsset, blendPoseType);
            }
        }

        /// <summary>
        ///     Adopts a given hand pose by changing the transforms immediately. The bones may be overriden at any other point or
        ///     the next frame if there is a hand pose currently enabled using <see cref="SetCurrentHandPose" />.
        /// </summary>
        /// <param name="handSide">Which hand to apply the pose to</param>
        /// <param name="handPoseAsset">Hand pose</param>
        /// <param name="blendPoseType">Blend pose to use if the hand pose is <see cref="UxrHandPoseType.Blend" /></param>
        public void SetCurrentHandPoseImmediately(UxrHandSide handSide, UxrHandPoseAsset handPoseAsset, UxrBlendPoseType blendPoseType = UxrBlendPoseType.None)
        {
            UxrAvatarRig.UpdateHandUsingDescriptor(GetHand(handSide),
                                                   handPoseAsset.GetHandDescriptor(handSide, handPoseAsset.PoseType, blendPoseType),
                                                   AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes,
                                                   AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Called whenever the avatar was moved.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void NotifyAvatarMoved(UxrAvatarMoveEventArgs e)
        {
            StateChanged?.Invoke(this, new UxrAvatarSyncEventArgs(e));
        }

        /// <summary>
        ///     Updates the hand transforms using their current pose information using smooth blending.
        /// </summary>
        internal void UpdateHandPoseTransforms()
        {
            _leftHandState.Update(this, UxrHandSide.Left, Time.deltaTime);
            _rightHandState.Update(this, UxrHandSide.Right, Time.deltaTime);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the avatar.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Make sure the UxrManager singleton is available at this point.
            UxrManager.Instance.Poke();

            AvatarController = GetComponent<UxrAvatarController>();
            _camera          = GetComponentInChildren<Camera>();

            // Find Camera controller
            if (_camera != null)
            {
                CameraController = _camera.transform.parent;
            }

            while (CameraController != null && CameraController.parent != transform)
            {
                CameraController = CameraController.parent;
            }

            if (CameraController.parent != transform)
            {
                Debug.LogWarning("Error finding Camera Controller. Camera Controller is a GameObject that needs to be child of the avatar and have the avatar camera as a child");
            }

            if (_camera != null)
            {
                _startCameraHeight = _camera.transform.position.y - transform.position.y;
            }

            if (CameraController != null)
            {
                _startCameraControllerHeight = CameraController.position.y - transform.position.y;
            }

            // We do the following because we update the skin bones manually
            foreach (Renderer avatarRenderer in _avatarRenderers)
            {
                SkinnedMeshRenderer skin = avatarRenderer as SkinnedMeshRenderer;

                if (skin != null)
                {
                    skin.updateWhenOffscreen = true;
                }
            }

            // Compute initial bone positions/rotations
            _initialBoneLocalRotations = new Dictionary<Transform, Quaternion>();
            _initialBoneLocalPositions = new Dictionary<Transform, Vector3>();

            foreach (Renderer avatarRenderer in _avatarRenderers)
            {
                SkinnedMeshRenderer skin = avatarRenderer as SkinnedMeshRenderer;

                if (skin != null)
                {
                    foreach (Transform bone in skin.bones)
                    {
                        if (bone != null && _initialBoneLocalRotations.ContainsKey(bone) == false)
                        {
                            _initialBoneLocalRotations.Add(bone, bone.localRotation);
                            _initialBoneLocalPositions.Add(bone, bone.localPosition);
                        }
                    }
                }
            }

            // Try to infer missing elements

            TryToInferMissingRigElements();

            if (AvatarRigInfo.SerializedVersion != UxrAvatarRigInfo.CurrentVersion)
            {
                CreateRigInfo();
            }

            // Subscribe to device events

            foreach (UxrTrackingDevice tracking in UxrTrackingDevice.GetComponents(this, true))
            {
                tracking.DeviceConnected += Tracking_DeviceConnected;
            }

            foreach (UxrControllerInput controllerInput in UxrControllerInput.GetComponents(this, true))
            {
                controllerInput.DeviceConnected += ControllerInput_DeviceConnected;
            }

            // Cache hand poses by name

            CreateHandPoseCache();
            
#if ULTIMATEXR_UNITY_XR_MANAGEMENT

            // New Unity XR requires TrackedPoseDriver component in cameras

            if (CameraController != null && _avatarMode == UxrAvatarMode.Local)
            {
                Camera[] avatarCameras = CameraController.GetComponentsInChildren<Camera>();

                foreach (Camera camera in avatarCameras)
                {
                    bool hasInputSystemPoseDriver = false;
            
#if ULTIMATEXR_USE_UNITYINPUTSYSTEM_SDK
                    hasInputSystemPoseDriver = camera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() != null;
#endif
                    TrackedPoseDriver trackedPoseDriver = camera.GetComponent<TrackedPoseDriver>();
                    
                    if (trackedPoseDriver == null && !hasInputSystemPoseDriver)
                    {
                        trackedPoseDriver = camera.gameObject.AddComponent<TrackedPoseDriver>();

                        trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
                        trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                        trackedPoseDriver.updateType   = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                    }
                }
            }

#endif
        }

        /// <summary>
        ///     Makes sure the rig constructor is called when the component is reset.
        /// </summary>
        protected override void Reset()
        {
            _rig     = new UxrAvatarRig();
            _rigInfo = new UxrAvatarRigInfo();
        }

        /// <summary>
        ///     Additional avatar initialization.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            RenderMode = _renderMode;

            if (AvatarMode == UxrAvatarMode.Local)
            {
                LocalAvatarStarted?.Invoke(this, new UxrAvatarStartedEventArgs(this));
            }
        }

        /// <summary>
        ///     Called when inspector fields are changed. It is used to regenerate the rig information.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            CreateRigInfo();

            if (_avatarRenderers == null || _avatarRenderers.Count == 0)
            {
                _avatarRenderers = GetAllAvatarRendererComponents().Cast<Renderer>().ToList();
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever a tracking device is connected. The tracking device list is sorted so that they can be iterated in
        ///     the correct update order.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Tracking_DeviceConnected(object sender, UxrDeviceConnectEventArgs e)
        {
            // Sort tracking components by update priority to perform avatar updates in the correct order
            UxrTrackingDevice.SortComponents((a, b) => a.TrackingUpdateOrder.CompareTo(b.TrackingUpdateOrder));
        }

        /// <summary>
        ///     Called whenever an input device is connected. The avatar render mode is refreshed in order to update possible
        ///     controller 3D models.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void ControllerInput_DeviceConnected(object sender, UxrDeviceConnectEventArgs e)
        {
            // Refresh avatar
            RenderMode = _renderMode;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="HandPoseChanging" /> and <see cref="GlobalHandPoseChanging" /> events.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnHandPoseChanging(UxrAvatarHandPoseChangeEventArgs e)
        {
            HandPoseChanging?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="HandPoseChanged" /> and <see cref="GlobalHandPoseChanged" /> events.
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected virtual void OnHandPoseChanged(UxrAvatarHandPoseChangeEventArgs e)
        {
            HandPoseChanged?.Invoke(this, e);
            StateChanged?.Invoke(this, new UxrAvatarSyncEventArgs(e));
        }

        /// <summary>
        ///     Raises the <see cref="AvatarUpdating" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseAvatarUpdating(UxrAvatarUpdateEventArgs e)
        {
            AvatarUpdating?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="AvatarUpdated" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseAvatarUpdated(UxrAvatarUpdateEventArgs e)
        {
            AvatarUpdated?.Invoke(this, e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the first non-null default hand pose in the prefab hierarchy.
        /// </summary>
        /// <returns>Default hand pose asset or null if not found</returns>
        private UxrHandPoseAsset GetDefaultPoseInHierarchy()
        {
            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                if (avatar._defaultHandPose != null)
                {
                    return avatar._defaultHandPose;
                }
            }

            return null;
        }

        /// <summary>
        ///     Creates the rig information object.
        /// </summary>
        private void CreateRigInfo()
        {
            _rigInfo.Compute(this);
        }

        /// <summary>
        ///     Creates a dictionary to map pose names to <see cref="UxrRuntimeHandPose" />. If a given pose name is present more
        ///     than once, the overriden pose in the child prefab/instance is stored.
        /// </summary>
        private void CreateHandPoseCache()
        {
            if (!AvatarRig.HasFingerData())
            {
                return;
            }

            _cachedRuntimeHandPoses = new Dictionary<string, UxrRuntimeHandPose>();

            foreach (UxrAvatar avatar in GetAvatarChain())
            {
                foreach (UxrHandPoseAsset handPoseAsset in avatar.GetHandPoses())
                {
                    if (handPoseAsset != null && !_cachedRuntimeHandPoses.ContainsKey(handPoseAsset.name))
                    {
                        _cachedRuntimeHandPoses.Add(handPoseAsset.name, new UxrRuntimeHandPose(this, handPoseAsset));
                    }
                }
            }
        }

        #endregion

        #region Private Types & Data

        private readonly HandState _leftHandState  = new HandState();
        private readonly HandState _rightHandState = new HandState();

        private float                                  _startCameraHeight;
        private float                                  _startCameraControllerHeight;
        private Camera                                 _camera;
        private Dictionary<Transform, Quaternion>      _initialBoneLocalRotations = new Dictionary<Transform, Quaternion>();
        private Dictionary<Transform, Vector3>         _initialBoneLocalPositions = new Dictionary<Transform, Vector3>();
        private Dictionary<string, UxrRuntimeHandPose> _cachedRuntimeHandPoses;

        #endregion
    }
}

#pragma warning restore 414