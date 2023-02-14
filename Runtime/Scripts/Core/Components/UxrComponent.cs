// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Base class for components in UltimateXR. Has functionality to access the global lists of UltimateXR components,
    ///     cache Unity components, access initial transform values and some other common utilities.
    ///     To enumerate all components use the static properties <see cref="AllComponents" /> and
    ///     <see cref="EnabledComponents" />.
    /// </summary>
    /// <remarks>
    ///     Make sure to override the Unity methods used, and call the base implementation in the body.
    ///     Components get registered through their Awake() call. This means that components get registered
    ///     the first time they are enabled. Disabled objects that have been enabled at some point are enumerated, but objects
    ///     that have never been enabled don't get enumerated, which means that they will not appear in
    ///     <see cref="AllComponents" />.
    /// </remarks>
    public abstract class UxrComponent : MonoBehaviour
    {
        #region Public Types & Data

        /// <summary>
        ///     Called before registering a component.
        /// </summary>
        public static event Action<UxrComponent> GlobalRegistering;

        /// <summary>
        ///     Called when a component was registered.
        /// </summary>
        public static event Action<UxrComponent> GlobalRegistered;

        /// <summary>
        ///     Called before unregistering a component.
        /// </summary>
        public static event Action<UxrComponent> GlobalUnregistering;

        /// <summary>
        ///     Called when a component was unregistered.
        /// </summary>
        public static event Action<UxrComponent> GlobalUnregistered;

        /// <summary>
        ///     Called when a component was enabled.
        /// </summary>
        public static event Action<UxrComponent> GlobalEnabled;

        /// <summary>
        ///     Called when a component was disabled.
        /// </summary>
        public static event Action<UxrComponent> GlobalDisabled;

        /// <summary>
        ///     Called when a component is about to change its unique id by using <see cref="TrySetUniqueId" />.
        ///     Parameters are oldId, newId.
        /// </summary>
        public static event Action<string, string> GlobalIdChanging;

        /// <summary>
        ///     Called when a component changed its unique id by using <see cref="TrySetUniqueId" />.
        ///     Parameters are oldId, newId.
        /// </summary>
        public static event Action<string, string> GlobalIdChanged;

        /// <summary>
        ///     Gets all the components, enabled or not, in all open scenes.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="GameObject.GetComponentsInChildren{T}(bool)" /> or
        ///     <see cref="UnityEngine.Object.FindObjectsOfType{T}(bool)" />.
        /// </remarks>
        public static IEnumerable<UxrComponent> AllComponents => s_componentsById.Values;

        /// <summary>
        ///     Gets all components that are enabled, in all open scenes.
        /// </summary>
        public static IEnumerable<UxrComponent> EnabledComponents => AllComponents.Where(c => c.enabled);

        /// <summary>
        ///     Gets the unique Id of the component.
        /// </summary>
        public string UniqueId { get; private set; }

        /// <summary>
        ///     Gets or sets whether the application is quitting. An application is known to be quitting when
        ///     <see cref="OnApplicationQuit" /> was called.
        /// </summary>
        public bool IsApplicationQuitting { get; private set; }

        /// <summary>
        ///     Gets the <see cref="Transform.parent" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Transform InitialParent { get; private set; }

        /// <summary>
        ///     Gets the <see cref="Transform.localPosition" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialLocalPosition { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the <see cref="Transform.localRotation" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Quaternion InitialLocalRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        ///     Gets the <see cref="Transform.localEulerAngles" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialLocalEulerAngles { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the <see cref="Transform.localScale" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialLocalScale { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the <see cref="Transform.lossyScale" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialLossyScale { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the <see cref="Transform.position" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialPosition { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the <see cref="Transform.rotation" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Quaternion InitialRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        ///     Gets the <see cref="Transform.eulerAngles" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Vector3 InitialEulerAngles { get; private set; } = Vector3.zero;

        /// <summary>
        ///     Gets the transformation matrix relative to the parent transform at the moment of <see cref="Awake"/> 
        /// </summary>
        public Matrix4x4 InitialRelativeMatrix { get; private set; } = Matrix4x4.identity;
        
        /// <summary>
        ///     Gets the <see cref="Transform.localToWorldMatrix"/> value at the moment of <see cref="Awake"/> 
        /// </summary>
        public Matrix4x4 InitialLocalToWorldMatrix { get; private set; } = Matrix4x4.identity;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Destroys all components.
        /// </summary>
        public static void DestroyAllComponents()
        {
            foreach (UxrComponent component in AllComponents)
            {
                Destroy(component);
            }
        }

        /// <summary>
        ///     Destroys all gameObjects the components belong to.
        /// </summary>
        public static void DestroyAllGameObjects()
        {
            foreach (UxrComponent component in AllComponents)
            {
                Destroy(component.gameObject);
            }
        }

        /// <summary>
        ///     Tries to get a component by its unique id.
        /// </summary>
        /// <param name="id">Id of the component to retrieve</param>
        /// <param name="component">Returns the component if the id exists</param>
        /// <returns>Whether the id was found</returns>
        public static bool TryGetComponentById(string id, out UxrComponent component)
        {
            return s_componentsById.TryGetValue(id, out component);
        }

        /// <summary>
        ///     Changes the object's unique Id if it doesn't exist.
        /// </summary>
        /// <param name="newUniqueId">New id</param>
        /// <returns>Whether the new id was valid, meaning it didn't exist</returns>
        public bool TrySetUniqueId(string newUniqueId)
        {
            if (!s_componentsById.ContainsKey(newUniqueId))
            {
                if (s_componentsById.ContainsKey(UniqueId))
                {
                    s_componentsById.Remove(UniqueId);
                }

                OnUniqueIdChanging(UniqueId, newUniqueId);
                UniqueId = newUniqueId;
                s_componentsById.Add(UniqueId, this);
                OnUniqueIdChanged(UniqueId, newUniqueId);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Caches the data of the GameObject's <see cref="Transform" /> component.
        ///     This is called on Awake() but can be called by the user at any point of the program to re-compute the values again
        ///     using the current state. This can be useful when an object is re-parented and the data using the new parenting
        ///     is more meaningful.
        /// </summary>
        public void RecomputeInitialTransformData()
        {
            Transform tf = transform;
            InitialParent             = tf.parent;
            InitialLocalPosition      = tf.localPosition;
            InitialLocalRotation      = tf.localRotation;
            InitialLocalEulerAngles   = tf.localEulerAngles;
            InitialLocalScale         = tf.localScale;
            InitialLossyScale         = tf.lossyScale;
            InitialPosition           = tf.position;
            InitialRotation           = tf.rotation;
            InitialEulerAngles        = tf.eulerAngles;
            InitialRelativeMatrix     = Matrix4x4.TRS(tf.localPosition, tf.localRotation, tf.localScale);
            InitialLocalToWorldMatrix = tf.localToWorldMatrix;
        }

        /// <summary>
        ///     Returns a Unity <see cref="Component" /> cached by type given that there is only one in the GameObject.
        ///     If there is more than one, it will return the first that <see cref="GameObject.GetComponent{T}" /> gets.
        ///     This method is mainly used to avoid boilerplate code in property getters that return internally cached components.
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Cached component or null if there is none.</returns>
        public T GetCachedComponent<T>() where T : Component
        {
            if (_cachedComponents.TryGetValue(typeof(T), out Component component))
            {
                return (T)component;
            }

            component = GetComponent<T>();

            if (component != null)
            {
                _cachedComponents.Add(typeof(T), component);
            }

            return (T)component;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Stores all initial transform values that can be useful for child classes.
        /// </summary>
        protected virtual void Awake()
        {
            // Store initial transform data

            RecomputeInitialTransformData();

            // Compute Id using unique scene path and handling collisions

            if (!s_idCollisions.ContainsKey(this.GetUniqueScenePath()))
            {
                s_idCollisions.Add(this.GetUniqueScenePath(), 0);
                UniqueId = this.GetUniqueScenePath().GetMd5x2();
            }
            else
            {
                int collisionCount = s_idCollisions[this.GetUniqueScenePath()]++;
                UniqueId = (this.GetUniqueScenePath() + $"Collision{collisionCount}").GetMd5x2();
            }

            if (s_componentsById.ContainsKey(UniqueId))
            {
                Debug.LogError($"Component {this.GetPathUnderScene()} caused a unique ID collision. This shouldn't be happening. Generating random ID.");
                UniqueId = Guid.NewGuid().ToString();
            }

            // Register component
            OnRegistering();
            s_componentsById.Add(UniqueId, this);
            OnRegistered();
        }

        /// <summary>
        ///     Unity <see cref="OnDestroy" /> handling.
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnUnregistering();
            s_componentsById.Remove(UniqueId);
            OnUnregistered();
        }

        /// <summary>
        ///     Sets the <see cref="IsApplicationQuitting" /> value to indicate that the application is quitting.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            IsApplicationQuitting = true;
        }

        /// <summary>
        ///     Unity <see cref="OnEnable" /> handling.
        /// </summary>
        protected virtual void OnEnable()
        {
            OnEnabled();
        }

        /// <summary>
        ///     Unity <see cref="OnDisable" /> handling.
        /// </summary>
        protected virtual void OnDisable()
        {
            OnDisabled();
        }

        /// <summary>
        ///     Unity <see cref="Reset" /> handling.
        /// </summary>
        protected virtual void Reset()
        {

        }

        /// <summary>
        ///     Unity <see cref="Start" /> handling.
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        ///     Unity <see cref="OnValidate" /> handling.
        /// </summary>
        protected virtual void OnValidate()
        {
            
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     <see cref="GlobalRegistering" /> event trigger.
        /// </summary>
        private void OnRegistering()
        {
            GlobalRegistering?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalRegistered" /> event trigger.
        /// </summary>
        private void OnRegistered()
        {
            GlobalRegistered?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalUnregistering" /> event trigger.
        /// </summary>
        private void OnUnregistering()
        {
            GlobalUnregistering?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalUnregistered" /> event trigger.
        /// </summary>
        private void OnUnregistered()
        {
            GlobalUnregistered?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalEnabled" /> event trigger.
        /// </summary>
        private void OnEnabled()
        {
            GlobalEnabled?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalDisabled" /> event trigger.
        /// </summary>
        private void OnDisabled()
        {
            GlobalDisabled?.Invoke(this);
        }

        /// <summary>
        ///     <see cref="GlobalIdChanging" /> event trigger.
        /// </summary>
        /// <param name="oldId">Old id</param>
        /// <param name="newId">New id</param>
        private void OnUniqueIdChanging(string oldId, string newId)
        {
            GlobalIdChanging?.Invoke(oldId, newId);
        }

        /// <summary>
        ///     <see cref="GlobalIdChanged" /> event trigger.
        /// </summary>
        /// <param name="oldId">Old id</param>
        /// <param name="newId">New id</param>
        private void OnUniqueIdChanged(string oldId, string newId)
        {
            GlobalIdChanged?.Invoke(oldId, newId);
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Static dictionary of all <see cref="UxrComponent" /> components by id.
        /// </summary>
        private static readonly Dictionary<string, UxrComponent> s_componentsById = new Dictionary<string, UxrComponent>();

        /// <summary>
        ///     Static dictionary of path collisions so that unique ids are generated.
        /// </summary>
        private static readonly Dictionary<string, int> s_idCollisions = new Dictionary<string, int>();

        /// <summary>
        ///     Dictionary of cached components by type.
        /// </summary>
        private readonly Dictionary<Type, Component> _cachedComponents = new Dictionary<Type, Component>();

        #endregion
    }
}