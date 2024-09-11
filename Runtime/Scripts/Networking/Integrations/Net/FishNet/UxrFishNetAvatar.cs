// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFishNetAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
#if ULTIMATEXR_USE_FISHNET_SDK
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSave;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Collections;
using FishNet.Connection;
using FishNet.Object;
using UltimateXR.Attributes;
using UltimateXR.Core.Instantiation;
using TransformExt = UltimateXR.Extensions.Unity.TransformExt;
#endif

namespace UltimateXR.Networking.Integrations.Net.FishNet
{
#if ULTIMATEXR_USE_FISHNET_SDK
    public class UxrFishNetAvatar : NetworkBehaviour, IUxrNetworkAvatar
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("List of objects that will be disabled when the avatar is in local mode, to avoid intersections with the camera for example")] [SerializeField] private List<GameObject> _localDisabledGameObjects;
        [SerializeField] [ReadOnly]                                                                                                                              private GameObject       _networkCamera;
        [SerializeField] [ReadOnly]                                                                                                                              private GameObject       _networkHandLeft;
        [SerializeField] [ReadOnly]                                                                                                                              private GameObject       _networkHandRight;

        #endregion

        #region Implicit IUxrNetworkAvatar

        /// <inheritdoc />
        public IList<GameObject> LocalDisabledGameObjects => _localDisabledGameObjects;

        /// <inheritdoc />
        public bool IsLocal { get; private set; }

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
            IsLocal           = isLocal;
            AvatarName        = avatarName;
            avatar.AvatarMode = isLocal ? UxrAvatarMode.Local : UxrAvatarMode.UpdateExternally;

            if (isLocal)
            {
                LocalDisabledGameObjects.ForEach(o => o.SetActive(false));
            }

            avatar.CombineUniqueId(uniqueId.GetGuid());
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sets up the dummy network transforms that hang from the avatar. These network transforms will be used
        ///     to propagate the transform data, since FishNet only can synchronize transforms in local space.
        /// </summary>
        /// <param name="networkCamera">Dummy GameObject that will synchronize the camera transform</param>
        /// <param name="networkHandLeft">Dummy GameObject that will synchronize the left hand</param>
        /// <param name="networkHandRight">Dummy GameObject that will synchronize the right hand</param>
        public void SetupDummyNetworkTransforms(GameObject networkCamera, GameObject networkHandLeft, GameObject networkHandRight)
        {
            _networkCamera    = networkCamera;
            _networkHandLeft  = networkHandLeft;
            _networkHandRight = networkHandRight;
        }

        /// <summary>
        ///     Request authority of the local avatar over an object.
        /// </summary>
        /// <param name="networkObject">The object to get authority over</param>
        public void RequestAuthority(NetworkObject networkObject)
        {
            RequestAuthorityServerRpc(networkObject);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        private void OnEnable()
        {
            UxrManager.StageUpdated += UxrManager_StageUpdated;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        private void OnDisable()
        {
            UxrManager.StageUpdated -= UxrManager_StageUpdated;
        }

        /// <summary>
        ///     Unity's Update() method. This will check for correct setup.
        /// </summary>
        private void Update()
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Errors)
            {
                if (_networkCamera == null || _networkHandLeft == null || _networkHandRight == null)
                {
                    Debug.LogError($"{UxrConstants.NetworkingModule} Avatar {this}'s prefab needs to be setup again using {nameof(UxrNetworkManager)} because dummy NetworkTransforms are missing. If the avatar is already registered, remove it and add it again.");
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when <see cref="UxrManager" /> finished an update stage during a frame. This will update the avatar if it's
        ///     a remote avatar.
        /// </summary>
        /// <param name="stage">The stage that finished updating</param>
        private void UxrManager_StageUpdated(UxrUpdateStage stage)
        {
            if (Avatar == null)
            {
                return;
            }

            if (stage == UxrUpdateStage.AvatarUsingTracking && !IsOwner)
            {
                // Copy from network transforms to avatar
                
                if (Avatar.CameraComponent && _networkCamera)
                {
                    TransformExt.SetPositionAndRotation(Avatar.CameraComponent.transform, _networkCamera.transform);
                }

                if (Avatar.LeftHandBone != null && _networkHandLeft != null)
                {
                    TransformExt.SetPositionAndRotation(Avatar.LeftHandBone, _networkHandLeft.transform);
                }

                if (Avatar.RightHandBone != null && _networkHandRight != null)
                {
                    TransformExt.SetPositionAndRotation(Avatar.RightHandBone, _networkHandRight.transform);
                }
            }
            else if (stage == UxrUpdateStage.PostProcess && IsOwner)
            {
                // Copy from avatar to network transforms. Do it at the last stage to make sure the hands are
                // In their right position after all manipulation logic has taken place.
                
                if (Avatar.CameraComponent && _networkCamera)
                {
                    TransformExt.SetPositionAndRotation(_networkCamera.transform, Avatar.CameraComponent.transform);
                }

                if (Avatar.LeftHandBone != null && _networkHandLeft != null)
                {
                    TransformExt.SetPositionAndRotation(_networkHandLeft.transform, Avatar.LeftHandBone);
                }

                if (Avatar.RightHandBone != null && _networkHandRight != null)
                {
                    TransformExt.SetPositionAndRotation(_networkHandRight.transform, Avatar.RightHandBone);
                }
            }
        }

        /// <summary>
        ///     Called when a component in UltimateXR had a state change.
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="eventArgs">Event parameters</param>
        private void UxrManager_ComponentStateChanged(IUxrStateSync component, UxrSyncEventArgs eventArgs)
        {
            if (!IsOwner)
            {
                return;
            }

            if (eventArgs.Options.HasFlag(UxrStateSyncOptions.Network))
            {
                byte[] serializedEvent = eventArgs.SerializeEventBinary(component);

                if (serializedEvent != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Sending {serializedEvent.Length} bytes from {component.Component.name} ({component.UniqueId}) {eventArgs}");
                    }

                    ComponentStateChangedServerRpc(serializedEvent);
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        public override void OnStartServer()
        {
            base.OnStartServer();

            if (!UxrNetworkManager.IsHost)
            {
                // Avoid calling in host mode, since OnStartClient() is already called. 
                StartNetworkAvatar();
            }
        }

        /// <inheritdoc />
        public override void OnStartClient()
        {
            base.OnStartClient();

            StartNetworkAvatar();
        }

        /// <inheritdoc />
        public override void OnStopServer()
        {
            base.OnStopServer();

            if (!UxrNetworkManager.IsHost)
            {
                // Avoid calling in host mode, since OnStopClient() is already called.
                StopNetworkAvatar();
            }
        }

        /// <inheritdoc />
        public override void OnStopClient()
        {
            base.OnStopClient();

            StopNetworkAvatar();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Finishes the network avatar initialization.
        /// </summary>
        private void StartNetworkAvatar()
        {
            Avatar = GetComponent<UxrAvatar>();

            InitializeNetworkAvatar(Avatar, IsOwner, OwnerId.ToString(), $"Player {OwnerId} ({(IsOwner ? "Local" : "External")})");

            if (IsOwner)
            {
                UxrManager.ComponentStateChanged += UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrFishNetAvatar)}.{nameof(StartNetworkAvatar)}: Is Local? {IsLocal}, Name: {AvatarName}. OwnerId: {OwnerId}, UniqueId: {Avatar.UniqueId}.");
            }

            AvatarSpawned?.Invoke();

            if (UxrInstanceManager.HasInstance)
            {
                UxrInstanceManager.Instance.NotifyNetworkSpawn(Avatar.gameObject);
            }

            if (IsOwner)
            {
                if (!UxrNetworkManager.IsServer)
                {
                    byte[] localAvatarState = UxrManager.Instance.SaveStateChanges(new List<GameObject> { Avatar.gameObject }, null, UxrStateSaveLevel.ChangesSinceBeginning, UxrGlobalSettings.Instance.NetFormatInitialState);

                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Requesting global state and sending local avatar state in {localAvatarState.Length} bytes.");
                    }

                    // Send the initial avatar state to the server and request the current scene state.  
                    // Call after AvatarSpawned() in case any event handler changes the avatar state.
                    NewAvatarJoinedServerRpc(localAvatarState);
                }
                else
                {
                    // Server creates the session and doesn't need to send the initial state.
                    s_initialStateLoaded = true;
                }
            }
        }

        /// <summary>
        ///     Stops a network avatar.
        /// </summary>
        private void StopNetworkAvatar()
        {
            if (Avatar && IsOwner)
            {
                UxrManager.ComponentStateChanged -= UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrFishNetAvatar)}.{nameof(StopNetworkAvatar)}: Is Local? {IsLocal}, Name: {AvatarName}");
            }

            if (UxrInstanceManager.HasInstance)
            {
                UxrInstanceManager.Instance.NotifyNetworkDespawn(Avatar.gameObject);
            }

            AvatarDespawned?.Invoke();
        }

        /// <summary>
        ///     Server RPC to request the current global state upon joining.
        /// </summary>
        /// <param name="avatarState">The initial state of the avatar that joined</param>
        /// <param name="conn">Filled by FishNet with info</param>
        [ServerRpc]
        private void NewAvatarJoinedServerRpc(byte[] avatarState, NetworkConnection conn = null)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Received request for global state from client {conn.ClientId}.");
            }

            // First load the avatar state
            UxrManager.Instance.LoadStateChanges(avatarState);

            // Now export the scenario state, except for the new avatar, and send it back
            byte[] serializedState = UxrManager.Instance.SaveStateChanges(null, new List<GameObject> { gameObject }, UxrStateSaveLevel.ChangesSinceBeginning, UxrGlobalSettings.Instance.NetFormatInitialState);

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Sending global state in {serializedState.Length} bytes to client {conn.ClientId}. Broadcasting {avatarState.Length} bytes to sync new avatar.");
            }

            // Send global state to new user.
            LoadGlobalStateTargetRpc(conn, serializedState);

            // Broadcast initial state of new avatar.
            LoadAvatarStateClientRpc(avatarState);
        }

        /// <summary>
        ///     Server RPC call to propagate state change events to all other clients.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [ServerRpc]
        private void ComponentStateChangedServerRpc(byte[] serializedEventData)
        {
            ComponentStateChangedClientRpc(serializedEventData);
        }

        /// <summary>
        ///     Server RPC requesting authority over an object.
        /// </summary>
        /// <param name="networkObject">Object to get authority over</param>
        /// <param name="conn">Filled by FishNet with info</param>
        [ServerRpc]
        private void RequestAuthorityServerRpc(NetworkObject networkObject, NetworkConnection conn = null)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Granting authority to owner {OwnerId} over network object {networkObject.name}.");
            }

            NetworkObject[] networkObjects = networkObject.GetComponentsInChildren<NetworkObject>();

            foreach (NetworkObject no in networkObjects)
            {
                no.GiveOwnership(conn);
            }
        }

        /// <summary>
        ///     Targeted client RPC to client that joined to sync to the current state.
        /// </summary>
        /// <param name="serializedStateData">The serialized state data</param>
        /// <param name="clientRpcParams">Target of the RPC</param>
        [TargetRpc]
        private void LoadGlobalStateTargetRpc(NetworkConnection conn, byte[] serializedStateData)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Receiving {serializedStateData.Length} bytes of global state data.");
            }

            UxrManager.Instance.LoadStateChanges(serializedStateData);
            s_initialStateLoaded = true;
        }

        /// <summary>
        ///     Client RPC to sync the state of a new avatar that joined.
        /// </summary>
        /// <param name="serializedStateData">The serialized state data</param>
        [ObserversRpc]
        private void LoadAvatarStateClientRpc(byte[] serializedStateData)
        {
            if (IsOwner)
            {
                // Don't execute on the source of the event, we don't want to load our own avatar data.
                return;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Receiving {serializedStateData.Length} bytes of avatar state data.");
            }

            UxrManager.Instance.LoadStateChanges(serializedStateData);
        }

        /// <summary>
        ///     Client RPC call to execute a state change event. It will execute on all clients except the one that generated it,
        ///     which can be identified because it's the one with ownership.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [ObserversRpc]
        private void ComponentStateChangedClientRpc(byte[] serializedEventData)
        {
            if (IsOwner)
            {
                // Don't execute on the source of the event.
                return;
            }

            if (s_initialStateLoaded == false)
            {
                // Ignore sync events until the initial state is sent, to make sure the syncs are only processed after the initial state.
                return;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Receiving {serializedEventData.Length} bytes of data. Base64: {Convert.ToBase64String(serializedEventData)}");
            }

            UxrStateSyncResult result = UxrManager.Instance.ExecuteStateSyncEvent(serializedEventData);
        }

        #endregion

        #region Private Types & Data

        private static bool s_initialStateLoaded;

        private string _avatarName;

        #endregion
    }
#else
    public class UxrFishNetAvatar : MonoBehaviour
    {
    }
#endif
}