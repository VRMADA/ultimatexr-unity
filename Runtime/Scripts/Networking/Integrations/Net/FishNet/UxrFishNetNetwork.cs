// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFishNetNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
#if ULTIMATEXR_USE_FISHNET_SDK && UNITY_EDITOR
using UnityEditor;
#endif
#if ULTIMATEXR_USE_FISHNET_SDK
using System.Linq;
using FishNet;
using FishNet.Component.Spawning;
using FishNet.Component.Transforming;
using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Managing.Observing;
using FishNet.Object;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
#endif

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Networking.Integrations.Net.FishNet
{
    /// <summary>
    ///     Implementation of networking support using FishNet.
    /// </summary>
    public partial class UxrFishNetNetwork : UxrNetworkImplementation
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("Show a UI during play mode with connection options to quickly prototype networking functionality")] [SerializeField] private bool _usePrototypingUI = true;

        #endregion

        #region Public Overrides UxrNetworkImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkFishNet;

        /// <inheritdoc />
        public override bool IsServer
        {
            get
            {
#if ULTIMATEXR_USE_FISHNET_SDK
                // If IsServerStarted is not recognized, please update to a newer version of FishNet > 4.1.0.
                return _networkManager != null && _networkManager.IsServerStarted; 
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
#if ULTIMATEXR_USE_FISHNET_SDK
                // If IsClientStarted is not recognized, please update to a newer version of FishNet > 4.1.0.
                return _networkManager != null && _networkManager.IsClientStarted;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override UxrNetworkCapabilities Capabilities => UxrNetworkCapabilities.NetworkTransform | UxrNetworkCapabilities.NetworkRigidbody;
/*
        /// <inheritdoc />
        public override string NetworkRigidbodyWarning => $"{UxrConstants.SdkFishNet} does not use Rigidbodies";
*/
        /// <inheritdoc />
        public override void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_FISHNET_SDK && UNITY_EDITOR
            GameObject networkManagerGo = new GameObject("FishNetNetworkManager");
            Undo.RegisterCreatedObjectUndo(networkManagerGo, "Create FishNet Network Manager");
            networkManagerGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            networkManagerGo.transform.SetSiblingIndex(networkManager.transform.GetSiblingIndex() + 1);

            NetworkManager  fishNetNetworkManager = networkManagerGo.GetOrAddComponent<NetworkManager>();
            ObserverManager observerManager       = networkManagerGo.GetOrAddComponent<ObserverManager>();
            PlayerSpawner   playerSpawner         = networkManagerGo.GetOrAddComponent<PlayerSpawner>();

            // Try to assign DefaultPrefabObjects asset to NetworkManager. It will still throw an unassigned error,
            // adding the component triggers an internal check before we can assign anything.

            string[] guids = AssetDatabase.FindAssets("t:DefaultPrefabObjects", new[] { "Assets/" });

            if (guids.Any() && fishNetNetworkManager.SpawnablePrefabs == null)
            {
                fishNetNetworkManager.SpawnablePrefabs = AssetDatabase.LoadAssetAtPath<DefaultPrefabObjects>(AssetDatabase.GUIDToAssetPath(guids.First()));
            }

            Undo.RegisterFullObjectHierarchyUndo(networkManager.gameObject, "Setup FishNet NetworkManager");

            newGameObjects.Add(networkManagerGo);

            newComponents.Add(fishNetNetworkManager);
            newComponents.Add(observerManager);
            newComponents.Add(playerSpawner);

#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_FISHNET_SDK && UNITY_EDITOR
            if (avatar == null)
            {
                return;
            }
            NetworkObject avatarNetworkIdentity = avatar.gameObject.GetOrAddComponent<NetworkObject>();
            newComponents.Add(avatarNetworkIdentity);

            UxrFishNetAvatar fishNetAvatar = avatar.GetOrAddComponent<UxrFishNetAvatar>();
            newComponents.Add(fishNetAvatar);

            // FishNet doesn't allow world-space NetworkTransform synchronization, so we create dummies hanging from the avatar.
            // Local-space hanging from the avatar will work. 

            GameObject networkCamera = new GameObject("NetworkCamera");
            Undo.RegisterCreatedObjectUndo(networkCamera, "Create avatar network camera");
            Undo.SetTransformParent(networkCamera.transform, avatar.transform, "Parent network camera");
            networkCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject networkHandLeft = new GameObject("NetworkHandLeft");
            Undo.RegisterCreatedObjectUndo(networkHandLeft, "Create avatar network hand left");
            Undo.SetTransformParent(networkHandLeft.transform, avatar.transform, "Parent network hand left");
            networkHandLeft.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject networkHandRight = new GameObject("NetworkHandRight");
            Undo.RegisterCreatedObjectUndo(networkHandRight, "Create avatar network hand right");
            Undo.SetTransformParent(networkHandRight.transform, avatar.transform, "Parent network hand right");
            networkHandRight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            IEnumerable<Behaviour> avatarComponents    = SetupNetworkTransform(avatar.gameObject, true, UxrNetworkTransformFlags.ChildAll);
            IEnumerable<Behaviour> cameraComponents    = SetupNetworkTransform(networkCamera,     true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> leftHandComponents  = SetupNetworkTransform(networkHandLeft,   true, UxrNetworkTransformFlags.ChildTransform);
            IEnumerable<Behaviour> rightHandComponents = SetupNetworkTransform(networkHandRight,  true, UxrNetworkTransformFlags.ChildTransform);

            newComponents.AddRange(avatarComponents.ToList().Concat(cameraComponents).Concat(leftHandComponents).Concat(rightHandComponents));
            newGameObjects.AddRange(new[] { networkHandLeft, networkHandRight, networkCamera });

            fishNetAvatar.SetupDummyNetworkTransforms(networkCamera, networkHandLeft, networkHandRight);

            Undo.RegisterFullObjectHierarchyUndo(avatar.gameObject, "Setup FishNet Avatar");
#endif
        }

        /// <inheritdoc />
        public override void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs)
        {
#if ULTIMATEXR_USE_FISHNET_SDK && UNITY_EDITOR
            
            // Find the player spawner and assign the first avatar in the list as the spawnable avatar.

            PlayerSpawner fishNetPlayerSpawner = FindObjectOfType<PlayerSpawner>();

            if (fishNetPlayerSpawner != null)
            {
                SerializedObject so = new SerializedObject(fishNetPlayerSpawner);

                if (so != null)
                {
                    so.Update();
                    SerializedProperty sp = so.FindProperty("_playerPrefab");

                    if (sp != null && avatarPrefabs.Any() && sp.objectReferenceValue == null)
                    {
                        // If there is no avatar assigned and there is an avatar prefab available, assign the first.
                        sp.objectReferenceValue = avatarPrefabs.First().GetComponent<NetworkObject>();
                        so.ApplyModifiedProperties();
                    }
                }
            }
            
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags)
        {
#if ULTIMATEXR_USE_FISHNET_SDK && UNITY_EDITOR
            if (networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ChildTransform) == false)
            {
                NetworkObject networkObject = gameObject.GetOrAddComponent<NetworkObject>();
                yield return networkObject;
            }

            NetworkTransform networkTransform = gameObject.GetOrAddComponent<NetworkTransform>();

            yield return networkTransform;

#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlags)
        {
            // FishNet does not use NetworkRigidbody.
            // We can just use the NetworkTransform if the Rigidbody is kinematic.
            // Source: https://www.reddit.com/r/Unity3D/comments/vr4du5/comment/iettaaw/

            return AddNetworkTransform(gameObject, worldSpace, UxrNetworkTransformFlags.All);
        }

        /// <inheritdoc />
        public override void EnableNetworkTransform(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_FISHNET_SDK
            
            NetworkTransform[] networkTransforms = gameObject.GetComponentsInChildren<NetworkTransform>();
            networkTransforms.ForEach(nt => nt.SetEnabled(enable));
            
#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkRigidbody(GameObject gameObject, bool enable)
        {
            // FishNet does not use NetworkRigidbody.
            // We can just use the NetworkTransform if the Rigidbody is kinematic.
            // Source: https://www.reddit.com/r/Unity3D/comments/vr4du5/comment/iettaaw/

            EnableNetworkTransform(gameObject, enable);
        }

        /// <inheritdoc />
        public override bool HasAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_FISHNET_SDK
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            return networkObject != null && networkObject.IsOwner;

#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override void RequestAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_FISHNET_SDK
            if (gameObject == null)
            {
                return;
            }

            UxrFishNetAvatar fishNetAvatar = UxrAvatar.LocalAvatar.GetComponentInChildren<UxrFishNetAvatar>();
            NetworkObject    networkObject = gameObject.GetComponent<NetworkObject>();

            if (networkObject != null && fishNetAvatar != null)
            {
                fishNetAvatar.RequestAuthority(networkObject);
            }
#endif
        }

        /// <inheritdoc />
        public override void CheckReassignGrabAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_FISHNET_SDK

            UxrGrabbableObject grabbableObject = gameObject.GetComponent<UxrGrabbableObject>();
            NetworkObject      networkObject   = gameObject.GetComponent<NetworkObject>();

            if (networkObject != null && grabbableObject != null)
            {
                UxrAvatar avatarAuthority = UxrAvatar.EnabledComponents.FirstOrDefault(a => a.GetComponent<NetworkObject>() != null && a.GetComponent<NetworkObject>().OwnerId == networkObject.OwnerId);

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
#if ULTIMATEXR_USE_FISHNET_SDK
            return gameObject.GetComponent<NetworkTransform>() != null;
#else
            return false;
#endif
        }

        #endregion

#if ULTIMATEXR_USE_FISHNET_SDK

        #region Unity

        /// <summary>
        ///     Unity OnGUI call that will draw the prototyping UI if it's enabled.
        /// </summary>
        private void OnGUI()
        {
            if (!_usePrototypingUI || !TryGetNetworkManager())
            {
                return;
            }

            PosY = 0;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), string.Empty);

            GUI.Box(new Rect(0, PosY, ButtonWidth, ButtonHeight), "UltimateXR Fish Networking");
            PosY += ButtonHeight;
            
            if (UxrNetworkManager.IsServer || UxrNetworkManager.IsClient)
            {
                if (UxrNetworkManager.IsServerOnly)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Server"))
                    {
                        _networkManager.ServerManager.StopConnection(true);
                    }

                    PosY += ButtonHeight;

                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Client"))
                    {
                        _networkManager.ClientManager.StartConnection("localhost", NetworkPort);
                    }
                }
                else if (UxrNetworkManager.IsClientOnly)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Disconnect Client"))
                    {
                        _networkManager.ClientManager.StopConnection();
                    }
                }
                else if (UxrNetworkManager.IsHost)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Server"))
                    {
                        _networkManager.ServerManager.StopConnection(true);
                    }

                    PosY += ButtonHeight;

                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Client"))
                    {
                        _networkManager.ClientManager.StopConnection();
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

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Host"))
            {
                _networkManager.ServerManager.StartConnection(NetworkPort);
                _networkManager.ClientManager.StartConnection(_networkAddress, NetworkPort);
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Server"))
            {
                _networkManager.ServerManager.StartConnection(NetworkPort);
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Client"))
            {
                _networkManager.ClientManager.StartConnection(_networkAddress, NetworkPort);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to get the FishNet network manager.
        /// </summary>
        private NetworkManager TryGetNetworkManager()
        {
            _networkManager = InstanceFinder.NetworkManager;
            return _networkManager;
        }
        
        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the network port specified by the user through the UI.
        /// </summary>
        private ushort NetworkPort => ushort.Parse(_networkPort);

        private int PosY { get; set; }

        private const int LabelHeight  = 25;
        private const int ButtonWidth  = 200;
        private const int ButtonHeight = 40;

        private NetworkManager _networkManager;

        private string _networkAddress = "localhost";
        private string _networkPort    = "7777";

        #endregion

#endif
    }
}

#pragma warning restore 414