// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPhotonFusionNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
#if ULTIMATEXR_USE_PHOTONFUSION_SDK && UNITY_EDITOR
using UnityEditor;
#endif
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UltimateXR.Core.Settings;
using UltimateXR.Core.Threading.TaskControllers;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using Behaviour = UnityEngine.Behaviour;
#endif

namespace UltimateXR.Networking.Integrations.Net.PhotonFusion
{
    /// <summary>
    ///     Implementation of networking support using Photon Fusion.
    /// </summary>
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
    public partial class UxrPhotonFusionNetwork : UxrNetworkImplementation, INetworkRunnerCallbacks
#else
    public class UxrPhotonFusionNetwork : UxrNetworkImplementation
#endif
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("Show a UI during play mode with connection options to quickly prototype networking functionality")] [SerializeField] private bool _usePrototypingUI = true;

        #endregion

        #region Public Overrides UxrNetworkImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkPhotonFusion;

        /// <inheritdoc />
        public override bool IsServer
        {
            get
            {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
                return _networkRunner != null && _networkRunner.IsRunning && _networkRunner.IsServer;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override bool IsClient
        {
            get
            {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
                return _networkRunner != null && _networkRunner.IsRunning && _networkRunner.IsClient;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override UxrNetworkCapabilities Capabilities => UxrNetworkCapabilities.NetworkTransform | UxrNetworkCapabilities.NetworkRigidbody;

        /// <inheritdoc />
        public override string NetworkRigidbodyWarning => "Photon Fusion's NetworkRigidbody components are meant to be used in Client/Server mode. If you plan to use Photon Fusion in Shared mode, do not set up NetworkRigidbody components here. Don't worry! UltimateXR will still synchronize grabbable physics-driven rigidbodies using RPC calls to try to keep the same position/velocity on all users.";

        /// <inheritdoc />
        public override void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_PHOTONFUSION_SDK && UNITY_EDITOR
            Component newComponent = networkManager.GetComponent<NetworkRunner>();

            if (newComponent == null)
            {
                newComponent = Undo.AddComponent<NetworkRunner>(networkManager.gameObject);
                Undo.RegisterFullObjectHierarchyUndo(networkManager.gameObject, "Setup Photon Component");
            }

            newComponents.Add(newComponent);

#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

            if (avatar == null)
            {
            }

#if ULTIMATEXR_USE_PHOTONFUSION_SDK && UNITY_EDITOR
            UxrPhotonFusionAvatar fusionAvatar = avatar.GetOrAddComponent<UxrPhotonFusionAvatar>();
            newComponents.Add(fusionAvatar);

            GameObject networkRig = new GameObject("PhotonNetworkRig");
            Undo.RegisterCreatedObjectUndo(networkRig, "Create avatar Photon network rig");
            Undo.SetTransformParent(networkRig.transform, avatar.transform, "Parent network rig");
            networkRig.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject networkCamera = new GameObject("NetworkCamera");
            Undo.RegisterCreatedObjectUndo(networkCamera, "Create avatar network camera");
            Undo.SetTransformParent(networkCamera.transform, networkRig.transform, "Parent network camera");
            networkCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject networkHandLeft = new GameObject("NetworkHandLeft");
            Undo.RegisterCreatedObjectUndo(networkHandLeft, "Create avatar network hand left");
            Undo.SetTransformParent(networkHandLeft.transform, networkRig.transform, "Parent network hand left");
            networkHandLeft.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject networkHandRight = new GameObject("NetworkHandRight");
            Undo.RegisterCreatedObjectUndo(networkHandRight, "Create avatar network hand right");
            Undo.SetTransformParent(networkHandRight.transform, networkRig.transform, "Parent network hand right");
            networkHandRight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            NetworkObject avatarNetworkObject = avatar.gameObject.GetOrAddComponent<NetworkObject>();

            if (avatarNetworkObject)
            {
                avatarNetworkObject.AssignSerializedProperty("DestroyWhenStateAuthorityLeaves", p => p.boolValue = true);
            }

            IEnumerable<Behaviour> rigComponents       = SetupNetworkTransform(networkRig,       true, UxrNetworkTransformFlags.ChildAll);
            IEnumerable<Behaviour> cameraComponents    = SetupNetworkTransform(networkCamera,    true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> leftHandComponents  = SetupNetworkTransform(networkHandLeft,  true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> rightHandComponents = SetupNetworkTransform(networkHandRight, true, UxrNetworkTransformFlags.ChildPositionAndRotation);

            newComponents.AddRange(new[] { avatarNetworkObject }.Concat(rigComponents).Concat(cameraComponents).Concat(leftHandComponents).Concat(rightHandComponents));
            newGameObjects.AddRange(new[] { networkHandLeft, networkHandRight, networkCamera, networkRig });

            fusionAvatar.SetNetworkRig(networkRig, networkCamera, networkHandLeft, networkHandRight);

            Undo.RegisterFullObjectHierarchyUndo(avatar.gameObject, "Setup Photon Avatar");
#endif
        }

        /// <inheritdoc />
        public override void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs)
        {
            
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK && UNITY_EDITOR
            if (networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ChildTransform) == false)
            {
                NetworkObject networkObject = gameObject.GetOrAddComponent<NetworkObject>();
                yield return networkObject;
            }

            NetworkTransform networkTransform = gameObject.GetOrAddComponent<NetworkTransform>();
            networkTransform.InterpolationSpace = worldSpace ? Spaces.World : Spaces.Local;
            yield return networkTransform;
#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlagsFlags)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK && UNITY_EDITOR
            NetworkObject    networkObject    = gameObject.GetOrAddComponent<NetworkObject>();
            NetworkRigidbody networkRigidbody = gameObject.GetOrAddComponent<NetworkRigidbody>();

            networkRigidbody.InterpolationSpace = worldSpace ? Spaces.World : Spaces.Local;

            yield return networkObject;
            yield return networkRigidbody;
#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkTransform(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            NetworkTransform[] networkTransforms = gameObject.GetComponentsInChildren<NetworkTransform>();
            networkTransforms.ForEach(nt => nt.SetEnabled(enable));
#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkRigidbody(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            NetworkRigidbody[] networkRigidbodies = gameObject.GetComponentsInChildren<NetworkRigidbody>();
            networkRigidbodies.ForEach(nrb => nrb.SetEnabled(enable));
#endif
        }

        /// <inheritdoc />
        public override bool HasAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            if (_networkRunner == null)
            {
                return false;
            }

            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return false;
            }

            return networkObject.HasStateAuthority;
#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override void RequestAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            if (_networkRunner && _networkRunner.GameMode == GameMode.Shared)
            {
                NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

                if (networkObject)
                {
                    networkObject.RequestStateAuthority();
                }
            }
#endif
        }

        /// <inheritdoc />
        public override void CheckReassignGrabAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            UxrGrabbableObject grabbableObject = gameObject.GetComponent<UxrGrabbableObject>();
            NetworkObject      networkObject   = gameObject.GetComponent<NetworkObject>();

            if (networkObject != null && grabbableObject != null)
            {
                UxrAvatar avatarAuthority = UxrAvatar.EnabledComponents.FirstOrDefault(a => a.GetComponent<NetworkObject>() != null && a.GetComponent<NetworkObject>().StateAuthority == networkObject.StateAuthority);

                if (avatarAuthority == null || !UxrGrabManager.Instance.IsBeingGrabbedBy(grabbableObject, avatarAuthority))
                {
                    // No avatar has authority or the avatar that grabbed it doesn't have it anymore. Change authority to first one.

                    UxrAvatar firstAvatar = UxrGrabManager.Instance.GetGrabbingHands(grabbableObject).First().Avatar;

                    if (firstAvatar == UxrAvatar.LocalAvatar)
                    {
                        UxrNetworkManager.Instance.RequestAuthority(gameObject);
                    }
                }
            }
#endif
        }

        /// <inheritdoc />
        public override bool HasNetworkTransformSyncComponents(GameObject gameObject)
        {
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
            return gameObject.GetComponent<NetworkTransform>() != null || gameObject.GetComponent<NetworkRigidbody>() != null;
#else
            return false;
#endif
        }

        #endregion

