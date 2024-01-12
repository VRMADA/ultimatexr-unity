// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleGroup.InitState.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.UI.UnityInputModule.Controls
{
    public partial class UxrToggleGroup
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates the possible initial states, telling which child is currently selected.
        /// </summary>
        public enum InitState
        {
            /// <summary>
            ///     Initial state is determined by the state at edit-time.
            /// </summary>
            DontChange,

            /// <summary>
            ///     First child is toggled on, the rest are toggled off.
            /// </summary>
            FirstChild
        }

        #endregion
    }
}