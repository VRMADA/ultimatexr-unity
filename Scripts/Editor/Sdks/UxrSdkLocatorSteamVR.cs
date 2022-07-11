// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocatorSteamVR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Sdks
{
#if ULTIMATEXR_USE_STEAMVR_SDK
    using Valve.VR;
#endif
    /// <summary>
    ///     SDK Locator for the SteamVR SDK.
    /// </summary>
    public sealed class UxrSdkLocatorSteamVR : UxrSdkLocator
    {
        #region Public Overrides UxrSdkLocator

        /// <inheritdoc />
        public override string Name => UxrManager.SdkSteamVR;

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
                        return new[] { "ULTIMATEXR_USE_STEAMVR_SDK" };
                    }
                }

                return new string[0];
            }
        }

        /// <inheritdoc />
        public override string[] AllSymbols
        {
            get { return new[] { "ULTIMATEXR_USE_STEAMVR_SDK" }; }
        }

        /// <inheritdoc />
        public override bool CanBeUpdated => true;

        /// <inheritdoc />
        public override void TryLocate()
        {
#if UNITY_5_6_OR_NEWER

            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64)
            {
                CurrentState = State.NotInstalled;

                if (IsTypeInAssemblies("Valve.VR.SteamVR_Events"))
                {
                    CurrentVersion = 0;
                    CurrentState   = State.Available;
                }
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
            Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/steamvr-plugin-32647");
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
            UxrSdkManager.RegisterLocator(new UxrSdkLocatorSteamVR());
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
#if ULTIMATEXR_USE_STEAMVR_SDK
            if (SteamVR_Input.DoesActionsFileExist())
            {
                if (SteamVR_Input.actionFile != null && SteamVR_Input.actionFile.action_sets != null)
                {
                    bool needsSetup = SteamVRActionsExporter.NeedsActionsSetup();

                    if (!needsSetup)
                    {
                        EditorGUILayout.LabelField("Found UltimateXR actions. Input is available.");
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("UltimateXR actions need to be set up to use SteamVR input", MessageType.Warning);
                    }

                    bool guiEnabled = GUI.enabled;
                    GUI.enabled = guiEnabled && needsSetup;

                    if (UxrEditorUtils.CenteredButton(new GUIContent("Create Actions")))
                    {
                        SteamVRActionsExporter.TrySetupActions();
                    }

                    GUI.enabled = guiEnabled && !needsSetup;

                    if (UxrEditorUtils.CenteredButton(new GUIContent("Delete Actions")))
                    {
                        SteamVRActionsExporter.TryRemoveActions();
                    }

                    GUI.enabled = guiEnabled;
                }
                else
                {
                    if (SteamVR_Input.actionFile == null)
                    {
                        SteamVR_Input.InitializeFile(false, false);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("SteamVR actions have not been set up and need to be created first.\nNavigate to Unity's top menu Window->SteamVR Input to generate the actions. When finished continue setup here.", MessageType.Warning);
            }
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Allows to remove dependencies from the project in case the user removed SDK folders manually.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDKs/Remove Symbols for SteamVR")]
        private static void RemoveSymbols()
        {
            UxrSdkManager.RemoveSymbols(new UxrSdkLocatorSteamVR());
        }

        #endregion
    }
}