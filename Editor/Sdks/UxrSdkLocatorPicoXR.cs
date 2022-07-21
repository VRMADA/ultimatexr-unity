// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorPicoXR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     SDK Locator for the PicoXR SDK.
    /// </summary>
    public sealed class UxrSdkLocatorPicoXR : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override string PackageName => "com.unity.xr.picoxr";

        /// <inheritdoc />
        public override string Name => UxrManager.SdkPicoXR;

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2021.1";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_PICOXR_SDK" };
                    }
                }

                return Array.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_PICOXR_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_2019_4_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // UltimateXR assembly already sets up version define for the PicoXR package
#if ULTIMATEXR_USE_PICOXR_SDK
                CurrentVersion = 0;
                CurrentState = State.Available;
#else
                CurrentState = State.NotInstalled;
#endif
            }
            else
            {
                CurrentState = State.CurrentTargetNotSupported;
            }

#else
            CurrentState = State.NeedsHigherUnityVersion
#endif
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://developer.pico-interactive.com/sdk");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorPicoXR());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for PicoXR")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorPicoXR());
        }

        #endregion
    }
}