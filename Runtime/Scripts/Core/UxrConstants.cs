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

        public const int MajorVersion = 1;
        public const int MinorVersion = 0;
        public const int PatchVersion = 0;

        public const string UltimateXR = "UltimateXR";

#if ULTIMATEXR_PACKAGE
        public const string PackageName = "com.vrmada.ultimatexr-unity";
#endif

        public const string AnimationModule    = "<b>[UltimateXR.Animation]</b>";
        public const string AvatarModule       = "<b>[UltimateXR.Avatar]</b>";
        public const string CoreModule         = "<b>[UltimateXR.Core]</b>";
        public const string DevicesModule      = "<b>[UltimateXR.Devices]</b>";
        public const string LocomotionModule   = "<b>[UltimateXR.Locomotion]</b>";
        public const string ManipulationModule = "<b>[UltimateXR.Manipulation]</b>";
        public const string NetworkingModule   = "<b>[UltimateXR.Networking]</b>";
        public const string RenderingModule    = "<b>[UltimateXR.Rendering]</b>";
        public const string UiModule           = "<b>[UltimateXR.UI]</b>";
        public const string WeaponsModule      = "<b>[UltimateXR.Weapons]</b>";

        public static string Version => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";

        public static string MainCamera = "MainCamera";

        #endregion
    }
}