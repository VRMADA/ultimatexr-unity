// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveMonitor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Monitor that helps intercepting relevant state serialization events.
    /// </summary>
    public class UxrStateSaveMonitor
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called right before the state of an object with the <see cref="IUxrStateSave" /> interface is about to be
        ///     serialized using <see cref="IUxrStateSave.SerializeState" />.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> StateSerializing;

        /// <summary>
        ///     Event called right after the state of an object with the <see cref="IUxrStateSave" /> interface was serialized
        ///     using <see cref="IUxrStateSave.SerializeState" />.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> StateSerialized;

        /// <summary>
        ///     Event called right before a state variable is about to be serialized in a
        ///     <see cref="IUxrStateSave.SerializeState" /> call.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> VarSerializing;

        /// <summary>
        ///     Event called right after a state variable was serialized in a <see cref="IUxrStateSave.SerializeState" /> call.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> VarSerialized;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="target">The <see cref="IUxrStateSave" /> that is being monitored</param>
        public UxrStateSaveMonitor(IUxrStateSave target)
        {
            _target = target;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Raises the <see cref="StateSerializing" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseStateSerializing(UxrStateSaveEventArgs e)
        {
            StateSerializing?.Invoke(_target, e);
        }

        /// <summary>
        ///     Raises the <see cref="StateSerialized" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseStateSerialized(UxrStateSaveEventArgs e)
        {
            StateSerialized?.Invoke(_target, e);
        }

        /// <summary>
        ///     Raises the <see cref="VarSerializing" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseVarSerializing(UxrStateSaveEventArgs e)
        {
            VarSerializing?.Invoke(_target, e);
        }

        /// <summary>
        ///     Raises the <see cref="VarSerialized" /> event.
        /// </summary>
        /// <param name="e">Event parameters</param>
        internal void RaiseVarSerialized(UxrStateSaveEventArgs e)
        {
            VarSerialized?.Invoke(_target, e);
        }

        #endregion

        #region Private Types & Data

        private readonly IUxrStateSave _target;

        #endregion
    }
}