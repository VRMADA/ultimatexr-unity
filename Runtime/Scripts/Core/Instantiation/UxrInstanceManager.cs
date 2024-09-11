// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSync;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Networking;
using UnityEngine;

#pragma warning disable 414 // Unused values

namespace UltimateXR.Core.Instantiation
{
    /// <summary>
    ///     The Instance Manager is responsible for making sure that relevant objects that are instantiated and destroyed at
    ///     runtime, are synchronized through the network in multi-player environments, saved in save-files and in replays.
    ///     It also allows to change <see cref="Transform" /> parameters of GameObjects in the scene, keeping them in sync.
    /// </summary>
    /// <remarks>
    ///     Instantiable prefabs are required to have at least one component with the <see cref="IUxrUniqueId" /> interface on
    ///     the root, such as any component derived from <see cref="UxrComponent" />, to be able to track them.
    ///     If no specific <see cref="IUxrUniqueId" /> is needed, a <see cref="UxrSyncObject" /> component can be used.
    /// </remarks>
    public partial class UxrInstanceManager : UxrSingleton<UxrInstanceManager>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                _registerAutomatically = true;
        [SerializeField] private bool                _includeFrameworkPrefabs;
        [SerializeField] private List<GameObject>    _automaticPrefabs; // GameObjects require at least one component with IUxrUniqueId so that the prefab ID can be known.
        [SerializeField] private List<UxrPrefabList> _userDefinedPrefabs;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when a prefab/empty GameObject is about to be instantiated.
        /// </summary>
        public event EventHandler<UxrInstanceEventArgs> Instantiating;

        /// <summary>
        ///     Called right after a prefab/empty GameObject was instantiated.
        /// </summary>
        public event EventHandler<UxrInstanceEventArgs> Instantiated;

        /// <summary>
        ///     Called when an instance is about to be destroyed.
        /// </summary>
        public event EventHandler<UxrInstanceEventArgs> Destroying;

        /// <summary>
        ///     Called when an instance was destroyed.
        /// </summary>
        public event EventHandler<UxrInstanceEventArgs> Destroyed;

        /// <summary>
        ///     Gets all the available prefabs registered in the instance manager.
        /// </summary>
        public IEnumerable<GameObject> AvailablePrefabs
        {
            get
            {
                InitializeIfNecessary();
                return _prefabsById.Values;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Returns the prefab corresponding to a given unity prefab id. Unity prefab IDs can be accessed using the
        ///     <see cref="IUxrUniqueId" /> interface of all components that implement it, such as <see cref="UxrComponent" />.
        /// </summary>
        /// <param name="unityPrefabId">The unity prefab id</param>
        /// <returns>
        ///     Prefab or null if the id is invalid or isn't registered in the <see cref="UxrInstanceManager" /> inspector
        ///     panel
        /// </returns>
        public GameObject GetPrefab(string unityPrefabId)
        {
            InitializeIfNecessary();

            if (_prefabsById != null && _prefabsById.TryGetValue(unityPrefabId, out GameObject prefab))
            {
                return prefab;
            }

            return null;
        }

        /// <summary>
        ///     Instantiates a prefab registered in the <see cref="UxrInstanceManager" />, ensuring synchronization across
        ///     environments:
        ///     <list type="bullet">
        ///         <item>All other clients in a multiplayer session.</item>
        ///         <item>Saved in save-files.</item>
        ///         <item>Saved in replays.</item>
        ///     </list>
        ///     All registered prefabs must have at least one component implementing <see cref="IUxrUniqueId" /> on the root
        ///     GameObject, such as any component derived from <see cref="UxrComponent" />.
        ///     If no specific <see cref="IUxrUniqueId" /> is needed, a <see cref="UxrSyncObject" /> component can be used.<br />
        ///     The instantiation ensures consistent unique Ids for all components in the hierarchy across different clients in
        ///     multiplayer, ensuring proper synchronization.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate (registered in <see cref="UxrInstanceManager" />)</param>
        /// <param name="parent">
        ///     Parent object to attach to or null for no parenting. The parent should have at least one component implementing
        ///     <see cref="IUxrUniqueId" /> for proper synchronization. If no specific <see cref="IUxrUniqueId" /> is needed, a
        ///     <see cref="UxrSyncObject" /> component can be used.
        /// </param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>New instance</returns>
        public GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            InitializeIfNecessary();

            if (prefab == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiatePrefab)}(): Prefab is null");
                }

