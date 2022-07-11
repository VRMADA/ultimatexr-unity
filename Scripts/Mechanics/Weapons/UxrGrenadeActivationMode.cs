// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrenadeActivationMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Enumerates the different modes to activate a grenade.
    /// </summary>
    public enum UxrGrenadeActivationMode
    {
        /// <summary>
        ///     Grenade has no activation mode.
        /// </summary>
        NoActivation,

        /// <summary>
        ///     Grenade requires to remove a pin to activate a detonation timer.
        /// </summary>
        TriggerPin,

        /// <summary>
        ///     A detonator timer is started after being launched.
        /// </summary>
        OnHandLaunch,
    }
}