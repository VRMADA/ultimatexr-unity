// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTweenFinishedActions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Different actions that can be executed once a <see cref="UxrTween" /> animation finished.
    /// </summary>
    [Flags]
    public enum UxrTweenFinishedActions
    {
        /// <summary>
        ///     No action.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Restores the original animated value that the component had before the animation.
        /// </summary>
        RestoreOriginalValue = 1 << 0,

        /// <summary>
        ///     Disable the component that the <see cref="UxrTween" /> is animating.
        /// </summary>
        DisableTargetComponent = 1 << 1,

        /// <summary>
        ///     Deactivate the <see cref="GameObject" /> where the component is located.
        /// </summary>
        DeactivateGameObject = 1 << 2,

        /// <summary>
        ///     Destroy the <see cref="UxrTween" /> component.
        /// </summary>
        DestroyTween = 1 << 3,

        /// <summary>
        ///     Destroy the component that the tween is animating.
        /// </summary>
        DestroyTargetComponent = 1 << 4,

        /// <summary>
        ///     Destroy the <see cref="GameObject" /> where the component is located.
        /// </summary>
        DestroyGameObject = 1 << 5
    }
}