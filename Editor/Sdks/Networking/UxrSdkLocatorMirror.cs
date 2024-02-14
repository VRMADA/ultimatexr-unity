// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorMirror.cs" company="VRMADA">
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
    ///     SDK Locator for the Mirror SDK.
    /// </summary>
    public sealed class UxrSdkLocatorMirror : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.Networking;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkMirror;

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
                        return new[] { "ULTIMATEXR_USE_MIRROR_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_MIRROR_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
            CurrentState = State.NotInstalled;

            if (IsTypeInAssemblies("Mirror.NetworkBehaviour"))
            {
                CurrentVersion = 0;
                CurrentState   = State.Available;
            }
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/network/mirror-129321");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorMirror());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworking + "Remove Symbols for Mirror", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworking)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorMirror());
        }

        #endregion
    }
}