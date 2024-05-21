// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMirrorAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
#if ULTIMATEXR_USE_MIRROR_SDK
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
        private void UxrManager_ComponentStateChanged(IUxrStateSync component, UxrSyncEventArgs eventArgs)
        {
            if (!netIdentity.isOwned)
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

                    CmdComponentStateChanged(serializedEvent);
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        public override void OnStartClient()
        {
            Avatar = GetComponent<UxrAvatar>();

            InitializeNetworkAvatar(Avatar, netIdentity.isOwned, netId.ToString(), $"Player {netId} ({(netIdentity.isOwned ? "Local" : "External")})");

            if (netIdentity.isOwned)
            {
                UxrManager.ComponentStateChanged += UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrMirrorAvatar)}.{nameof(OnStartClient)}: Is Local? {IsLocal}, Name: {AvatarName}. NetId: {netId}, UniqueId: {Avatar.UniqueId}.");
            }

            AvatarSpawned?.Invoke();

            if (UxrInstanceManager.HasInstance)
            {
                UxrInstanceManager.Instance.NotifyNetworkSpawn(Avatar.gameObject);
            }

            if (netIdentity.isOwned)
            {
                if (!netIdentity.isServer)
                {
                    byte[] localAvatarState = UxrManager.Instance.SaveStateChanges(new List<GameObject> { Avatar.gameObject }, null, UxrStateSaveLevel.ChangesSinceBeginning, UxrGlobalSettings.Instance.NetFormatInitialState);

                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Requesting global state and sending local avatar state in {localAvatarState.Length} bytes.");
                    }

                    // Send the initial avatar state to the server and request the current scene state.  
                    // Call after AvatarSpawned() in case any event handler changes the avatar state.
                    CmdNewAvatarJoined(localAvatarState);
                }
                else
                {
                    // Server creates the session and doesn't need to send the initial state.
                    s_initialStateLoaded = true;
                }
            }
        }

        /// <inheritdoc />
        public override void OnStopClient()
        {
            if (Avatar && netIdentity.isOwned)
            {
                UxrManager.ComponentStateChanged -= UxrManager_ComponentStateChanged;
            }

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrMirrorAvatar)}.{nameof(OnStopClient)}: Is Local? {IsLocal}, Name: {AvatarName}");
            }

            AvatarDespawned?.Invoke();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Server RPC to request the current global state upon joining.
        /// </summary>
        /// <param name="avatarState">The initial state of the avatar that joined</param>
        /// <param name="sender">Information filled by Mirror with information about the sender</param>
        [Command]
        private void CmdNewAvatarJoined(byte[] avatarState, NetworkConnectionToClient sender = null)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Received request for global state from client {sender.identity.netId.ToString()}.");
            }

            // First load the avatar state
            UxrManager.Instance.LoadStateChanges(avatarState);

            // Now export the scenario state, except for the new avatar, and send it back
            byte[] serializedState = UxrManager.Instance.SaveStateChanges(null, new List<GameObject> { gameObject }, UxrStateSaveLevel.ChangesSinceBeginning, UxrGlobalSettings.Instance.NetFormatInitialState);

            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Verbose)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Sending global state in {serializedState.Length} bytes to client {sender.identity.netId.ToString()}. Broadcasting {avatarState.Length} bytes to sync new avatar.");
            }

            // Send global state to new user.
            TargetLoadGlobalState(sender, serializedState);

            // Broadcast initial state of new avatar.
            RpcLoadAvatarState(avatarState);
        }

        /// <summary>
        ///     Server RPC to propagate state change events to all other clients.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [Command]
        private void CmdComponentStateChanged(byte[] serializedEventData)
        {
            RpcComponentStateChanged(serializedEventData);
        }

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
        ///     Targeted client RPC to client that joined to sync to the current state.
        /// </summary>
        /// <param name="target">Target of the RPC</param>
        /// <param name="serializedStateData">The serialized state data</param>
        [TargetRpc]
        private void TargetLoadGlobalState(NetworkConnectionToClient target, byte[] serializedStateData)
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
        private void RpcLoadAvatarState(byte[] serializedStateData)
        {
            if (netIdentity.isOwned)
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
        ///     Client RPC to execute a state change event. It will execute on all clients except the one that generated it,
        ///     which can be identified because it's the one with ownership.
        /// </summary>
        /// <param name="serializedEventData">The serialized state change data</param>
        [ClientRpc]
        private void RpcComponentStateChanged(byte[] serializedEventData)
        {
            if (netIdentity.isOwned)
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

            UxrManager.Instance.ExecuteStateSyncEvent(serializedEventData);
        }

        #endregion

        #region Private Types & Data

        private static bool s_initialStateLoaded;

        private string _avatarName;

        #endregion
    }
#else
    public class UxrMirrorAvatar : MonoBehaviour
    {
    }
#endif
}