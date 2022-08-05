// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    /// <summary>
    ///     Static class containing common constants used across the framework.
    /// </summary>
    public static partial class UxrConstants
    {
        #region Public Types & Data

        public const int MajorVersion = 0;
        public const int MinorVersion = 8;
        public const int PatchVersion = 4;

        public const string CoreModule         = "Core";
        public const string LocomotionModule   = "Locomotion";
        public const string ManipulationModule = "Manipulation";
        public const string UiModule           = "UI";

        public const float TeleportTranslationSeconds = 0.2f;
        public const float TeleportRotationSeconds    = 0.1f;

        public static string Version => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";

        #endregion
    }
}