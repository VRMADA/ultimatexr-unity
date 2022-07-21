// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLoopController.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Threading;

namespace UltimateXR.Core.Threading.TaskControllers
{
    /// <summary>
    ///     A wrapper class to turn a cancelable action into a controllable <see cref="UxrCancellableController.Start()" />/
    ///     <see cref="UxrCancellableController.Stop" /> pattern and run it uninterruptedly in a loop.
    /// </summary>
    public sealed class UxrLoopController : UxrCancellableController
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="loopAction">
        ///     A cancelable and loopable action that will be executing repeatedly until
        ///     <see cref="UxrCancellableController.Stop" /> is called.
        /// </param>
        /// <param name="autoStartDelay">
        ///     <list type="bullet">
        ///         <item>
        ///             If set, <paramref name="loopAction" /> starts looping after <paramref name="autoStartDelay" />
        ///             milliseconds.
        ///         </item>
        ///         <item>If not set, <paramref name="loopAction" /> starts looping immediately.</item>
        ///     </list>
        /// </param>
        public UxrLoopController(Action<CancellationToken> loopAction, int autoStartDelay = -1)
        {
            _loopAction = loopAction;

            if (autoStartDelay == 0)
            {
                Start();
            }
            else if (autoStartDelay > 0)
            {
                StartAfter(autoStartDelay);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Explicit conversion operator from <see cref="Action{CancellationToken}" /> to <see cref="UxrLoopController" />.
        /// </summary>
        /// <param name="loopAction">
        ///     A cancelable and loopable action that will be executing repeatedly until
        ///     <see cref="UxrCancellableController.Stop" /> is called.
        /// </param>
        /// <returns>
        ///     A new instance of <see cref="UxrLoopController" /> wrapping <paramref name="loopAction" />.
        /// </returns>
        public static explicit operator UxrLoopController(Action<CancellationToken> loopAction)
        {
            return new UxrLoopController(loopAction);
        }

        #endregion

        #region Protected Overrides UxrCancellableController

        /// <inheritdoc />
        protected override void StartInternal(CancellationToken ct, Action onCompleted)
        {
            _loopAction(ct); // Executes _loopAction until cancellation 
        }

        #endregion

        #region Private Types & Data

        private readonly Action<CancellationToken> _loopAction;

        #endregion
    }
}