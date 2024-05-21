// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityNetCodeNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
using UnityEditor;
#endif

#if ULTIMATEXR_USE_UNITY_NETCODE
using System.Linq;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using NetworkObject    = Unity.Netcode.NetworkObject;
using NetworkRigidbody = Unity.Netcode.Components.NetworkRigidbody;
using NetworkTransform = Unity.Netcode.Components.NetworkTransform;
#endif

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Networking.Integrations.Net.UnityNetCode
{
    /// <summary>
    ///     Implementation of networking support using Unity NetCode.
    /// </summary>
    public class UxrUnityNetCodeNetwork : UxrNetworkImplementation
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("Show a UI during play mode with connection options to quickly prototype networking functionality")] [SerializeField] private bool _usePrototypingUI = true;

        #endregion

        #region Public Overrides UxrNetworkImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkUnityNetCode;

        /// <inheritdoc />
        public override bool IsServer
        {
            get
            {
#if ULTIMATEXR_USE_UNITY_NETCODE
                return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
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
#if ULTIMATEXR_USE_UNITY_NETCODE
                return NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override UxrNetworkCapabilities Capabilities => UxrNetworkCapabilities.NetworkTransform; // The following is momentarily disabled until we get a workaround for not allowing re-parenting during startup | UxrNetworkCapabilities.NetworkRigidbody;

        /// <inheritdoc />
        public override string NetworkRigidbodyWarning => $"{UxrConstants.SdkUnityNetCode} does not allow re-parenting NetworkIdentity GameObjects during startup. Until there is a workaround, NetworkRigidbody support here will be disabled. Don't worry! UltimateXR will still synchronize grabbable physics-driven rigidbodies using RPC calls to try to keep the same position/velocity on all users.";

        /// <inheritdoc />
        public override void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
            GameObject networkManagerGo = new GameObject("NetCodeNetworkManager");
            Undo.RegisterCreatedObjectUndo(networkManagerGo, "Create NetCode Network Manager");
            networkManagerGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            networkManagerGo.transform.SetSiblingIndex(networkManager.transform.GetSiblingIndex() + 1);

            NetworkManager netCodeNetworkManager = networkManagerGo.GetOrAddComponent<NetworkManager>();
            UnityTransport unityTransport        = networkManagerGo.GetOrAddComponent<UnityTransport>();
            netCodeNetworkManager.NetworkConfig.NetworkTransport = unityTransport;
            Undo.RegisterFullObjectHierarchyUndo(networkManager.gameObject, "Setup NetCode NetworkManager");

            newComponents.Add(unityTransport);
            newComponents.Add(netCodeNetworkManager);
            newGameObjects.Add(networkManagerGo);

#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
            if (avatar == null)
            {
                return;
            }

            UxrUnityNetCodeAvatar netCodeAvatar = avatar.GetOrAddComponent<UxrUnityNetCodeAvatar>();
            newComponents.Add(netCodeAvatar);

            IEnumerable<Behaviour> avatarComponents    = SetupClientNetworkTransform(avatar.gameObject,                                  true, UxrNetworkTransformFlags.All);
            IEnumerable<Behaviour> cameraComponents    = SetupClientNetworkTransform(avatar.CameraComponent.gameObject,                  true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> leftHandComponents  = SetupClientNetworkTransform(avatar.GetHand(UxrHandSide.Left).Wrist.gameObject,  true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> rightHandComponents = SetupClientNetworkTransform(avatar.GetHand(UxrHandSide.Right).Wrist.gameObject, true, UxrNetworkTransformFlags.ChildPositionAndRotation);

            newComponents.AddRange(avatarComponents.ToList().Concat(cameraComponents).Concat(leftHandComponents).Concat(rightHandComponents));
            Undo.RegisterFullObjectHierarchyUndo(avatar.gameObject, "Setup NetCode Avatar");
#endif
        }

        /// <inheritdoc />
        public override void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
            NetworkManager netCodeNetworkManager = FindObjectOfType<NetworkManager>();

            if (netCodeNetworkManager != null && netCodeNetworkManager.NetworkConfig.PlayerPrefab == null && avatarPrefabs.Any())
            {
                netCodeNetworkManager.NetworkConfig.PlayerPrefab = avatarPrefabs.First().gameObject;
                Undo.RegisterCompleteObjectUndo(netCodeNetworkManager, "Setup NetCode Avatar");
            }
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
            if (networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ChildTransform) == false)
            {
                NetworkObject networkObject = gameObject.GetOrAddComponent<NetworkObject>();
                yield return networkObject;
            }

            NetworkTransform networkTransform = gameObject.GetOrAddComponent<NetworkTransform>();
            networkTransform.InLocalSpace  = !worldSpace;
            networkTransform.SyncPositionX = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionX);
            networkTransform.SyncPositionY = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionY);
            networkTransform.SyncPositionZ = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionZ);
            networkTransform.SyncRotAngleX = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationX);
            networkTransform.SyncRotAngleY = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationY);
            networkTransform.SyncRotAngleZ = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationZ);
            networkTransform.SyncScaleX    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleX);
            networkTransform.SyncScaleY    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleY);
            networkTransform.SyncScaleZ    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleZ);
            yield return networkTransform;

#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlags)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE && UNITY_EDITOR
            // Building list forces evaluation of AddNetworkTransform IEnumerable and creates the components
            List<Behaviour> networkTransformComponents = new List<Behaviour>(AddNetworkTransform(gameObject, worldSpace, UxrNetworkTransformFlags.All));

            NetworkRigidbody networkRigidbody = gameObject.GetOrAddComponent<NetworkRigidbody>();
            yield return networkRigidbody;

            // Return transform components after, so that when removing the components the NetworkRigidbody is removed before the identity. Otherwise Mirror will complain.

            foreach (Behaviour newBehaviour in networkTransformComponents)
            {
                yield return newBehaviour;
            }

