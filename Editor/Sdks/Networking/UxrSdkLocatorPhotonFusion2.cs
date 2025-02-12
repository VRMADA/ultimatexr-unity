// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorPhotonFusion2.cs" company="VRMADA">
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
    ///     SDK Locator for the Photon Fusion 2 SDK.
    /// </summary>
    public sealed class UxrSdkLocatorPhotonFusion2 : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.Networking;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkPhotonFusion2;

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2021.3.18";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_PHOTONFUSION2_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_PHOTONFUSION2_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_2021_3_OR_NEWER

            CurrentState = State.NotInstalled;

            if (IsTypeInAssemblies("Fusion.NetworkBehaviour") && IsTypeInAssemblies("Fusion.NetworkSceneInfo"))
            {
                CurrentVersion = 0;
                CurrentState   = State.Available;
            }

#else
            CurrentState = State.NeedsHigherUnityVersion
#endif
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://doc.photonengine.com/fusion/current/getting-started/sdk-download");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorPhotonFusion2());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworking + "Remove Symbols for Photon Fusion 2", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworking)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorPhotonFusion2());
        }

        #endregion
    }
}