// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStandardAvatarController.EventProcessing.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    public partial class UxrStandardAvatarController
    {
        #region Private Types & Data

        /// <summary>
        ///     Flags that control which actions to perform when processing events.
        /// </summary>
        [Flags]
        private enum EventProcessing
        {
            /// <summary>
            ///     Do nothing.
            /// </summary>
            None = 0,

            /// <summary>
            ///     Update the internal state.
            /// </summary>
            InternalVars = 1,

            /// <summary>
            ///     Execute the actions that change <see cref="Animator" /> variables.
            /// </summary>
            ExecuteActions = 2,

            /// <summary>
            ///     Everything.
            /// </summary>
            All = ~None
        }

        #endregion
    }
}