// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUniqueIdImplementer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UnityEngine;

namespace UltimateXR.Core.Unique
{
    /// <summary>
    ///     Base class for <see cref="UxrUniqueIdImplementer{T}" />.
    /// </summary>
    public abstract class UxrUniqueIdImplementer
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets all registered components that implement the <see cref="IUxrUniqueId" /> interface.
        ///     This includes all <see cref="UxrComponent" /> components and all user custom classes that implement
        ///     <see cref="IUxrUniqueId" /> using <see cref="UxrUniqueIdImplementer{T}" />.
        /// </summary>
        public static IEnumerable<IUxrUniqueId> AllComponents
        {
            get
            {
                foreach (KeyValuePair<Type, UxrUniqueIdImplementer> pair in s_implementerTypes)
                {
                    foreach (IUxrUniqueId unique in pair.Value.GetAllComponentsInternal())
                    {
                        yield return unique;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the unique Id used for combination by <see cref="UxrUniqueIdImplementer{T}.CombineUniqueId"/>.
        /// </summary>
        public Guid CombineIdSource { get; set; }

        /// <summary>
        ///     Gets or sets the original unique Id.
        /// </summary>
        public Guid OriginalUniqueId { get; set; }

        /// <summary>
        ///     Gets or sets the component that was initialized.
        /// </summary>
        public IUxrUniqueId InitializedComponent { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Generates a new unique Id.
        /// </summary>
        /// <returns>Unique Id</returns>
        public static Guid GetNewUniqueId()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        ///     Finds a component using its unique ID. The component should implement the <see cref="IUxrUniqueId" /> interface,
        ///     such
        ///     as <see cref="UxrComponent" />.
        /// </summary>
        /// <param name="id">Component's unique ID</param>
        /// <param name="component">Returns the component or null if the ID wasn't found</param>
        /// <returns>Whether the given ID was found and a component is returned</returns>
        public static bool TryGetComponentById(Guid id, out IUxrUniqueId component)
        {
            // Iterate over component types. By default, it will contain the UxrComponent implementer only.

            foreach (KeyValuePair<Type, UxrUniqueIdImplementer> pair in s_implementerTypes)
            {
                if (pair.Value.TryGetComponentByIdInternal(id, out component))
                {
                    return true;
                }
            }

            component = null;
            return false;
        }

        /// <summary>
        ///     Finds a component using its unique ID. The component should implement the <see cref="IUxrUniqueId" /> interface,
        ///     such
        ///     as <see cref="UxrComponent" />.
        /// </summary>
        /// <param name="id">Component's unique ID</param>
        /// <param name="component">Returns the component or null if the ID wasn't found</param>
        /// <returns>Whether the given ID was found and a component is returned</returns>
        public static bool TryGetComponentById<T>(Guid id, out T component) where T : Component, IUxrUniqueId
        {
            foreach (KeyValuePair<Type, UxrUniqueIdImplementer> pair in s_implementerTypes)
            {
                if (pair.Value.TryGetComponentByIdInternal(id, out IUxrUniqueId unique))
                {
                    component = unique as T;

                    if (component == null)
                    {
                        if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                        {
                            Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrUniqueIdImplementer)}.{nameof(TryGetComponentById)} type mismatch with ID {id}. Expected type is ({typeof(T).Name}) and actual type is ({unique.GetType().Name}).");
                        }

                        return false;
                    }
                }
            }

            component = default;
            return false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Tries to find a component using the unique ID in an <see cref="UxrUniqueIdImplementer{T}" /> implementation.
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="component">Returns the component</param>
        /// <returns></returns>
        protected abstract bool TryGetComponentByIdInternal(Guid id, out IUxrUniqueId component);

        /// <summary>
        ///     Returns all components of the type in an <see cref="UxrUniqueIdImplementer{T}" /> implementation.
        /// </summary>
        /// <returns>Returns all registered components of the type in an <see cref="UxrUniqueIdImplementer{T}" /> implementation</returns>
        protected abstract IEnumerable<IUxrUniqueId> GetAllComponentsInternal();

        /// <summary>
        ///     Registers an implementer type if it wasn't already registered. This allows to statically get components of custom
        ///     types based on their ID using <see cref="TryGetComponentById" />.
        ///     Also registers the implementer so that it can be retrieved using the component as key.
        /// </summary>
        /// <param name="targetComponent">The target component</param>
        /// <param name="implementer">The implementer</param>
        protected void RegisterImplementer<T>(T targetComponent, UxrUniqueIdImplementer<T> implementer) where T : Component, IUxrUniqueId
        {
            if (!s_implementerTypes.ContainsKey(typeof(T)))
            {
                s_implementerTypes.Add(typeof(T), implementer);
            }

            if (!s_allImplementers.ContainsKey(targetComponent))
            {
                s_allImplementers.Add(targetComponent, implementer);
            }
        }

        /// <summary>
        ///     Unregisters the implementer.
        /// </summary>
        /// <param name="targetComponent">The target component</param>
        /// <param name="implementer">The implementer</param>
        /// <typeparam name="T">The component type</typeparam>
        protected void UnregisterImplementer<T>(T targetComponent, UxrUniqueIdImplementer<T> implementer) where T : Component, IUxrUniqueId
        {
            s_allImplementers.Remove(targetComponent);
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Static dictionary of path collisions to ensure unique IDs are generated.
        /// </summary>
        protected static readonly Dictionary<Guid, int> s_idCollisions = new Dictionary<Guid, int>();

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Contains the first implementer registered for each type, including <see cref="UxrComponent" /> and any user custom
        ///     types.
        /// </summary>
        private static readonly Dictionary<Type, UxrUniqueIdImplementer> s_implementerTypes = new Dictionary<Type, UxrUniqueIdImplementer>();

        /// <summary>
        ///     Contains the implementer for each component.
        /// </summary>
        private static readonly Dictionary<IUxrUniqueId, UxrUniqueIdImplementer> s_allImplementers = new Dictionary<IUxrUniqueId, UxrUniqueIdImplementer>();

        #endregion
    }
}