// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshExt.ExtractSubMeshOperation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Extensions.Unity.Render
{
    public static partial class MeshExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates possible mesh extraction algorithms.
        /// </summary>
        public enum ExtractSubMeshOperation
        {
            /// <summary>
            ///     Creates a new mesh copying all the mesh that is influenced by the bone or any of its children.
            /// </summary>
            BoneAndChildren,

            /// <summary>
            ///     Creates a new mesh copying all the mesh that is not influenced by the reference bone or any of its children.
            /// </summary>
            NotFromBoneOrChildren
        }

        #endregion
    }
}