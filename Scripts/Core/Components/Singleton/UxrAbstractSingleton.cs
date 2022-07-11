// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAbstractSingleton.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using UltimateXR.Core.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace UltimateXR.Core.Components.Singleton
{
    /// <summary>
    ///     <para>
    ///         A singleton base class that can be used with abstract classes.
    ///     </para>
    ///     <para>
    ///         The difference with <see cref="UxrSingleton{T}" /> is that <see cref="UxrSingleton{T}" /> guarantees
    ///         that an instance will always be available in the scene by instantiating the component if it's not found.
    ///         This means <see cref="UxrSingleton{T}.Instance" /> will always be non-null and can be used with or
    ///         without an instance available in the scene. <see cref="UxrSingleton{T}" /> also allows to use automatic
    ///         prefab instantiation if a compatible singleton prefab is present in a special Resources folder.
    ///         Since abstract classes can't be instantiated, <see cref="Instance" /> in <see cref="UxrAbstractSingleton{T}" />
    ///         will be non-null only if a child component is available somewhere in the scene.
    ///     </para>
    ///     <para>
    ///         For design purposes, a singleton may still be desirable when programming an abstract class, hence
    ///         this <see cref="UxrAbstractSingleton{T}" /> component base class.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">Class the singleton is for</typeparam>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Make sure to call base.Awake() first in child classes where <see cref="Awake" /> is used.</item>
    ///         <item>Use <see cref="HasInstance" /> to check whether the instance exists.</item>
    ///     </list>
    /// </remarks>
    public abstract class UxrAbstractSingleton<T> : UxrComponent where T : UxrAbstractSingleton<T>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the unique, global instance of the given component.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_instance is null)
                {
                    UxrMonoDispatcher.RunOnMainThread(FindInstance);
                    while (s_instance is null && !UxrMonoDispatcher.IsCurrentThreadMain)
                    {
                        Thread.Sleep(25);
                    }
                }

                return s_instance;
            }
        }

        /// <summary>
        ///     Gets whether there is a singleton instance available.
        /// </summary>
        public static bool HasInstance => s_instance != null;

        /// <summary>
        ///     Gets or sets whether the singleton has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Tries to set the singleton instance.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            TrySetInstance(this);
        }

        /// <summary>
        ///     Destroys the singleton instance.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ReferenceEquals(this, s_instance))
            {
                Release();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Gets the singleton instance.
        /// </summary>
        /// <returns></returns>
        protected static T GetInstance()
        {
            return s_instance;
        }

        /// <summary>
        ///     Tries to set the singleton instance.
        /// </summary>
        /// <param name="value">Candidate to set as singleton instance</param>
        /// <returns>Whether the instance was set</returns>
        protected static bool TrySetInstance(UxrAbstractSingleton<T> value)
        {
            if (value is null)
            {
                return false;
            }

            if (ReferenceEquals(s_instance, value))
            {
                return true;
            }

            if (!(s_instance is null))
            {
                Debug.LogWarning($"[{typeof(T).Name}] singleton already added. Destroying second instance @ {value.gameObject}", value);
                Destroy(value);
                return false;
            }

            Assert.IsTrue(value is T, $"Incoherent types: {value.GetType().Name} vs {typeof(T).Name}");

            if (Application.isPlaying && value.NeedsDontDestroyOnLoad)
            {
                DontDestroyOnLoad(value.gameObject);
            }

            s_instance = (T)value;
            s_instance.Init();

            Debug.Log($"[{typeof(T).Name}] singleton successfully initialized", s_instance);
            return true;
        }

        /// <summary>
        ///     The default internal initialization. Child classes can override this method if they require initialization code.
        /// </summary>
        /// <param name="initializedCallback">Callback called when the initialization finished.</param>
        protected virtual void InitInternal(Action initializedCallback)
        {
            initializedCallback?.Invoke();
        }

        /// <summary>
        ///     The default internal release. Child classes can override this method if they required deallocation code.
        /// </summary>
        /// <param name="releasedCallback">Callback called when the releasing finished.</param>
        protected virtual void ReleaseInternal(Action releasedCallback)
        {
            releasedCallback?.Invoke();
        }

        #endregion

        #region Private Methods

/*
        /// <summary>
        ///     Called by Unity before a scene is loaded. Calls <see cref="TryFindInstance" />.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            TryFindInstance();
        }
*/
        /// <summary>
        ///     Tries to find a pre-existing instance in the scene and set it as the singleton instance.
        /// </summary>
        /// <returns></returns>
        private static bool TryFindInstance()
        {
            return TrySetInstance(FindObjectOfType<T>());
        }

        /// <summary>
        ///     Tries to find a pre-existing instance in the scene.
        /// </summary>
        private static void FindInstance()
        {
            TryFindInstance();
        }

        /// <summary>
        ///     Initializes the singleton.
        /// </summary>
        private void Init()
        {
            InitInternal(() => IsInitialized = true);
        }

        /// <summary>
        ///     Releases any resources allocated by the singleton.
        /// </summary>
        private void Release()
        {
            ReleaseInternal(() =>
                            {
                                if (!IsApplicationQuitting)
                                {
                                    s_instance = null;
                                }
                            });
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Whether the singleton requires <see cref="UnityEngine.Object.DontDestroyOnLoad" /> applied to the GameObject so
        ///     that it doesn't get destroyed when a new scene is loaded.
        /// </summary>
        protected virtual bool NeedsDontDestroyOnLoad => true;

        #endregion

        #region Private Types & Data

        private static T s_instance;

        #endregion
    }
}