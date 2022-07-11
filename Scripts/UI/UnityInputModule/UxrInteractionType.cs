// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInteractionType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Enumerates the types of interaction supported.
    /// </summary>
    public enum UxrInteractionType
    {
        /// <summary>
        ///     Interaction using <see cref="UxrFingerTip" /> components attached to the finger tips of an <see cref="UxrAvatar" />
        ///     . Enables touch interaction.
        /// </summary>
        FingerTips,

        /// <summary>
        ///     Interaction using <see cref="UxrLaserPointer" /> components from a distance.
        /// </summary>
        LaserPointers
    }
}