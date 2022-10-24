// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFlipbookFinishedAction.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     What should be done when a <see cref="UxrAnimatedTextureFlipbook" /> animation finished. This is only supported
    ///     with <see cref="UxrFlipbookAnimationMode.SingleSequence" />.
    /// </summary>
    public enum UxrFlipbookFinishedAction
    {
        /// <summary>
        ///     Nothing happens when the animation finished.
        /// </summary>
        DoNothing,

        /// <summary>
        ///     After showing the last frame, the renderer is disabled.
        /// </summary>
        DisableRenderer,

        /// <summary>
        ///     After showing the last frame, the GameObject the component is attached to is disabled.
        /// </summary>
        DisableGameObject,

        /// <summary>
        ///     After showing the last frame, the GameObject the component is attached to is destroyed.
        /// </summary>
        DestroyGameObject
    }
}