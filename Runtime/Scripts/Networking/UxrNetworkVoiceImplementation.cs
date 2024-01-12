// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkVoiceImplementation.cs" company="VRMADA">
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
    ///     Base class required to add support for a network voice communication SDK.
    /// </summary>
    public abstract class UxrNetworkVoiceImplementation : UxrComponent, IUxrNetworkVoiceImplementation
    {
        #region Implicit IUxrNetworkSdk

        /// <inheritdoc />
        public abstract string SdkName { get; }

        #endregion

        #region Implicit IUxrNetworkVoiceImplementation

        /// <inheritdoc />
        public abstract IEnumerable<string> CompatibleNetworkSDKs { get; }

        /// <inheritdoc />
        public abstract void SetupGlobal(string networkingSdk, UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents);

        /// <inheritdoc />
        public abstract void SetupAvatar(string networkingSdk, UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents);

        #endregion
    }
}