                return null;
            }

            IUxrUniqueId parentUnique = parent != null ? parent.GetComponent<IUxrUniqueId>() : null;

            if (parent != null && parentUnique == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiatePrefab)}(): Instantiating prefab {prefab.name} to a parent with no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the parent to be able to track it.");
                }
            }

            IUxrUniqueId component = prefab.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiatePrefab)}(): Prefab {prefab.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to be able to track it.");
                }

                return null;
            }

            return InstantiatePrefabInternal(component.UnityPrefabId, parentUnique, position, rotation, UxrStateSyncImplementer.SyncCallDepth);
        }

        /// <summary>
        ///     Creates an empty GameObject, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="objectName">The name given to the GameObject</param>
        /// <param name="parent">
        ///     Parent object to attach to or null for no parenting. The parent should have at least one component implementing
        ///     <see cref="IUxrUniqueId" /> for proper synchronization. If no specific <see cref="IUxrUniqueId" /> is needed, a
        ///     <see cref="UxrSyncObject" /> component can be used.
        /// </param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>New instance</returns>
        public GameObject InstantiateEmptyGameObject(string objectName, Transform parent, Vector3 position, Quaternion rotation)
        {
            InitializeIfNecessary();

            IUxrUniqueId parentUnique = parent != null ? parent.GetComponent<IUxrUniqueId>() : null;

            if (parent != null && parentUnique == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiateEmptyGameObject)}(): Instantiating empty GameObject to a parent with no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the parent to be able to track it.");
                }
            }

            return InstantiateEmptyGameObjectInternal(objectName, parentUnique, position, rotation);
        }

        /// <summary>
        ///     Destroys a GameObject instantiated using <see cref="InstantiatePrefab" /> or
        ///     <see cref="InstantiateEmptyGameObject" />,
        ///     ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="target">
        ///     The GameObject to destroy.
        /// </param>
        public void DestroyGameObject(GameObject target)
        {
            if (target == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(DestroyGameObject)}(): Target is null");
                }

                return;
            }

            IUxrUniqueId unique = target.GetComponent<IUxrUniqueId>();

            if (unique == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(DestroyGameObject)}(): Target {target.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            DestroyGameObjectInternal(unique);
        }

        /// <summary>
        ///     Notifies that a prefab was spawned externally by a networking SDK, not using the instance manager.
        /// </summary>
        /// <param name="instance">New instance</param>
        public void NotifyNetworkSpawn(GameObject instance)
        {
            if (instance == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(NotifyNetworkSpawn)}(): Instance is null");
                }

                return;
            }

            IUxrUniqueId component = instance.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(NotifyNetworkSpawn)}(): Instance {instance.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            if (component.CombineIdSource == Guid.Empty)
            {
                Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(NotifyNetworkSpawn)}(): Network instance {instance.name} needs to have unique IDs to ensure correct synchronization. Consider using {nameof(GameObjectExt)}.{nameof(CombineUniqueId)}() on the instantiated prefab to ensure unique ids. As guid parameter you may use the id assigned by the networking SDK to the spawned object together with the string.GetGuid() extension defined in {nameof(StringExt)}.");
            }

            NotifyNetworkSpawnInternal(component.UnityPrefabId, component.CombineIdSource, component.UniqueId);
        }

        /// <summary>
        ///     Notifies that an instance is going to be despawned externally by a networking SDK, not using the instance manager.
        /// </summary>
        /// <param name="instance">Instance that will be despawned</param>
        public void NotifyNetworkDespawn(GameObject instance)
        {
            if (instance == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(NotifyNetworkDespawn)}(): Instance is null");
                }

                return;
            }

            IUxrUniqueId component = instance.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(NotifyNetworkDespawn)}(): Instance {instance.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            NotifyNetworkDespawnInternal(component, false);
        }

        /// <summary>
        ///     Changes the parent of a GameObject, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to parent. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="newParent">
        ///     The new parent or null to remove the parent. If non-null, to be able to track it, the parent needs to have at least
        ///     one component with the <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived
        ///     from <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="clearLocalPositionAndRotation">Whether to set the local position and rotation to zero after parenting</param>
        public void SetParent(Transform transform, Transform newParent, bool clearLocalPositionAndRotation)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetParent)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component       = transform.GetComponent<IUxrUniqueId>();
            IUxrUniqueId parentComponent = newParent != null ? newParent.GetComponent<IUxrUniqueId>() : null;

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetParent)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            if (newParent != null && parentComponent == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetParent)}(): Parent {newParent.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            if (component != null)
            {
                SetParentInternal(component, parentComponent, clearLocalPositionAndRotation);
            }
        }

        /// <summary>
        ///     Changes the local position of a Transform, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="localPosition">The new local position</param>
        public void SetLocalPosition(Transform transform, Vector3 localPosition)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalPosition)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalPosition)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetLocalPositionInternal(component, localPosition);
        }

        /// <summary>
        ///     Changes the position of a Transform, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="position">The new position</param>
        public void SetPosition(Transform transform, Vector3 position)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetPosition)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetPosition)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetPositionInternal(component, position);
        }

        /// <summary>
        ///     Changes the local rotation of a Transform, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="localRotation">The new local rotation</param>
        public void SetLocalRotation(Transform transform, Quaternion localRotation)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalRotation)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalRotation)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetLocalRotationInternal(component, localRotation);
        }

        /// <summary>
        ///     Changes the rotation of a Transform, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="rotation">The new rotation</param>
        public void SetRotation(Transform transform, Quaternion rotation)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetRotation)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetRotation)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetRotationInternal(component, rotation);
        }

        /// <summary>
        ///     Changes the position and rotation of a Transform, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="position">The new position</param>
        /// <param name="rotation">The new rotation</param>
        public void SetPositionAndRotation(Transform transform, Vector3 position, Quaternion rotation)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetPositionAndRotation)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetPositionAndRotation)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetPositionAndRotationInternal(component, position, rotation);
        }

        /// <summary>
        ///     Changes the local position and local rotation of a Transform, ensuring that the operation is synced in all
        ///     environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="localPosition">The new local position</param>
        /// <param name="localRotation">The new local rotation</param>
        public void SetLocalPositionAndRotation(Transform transform, Vector3 localPosition, Quaternion localRotation)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalPositionAndRotation)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetLocalPositionAndRotation)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetLocalPositionAndRotationInternal(component, localPosition, localRotation);
        }

        /// <summary>
        ///     Changes the scale of a GameObject, ensuring that the operation is synced in all environments.
        /// </summary>
        /// <param name="transform">
        ///     The transform to change. To be able to track it, it needs to have at least one component with the
        ///     <see cref="IUxrUniqueId" /> interface on the same GameObject, such as any component derived from
        ///     <see cref="UxrComponent" />. A <see cref="UxrSyncObject" /> component can be used if there is none.
        /// </param>
        /// <param name="scale">The new scale</param>
        public void SetScale(Transform transform, Vector3 scale)
        {
            if (transform == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetScale)}(): Transform is null");
                }

                return;
            }

            IUxrUniqueId component = transform.GetComponent<IUxrUniqueId>();

            if (component == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(SetScale)}(): Target {transform.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to the GameObject to be able to track it.");
                }

                return;
            }

            SetScaleInternal(component, scale);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Clears the nested instance queue. TODO: Remove once the replay manager is published and use replay manager events
        ///     instead.
        /// </summary>
        internal void ClearNestedInstanceQueue()
        {
            _nestedInstances.Clear();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the singleton.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializeIfNecessary();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the internal data if necessary.
        /// </summary>
        private void InitializeIfNecessary()
        {
            if (_prefabsById != null)
            {
                return;
            }

            if (_automaticPrefabs == null)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)} needs to be pre-instantiated in your startup scene to create the prefab list.");
                }
            }

            _prefabsById = new Dictionary<string, GameObject>();

            if (_registerAutomatically)
            {
                // Use automatically created list

                if (_automaticPrefabs != null)
                {
                    foreach (GameObject prefab in _automaticPrefabs)
                    {
                        TryRegisterPrefab(prefab);
                    }
                }
            }
            else
            {
                // Use user-defined prefab lists

                foreach (UxrPrefabList list in _userDefinedPrefabs)
                {
                    foreach (GameObject prefab in list.PrefabList)
                    {
                        TryRegisterPrefab(prefab);
                    }
                }
            }

            void TryRegisterPrefab(GameObject prefab)
            {
                if (prefab == null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Warnings)
                    {
                        Debug.LogWarning($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)} found a null prefab in the prefab list. Consider updating the list.");
                    }

                    return;
                }

                IUxrUniqueId component = prefab.GetComponent<IUxrUniqueId>();

                if (component != null)
                {
                    if (!_prefabsById.ContainsKey(component.UnityPrefabId))
                    {
                        if (!string.IsNullOrEmpty(component.UnityPrefabId))
                        {
                            _prefabsById.Add(component.UnityPrefabId, prefab);
                        }
                        else
                        {
                            if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Warnings)
                            {
                                Debug.LogWarning($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)} prefab {prefab.name} has empty or null id.");
                            }
                        }
                    }
                    else
                    {
                        if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Warnings)
                        {
                            Debug.LogWarning($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)} prefab {prefab.name} (id: {component.UnityPrefabId}) was already added.");
                        }
                    }
                }
                else
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)} found prefab {prefab.name} has no components with a unique ID. Consider adding a {nameof(UxrSyncObject)} component to be able to track it.");
                    }
                }
            }
        }

        /// <summary>
        ///     Method responsible for the instantiation.
        /// </summary>
        /// <param name="prefabId">Id of the prefab to instantiate</param>
        /// <param name="parent">Parent or null for no parenting</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <param name="syncNestingDepth">
        ///     The StateSync nesting depth when calling, which will be used to know when to return an
        ///     existing instance or create a new one
        /// </param>
        /// <param name="uniqueId">New unique ID or null to generate one</param>
        /// <returns>Instantiated object</returns>
        private GameObject InstantiatePrefabInternal(string prefabId, IUxrUniqueId parent, Vector3 position, Quaternion rotation, int syncNestingDepth = 0, Guid uniqueId = default)
        {
            if (syncNestingDepth > 0 && UxrManager.Instance.IsInsideStateSync && uniqueId == default)
            {
                // We are inside UxrManager.ExecutaStateSyncEvent(). Instantiation is synchronized using UxrStateSyncOptions.IgnoreNestingCheck for the random Guid algorithm to work correctly.
                // The Instantiate calls will be synchronized no matter the nesting depth, but the replication calls need to use the instance guids generated by the original source.

                if (_nestedInstances.Count == 0)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiatePrefabInternal)}: Nested instances is empty.");
                    }

                    return null;
                }

                return _nestedInstances.Dequeue();
            }

            if (_prefabsById.TryGetValue(prefabId, out GameObject prefab))
            {
                BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.IgnoreNestingCheck);

                Instantiating?.Invoke(this, new UxrInstanceEventArgs(null, prefab, prefabId));

                Transform  parentTransform = parent?.Transform;
                GameObject newInstance     = Instantiate(prefab, position, rotation, parentTransform);

                if (newInstance == null)
                {
                    // Can't happen if prefab was retrieved.
                    CancelSync();
                    return null;
                }

                // We use a trick where we sync the call with the generated Unique ID as parameter. 

                if (uniqueId == default)
                {
                    uniqueId = Guid.NewGuid();
                }

                IUxrUniqueId component = newInstance.GetComponent<IUxrUniqueId>();

                if (component == null)
                {
                    // Can't happen if prefab is registered.
                    CancelSync();
                    return null;
                }

                component.CombineUniqueId(uniqueId);

                _currentInstancedPrefabs.TryAdd(uniqueId, new InstanceInfo(component, prefabId));
                _currentInstances.TryAdd(uniqueId, newInstance);

                if (syncNestingDepth > 0)
                {
                    // When it's a nested sync call, enqueue the instance because it's going to be used later on.
                    _nestedInstances.Enqueue(newInstance);
                }

                Instantiated?.Invoke(this, new UxrInstanceEventArgs(newInstance, prefab, prefabId));

                EndSyncMethod(new object[] { prefabId, parent, position, rotation, syncNestingDepth, uniqueId });

                return newInstance;
            }

            if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
            {
                Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiatePrefabInternal)}(): Prefab with id {prefabId} is not registered in the {nameof(UxrInstanceManager)}. Register it in the inspector panel first.");
            }

            return null;
        }

        /// <summary>
        ///     Internal method that instantiates an empty GameObject.
        /// </summary>
        /// <param name="objectName">New GameObject name</param>
        /// <param name="parent">Parent or null for no parenting</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <param name="syncNestingDepth">
        ///     The StateSync nesting depth when calling, which will be used to know when to return an
        ///     existing instance or create a new one
        /// </param>
        /// <param name="uniqueId">New unique ID or null to generate one</param>
        /// <returns>Instantiated object</returns>
        private GameObject InstantiateEmptyGameObjectInternal(string objectName, IUxrUniqueId parent, Vector3 position, Quaternion rotation, int syncNestingDepth = 0, Guid uniqueId = default)
        {
            if (syncNestingDepth > 0 && UxrManager.Instance.IsInsideStateSync && uniqueId == default)
            {
                // We are inside a BeginSync/EndSync block. Instantiation is synchronized using UxrStateSyncOptions.IgnoreNestingCheck for the random Guid algorithm to work correctly.
                // The Instantiate calls will be synchronized no matter the nesting depth, but the replication calls need to use the instance guids generated by the original source.

                if (_nestedInstances.Count == 0)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} {nameof(UxrInstanceManager)}.{nameof(InstantiateEmptyGameObjectInternal)}: Nested instances is empty.");
                    }

                    return null;
                }

                return _nestedInstances.Dequeue();
            }

            BeginSync(UxrStateSyncOptions.Default | UxrStateSyncOptions.IgnoreNestingCheck);

            Instantiating?.Invoke(this, new UxrInstanceEventArgs(null, null, null));

            Transform  parentTransform = parent?.Transform;
            GameObject newInstance     = new GameObject(objectName ?? "Empty GameObject");
            newInstance.transform.SetParent(parentTransform);
            newInstance.transform.SetPositionAndRotation(position, rotation);

            // We use a trick where we sync the call with the generated Unique ID as parameter. 

            if (uniqueId == default)
            {
                uniqueId = Guid.NewGuid();
            }

            IUxrUniqueId component = newInstance.AddComponent<UxrSyncObject>();
            component.ChangeUniqueId(uniqueId);

            _currentInstancedPrefabs.TryAdd(uniqueId, new InstanceInfo(component, null));
            _currentInstances.TryAdd(uniqueId, component.GameObject);

            Instantiated?.Invoke(this, new UxrInstanceEventArgs(newInstance, null, null));

            EndSyncMethod(new object[] { objectName, parent, position, rotation, syncNestingDepth, uniqueId });

            return newInstance;
        }

        /// <summary>
        ///     Destroys a GameObject, identified by a component on the object with the <see cref="IUxrUniqueId" /> interface.
        /// </summary>
        /// <param name="component">
        ///     A component with the <see cref="IUxrUniqueId" /> interface on the root of the GameObject.
        /// </param>
        private void DestroyGameObjectInternal(IUxrUniqueId component)
        {
            if (component != null)
            {
                BeginSync();

                _prefabsById.TryGetValue(component.UnityPrefabId, out GameObject prefab);

                Destroying?.Invoke(this, new UxrInstanceEventArgs(component.GameObject, prefab, component.UnityPrefabId));

                _currentInstancedPrefabs.Remove(component.CombineIdSource);
                _currentInstances.Remove(component.CombineIdSource);

                // Avoid unique ID collisions by unregistering ahead of time since component destruction might get delayed.  
                IUxrUniqueId[] components = component.GameObject.GetComponentsInChildren<IUxrUniqueId>(true);
                components.ForEach(c => c.Unregister());

                Destroy(component.GameObject);

                Destroyed?.Invoke(this, new UxrInstanceEventArgs(null, prefab, component.UnityPrefabId));

                EndSyncMethod(new object[] { component });
            }
        }

        /// <summary>
        ///     Notifies that a prefab was spawned externally by a networking SDK, not using the instance manager.
        /// </summary>
        /// <param name="prefabId">Prefab id</param>
        /// <param name="combineGuid">Guid used for combination</param>
        /// <param name="instanceGuid">Guid of the instantiated component or default if it needs to be instantiated</param>
        private void NotifyNetworkSpawnInternal(string prefabId, Guid combineGuid, Guid instanceGuid)
        {
            // Do not sync in multiplayer since the prefab was spawned using networking already.
            BeginSync(UxrStateSyncOptions.Default ^ UxrStateSyncOptions.Network);

            IUxrUniqueId component = null;

            if (instanceGuid == default)
            {
                if (_prefabsById.TryGetValue(prefabId, out GameObject prefab))
                {
                    GameObject newInstance = Instantiate(prefab);

                    if (newInstance == null)
                    {
                        // Can't happen if prefab was retrieved.
                        CancelSync();
                    }

                    component = newInstance.GetComponent<IUxrUniqueId>();

                    if (component == null)
                    {
                        // Can't happen if prefab is registered.
                        CancelSync();
                    }

                    component.CombineUniqueId(combineGuid);

                    CheckNetworkSpawnPostprocess(newInstance);
                }
            }
            else
            {
                UxrUniqueIdImplementer.TryGetComponentById(instanceGuid, out component);
            }

            if (component != null)
            {
                _currentInstancedPrefabs.TryAdd(combineGuid, new InstanceInfo(component, prefabId));
                _currentInstances.TryAdd(combineGuid, component.GameObject);
            }

            // The trick here is that we force the instantiate parameter to be true so that the call itself doesn't instantiate anything but the synced call does.
            EndSyncMethod(new object[] { prefabId, combineGuid, default });
        }

        /// <summary>
        ///     Notifies that an instance is going to be despawned externally by a networking SDK, not using the instance manager.
        /// </summary>
        /// <param name="component">Component in the instance that will be despawned</param>
        private void NotifyNetworkDespawnInternal(IUxrUniqueId component, bool destroy)
        {
            if (component != null)
            {
                BeginSync(UxrStateSyncOptions.Default ^ UxrStateSyncOptions.Network);

                if (destroy)
                {
                    _currentInstancedPrefabs.Remove(component.CombineIdSource);
                    _currentInstances.Remove(component.CombineIdSource);
                    Destroy(component.GameObject);
                }

                EndSyncMethod(new object[] { component, true });
            }
        }

        /// <summary>
        ///     Parents a component.
        /// </summary>
        /// <param name="component">Component to parent</param>
        /// <param name="newParent">New parent</param>
        /// <param name="clearLocalPositionAndRotation">Whether to clear the local position and rotation after</param>
        private void SetParentInternal(IUxrUniqueId component, IUxrUniqueId newParent, bool clearLocalPositionAndRotation)
        {
            BeginSync();

            Transform parent = newParent?.Transform;
            component.Transform.SetParent(parent);

            if (clearLocalPositionAndRotation)
            {
                TransformExt.SetLocalPositionAndRotation(component.Transform, Vector3.zero, Quaternion.identity);
            }

            EndSyncMethod(new object[] { component, newParent, clearLocalPositionAndRotation });
        }

        /// <summary>
        ///     Changes the local position the GameObject defined by a component on it with the <see cref="IUxrUniqueId" />
        ///     interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="localPosition">The new local position</param>
        private void SetLocalPositionInternal(IUxrUniqueId component, Vector3 localPosition)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.localPosition = localPosition;
                EndSyncMethod(new object[] { component, localPosition });
            }
        }

        /// <summary>
        ///     Changes the position the GameObject defined by a component on it with the <see cref="IUxrUniqueId" /> interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="position">The new position</param>
        private void SetPositionInternal(IUxrUniqueId component, Vector3 position)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.position = position;
                EndSyncMethod(new object[] { component, position });
            }
        }

        /// <summary>
        ///     Changes the local rotation the GameObject defined by a component on it with the <see cref="IUxrUniqueId" />
        ///     interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="localRotation">The new local rotation</param>
        private void SetLocalRotationInternal(IUxrUniqueId component, Quaternion localRotation)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.localRotation = localRotation;
                EndSyncMethod(new object[] { component, localRotation });
            }
        }

        /// <summary>
        ///     Changes the rotation the GameObject defined by a component on it with the <see cref="IUxrUniqueId" /> interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="rotation">The new rotation</param>
        private void SetRotationInternal(IUxrUniqueId component, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.rotation = rotation;
                EndSyncMethod(new object[] { component, rotation });
            }
        }

        /// <summary>
        ///     Changes the position and rotation the GameObject defined by a component on it with the <see cref="IUxrUniqueId" />
        ///     interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="position">The new position</param>
        /// <param name="rotation">The new rotation</param>
        private void SetPositionAndRotationInternal(IUxrUniqueId component, Vector3 position, Quaternion rotation)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.position = position;
                component.Transform.rotation = rotation;
                EndSyncMethod(new object[] { component, position, rotation });
            }
        }

        /// <summary>
        ///     Changes the local position and local rotation of the GameObject defined by a component on it with the
        ///     <see cref="IUxrUniqueId" /> interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="localPosition">The new local position</param>
        /// <param name="localRotation">The new local rotation</param>
        private void SetLocalPositionAndRotationInternal(IUxrUniqueId component, Vector3 localPosition, Quaternion localRotation)
        {
            if (component != null)
            {
                BeginSync();
                component.Transform.localPosition = localPosition;
                component.Transform.localRotation = localRotation;
                EndSyncMethod(new object[] { component, localPosition, localRotation });
            }
        }

        /// <summary>
        ///     Changes the scale of the GameObject defined by a component on it with the <see cref="IUxrUniqueId" /> interface.
        /// </summary>
        /// <param name="component">
        ///     Any <see cref="IUxrUniqueId" /> component on the GameObject.
        /// </param>
        /// <param name="scale">The new scale</param>
        private void SetScaleInternal(IUxrUniqueId component, Vector3 scale)
        {
            if (component != null)
            {
                BeginSync();

                component.Transform.localScale = scale;

                EndSyncMethod(new object[] { component, scale });
            }
        }

        /// <summary>
        ///     Checks whether to disable/destroy networking components when instantiating a prefab result of a network spawn,
        ///     including avatars.
        /// </summary>
        /// <param name="newInstance">The new instance</param>
        private void CheckNetworkSpawnPostprocess(GameObject newInstance)
        {
            // Remove networking components right after instantiation since at this point were are not in
            // a multiplayer environment but we want to sync the spawn in replays.
            // Multiplayer components may throw errors and we don't need them anyway so we destroy them. 

            UxrNetworkComponentReferences[] references = newInstance.GetComponents<UxrNetworkComponentReferences>();

            foreach (UxrNetworkComponentReferences networkReferences in references)
            {
                foreach (Component c in networkReferences.AddedComponents)
                {
                    if (c != null)
                    {
                        Destroy(c);
                    }
                }

                foreach (GameObject go in networkReferences.AddedGameObjects)
                {
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
            }

            // If it's an avatar, put it into UpdateExternally mode

            UxrAvatar avatar = newInstance.GetComponent<UxrAvatar>();

            if (avatar != null)
            {
                avatar.AvatarMode = UxrAvatarMode.UpdateExternally;
            }
        }

        #endregion

        #region Private Types & Data

        private readonly Dictionary<Guid, GameObject> _currentInstances = new Dictionary<Guid, GameObject>(); // (combine Guid -> instantiated GameObject). Contains the current instances in the scene.

        private Dictionary<string, GameObject> _prefabsById;
        private Dictionary<Guid, InstanceInfo> _currentInstancedPrefabs = new Dictionary<Guid, InstanceInfo>(); // (combine Guid -> prefabId). This one is serialized by the StateSave functionality. Contains which prefabs are currently instantiated.

        private readonly Queue<GameObject> _nestedInstances = new Queue<GameObject>();

        #endregion
    }
}

#pragma warning restore 414