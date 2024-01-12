// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPhotonFusionAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Attributes;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
using Fusion;
#endif


namespace UltimateXR.Networking.Integrations.Net.PhotonFusion
{
#if ULTIMATEXR_USE_PHOTONFUSION_SDK

    [OrderAfter(typeof(NetworkTransform), typeof(NetworkRigidbody))]
    public class UxrPhotonFusionAvatar : NetworkBehaviour, IUxrNetworkAvatar
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("List of objects that will be disabled when the avatar is in local mode, to avoid intersections with the camera for example")] [SerializeField] private List<GameObject> _localDisabledGameObjects;
        [SerializeField] [ReadOnly]                                                                                                                              private NetworkTransform _networkTransformRigAvatar;
        [SerializeField] [ReadOnly]                                                                                                                              private NetworkTransform _networkTransformRigCamera;
        [SerializeField] [ReadOnly]                                                                                                                              private NetworkTransform _networkTransformRigHandLeft;
        [SerializeField] [ReadOnly]                                                                                                                              private NetworkTransform _networkTransformRigHandRight;

        #endregion

        #region Implicit IUxrNetworkAvatar

        /// <inheritdoc />
        public IList<GameObject> LocalDisabledGameObjects => _localDisabledGameObjects;

        /// <inheritdoc />
        public UxrAvatar Avatar { get; private set; }

        /// <inheritdoc />
        public string AvatarName
        {
            get => _avatarName;
            set
            {
                _avatarName = value;

                if (Avatar != null)
                {
                    Avatar.name = value;
                }
            }
        }

        /// <inheritdoc />
        public event Action AvatarSpawned;

        /// <inheritdoc />
        public event Action AvatarDespawned;

        /// <inheritdoc />
        public void InitializeNetworkAvatar(UxrAvatar avatar, bool isLocal, string uniqueId, string avatarName)
        {
            AvatarName        = avatarName;
            avatar.AvatarMode = isLocal ? UxrAvatarMode.Local : UxrAvatarMode.UpdateExternally;

            if (isLocal)
            {
                LocalDisabledGameObjects.ForEach(o => o.SetActive(false));
            }

            avatar.ChangeUniqueId(uniqueId, true);
        }

        #endregion

        #region Public Overrides NetworkBehaviour

        /// <inheritdoc />
        public override void Spawned()
        {
            base.Spawned();

            Avatar = GetComponent<UxrAvatar>();

            InitializeNetworkAvatar(Avatar, Object.HasInputAuthority, Object.Id.ToString(), $"Player {Object.InputAuthority.PlayerId} ({(Object.HasInputAuthority ? "Local" : "External")})");

            if (Object.HasInputAuthority)
            {
                UxrManager.ComponentStateChanged += UxrManager_ComponentStateChanged;
            }

            AvatarSpawned?.Invoke();
        }

        /// <inheritdoc />
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

            if (Avatar && Object.HasInputAuthority)
            {
                UxrManager.ComponentStateChanged -= UxrManager_ComponentStateChanged;
            }

            AvatarDespawned?.Invoke();
        }

        /// <inheritdoc />
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            // update the rig at each network tick
            if (GetInput<UxrPhotonFusionNetwork.RigInput>(out var input))
            {
                _networkTransformRigAvatar.transform.position    = input.avatarPosition;
                _networkTransformRigAvatar.transform.rotation    = input.avatarRotation;
                _networkTransformRigAvatar.transform.localScale  = input.avatarScale;
                _networkTransformRigCamera.transform.position    = input.cameraPosition;
                _networkTransformRigCamera.transform.rotation    = input.cameraRotation;
                _networkTransformRigHandLeft.transform.position  = input.leftHandPosition;
                _networkTransformRigHandLeft.transform.rotation  = input.leftHandRotation;
                _networkTransformRigHandRight.transform.position = input.rightHandPosition;
                _networkTransformRigHandRight.transform.rotation = input.rightHandRotation;
            }
        }

        #endregion

        #region Public Overrides SimulationBehaviour

        /// <inheritdoc />
        public override void Render()
        {
            base.Render();

            if (Object.HasInputAuthority)
            {
                // Extrapolate for local user

                _networkTransformRigAvatar.InterpolationTarget.position   = _actualAvatarPosition;
                _networkTransformRigAvatar.InterpolationTarget.rotation   = _actualAvatarRotation;
                _networkTransformRigAvatar.InterpolationTarget.localScale = Avatar.transform.localScale;
                _networkTransformRigCamera.InterpolationTarget.position   = Avatar.CameraComponent.transform.position;
                _networkTransformRigCamera.InterpolationTarget.rotation   = Avatar.CameraComponent.transform.rotation;

                if (Avatar.FirstControllerTracking != null)
                {
                    _networkTransformRigHandLeft.InterpolationTarget.position  = Avatar.FirstControllerTracking.SensorLeftHandPos;
                    _networkTransformRigHandLeft.InterpolationTarget.rotation  = Avatar.FirstControllerTracking.SensorLeftHandRot;
                    _networkTransformRigHandRight.InterpolationTarget.position = Avatar.FirstControllerTracking.SensorRightHandPos;
                    _networkTransformRigHandRight.InterpolationTarget.rotation = Avatar.FirstControllerTracking.SensorRightHandRot;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Initializes the network rig, that synchronizes the relevant avatar transforms.
        /// </summary>
        /// <param name="root">The GameObject that synchronizes the root transform</param>
        /// <param name="cam">The GameObject that synchronizes the camera transform</param>
        /// <param name="handLeft">The GameObject that synchronizes the left hand transform</param>
        /// <param name="handRight">The GameObject that synchronizes the right hand transform</param>
        internal void SetNetworkRig(GameObject root, GameObject cam, GameObject handLeft, GameObject handRight)
        {
            _networkTransformRigAvatar    = root.GetComponent<NetworkTransform>();
            _networkTransformRigCamera    = cam.GetComponent<NetworkTransform>();
            _networkTransformRigHandLeft  = handLeft.GetComponent<NetworkTransform>();
            _networkTransformRigHandRight = handRight.GetComponent<NetworkTransform>();

            UxrAvatar avatar = GetComponentInParent<UxrAvatar>();

            _networkTransformRigAvatar.InterpolationTarget    = avatar.transform;
            _networkTransformRigCamera.InterpolationTarget    = avatar.CameraComponent.transform;
            _networkTransformRigHandLeft.InterpolationTarget  = avatar.GetHand(UxrHandSide.Left).Wrist;
            _networkTransformRigHandRight.InterpolationTarget = avatar.GetHand(UxrHandSide.Right).Wrist;

            _networkTransformRigAvatar.InterpolateErrorCorrection    = false;
            _networkTransformRigCamera.InterpolateErrorCorrection    = false;
            _networkTransformRigHandLeft.InterpolateErrorCorrection  = false;
            _networkTransformRigHandRight.InterpolateErrorCorrection = false;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        private void OnEnable()
        {
            UxrAvatar.GlobalAvatarMoved += Avatar_GlobalAvatarMoved;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        private void OnDisable()
        {
            UxrAvatar.GlobalAvatarMoved -= Avatar_GlobalAvatarMoved;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when an avatar moved.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void Avatar_GlobalAvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            if (Object.HasInputAuthority && ReferenceEquals(sender, UxrAvatar.LocalAvatar))
            {
                _actualAvatarPosition = e.NewPosition;
                _actualAvatarRotation = e.NewRotation;
            }
        }

        /// <summary>
        ///     Called when a component in UltimateXR had a state change.
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="eventArgs">Event parameters</param>
        private void UxrManager_ComponentStateChanged(UxrComponent component, UxrSyncEventArgs eventArgs)
        {
            if (!Object.HasInputAuthority)
            {
                return;
            }

            if (eventArgs.ShouldSyncNetworkEvent)
            {
                byte[] serializedEvent = eventArgs.SerializeEventBinary(component);

                if (serializedEvent != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Sending {serializedEvent.Length} bytes from {component.name} ({component.UniqueId}) {eventArgs}");
                    }

                    RPC_UxrComponentStateChanged(serializedEvent);
                }
            }
        }

        /// <summary>
        ///     RPC call to propagate state change events to all other clients.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_UxrComponentStateChanged(byte[] serializedEventData)
        {
            if (Object.HasInputAuthority)
            {
                return;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Receiving {serializedEventData.Length} bytes of data. Base64: {Convert.ToBase64String(serializedEventData)}");
            }

            UxrStateSyncResult result = UxrManager.Instance.ExecuteStateChange(serializedEventData, UxrConstants.Serialization.CurrentBinaryVersion);

            if (!result.IsError)
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
                {
                    Debug.Log($"{UxrConstants.NetworkingModule} Processed {serializedEventData.Length} bytes of data: {result}");
                }
            }
        }

        #endregion

        #region Private Types & Data

        private Vector3    _actualAvatarPosition;
        private Quaternion _actualAvatarRotation;
        private Vector3    _actualAvatarCameraPosition;
        private Quaternion _actualAvatarCameraRotation;
        private Vector3    _actualAvatarLeftHandPosition;
        private Quaternion _actualAvatarLeftHandRotation;
        private Vector3    _actualAvatarRightHandPosition;
        private Quaternion _actualAvatarRightHandRotation;

        private string _avatarName;

        #endregion
    }
#else
    public class UxrPhotonFusionAvatar : MonoBehaviour
    {
    }
#endif
}