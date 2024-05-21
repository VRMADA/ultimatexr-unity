// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateInterpolationVars.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Stores variables that can be interpolated during a frame.
    /// </summary>
    public struct UxrStateInterpolationVars
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the variables as a dictionary where the keys are the variable names and the values are tuples (oldValue,
        ///     newValue).
        /// </summary>
        public IDictionary<string, (object OldValue, object NewValue)> Values { get; internal set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="values">Values</param>
        public UxrStateInterpolationVars(IDictionary<string, (object OldValue, object NewValue)> values)
        {
            Values = values;
        }

        #endregion
    }
}