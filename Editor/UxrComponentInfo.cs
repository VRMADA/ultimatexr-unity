// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Used by <see cref="UxrComponentProcessor{T}" /> to send information about the component that needs to be processed.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    public class UxrComponentInfo<T> where T : Component
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the target component that needs to be processed.
        /// </summary>
        public T TargetComponent { get; }

        /// <summary>
        ///     Gets the prefab where the component that needs to be processed is located. It is null if the
        ///     <see cref="TargetComponent" /> being processed is in a scene.
        /// </summary>
        public GameObject TargetPrefab { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="component">Component to process</param>
        /// <param name="prefab">
        ///     Prefab to process. It is null if the <paramref name="component" /> being processed is in a scene
        /// </param>
        public UxrComponentInfo(T component, GameObject prefab)
        {
            TargetComponent = component;
            TargetPrefab    = prefab;
        }

        #endregion
    }
}