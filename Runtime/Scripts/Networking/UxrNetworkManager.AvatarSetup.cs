// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkManager.AvatarSetup.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar;
using UnityEngine;

namespace UltimateXR.Networking
{
    public partial class UxrNetworkManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores an avatar setup entry. It contains the avatar prefab that was enabled for networking and the components that
        ///     were added from an arbitrary SDK to add support.
        /// </summary>
        [Serializable]
        private class AvatarSetup
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private UxrAvatar _avatarPrefab;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the avatar component in the prefab.
            /// </summary>
            public UxrAvatar AvatarPrefab => _avatarPrefab;

            #endregion
        }

        #endregion
    }
}