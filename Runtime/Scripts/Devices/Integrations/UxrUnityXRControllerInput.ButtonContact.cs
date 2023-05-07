// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityXRControllerInput.ButtonContact.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices.Integrations
{
    public abstract partial class UxrUnityXRControllerInput
    {
        #region Private Types & Data

        /// <summary>
        ///     Types of button contact.
        /// </summary>
        protected enum ButtonContact
        {
            /// <summary>
            ///     Button press.
            /// </summary>
            Press,

            /// <summary>
            ///     Button contact without pressing.
            /// </summary>
            Touch
        }

        #endregion
    }
}