// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorMagicLeap.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     SDK Locator for the Magic Leap SDK.
    /// </summary>
    public sealed class UxrSdkLocatorMagicLeap : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator
        
        /// <inheritdoc />
        public override string PackageName => "com.magicleap.unitysdk";

        /// <inheritdoc />
        public override string Name => UxrManager.SdkMagicLeap;

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2022.2";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_MAGICLEAP_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_MAGICLEAP_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_2022_2_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.Android or BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64)
            {
                // UltimateXR assembly already sets up version define for the Magic Leap package
#if ULTIMATEXR_USE_MAGICLEAP_SDK
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
            CurrentState = State.NeedsHigherUnityVersion;
#endif
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            Application.OpenURL("https://developer-docs.magicleap.cloud/docs/guides/unity/getting-started/install-the-tools");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorMagicLeap());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for Magic Leap")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorMagicLeap());
        }

        #endregion
    }
}