// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrArmOverExtendMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.IK
{
    /// <summary>
    ///     Enumerates the different solutions that can be used when an avatar with visible arms moves a hand farther than the
    ///     actual length of the arm.
    /// </summary>
    public enum UxrArmOverExtendMode
    {
        /// <summary>
        ///     Hand reach will be limited to what the actual arm length permits.
        /// </summary>
        LimitHandReach,

        /// <summary>
        ///     Stretch the forearm.
        /// </summary>
        ExtendForearm,

        /// <summary>
        ///     Stretch the upper arm.
        /// </summary>
        ExtendUpperArm,

        /// <summary>
        ///     Stretch both the upper arm and forearm.
        /// </summary>
        ExtendArm
    }
}