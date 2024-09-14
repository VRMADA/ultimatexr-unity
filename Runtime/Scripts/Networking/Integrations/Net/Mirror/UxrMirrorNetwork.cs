// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMirrorNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ULTIMATEXR_USE_MIRROR_SDK
using System.Linq;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using Mirror;
using kcp2k;
#endif

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Networking.Integrations.Net.Mirror
{
    /// <summary>
    ///     Implementation of networking support using Mirror.
    /// </summary>
    public class UxrMirrorNetwork : UxrNetworkImplementation
    {
        #region Inspector Properties/Serialized Fields

        [Tooltip("Show a UI during play mode with connection options to quickly prototype networking functionality")] [SerializeField] private bool _usePrototypingUI = true;

        #endregion

        #region Public Overrides UxrNetworkImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkMirror;

        /// <inheritdoc />
        public override bool IsServer
        {
            get
            {
#if ULTIMATEXR_USE_MIRROR_SDK
                return NetworkManager.singleton.isNetworkActive && (NetworkManager.singleton.mode == NetworkManagerMode.Host || NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly);
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
#if ULTIMATEXR_USE_MIRROR_SDK
                return NetworkManager.singleton.isNetworkActive && (NetworkManager.singleton.mode == NetworkManagerMode.Host || NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly);
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override string NetworkRigidbodyWarning => "Mirror doesn't support nested NetworkIdentity components, which is required when using grabbable rigidbodies with other grabbable rigidbodies attached. If you're using nested grabbable rigidbodies, do not set up NetworkRigidbody components here. Don't worry! UltimateXR will still synchronize them using RPC calls to try to keep the same position/velocity on all clients.";

        /// <inheritdoc />
        public override UxrNetworkCapabilities Capabilities => UxrNetworkCapabilities.NetworkTransform | UxrNetworkCapabilities.NetworkRigidbody;

        /// <inheritdoc />
        public override void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_MIRROR_SDK && UNITY_EDITOR

            GameObject networkManagerGo = new GameObject("MirrorNetworkManager");
            Undo.RegisterCreatedObjectUndo(networkManagerGo, "Create Mirror Network Manager");
            networkManagerGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            networkManagerGo.transform.SetSiblingIndex(networkManager.transform.GetSiblingIndex() + 1);

            NetworkManager mirrorNetworkManager = networkManagerGo.GetOrAddComponent<NetworkManager>();
            KcpTransport   mirrorTransport      = networkManagerGo.GetOrAddComponent<KcpTransport>();

            mirrorNetworkManager.transport = mirrorTransport;

            Undo.RegisterFullObjectHierarchyUndo(networkManager.gameObject, "Setup Mirror NetworkManager");

            newComponents.Add(mirrorTransport);
            newComponents.Add(mirrorNetworkManager);
            newGameObjects.Add(networkManagerGo);

#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_MIRROR_SDK && UNITY_EDITOR

            if (avatar == null)
            {
                return;
            }

            NetworkIdentity avatarNetworkIdentity = avatar.gameObject.GetOrAddComponent<NetworkIdentity>();

            UxrMirrorAvatar mirrorAvatar = avatar.GetOrAddComponent<UxrMirrorAvatar>();
            newComponents.Add(mirrorAvatar);

            IEnumerable<Behaviour> avatarComponents    = SetupNetworkTransform(avatar.gameObject,                                  true, UxrNetworkTransformFlags.ChildAll);
            IEnumerable<Behaviour> cameraComponents    = SetupNetworkTransform(avatar.CameraComponent.gameObject,                  true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> leftHandComponents  = SetupNetworkTransform(avatar.GetHand(UxrHandSide.Left).Wrist.gameObject,  true, UxrNetworkTransformFlags.ChildPositionAndRotation);
            IEnumerable<Behaviour> rightHandComponents = SetupNetworkTransform(avatar.GetHand(UxrHandSide.Right).Wrist.gameObject, true, UxrNetworkTransformFlags.ChildPositionAndRotation);

            newComponents.AddRange(avatarComponents.ToList().Concat(cameraComponents).Concat(leftHandComponents).Concat(rightHandComponents));
            newComponents.Add(avatarNetworkIdentity);

            foreach (Behaviour behaviour in newComponents)
            {
                if (behaviour is NetworkTransformUnreliable networkTransformUnreliable)
                {
                    networkTransformUnreliable.syncDirection = SyncDirection.ClientToServer;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(avatar.gameObject, "Setup Mirror Avatar");

#endif
        }

        /// <inheritdoc />
        public override void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs)
        {
#if ULTIMATEXR_USE_MIRROR_SDK && UNITY_EDITOR

            NetworkManager mirrorNetworkManager = FindObjectOfType<NetworkManager>();

            if (mirrorNetworkManager != null)
            {
                mirrorNetworkManager.playerPrefab = avatarPrefabs.Any() ? avatarPrefabs.First().gameObject : null;
                Undo.RegisterCompleteObjectUndo(mirrorNetworkManager, "Setup Mirror Avatar");
            }

#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags)
        {
#if ULTIMATEXR_USE_MIRROR_SDK && UNITY_EDITOR

            NetworkIdentity networkIdentity = null;

            if (networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ChildTransform) == false)
            {
                networkIdentity = gameObject.GetOrAddComponent<NetworkIdentity>();
            }

            NetworkTransformUnreliable networkTransform = gameObject.GetOrAddComponent<NetworkTransformUnreliable>();
            networkTransform.coordinateSpace = worldSpace ? CoordinateSpace.World : CoordinateSpace.Local;
            networkTransform.syncPosition    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionX) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionY) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.PositionZ);
            networkTransform.syncRotation    = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationX) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationY) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.RotationZ);
            networkTransform.syncScale       = networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleX) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleY) | networkTransformFlags.HasFlag(UxrNetworkTransformFlags.ScaleZ);
            networkTransform.positionSensitivity = 0.001f;
            yield return networkTransform;

            if (networkIdentity)
            {
                // return after the transform, so that when they are removed, the transform is removed before. Otherwise Mirror complains about transform requiring identity.
                yield return networkIdentity;
            }
