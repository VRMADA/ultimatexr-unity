// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorWebXR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     SDK Locator for WebXR' SDK.
    /// </summary>
    public sealed class UxrSdkLocatorWebXR : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override string Name => "WebXR";

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2020.1";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_WEBXR_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_WEBXR_SDK" }; }
        }

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_2018_3_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.WebGL)
            {
#if ULTIMATEXR_USE_WEBXR_SDK
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
            if (EditorUtility.DisplayDialog("Install package?", $"WebXR package needs to be installed. Proceed to the OpenUPM?", "Yes", "Cancel"))
            {
                Application.OpenURL("https://openupm.com/packages/com.de-panther.webxr/");
            }
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorWebXR());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for WebXR SDK")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorWebXR());
        }

        #endregion
    }
}