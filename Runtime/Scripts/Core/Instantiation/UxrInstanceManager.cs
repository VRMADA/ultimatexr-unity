// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    /// <summary>
    ///     The Instance Manager is responsible for making sure that relevant objects that are instantiated and destroyed at
    ///     runtime, are synchronized through the network in multi-player environments. <br/>
    ///     It also allows to change <see cref="Transform" /> parameters of GameObjects in the scene, and keep them in sync
    ///     on all clients.
    /// </summary>
    public class UxrInstanceManager : UxrSingleton<UxrInstanceManager>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private List<UxrComponent> _registeredPrefabs;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when a prefab is about to be instantiated. The event parameter is a <see cref="UxrComponent" /> component in
        ///     the root GameObject of the prefab, and is used to identify it using its <see cref="UxrComponent.UniqueId" />.
        /// </summary>
        public event Action<UxrComponent> Instantiating;

        /// <summary>
        ///     Called right after a prefab was instantiated. The event parameter is a <see cref="UxrComponent" /> component in the
        ///     root GameObject of the newly instantiated GameObject, and has a new unique identifier in the scene
        ///     <see cref="UxrComponent.UniqueId" /> that will be used to send and receive messages.
        /// </summary>
        public event Action<UxrComponent> Instantiated;

        /// <summary>
        ///     Called when a GameObject is about to be destroyed. The event parameter is a <see cref="UxrComponent" /> component
        ///     in the root GameObject of the instantiated GameObject, which is used to identify it using the
        ///     <see cref="UxrComponent.UniqueId" />.
        /// </summary>
        public event Action<UxrComponent> Destroying;

        /// <summary>
        ///     Called when an object was destroyed. The event parameter is the <see cref="UxrComponent.UniqueId" /> of a
        ///     <see cref="UxrComponent" /> component in the root GameObject that was destroyed.
        /// </summary>
        public event Action<Guid> Destroyed;

        /// <summary>
        ///     Gets all the available prefabs registered in the instance manager.
        /// </summary>
        public IReadOnlyList<UxrComponent> AvailablePrefabs => _registeredPrefabs;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Instantiates a new GameObject using a prefab defined by an <see cref="UxrComponent" /> on its root.
        ///     If there is no need for any specific <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        ///     The reason to require an <see cref="UxrComponent" /> is to be able to identify the prefab using a Unique ID and,
        ///     more importantly, assigning the same Unique ID to all instances in the different clients. This includes also all
        ///     other <see cref="UxrComponent" /> components in the hierarchy, which will also have the same unique Id in all
        ///     clients. Unique IDs in child objects will be generated using the parent ID as a base, to ensure the same
        ///     uniqueness on all clients.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate, defined by any UxrComponent on its root</param>
        /// <param name="parent">Parent object to parent it to, or null to not parent it</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <param name="uniqueId">Unique ID or default to generate a new random unique ID</param>
        /// <returns>New instance</returns>
        public UxrComponent InstantiateGameObject(UxrComponent prefab, UxrComponent parent, Vector3 position, Quaternion rotation, Guid uniqueId)
        {
            CheckInitialized();
            return InstantiateGameObjectInternal(prefab.UniqueId, parent, position, rotation, uniqueId);
        }

        /// <summary>
        ///     Destroys a GameObject.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> in the root of the GameObject. Use <see cref="UxrDummy" /> if
        ///     there is no need for any specific <see cref="UxrComponent" />
        /// </param>
        public void DestroyGameObject(UxrComponent component)
        {
            if (component != null)
            {
                BeginSync();

                Guid id = component.UniqueId;
                Destroying?.Invoke(component);
                Destroy(component.gameObject);
                Destroyed?.Invoke(id);

                EndSyncMethod(new object[] { component });
            }
        }

        /// <summary>
        ///     Changes the parent of a GameObject, identified by a <see cref="UxrComponent" />.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="newParent">The new parent</param>
        /// <param name="clearLocalPositionAndRotation">Whether to set the local position and rotation to zero after parenting</param>
        public void SetParent(UxrComponent component, UxrComponent newParent, bool clearLocalPositionAndRotation)
        {
            if (component != null)
            {
                BeginSync();

                Transform parent = newParent != null ? newParent.transform : null;
                component.transform.SetParent(parent);

                if (clearLocalPositionAndRotation)
                {
                    component.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }

                EndSyncMethod(new object[] { component, newParent, clearLocalPositionAndRotation });
            }
        }

        /// <summary>
        ///     Changes the local position of a GameObject, making sure it syncs on all clients in a multi-player environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="position">The new local position</param>
        public void SetLocalPosition(UxrComponent component, Vector3 position)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.localPosition = position;

                EndSyncMethod(new object[] { component, position });
            }
        }

        /// <summary>
        ///     Changes the position of a GameObject, making sure it syncs on all clients in a multi-player environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="position">The new position</param>
        public void SetPosition(UxrComponent component, Vector3 position)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.position = position;

                EndSyncMethod(new object[] { component, position });
            }
        }

        /// <summary>
        ///     Changes the local rotation of a GameObject, making sure it syncs on all clients in a multi-player environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="rotation">The new local rotation</param>
        public void SetLocalRotation(UxrComponent component, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.localRotation = rotation;

                EndSyncMethod(new object[] { component, rotation });
            }
        }

        /// <summary>
        ///     Changes the rotation of a GameObject, making sure it syncs on all clients in a multi-player environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="rotation">The new rotation</param>
        public void SetRotation(UxrComponent component, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.rotation = rotation;

                EndSyncMethod(new object[] { component, rotation });
            }
        }

        /// <summary>
        ///     Changes the position and rotation of a GameObject, making sure it syncs on all clients in a multi-player
        ///     environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="position">The new position</param>
        /// <param name="rotation">The new rotation</param>
        public void SetPositionAndRotation(UxrComponent component, Vector3 position, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.SetPositionAndRotation(position, rotation);

                EndSyncMethod(new object[] { component, position, rotation });
            }
        }

        /// <summary>
        ///     Changes the local position and rotation of a GameObject, making sure it syncs on all clients in a multi-player
        ///     environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="position">The new local position</param>
        /// <param name="rotation">The new local rotation</param>
        public void SetLocalPositionAndRotation(UxrComponent component, Vector3 position, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.SetLocalPositionAndRotation(position, rotation);

                EndSyncMethod(new object[] { component, position, rotation });
            }
        }

        /// <summary>
        ///     Changes the scale of a GameObject, making sure it syncs on all clients in a multi-player environment.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="UxrComponent" /> on the GameObject. If there is no need for any specific
        ///     <see cref="UxrComponent" />, a dummy <see cref="UxrDummy" /> can be used.
        /// </param>
        /// <param name="scale">The new scale</param>
        public void SetScale(UxrComponent component, Vector3 scale)
        {
            if (component != null)
            {
                BeginSync();

                component.transform.localScale = scale;

                EndSyncMethod(new object[] { component, scale });
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            CheckInitialized();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the internal data if necessary.
        /// </summary>
        private void CheckInitialized()
        {
            if (_prefabsById != null)
            {
                return;
            }

            _prefabsById = new Dictionary<Guid, UxrComponent>();

            foreach (UxrComponent prefab in AvailablePrefabs)
            {
                _prefabsById.Add(prefab.UniqueId, prefab);
            }
        }

        /// <summary>
        ///     Method responsible for the instantiation.
        /// </summary>
        /// <param name="prefabUniqueId">Prefab unique ID</param>
        /// <param name="parent">Parent or null for no parenting</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <param name="uniqueId">New unique ID or null to generate one</param>
        /// <returns>Instantiated object</returns>
        private UxrComponent InstantiateGameObjectInternal(Guid prefabUniqueId, UxrComponent parent, Vector3 position, Quaternion rotation, Guid uniqueId = default)
        {
            UxrComponent newInstance = null;

            if (_prefabsById.TryGetValue(prefabUniqueId, out UxrComponent prefab))
            {
                BeginSync();

                Instantiating?.Invoke(prefab);

                Transform parentTransform = parent != null ? parent.transform : null;
                newInstance = Instantiate(prefab, position, rotation, parentTransform);
                
                // We use a trick where we sync the call with the generated Unique ID as parameter. 

                if (uniqueId == default)
                {
                    uniqueId = Guid.NewGuid();
                }

                newInstance.ChangeUniqueId(uniqueId, true);

                Instantiated?.Invoke(prefab);

                EndSyncMethod(new object[] { prefabUniqueId, parent, position, rotation, uniqueId });
            }

            return newInstance;
        }

        #endregion

        #region Private Types & Data

        private Dictionary<Guid, UxrComponent> _prefabsById;

        #endregion
    }
}