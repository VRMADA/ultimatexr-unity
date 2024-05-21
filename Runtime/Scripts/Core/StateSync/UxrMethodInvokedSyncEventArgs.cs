// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMethodInvokedSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Settings;
using UnityEngine;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Event args for the state sync of a method that was called. It supports sending the same parameters.
    /// </summary>
    public class UxrMethodInvokedSyncEventArgs : UxrSyncEventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the method name.
        /// </summary>
        public string MethodName
        {
            get => _methodName;
            private set => _methodName = value;
        }

        /// <summary>
        ///     Gets the call parameters.
        /// </summary>
        public object[] Parameters
        {
            get => _parameters;
            private set => _parameters = value;
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="methodName">The name of the method that was called</param>
        /// <param name="parameters">The parameters used in the call</param>
        public UxrMethodInvokedSyncEventArgs(string methodName, params object[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters ?? new object[] { };
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            if (MethodName == null && Parameters == null)
            {
                return "Unknown method call";
            }
            
            if (Parameters == null || Parameters.Length == 0)
            {
                return $"Method call {MethodName}()";
            }

            return $"Method call {MethodName ?? "unknown"}({string.Join(", ", Parameters.Select(p => p == null ? "null" : p.ToString()))})";
        }

        #endregion

        #region Protected Overrides UxrSyncEventArgs

        /// <inheritdoc />
        protected override void SerializeEventInternal(IUxrSerializer serializer)
        {
            bool isMethodKnown = false;
            
            try
            {
                serializer.Serialize(ref _methodName);
                isMethodKnown = true;
                serializer.Serialize(ref _parameters);
            }
            catch (Exception)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors && serializer.IsReading && isMethodKnown)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} Error deserializing invoked method {_methodName}(). Exception below.");
                }
                
                throw;
            }
        }

        #endregion

        #region Private Types & Data

        private string   _methodName;
        private object[] _parameters;

        #endregion
    }
}