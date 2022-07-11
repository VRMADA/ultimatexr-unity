// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSteamVRConstants.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices.Integrations.SteamVR
{
    /// <summary>
    ///     SteamVR constants needed across our framework. At runtime and also for editor classes.
    /// </summary>
    public static class UxrSteamVRConstants
    {
        #region Public Types & Data

        public const string ActionSetName               = "ultimatexr";
        public const string ActionNameHandSkeletonLeft  = "hand_left_skeleton";
        public const string ActionNameHandSkeletonRight = "hand_right_skeleton";
        public const string ActionNameHandHaptics       = "hand_haptics";
        public const string BindingInputClick           = "click";
        public const string BindingInputTouch           = "touch";
        public const string BindingVarBool              = "boolean";
        public const string BindingVarVector1           = "vector1";
        public const string BindingVarVector2           = "vector2";

        #endregion
    }
}