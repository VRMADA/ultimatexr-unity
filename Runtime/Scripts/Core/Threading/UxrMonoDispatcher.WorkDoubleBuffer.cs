// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMonoDispatcher.WorkDoubleBuffer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace UltimateXR.Core.Threading
{
    public sealed partial class UxrMonoDispatcher
    {
        #region Private Types & Data

        /// <summary>
        ///     A double buffered working queue.
        /// </summary>
        private sealed class WorkDoubleBuffer
        {
            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            public WorkDoubleBuffer()
            {
                _input  = new Queue<Action>();
                _output = new Queue<Action>();
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="capacity">Initial input and output queue capacity</param>
            public WorkDoubleBuffer(int capacity)
            {
                _input  = new Queue<Action>(capacity);
                _output = new Queue<Action>(capacity);
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Executes all enqueued actions.
            /// </summary>
            /// <exception cref="Exception">
            ///     A delegate callback throws an exception.
            /// </exception>
            public void Flush()
            {
                Switch();

                foreach (var action in _output)
                {
                    action?.Invoke();
                }
            }

            /// <summary>
            ///     Enqueues a new action that should be executed.
            /// </summary>
            /// <param name="workItem">Action to execute</param>
            public void Enqueue(Action workItem)
            {
                lock (_lock)
                {
                    _input.Enqueue(workItem);
                }
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///     Switches the buffers.
            /// </summary>
            private void Switch()
            {
                lock (_lock)
                {
                    (_output, _input) = (_input, _output);
                }
            }

            #endregion

            #region Private Types & Data

            private readonly object _lock = new object();

            private Queue<Action> _input;
            private Queue<Action> _output;

            #endregion
        }

        #endregion
    }
}