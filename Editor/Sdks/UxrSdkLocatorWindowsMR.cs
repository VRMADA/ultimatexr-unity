// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorWindowsMR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     SDK Locator for the Windows Mixed Reality SDK.
    /// </summary>
    public sealed class UxrSdkLocatorWindowsMixedReality : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override string Name => UxrManager.SdkWindowsMixedReality;

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
                        return new[] { "ULTIMATEXR_USE_WINDOWSMIXEDREALITY_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_WINDOWSMIXEDREALITY_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => false;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_2017_2_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.WSAPlayer or BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64)
            {
                CurrentState   = State.Available;
                CurrentVersion = 0;
            }
            else
            {
                CurrentState = State.CurrentTargetNotSupported;
            }
#else
            CurrentState = State.NeedsHigherUnityVersion;
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Auto-registers the locator each time Unity is launched or the project folder is updated.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void RegisterLocator()
        {
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorWindowsMixedReality());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for Windows Mixed Reality")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorWindowsMixedReality());
        }

        #endregion
    }
}