// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncImplementer_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Helper class simplifying the implementation of the <see cref="IUxrStateSync" /> interface.
    ///     This class includes functionality for automatic synchronization of property changes and method calls
    ///     through reflection, using a convenient BeginSync/EndSync pattern.
    ///     It is utilized by <see cref="UxrComponent" /> to implement <see cref="IUxrStateSync" />.
    ///     In scenarios where custom classes cannot inherit from <see cref="UxrComponent" /> for automatic sync capabilities,
    ///     this class is designed to implement the interface.
    /// </summary>
    public class UxrStateSyncImplementer<T> : UxrStateSyncImplementer where T : Component, IUxrStateSync
    {
        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="targetComponent">Target component for all the methods called on this object</param>
        public UxrStateSyncImplementer(T targetComponent)
        {
            _targetComponent = targetComponent;
        }

        /// <summary>
        ///     Default constructor is private to use public constructor with target component.
        /// </summary>
        private UxrStateSyncImplementer()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Executes the state change described by <see cref="e" /> on the component.<br />
        ///     First it will check if they are built-in state sync events such as property change (
        ///     <see cref="UxrPropertyChangedSyncEventArgs" />, using <see cref="BeginSync" />/<see cref="EndSyncProperty" />) and
        ///     method call (<see cref="UxrMethodInvokedSyncEventArgs" />, using <see cref="BeginSync" />/
        ///     <see cref="EndSyncMethod" />).
        ///     If it's a different, custom event, it will be handled using the provided
        ///     <paramref name="fallbackSyncStateHandler" />.
        /// </summary>
        /// <param name="e">State change</param>
        /// <param name="fallbackSyncStateHandler">Fallback event handler</param>
        public void SyncState(UxrSyncEventArgs e, Action<UxrSyncEventArgs> fallbackSyncStateHandler)
        {
            // First check if it's a synchronization that can be solved at the base level

            if (e is UxrPropertyChangedSyncEventArgs propertyChangedEventArgs)
            {
                try
                {
                    // Set new property value using reflection
                    _targetComponent.GetType().GetProperty(propertyChangedEventArgs.PropertyName, PropertyFlags).SetValue(_targetComponent, propertyChangedEventArgs.Value);
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync property {propertyChangedEventArgs.PropertyName} to value {propertyChangedEventArgs.Value} . Component: {_targetComponent.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
            else if (e is UxrMethodInvokedSyncEventArgs methodInvokedEventArgs)
            {
                try
                {
                    if (methodInvokedEventArgs.Parameters == null || !methodInvokedEventArgs.Parameters.Any())
                    {
                        // Invoke without arguments
                        _targetComponent.GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags).Invoke(_targetComponent, null);
                    }
                    else
                    {
                        // Invoke method using same parameters using reflection. Make sure we select the correct overload.

                        bool anyIsNull = methodInvokedEventArgs.Parameters.Any(p => p == null);

                        if (_targetComponent.GetType().GetMethods(MethodFlags).Count(m => m.Name.Equals(methodInvokedEventArgs.MethodName)) == 1)
                        {
                            // There are no overloads
                            _targetComponent.GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags).Invoke(_targetComponent, methodInvokedEventArgs.Parameters);
                        }
                        else if (!anyIsNull)
                        {
                            // We can look for a method specifying the parameter types.
                            _targetComponent.GetType().GetMethod(methodInvokedEventArgs.MethodName, MethodFlags, null, methodInvokedEventArgs.Parameters.Select(p => p.GetType()).ToArray(), null).Invoke(_targetComponent, methodInvokedEventArgs.Parameters);
                        }
                        else
                        {
                            // We have a call where a parameter is null, so we can't infer the parameter type. Try to find a method with the same parameter count.

                            MethodInfo method = _targetComponent.GetType().GetMethods(MethodFlags).FirstOrDefault(x => x.Name.Equals(methodInvokedEventArgs.MethodName) && x.GetParameters().Length == methodInvokedEventArgs.Parameters.Length);

                            if (method != null)
                            {
                                method.Invoke(_targetComponent, methodInvokedEventArgs.Parameters);
                            }
                            else
                            {
                                throw new Exception("Could not find a method with the given name and parameter count");
                            }
                        }
                    }
                }
                catch (AmbiguousMatchException ambiguousMatchException)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Trying to sync a method that has ambiguous call. {e}. Component: {_targetComponent.GetPathUnderScene()}. Exception: {ambiguousMatchException}");
                    }
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync method. It could be that an exception inside the method was thrown, that {nameof(EndSyncMethod)} was used with the wrong parameters or it has an overload that could not be resolved. {e}. Component: {_targetComponent.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
            else
            {
                // It's a synchronization using a custom UxrSyncEventArgs object. Pass it to the fallback handler.

                try
                {
                    fallbackSyncStateHandler?.Invoke(e);
                }
                catch (Exception exception)
                {
                    if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                    {
                        Debug.LogError($"{UxrConstants.CoreModule} Error trying to sync state. {e}. Component: {_targetComponent.GetPathUnderScene()}. Exception: {exception}");
                    }
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Starts a synchronization block that will end with an EndSync method like <see cref="EndSyncProperty" />,
        ///         <see cref="EndSyncMethod" /> or <see cref="EndSyncState" />, which causes the
        ///         <see cref="IUxrStateSync.StateChanged" /> event to be triggered.
        ///     </para>
        ///     <para>
        ///         See <see cref="UxrComponent.BeginSync" />.
        ///     </para>
        /// </summary>
        /// <param name="options">Options. It's saved/used in all environments by default.</param>
        public void BeginSync(UxrStateSyncOptions options = UxrStateSyncOptions.Default)
        {
            SyncCallDepth++;
            _optionStack.Push(options);

            if (SyncCallDepth > StateSyncCallDepthErrorThreshold)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/EndSync mismatch when calling BeginSync. Did you forget an EndSync call? Component: {_targetComponent.GetPathUnderScene()}");
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Cancels a <see cref="BeginSync" /> to escape when a condition is found that makes it not require to sync.
        ///     </para>
        ///     <para>
        ///         See <see cref="UxrComponent.CancelSync" />.
        ///     </para>
        /// </summary>
        public void CancelSync()
        {
            if (SyncCallDepth > 0)
            {
                SyncCallDepth--;
                _optionStack.Pop();
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/CancelSync mismatch when calling CancelSync. Did you forget a BeginSync call? State call depth is < 1. Component: {_targetComponent.GetPathUnderScene()}");
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Ends synchronization for a property change. It notifies that a property was changed in a component that
        ///         requires network/state synchronization, ensuring that the change is performed in all other clients too.
        ///         The synchronization should begin using <see cref="BeginSync" />.
        ///     </para>
        ///     <para>
        ///         See <see cref="UxrComponent.EndSyncProperty" />.
        ///     </para>
        /// </summary>
        /// <param name="raiseChangedEvent">Delegate that will call the <see cref="IUxrStateSync.StateChanged" /> event</param>
        /// <param name="value">New property value</param>
        /// <param name="propertyName">Property name</param>
        public void EndSyncProperty(Action<UxrSyncEventArgs> raiseChangedEvent, in object value, [CallerMemberName] string propertyName = null)
        {
            EndSyncState(raiseChangedEvent, new UxrPropertyChangedSyncEventArgs(propertyName, value));
        }

        /// <summary>
        ///     <para>
        ///         Ends synchronization for a method call. It notifies that a method was invoked in a component that requires
        ///         network/state synchronization, ensuring that the call is performed in all other clients too.
        ///         The synchronization should begin using <see cref="BeginSync" />.
        ///     </para>
        ///     <para>
        ///         See <see cref="UxrComponent.EndSyncMethod" />.
        ///     </para>
        /// </summary>
        public void EndSyncMethod(Action<UxrSyncEventArgs> raiseChangedEvent, object[] parameters = null, [CallerMemberName] string methodName = null)
        {
            EndSyncState(raiseChangedEvent, new UxrMethodInvokedSyncEventArgs(methodName, parameters));
        }

        /// <summary>
        ///     <para>
        ///         Ends a synchronization block for a custom event. The synchronization block should begin using
        ///         <see cref="BeginSync" />. The event ensures that the code is executed in all other receivers too.
        ///     </para>
        ///     <para>
        ///         See <see cref="UxrComponent.EndSyncState" />.
        ///     </para>
        /// </summary>
        /// <param name="raiseChangedEvent">
        ///     The delegate to call to raise the <see cref="IUxrStateSync.StateChanged" /> event. The
        ///     delegate will receive a <see cref="UxrSyncEventArgs" /> as parameter describing the state change.
        /// </param>
        /// <param name="e">The state change</param>
        public void EndSyncState(Action<UxrSyncEventArgs> raiseChangedEvent, UxrSyncEventArgs e)
        {
            if (SyncCallDepth > 0)
            {
                e.Options = _optionStack.Pop();
                raiseChangedEvent?.Invoke(e);
                SyncCallDepth--;
            }
            else
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} BeginSync/EndSync mismatch when calling EndSync. Did you forget a BeginSync call? State call depth is < 1. Component: {_targetComponent.GetPathUnderScene()}");
                }
            }
        }

        /// <summary>
        ///     Registers the component if it hasn't been registered already.
        /// </summary>
        public void RegisterIfNecessary()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (!_registered)
            {
                UxrManager.Instance.RegisterStateSyncComponent<T>(_targetComponent);
                _registered = true;
            }
        }

        /// <summary>
        ///     Unregisters the component.
        /// </summary>
        public void Unregister()
        {
            UxrManager.Instance.UnregisterStateSyncComponent<T>(_targetComponent);
        }

        #endregion

        #region Private Types & Data

        private const int          StateSyncCallDepthErrorThreshold = 100;
        private const BindingFlags EventFlags                       = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags MethodFlags                      = EventFlags | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;
        private const BindingFlags PropertyFlags                    = EventFlags | BindingFlags.SetProperty;

        private readonly T                          _targetComponent;
        private readonly Stack<UxrStateSyncOptions> _optionStack = new Stack<UxrStateSyncOptions>();
        private          bool                       _registered;

        #endregion
    }
}