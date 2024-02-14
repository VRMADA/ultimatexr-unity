// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrUniqueId.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.Unique
{
    /// <summary>
    ///     Interface for classes whose objects can be uniquely identified using an Id.<br />
    ///     To leverage the implementation of this interface, consider using <see cref="UxrUniqueIdImplementer{T}" />.<br />
    ///     <see cref="UxrUniqueIdImplementer.TryGetComponentById" /> can be used at runtime to get a component based on the
    ///     unique Id.<br />
    /// </summary>
    public interface IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the unique Id of the object.
        ///     Use <see cref="ChangeUniqueId" /> to change the unique ID at runtime.
        /// </summary>
        Guid UniqueId { get; }

        /// <summary>
        ///     Gets whether the unique ID of the object is generated based on the full type name.
        ///     This can be used, for example, to ensure singletons have the same Unique ID.
        /// </summary>
        bool UniqueIdIsTypeName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Registers the component, making sure that its Unique ID is available to exchange synchronization messages.
        ///     If the component was already registered, the call is ignored.
        ///     Components are registered automatically without user intervention, unless they are instantiated at runtime and
        ///     initially disabled.
        /// </summary>
        void RegisterUniqueIdIfNecessary();

        /// <summary>
        ///     Changes the object's unique Id, optionally changing all children recursively based on the
        ///     parent unique ID. This is useful in multiplayer environments to make sure that network
        ///     instantiated objects share the same ID, including all children.
        /// </summary>
        /// <param name="newUniqueId">New id</param>
        /// <param name="recursive">
        ///     Whether to change also the unique ID's of the components in the same GameObject and all children, based on the new
        ///     unique ID.
        /// </param>
        /// <returns>
        ///     The new unique ID. If the requested unique already existed, the returned value will be different
        ///     to make sure it is unique.
        /// </returns>
        Guid ChangeUniqueId(Guid newUniqueId, bool recursive = false);

        #endregion
    }
}