// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SteamVRActionsExporter.ButtonUsageFlags.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Editor.Sdks
{
    public static partial class SteamVRActionsExporter
    {
        #region Private Types & Data

        /// <summary>
        ///     Enumerates the possible button interaction types.
        /// </summary>
        [Flags]
        private enum ButtonUsageFlags
        {
            None  = 0,
            Click = 1,
            Touch = 1 << 1,
            All   = 0x7FFFFFFF
        }

        #endregion
    }
}