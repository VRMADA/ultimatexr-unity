// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorUltraleap.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     SDK Locator for the hand-tracking Ultraleap SDK.
    /// </summary>
    public sealed class UxrSdkLocatorUltraleap : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override string Name => UxrManager.SdkUltraleap;

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
                        return new[] { "ULTIMATEXR_USE_ULTRALEAP_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_ULTRALEAP_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_5_6_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                /*
                CurrentState = State.NotInstalled;

                if (IsTypeInAssemblies("Leap.Unity.RigidHand"))
                {
                    CurrentVersion = 0;
                    CurrentState   = State.Available;
                }

                CurrentVersion = 0;*/
                CurrentState = State.SoonSupported;
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
            Application.OpenURL("https://github.com/ultraleap/UnityPlugin/releases/");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorUltraleap());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for Ultraleap")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorUltraleap());
        }

        #endregion
    }
}