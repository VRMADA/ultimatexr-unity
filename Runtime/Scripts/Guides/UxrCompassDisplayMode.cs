// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCompassDisplayMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Guides
{
    /// <summary>
    ///     Enumerates the different display modes for <see cref="UxrCompass" />.
    /// </summary>
    public enum UxrCompassDisplayMode
    {
        /// <summary>
        ///     Only the compass will be shown while the target is not in sight.
        /// </summary>
        OnlyCompass,

        /// <summary>
        ///     Show compass while the target is not in sight. Show location icon on top of target while the target is in sight.
        /// </summary>
        Location,

        /// <summary>
        ///     Show compass while the target is not in sight. Show grab icon on top of target while the target is in sight.
        /// </summary>
        Grab,

        /// <summary>
        ///     Show compass while the target is not in sight. Show look icon on top of target while the target is in sight.
        /// </summary>
        Look,

        /// <summary>
        ///     Show compass while the target is not in sight. Show use icon on top of target while the target is in sight.
        /// </summary>
        Use
    }
}