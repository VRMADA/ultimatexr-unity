// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrNetworkImplementation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UnityEngine;

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Interface for classes that implement network functionality.
    /// </summary>
    public interface IUxrNetworkImplementation : IUxrNetworkSdk
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether there is a networking session currently active and the local user is the server.
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        ///     Gets whether there is a networking session currently active and the local user is a client.
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        ///     Gets the SDK capabilities.
        /// </summary>
        UxrNetworkCapabilities Capabilities { get; }

        /// <summary>
        ///     Gets a warning string if the NetworkRigidbody support requires extra attention. This is used by the
        ///     UxrNetworkManagerEditor to show a warning text box in case this implementation is selected.
        /// </summary>
        string NetworkRigidbodyWarning { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds global support for the SDK if necessary, by adding required GameObjects and/or components to the
        ///     <see cref="UxrNetworkManager" /> or the scene where it is located.
        /// </summary>
        /// <param name="networkManager">The network manager</param>
        /// <param name="newGameObjects">Returns a list of GameObjects that were created, if any</param>
        /// <param name="newComponents">Returns a list of components that were created, if any</param>
        void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <summary>
        ///     Adds network synchronization functionality to an <see cref="UxrAvatar" />. Each avatar passed as argument is an
        ///     instance of a prefab in the <see cref="UxrNetworkManager" /> avatar list.
        ///     All changes will be applied to the prefab after the setup returns and the avatar instance will be destroyed. To get
        ///     references to the actual prefabs to, for example, fill a list of spawnable prefabs, use
        ///     <see cref="SetupPostProcess" />.
        /// </summary>
        /// <param name="avatar">The avatar to add functionality to</param>
        /// <param name="newGameObjects">Returns a list of GameObjects that were created, if any</param>
        /// <param name="newComponents">Returns a list of components that were created, if any</param>
        void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <summary>
        ///     Called after the setup finished. At this point the modifications on the avatar prefabs have been applied.
        ///     This can be used to fill a list of spawnable prefabs which is a common requirement in networking SDKs.
        /// </summary>
        void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs);

        /// <summary>
        ///     Adds network synchronization functionality to a <see cref="Transform" />.
        /// </summary>
        /// <param name="gameObject">The GameObject to add functionality to</param>
        /// <param name="worldSpace">Whether to synchronize world space coordinates (true) or local space (false)</param>
        /// <param name="networkTransformFlags">Which elements to synchronize</param>
        /// <returns>List of components that were added</returns>
        IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags);

        /// <summary>
        ///     Adds network synchronization functionality to a <see cref="Rigidbody" />.
        /// </summary>
        /// <param name="gameObject">The GameObject with the rigidbody to add functionality to</param>
        /// <param name="worldSpace">Whether to synchronize world space coordinates (true) or local space (false)</param>
        /// <param name="networkRigidbodyFlags">Options</param>
        /// <returns>List of components that were added</returns>
        IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlags);

        /// <summary>
        ///     Enables or disables a network transform component.
        /// </summary>
        /// <param name="gameObject">GameObject where the network transform is located</param>
        /// <param name="enable">Whether to enable or disable the component</param>
        void EnableNetworkTransform(GameObject gameObject, bool enable);

        /// <summary>
        ///     Enables or disables a network rigidbody component.
        /// </summary>
        /// <param name="gameObject">GameObject where the network rigidbody is located</param>
        /// <param name="enable">Whether to enable or disable the component</param>
        void EnableNetworkRigidbody(GameObject gameObject, bool enable);

        /// <summary>
        ///     Gets whether the current client has the authority over a network GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to check the authority of</param>
        bool HasAuthority(GameObject gameObject);

        /// <summary>
        ///     Requests authority of the local user over a network GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to request authority over</param>
        void RequestAuthority(GameObject gameObject);

        /// <summary>
        ///     Checks if an object that is being grabbed is missing a client authority, and assigns a new avatar as authority.
        /// </summary>
        /// <param name="gameObject">The GameObject with a <see cref="UxrGrabbableOBject" /> component</param>
        void CheckReassignGrabAuthority(GameObject gameObject);

        /// <summary>
        ///     Gets whether an object has networking components (NetworkTransform/NetworkRigidbody) to sync its transform.
        /// </summary>
        /// <param name="gameObject">GameObject to check</param>
        /// <returns>
        ///     Boolean telling whether the object has any NetworkTransform/NetworkRigidbody components using the
        ///     implementation.
        /// </returns>
        bool HasNetworkTransformSyncComponents(GameObject gameObject);

        #endregion
    }
}