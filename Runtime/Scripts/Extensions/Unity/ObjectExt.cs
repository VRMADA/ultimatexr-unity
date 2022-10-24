// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     Unity <see cref="Object" /> extensions
    /// </summary>
    public static class ObjectExt
    {
        #region Public Methods

        /// <summary>
        ///     Controls whether to show the current object in the inspector.
        /// </summary>
        /// <param name="self">The object to show</param>
        /// <param name="show">Whether to show the object or now</param>
        public static void ShowInInspector(this Object self, bool show = true)
        {
            self.ShowInInspector(show, show);
        }

        /// <summary>
        ///     Controls whether to show the current object in the inspector and whether it is editable.
        /// </summary>
        /// <param name="self">The object to set</param>
        /// <param name="show">Whether to show it in the inspector</param>
        /// <param name="editable">Whether it is editable</param>
        public static void ShowInInspector(this Object self, bool show, bool editable)
        {
            if (show)
            {
                self.hideFlags &= ~HideFlags.HideInInspector;
            }
            else
            {
                self.hideFlags |= HideFlags.HideInInspector;
            }

            if (editable)
            {
                self.hideFlags &= ~HideFlags.NotEditable;
            }
            else
            {
                self.hideFlags |= HideFlags.NotEditable;
            }
        }

        #endregion
    }
}