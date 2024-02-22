// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPrefabList.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    /// <summary>
    ///     List of user-defined instantiable prefabs used by <see cref="UxrInstanceManager" />.
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabList", menuName = "UltimateXR/Prefab List", order = 1)]
    public class UxrPrefabList : ScriptableObject
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private List<GameObject> _prefabList;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the prefab list.
        /// </summary>
        public IReadOnlyList<GameObject> PrefabList => _prefabList;

        #endregion
    }
}