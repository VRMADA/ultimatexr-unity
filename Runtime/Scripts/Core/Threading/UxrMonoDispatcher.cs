// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMonoDispatcher.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Threading;
using UnityEngine;

namespace UltimateXR.Core.Threading
{
    /// <summary>
    ///     A dispatcher that helps ensuring code runs on the main thread. Most Unity user functionality requires to be called
    ///     from the main thread.
    /// </summary>
    public sealed partial class UxrMonoDispatcher : UxrComponent
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the caller is running on the main thread.
        /// </summary>
        public static bool IsCurrentThreadMain => !Application.isPlaying || s_mainThread == Thread.CurrentThread;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Runs code on the main thread.
        /// </summary>
        /// <param name="action">
        ///     Action to execute.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="action" /> is <see langword="null" />
        /// </exception>
        /// <exception cref="Exception">
        ///     A delegate callback throws an exception.
        /// </exception>
        public static void RunOnMainThread(Action action)
        {
            action.ThrowIfNull(nameof(action));

            if (!Application.isPlaying || Thread.CurrentThread == s_mainThread)
            {
                action();
            }
            else
            {
                Instance.Enqueue(action);
                Instance.StartDispatching();
            }
        }

        /// <summary>
        ///     Runs code on the main thread.
        /// </summary>
        /// <param name="actions">
        ///     An variable set of actions that will run on the main thread sequentially.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="actions" /> is <see langword="null" />
        /// </exception>
        /// <exception cref="Exception">
        ///     A delegate callback throws an exception.
        /// </exception>
        public static void RunOnMainThread(params Action[] actions)
        {
            if (!Application.isPlaying || Thread.CurrentThread == s_mainThread)
            {
                foreach (Action action in actions)
                {
                    action();
                }
            }
            else
            {
                foreach (Action action in actions)
                {
                    Instance.Enqueue(action);
                }

                Instance.StartDispatching();
            }
        }

        /// <summary>
        ///     Runs code on the main thread, asynchronously.
        /// </summary>
        /// <param name="ct">
        ///     Cancellation token that allows to cancel the task.
        /// </param>
        /// <param name="action">
        ///     The action to execute.
        /// </param>
        /// <exception cref="Exception">
        ///     A delegate callback throws an exception.
        /// </exception>
        /// <returns>
        ///     An awaitable <see cref="Task" /> that finishes when the operation finished.
        /// </returns>
        public static Task RunOnMainThreadAsync(CancellationToken ct, Action action)
        {
            if (ct.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            if (!Application.isPlaying || Thread.CurrentThread == s_mainThread)
            {
                action();
                return Task.CompletedTask;
            }

            Instance.Enqueue(action);
            return Instance.DispatchAsync(ct);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (s_instance is null)
            {
                s_instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log($"[{nameof(UxrMonoDispatcher)}] singleton successfully initialized on Awake", this);
            }
            else if (!ReferenceEquals(s_instance, this))
            {
                Debug.LogWarning($"[{nameof(UxrMonoDispatcher)}] singleton already initialized. Destroying secondary instance on Awake", this);
                Destroy(this);
            }
        }

        /// <summary>
        ///     Flushes the queue, executing all remaining actions, and disables the component.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _workQueue.Flush();
            enabled = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to find the singleton.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            s_mainThread = Thread.CurrentThread;
            s_instance   = FindObjectOfType<UxrMonoDispatcher>();
            if (!(s_instance is null))
            {
                Debug.Log($"[{nameof(UxrMonoDispatcher)} singleton successfully found in scene.");
            }
        }

        /// <summary>
        ///     Dispatches the current actions.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable <see cref="Task" /> that finishes when the actions were dispatched</returns>
        private Task DispatchAsync(CancellationToken ct)
        {
            StartDispatching();
            return TaskExt.WaitWhile(() => IsDispatching, ct);
        }

        /// <summary>
        ///     Enqueues a new action.
        /// </summary>
        /// <param name="workItem">Action to enqueue</param>
        private void Enqueue(Action workItem)
        {
            _workQueue.Enqueue(workItem);
        }

        /// <summary>
        ///     Enables the component so that it starts dispatching the enqueued actions.
        /// </summary>
        private void StartDispatching()
        {
            enabled = true;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the singleton.
        /// </summary>
        private static UxrMonoDispatcher Instance => s_instance ? s_instance : s_instance = new GameObject(nameof(UxrMonoDispatcher)).AddComponent<UxrMonoDispatcher>();

        /// <summary>
        ///     Gets whether it is currently dispatching.
        /// </summary>
        private bool IsDispatching => enabled;

        private static   UxrMonoDispatcher s_instance;
        private static   Thread            s_mainThread;
        private readonly WorkDoubleBuffer  _workQueue = new WorkDoubleBuffer(8);

        #endregion
    }
}