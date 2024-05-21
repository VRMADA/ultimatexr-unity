// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUniqueIdImplementer_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Core.Unique
{
    /// <summary>
    ///     Helper class simplifying the implementation of the <see cref="IUxrUniqueId" /> interface.
    ///     This class includes functionality to leverage the generation of unique IDs and to keep track of
    ///     all components using their ID. Any component can be retrieved using the ID only.
    ///     In scenarios where custom classes cannot inherit from <see cref="UxrComponent" /> to benefit from
    ///     these unique ID capabilities, this class is designed to implement the interface.
    /// </summary>
    public class UxrUniqueIdImplementer<T> : UxrUniqueIdImplementer where T : Component, IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the global dictionary where all components of type T are indexed by their id.
        /// </summary>
        public static IReadOnlyDictionary<Guid, T> ComponentsById => s_componentsById;

        /// <summary>
        ///     Gets whether the component was unregistered using <see cref="Unregister"/>.
        /// </summary>
        public bool IsUnregistered { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="targetComponent">Target component for all the methods called on this object</param>
        public UxrUniqueIdImplementer(T targetComponent)
        {
            _targetComponent = targetComponent;
            RegisterImplementer(targetComponent, this);
        }

        /// <summary>
        ///     Default constructor is private to use public constructor with target component.
        /// </summary>
        private UxrUniqueIdImplementer()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the component has been registered, and registers it if not.
        ///     The main reason why it exists is to make sure that it has been called if ChangeUniqueId()
        ///     changes the ID. Since this might happen right after instantiation, before Awake() gets called at
        ///     the end of the frame, it might need to be called there before.
        /// </summary>
        /// <param name="component">Component to initialize</param>
        /// <param name="getImplementer">A function that gets the implementer from a component</param>
        /// <param name="assignId">
        ///     An action that allows to assign a unique ID to a component. This avoids exposing the
        ///     unique ID as public.
        /// </param>
        /// <param name="onChanging">
        ///     An optional delegate that will be called right before assigning a new ID to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onChanged">
        ///     An optional delegate that will be called right after a new ID has been assigned to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onRegistering">
        ///     An optional delegate that will be called right before registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="onRegistered">
        ///     An optional delegate that will be called right after registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="defaultGuid">
        ///     The unique Id to assign if the unique ID is not initialized.
        ///     This parameter will be ignored if the component uses <see cref="IUxrUniqueId.UniqueIdIsTypeName" />.
        ///     If the parameter is the default value, it will generate a new Id based on the unique scene path.
        /// </param>
        public void InitializeUniqueIdIfNecessary(T                               component,
                                                  Func<T, UxrUniqueIdImplementer> getImplementer,
                                                  Action<T, Guid>                 assignId,
                                                  Action<T, Guid, Guid>           onChanging    = null,
                                                  Action<T, Guid, Guid>           onChanged     = null,
                                                  Action<T>                       onRegistering = null,
                                                  Action<T>                       onRegistered  = null,
                                                  Guid                            defaultGuid   = default)
        {
            UxrUniqueIdImplementer implementer = getImplementer(component);

            if (ReferenceEquals(implementer.InitializedComponent, component))
            {
                return;
            }

            // Not yet generated and serialized?

            if (component.UniqueId == default)
            {
                if (component.UniqueIdIsTypeName)
                {
                    // Unique ID will be based on the type name. Good for singletons. 
                    assignId(component, component.GetType().FullName.GetGuid());
                }
                else if (defaultGuid != default)
                {
                    // Assign user-defined Guid. If it causes collisions, these will be handled in RegisterUniqueId().
                    assignId(component, defaultGuid);
                }
                else
                {
                    // Fallback: Generate ID using unique scene path.
                    // Warning: This is not 100% safe for several reasons:
                    //   -In Unity root GameObjects don't have a sibling index at runtime
                    //   -Index in root GameObjects is not consistent across platforms
                    //   -Can create collisions when instantiating, but these are taken care of in RegisterUniqueId().
                    assignId(component, component.GetUniqueScenePath().GetGuid());
                }
            }

            // Store original unique ID
            implementer.OriginalUniqueId = component.UniqueId;

            // Register, taking care of collisions
            RegisterUniqueId(component, assignId, onChanging, onChanged, onRegistering, onRegistered, component.UniqueId);
            implementer.InitializedComponent = component;
        }

        /// <summary>
        ///     Tries to change the object's unique Id, ensuring no collisions with an existing Id.
        /// </summary>
        /// <param name="newUniqueId">New id to try to assign</param>
        /// <param name="getImplementer">A function that gets the implementer from a component</param>
        /// <param name="assignId">
        ///     An action that allows to assign a unique ID to a component. This avoids exposing the
        ///     unique ID as public.
        /// </param>
        /// <param name="onChanging">
        ///     An optional delegate that will be called right before assigning a new ID to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onChanged">
        ///     An optional delegate that will be called right after a new ID has been assigned to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onRegistering">
        ///     An optional delegate that will be called right before registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="onRegistered">
        ///     An optional delegate that will be called right after registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <returns>
        ///     The new unique ID. If the requested unique already existed, the returned value will be different
        ///     to make sure it is unique
        /// </returns>
        public Guid ChangeUniqueId(Guid                            newUniqueId,
                                   Func<T, UxrUniqueIdImplementer> getImplementer,
                                   Action<T, Guid>                 assignId,
                                   Action<T, Guid, Guid>           onChanging    = null,
                                   Action<T, Guid, Guid>           onChanged     = null,
                                   Action<T>                       onRegistering = null,
                                   Action<T>                       onRegistered  = null)
        {
            // If called during edit-time, simply generate unique IDs

            if (!Application.isPlaying)
            {
                assignId.Invoke(_targetComponent, GetNewUniqueId());
                return _targetComponent.UniqueId;
            }

            // At runtime, generate new ID for the component.

            // Make sure original ID has been initialized.
            InitializeUniqueIdIfNecessary(_targetComponent, getImplementer, assignId, onChanging, onChanged, onRegistering, onRegistered, newUniqueId);

            // Register new ID.
            RegisterUniqueId(_targetComponent, assignId, onChanging, onChanged, onRegistering, onRegistered, newUniqueId);

            return _targetComponent.UniqueId;
        }

        /// <summary>
        ///     <see cref="IUxrUniqueId.CombineUniqueId" />.
        /// </summary>
        /// <param name="guid">Id to combine the existing Ids with</param>
        /// <param name="getImplementer">A function that gets the implementer from a component</param>
        /// <param name="assignId">
        ///     An action that allows to assign a unique ID to a component. This avoids exposing the
        ///     unique ID as public.
        /// </param>
        /// <param name="onChanging">
        ///     An optional delegate that will be called right before assigning a new ID to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onChanged">
        ///     An optional delegate that will be called right after a new ID has been assigned to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onRegistering">
        ///     An optional delegate that will be called right before registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="onRegistered">
        ///     An optional delegate that will be called right after registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="recursive">
        ///     Whether to change also the unique Ids of the child components in the same GameObject
        ///     and all children.
        /// </param>
        public void CombineUniqueId(Guid                            guid,
                                    Func<T, UxrUniqueIdImplementer> getImplementer,
                                    Action<T, Guid>                 assignId,
                                    Action<T, Guid, Guid>           onChanging,
                                    Action<T, Guid, Guid>           onChanged,
                                    Action<T>                       onRegistering,
                                    Action<T>                       onRegistered,
                                    bool                            recursive)
        {
            T[] components = recursive ? _targetComponent.GetComponentsInChildren<T>(true) : new[] { _targetComponent };

            foreach (T unique in components)
            {
                // Make sure original ID has been initialized.
                InitializeUniqueIdIfNecessary(unique, getImplementer, assignId, onChanging, onChanged, onRegistering, onRegistered);

                UxrUniqueIdImplementer implementer = getImplementer(unique);

                if (implementer != null)
                {
                    Guid originalUniqueId = implementer.OriginalUniqueId != default ? implementer.OriginalUniqueId : unique.UniqueId;
                    implementer.CombineIdSource = guid;

                    // Ensure the same ID on all devices using the combination.
                    // We also replace the OriginalUniqueId because it works better when using CombineUniqueId multiple times over a hierarchy.
                    Guid combinedGuid = GuidExt.Combine(originalUniqueId, guid);
                    RegisterUniqueId(unique, assignId, onChanging, onChanged, onRegistering, onRegistered, combinedGuid);
                    implementer.OriginalUniqueId = combinedGuid;
                }
            }
        }

        /// <summary>
        ///     To be called from the component's OnValidate() Unity method. This will update come key variables used for unique ID
        ///     generation.
        /// </summary>
        /// <param name="assignId">
        ///     An action that allows to assign a unique ID to a component. This avoids exposing the
        ///     unique ID as public.
        /// </param>
        /// <param name="refIsInPrefab">
        ///     Reference to the component boolean that will tell whether the component lies in a prefab or
        ///     is instantiated in the scene
        /// </param>
        /// <param name="refPrefabGuid">
        ///     Reference to the string that will tell the GUID assigned by Unity to the prefab, if the
        ///     component lies in a prefab
        /// </param>
        public void NotifyOnValidate(Action<T, Guid> assignId, ref bool refIsInPrefab, ref string refPrefabGuid)
        {
#if UNITY_EDITOR

            Guid InternalGetUniqueId()
            {
                if (_targetComponent.UniqueIdIsTypeName)
                {
                    // Unique ID will be based on the type name. This is useful for singletons that are instantiated dynamically, in order to ensure same values on all devices.
                    return _targetComponent.GetType().FullName.GetGuid();
                }

                // Unique ID is generated randomly.
                return GetNewUniqueId();
            }

            if (EditorPrefs.GetBool(UxrConstants.Editor.AutomaticIdGenerationPrefs, true) &&
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling &&
                !BuildPipeline.isBuildingPlayer &&
                !EditorApplication.isUpdating)
            {
                if (_targetComponent.gameObject.scene.name != null && !_targetComponent.gameObject.scene.isLoaded)
                {
                    // Returning from play-mode, re-loading after build, among others.  
                    return;
                }

                bool setDirty = false;

                if (_targetComponent.UniqueId == default)
                {
                    // Generate unique ID
                    assignId(_targetComponent, InternalGetUniqueId());
                    setDirty = true;
                }

                // Check if a prefab was instantiated or an object was made prefab, to generate a different ID.
                // We want to avoid multiple instantiated prefabs to share the same ID.

                try
                {
                    // Sometimes, in prefab mode, GetPrefabGuid() can throw an exception when accessing
                    // PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot during OnValidate().
                    // It seems to happen when loading the prefab for the first time.

                    if (_targetComponent.GetPrefabGuid(out string prefabGuid, out string _))
                    {
                        if (_targetComponent.IsInPrefab() != refIsInPrefab || prefabGuid != refPrefabGuid)
                        {
                            assignId(_targetComponent, InternalGetUniqueId());
                            refIsInPrefab = _targetComponent.IsInPrefab();
                            refPrefabGuid = prefabGuid;
                            setDirty      = true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore exception.
                }

                if (setDirty)
                {
                    EditorUtility.SetDirty(_targetComponent);
                }
            }
#endif
        }

        /// <summary>
        ///     Unregisters the target component unique ID.
        /// </summary>
        public void Unregister()
        {
            s_componentsById.Remove(_targetComponent.UniqueId);
            UnregisterImplementer(_targetComponent, this);
            IsUnregistered = true;
        }

        #endregion

        #region Protected Overrides UxrUniqueIdImplementer

        /// <inheritdoc />
        protected override bool TryGetComponentByIdInternal(Guid id, out IUxrUniqueId component)
        {
            if (s_componentsById.TryGetValue(id, out T c))
            {
                component = c;
                return true;
            }

            component = null;
            return false;
        }

        /// <inheritdoc />
        protected override IEnumerable<IUxrUniqueId> GetAllComponentsInternal()
        {
            foreach (KeyValuePair<Guid, T> pair in s_componentsById)
            {
                yield return pair.Value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Returns a generated unique ID based on the given component, taking care of collisions.
        /// </summary>
        /// <param name="component">The component to process the unique ID for</param>
        /// <param name="assignId">
        ///     An action that allows to assign a unique ID to a component. This avoids exposing the
        ///     unique ID as public.
        /// </param>
        /// <param name="onChanging">
        ///     An optional delegate that will be called right before assigning a new ID to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onChanged">
        ///     An optional delegate that will be called right after a new ID has been assigned to a component.
        ///     The parameters passed are the component, the old ID and the new ID.
        /// </param>
        /// <param name="onRegistering">
        ///     An optional delegate that will be called right before registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="onRegistered">
        ///     An optional delegate that will be called right after registering a new component with its
        ///     ID. The parameter passed is the component.
        /// </param>
        /// <param name="requestedId">
        ///     If non-null, it will try to assign this Id. If it already exists or is null, it will generate a new one.
        /// </param>
        /// <returns>New generated ID</returns>
        private static Guid RegisterUniqueId(T                     component,
                                             Action<T, Guid>       assignId,
                                             Action<T, Guid, Guid> onChanging,
                                             Action<T, Guid, Guid> onChanged,
                                             Action<T>             onRegistering,
                                             Action<T>             onRegistered,
                                             Guid                  requestedId)
        {
            Guid unprocessedId = requestedId == default ? GetNewUniqueId() : requestedId;
            Guid newId         = unprocessedId;
            Guid unregisterId  = default;
            
            // First unregister if it was already registered

            if (component.UniqueId != default && ComponentsById.TryGetValue(component.UniqueId, out T existingComponent) && component == existingComponent)
            {
                unregisterId = component.UniqueId;
            }

            // Try to get new unique ID

            if (ComponentsById.ContainsKey(newId))
            {
                // Handle collisions

                int collisionIterations = 0;

                while (ComponentsById.ContainsKey(newId))
                {
                    int collisionCount = s_idCollisions[unprocessedId];

                    collisionCount++;
                    collisionIterations++;
                    newId = GuidExt.Combine(unprocessedId, $"Collision{collisionCount}".GetGuid());

                    s_idCollisions[unprocessedId] = collisionCount;
                }

                s_idCollisions[newId] = 0;
            }
            else
            {
                // New unique ID not in use: Initialize or update collision dictionary for this ID

                if (!s_idCollisions.TryAdd(unprocessedId, 0))
                {
                    s_idCollisions[unprocessedId]++;
                }
            }

            // Call ID change events

            if (unregisterId != default)
            {
                onChanging?.Invoke(component, unregisterId, newId);
                s_componentsById.Remove(unregisterId);
            }

            // Register new ID

            onRegistering?.Invoke(component);
            assignId.Invoke(component, newId);

            s_componentsById[newId] = component;
            
            onRegistered?.Invoke(component);

            // Call ID change events

            if (unregisterId != default)
            {
                onChanged?.Invoke(component, unregisterId, newId);
            }

            return newId;
        }

        #endregion

        #region Private Types & Data

        private static readonly Dictionary<Guid, T> s_componentsById = new Dictionary<Guid, T>();
        private readonly        T                   _targetComponent;

        #endregion
    }
}