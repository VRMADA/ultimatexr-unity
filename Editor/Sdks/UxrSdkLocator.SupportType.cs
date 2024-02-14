// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocator.Type.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor.Sdks
{
    public abstract partial class UxrSdkLocator
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates the different SDK types.
        /// </summary>
        public enum SupportType
        {
            /// <summary>
            ///     Input/Tracking devices.
            /// </summary>
            InputTracking,

            /// <summary>
            ///     Networking support.
            /// </summary>
            Networking,

            /// <summary>
            ///     Voice over network support.
            /// </summary>
            VoiceOverNetwork
        }

        #endregion
    }
}