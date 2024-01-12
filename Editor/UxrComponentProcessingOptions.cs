// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentProcessingOptions.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Enumerates the different options for <see cref="UxrEditorUtils.ModifyComponent{T}" />
    /// </summary>
    [Flags]
    public enum UxrComponentProcessingOptions
    {
        /// <summary>
        ///     No options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Recurse into children in the hierarchy.
        /// </summary>
        RecurseIntoChildren = 1 << 0,

        /// <summary>
        ///     Recurse into prefabs up in the prefab chain or only process those at the start level. For an instance this would be
        ///     the instance itself and for a prefab it would be the prefab itself.
        /// </summary>
        RecurseIntoPrefabs = 1 << 1,

        /// <summary>
        ///     Process components in the scene that do not come from an instantiated prefab.
        /// </summary>
        ProcessOriginalSceneComponents = 1 << 2,

        /// <summary>
        ///     Process components in the scene that come from an instantiated prefab.
        /// </summary>
        ProcessPrefabSceneComponents = 1 << 3,

        /// <summary>
        ///     Process all components in prefabs up in the hierarchy of prefabs if they are not in the prefab where the component
        ///     was originally added, the original source. This means components that are inherited from another prefab.
        /// </summary>
        ProcessNonOriginalPrefabComponents = 1 << 4,

        /// <summary>
        ///     Process all components in prefabs up in the hierarchy of prefabs only if they are in the original prefab where the
        ///     component was added. This means the component isn't inherited from another prefab.
        /// </summary>
        ProcessOriginalPrefabComponents = 1 << 5,

        /// <summary>
        ///     Process all components in UltimateXR folders.
        /// </summary>
        ProcessUltimateXRAssetComponents = 1 << 6,

        /// <summary>
        ///     All options.
        /// </summary>
        All = 0x7FFFFFF
    }
}