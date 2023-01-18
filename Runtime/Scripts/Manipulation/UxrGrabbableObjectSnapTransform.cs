// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectSnapTransform.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Component used internally by the editor. It renders grab pose meshes in the Scene Window when a grab point's snap
    ///     transform is selected and moved/rotated.
    ///     It also allows to modify the blend factor in blend poses and gives access to some handy repositioning tools.
    /// </summary>
    public class UxrGrabbableObjectSnapTransform : UxrComponent
    {
        #region Unity

        /// <summary>
        ///     Makes sure to hide the GameObject initially during play mode when working from the editor.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Application.isEditor && Application.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }
}