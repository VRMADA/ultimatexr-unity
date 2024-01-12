// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkManager.GrabbablePhysicsSetup.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Networking
{
    public partial class UxrNetworkManager
    {
        #region Public Types & Data

        /// <summary>
        ///     Stores information of NetworkTransform components that were added to synchronize <see cref="UxrGrabbableObject" />
        ///     with rigidbodies.
        /// </summary>
        [Serializable]
        public class GrabbablePhysicsSetup
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private string                   _sdkUsed;
            [SerializeField] private List<string>             _processedScenePaths = new List<string>();
            [SerializeField] private bool                     _processedPathPrefabs;
            [SerializeField] private string                   _processedRootPath;
            [SerializeField] private List<UxrGrabbableObject> _processedPrefabs = new List<UxrGrabbableObject>();
            [SerializeField] private List<string>             _debugInfoLines   = new List<string>();

            #endregion
        }

        #endregion
    }
}