// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityNetCodeAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
#if ULTIMATEXR_USE_UNITY_NETCODE
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSave;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Core.Instantiation;
using Unity.Netcode;
#endif

namespace UltimateXR.Networking.Integrations.Net.UnityNetCode
{
#if ULTIMATEXR_USE_UNITY_NETCODE
    public class UxrUnityNetCodeAvatar : NetworkBehaviour, IUxrNetworkAvatar
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("List of objects that will be disabled when the avatar is in local mode, to avoid intersections with the camera for example")] [SerializeField] private List<GameObject> _localDisabledGameObjects;

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

            avatar.CombineUniqueId(uniqueId.GetGuid(), true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Request authority of the local avatar over an object.
        /// </summary>
        /// <param name="networkObject">The object to get authority over</param>
        public void RequestAuthority(NetworkObject networkObject)
        {
            RequestAuthorityServerRpc(networkObject);
        }

        #endregion

        #region Event Handling Methods

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
        public override void OnNetworkSpawn()
        {
            Avatar = GetComponent<UxrAvatar>();

            InitializeNetworkAvatar(Avatar, IsOwner, OwnerClientId.ToString(), $"Player {OwnerClientId} ({(IsOwner ? "Local" : "External")})");

            if (IsOwner)
            {
                UxrManager.ComponentStateChanged += UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrUnityNetCodeAvatar)}.{nameof(OnNetworkSpawn)}: Is Local? {IsLocal}, Name: {AvatarName}. OwnerClientId: {OwnerClientId}, UniqueId: {Avatar.UniqueId}.");
            }

            AvatarSpawned?.Invoke();

            if (UxrInstanceManager.HasInstance)
            {
                UxrInstanceManager.Instance.NotifyNetworkSpawn(Avatar.gameObject);
            }

            if (IsOwner)
            {
                if (!IsServer)
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

        /// <inheritdoc />
        public override void OnNetworkDespawn()
        {
            if (Avatar && IsOwner)
            {
                UxrManager.ComponentStateChanged -= UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrUnityNetCodeAvatar)}.{nameof(OnNetworkDespawn)}: Is Local? {IsLocal}, Name: {AvatarName}");
            }

            AvatarDespawned?.Invoke();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Server RPC to request the current global state upon joining.
        /// </summary>
        /// <param name="avatarState">The initial state of the avatar that joined</param>
        /// <param name="serverRpcParams">Filled by NetCode with info</param>
        [ServerRpc]
        private void NewAvatarJoinedServerRpc(byte[] avatarState, ServerRpcParams serverRpcParams = default)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Received request for global state from client {serverRpcParams.Receive.SenderClientId}.");
            }

            // First load the avatar state
            UxrManager.Instance.LoadStateChanges(avatarState);

            // Now export the scenario state, except for the new avatar, and send it back
            byte[] serializedState = UxrManager.Instance.SaveStateChanges(null, new List<GameObject> { gameObject }, UxrStateSaveLevel.ChangesSinceBeginning, UxrGlobalSettings.Instance.NetFormatInitialState);

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Sending global state in {serializedState.Length} bytes to client {serverRpcParams.Receive.SenderClientId}. Broadcasting {avatarState.Length} bytes to sync new avatar.");
            }

            // Send global state to new user.

            ClientRpcParams clientRpcParams = new ClientRpcParams
                                              {
                                                          Send = new ClientRpcSendParams
                                                                 {
                                                                             TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                                                                 }
                                              };
            LoadGlobalStateClientRpc(serializedState, clientRpcParams);

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
        /// <param name="networkObjectReference">Object to get authority over</param>
        /// <param name="serverRpcParams">Filled by NetCode with info</param>
        [ServerRpc]
        private void RequestAuthorityServerRpc(NetworkObjectReference networkObjectReference, ServerRpcParams serverRpcParams = default)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject))
            {
                NetworkManager networkManager = UxrNetworkManager.Instance.GetComponent<NetworkManager>();

                if (networkManager != null)
                {
                    networkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);
                }
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                {
                    Debug.LogWarning($"{UxrConstants.NetworkingModule} {nameof(UxrUnityNetCodeAvatar)}.{nameof(RequestAuthorityServerRpc)}() Cannot find target network object.");
                }
            }
        }

        /// <summary>
        ///     Targeted client RPC to client that joined to sync to the current state.
        /// </summary>
        /// <param name="serializedStateData">The serialized state data</param>
        /// <param name="clientRpcParams">Target of the RPC</param>
        [ClientRpc]
        private void LoadGlobalStateClientRpc(byte[] serializedStateData, ClientRpcParams clientRpcParams = default)
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
        [ClientRpc]
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
        [ClientRpc]
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
    public class UxrPhotonFusionAvatar : MonoBehaviour
    {
    }
#endif
}