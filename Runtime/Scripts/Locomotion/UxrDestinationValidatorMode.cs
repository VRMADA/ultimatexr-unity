// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDestinationValidatorMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Enumerates the different ways a destination validator may run. It's used by methods such as
    ///     <see cref="UxrTeleportLocomotionBase.AddDestinationValidator" />.
    /// </summary>
    public enum UxrDestinationValidatorMode
    {
        /// <summary>
        ///     The destination validator will be executed every frame. For teleportation, for example, this means that the arc
        ///     will show the valid/invalid state.<br/>
        ///     This mode can be used when a more complex validation is required each frame.
        /// </summary>
        EveryFrame = 1,

        /// <summary>
        ///     The destination validator will be executed only when the user confirms the "move to destination". For
        ///     teleportation this means that even if the arc shows a valid state, when the user inputs the move action, the
        ///     destination validator may cancel if the validation returned false.<br/>
        ///     This mode can be used during tutorials to notify the user selected a wrong teleportation destination.
        /// </summary>
        OnConfirmationOnly = 2
    }
}