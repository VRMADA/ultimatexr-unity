// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrPrecacheable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Core.Caching
{
    /// <summary>
    ///     The <see cref="IUxrPrecacheable" /> interface is used in components that need to create instances at runtime and
    ///     want a way to precache them so that there aren't any hiccups on instantiation.
    ///     The <see cref="UxrManager" /> will look for <see cref="IUxrPrecacheable" /> components when the scene is loaded and
    ///     will instantiate and render the objects specified by <see cref="PrecachedInstances" /> a certain amount of frames
    ///     while the screen is still black.
    ///     This will make sure their resources (meshes, textures) are cached in order to minimize instantiation delays.
    /// </summary>
    public interface IUxrPrecacheable
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the GameObjects, usually prefabs, that will be precached when the scene is loaded.
        /// </summary>
        IEnumerable<GameObject> PrecachedInstances { get; }

        #endregion
    }
}