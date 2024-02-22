// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;
using UltimateXR.Core.Unique;
using UnityEngine;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Minimal <see cref="UxrComponent" /> component that can be used to identify GameObjects that don't have any other
    ///     <see cref="UxrComponent" />. Additionally, it can control which Transform to sync, if any.
    ///     This can be useful in multiplayer environments to reference objects. For example, an avatar might be parented to a
    ///     moving platform and this operation needs to be executed in other client PCs.
    ///     If there is no reference to this platform (it might be the result of performing a raycast and not because a script
    ///     has a reference to it, for instance), there needs to be a way to get the same GameObject in the other PCS.
    ///     Since <see cref="UxrComponent" /> has a way to identify and obtain components (<see cref="UxrComponent.UniqueId" />
    ///     and <see cref="UxrUniqueIdImplementer.TryGetComponentById" />), a simple way to send references through the network
    ///     is by using a <see cref="UxrSyncObject" /> component on the GameObject.
    /// </summary>
    public class UxrSyncObject : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool              _syncTransform;
        [SerializeField] private UxrTransformSpace _transformSpace = UxrTransformSpace.World;

        #endregion

        #region Public Overrides UxrComponent

        /// <inheritdoc />
        public override UxrTransformSpace TransformStateSaveSpace => _transformSpace;

        /// <inheritdoc />
        public override bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            return _syncTransform;
        }

        #endregion
    }
}