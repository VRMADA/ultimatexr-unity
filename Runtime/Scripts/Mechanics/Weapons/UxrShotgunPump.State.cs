// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrShotgunPump.State.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Mechanics.Weapons
{
    public partial class UxrShotgunPump
    {
        #region Private Types & Data

        /// <summary>
        ///     Enumerates the different states in a <see cref="UxrShotgunPump" /> component.
        /// </summary>
        private enum State
        {
            /// <summary>
            ///     Waiting for the pump action in the first direction.
            /// </summary>
            WaitPump,

            /// <summary>
            ///     Waiting for the pump action back.
            /// </summary>
            WaitPumpBack
        }

        #endregion
    }
}