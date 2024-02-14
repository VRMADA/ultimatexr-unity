// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorPhotonVoice.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks.VoiceOverNetwork
{
    /// <summary>
    ///     SDK Locator for the Photon Voice SDK.
    /// </summary>
    public sealed class UxrSdkLocatorPhotonVoice : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.VoiceOverNetwork;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkPhotonVoice;

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
                        return new[] { "ULTIMATEXR_USE_PHOTONVOICE_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_PHOTONVOICE_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
            CurrentState = State.NotInstalled;

            if (IsTypeInAssemblies("Photon.Voice.Unity.Recorder"))
            {
                CurrentVersion = 0;
                CurrentState   = State.Available;
            }
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorPhotonVoice());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworkingVoice + "Remove Symbols for Photon Voice", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworkingVoice)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorPhotonVoice());
        }

        #endregion
    }
}