// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkManagerEditor.GrabbableObjectStats.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor.Networking
{
    public partial class UxrNetworkManagerEditor
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores stats of adding or removing components to physics-driven grabbable objects.
        /// </summary>
        private class GrabbableObjectStats
        {
            #region Public Types & Data

            public bool HasAny => InstanceComponentsProcessedCount > 0 || PrefabComponentsProcessedCount > 0;  
            public int  SceneCount;
            public int  InstanceComponentsProcessedCount;
            public int  InstanceComponentsAddedOrRemovedCount;
            public int  PrefabComponentsProcessedCount;
            public int  PrefabComponentsAddedOrRemovedCount;

            #endregion
        }

        #endregion
    }
}