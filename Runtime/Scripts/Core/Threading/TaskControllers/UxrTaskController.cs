// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTaskController.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UltimateXR.Core.Threading.TaskControllers
{
    /// <summary>
    ///     A class that simplifies running tasks in Unity, taking care of stopping them automatically if an application quits
    ///     or Unity exits playmode.
    /// </summary>
    /// <example>
    ///     <para>
    ///         UxrTaskController simplifies running the task and takes care of stopping it automatically if Unity or the
    ///         application stops/quits.
    ///         The constructor lets you start the task automatically, without requiring any further instructions, and also
    ///         start/stop it manually if needed.
    ///     </para>
    ///     <code>
    ///     // An asynchronous task
    ///     public async Task MyTask(int parameterA, CancellationToken ct)
    ///     {
    ///         await SomethingAsync(ct);
    ///     }
    ///     <br />
    ///     // Create the task but don't start it yet (autoStart = false).
    ///     UxrTaskController taskController = new UxrTaskController(ct => MyTask(10, ct), false);<br />
    ///     <br />
    ///     // Start the task manually. There are optional parameters for delayed start or forced duration.
    ///     taskController.Start();<br />
    ///     <br />
    ///     // Stop the task manually at any point.
    ///     taskController.Stop();
    ///     </code>
    /// </example>
    public sealed class UxrTaskController : UxrCancellableController
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="taskFunc">
        ///     A cancelable task which that be executed asynchronously until completion or
        ///     <see cref="UxrCancellableController.Stop" /> is called.
        /// </param>
        /// <param name="autoStart">
        ///     <list type="bullet">
        ///         <item>
        ///             <term><see langword="false" />: </term>
        ///             <description>
        ///                 <see cref="UxrCancellableController.Start()" /> needs to be called in order to start
        ///                 <paramref name="taskFunc" /> execution.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term><see langword="true" />: </term>
        ///             <description><paramref name="taskFunc" /> starts executing immediately.</description>
        ///         </item>
        ///     </list>
        /// </param>
        public UxrTaskController(Func<CancellationToken, Task> taskFunc, bool autoStart = false)
        {
            _taskFunc = taskFunc;
            if (autoStart)
            {
                Start();
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Explicit conversion operator from <see cref="Func{CancellationToken,Task}" /> to <see cref="UxrTaskController" />.
        /// </summary>
        /// <param name="taskFunc">
        ///     A cancelable task that will be executed asynchronously until completion or
        ///     <see cref="UxrCancellableController.Stop" /> is called.
        /// </param>
        /// <returns>
        ///     A new instance of <see cref="UxrTaskController" /> wrapping <paramref name="taskFunc" />.
        /// </returns>
        public static explicit operator UxrTaskController(Func<CancellationToken, Task> taskFunc)
        {
            return new UxrTaskController(taskFunc);
        }

        #endregion

        #region Protected Overrides UxrCancellableController

        /// <inheritdoc />
        protected override async void StartInternal(CancellationToken ct, Action onCompleted)
        {
            await _taskFunc(ct);
            onCompleted();
        }

        #endregion

        #region Private Types & Data

        private readonly Func<CancellationToken, Task> _taskFunc;

        #endregion
    }
}