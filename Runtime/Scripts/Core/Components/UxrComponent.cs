// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponent.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UltimateXR.Attributes;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.StateSave;
using UltimateXR.Core.StateSync;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.System.IO;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     Base class for all components in UltimateXR. It provides built-in functionality such as:
    ///     <list type="bullet">
    ///         <item>
    ///             Provide access to all components statically using <see cref="AllComponents" /> or
    ///             <see cref="EnabledComponents" />.
    ///         </item>
    ///         <item>
    ///             Identify each component using an unique ID, and be able to get a component using
    ///             <see cref="UxrUniqueIdImplementer.TryGetComponentById" />.
    ///         </item>
    ///         <item>
    ///             Synchronize the state and changes of all components through the network, or serialize them to disk.
    ///             This provides native multiplayer support, state saving and replay functionality.
    ///         </item>
    ///         <item>Caching of Unity components.</item>
    ///         <item>Caching of initial transform values and other transform utilities.</item>
    ///     </list>
    ///     Custom components that do not inherit from <see cref="UxrComponent" /> can still integrate with UltimateXR
    ///     functionality such as:
    ///     <list type="bullet">
    ///         <item>Unique ID component identification and access.</item>
    ///         <item>State synchronization over network.</item>
    ///         <item>Save state to disk.</item>
    ///         <item>Integration with the replay system.</item>
    ///     </list>
    ///     Custom user components that desire this built-in functionality of <see cref="UxrComponent" /> but cannot
    ///     inherit due to multiple inheritance limitations can still achieve the desired functionality implementing the
    ///     following interfaces:<br />
    ///     <see cref="IUxrUniqueId" />, <see cref="IUxrStateSave" /> and <see cref="IUxrStateSync" />.<br />
    ///     UltimateXR provides <see cref="UxrUniqueIdImplementer{T}" />, <see cref="UxrStateSaveImplementer{T}" />,
    ///     <see cref="UxrStateSyncImplementer" />, and <see cref="UxrBinarySerializer" /> to facilitate the implementation of
    ///     all required interfaces.
    /// </summary>
    /// <remarks>
    ///     When inheriting, make sure to override any Unity methods used such as Awake(), Start(), OnEnable() etc, and call
    ///     the base implementation in the body using base.Awake(), base.Start() etc.<br />
    ///     Components get registered through their Awake() call. Although objects that are initially disabled in the scene get
    ///     registered, components that are instantiated at runtime and are initially disabled don't get registered until they
    ///     are enabled the first time.
    ///     Unregistered objects can't be accessed using <see cref="UxrUniqueIdImplementer.TryGetComponentById" /> or
    ///     enumerated using
    ///     <see cref="AllComponents" /> or similar.
    ///     To overcome this limitation, <see cref="RegisterUniqueIdIfNecessary" /> can be called on the disabled objects to
    ///     register them manually.
    /// </remarks>
    public abstract class UxrComponent : MonoBehaviour, IUxrStateSave, IUxrStateSync
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
        public static event Action<Guid, Guid> GlobalIdChanging;

        /// <summary>
        ///     Called when a component changed its unique id by using <see cref="ChangeUniqueId" />.
        ///     Parameters are oldId, newId.
        /// </summary>
        public static event Action<Guid, Guid> GlobalIdChanged;

        /// <summary>
        ///     Gets all the components, enabled or not, in all open scenes.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="GameObject.GetComponentsInChildren{T}(bool)" /> or
        ///     <see cref="UnityEngine.Object.FindObjectsOfType{T}(bool)" />.
        /// </remarks>
        public static IEnumerable<UxrComponent> AllComponents => UxrUniqueIdImplementer<UxrComponent>.ComponentsById.Values.Where(c => c != null);

        /// <summary>
        ///     Gets all components that are enabled, in all open scenes.
        /// </summary>
        public static IEnumerable<UxrComponent> EnabledComponents => AllComponents.Where(c => c != null && c.isActiveAndEnabled);

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

        #region Implicit IUxrStateSave

        /// <inheritdoc />
        public virtual UxrTransformSpace TransformStateSaveSpace => UxrTransformSpace.World;

        /// <inheritdoc />
        public int StateSerializationVersion => 0;

        /// <inheritdoc />
        public virtual bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            return false;
        }

        /// <inheritdoc />
        public bool SerializeState(IUxrSerializer serializer, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            int serializeCounter = StateSaveImplementer.SerializeCounter;

            _stateSerializer = serializer;
            StateSaveImplementer.SerializeState(serializer, level, options, SerializeStateInternal);

            return StateSaveImplementer.SerializeCounter != serializeCounter;
        }

        #endregion

        #region Implicit IUxrStateSync

        /// <inheritdoc />
        public event EventHandler<UxrSyncEventArgs> StateChanged;

        /// <inheritdoc />
        public void SyncState(UxrSyncEventArgs e)
        {
            StateSyncImplementer.SyncState(e, SyncStateInternal);
        }

        #endregion

        #region Implicit IUxrUniqueId

        /// <inheritdoc />
        public virtual bool UniqueIdIsTypeName => false;

        /// <inheritdoc />
        public Guid UniqueId
        {
            get
            {
                // Cache the Guid if it hasn't been cached yet. The cached id is derived from parsing the _uxrUniqueId string,
                // which represents the Guid. Unity doesn't serialize Guids directly, requiring the caching approach.

                if (_cachedGuid == default && !string.IsNullOrEmpty(_uxrUniqueId))
                {
                    _cachedGuid = new Guid(_uxrUniqueId);
                }

                return _cachedGuid;
            }

            private set
            {
                _uxrUniqueId = value.ToString();
                _cachedGuid  = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Registers the <see cref="UxrComponent" />, making sure that its Unique ID is available enabling it
        ///         to exchange synchronization messages. If the component was already registered, the call is ignored.
        ///     </para>
        ///     <para>
        ///         <see cref="UxrComponent" />s are automatically registered during <see cref="Awake" /> completely
        ///         transparent to the user.<br />
        ///         For objects that are initially disabled, however, this means that they will not be able to receive
        ///         synchronization messages because their Unique ID has not been registered yet.<br />
        ///         If a component gets enabled on a remote session, and sends a state synchronization message, the other
        ///         devices where the object is disabled will not be able to find it. Calling this method forces the object
        ///         to be registered, making it possible to exchange messages.
        ///     </para>
        /// </summary>
        /// <returns></returns>
        public void RegisterUniqueIdIfNecessary()
        {
            UniqueIdImplementer.InitializeUniqueIdIfNecessary(this,
                                                              (c, id) => c.UniqueId = id,
                                                              c => c.UniqueIdImplementer,
                                                              (c, oldId, newId) => c.OnUniqueIdChanging(oldId, newId),
                                                              (c, oldId, newId) => c.OnUniqueIdChanged(oldId, newId),
                                                              c => c.OnRegistering(),
                                                              c => c.OnRegistered());
        }

        /// <inheritdoc />
        public Guid ChangeUniqueId(Guid newUniqueId, bool recursive = false)
        {
            return UniqueIdImplementer.ChangeUniqueId(newUniqueId,
                                                      (c, id) => c.UniqueId = id,
                                                      c => c.UniqueIdImplementer,
                                                      (c, oldId, newId) => c.OnUniqueIdChanging(oldId, newId),
                                                      (c, oldId, newId) => c.OnUniqueIdChanged(oldId, newId),
                                                      c => c.OnRegistering(),
                                                      c => c.OnRegistered(),
                                                      recursive);
        }

        #endregion

        #region Explicit IUxrStateSync

        /// <inheritdoc />
        string IUxrStateSync.Name => StateSyncName;

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
            RegisterUniqueIdIfNecessary();
        }

        /// <summary>
        ///     Unity <see cref="OnDestroy" /> handling.
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnUnregistering();
            UniqueIdImplementer.NotifyOnDestroy();
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
            StateSyncImplementer.NotifyOnEnable();
            GlobalEnabled?.Invoke(this);

            if (!_initialStateSerialized)
            {
                StartCoroutine(NotifyEndOfFirstFrameCoroutine());
            }
        }

        /// <summary>
        ///     Unity <see cref="OnDisable" /> handling.
        /// </summary>
        protected virtual void OnDisable()
        {
            StateSyncImplementer.NotifyOnDisable();
            GlobalDisabled?.Invoke(this);
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
            UniqueIdImplementer.NotifyOnValidate((c, id) => c.UniqueId = id, ref __isInPrefab, ref __prefabGuid);
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that will wait until the end of the first frame and cache the initial state of the component for
        ///     serialization.
        /// </summary>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator NotifyEndOfFirstFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();

            StateSaveImplementer.NotifyEndOfFirstFrame();
            _initialStateSerialized = true;
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
        ///     <see cref="GlobalIdChanging" /> event trigger.
        /// </summary>
        /// <param name="oldId">Old id</param>
        /// <param name="newId">New id</param>
        private void OnUniqueIdChanging(Guid oldId, Guid newId)
        {
            GlobalIdChanging?.Invoke(oldId, newId);
        }

        /// <summary>
        ///     <see cref="GlobalIdChanged" /> event trigger.
        /// </summary>
        /// <param name="oldId">Old id</param>
        /// <param name="newId">New id</param>
        private void OnUniqueIdChanged(Guid oldId, Guid newId)
        {
            GlobalIdChanged?.Invoke(oldId, newId);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Serializes the component state. To be implemented in child classes that have custom state saving.
        ///     Serialization will be performed with the help of <see cref="SerializeStateValue{T}" />.
        /// </summary>
        /// <param name="isReading">Whether the serializer is reading or writing</param>
        /// <param name="stateSerializationVersion">
        ///     When reading it tells the <see cref="StateSerializationVersion" /> the data was
        ///     serialized with. When writing it uses the latest <see cref="StateSerializationVersion" /> version.
        /// </param>
        /// <param name="level">
        ///     The amount of data to read/write.
        /// </param>
        /// <param name="options">Options</param>
        protected virtual void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
        }

        /// <summary>
        ///     Executes the state change described by <see cref="e" />. To be implemented in child classes
        ///     that have custom state synchronization.
        /// </summary>
        /// <param name="e">State change information</param>
        protected virtual void SyncStateInternal(UxrSyncEventArgs e)
        {
        }

        /// <summary>
        ///     Returns <see cref="UxrTransformSpace.Local" /> if the object is parented to an object with a
        ///     <see cref="IUxrUniqueId" /> component.
        ///     Otherwise returns the space passed as parameter.
        /// </summary>
        /// <param name="alternative">
        ///     Alternative space returned if the object isn't parented to an object with
        ///     <see cref="IUxrUniqueId" />
        /// </param>
        /// <returns>Space</returns>
        protected UxrTransformSpace GetLocalTransformIfParentedOr(UxrTransformSpace alternative)
        {
            if (transform.parent != null && transform.parent.GetComponent<IUxrUniqueId>() != null)
            {
                return UxrTransformSpace.Local;
            }

            return alternative;
        }

        /// <summary>
        ///     See <see cref="UxrStateSaveImplementer{T}.SerializeStateValue{TV}" />.
        /// </summary>
        protected void SerializeStateValue<T>(UxrStateSaveLevel level, UxrStateSaveOptions options, string name, ref T value)
        {
            StateSaveImplementer.SerializeStateValue(_stateSerializer, level, options, name, ref value);
        }

        /// <summary>
        ///     See <see cref="UxrStateSaveImplementer{T}.SerializeStateTransform" />.
        /// </summary>
        protected void SerializeStateTransform(UxrStateSaveLevel level, UxrStateSaveOptions options, string name, UxrTransformSpace space, Transform transform)
        {
            if (transform != null)
            {
                StateSaveImplementer.SerializeStateTransform(_stateSerializer, level, options, name, space, transform);
            }
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
        ///         Executing an event serialized in a byte array can be done using <see cref="UxrManager.ExecuteStateSyncEvent" />
        ///         .
        ///     </para>
        /// </summary>
        /// <param name="targetEnvironments">Target environment(s) where the state sync should be used/saved. It's All by default.</param>
        protected void BeginSync(UxrStateSyncEnvironments targetEnvironments = UxrStateSyncEnvironments.All)
        {
            StateSyncImplementer.BeginSync(targetEnvironments);
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
            StateSyncImplementer.CancelSync();
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
        ///         protected override void SyncStateInternal(UxrSyncEventArgs e)
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
            StateSyncImplementer.EndSyncState(OnStateChanged, e);
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets the name of the object, to identify it in debug strings. Uses Unity's name object by default, but it can be
        ///     overriden to provide a better, comprehensible name.
        /// </summary>
        /// <returns>Name of the object</returns>
        protected virtual string StateSyncName => name;

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the implementer for the <see cref="IUxrStateSave" /> interface.
        /// </summary>
        private UxrUniqueIdImplementer<UxrComponent> UniqueIdImplementer
        {
            get
            {
                if (_uniqueIdImplementer == null)
                {
                    _uniqueIdImplementer = new UxrUniqueIdImplementer<UxrComponent>(this);
                }

                return _uniqueIdImplementer;
            }
        }

        /// <summary>
        ///     Gets the implementer for the <see cref="IUxrStateSave" /> interface.
        /// </summary>
        private UxrStateSaveImplementer<UxrComponent> StateSaveImplementer
        {
            get
            {
                if (_stateSaveImplementer == null)
                {
                    _stateSaveImplementer = new UxrStateSaveImplementer<UxrComponent>(this);
                }

                return _stateSaveImplementer;
            }
        }

        /// <summary>
        ///     Gets the implementer for the <see cref="IUxrStateSync" /> interface.
        /// </summary>
        private UxrStateSyncImplementer<UxrComponent> StateSyncImplementer
        {
            get
            {
                if (_stateSyncImplementer == null)
                {
                    _stateSyncImplementer = new UxrStateSyncImplementer<UxrComponent>(this);
                }

                return _stateSyncImplementer;
            }
        }

        // Private vars

        private readonly Dictionary<Type, Component> _cachedComponents = new Dictionary<Type, Component>();

        private Guid _cachedGuid;

        // Helpers that leverage implementation of IUxrUniqueId, IUxrStateSync and IUxrStateSave

        private UxrUniqueIdImplementer<UxrComponent>  _uniqueIdImplementer;
        private UxrStateSaveImplementer<UxrComponent> _stateSaveImplementer;
        private UxrStateSyncImplementer<UxrComponent> _stateSyncImplementer;
        private IUxrSerializer                        _stateSerializer;
        private bool                                  _initialStateSerialized;

        // Transform stacks

        private Stack<Vector3>    _localPositionStack;
        private Stack<Quaternion> _localRotationStack;
        private Stack<Vector3>    _localScaleStack;
        private Stack<Vector3>    _positionStack;
        private Stack<Quaternion> _rotationStack;
        private Stack<Vector3>    _scaleStack;

        #endregion
    }
}