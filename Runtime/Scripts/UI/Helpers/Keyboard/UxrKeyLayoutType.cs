// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyLayoutType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.UI.Helpers.Keyboard
{
    /// <summary>
    ///     Enumerates the different layouts of labels in a keyboard key.
    /// </summary>
    public enum UxrKeyLayoutType
    {
        /// <summary>
        ///     Single character label.
        /// </summary>
        SingleChar,

        /// <summary>
        ///     Key supports multiple outputs depending on shift/alt gr and has multiple labels because of that.
        /// </summary>
        MultipleChar
    }
}