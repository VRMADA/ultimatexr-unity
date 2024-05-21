// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkImplementation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Base class required to add support for a network SDK.
    /// </summary>
    public abstract class UxrNetworkImplementation : UxrComponent, IUxrNetworkImplementation
    {
        #region Implicit IUxrNetworkImplementation

        /// <inheritdoc />
        public abstract bool IsServer { get; }

        /// <inheritdoc />
        public abstract bool IsClient { get; }

        /// <inheritdoc />
        public abstract UxrNetworkCapabilities Capabilities { get; }

        /// <inheritdoc />
        public virtual string NetworkRigidbodyWarning => null;

        /// <inheritdoc />
        public abstract void SetupGlobal(UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <inheritdoc />
        public abstract void SetupAvatar(UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <inheritdoc />
        public abstract void SetupPostProcess(IEnumerable<UxrAvatar> avatarPrefabs);

        /// <inheritdoc />
        public abstract IEnumerable<Behaviour> AddNetworkTransform(GameObject gameObject, bool worldSpace, UxrNetworkTransformFlags networkTransformFlags);

        /// <inheritdoc />
        public abstract IEnumerable<Behaviour> AddNetworkRigidbody(GameObject gameObject, bool worldSpace, UxrNetworkRigidbodyFlags networkRigidbodyFlags);

        /// <inheritdoc />
        public abstract void EnableNetworkTransform(GameObject gameObject, bool enable);

        /// <inheritdoc />
        public abstract void EnableNetworkRigidbody(GameObject gameObject, bool enable);

        /// <inheritdoc />
        public abstract bool HasAuthority(GameObject gameObject);

        /// <inheritdoc />
        public abstract void RequestAuthority(GameObject gameObject);

        /// <inheritdoc />
        public abstract void CheckReassignGrabAuthority(GameObject gameObject);

        /// <inheritdoc />
        public abstract bool HasNetworkTransformSyncComponents(GameObject gameObject);

        #endregion

        #region Implicit IUxrNetworkSdk

        /// <inheritdoc />
        public abstract string SdkName { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Helper method to set up NetworkTransform components for a given object.
        /// </summary>
        /// <param name="go">The GameObject to set up</param>
        /// <param name="worldSpace">Whether to use world-space coordinates or local-space coordinates</param>
        /// <param name="flags">Option flags</param>
        /// <returns>List of components that were added, usually a NetworkTransform and NetworkObject or similar</returns>
        protected IEnumerable<Behaviour> SetupNetworkTransform(GameObject go, bool worldSpace, UxrNetworkTransformFlags flags)
        {
            if (go != null)
            {
                IEnumerable<Behaviour> newComponents = ((IUxrNetworkImplementation)this).AddNetworkTransform(go, worldSpace, flags);

                foreach (Behaviour newBehaviour in newComponents)
                {
                    yield return newBehaviour;
                }
            }
        }

        #endregion
    }
}