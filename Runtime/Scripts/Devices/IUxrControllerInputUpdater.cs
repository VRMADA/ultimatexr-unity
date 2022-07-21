// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrControllerInputUpdater.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Internal interface to be able to update controller states only from the UltimateXR assembly.
    /// </summary>
    internal interface IUxrControllerInputUpdater
    {
        #region Public Methods

        /// <summary>
        ///     Updates the input state.
        /// </summary>
        void UpdateInput();

        #endregion
    }
}