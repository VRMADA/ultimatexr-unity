// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlyAttribute.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Attributes
{
    /// <summary>
    ///     Attribute to visualize inspector fields as read-only so that they can't be edited.
    ///     This can be used for debugging purposes, to expose component information to the user but without the possibility to
    ///     edit the data. It also provides additional functionality:
    ///     <list type="bullet">
    ///         <item>Make the field read-only during play mode but editable during edit mode.</item>
    ///         <item>Hide the field during edit-mode.</item>
    ///     </list>
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        #region Public Types & Data

        /// <summary>
        ///     Whether to apply the read-only mode only while playing.
        /// </summary>
        public bool OnlyWhilePlaying { get; set; } = false;

        /// <summary>
        ///     Whether to hide the variable during edit-mode.
        /// </summary>
        public bool HideInEditMode { get; set; } = false;

        #endregion
    }
}