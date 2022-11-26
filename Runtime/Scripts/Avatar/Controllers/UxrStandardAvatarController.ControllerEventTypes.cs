// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStandardAvatarController.ControllerEventTypes.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Avatar.Controllers
{
    public partial class UxrStandardAvatarController
    {
        #region Private Types & Data

        /// <summary>
        ///     Flags that control which <see cref="UxrAnimationType" /> to process when processing events.
        /// </summary>
        [Flags]
        private enum ControllerEventTypes
        {
            /// <summary>
            ///     No types.
            /// </summary>
            None = 0,

            /// <summary>
            ///     All other animation types that are not <see cref="Grab" /> or <see cref="Point" />.
            /// </summary>
            Other = 1,

            /// <summary>
            ///     Events that are for the grab animation.
            /// </summary>
            Grab = 1 << 1,

            /// <summary>
            ///     Events that are for the finger pointing animation.
            /// </summary>
            Point = 1 << 2,

            /// <summary>
            ///     All event.
            /// </summary>
            All = ~None
        }

        #endregion
    }
}