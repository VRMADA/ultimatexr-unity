// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrShotCycle.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Enumerates the supported firearm shot cycles.
    /// </summary>
    public enum UxrShotCycle
    {
        /// <summary>
        ///     Weapon requires a manual reload to fire the next round.
        /// </summary>
        ManualReload,

        /// <summary>
        ///     Weapon fires a single round each time the trigger is pressed. The next round requires to release the trigger and
        ///     press it again.
        /// </summary>
        SemiAutomatic,

        /// <summary>
        ///     Weapon keeps firing one round after another while the trigger is being pressed.
        /// </summary>
        FullyAutomatic
    }
}