#else
            yield break;
#endif
        }

        /// <inheritdoc />
        public override IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlags)
        {
#if ULTIMATEXR_USE_MIRROR_SDK && UNITY_EDITOR

            UxrGrabbableObject grabbableObject = gameObject.GetComponent<UxrGrabbableObject>();

            if (grabbableObject != null && grabbableObject.RigidBodySource != null && grabbableObject.RigidBodyDynamicOnRelease && grabbableObject.transform.parent != null)
            {
                UxrGrabbableObject[] parentGrabbableObjects = grabbableObject.transform.parent.GetComponentsInParent<UxrGrabbableObject>(true);
                UxrGrabbableObject   physicsDrivenParent    = parentGrabbableObjects.FirstOrDefault(g => g.RigidBodySource != null && g.RigidBodyDynamicOnRelease);

                if (physicsDrivenParent != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.NetworkingModule} Ignoring physics-driven grabbable object {grabbableObject.GetPathUnderScene()} because there is already a parent physics-driven grabbable object ({physicsDrivenParent.GetPathUnderScene()}) and Mirror doesn't support nested NetworkIdentity components. UltimateXR will sync the rigidbody using RPC calls.");
                    }

                    yield break;
                }
            }

            // Building list forces evaluation of AddNetworkTransform IEnumerable and creates the components
            List<Behaviour> networkTransformComponents = new List<Behaviour>(AddNetworkTransform(gameObject, worldSpace, UxrNetworkTransformFlags.All));

            NetworkRigidbodyUnreliable networkRigidbody = gameObject.GetOrAddComponent<NetworkRigidbodyUnreliable>();
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
#if ULTIMATEXR_USE_MIRROR_SDK

            NetworkTransformUnreliable[] networkTransforms = gameObject.GetComponentsInChildren<NetworkTransformUnreliable>();
            networkTransforms.ForEach(nt => nt.SetEnabled(enable));

#endif
        }

        /// <inheritdoc />
        public override void EnableNetworkRigidbody(GameObject gameObject, bool enable)
        {
#if ULTIMATEXR_USE_MIRROR_SDK
            EnableNetworkTransform(gameObject, enabled);

            NetworkRigidbodyUnreliable[] networkRigidbodies = gameObject.GetComponentsInChildren<NetworkRigidbodyUnreliable>();
            networkRigidbodies.ForEach(nrb => nrb.SetEnabled(enable));
#endif
        }

        /// <inheritdoc />
        public override bool HasAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_MIRROR_SDK

            NetworkIdentity networkIdentity = gameObject.GetComponent<NetworkIdentity>();

            if (networkIdentity == null)
            {
                return false;
            }

            return networkIdentity.isOwned;