#if ULTIMATEXR_USE_PHOTONFUSION_SDK

        #region INetworkRunnerCallbacks

        /// <inheritdoc />
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnPlayerJoined)} PlayerId = {player.PlayerId}");
            }

            if (!_usePrototypingUI)
            {
                return;
            }

            if (_gameMode == GameMode.Single || _gameMode == GameMode.Server || _gameMode == GameMode.Host || (_gameMode == GameMode.AutoHostOrClient && _networkRunner.IsServer))
            {
                SpawnPlayer(runner, player);
            }

            if (_gameMode == GameMode.Shared && player == _networkRunner.LocalPlayer)
            {
                SpawnPlayer(runner, player);
            }
        }

        /// <inheritdoc />
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnPlayerLeft)} PlayerId = {player.PlayerId}");
            }

            if (!_usePrototypingUI)
            {
                return;
            }

            if (_gameMode == GameMode.Single || _gameMode == GameMode.Server || _gameMode == GameMode.Host)
            {
                TryDespawnPlayer(runner, player);
            }

            if (_gameMode == GameMode.Shared && player == _networkRunner.LocalPlayer)
            {
                // Avatar has "destroy when state authority leaves" enabled. 
            }
        }

        /// <inheritdoc />
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (UxrAvatar.LocalAvatar == null)
            {
                return;
            }

            if (!_usePrototypingUI)
            {
                return;
            }

            RigInput rigInput = new RigInput();
            rigInput.avatarPosition    = UxrAvatar.LocalAvatar.transform.position;
            rigInput.avatarRotation    = UxrAvatar.LocalAvatar.transform.rotation;
            rigInput.avatarScale       = UxrAvatar.LocalAvatar.transform.localScale;
            rigInput.cameraPosition    = UxrAvatar.LocalAvatar.CameraComponent.transform.position;
            rigInput.cameraRotation    = UxrAvatar.LocalAvatar.CameraComponent.transform.rotation;
            rigInput.leftHandPosition  = UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Left).transform.position;
            rigInput.leftHandRotation  = UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Left).transform.rotation;
            rigInput.rightHandPosition = UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Right).transform.position;
            rigInput.rightHandRotation = UxrAvatar.LocalAvatar.GetHandBone(UxrHandSide.Right).transform.rotation;
            input.Set(rigInput);
        }

        /// <inheritdoc />
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        /// <inheritdoc />
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
            {
                Debug.LogWarning($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnShutdown)} Reason: {shutdownReason}");
            }

            _spawnedAvatars.Clear();
        }

        /// <inheritdoc />
        public void OnConnectedToServer(NetworkRunner runner)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnConnectedToServer)}");
            }
        }

        /// <inheritdoc />
        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnDisconnectedFromServer)}");
            }
        }

        /// <inheritdoc />
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnConnectRequest)} from {request.RemoteAddress}");
            }
        }

        /// <inheritdoc />
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
            {
                Debug.LogWarning($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnConnectFailed)} Reason: {reason}");
            }
        }

        /// <inheritdoc />
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        /// <inheritdoc />
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        /// <inheritdoc />
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        /// <inheritdoc />
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        /// <inheritdoc />
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        /// <inheritdoc />
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(OnSceneLoadDone)}");
            }
        }

        /// <inheritdoc />
        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        #endregion
        
        #region Unity

        /// <summary>
        ///     Gets the network runner.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (!enabled)
            {
                return;
            }

            _networkRunner = gameObject.GetComponent<NetworkRunner>();

            if (_networkRunner == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.NetworkingModule} Can't get network runner. Is Photon selected in the {nameof(UxrNetworkManager)}?");
                }
            }
        }

        /// <summary>
        ///     Shows the connection UI if its enabled.
        /// </summary>
        private void OnGUI()
        {
            if (!_usePrototypingUI)
            {
                return;
            }

            if (_networkRunner != null && _networkRunner.IsRunning)
            {
                return;
            }

            int labelHeight  = 25;
            int buttonWidth  = 200;
            int buttonHeight = 40;
            int posY         = 0;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), string.Empty);

            GUI.Box(new Rect(0, posY, buttonWidth, buttonHeight), "UltimateXR Photon Fusion");
            posY += buttonHeight;

            if (_networkRunner != null && _networkRunner.IsStarting)
            {
                GUI.Box(new Rect(0, posY, buttonWidth, labelHeight), "Starting network");
                return;
            }

            GUI.Box(new Rect(0, posY, buttonWidth, labelHeight), "Select Room Name:");
            posY += labelHeight;

            _roomName =  GUI.TextField(new Rect(0, posY, buttonWidth, labelHeight), _roomName);
            posY      += buttonHeight;

            if (GUI.Button(new Rect(0, posY, buttonWidth, buttonHeight), "No Multiplayer"))
            {
                new UxrTaskController(ct => StartPrototypeSession(GameMode.Single), true);
            }

            posY += buttonHeight;

            if (GUI.Button(new Rect(0, posY, buttonWidth, buttonHeight), "Start Host"))
            {
                new UxrTaskController(ct => StartPrototypeSession(GameMode.Host), true);
            }

            posY += buttonHeight;

            if (GUI.Button(new Rect(0, posY, buttonWidth, buttonHeight), "Start Client"))
            {
                new UxrTaskController(ct => StartPrototypeSession(GameMode.Client), true);
            }

            posY += buttonHeight;

            if (GUI.Button(new Rect(0, posY, buttonWidth, buttonHeight), "Auto Host/Client"))
            {
                new UxrTaskController(ct => StartPrototypeSession(GameMode.AutoHostOrClient), true);
            }

            posY += buttonHeight;

            if (GUI.Button(new Rect(0, posY, buttonWidth, buttonHeight), "Start Shared"))
            {
                new UxrTaskController(ct => StartPrototypeSession(GameMode.Shared), true);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Starts a multi-user session for prototyping.
        /// </summary>
        /// <param name="mode">The game mode</param>
        private async Task StartPrototypeSession(GameMode mode)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(StartPrototypeSession)} in mode {mode}");
            }

            _gameMode                   = mode;
            _networkRunner.ProvideInput = true;

            INetworkSceneManager networkSceneManager = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(NetworkSceneManagerDefault);

                if (type != null)
                {
                    networkSceneManager = gameObject.AddComponent(type) as INetworkSceneManager;
                    break;
                }
            }

            await _networkRunner.StartGame(new StartGameArgs
                                           {
                                                       GameMode     = mode,
                                                       SessionName  = _roomName,
                                                       Scene        = SceneManager.GetActiveScene().buildIndex,
                                                       SceneManager = networkSceneManager
                                           });
        }

        /// <summary>
        ///     Spawns a player's avatar.
        /// </summary>
        /// <param name="runner">The network runner</param>
        /// <param name="player">The player to spawn the avatar for</param>
        private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            UxrAvatar firstAvatarPrefab = UxrNetworkManager.Instance.RegisteredAvatarPrefabs.FirstOrDefault();

            if (firstAvatarPrefab == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.NetworkingModule} Can't spawn avatar prefab. Register avatars in {nameof(UxrNetworkManager)} first.");
                }
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                {
                    Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(SpawnPlayer)} Spawning player for PlayerId = {player.PlayerId} in {_gameMode} mode");
                }

                NetworkObject playerObject = runner.Spawn(firstAvatarPrefab.gameObject, Vector3.zero, Quaternion.identity, player);
                _spawnedAvatars.Add(player, playerObject);
            }
        }

        /// <summary>
        ///     Tries to despawn a player.
        /// </summary>
        /// <param name="runner">Network runner</param>
        /// <param name="player">The player to despawn</param>
        private void TryDespawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedAvatars.TryGetValue(player, out NetworkObject networkObject))
            {
                if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                {
                    Debug.Log($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonFusionNetwork)}.{nameof(TryDespawnPlayer)} Despawning player for PlayerId = {player.PlayerId} in {_gameMode} mode");
                }

                runner.Despawn(networkObject);
                _spawnedAvatars.Remove(player);
            }
        }

        #endregion

        #region Private Data

        private const string NetworkSceneManagerDefault = "Fusion.NetworkSceneManagerDefault";

        private          GameMode                             _gameMode = GameMode.Single;
        private          NetworkRunner                        _networkRunner;
        private          string                               _roomName       = "TestRoom";
        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedAvatars = new Dictionary<PlayerRef, NetworkObject>();

        #endregion

#endif
    }
}