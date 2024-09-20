// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorOvr.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks.InputTracking
{
    /// <summary>
    ///     SDK Locator for Oculus/Meta SDK.
    /// </summary>
    public sealed class UxrSdkLocatorOvr : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override SupportType Support => SupportType.InputTracking;

        /// <inheritdoc />
        public override string Name => UxrConstants.SdkOculus;

        /// <inheritdoc />
        public override string MinimumUnityVersion => "2021.3";

        /// <inheritdoc />
        public override string[] AvailableSymbols
        {
            get
            {
                if (CurrentState == State.Available)
                {
                    if (CurrentVersion == 0)
                    {
                        return new[] { "ULTIMATEXR_USE_OCULUS_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_OCULUS_SDK" }; }
        }

        /// <inheritdoc />
        public override void TryLocate()
        {
            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.Android or BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64)
            {
                CurrentState = State.NotInstalled;

                if (IsTypeInAssemblies("OVRManager"))
                {
                    CurrentVersion = 0;
                    CurrentState   = State.Available;
                }
            }
            else
            {
                CurrentState = State.CurrentTargetNotSupported;
            }
        }

        /// <inheritdoc />
        public override void TryGet()
        {
            if (EditorUtility.DisplayDialog("Install integration?", $"Meta SDK integration asset needs to be installed. Proceed to the Asset Store?", UxrConstants.Editor.Yes, UxrConstants.Editor.Cancel))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/integration/meta-xr-core-sdk-269169");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorOvr());
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdksInputTracking + "Remove Symbols for Meta SDK", priority = UxrConstants.Editor.PriorityMenuPathSdksInputTracking)]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorOvr());
        }

        #endregion
    }
}