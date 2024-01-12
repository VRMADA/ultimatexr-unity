// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrNetworkSdk.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Networking
{
    /// <summary>
    ///     Interface for classes that implement network functionality using an SDK.
    /// </summary>
    public interface IUxrNetworkSdk
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the SDK implemented.
        /// </summary>
        string SdkName { get; }

        #endregion
    }
}