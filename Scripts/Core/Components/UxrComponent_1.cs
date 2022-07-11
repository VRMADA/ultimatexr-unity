// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponent_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Like <see cref="UxrComponent"/> but it allows to enumerate all components of a specific type.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <remarks>
    ///     Components get registered through their Awake() call. This means that components get registered
    ///     the first time they are enabled. Disabled objects that have been enabled at some point are enumerated, but objects
    ///     that have never been enabled don't get enumerated, which means that they will not appear in
    ///     <see cref="AllComponents" />.
    /// </remarks>
    /// <seealso cref="UxrAvatarComponent{T}" />
    public abstract class UxrComponent<T> : UxrComponent where T : UxrComponent<T>
    {
        #region Public Types & Data

        /// <summary>
        ///     Called before registering a component.
        /// </summary>
        public new static event Action<T> GlobalRegistering;

        /// <summary>
        ///     Called when a component was registered.
        /// </summary>
        public new static event Action<T> GlobalRegistered;

        /// <summary>
        ///     Called before unregistering a component.
        /// </summary>
        public new static event Action<T> GlobalUnregistering;

        /// <summary>
        ///     Called when a component was unregistered.
        /// </summary>
        public new static event Action<T> GlobalUnregistered;

        /// <summary>
        ///     Called when a component was enabled.
        /// </summary>
        public new static event Action<T> GlobalEnabled;

        /// <summary>
        ///     Called when a component was disabled.
        /// </summary>
        public new static event Action<T> GlobalDisabled;

        /// <summary>
        ///     Gets all the components of this specific type, enabled or not, in all open scenes.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="GameObject.GetComponentsInChildren{T}(bool)" /> or
        ///     <see cref="UnityEngine.Object.FindObjectsOfType{T}(bool)" />.
        /// </remarks>
        public new static IReadOnlyList<T> AllComponents => s_typeComponents;

        /// <summary>
        ///     Gets all components of this specific type that are enabled, in all open scenes.
        /// </summary>
        public new static IEnumerable<T> EnabledComponents
        {
            get
            {
                foreach (T component in s_typeComponents)
                {
                    if (component.enabled && component.gameObject.activeInHierarchy)
                    {
                        yield return component;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sorts the internal list of components. This is useful if iterating over the components requires a certain order.
        /// </summary>
        /// <param name="comparison">Comparison to use for sorting</param>
        public static void SortComponents(Comparison<T> comparison)
        {
            s_typeComponents.Sort(comparison);
        }

        /// <summary>
        ///     Destroys all components.
        /// </summary>
        public new static void DestroyAllComponents()
        {
            foreach (T component in s_typeComponents)
            {
                Destroy(component);
            }
        }

        /// <summary>
        ///     Destroys all gameObjects the components belong to.
        /// </summary>
        public new static void DestroyAllGameObjects()
        {
            foreach (T component in s_typeComponents)
            {
                Destroy(component.gameObject);
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Registers itself in the static list of components.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            OnRegistering();
            s_typeComponents.Add((T)this);
            OnRegistered();
        }

        /// <summary>
        ///     Removes itself from the static list of components.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUnregistering();
            s_typeComponents.Remove((T)this);
            OnUnregistered();
        }

        /// <summary>
        ///     Triggers enabled events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            OnEnabled();
        }

        /// <summary>
        ///     Triggers disabled events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            OnDisabled();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     <see cref="GlobalRegistering" /> event trigger.
        /// </summary>
        private void OnRegistering()
        {
            GlobalRegistering?.Invoke(this as T);
        }

        /// <summary>
        ///     <see cref="GlobalRegistered" /> event trigger.
        /// </summary>
        private void OnRegistered()
        {
            GlobalRegistered?.Invoke(this as T);
        }

        /// <summary>
        ///     <see cref="GlobalUnregistering" /> event trigger.
        /// </summary>
        private void OnUnregistering()
        {
            GlobalUnregistering?.Invoke(this as T);
        }

        /// <summary>
        ///     <see cref="GlobalUnregistered" /> event trigger.
        /// </summary>
        private void OnUnregistered()
        {
            GlobalUnregistered?.Invoke(this as T);
        }

        /// <summary>
        ///     <see cref="GlobalEnabled" /> event trigger.
        /// </summary>
        private void OnEnabled()
        {
            GlobalEnabled?.Invoke(this as T);
        }

        /// <summary>
        ///     <see cref="GlobalDisabled" /> event trigger.
        /// </summary>
        private void OnDisabled()
        {
            GlobalDisabled?.Invoke(this as T);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Static list containing all registered components of this type.
        /// </summary>
        private static readonly List<T> s_typeComponents = new List<T>();

        #endregion
    }
}