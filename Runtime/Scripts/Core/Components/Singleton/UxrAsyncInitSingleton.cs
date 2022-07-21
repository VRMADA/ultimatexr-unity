// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAsyncInitSingleton.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Core.Threading.TaskControllers;

namespace UltimateXR.Core.Components.Singleton
{
    /// <summary>
    ///     Same as <see cref="UxrSingleton{T}" /> but allows asynchronous initialization.
    ///     This can be useful where singletons require initialization through config files that are loaded asynchronously from
    ///     disk or through network.
    /// </summary>
    /// <typeparam name="T">Type the singleton is for</typeparam>
    public abstract class UxrAsyncInitSingleton<T> : UxrSingleton<T> where T : UxrAsyncInitSingleton<T>
    {
        #region Protected Overrides UxrAbstractSingleton<T>

        /// <summary>
        ///     Initializes the singleton asynchronously. Calls <see cref="InitAsync" /> which is required to be implemented in
        ///     child classes.
        /// </summary>
        /// <param name="initializedCallback">Callback required to run when the initialization finished.</param>
        protected override void InitInternal(Action initializedCallback)
        {
            _initController           =  (UxrTaskController)InitAsync;
            _initController.Completed += (o, e) => initializedCallback?.Invoke();
            _initController.Start();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Initializes the singleton asynchronously.
        /// </summary>
        /// <param name="ct">Allows to cancel the asynchronous process if necessary</param>
        /// <returns>Task representing the initialization</returns>
        protected abstract Task InitAsync(CancellationToken ct = default);

        #endregion

        #region Private Types & Data

        private UxrTaskController _initController;

        #endregion
    }
}