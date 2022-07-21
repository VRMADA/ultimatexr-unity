// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLoopMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Supported interpolation loop modes.
    /// </summary>
    public enum UxrLoopMode
    {
        /// <summary>
        ///     No looping.
        /// </summary>
        None,

        /// <summary>
        ///     Will start from the beginning again when reaching the end.
        /// </summary>
        Loop,

        /// <summary>
        ///     Will go back and forth from beginning to end.
        /// </summary>
        PingPong
    }
}