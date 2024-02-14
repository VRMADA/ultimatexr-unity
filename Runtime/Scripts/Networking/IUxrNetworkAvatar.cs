// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrNetworkAvatar.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UnityEngine;

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Interface for network avatar components. Network avatar components are responsible for setting the avatar in the
    ///     correct mode (local/external) and sending/receiving global component state changes.
    /// </summary>
    public interface IUxrNetworkAvatar
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called right after the avatar was spawned.
        /// </summary>
        event Action AvatarSpawned;

        /// <summary>
        ///     Event called right after the avatar was despawned.
        /// </summary>
        event Action AvatarDespawned;

        /// <summary>
        ///     Gets whether this avatar is the avatar controller by the user (true) or a remote avatar (false).
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        ///     Gets the avatar component.
        /// </summary>
        UxrAvatar Avatar { get; }

        /// <summary>
        ///     Gets the list of objects that will be disabled when the avatar is in local mode. This allows to avoid rendering the
        ///     head or elements that could intersect with the camera.
        /// </summary>
        IList<GameObject> LocalDisabledGameObjects { get; }

        /// <summary>
        ///     Gets or sets the avatar name.
        /// </summary>
        string AvatarName { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes an avatar. Should be called by the implementation right after the avatar was spawned.
        /// </summary>
        /// <param name="avatar">Avatar component</param>
        /// <param name="isLocal">Whether the avatar is local</param>
        /// <param name="uniqueId">A unique Id to identify the avatar, usually the user unique network ID</param>
        /// <param name="avatarName">The name of the avatar, to assign it to the avatar GameObject and a label if there is a label</param>
        void InitializeNetworkAvatar(UxrAvatar avatar, bool isLocal, string uniqueId, string avatarName);

        #endregion
    }
}