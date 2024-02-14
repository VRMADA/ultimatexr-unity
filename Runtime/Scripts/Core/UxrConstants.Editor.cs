// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Editor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Contains constants used in the Unity Editor that need access from runtime scripts.
        /// </summary>
        public static class Editor
        {
            #region Public Types & Data

            public const string Yes     = "Yes";
            public const string No      = "No";
            public const string Ok      = "OK";
            public const string Cancel  = "Cancel";
            public const string Error   = "Error";
            public const string Warning = "Warning";
            public const string Confirm = "Confirm";

            public const string PropertyBehaviourEnabled = "m_Enabled";
            public const string PropertyObjectHideFlags  = "m_ObjectHideFlags";

            public const string MenuPathRoot                = "Tools/UltimateXR/";
            public const string MenuPathAvatar              = "Tools/UltimateXR/Avatar/";
            public const string MenuPathAddAvatar           = "Tools/UltimateXR/Avatar/Add built-in avatar to scene/";
            public const string MenuPathNetworking          = "Tools/UltimateXR/Networking/";
            public const string MenuPathSdks                = "Tools/UltimateXR/SDKs/";
            public const string MenuPathSdksInputTracking   = "Tools/UltimateXR/SDKs/Input Tracking/";
            public const string MenuPathSdksNetworking      = "Tools/UltimateXR/SDKs/Networking/";
            public const string MenuPathSdksNetworkingVoice = "Tools/UltimateXR/SDKs/Voice Over Network/";
            public const string MenuPathUtils               = "Tools/UltimateXR/Utils/";

            public const int PriorityMenuPathAvatar              = 1;
            public const int PriorityMenuPathNetworking          = 2;
            public const int PriorityMenuPathSdks                = 3;
            public const int PriorityMenuPathSdksInputTracking   = 4;
            public const int PriorityMenuPathSdksNetworking      = 6;
            public const int PriorityMenuPathSdksNetworkingVoice = 8;
            public const int PriorityMenuPathUtils               = 10;
            public const int PriorityMenuPathRoot                = 500;
            
            /// <summary>
            ///     Editor prefs key for whether automatic unique ID generation for <see cref="IUxrUnique" /> is performed in
            ///     OnValidate().
            /// </summary>
            public const string AutomaticIdGenerationPrefs = "UltimateXR.Editor.AutomaticUniqueIdGeneration";

            #endregion
        }

        #endregion
    }
}