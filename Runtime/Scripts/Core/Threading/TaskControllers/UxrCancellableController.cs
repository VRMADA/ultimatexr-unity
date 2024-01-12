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
    ///     Parent abstract class that simplifies running a job which can be canceled through a
    ///     <see cref="CancellationToken" />.<br />
    ///     It wraps a <see cref="CancellationTokenSource" /> into a <see cref="Start()" /> and <see cref="Stop" /> pattern,
    ///     ensuring that the
    ///     <see cref="Stop" /> is called if the application quits abruptly or the Unity editor exits playmode.
    /// </summary>
    /// <seealso cref="UxrTaskController" />
    /// <seealso cref="UxrLoopController" />
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
        ///     Starts running the inner job until completion or <see cref="Stop" /> is called.
        /// </summary>
        public void Start()
        {
            Start(0, 0);
        }

        /// <summary>
        ///     Starts using an initial <paramref name="delayMilliseconds" />.
        /// </summary>
        /// <param name="delayMilliseconds">
        ///     Delay in milliseconds before <see cref="Start()" /> is automatically called.
        /// </param>
        public void StartAfterMilliseconds(int delayMilliseconds)
        {
            Start(delayMilliseconds, 0);
        }


        /// <summary>
        ///     Calls <see cref="Start()" /> and will automatically call <see cref="Stop" /> after
        ///     <paramref name="durationMilliseconds" /> milliseconds.
        /// </summary>
        /// <param name="durationMilliseconds">
        ///     Allowed running time until <see cref="Stop" /> is automatically called, in milliseconds
        /// </param>
        public void StartAndRunForMilliseconds(int durationMilliseconds)
        {
            Start(0, durationMilliseconds);
        }

        /// <summary>
        ///     Combines functionality of <see cref="StartAfterMilliseconds" /> and <see cref="StartAndRunForMilliseconds" />,
        ///     allowing to <see cref="Start()" /> after a delay and run for a certain amount of time.
        /// </summary>
        /// <param name="delayMilliseconds">
        ///     Delay in milliseconds before <see cref="Start()" /> is automatically called.
        /// </param>
        /// <param name="durationMilliseconds">
        ///     Allowed running time starting after the initial delay until <see cref="Stop" /> is automatically called, in
        ///     milliseconds.
        /// </param>
        public async void Start(int delayMilliseconds, int durationMilliseconds)
        {
            if (delayMilliseconds > 0)
            {
                Stop();
                _cts = new CancellationTokenSource(delayMilliseconds);
                await TaskExt.Delay(delayMilliseconds, _cts.Token);

                if (_cts.IsCancellationRequested)
                {
                    return;
                }
            }

            Stop();
            _cts = durationMilliseconds > 0 ? new CancellationTokenSource(durationMilliseconds) : new CancellationTokenSource();

            StartInternal(_cts.Token, OnCompleted);
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

        /// <summary>
        ///     Creates a linked <see cref="CancellationTokenSource" /> using the internal controller token source.
        ///     This allows to create token sources that will be cancelled if the controller gets cancelled.
        /// </summary>
        /// <returns>Linked cancellation token source</returns>
        /// <exception cref="InvalidOperationException">
        ///     The Task was not started, which means that there is no internal cancellation token source available yet. The Task
        ///     must have been started to create a linked cancellation token source.
        /// </exception>
        public CancellationTokenSource CreateLinkedTokenSource()
        {
            if (_cts == null)
            {
                throw new InvalidOperationException($"{nameof(GetType)}: Cannot create linked token source when Task is not running.");
            }

            return CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
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
        ///     Optional callback when the logic has completed, so that the base class can free resources. The callback is only
        ///     invoked if the logic fully completed. If the logic was stopped manually, the callback is not invoked.
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