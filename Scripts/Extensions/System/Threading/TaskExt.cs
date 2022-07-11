// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Animation.Interpolation;
using UnityEngine;

namespace UltimateXR.Extensions.System.Threading
{
    /// <summary>
    ///     <see cref="Task" /> extensions.
    /// </summary>
    public static class TaskExt
    {
        #region Public Methods

        /// <summary>
        ///     Allows to run a task in "fire and forget" mode, when it is not required to await nor is it relevant whether it
        ///     succeeds or not. There still needs to be a way to handle exceptions to avoid unhandled exceptions and process
        ///     termination.
        /// </summary>
        /// <param name="self">Task to run in fire and forget mode</param>
        /// <exception cref="ArgumentNullException">The task is null</exception>
        public static async void FireAndForget(this Task self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            try
            {
                // Simply awaiting the task in an async void method already guarantees exception propagation
                await self;
            }
            catch (Exception e) // ...but default LogException behaviour only shows innerException.
            {
                Debug.LogError($"{nameof(TaskExt)}::{nameof(FireAndForget)}>> Exception missed (stack trace below):{e.Message}\n\n{e}");
                Debug.LogException(e); // Log and ignore exceptions, until playlists are empty.
            }
        }

        /// <summary>
        ///     Creates an awaitable task that finishes the next frame.
        /// </summary>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static Task WaitForNextFrame(CancellationToken ct = default)
        {
            return SkipFrames(1, ct);
        }

        /// <summary>
        ///     Creates an awaitable task that finishes after a given amount of frames.
        /// </summary>
        /// <param name="frameCount">Number of frames to wait</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static async Task SkipFrames(int frameCount, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested || frameCount <= 0)
            {
                return;
            }

            for (uint i = 0; i < frameCount && !ct.IsCancellationRequested; ++i)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        ///     Creates an awaitable task that finishes after a given amount of seconds.
        /// </summary>
        /// <param name="seconds">Number of seconds to wait</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static Task Delay(float seconds, CancellationToken ct = default)
        {
            return Delay(Mathf.RoundToInt(1000f * seconds), ct);
        }

        /// <summary>
        ///     Creates an awaitable task that finishes after a given amount of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to wait</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable task</returns>
        public static async Task Delay(int milliseconds, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested || milliseconds <= 0)
            {
                return;
            }

            try
            {
                await Task.Delay(milliseconds, ct);
            }
            catch (OperationCanceledException)
            {
                // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
                // In this case, we only want to stop polling and finish this async Task.
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks while a condition is true or the task is canceled.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        public static async Task WaitWhile(Func<bool> condition, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested && condition())
            {
                await Task.Yield();
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks until a condition is true or the task is canceled.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        public static async Task WaitUntil(Func<bool> condition, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested && !condition())
            {
                await Task.Yield();
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks while a condition is true, a timeout occurs or the task is canceled.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="timeout">Timeout, in milliseconds</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        /// <exception cref="TimeoutException">Thrown after <see cref="timeout" /> milliseconds</exception>
        public static async Task WaitWhile(Func<bool> condition, int timeout, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            using CancellationTokenSource cts          = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Task                          waitTask     = WaitWhile(condition, cts.Token);
            Task                          timeoutTask  = Delay(timeout, cts.Token);
            Task                          finishedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (!finishedTask.IsCanceled)
            {
                cts.Cancel();       // Cancel unfinished task
                await finishedTask; // Propagate exceptions
                if (finishedTask == timeoutTask)
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks until a condition is true, a timeout occurs or the task is canceled.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="timeout">Timeout, in milliseconds</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        /// <exception cref="TimeoutException">Thrown after <see cref="timeout" /> milliseconds</exception>
        public static async Task WaitUntil(Func<bool> condition, int timeout, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            using CancellationTokenSource cts          = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Task                          waitTask     = WaitUntil(condition, cts.Token);
            Task                          timeoutTask  = Delay(timeout, cts.Token);
            Task                          finishedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (!finishedTask.IsCanceled)
            {
                cts.Cancel();       // Cancel unfinished task
                await finishedTask; // Propagate exceptions
                if (finishedTask == timeoutTask)
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks while a condition is true, waiting a certain amount of seconds at maximum. An
        ///     optional action can be called if the task was cancelled or it timed out.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="duration">The maximum amount of seconds to wait while the condition is true</param>
        /// <param name="cancelCallback">Optional action to execute if the task was canceled or it timed out</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        public static async Task WaitWhile(Func<bool> condition, float duration, Action cancelCallback = null, CancellationToken ct = default)
        {
            int  timeout = Mathf.RoundToInt(duration * 1200f);
            bool mustCancel;
            try
            {
                await WaitWhile(condition, timeout, ct);
                mustCancel = ct.IsCancellationRequested;
            }
            catch (TimeoutException)
            {
                mustCancel = true;
            }

            if (mustCancel)
            {
                cancelCallback?.Invoke();
            }
        }

        /// <summary>
        ///     Creates an awaitable task that blocks until a condition is true, waiting a certain amount of seconds at maximum. An
        ///     optional action can be called if the task was cancelled or it timed out.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block</param>
        /// <param name="duration">The maximum amount of seconds to wait while the condition is true</param>
        /// <param name="cancelCallback">Optional action to execute if the task was canceled or it timed out</param>
        /// <param name="ct">Optional cancellation token, to cancel the task</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        public static async Task WaitUntil(Func<bool> condition, float duration, Action cancelCallback = null, CancellationToken ct = default)
        {
            int  timeout = Mathf.RoundToInt(duration * 1200f);
            bool mustCancel;
            try
            {
                await WaitUntil(condition, timeout, ct);
                mustCancel = ct.IsCancellationRequested;
            }
            catch (TimeoutException)
            {
                mustCancel = true;
            }

            if (mustCancel)
            {
                cancelCallback?.Invoke();
            }
        }

        /// <summary>
        ///     Provides a one-liner method to await until a task is cancelled.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        public static async Task WaitUntilCancelled(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        ///     Loops iterating once per frame during a specified amount of time, executing a user-defined action.
        /// </summary>
        /// <param name="ct">The cancellation token</param>
        /// <param name="durationSeconds">Loop duration in seconds</param>
        /// <param name="loopAction">
        ///     The action performed each frame, which will receive the interpolation [0.0, 1.0] parameter as
        ///     argument.
        /// </param>
        /// <param name="easing">The easing used to compute the interpolation parameter over time</param>
        /// <param name="forceLastT1">
        ///     Will enforce a last iteration with 1.0 interpolation parameter. This will avoid
        ///     having a last step with close than, but not 1.0, interpolation.
        /// </param>
        public static async Task Loop(CancellationToken ct,
                                      float             durationSeconds,
                                      Action<float>     loopAction,
                                      UxrEasing         easing      = UxrEasing.Linear,
                                      bool              forceLastT1 = false)
        {
            float startTime = Time.time;

            while (Time.time - startTime < durationSeconds)
            {
                float t = UxrInterpolator.Interpolate(0.0f, 1.0f, Time.time - startTime, new UxrInterpolationSettings(durationSeconds, 0.0f, easing));
                loopAction(t);
                await Task.Yield();
            }

            if (forceLastT1)
            {
                loopAction(1.0f);
            }
        }

        #endregion
    }
}