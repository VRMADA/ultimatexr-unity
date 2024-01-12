// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleControlInput.InitState.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.UI.UnityInputModule.Controls
{
    public partial class UxrToggleControlInput
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates the different initial states of the toggle control.
        /// </summary>
        public enum InitState
        {
            /// <summary>
            ///     Initially toggled off.
            /// </summary>
            ToggledOff = 0,

            /// <summary>
            ///     Initially toggled on.
            /// </summary>
            ToggledOn = 1,

            /// <summary>
            ///     Don't change the toggle.
            /// </summary>
            DontChange = 2
        }

        #endregion
    }
}