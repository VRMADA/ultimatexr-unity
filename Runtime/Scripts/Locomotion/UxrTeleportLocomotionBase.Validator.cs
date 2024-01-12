// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotionBase.Validator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Locomotion
{
    public abstract partial class UxrTeleportLocomotionBase
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores a destination validator function and its mode.
        /// </summary>
        private class Validator
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the validator function.
            /// </summary>
            public Func<UxrTeleportDestination, bool> ValidatorFunc { get; }

            /// <summary>
            ///     Gets the validator execution mode.
            /// </summary>
            public UxrDestinationValidatorMode Mode { get; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="validatorFunc">The validator function</param>
            /// <param name="mode">The validator execution mode</param>
            public Validator(Func<UxrTeleportDestination, bool> validatorFunc, UxrDestinationValidatorMode mode)
            {
                ValidatorFunc = validatorFunc;
                Mode          = mode;
            }

            #endregion
        }

        #endregion
    }
}