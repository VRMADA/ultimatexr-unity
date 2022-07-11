// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SteamVRActionsExporter.SideFlags.cs" company="VRMADA">
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
        ///     Enumerates the different sides supported by controller elements.
        /// </summary>
        [Flags]
        private enum SideFlags
        {
            None      = 0,
            Left      = 1,
            Right     = 1 << 1,
            BothSides = Left | Right
        }

        #endregion
    }
}