// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorUnityNetCode.cs" company="VRMADA">
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
    ///     SDK Locator for the Unity NetCode package.
    /// </summary>
    public sealed class UxrSdkLocatorUnityNetCode : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.Networking;

        /// <inheritdoc />
        public override string PackageName => "com.unity.netcode.gameobjects";

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkUnityNetCode;

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
                        return new[] { "ULTIMATEXR_USE_UNITY_NETCODE" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_UNITY_NETCODE" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
            // UltimateXR assembly already sets up version define for the Unity NetCode package
#if ULTIMATEXR_USE_UNITY_NETCODE
            CurrentVersion = 0;
            CurrentState = State.Available;
#else
            CurrentState = State.NotInstalled;
#endif
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://docs-multiplayer.unity3d.com/netcode/current/installation/");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorUnityNetCode());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworking + "Remove Symbols for Unity NetCode", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworking)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorUnityNetCode());
        }

        #endregion
    }
}