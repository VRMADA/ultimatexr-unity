// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorDissonance.cs" company="VRMADA">
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
    ///     SDK Locator for the Dissonance Voice Chat SDK.
    /// </summary>
    public sealed class UxrSdkLocatorDissonance : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.VoiceOverNetwork;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkDissonance;

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
                        return new[] { "ULTIMATEXR_USE_DISSONANCE_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_DISSONANCE_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
            CurrentState = State.NotInstalled;

            if (IsTypeInAssemblies("Dissonance.DissonanceComms"))
            {
                CurrentVersion = 0;
                CurrentState   = State.Available;
            }
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorDissonance());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksNetworkingVoice + "Remove Symbols for Dissonance", priority = UxrConstants.Editor.PriorityMenuPathSdksNetworkingVoice)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorDissonance());
        }

        #endregion
    }
}