#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkTransform(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE
            NetworkTransform[] networkTransforms = gameObject.GetComponentsInChildren<NetworkTransform>();
            networkTransforms.ForEach(nt => nt.SetEnabled(enable));

#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkRigidbody(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE
            EnableNetworkTransform(gameObject, enabled);

            NetworkRigidbody[] networkRigidbodies = gameObject.GetComponentsInChildren<NetworkRigidbody>();
            networkRigidbodies.ForEach(nrb => nrb.SetEnabled(enable));
#endif
        }

        /// <inheritdoc />
        public override bool HasAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return false;
            }

            return networkObject.IsOwner;
#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override void RequestAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE
            if (gameObject == null)
            {
                return;
            }

            UxrUnityNetCodeAvatar netCodeAvatar = UxrAvatar.LocalAvatar.GetComponentInChildren<UxrUnityNetCodeAvatar>();
            NetworkObject         networkObject = gameObject.GetComponent<NetworkObject>();

            if (netCodeAvatar != null && networkObject != null)
            {
                netCodeAvatar.RequestAuthority(networkObject);
            }
#endif
        }

        /// <inheritdoc />
        public override void CheckReassignGrabAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_UNITY_NETCODE
            UxrGrabbableObject grabbableObject = gameObject.GetComponent<UxrGrabbableObject>();
            NetworkObject      networkObject   = gameObject.GetComponent<NetworkObject>();

            if (networkObject != null && grabbableObject != null)
            {
                UxrAvatar avatarAuthority = UxrAvatar.EnabledComponents.FirstOrDefault(a => a.GetComponent<NetworkObject>() != null && a.GetComponent<NetworkObject>().OwnerClientId == networkObject.OwnerClientId);

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
#if ULTIMATEXR_USE_UNITY_NETCODE
            return gameObject.GetComponent<NetworkTransform>() != null || gameObject.GetComponent<NetworkRigidbody>() != null;
#else
            return false;
#endif
        }

        #endregion

#if ULTIMATEXR_USE_UNITY_NETCODE

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport unityTransport)
            {
                _networkAddress = unityTransport.ConnectionData.Address;
                _networkPort    = unityTransport.ConnectionData.Port.ToString();
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

            if (NetworkManager.Singleton == null)
            {
                return;
            }

            PosY = 0;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), string.Empty);

            GUI.Box(new Rect(0, PosY, ButtonWidth, ButtonHeight), "UltimateXR Unity NetCode");
            PosY += ButtonHeight;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton.ShutdownInProgress)
                {
                    return;
                }

                if (NetworkManager.Singleton.IsHost)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Host"))
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }
                else if (NetworkManager.Singleton.IsServer)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Server"))
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Disconnect Client"))
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }

                return;
            }

            GUI.Box(new Rect(0,                PosY, ButtonWidth,     LabelHeight), "Network Address:");
            GUI.Box(new Rect(ButtonWidth + 10, PosY, ButtonWidth / 2, LabelHeight), "Port:");

            PosY += LabelHeight;

            _networkAddress =  GUI.TextField(new Rect(0,                PosY, ButtonWidth,     LabelHeight), _networkAddress);
            _networkPort    =  GUI.TextField(new Rect(ButtonWidth + 10, PosY, ButtonWidth / 2, LabelHeight), _networkPort);
            PosY            += ButtonHeight;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport unityTransport)
            {
                ushort.TryParse(_networkPort, out ushort port);
                unityTransport.SetConnectionData(string.IsNullOrEmpty(_networkAddress) ? DefaultNetworkAddress : _networkAddress, port);
            }

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Server"))
            {
                NetworkManager.Singleton.StartServer();
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Helper method to set up UxrClientNetworkTransform components for a given object.
        /// </summary>
        /// <param name="go">The GameObject to set up</param>
        /// <param name="worldSpace">Whether to use world-space coordinates or local-space coordinates</param>
        /// <param name="flags">Option flags</param>
        /// <returns>List of components that were added: an UxrClientNetworkTransform and NetworkObject</returns>
        private IEnumerable<Behaviour> SetupClientNetworkTransform(GameObject go, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags)
        {
            if (go != null)
            {
                if (networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ChildTransform) == false)
                {
                    NetworkObject networkObject = go.GetOrAddComponent<NetworkObject>();
                    yield return networkObject;
                }

                UxrClientNetworkTransform clientNetworkTransform = go.GetOrAddComponent<UxrClientNetworkTransform>();
                clientNetworkTransform.InLocalSpace  = !worldSpace;
                clientNetworkTransform.SyncPositionX = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionX);
                clientNetworkTransform.SyncPositionY = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionY);
                clientNetworkTransform.SyncPositionZ = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionZ);
                clientNetworkTransform.SyncRotAngleX = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationX);
                clientNetworkTransform.SyncRotAngleY = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationY);
                clientNetworkTransform.SyncRotAngleZ = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationZ);
                clientNetworkTransform.SyncScaleX    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleX);
                clientNetworkTransform.SyncScaleY    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleY);
                clientNetworkTransform.SyncScaleZ    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleZ);
                yield return clientNetworkTransform;
            }
        }

        #endregion

        #region Private Data

        private const string DefaultNetworkAddress = "127.0.0.1";
        private const ushort DefaultNetworkPort    = 7777;
        private const int    LabelHeight           = 25;
        private const int    ButtonWidth           = 200;
        private const int    ButtonHeight          = 40;

        private int PosY { get; set; }

        private string _networkAddress = DefaultNetworkAddress;
        private string _networkPort    = DefaultNetworkPort.ToString();

        #endregion

#endif
    }
}

#pragma warning restore 414