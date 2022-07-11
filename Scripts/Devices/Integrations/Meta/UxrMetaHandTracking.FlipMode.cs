// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMetaHandTracking.FlipMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices.Integrations.Meta
{
    public partial class UxrMetaHandTracking
    {
        #region Private Types & Data

        /// <summary>
        ///     Enumerates the different ways to retrieve an Oculus SDK Quaternion.
        /// </summary>
        private enum FlipMode
        {
            None,
            FlipX,
            FlipZ
        }

        #endregion
    }
}