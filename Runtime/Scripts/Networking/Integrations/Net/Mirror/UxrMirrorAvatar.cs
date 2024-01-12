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
#if ULTIMATEXR_USE_MIRROR_SDK
using UltimateXR.Core.Settings;
using Mirror;
#endif

namespace UltimateXR.Networking.Integrations.Net.Mirror
{
#if ULTIMATEXR_USE_MIRROR_SDK
    public class UxrMirrorAvatar : NetworkBehaviour, IUxrNetworkAvatar
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
        /// <param name="networkIdentity">The object to get authority over</param>
        public void RequestAuthority(NetworkIdentity networkIdentity)
        {
            CmdRequestAuthority(networkIdentity);
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
            if (!netIdentity.isOwned)
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
                    
                    CmdUxrComponentStateChanged(serializedEvent);
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        public override void OnStartClient()
        {
            Avatar = GetComponent<UxrAvatar>();

            InitializeNetworkAvatar(Avatar, netIdentity.isOwned, netIdentity.netId.ToString(), $"Player {netIdentity.netId} ({(netIdentity.isOwned ? "Local" : "External")})");

            if (netIdentity.isOwned)
            {
                UxrManager.ComponentStateChanged += UxrManager_ComponentStateChanged;
            }

            AvatarSpawned?.Invoke();
        }

        /// <inheritdoc />
        public override void OnStopClient()
        {
            if (Avatar && netIdentity.isOwned)
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
        /// <param name="networkIdentity">Object to get authority over</param>
        [Command]
        private void CmdRequestAuthority(NetworkIdentity networkIdentity)
        {
            networkIdentity.AssignClientAuthority(netIdentity.connectionToClient);
        }

        /// <summary>
        ///     Server RPC call to propagate state change events to all other clients.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [Command]
        private void CmdUxrComponentStateChanged(byte[] serializedEventData)
        {
            RpcUxrComponentStateChanged(serializedEventData);
        }

        /// <summary>
        ///     Client RPC call to execute a state change event. It will execute on all clients except the one that generated it,
        ///     which can be identified because it's the one with ownership.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [ClientRpc]
        private void RpcUxrComponentStateChanged(byte[] serializedEventData)
        {
            if (netIdentity.isOwned)
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
    public class UxrMirrorAvatar : MonoBehaviour
    {
    }
#endif
}