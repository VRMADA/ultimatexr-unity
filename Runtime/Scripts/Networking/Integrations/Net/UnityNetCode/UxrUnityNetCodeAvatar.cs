// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityNetCodeAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;
#if ULTIMATEXR_USE_UNITY_NETCODE
using UltimateXR.Core.Settings;
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
        private void UxrManager_ComponentStateChanged(UxrComponent component, UxrSyncEventArgs eventArgs)
        {
            if (!IsOwner)
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
                    
                    UxrComponentStateChangedServerRpc(serializedEvent);
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

            AvatarSpawned?.Invoke();
        }

        /// <inheritdoc />
        public override void OnNetworkDespawn()
        {
            if (Avatar && IsOwner)
            {
                UxrManager.ComponentStateChanged -= UxrManager_ComponentStateChanged;
            }

            AvatarDespawned?.Invoke();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Server RPC requesting authority over an object.
        /// </summary>
        /// <param name="networkObjectReference">Object to get authority over</param>
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
        ///     Server RPC call to propagate state change events to all other clients.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        /// <param name="serverRpcParams">RPC parameters</param>
        [ServerRpc]
        private void UxrComponentStateChangedServerRpc(byte[] serializedEventData)
        {
            UxrComponentStateChangedClientRpc(serializedEventData);
        }

        /// <summary>
        ///     Client RPC call to execute a state change event. It will execute on all clients except the one that generated it,
        ///     which can be identified because it's the one with ownership.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [ClientRpc]
        private void UxrComponentStateChangedClientRpc(byte[] serializedEventData)
        {
            if (IsOwner)
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
                    Debug.Log($"Processed {serializedEventData.Length} bytes of data: {result}");
                }
            }
        }

        #endregion

        #region Private Types & Data

        private string _avatarName;

        #endregion
    }
#else
    public class UxrPhotonFusionAvatar : MonoBehaviour
    {
    }
#endif
}