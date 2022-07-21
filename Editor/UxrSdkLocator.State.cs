// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocator.State.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor
{
    public abstract partial class UxrSdkLocator
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates the different SDK states.
        /// </summary>
        public enum State
        {
            /// <summary>
            ///     Not initialized.
            /// </summary>
            Unknown,

            /// <summary>
            ///     SDK needs a higher Unity version to work.
            /// </summary>
            NeedsHigherUnityVersion,

            /// <summary>
            ///     SDK does not support the current build target.
            /// </summary>
            CurrentTargetNotSupported,

            /// <summary>
            ///     SDK is not installed.
            /// </summary>
            NotInstalled,
            
            /// <summary>
            ///     SDK is registered but support will come soon.
            /// </summary>
            SoonSupported,
            
            /// <summary>
            ///     SDK is installed and available.
            /// </summary>
            Available
        }

        #endregion
    }
}