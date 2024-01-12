// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Core.Instantiation;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UnityEngine;

#pragma warning disable 414 // Unused values

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Network Manager. This singleton will take care of all communication between the different users to keep them in
    ///     sync.
    /// </summary>
    [RequireComponent(typeof(UxrInstanceManager))]
    public partial class UxrNetworkManager : UxrSingleton<UxrNetworkManager>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrNetworkImplementation      _networkImplementation;
        [SerializeField] private UxrNetworkVoiceImplementation _networkVoiceImplementation;
        [SerializeField] private bool                          _useSameSdkVoice               = true;
        [SerializeField] private List<GameObject>              _createdGlobalGameObjects      = new List<GameObject>();
        [SerializeField] private List<Component>               _createdGlobalComponents       = new List<Component>();
        [SerializeField] private List<GameObject>              _createdGlobalVoiceGameObjects = new List<GameObject>();
        [SerializeField] private List<Component>               _createdGlobalVoiceComponents  = new List<Component>();
        [SerializeField] private List<AvatarSetup>             _registeredAvatars             = new List<AvatarSetup>();
        [SerializeField] private bool                          _grabbablePhysicsAddProjectScenes;
        [SerializeField] private bool                          _grabbablePhysicsAddPathPrefabs;
        [SerializeField] private string                        _grabbablePhysicsPathRoot;
        [SerializeField] private bool                          _grabbablePhysicsOnlyLog;
        [SerializeField] private GrabbablePhysicsSetup         _grabbablePhysicsSetupInfo = new GrabbablePhysicsSetup();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called right after the authority of the local user over a GameObject was requested.
        /// </summary>
        public event Action<GameObject> LocalAuthorityRequested;

        /// <summary>
        ///     Event called right after a NetworkTransform component was enabled or disabled.
        /// </summary>
        public event Action<GameObject, bool> NetworkTransformEnabled;

        /// <summary>
        ///     Event called right after a NetworkRigidbody component was enabled or disabled.
        /// </summary>
        public event Action<GameObject, bool> NetworkRigidbodyEnabled;

        /// <summary>
        ///     Gets the network implementation.
        /// </summary>
        public UxrNetworkImplementation NetworkImplementation => _networkImplementation;

        /// <summary>
        ///     Gets the network voice implementation.
        /// </summary>
        public UxrNetworkVoiceImplementation NetworkVoiceImplementation => _networkVoiceImplementation;

        /// <summary>
        ///     Gets the global GameObjects created to add support for the given network SDK.
        /// </summary>
        public IEnumerable<GameObject> CreatedGlobalGameObjects => _createdGlobalGameObjects;

        /// <summary>
        ///     Gets the global Components created to add support for the given network SDK.
        /// </summary>
        public IEnumerable<Component> CreatedGlobalComponents => _createdGlobalComponents;

        /// <summary>
        ///     Gets the global GameObjects created to add support for the given network voice SDK.
        /// </summary>
        public IEnumerable<GameObject> CreatedGlobalVoiceGameObjects => _createdGlobalVoiceGameObjects;

        /// <summary>
        ///     Gets the global Components created to add support for the given network voice SDK.
        /// </summary>
        public IEnumerable<Component> CreatedGlobalVoiceComponents => _createdGlobalVoiceComponents;

        /// <summary>
        ///     Gets the registered avatar prefabs.
        /// </summary>
        public IEnumerable<UxrAvatar> RegisteredAvatarPrefabs
        {
            get
            {
                foreach (var avatar in _registeredAvatars)
                {
                    yield return avatar.AvatarPrefab;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Requests authority of the local avatar over the given target object. The target object should have a valid network
        ///     component using any given SDK, such as NetworkObject.
        /// </summary>
        /// <param name="gameObject">GameObject with networking component</param>
        public void RequestAuthority(GameObject gameObject)
        {
            NetworkImplementation.RequestAuthority(gameObject);
            OnAuthorityRequested(gameObject);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (NetworkImplementation)
            {
                UxrGrabManager.Instance.ObjectGrabbed += GrabManager_ObjectGrabbed;
                UxrGrabManager.Instance.ObjectGrabbed += GrabManager_ObjectReleased;
            }
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (NetworkImplementation)
            {
                UxrGrabManager.Instance.ObjectGrabbed -= GrabManager_ObjectGrabbed;
                UxrGrabManager.Instance.ObjectGrabbed -= GrabManager_ObjectReleased;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when an object was grabbed. Checks if the grabbed object's authority or a NetworkRigidbody enabled
        ///     state need to change. A free object that was grabbed will now belong to the grabbing avatar and have its
        ///     NetworkRigidbody disabled because it is not driven by physics anymore, but by the avatar's tracked hand.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void GrabManager_ObjectGrabbed(object sender, UxrManipulationEventArgs e)
        {
            if (e.IsGrabbedStateChanged && e.GrabbableObject.RigidBodySource != null && e.GrabbableObject.RigidBodyDynamicOnRelease && e.GrabbableObject.CanUseRigidBody)
            {
                // From not grabbed to grabbed. Switch authority to local avatar if it's the one that grabbed it.

                if (e.Grabber != null && e.Grabber.Avatar == UxrAvatar.LocalAvatar && NetworkImplementation.HasAuthority(e.GrabbableObject.gameObject))
                {
                    NetworkImplementation.RequestAuthority(e.GrabbableObject.gameObject);
                    OnAuthorityRequested(e.GrabbableObject.gameObject);
                }

                // Disable network rigidbody because avatar will drive the grabbed object's transform.

                NetworkImplementation.EnableNetworkRigidbody(e.GrabbableObject.gameObject, false);
                OnNetworkRigidbodyEnabled(e.GrabbableObject.gameObject, false);
            }
        }

        /// <summary>
        ///     Called when an object was released. Checks if the avatar that had the authority needs to give it to another avatar
        ///     that keeps the grab or if the object was thrown and the NetworkRigidbody needs to be enabled.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void GrabManager_ObjectReleased(object sender, UxrManipulationEventArgs e)
        {
            if (e.GrabbableObject.RigidBodySource != null && e.GrabbableObject.RigidBodyDynamicOnRelease && e.GrabbableObject.CanUseRigidBody)
            {
                if (e.IsGrabbedStateChanged)
                {
                    // From grabbed to released. Enable network rigidbody.
                    NetworkImplementation.EnableNetworkRigidbody(e.GrabbableObject.gameObject, true);
                    OnNetworkRigidbodyEnabled(e.GrabbableObject.gameObject, true);
                }
                else if (e.Grabber != null && !UxrGrabManager.Instance.IsBeingGrabbedBy(e.GrabbableObject, e.Grabber.Avatar))
                {
                    // An avatar released its last grip, check if we need to reassign the network authority  
                    NetworkImplementation.CheckReassignGrabAuthority(e.GrabbableObject.gameObject);
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="NetworkRigidbodyEnabled" />.
        /// </summary>
        /// <param name="gameObject">The GameObject with the NetworkRigidbody</param>
        /// <param name="enabled">Whether the component was enabled or disabled</param>
        private void OnNetworkRigidbodyEnabled(GameObject gameObject, bool enabled)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} NetworkRigidbody {(enabled ? "enabled" : "disabled")} on GameObject {gameObject.GetPathUnderScene()}.");
            }

            NetworkRigidbodyEnabled?.Invoke(gameObject, enabled);
        }

        /// <summary>
        ///     Event trigger for <see cref="NetworkTransformEnabled" />.
        /// </summary>
        /// <param name="gameObject">The GameObject with the NetworkRigidbody</param>
        /// <param name="enabled">Whether the component was enabled or disabled</param>
        private void OnNetworkTransformEnabled(GameObject gameObject, bool enabled)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} NetworkTransform {(enabled ? "enabled" : "disabled")} on GameObject {gameObject.GetPathUnderScene()}.");
            }

            NetworkTransformEnabled?.Invoke(gameObject, enabled);
        }

        /// <summary>
        ///     Event trigger for <see cref="LocalAuthorityRequested" />.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the authority for</param>
        private void OnAuthorityRequested(GameObject gameObject)
        {
            if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{UxrConstants.NetworkingModule} Authority requested over GameObject {gameObject.GetPathUnderScene()}.");
            }

            LocalAuthorityRequested?.Invoke(gameObject);
        }

        #endregion
    }
}

#pragma warning restore 414