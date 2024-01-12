// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrNetworkVoiceImplementation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UnityEngine;

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Interface for classes that implement network voice transmission functionality.
    /// </summary>
    public interface IUxrNetworkVoiceImplementation : IUxrNetworkSdk
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the compatible networking SDKs that this voice implementation can work with.
        /// </summary>
        IEnumerable<string> CompatibleNetworkSDKs { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds global support for the SDK if necessary, by adding required GameObjects and/or components to the
        ///     <see cref="UxrNetworkManager" /> or the scene where it is located.
        /// </summary>
        /// <param name="networkingSdk">
        ///     The networking SDK that is used. Since a voice implementation might support more than one
        ///     networking SDK, this parameter tells which networking SDK is currently selected
        /// </param>
        /// <param name="networkManager">The network manager</param>
        /// <param name="newGameObjects">Returns a list of GameObjects that were created, if any</param>
        /// <param name="newComponents">Returns a list of components that were created, if any</param>
        void SetupGlobal(string networkingSdk, UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <summary>
        ///     Adds network voice functionality to an <see cref="UxrAvatar" />.
        /// </summary>
        /// <param name="networkingSdk">
        ///     The networking SDK that is used. Since a voice implementation might support more than one
        ///     networking SDK, this parameter tells which networking SDK is currently selected
        /// </param>
        /// <param name="avatar">The avatar to add voice functionality to</param>
        /// <param name="newGameObjects">Returns a list of GameObjects that were created, if any</param>
        /// <param name="newComponents">Returns a list of components that were created, if any</param>
        void SetupAvatar(string networkingSdk, UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents);

        #endregion
    }
}