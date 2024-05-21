// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrUniqueId.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Core.Unique
{
    /// <summary>
    ///     Interface for components that can be uniquely identified using an Id.<br />
    ///     To leverage the implementation of this interface, consider using <see cref="UxrUniqueIdImplementer{T}" />.<br />
    ///     <see cref="UxrUniqueIdImplementer.TryGetComponentById" /> can be used at runtime to get a component based on the
    ///     unique Id.<br />
    /// </summary>
    public interface IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the component unique Id.
        ///     Use <see cref="ChangeUniqueId" /> to change the unique ID at runtime.
        /// </summary>
        Guid UniqueId { get; }

        /// <summary>
        ///     Gets the Guid that was used as parameter for <see cref="CombineUniqueId" />. <see cref="Guid.Empty" /> if the
        ///     UniqueId was not combined.
        /// </summary>
        Guid CombineIdSource { get; }

        /// <summary>
        ///     Gets the prefab Id assigned by Unity if the component is in a prefab. If the component is in an instance in the
        ///     scene it gets the Id of the prefab it was originally instantiated from.
        /// </summary>
        string UnityPrefabId { get; }

        /// <summary>
        ///     Gets the Component. We consider a component a type that inherits from Behaviour instead of Component, which
        ///     simplifies some things like access to enabled or isActiveAndEnabled.
        /// </summary>
        Behaviour Component { get; }

        /// <summary>
        ///     Gets the GameObject.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        ///     Gets the Transform.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        ///     Gets whether the unique ID of the component is generated based on the full type name instead of randomly.
        ///     This can be used, for example, to ensure singletons have always the same Unique ID.
        /// </summary>
        bool UniqueIdIsTypeName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Registers the component, making sure that its Unique ID is available to exchange synchronization messages.
        ///     If the component was already registered, the call is ignored.
        ///     Components are registered automatically without user intervention, unless they are instantiated at runtime and
        ///     initially disabled. Use this method to register the component ahead of time when it's initially disabled if
        ///     necessary.
        /// </summary>
        void RegisterIfNecessary();

        /// <summary>
        ///     Unregisters the component, removing the Unique ID from the internal list. Components are unregistered manually,
        ///     use this method to unregister the ID ahead of time if necessary.    
        /// </summary>
        void Unregister();

        /// <summary>
        ///     Tries to change the component's unique Id.
        /// </summary>
        /// <param name="newUniqueId">New id</param>
        /// <returns>
        ///     The new unique ID. If the requested unique already existed, the returned value will be different
        ///     to make sure it is unique.
        /// </returns>
        Guid ChangeUniqueId(Guid newUniqueId);

        /// <summary>
        ///     Changes the component's unique Id by combining it with another Id, optionally also changing all other components in
        ///     the same GameObject and its children. The combination is a mathematical operation that will use the original Id and
        ///     the provided Id to generate a new Id.<br />
        ///     When instantiating a same prefab multiple times, for example a player prefab in a multiplayer environment, the
        ///     different instances require new Ids that have to be the same on all devices.
        ///     All instances share the same source Ids coming from the prefab, but will require different Ids to tell one from
        ///     another once instantiated. Furthermore, these Ids need to be the same on all devices to make sure they can be
        ///     synchronized correctly.
        ///     The combination operation will use a Guid to change the component, ensuring that by using the same ID on other
        ///     devices the resulting Ids will be the same. When using a recursive combination and the GameObject has multiple
        ///     components implementing the <see cref="IUxrUniqueId" /> interface, the result will be the same no matter which
        ///     component the combination gets called on.
        /// </summary>
        /// <param name="guid">Id to combine the the original Ids with</param>
        /// <param name="recursive">
        ///     Whether to also change the unique Ids of the components in the same GameObject and all children, based on the new
        ///     unique Id.
        /// </param>
        void CombineUniqueId(Guid guid, bool recursive = true);

        #endregion
    }
}