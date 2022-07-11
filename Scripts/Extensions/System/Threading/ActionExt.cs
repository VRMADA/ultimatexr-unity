// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Core.Threading.TaskControllers;
using UnityEngine;

namespace UltimateXR.Extensions.System.Threading
{
    /// <summary>
    ///     <see cref="Action" /> extensions.
    /// </summary>
    public static class ActionExt
    {
        #region Public Methods

        /// <summary>
        ///     Executes repeatedly this <see cref="Action" />, in the main thread, at <paramref name="rate" /> until cancellation
        ///     is requested with <paramref name="ct" />.
        /// </summary>
        /// <param name="self"><see cref="Action" /> to loop at <paramref name="rate" /> Hz</param>
        /// <param name="rate">Loop frequency in Hz</param>
        /// <param name="ct">Cancellation token</param>
        /// <seealso cref="LoopThreaded" />
        /// <seealso cref="ToLoop" />
        public static async void Loop(this Action self, float rate = 10f, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            int deltaTimeMs = Mathf.RoundToInt(1000f / rate);

            while (!ct.IsCancellationRequested)
            {
                // Start delay timer parallel to action execution
                Task delayTask = TaskExt.Delay(deltaTimeMs, ct);
                self();
                await delayTask;
            }
        }

        /// <summary>
        ///     Executes repeatedly this <see cref="Action" />, in a separated thread, at <paramref name="rate" /> Hz until
        ///     cancellation is requested using <paramref name="ct" />.
        /// </summary>
        /// <param name="self"><see cref="Action" /> to loop at <paramref name="rate" /> Hz</param>
        /// <param name="rate">Loop frequency in Hz</param>
        /// <param name="ct">Cancellation token</param>
        public static async void LoopThreaded(this Action self, float rate = 10f, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            int deltaTimeMs = Mathf.RoundToInt(1000f / rate);

            while (!ct.IsCancellationRequested)
            {
                // We don't want to abort current thread (Task.Run) with ct
                // Instead, we wait for action to end, breaking the loop after that.
                Task delayTask = TaskExt.Delay(deltaTimeMs, ct);
                Task runTask   = Task.Run(self, CancellationToken.None);
                await Task.WhenAll(delayTask, runTask);
            }
        }

        /// <summary>
        ///     Creates a <see cref="UxrLoopController" /> which wraps a cancellable loop executing this <see cref="Action" /> in
        ///     the main thread.
        /// </summary>
        /// <param name="self"><see cref="Action" /> to loop at <paramref name="rate" /> Hz</param>
        /// <param name="rate">Loop frequency in Hz</param>
        /// <param name="autoStartDelay">
        ///     Delay in milliseconds before loop executes its first iteration.
        ///     <list type="bullet">
        ///         <item>
        ///             Equal or greater than zero: tells <see cref="UxrLoopController" /> to automatically start looping
        ///             <paramref name="autoStartDelay" /> milliseconds after creation.
        ///         </item>
        ///         <item>
        ///             Negative (default) <see cref="UxrLoopController.Start()" /> needs to be called on returned
        ///             <see cref="UxrLoopController" /> to start looping.
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>
        ///     A <see cref="UxrLoopController" /> to handle (<see cref="UxrLoopController.Start()" />,
        ///     <see cref="UxrLoopController.Stop" />) the loop execution.
        /// </returns>
        /// <seealso cref="UxrLoopController" />
        /// <seealso cref="Loop" />
        /// <seealso cref="ToThreadedLoop" />
        public static UxrLoopController ToLoop(this Action self, float rate = 10f, int autoStartDelay = -1)
        {
            return new UxrLoopController(ct => Loop(self, rate, ct), autoStartDelay);
        }

        /// <summary>
        ///     Creates a <see cref="UxrLoopController" /> which wraps a cancellable loop executing this <see cref="Action" /> in a
        ///     separate thread.
        /// </summary>
        /// <param name="self"><see cref="Action" /> to loop, in a separate thread, at <paramref name="rate" /> Hz</param>
        /// <param name="rate">Loop frequency in Hz</param>
        /// <param name="autoStartDelay">
        ///     Delay in milliseconds before loop executes its first iteration.
        ///     <list type="bullet">
        ///         <item>
        ///             Equal or greater than zero: tells <see cref="UxrLoopController" /> to automatically start looping
        ///             <paramref name="autoStartDelay" /> milliseconds after creation.
        ///         </item>
        ///         <item>
        ///             Negative (default) <see cref="UxrLoopController.Start()" /> needs to be called on returned
        ///             <see cref="UxrLoopController" /> to start looping.
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>
        ///     A <see cref="UxrLoopController" /> to handle (<see cref="UxrLoopController.Start()" />,
        ///     <see cref="UxrLoopController.Stop" />) the loop execution.
        /// </returns>
        /// <seealso cref="UxrLoopController" />
        /// <seealso cref="Loop" />
        public static UxrLoopController ToThreadedLoop(this Action self, float rate = 10f, int autoStartDelay = -1)
        {
            return new UxrLoopController(ct => LoopThreaded(self, rate, ct), autoStartDelay);
        }

        #endregion
    }
}