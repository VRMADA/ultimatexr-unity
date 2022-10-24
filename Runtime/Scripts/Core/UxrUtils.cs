// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUtils.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static class UxrUtils
    {
        #region Public Methods

        /// <summary>
        ///     Gets the opposite side.
        /// </summary>
        /// <param name="handSide">Side</param>
        /// <returns>Opposite side</returns>
        public static UxrHandSide GetOppositeSide(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;
        }

        #endregion
    }
}