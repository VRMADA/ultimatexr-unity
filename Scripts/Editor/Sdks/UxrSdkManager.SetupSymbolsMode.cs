// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkManager.SetupSymbolsMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor.Sdks
{
    public static partial class UxrSdkManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Enumerates the different operations on the SDK preprocessor symbols.
        /// </summary>
        private enum SetupSymbolsMode
        {
            /// <summary>
            ///     Adds or removes the preprocessor symbols depending on whether the SDK is present.
            /// </summary>
            AddOrRemove,

            /// <summary>
            ///     Removes the symbols.
            /// </summary>
            ForceRemove
        }

        #endregion
    }
}