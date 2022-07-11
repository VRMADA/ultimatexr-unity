// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCancellableController.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using UltimateXR.Extensions.System.Threading;
using UnityEngine;

namespace UltimateXR.Core.Threading.TaskControllers
{
    /// <summary>
    ///     Parent abstract class for <see cref="UxrLoopController" /> and <see cref="UxrTaskController" />.
    /// </summary>
    /// <remarks>
    ///     Wraps a <see cref="CancellationTokenSource" /> into a <see cref="Start()" /> and <see cref="Stop" /> pattern,
    ///     ensuring that <see cref="Stop" /> is called when the application quits.
    /// </remarks>
    public abstract class UxrCancellableController
    {
        #region Public Types & Data

        /// <summary>
        ///     Triggered when the inner job is completed, without having been canceled.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        ///     Gets whether the inner job is currently running.
        /// </summary>
        public bool IsRunning => _cts != null;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        protected UxrCancellableController()
        {
            Application.quitting += Application_quitting;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts running inner job until completion or <see cref="Stop" /> is called.
        /// </summary>
        public void Start()
        {
            Stop();
            _cts = new CancellationTokenSource();
            StartInternal(_cts.Token, OnCompleted);
        }

        /// <summary>
        ///     Similar to <see cref="Start()" />, but adding an initial <paramref name="delay" />.
        /// </summary>
        /// <param name="delay">
        ///     Delay in milliseconds before <see cref="Start()" /> is automatically called.
        /// </param>
        public async void StartAfter(int delay)
        {
            if (delay <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), delay, "Delay must be a value in milliseconds greater than zero.");
            }

            Stop();

            _cts = new CancellationTokenSource(delay);
            await TaskExt.Delay(delay, _cts.Token);
            StartInternal(_cts.Token, OnCompleted);
        }


        /// <summary>
        ///     Similar to <see cref="Start()" />, but it automatically calls <see cref="Stop" /> after
        ///     <paramref name="duration" /> milliseconds.
        /// </summary>
        /// <param name="duration">
        ///     Allowed running time until <see cref="Stop" /> is automatically called, in milliseconds
        /// </param>
        public void Start(int duration)
        {
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Time before cancelling must be a value in milliseconds greater than zero.");
            }

            Stop();
            _cts = new CancellationTokenSource(duration);
            StartInternal(_cts.Token, OnCompleted);
        }

        /// <summary>
        ///     Similar to <see cref="Start()" />, but it automatically calls <see cref="Stop" /> after
        ///     <paramref name="duration" /> seconds.
        /// </summary>
        /// <param name="duration">
        ///     Allowed running time until <see cref="Stop" /> is automatically called, in seconds
        /// </param>
        public void Start(float duration)
        {
            Start(Mathf.RoundToInt(duration * 1000f));
        }

        /// <summary>
        ///     Cancels the inner job.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the application is about to quit. Ensures that the task is stopped.
        /// </summary>
        private void Application_quitting()
        {
            Stop();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="Completed" />.
        /// </summary>
        private void OnCompleted()
        {
            if (_cts is { IsCancellationRequested: false })
            {
                Completed?.Invoke(this, EventArgs.Empty);
                Stop();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Implements the internal logic between with <see cref="Start()" /> and <see cref="Stop" />.
        /// </summary>
        /// <param name="ct">
        ///     Flags, with
        ///     <see cref="CancellationToken.IsCancellationRequested" />, when <see cref="Stop" /> has been requested.
        /// </param>
        /// <param name="onCompleted">
        ///     Optional callback when the logic has completed, so that the base class can free resources.
        /// </param>
        /// <remarks>
        ///     In case the implementation can finish on its own, please invoke <paramref name="onCompleted" /> instead of
        ///     <see cref="Stop" />.
        /// </remarks>
        /// <seealso cref="UxrCancellableController" />
        protected abstract void StartInternal(CancellationToken ct, Action onCompleted);

        #endregion

        #region Private Types & Data

        private CancellationTokenSource _cts;

        #endregion
    }
}