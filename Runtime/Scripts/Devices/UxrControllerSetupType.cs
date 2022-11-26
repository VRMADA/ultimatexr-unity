// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerSetupType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Devices
{
    /// <summary>
    ///     Enumerates the different controller setups supported by an <see cref="UxrControllerInput" /> component.
    /// </summary>
    public enum UxrControllerSetupType
    {
        /// <summary>
        ///     <para>
        ///         Single controller setup, such as a gamepad, remote or a gun. If
        ///         <see cref="IUxrControllerInput.IsHandednessSupported" /> is available,
        ///         <see cref="IUxrControllerInput.Handedness" /> allows to determine which hand is being used. In some devices,
        ///         such as a gamepad, <see cref="IUxrControllerInput.Handedness" /> is not applicable and thus not supported.
        ///     </para>
        ///     Some other devices, such as a remote, may use <see cref="IUxrControllerInput.Handedness" /> to help with tracking
        ///     and also display the correct hand if the virtual hands are rendered on the controller.
        /// </summary>
        Single,

        /// <summary>
        ///     Dual controller setup (left+right controllers). <see cref="IUxrControllerInput.Handedness" /> can be used to assign
        ///     a primary device and a secondary device and get input from the dominant and non-dominant hands respectively.
        /// </summary>
        Dual
    }
}