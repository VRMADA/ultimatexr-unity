// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    /// <summary>
    ///     Instantiation event args for <see cref="UxrInstanceManager" />.
    /// </summary>
    public class UxrInstanceEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the instance of the instantiation/destroy operation. Will be null on
        ///     <see cref="UxrInstanceManager.Instantiating" /> and
        ///     <see cref="UxrInstanceManager.Destroyed" /> events, since the instance will not exist.
        /// </summary>
        public GameObject Instance { get; }

        /// <summary>
        ///     Gets the prefab of the instantiation/destroy operation. Null when using
        ///     <see cref="UxrInstanceManager.InstantiateEmptyGameObject" />.
        /// </summary>
        public GameObject Prefab { get; }

        /// <summary>
        ///     Gets the prefab Id of the instantiation/destroy operation. The prefab Id is the Id assigned by Unity to the prefab
        ///     asset. Null when using <see cref="UxrInstanceManager.InstantiateEmptyGameObject" /> for instantiation.
        /// </summary>
        public string PrefabId { get; }

        #endregion

        #region Constructors & Finalizer

        /// <inheritdoc />
        public UxrInstanceEventArgs(GameObject instance, GameObject prefab, string prefabId)
        {
            Instance = instance;
            Prefab   = prefab;
            PrefabId = prefabId;
        }

        #endregion
    }
}