#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override void RequestAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_MIRROR_SDK

            if (gameObject == null)
            {
                return;
            }

            UxrMirrorAvatar mirrorAvatar    = UxrAvatar.LocalAvatar.GetComponentInChildren<UxrMirrorAvatar>();
            NetworkIdentity networkIdentity = gameObject.GetComponent<NetworkIdentity>();

            if (mirrorAvatar != null && networkIdentity != null)
            {
                mirrorAvatar.RequestAuthority(networkIdentity);
            }
#endif
        }

        /// <inheritdoc />
        public override void CheckReassignGrabAuthority(GameObject gameObject)
        {
#if ULTIMATEXR_USE_MIRROR_SDK

            UxrGrabbableObject grabbableObject = gameObject.GetComponent<UxrGrabbableObject>();
            NetworkIdentity    networkIdentity = gameObject.GetComponent<NetworkIdentity>();

            if (networkIdentity != null && grabbableObject != null)
            {
                UxrAvatar avatarAuthority = UxrAvatar.EnabledComponents.FirstOrDefault(a => a.GetComponent<NetworkIdentity>() != null && a.GetComponent<NetworkIdentity>().connectionToClient == networkIdentity.connectionToClient);

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
#if ULTIMATEXR_USE_MIRROR_SDK
            return gameObject.GetComponent<NetworkTransformUnreliable>() != null || gameObject.GetComponent<NetworkRigidbodyUnreliable>() != null;
#else
            return false;
#endif
        }

        #endregion

#if ULTIMATEXR_USE_MIRROR_SDK

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _networkAddress = NetworkManager.singleton?.networkAddress;

            if (Transport.active != null && Transport.active is KcpTransport kcpTransport)
            {
                _networkPort = kcpTransport.Port.ToString();
            }
        }

        /// <summary>
        ///     Shows the connection UI if its enabled.
        /// </summary>
        private void OnGUI()
        {
            if (!_usePrototypingUI || NetworkManager.singleton == null)
            {
                return;
            }

            PosY = 0;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), string.Empty);

            GUI.Box(new Rect(0, PosY, ButtonWidth, ButtonHeight), "UltimateXR Unity Mirror");
            PosY += ButtonHeight;

            if (NetworkManager.singleton.isNetworkActive)
            {
                if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Host"))
                    {
                        NetworkManager.singleton.StopHost();
                    }
                }
                else if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Stop Server"))
                    {
                        NetworkManager.singleton.StopServer();
                    }
                }
                else if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
                {
                    if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Disconnect Client"))
                    {
                        NetworkManager.singleton.StopClient();
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

            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.networkAddress = string.IsNullOrEmpty(_networkAddress) ? DefaultNetworkAddress : _networkAddress;
            }

            if (Transport.active != null && Transport.active is KcpTransport kcpTransport)
            {
                if (int.TryParse(string.IsNullOrEmpty(_networkPort) ? DefaultNetworkPort : _networkPort, out int port))
                {
                    kcpTransport.Port = (ushort)port;
                }
            }

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Host"))
            {
                NetworkManager.singleton.StartHost();
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Server"))
            {
                NetworkManager.singleton.StartServer();
            }

            PosY += ButtonHeight;

            if (GUI.Button(new Rect(0, PosY, ButtonWidth, ButtonHeight), "Start Client"))
            {
                NetworkManager.singleton.StartClient();
            }
        }

        #endregion

        #region Private Data

        private const string DefaultNetworkAddress = "localhost";
        private const string DefaultNetworkPort    = "7777";
        private const int    LabelHeight           = 25;
        private const int    ButtonWidth           = 200;
        private const int    ButtonHeight          = 40;

        private int PosY { get; set; }

        private string _networkAddress = DefaultNetworkAddress;
        private string _networkPort    = DefaultNetworkPort;

        #endregion

#endif
    }
}

#pragma warning restore 414
