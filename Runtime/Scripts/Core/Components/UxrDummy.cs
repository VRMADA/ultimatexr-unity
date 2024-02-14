// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDummy.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Unique;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Minimal <see cref="UxrComponent" /> component that can be used to identify GameObjects that don't have any other
    ///     <see cref="UxrComponent" />.
    ///     This can be useful in multiplayer environments to reference objects. For example, an avatar might be parented to a
    ///     moving platform and this operation needs to be executed in other client PCs.
    ///     If there is no reference to this platform (it might be the result of performing a raycast and not because a script
    ///     has a reference to it, for instance), there needs to be a way to get the same GameObject in the other PCS.
    ///     Since <see cref="UxrComponent" /> has a way to identify and obtain components (<see cref="UxrComponent.UniqueId" />
    ///     and <see cref="UxrUniqueIdImplementer.TryGetComponentById" />), a simple way to send references through the network
    ///     is by using a <see cref="UxrDummy" /> component on the GameObject.
    /// </summary>
    public sealed class UxrDummy : UxrComponent
    {
    }
}