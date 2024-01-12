// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UltimateXR.Attributes;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Settings;
using UltimateXR.Core.StateSync;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Base class for components in UltimateXR. Has functionality to access the global lists of UltimateXR components,
    ///     cache Unity components, access initial transform values and some other common utilities.
    ///     To enumerate all components use the static properties <see cref="AllComponents" /> and
    ///     <see cref="EnabledComponents" />.
    ///     This base class also provides functionality to synchronize the state, in order to add support for multiplayer
    ///     and save states.
    /// </summary>
    /// <remarks>
    ///     Make sure to override the Unity methods used, and call the base implementation in the body.
    ///     Components get registered through their Awake() call. This means that components get registered
    ///     the first time they are enabled. Disabled objects that have been enabled at some point are enumerated, but objects
    ///     that have never been enabled don't get enumerated, which means that they will not appear in
    ///     <see cref="AllComponents" />.
    /// </remarks>
    public abstract class UxrComponent : MonoBehaviour, IUxrStateSync
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] [HideInNormalInspector] private string _uxrUniqueId = string.Empty;
        [SerializeField] [HideInNormalInspector] private string __prefabGuid = string.Empty;
        [SerializeField] [HideInNormalInspector] private bool   __isInPrefab;

        #endregion

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
        ///     Called when a component is about to change its unique id by using <see cref="ChangeUniqueId" />.
        ///     Parameters are oldId, newId.
        /// </summary>
        public static event Action<string, string> GlobalIdChanging;

        /// <summary>
        ///     Called when a component changed its unique id by using <see cref="ChangeUniqueId" />.
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
        public static IEnumerable<UxrComponent> AllComponents => s_componentsById.Values.Where(c => c != null);

        /// <summary>
        ///     Gets all components that are enabled, in all open scenes.
        /// </summary>
        public static IEnumerable<UxrComponent> EnabledComponents => AllComponents.Where(c => c != null && c.isActiveAndEnabled);

        /// <summary>
        ///     Gets the unique Id of the component.
        /// </summary>
        public string UniqueId
        {
            get => _uxrUniqueId;
            private set => _uxrUniqueId = value;
        }

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
        ///     Gets the transformation matrix relative to the parent transform at the moment of <see cref="Awake" />
        /// </summary>
        public Matrix4x4 InitialRelativeMatrix { get; private set; } = Matrix4x4.identity;

        /// <summary>
        ///     Gets the <see cref="Transform.localToWorldMatrix" /> value at the moment of <see cref="Awake" />
        /// </summary>
        public Matrix4x4 InitialLocalToWorldMatrix { get; private set; } = Matrix4x4.identity;

        #endregion

        #region Implicit IUxrStateSync

        /// <inheritdoc />
        public virtual string SyncTargetName => name;

        /// <inheritdoc />
        public event EventHandler<UxrSyncEventArgs> StateChanged;

        /// <inheritdoc />
        public virtual void SerializeGlobalState(IUxrSerializer serializer)
        {
        }

        #endregion

        #region Explicit IUxrStateSync

        /// <inheritdoc />
        void IUxrStateSync.SyncState(UxrSyncEventArgs e)
        {
            // First check if it's a synchronization that can be solved at the base level

            if (e is UxrPropertyChangedSyncEventArgs propertyChangedEventArgs)
            {
                try
                {
                    // Set new property value using reflection
                    GetType().GetProperty(propertyChangedEventArgs.PropertyName, PropertyFlags).SetValue(this, propertyChangedEventArgs.Value);
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync property {propertyChangedEventArgs.PropertyName} to value {propertyChangedEventArgs.Value} . Component: {this.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
            else if (e is UxrMethodInvokedSyncEventArgs methodInvokedEventArgs)
            {
                try
                {
                    if (methodInvokedEventArgs.Parameters == null || !methodInvokedEventArgs.Parameters.Any())
                    {
                        // Invoke without arguments
                        GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags).Invoke(this, null);
                    }
                    else
                    {
                        // Invoke method using same parameters using reflection. Make sure we select the correct overload.

                        bool anyIsNull = methodInvokedEventArgs.Parameters.Any(p => p == null);

                        if (GetType().GetMethods().Length == 1)
                        {
                            // There are no overloads
                            GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags).Invoke(this, methodInvokedEventArgs.Parameters);
                        }
                        else if (!anyIsNull)
                        {
                            // We can look for a method specifying the parameter types.
                            GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags, null, methodInvokedEventArgs.Parameters.Select(p => p.GetType()).ToArray(), null).Invoke(this, methodInvokedEventArgs.Parameters);
                        }
                        else
                        {
                            // We have a call where a parameter is null, so we can't infer the parameter type. Try to find a method with the same parameter count.

                            MethodInfo method = GetType().GetMethods(MethodFlags).FirstOrDefault(x => x.Name.Equals(methodInvokedEventArgs.MethodName) && x.GetParameters().Length == methodInvokedEventArgs.Parameters.Length);

                            if (method != null)
                            {
                                method.Invoke(this, methodInvokedEventArgs.Parameters);
                            }
                            else
                            {
                                throw new Exception("Could not find a method with the given name and parameter count");
                            }
                        }
                    }
                }
                catch (AmbiguousMatchException ambiguousMatchException)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Trying to sync a method that has ambiguous call. {e}. Component: {this.GetPathUnderScene()}. Exception: {ambiguousMatchException}");
                    }
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync method. It could be that {nameof(EndSyncMethod)} was used with the wrong parameters or it has an overload that could not be resolved. {e}. Component: {this.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
            else
            {
                try
                {
                    // Try to sync using child class (overriden SyncState method).
                    SyncState(e);
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync state. {e}. Component: {this.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
        }

        #endregion

        #region Public Overrides Object

        /// <summary>
        ///     Returns the full path of the component in the scene.
        /// </summary>
        /// <returns>String representing the component</returns>
        public override string ToString()
        {
            return this.GetPathUnderScene();
        }

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
        ///     Generates a new unique Id.
        /// </summary>
        /// <returns>Unique Id</returns>
        public static string GetNewUniqueId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Changes the object's unique Id if it doesn't exist. This is useful in multiplayer
        ///     environments to make sure that network instantiated objects share the same ID.
        /// </summary>
        /// <param name="newUniqueId">New id</param>
        /// <param name="recursive">
        ///     Whether to change also the unique ID's of the UxrComponent components in the same GameObject
        ///     and all children, based on the new unique ID
        /// </param>
        /// <returns>
        ///     The new unique ID. If the requested unique already existed, the returned value will be different
        ///     to make sure it is unique
        /// </returns>
        public string ChangeUniqueId(string newUniqueId, bool recursive = false)
        {
            // If called during edit-time, simply generate unique IDs

            if (!Application.isPlaying)
            {
                UxrComponent[] components = GetComponentsInChildren<UxrComponent>(true);

                foreach (UxrComponent component in components)
                {
                    component.UniqueId = GetNewUniqueId();
                }

                return UniqueId;
            }

            // Make sure original ID has been initialized.
            CheckInitializeUniqueId();

            // During play time, generate new ID for the component and if recursion was requested, generate
            // "relative" IDs for the children to make sure that they are generated equally in different computers
            GenerateUniqueId(this, newUniqueId);

            if (recursive)
            {
                // Re-generate IDs for all components in same GameObject and children based on relative unique id:

                UxrComponent[] components = GetComponentsInChildren<UxrComponent>(true);

                foreach (UxrComponent component in components)
                {
                    if (component != this)
                    {
                        // Make sure original ID has been initialized.
                        component.CheckInitializeUniqueId();

                        // This makes sure that the children will also get the same unique ID on all devices 
                        GenerateUniqueId(component, (newUniqueId + component.OriginalUniqueId).GetMd5x2());
                    }
                }
            }

            return UniqueId;
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
        ///     Restores the local position, rotation and scale values stored during Awake() or
        ///     <see cref="RecomputeInitialTransformData" />.
        /// </summary>
        public void RestoreInitialLocalTransform()
        {
            transform.localPosition = InitialLocalPosition;
            transform.localRotation = InitialLocalRotation;
            transform.localScale    = InitialLocalScale;
        }

        /// <summary>
        ///     Restores the position, rotation and scale values stored during Awake() or
        ///     <see cref="RecomputeInitialTransformData" />.
        /// </summary>
        public void RestoreInitialWorldTransform()
        {
            transform.position   = InitialPosition;
            transform.rotation   = InitialRotation;
            transform.localScale = InitialLocalScale;
        }

        /// <summary>
        ///     Pushes the object's current position, rotation and local scale into the stack.
        ///     To restore the object's position, rotation and local scale later at any point, use <see cref="PopTransform" />.
        /// </summary>
        public void PushTransform()
        {
            if (_positionStack == null)
            {
                _positionStack = new Stack<Vector3>();
                _rotationStack = new Stack<Quaternion>();
                _scaleStack    = new Stack<Vector3>();
            }

            _positionStack.Push(transform.position);
            _rotationStack.Push(transform.rotation);
            _scaleStack.Push(transform.localScale);
        }

        /// <summary>
        ///     Pushes the object's current local position, local rotation and local scale into the stack.
        ///     To restore the object's local position, local rotation and local scale later at any point, use
        ///     <see cref="PopLocalTransform" />.
        /// </summary>
        public void PushLocalTransform()
        {
            if (_localPositionStack == null)
            {
                _localPositionStack = new Stack<Vector3>();
                _localRotationStack = new Stack<Quaternion>();
                _localScaleStack    = new Stack<Vector3>();
            }

            _localPositionStack.Push(transform.localPosition);
            _localRotationStack.Push(transform.localRotation);
            _localScaleStack.Push(transform.localScale);
        }

        /// <summary>
        ///     Restores the object's position, rotation and local scale using the information stored with
        ///     <see cref="PushTransform" />.
        /// </summary>
        public void PopTransform()
        {
            if (_positionStack != null && _positionStack.Count > 0)
            {
                transform.position   = _positionStack.Pop();
                transform.rotation   = _rotationStack.Pop();
                transform.localScale = _scaleStack.Pop();
            }
        }

        /// <summary>
        ///     Restores the object's position, rotation and local scale using the information stored with
        ///     <see cref="PushLocalTransform" />.
        /// </summary>
        public void PopLocalTransform()
        {
            if (_localPositionStack != null && _localPositionStack.Count > 0)
            {
                transform.localPosition = _localPositionStack.Pop();
                transform.localRotation = _localRotationStack.Pop();
                transform.localScale    = _localScaleStack.Pop();
            }
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
        ///     Also registers the unique ID at runtime.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                // Only store data in play mode
                return;
            }

            // Force the UxrManager singleton to be created at this point
            UxrManager.Instance.Poke();

            // Store initial transform data
            RecomputeInitialTransformData();

            // Register unique ID
            CheckInitializeUniqueId();
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
        ///     Compute unique ID for network/state-save syncing.
        /// </summary>
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR

            string InternalGetUniqueId()
            {
                if (UniqueIdIsTypeName)
                {
                    // Unique ID will be based on the type name. This is useful for singletons that are instantiated dynamically, in order to ensure same values on all devices.
                    return GetType().FullName.GetMd5x2();
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
                if (gameObject.scene.name != null && !gameObject.scene.isLoaded)
                {
                    // Returning from play-mode, re-loading after build, among others.  
                    return;
                }

                bool setDirty = false;

                if (string.IsNullOrEmpty(_uxrUniqueId))
                {
                    // Generate unique ID
                    _uxrUniqueId = InternalGetUniqueId();
                    setDirty     = true;
                }

                // Check if a prefab was instantiated or an object was made prefab, to generate a different ID.
                // We want to avoid multiple instantiated prefabs to share the same ID.

                try
                {
                    // Sometimes, in prefab mode, GetPrefabGuid() can throw an exception when accessing
                    // PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot during OnValidate().
                    // It seems to happen when loading the prefab for the first time.

                    if (this.GetPrefabGuid(out string prefabGuid, out string assetPath))
                    {
                        if (this.IsInPrefab() != __isInPrefab || prefabGuid != __prefabGuid)
                        {
                            _uxrUniqueId = InternalGetUniqueId();
                            __isInPrefab = this.IsInPrefab();
                            __prefabGuid = prefabGuid;
                            setDirty     = true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore exception.
                }

                if (setDirty)
                {
                    EditorUtility.SetDirty(this);
                }
            }

#endif
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="StateChanged" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        private void OnStateChanged(UxrSyncEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

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

        #region Protected Internal Methods

        /// <summary>
        ///     <para>
        ///         Registers the <see cref="UxrComponent" />, making sure that its Unique ID is available enabling it
        ///         to receive synchronization messages. If the component was already registered, the call is ignored.
        ///     </para>
        ///     <para>
        ///         <see cref="UxrComponent" />s are automatically registered during <see cref="Awake" /> completely
        ///         transparent to the user.
        ///         For objects that are initially disabled, however, this means that they will not be able to receive
        ///         synchronization messages because their Unique ID has not been registered yet.<br />
        ///         If a component gets enabled on a remote session, and sends a state synchronization message, the other
        ///         devices where the object is disabled will not be able to find it. Calling this method forces the object
        ///         to be registered, making it possible to receive messages.
        ///     </para>
        /// </summary>
        /// <returns></returns>
        protected internal void RegisterComponent()
        {
            CheckInitializeUniqueId();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Executes the state change described by <see cref="e" />. To be implemented in child classes
        ///     that have custom state synchronization.
        /// </summary>
        /// <param name="e">State change information</param>
        protected virtual void SyncState(UxrSyncEventArgs e)
        {
        }

        /// <summary>
        ///     <para>
        ///         Starts a synchronization block that will end with an EndSync method like <see cref="EndSyncProperty" />,
        ///         <see cref="EndSyncMethod" /> or <see cref="EndSyncState" />, which causes the <see cref="StateChanged" /> event
        ///         to be triggered.
        ///     </para>
        ///     <para>
        ///         All application <see cref="StateChanged" /> events can be processed using the single output point
        ///         <see cref="UxrManager.ComponentStateChanged" /> in <see cref="UxrManager" />, which means that all changes
        ///         can be tracked for easy multiplayer or replay functionality.
        ///     </para>
        ///     <para>
        ///         To be able to execute these changes on other PCs, state changes can be serialized to a byte array using
        ///         <see cref="UxrSyncEventArgs.SerializeEventBinary" />. These byte arrays can be saved to disk for replays or
        ///         sent through network for multiplayer synchronization.<br />
        ///         Executing an event serialized in a byte array can be done using <see cref="UxrManager.ExecuteStateChange" />.
        ///     </para>
        /// </summary>
        protected void BeginSync()
        {
            SyncCallDepth++;

            if (SyncCallDepth > StateSyncCallDepthErrorThreshold)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/EndSync mismatch when calling BeginSync. Did you forget an EndSync call? Component: {this.GetPathUnderScene()}");
                }
            }
        }

        /// <summary>
        ///     Cancels a <see cref="BeginSync" /> to escape when a condition is found that makes it not require to sync.
        /// </summary>
        /// <example>
        ///     <para>Synchronizing a method call</para>
        ///     <code>
        ///     public void MyMethod(int parameter1, bool parameter2)
        ///     {
        ///         BeginSync();
        ///         int i = GetValue();
        ///         if (i > 0)
        ///         {
        ///             CancelSync();
        ///             return;
        ///         }
        ///         DoSomething(parameter1);
        ///         DoSomethingElse(parameter2);
        ///         EndSyncMethod(new object[] { parameter1, parameter2 });
        ///     }
        ///     </code>
        /// </example>
        protected void CancelSync()
        {
            if (SyncCallDepth > 0)
            {
                SyncCallDepth--;
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/CancelSync mismatch when calling CancelSync. Did you forget a BeginSync call? State call depth is < 1. Component: {this.GetPathUnderScene()}");
                }
            }
        }

        /// <summary>
        ///     Ends synchronization for a property change. It notifies that a property was changed in a component that
        ///     requires network/state synchronization, ensuring that the change is performed in all other clients too.
        ///     The synchronization should begin using <see cref="BeginSync" />.
        /// </summary>
        /// <param name="value">
        ///     New property value (see <see cref="UxrVarType" /> for supported types). To add support for custom
        ///     types it is possible to implement the <see cref="IUxrSerializable" /> interface and use the
        ///     <see cref="BinaryReaderExt" /> and <see cref="BinaryWriterExt" /> extension methods for easy
        ///     serialization/deserialization
        /// </param>
        /// <param name="propertyName">Property name</param>
        /// <seealso cref="BeginSync" />
        /// <example>
        ///     <para>Synchronizing a property change</para>
        ///     <code>
        ///     public int Property
        ///     {
        ///         get => _property;
        ///         set
        ///         {
        ///             BeginSync();
        ///             _property = value;
        ///             EndSyncProperty(value);
        ///         }
        ///     }
        /// </code>
        /// </example>
        protected void EndSyncProperty(in object value, [CallerMemberName] string propertyName = null)
        {
            EndSyncState(new UxrPropertyChangedSyncEventArgs(propertyName, value));
        }

        /// <summary>
        ///     Ends synchronization for a method call. It notifies that a method was invoked in a component that requires
        ///     network/state synchronization, ensuring that the call is performed in all other clients too.
        ///     The synchronization should begin using <see cref="BeginSync" />.
        /// </summary>
        /// <param name="parameters">
        ///     Method call parameters. This must be the exact same parameters as the receiver or the
        ///     synchronization will not work correctly. See <see cref="UxrVarType" /> for all the supported parameter types.
        ///     To add support for custom types it is possible to implement the <see cref="IUxrSerializable" /> interface and
        ///     use the <see cref="BinaryReaderExt" /> and <see cref="BinaryWriterExt" /> extension methods for easy
        ///     serialization/deserialization.
        /// </param>
        /// <param name="methodName">Method name (will be passed automatically using the [CallerMemberName] attribute</param>
        /// <seealso cref="BeginSync" />
        /// <example>
        ///     <para>Synchronizing a method call</para>
        ///     <code>
        ///     public void MyMethod(int parameter1, bool parameter2)
        ///     {
        ///         BeginSync();
        ///         DoSomething(parameter1);
        ///         DoSomethingElse(parameter2);
        ///         EndSyncMethod(new object[] { parameter1, parameter2 });
        ///     }
        ///     </code>
        /// </example>
        protected void EndSyncMethod(object[] parameters = null, [CallerMemberName] string methodName = null)
        {
            EndSyncState(new UxrMethodInvokedSyncEventArgs(methodName, parameters));
        }

        /// <summary>
        ///     Ends a synchronization block for a custom event. The synchronization block should begin using
        ///     <see cref="BeginSync" />. The event ensures that the code is executed in all other receivers too.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <seealso cref="BeginSync" />
        /// <example>
        ///     <para>
        ///         Note that this example could be easily solved using a simple <see cref="BeginSync" /> and
        ///         <see cref="EndSyncMethod" /> block, but this is to illustrate how custom event synchronization works.<br />
        ///         Synchronizing a custom event with two parameters (see <see cref="UxrVarType" /> for all supported parameter
        ///         types)
        ///     </para>
        ///     <code>
        ///     public class LaserShot : UxrComponent
        ///     {
        ///         // Override this method that will receive custom events that need to be synchronized. For example, on
        ///         // all clients in a multiplayer environment.
        ///         protected override void SyncState(UxrSyncEventArgs e)
        ///         {
        ///             if (e is CoolLaserSyncEventArgs eventArgs)
        ///             {
        ///                 ShowCoolLaser(e.Position, e.Distance);
        ///             }
        ///         }
        /// 
        ///         void Update()
        ///         {
        ///             // Do we need to show a cool laser? Make sure to show it on all other clients too
        ///             if (buttonWasPressed &amp;&amp; GetPositionAndDistance(out Vector3 position, out float distance))
        ///             {
        ///                 BeginSync();
        ///                 ShowCoolLaser(position, distance);
        ///                 EndSyncState(new MyCoolLaserSyncEventArgs(position, distance));
        ///             } 
        ///         } 
        ///     }
        ///     // The custom event class will inherit from UxrSyncEventArgs and need to provide some methods. 
        ///     public CoolLaserSyncEventArgs : UxrSyncEventArgs
        ///     {
        ///         public Vector3 Position { get; set; }
        ///         public float   Distance { get; set; }
        /// 
        ///         // Constructor
        ///         public CoolLaserSyncEventArgs(Vector3 position, float distance)
        ///         {
        ///             Position = position;
        ///             Distance = distance;
        ///         }
        /// 
        ///         // Constructor using BinaryReader. This is mandatory because the deserialization
        ///         // will look for a constructor with BinaryReader as a parameter.
        ///         public CoolLaserSyncEventArgs(BinaryReader reader)
        ///         {
        ///             DeserializeEventInternal(reader);
        ///         }
        /// 
        ///         // This is optional but useful to have because the networking logging will output a readable string.
        ///         public override ToString()
        ///         {
        ///             return S"CoolLaserSyncEventArgs with position {Position} and distance {Distance}";
        ///         }
        /// 
        ///         // Event serializer
        ///         protected override void SerializeEventInternal(BinaryWriter writer)
        ///         {
        ///             writer.Write(Position);
        ///             writer.Write(Distance);
        ///         }
        /// 
        ///         // Event deserializer
        ///         protected override void DeserializeEventInternal(BinaryReader reader)
        ///         {
        ///             Position = reader.ReadVector3();
        ///             Distance = reader.ReadInt32();
        ///         }
        ///     }
        ///     </code>
        /// </example>
        protected void EndSyncState(UxrSyncEventArgs e)
        {
            if (SyncCallDepth > 0)
            {
                OnStateChanged(e);
                SyncCallDepth--;
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/EndSync mismatch when calling EndSync. Did you forget a BeginSync call? State call depth is < 1. Component: {this.GetPathUnderScene()}");
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Returns a generated unique ID based on the given component, taking care of collisions.
        /// </summary>
        /// <param name="component">Component to get a unique ID for</param>
        /// <param name="requestedId">
        ///     If non-null, it will try to assign this Id. If it already exists
        ///     or is null, it will generate a new one.
        /// </param>
        /// <returns>New generated ID</returns>
        private static string GenerateUniqueId(UxrComponent component, string requestedId = null)
        {
            string unprocessedId = requestedId ?? GetNewUniqueId();
            string newId         = unprocessedId;
            string unregisterId  = null;

            // First unregister if it was already registered

            if (!string.IsNullOrEmpty(component.UniqueId) && s_componentsById.TryGetValue(component.UniqueId, out UxrComponent existingComponent) && component == existingComponent)
            {
                unregisterId = component.UniqueId;
            }

            // Try to get new unique ID

            if (s_componentsById.ContainsKey(newId))
            {
                // Handle collisions

                int collisionIterations = 0;

                while (s_componentsById.ContainsKey(newId))
                {
                    int collisionCount = s_idCollisions[unprocessedId];

                    collisionCount++;
                    collisionIterations++;
                    newId = (unprocessedId + $"Collision{collisionCount}").GetMd5x2();

                    s_idCollisions[unprocessedId] = collisionCount;
                }

                s_idCollisions[newId] = 0;
            }
            else
            {
                // New unique ID not in use: Initialize or update collision dictionary for this ID

                if (!s_idCollisions.ContainsKey(unprocessedId))
                {
                    s_idCollisions.Add(unprocessedId, 0);
                }
                else
                {
                    s_idCollisions[unprocessedId]++;
                }
            }

            // Call ID change events

            if (unregisterId != null)
            {
                component.OnUniqueIdChanging(unregisterId, newId);
                s_componentsById.Remove(unregisterId);
            }

            // Register new ID

            component.OnRegistering();
            component.UniqueId = newId;

            // This should not happen unless a new singleton of a class is instantiated. The old one would be deleted later but since
            // they share the same ID we want to remove the old one here first too.

            if (s_componentsById.ContainsKey(newId))
            {
                s_componentsById[newId] = component;
            }
            else
            {
                s_componentsById.Add(newId, component);
            }

            component.OnRegistered();

            // Call ID change events

            if (unregisterId != null)
            {
                component.OnUniqueIdChanged(unregisterId, newId);
            }

            return newId;
        }

        /// <summary>
        ///     Checks if the component has been registered, and registers it if it not.
        ///     The main reason why it exists is to make sure that it has been called if ChangeUniqueId()
        ///     changes the ID. Since this might happen right after instantiation, before Awake() gets called at
        ///     the end of the frame, it might need to be called there before.
        /// </summary>
        private void CheckInitializeUniqueId()
        {
            if (_initializedComponent == this)
            {
                return;
            }

            // Not yet generated? Generate ID using unique scene path.
            // Warning: This is not 100% safe for several reasons:
            //   -In Unity root GameObjects don't have a sibling index at runtime
            //   -Index in root GameObjects is not consistent across platforms
            //   -Can create collisions when instantiating, but these are taken care of in the next step.
            if (string.IsNullOrEmpty(UniqueId))
            {
                if (UniqueIdIsTypeName)
                {
                    UniqueId = GetType().FullName.GetMd5x2();
                }
                else
                {
                    UniqueId = this.GetUniqueScenePath().GetMd5x2();
                }
            }

            // Store original unique ID
            _originalUniqueId = UniqueId;

            // Register, taking care of collisions
            GenerateUniqueId(this, UniqueId);
            _initializedComponent = this;
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     <para>
        ///         Gets the current call depth of BeginSync/EndSync calls, which are responsible for helping synchronize calls
        ///         over the network.
        ///         To avoid redundant synchronization, nested calls (where <see cref="SyncCallDepth" /> is greater than 1),
        ///         need to be ignored.
        ///     </para>
        ///     <para>
        ///         State synchronization, for networking or other functionality like saving gameplay replays, can be done
        ///         by subscribing to <see cref="UxrManager.ComponentStateChanged" />. By default, only top level calls will
        ///         trigger the event. This can be changed using <see cref="UxrManager.UseTopLevelStateChangesOnly" />.
        ///     </para>
        ///     <para>
        ///         In the following code, only PlayerShoot() needs to be synchronized. This will not only save bandwidth, but also
        ///         make sure that only a single particle system gets instantiated and the shot audio doesn't get played twice.
        ///     </para>
        ///     <code>
        ///         void PlayerShoot(int parameter1, bool parameter2)
        ///         {
        ///             BeginSync(); 
        ///             ShowParticles(parameter1);
        ///             PlayAudioShot(parameter2); 
        ///             EndSyncMethod(new object[] {parameter1, parameter2});
        ///         }
        ///         
        ///         void ShowParticles(int parameter);
        ///         {
        ///             BeginSync();
        ///             Instantiate(ParticleSystem);
        ///             EndSyncMethod(new object[] {parameter});
        ///         }
        ///         
        ///         void PlayAudioShot(bool parameter);
        ///         {
        ///             BeginSync();
        ///             _audio.Play();
        ///             EndSyncMethod(new object[] {parameter});
        ///         }
        ///     </code>
        /// </summary>
        protected static int SyncCallDepth { get; private set; }

        /// <summary>
        ///     Gets whether the unique ID for this component is generated based on the full type name. This is useful to let
        ///     singletons generate the same Unique ID in all devices to ensure that message exchanges will work correctly.
        /// </summary>
        protected virtual bool UniqueIdIsTypeName => false;

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     The original ID that was assigned at startup, which might be different than the one assigned during edit-time.
        ///     We want to keep the original one for <see cref="ChangeUniqueId" /> when computing IDs relative to the parent.
        /// </summary>
        private string OriginalUniqueId => _originalUniqueId ?? UniqueId;

        private const BindingFlags EventFlags                       = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags MethodFlags                      = EventFlags | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;
        private const BindingFlags PropertyFlags                    = EventFlags | BindingFlags.SetProperty;
        private const int          StateSyncCallDepthErrorThreshold = 100;

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

        private UxrComponent _initializedComponent;
        private string       _originalUniqueId;

        private Stack<Vector3>    _localPositionStack;
        private Stack<Quaternion> _localRotationStack;
        private Stack<Vector3>    _localScaleStack;
        private Stack<Vector3>    _positionStack;
        private Stack<Quaternion> _rotationStack;
        private Stack<Vector3>    _scaleStack;

        #endregion
    }
}