// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportTargetMaterialID.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Component that allows to specify which material ID needs to be changed by an <see cref="UxrTeleportTarget" /> when
    ///     different colors are used to indicate whether the teleport is currently valid or not.
    /// </summary>
    public class UxrTeleportTargetMaterialID : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int _materialID;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the Material ID.
        /// </summary>
        public int MaterialID
        {
            get => _materialID;
            set => _materialID = value;
        }

        #endregion
    }
}