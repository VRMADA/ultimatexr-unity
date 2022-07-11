// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerInput.ButtonFlags.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    public abstract partial class UxrControllerInput
    {
        #region Protected Types & Data

        /// <summary>
        ///     Enumerates the different button flags representing button states. Flags are described by
        ///     <see cref="UxrInputButtons" />.
        /// </summary>
        protected enum ButtonFlags
        {
            /// <summary>
            ///     Touch state flags for the left controller.
            /// </summary>
            TouchFlagsLeft,

            /// <summary>
            ///     Press state flags for the left controller.
            /// </summary>
            PressFlagsLeft,

            /// <summary>
            ///     Touch state flags for the right controller.
            /// </summary>
            TouchFlagsRight,

            /// <summary>
            ///     Press state flags for the right controller.
            /// </summary>
            PressFlagsRight
        }

        #endregion
    }
}