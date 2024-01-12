// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGlobalSettings.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Core.Settings
{
    /// <summary>
    ///     UltimateXR global settings. The global settings is stores in a file called UxrGlobalSettings inside a /Resources
    ///     folder so that it can be loaded at runtime.
    ///     It can be accessed using the Tools->UltimateXR Unity menu.
    /// </summary>
    public class UxrGlobalSettings : ScriptableObject
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrLogLevel _logLevelAnimation    = UxrLogLevel.Warnings;
        [SerializeField] private UxrLogLevel _logLevelAvatar       = UxrLogLevel.Warnings;
        [SerializeField] private UxrLogLevel _logLevelCore         = UxrLogLevel.Relevant;
        [SerializeField] private UxrLogLevel _logLevelDevices      = UxrLogLevel.Relevant;
        [SerializeField] private UxrLogLevel _logLevelLocomotion   = UxrLogLevel.Relevant;
        [SerializeField] private UxrLogLevel _logLevelManipulation = UxrLogLevel.Relevant;
        [SerializeField] private UxrLogLevel _logLevelNetworking   = UxrLogLevel.Warnings;
        [SerializeField] private UxrLogLevel _logLevelUI           = UxrLogLevel.Warnings;
        [SerializeField] private UxrLogLevel _logLevelWeapons      = UxrLogLevel.Relevant;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the global settings.
        /// </summary>
        /// <remarks>
        ///     Global settings must be stored in a file called UxrGlobalSettings inside a /Resources folder.
        ///     If no global settings could be found, default settings are used
        /// </remarks>
        public static UxrGlobalSettings Instance
        {
            get
            {
                if (s_current == null)
                {
                    s_current = Resources.Load<UxrGlobalSettings>(nameof(UxrGlobalSettings));

                    if (s_current == null)
                    {
                        s_current = CreateInstance<UxrGlobalSettings>();
                    }
                }

                return s_current;
            }
        }

        /// <summary>
        ///     Gets or sets the log level for animation events.
        /// </summary>
        public UxrLogLevel LogLevelAnimation
        {
            get => _logLevelAnimation;
            set => _logLevelAnimation = value;
        }

        /// <summary>
        ///     Gets or sets the log level for avatar events.
        /// </summary>
        public UxrLogLevel LogLevelAvatar
        {
            get => _logLevelAvatar;
            set => _logLevelAvatar = value;
        }

        /// <summary>
        ///     Gets or sets the log level for core events.
        /// </summary>
        public UxrLogLevel LogLevelCore
        {
            get => _logLevelCore;
            set => _logLevelCore = value;
        }

        /// <summary>
        ///     Gets or sets the log level for controller input device events.
        /// </summary>
        public UxrLogLevel LogLevelDevices
        {
            get => _logLevelDevices;
            set => _logLevelDevices = value;
        }

        /// <summary>
        ///     Gets or sets the log level for locomotion events.
        /// </summary>
        public UxrLogLevel LogLevelLocomotion
        {
            get => _logLevelLocomotion;
            set => _logLevelLocomotion = value;
        }
        
        /// <summary>
        ///     Gets or sets the log level for manipulation events.
        /// </summary>
        public UxrLogLevel LogLevelManipulation
        {
            get => _logLevelManipulation;
            set => _logLevelManipulation = value;
        }

        /// <summary>
        ///     Gets or sets the log level for networking events.
        /// </summary>
        public UxrLogLevel LogLevelNetworking
        {
            get => _logLevelNetworking;
            set => _logLevelNetworking = value;
        }

        /// <summary>
        ///     Gets or sets the log level for UI events.
        /// </summary>
        public UxrLogLevel LogLevelUI
        {
            get => _logLevelUI;
            set => _logLevelUI = value;
        }

        /// <summary>
        ///     Gets or sets the log level for weapon events.
        /// </summary>
        public UxrLogLevel LogLevelWeapons
        {
            get => _logLevelWeapons;
            set => _logLevelWeapons = value;
        }

        #endregion

        #region Public Methods

#if UNITY_EDITOR

        /// <summary>
        ///     Accesses the global settings using the Unity menu.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathRoot + "Global Settings", priority = UxrConstants.Editor.PriorityMenuPathRoot)]
        public static void SelectInProject()
        {
            UxrGlobalSettings settings = Resources.Load<UxrGlobalSettings>(nameof(UxrGlobalSettings));

            if (settings == null)
            {
                if (EditorUtility.DisplayDialog("Create global settings?", "Global settings file has not yet been created. Create one now?", "Yes", "Cancel"))
                {
                    settings = CreateInstance<UxrGlobalSettings>();

                    string resourcesFolder = "Resources";
                    string directory       = $"{Application.dataPath}/{resourcesFolder}/";

                    if (!File.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    AssetDatabase.CreateAsset(settings, $"Assets/{resourcesFolder}/{nameof(UxrGlobalSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    return;
                }
            }

            Selection.activeObject = settings;
        }
#endif

        #endregion

        #region Private Types & Data

        private static UxrGlobalSettings s_current;

        #endregion
    }
}