// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Math.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;
using UltimateXR.Extensions.System;

namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Math constants.
        /// </summary>
        public static class Math
        {
            #region Public Types & Data

            /// <summary>
            ///     Default precision threshold used by some functionality in the framework.
            ///     <see cref="UxrStateSaveImplementer{T}" /> for example, avoids serializing changes in specific types when they
            ///     didn't change using <see cref="ObjectExt.ValuesEqual(object,object,float)" />.
            /// </summary>
            public const float DefaultPrecisionThreshold = 0.0001f;

            #endregion
        }

        #endregion
    }
}