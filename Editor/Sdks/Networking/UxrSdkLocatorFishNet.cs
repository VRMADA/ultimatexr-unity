// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorFishNet.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks.Networking
{
    /// <summary>
    ///     SDK Locator for the FishNet SDK.
    /// </summary>
    public sealed class UxrSdkLocatorFishNet : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.Networking;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkFishNet;

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2020.3";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_FISHNET_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_FISHNET_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
            CurrentState = State.NotInstalled;

            if (IsTypeInAssemblies("FishNet.Managing.NetworkManager"))
            {
                CurrentVersion = 0;
                CurrentState   = State.Available;
            }
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/network/fish-net-networking-evolved-207815");
        }

        /// <inheritdoc />
        public override void TryUpdate()
        {
            TryGet();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Auto-registers the locator each time Unity is launched or the project folder is updated.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void RegisterLocator()
        {
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorFishNet());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworking + "Remove Symbols for FishNet", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworking)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorFishNet());
        }

        #endregion
    }
}