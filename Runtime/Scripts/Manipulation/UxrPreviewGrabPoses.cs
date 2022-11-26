// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPreviewGrabPoses.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Flags enumerating the different modes that can be used to preview the grab poses in the editor while an
    ///     <see cref="UxrGrabbableObject" /> is selected.
    /// </summary>
    [Flags]
    public enum UxrPreviewGrabPoses
    {
        /// <summary>
        ///     Don't preview the grab poses.
        /// </summary>
        DontShow = 0,

        /// <summary>
        ///     Preview the left grab poses.
        /// </summary>
        ShowLeftHand = 1,

        /// <summary>
        ///     Preview the right grab poses.
        /// </summary>
        ShowRightHand = 1 << 1,

        /// <summary>
        ///     Preview left and right grab poses.
        /// </summary>
        ShowBothHands = ShowLeftHand | ShowRightHand
    }
}