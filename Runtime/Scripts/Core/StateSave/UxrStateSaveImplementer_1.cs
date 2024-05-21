// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveImplementer_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
using UnityEngine;
using ObjectExt = UltimateXR.Extensions.System.ObjectExt;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Helper class simplifying the implementation of the <see cref="IUxrStateSave" /> interface.
    ///     This class includes functionality to serialize the state of components.
    ///     It is utilized by <see cref="UxrComponent" /> to implement <see cref="IUxrStateSave" />.
    ///     In scenarios where custom classes cannot inherit from <see cref="UxrComponent" /> for state saving capabilities,
    ///     this class is designed to implement the interface.
    /// </summary>
    public class UxrStateSaveImplementer<T> : UxrStateSaveImplementer where T : Component, IUxrStateSave
    {
        #region Public Types & Data

        /// <summary>
        ///     State serialization handler.
        /// </summary>
        public delegate void SerializeStateHandler(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options);

        /// <summary>
        ///     State interpolation handler.
        /// </summary>
        public delegate void InterpolateStateHandler(in UxrStateInterpolationVars vars, float t);

        /// <summary>
        ///     Gets the state save monitor.
        /// </summary>
        public UxrStateSaveMonitor Monitor { get; }

        /// <summary>
        ///     Gets incremented each time a value is serialized or deserialized using <see cref="SerializeStateValue{TV}" />.
        ///     This can be used to check whether a value was actually serialized. When it doesn't get incremented, it means that
        ///     the value didn't change (when writing) or doesn't need to change (when reading).
        /// </summary>
        /// <remarks>
        ///     When a value doesn't need serialization because it didn't change, a boolean is serialized to tell that no change
        ///     was made. If all values in a component didn't change, the component can be ignored to avoid writing many false
        ///     booleans and save space. This still requires a first
        ///     pass using <see cref="UxrStateSaveOptions.DontCacheChanges" /> and <see cref="UxrStateSaveOptions.DontSerialize" />
        ///     .
        /// </remarks>
        public int SerializeCounter { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="targetComponent">Target component for all the methods called on this object</param>
        public UxrStateSaveImplementer(T targetComponent)
        {
            _targetComponent = targetComponent;
            Monitor          = new UxrStateSaveMonitor(targetComponent);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Registers the component if necessary.
        /// </summary>
        public void RegisterIfNecessary()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (!_registered)
            {
                RegisterComponent(_targetComponent);
                _registered = true;
            }
        }

        /// <summary>
        ///     Unregisters the component.
        /// </summary>
        public void Unregister()
        {
            base.UnregisterComponent(_targetComponent);
        }

        /// <summary>
        ///     Notifies the component has been enabled.
        /// </summary>
        public void NotifyOnEnable()
        {
            base.NotifyOnEnable(_targetComponent);
        }

        /// <summary>
        ///     Notifies the component has been disabled.
        /// </summary>
        public void NotifyOnDisable()
        {
            base.NotifyOnDisable(_targetComponent);
        }

        /// <summary>
        ///     Serializes the state of a component, handling serialization of different elements if necessary:
        ///     <list type="bullet">
        ///         <item>
        ///             The enabled state of the component and active state of the GameObject, if required by
        ///             <see cref="IUxrStateSave.SerializeActiveAndEnabledState" />.
        ///         </item>
        ///         <item>
        ///             The transform, using <see cref="IUxrStateSave.RequiresTransformSerialization" /> and
        ///             <see cref="IUxrStateSave.TransformStateSaveSpace" />.
        ///         </item>
        ///     </list>
        ///     After that, it hands over the serialization to <paramref name="customSerializeStateHandler" /> to handle the custom
        ///     component serialization.
        /// </summary>
        /// <param name="serializer">The serializer</param>
        /// <param name="level">The amount of data to serialize</param>
        /// <param name="options">Options</param>
        /// <param name="customSerializeStateHandler">The handler that will serialize the remaining custom data</param>
        public void SerializeState(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, SerializeStateHandler customSerializeStateHandler)
        {
            if (_targetComponent == null)
            {
                return;
            }
            
            OnStateSerializing(_targetComponent, GetStateSaveEventArgs(serializer, level, options));

            // Enabled/Active states

            if (_targetComponent.SerializeActiveAndEnabledState)
            {
                Behaviour behaviour     = _targetComponent.Component;
                bool      enabled       = behaviour.enabled;
                bool      active        = _targetComponent.GameObject.activeSelf;
                bool      enabledBefore = enabled;
                bool      activeBefore  = active;

                SerializeStateValue(serializer, level, options, NameIsEnabled, ref enabled);
                SerializeStateValue(serializer, level, options, NameIsActive,  ref active);

                if (serializer.IsReading)
                {
                    if (enabled != enabledBefore && behaviour != null)
                    {
                        behaviour.enabled = enabled;
                    }

                    if (active != activeBefore)
                    {
                        _targetComponent.gameObject.SetActive(active);
                    }
                }
            }

            // Transform

            if (_targetComponent.RequiresTransformSerialization(level))
            {
                SerializeStateTransform(serializer, level, options, SelfTransformVarName, _targetComponent.TransformStateSaveSpace, _targetComponent.transform);
            }

            // User custom serialization

            customSerializeStateHandler?.Invoke(serializer.IsReading, _targetComponent.StateSerializationVersion, level, options);

            OnStateSerialized(_targetComponent, GetStateSaveEventArgs(serializer, level, options));
        }

        /// <summary>
        ///     Serializes a value only when necessary, depending on <paramref name="level" />, <paramref name="options" /> and if
        ///     the value changed.<br />
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="level">The amount of data to serialize</param>
        /// <param name="options">Options</param>
        /// <param name="varName">
        ///     The parameter name. It will be used to track value changes over time. If it is null or empty,
        ///     it will be serialized without checking for value changes. The name must be unique to any other transform or value
        ///     serialized for the target component using <see cref="SerializeStateValue{TV}" /> or
        ///     <see cref="SerializeStateTransform" />.
        /// </param>
        /// <param name="value">A reference to the value being loaded/saved</param>
        [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
        public void SerializeStateValue<TV>(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string varName, ref TV value)
        {
            // Initialize dictionaries if necessary. We store a deep copy to be able to check for changes later by comparing values.

            if (!string.IsNullOrEmpty(varName))
            {
                if (!_initialValues.ContainsKey(varName))
                {
                    _initialValues.Add(varName, ObjectExt.DeepCopy(value));
                }
                else if (options.HasFlag(UxrStateSaveOptions.ResetChangesCache))
                {
                    _initialValues[varName] = ObjectExt.DeepCopy(value);
                }

                if (!_lastValues.ContainsKey(varName))
                {
                    _lastValues.Add(varName, ObjectExt.DeepCopy(value));
                }
                else if (options.HasFlag(UxrStateSaveOptions.ResetChangesCache))
                {
                    _lastValues[varName] = ObjectExt.DeepCopy(value);
                }
            }

            // Here we need to write read/write parts separately to implement comparison logic

            bool serialize = false;

            if (!serializer.IsReading)
            {
                // When writing, check if the data changed to export changes only.
                // We use a floating point precision threshold for specific types to help avoiding redundant writes.

                if (options.HasFlag(UxrStateSaveOptions.DontCheckCache))
                {
                    serialize = true;
                }
                else
                {
                    switch (level)
                    {
                    
                        case UxrStateSaveLevel.None: return;

                        case UxrStateSaveLevel.ChangesSinceBeginning:

                            serialize = varName == null || !ObjectExt.ValuesEqual(value, _initialValues[varName], UxrConstants.Math.DefaultPrecisionThreshold);
                            break;

                        case UxrStateSaveLevel.ChangesSincePreviousSave:

                            serialize = varName == null || !ObjectExt.ValuesEqual(value, _lastValues[varName], UxrConstants.Math.DefaultPrecisionThreshold);
                            break;

                        case UxrStateSaveLevel.Complete:

                            serialize = true;
                            break;
                    }
                }
            }

            if (!options.HasFlag(UxrStateSaveOptions.DontSerialize))
            {
                // When reading, will deserialize a boolean telling whether there is data to be deserialized. If a false value is deserialized as a result, it means that the previous value didn't change and no new data needs to be read.
                // When writing, will serialize a boolean telling whether any value will be written. If a false value is serialized, it means that the previous value didn't change and no data needs to be written.
                serializer.Serialize(ref serialize);
            }

            if (serialize)
            {
                object oldValue = !string.IsNullOrEmpty(varName) ? ObjectExt.DeepCopy(value) : null;
                
                OnVarSerializing(_targetComponent, GetStateSaveEventArgs(serializer, level, options, varName, value, oldValue));

                if (!options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    serializer.SerializeAnyVar(ref value);
                }

                if (!options.HasFlag(UxrStateSaveOptions.DontCacheChanges) && !string.IsNullOrEmpty(varName))
                {
                    _lastValues[varName] = ObjectExt.DeepCopy(value);
                }

                SerializeCounter++;

                OnVarSerialized(_targetComponent, GetStateSaveEventArgs(serializer, level, options, varName, value, oldValue));
            }
        }

        /// <summary>
        ///     Serializes transform data.
        ///     The Transform can be for the target component or any other component tracked by it, normally children in the
        ///     hierarchy. For example, an avatar serializes the position of the head and hands.
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="level">The amount of data to serialize</param>
        /// <param name="options">Options</param>
        /// <param name="transformVarName">
        ///     A name to identify the transform. It will be used to track value changes over time. If it is null or empty,
        ///     it will be serialized without checking for value changes. The name must be unique to any other transform or
        ///     value serialized for the target component using <see cref="SerializeStateValue{TV}" /> or
        ///     <see cref="SerializeStateTransform" />.
        /// </param>
        /// <param name="space">
        ///     The space the transform data is specified in, when writing. Scale will always be stored in local
        ///     space.
        /// </param>
        /// <param name="transform">
        ///     The transform to serialize. It can be the target component's Transform or any other transform serialized by the
        ///     component.
        /// </param>
        public void SerializeStateTransform(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string transformVarName, UxrTransformSpace space, Transform transform)
        {
            // Can't use ref with Transform property directly, so we need to implement read/write paths separately

            if (serializer.IsReading)
            {
                // Read parent

                IUxrUniqueId newUniqueParent     = transform.parent != null ? transform.parent.GetComponent<IUxrUniqueId>() : null;
                IUxrUniqueId currentUniqueParent = newUniqueParent;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformParent), ref newUniqueParent);

                if (currentUniqueParent != newUniqueParent)
                {
                    Debug.Log($"{transform.name} parent changed from {currentUniqueParent} to {newUniqueParent}");
                    
                    if (newUniqueParent != null)
                    {
                        Component newParentComponent = newUniqueParent as Component;
                        Transform newParentTransform = newParentComponent != null ? newParentComponent.transform : null;

                        if (newParentTransform != null)
                        {
                            if (transform.parent != null)
                            {
                                // If there is a current parent, only switch parent if the new parent has also a UniqueID

                                Component currentParentComponent = currentUniqueParent as Component;

                                if (currentParentComponent != null && newParentComponent != null && transform.parent != newParentComponent.transform)
                                {
                                    transform.SetParent(newParentTransform);
                                }
                            }
                            else
                            {
                                // If there is no current parent, switch parent to the new UniqueID

                                transform.SetParent(newParentTransform);
                            }
                        }
                    }
                    else
                    {
                        if (currentUniqueParent != null)
                        {
                            // If the new parent is null, switch only if the current parent has a UniqueID
                            transform.SetParent(null);
                        }
                    }
                }

                // Read space

                object boxedSpace = space;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformSpace), ref boxedSpace);
                space = (UxrTransformSpace)boxedSpace;

                object boxedPos   = GetPosition(transform, space);
                object boxedRot   = GetRotation(transform, space);
                object boxedScale = transform.localScale;

                // Read position

                int counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformPos), ref boxedPos);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    SetPosition(transform, space, (Vector3)boxedPos);
                }

                // Read rotation

                counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformRot), ref boxedRot);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    SetRotation(transform, space, (Quaternion)boxedRot);
                }

                // Read scale

                counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformScale), ref boxedScale);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    transform.localScale = (Vector3)boxedScale;
                }
            }
            else
            {
                // Write parent

                int  counterBeforeParent = SerializeCounter;
                bool parentSerialized    = false;

                IUxrUniqueId uniqueParent = transform.parent != null ? transform.parent.GetComponent<IUxrUniqueId>() : null;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformParent), ref uniqueParent);

                if (counterBeforeParent != SerializeCounter)
                {
                    parentSerialized = true;
                }

                // Write space

                object boxedSpace = space;
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformSpace), ref boxedSpace);

                // Compute values

                Vector3    position = GetPosition(transform, space);
                Quaternion rotation = GetRotation(transform, space);

                // Write values

                object boxedPos   = position;
                object boxedRot   = rotation;
                object boxedScale = transform.localScale;

                if (parentSerialized)
                {
                    // If the parent was serialized, it means that it changed or was forced to be serialized.
                    // In this case we need to make sure that the transform components are serialized too.    
                    options |= UxrStateSaveOptions.DontCheckCache;
                }

                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformPos),   ref boxedPos);
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformRot),   ref boxedRot);
                SerializeStateValue(serializer, level, options, GetTransformVarName(transformVarName, NameTransformScale), ref boxedScale);
            }
        }

        /// <summary>
        ///     Interpolates state variables.
        /// </summary>
        /// <param name="vars">Contains the variables that can be interpolated for the target component</param>
        /// <param name="t">Interpolation value [0.0, 1.0]</param>
        /// <param name="customInterpolateStateHandler">The user-defined interpolate state handler for the component</param>
        /// <param name="getInterpolator">A function that gets the interpolator for a given serialized var</param>
        public void InterpolateState(in UxrStateInterpolationVars vars, float t, InterpolateStateHandler customInterpolateStateHandler, Func<string, UxrVarInterpolator> getInterpolator)
        {
            if (_targetComponent == null)
            {
                return;
            }
            
            InterpolateStateTransform(vars, t, SelfTransformVarName, _targetComponent.transform, _targetComponent.TransformStateSaveSpace, getInterpolator);

            customInterpolateStateHandler?.Invoke(vars, t);
        }

        /// <summary>
        ///     Interpolates a transform.
        /// </summary>
        /// <param name="vars">Contains the variables that can be interpolated for the target component</param>
        /// <param name="t">Interpolation value [0.0, 1.0]</param>
        /// <param name="transformVarName">
        ///     The name assigned to the transform when serializing it using
        ///     <see cref="SerializeStateTransform" />
        /// </param>
        /// <param name="targetTransform">The target transform</param>
        /// <param name="space">
        ///     The space in which the transform data was serialized using <see cref="SerializeStateTransform" />
        /// </param>
        /// <param name="getInterpolator">A function that gets the interpolator for a given serialized var</param>
        public void InterpolateStateTransform(in UxrStateInterpolationVars vars, float t, string transformVarName, Transform targetTransform, UxrTransformSpace space, Func<string, UxrVarInterpolator> getInterpolator)
        {
            if (getInterpolator == null)
            {
                return;
            }

            string posVarName   = GetTransformVarName(transformVarName, NameTransformPos);
            string rotVarName   = GetTransformVarName(transformVarName, NameTransformRot);
            string scaleVarName = GetTransformVarName(transformVarName, NameTransformScale);

            if (vars.Values.ContainsKey(posVarName))
            {
                if (getInterpolator(posVarName) is UxrVector3Interpolator positionInterpolator)
                {
                    SetPosition(targetTransform, space, positionInterpolator.Interpolate((Vector3)vars.Values[posVarName].OldValue, (Vector3)vars.Values[posVarName].NewValue, t));
                }
            }

            if (vars.Values.ContainsKey(rotVarName))
            {
                if (getInterpolator(rotVarName) is UxrQuaternionInterpolator rotationInterpolator)
                {
                    SetRotation(targetTransform, space, rotationInterpolator.Interpolate((Quaternion)vars.Values[rotVarName].OldValue, (Quaternion)vars.Values[rotVarName].NewValue, t));
                }
            }

            if (vars.Values.ContainsKey(scaleVarName))
            {
                if (getInterpolator(scaleVarName) is UxrVector3Interpolator scaleInterpolator)
                {
                    targetTransform.localScale = scaleInterpolator.Interpolate((Vector3)vars.Values[scaleVarName].OldValue, (Vector3)vars.Values[scaleVarName].NewValue, t);
                }
            }
        }

        /// <summary>
        ///     Gets the default interpolator for the given variable.
        /// </summary>
        /// <param name="varName">The variable name</param>
        /// <returns>Interpolator or null to not interpolate</returns>
        public UxrVarInterpolator GetDefaultInterpolator(string varName)
        {
            if (!_initialValues.ContainsKey(varName) || _initialValues[varName] == null)
            {
                return null;
            }

            Type type = _initialValues[varName].GetType();

            if (type == typeof(Vector3))
            {
                return UxrVector3Interpolator.DefaultInterpolator;
            }
            if (type == typeof(Quaternion))
            {
                return UxrQuaternionInterpolator.DefaultInterpolator;
            }
            if (type == typeof(float))
            {
                return UxrFloatInterpolator.DefaultInterpolator;
            }
            if (type == typeof(int))
            {
                return UxrIntInterpolator.DefaultInterpolator;
            }
            if (type == typeof(Vector2))
            {
                return UxrVector2Interpolator.DefaultInterpolator;
            }
            if (type == typeof(Vector4))
            {
                return UxrVector4Interpolator.DefaultInterpolator;
            }
            if (type == typeof(Color))
            {
                return UxrColorInterpolator.DefaultInterpolator;
            }
            if (type == typeof(Color32))
            {
                return UxrColor32Interpolator.DefaultInterpolator;
            }

            return null;
        }

        /// <summary>
        ///     Checks whether a serialized var name is the name given to the position component of a given transform serialized
        ///     using <see cref="SerializeStateTransform" />.
        /// </summary>
        /// <param name="varName">The variable name to check</param>
        /// <param name="transformVarName">The name given to the transform using <see cref="SerializeStateTransform" /></param>
        /// <returns>Whether <paramref name="varName" /> is the name assigned to the position component of the given transform</returns>
        public bool IsTransformPositionVarName(string varName, string transformVarName)
        {
            if (!_transformVarNameCache.TryGetValue(transformVarName, out Dictionary<string, string> innerDict))
            {
                return false;
            }

            return innerDict.TryGetValue(NameTransformPos, out string cachedValue) && cachedValue == varName;
        }

        /// <summary>
        ///     Checks whether a serialized var name is the name given to the rotation component of a given transform serialized
        ///     using <see cref="SerializeStateTransform" />.
        /// </summary>
        /// <param name="varName">The variable name to check</param>
        /// <param name="transformVarName">The name given to the transform using <see cref="SerializeStateTransform" /></param>
        /// <returns>Whether <paramref name="varName" /> is the name assigned to the rotation component of the given transform</returns>
        public bool IsTransformRotationVarName(string varName, string transformVarName)
        {
            if (!_transformVarNameCache.TryGetValue(transformVarName, out Dictionary<string, string> innerDict))
            {
                return false;
            }

            return innerDict.TryGetValue(NameTransformRot, out string cachedValue) && cachedValue == varName;
        }

        /// <summary>
        ///     Checks whether a serialized var name is the name given to the scale component of a given transform serialized using
        ///     <see cref="SerializeStateTransform" />.
        /// </summary>
        /// <param name="varName">The variable name to check</param>
        /// <param name="transformVarName">The name given to the transform using <see cref="SerializeStateTransform" /></param>
        /// <returns>Whether <paramref name="varName" /> is the name assigned to the scale component of the given transform</returns>
        public bool IsTransformScaleVarName(string varName, string transformVarName)
        {
            if (!_transformVarNameCache.TryGetValue(transformVarName, out Dictionary<string, string> innerDict))
            {
                return false;
            }

            return innerDict.TryGetValue(NameTransformScale, out string cachedValue) && cachedValue == varName;
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        protected override void OnStateSerializing(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            base.OnStateSerializing(stateSave, e);
            Monitor.RaiseStateSerializing(e);
        }

        /// <inheritdoc />
        protected override void OnStateSerialized(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            base.OnStateSerialized(stateSave, e);
            Monitor.RaiseStateSerialized(e);
        }

        /// <inheritdoc />
        protected override void OnVarSerializing(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            base.OnVarSerializing(stateSave, e);
            Monitor.RaiseVarSerializing(e);
        }

        /// <inheritdoc />
        protected override void OnVarSerialized(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            base.OnVarSerialized(stateSave, e);
            Monitor.RaiseVarSerialized(e);
        }

        #endregion

        #region Protected Overrides UxrStateSaveImplementer

        /// <inheritdoc />
        protected override void StoreInitialState()
        {
            if (_targetComponent == null)
            {
                return;
            }
            
            // Cache the initial state after the first frame. We use a dummy serializer in write mode to initialize the changes cache without saving any data.
            _targetComponent.SerializeState(UxrDummySerializer.WriteModeSerializer,
                                            _targetComponent.StateSerializationVersion,
                                            UxrStateSaveLevel.ChangesSinceBeginning,
                                            UxrStateSaveOptions.DontSerialize | UxrStateSaveOptions.ResetChangesCache | UxrStateSaveOptions.FirstFrame);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Fills the internal state event args object with the given parameters.
        /// </summary>
        /// <param name="serializer">The serializer</param>
        /// <param name="level">Serialization level</param>
        /// <param name="options">Serialization options</param>
        /// <param name="varName">Var name or null when not serializing any var</param>
        /// <param name="value">Var value or null when not serializing any var</param>
        /// <param name="oldValue">Old var value or null when not reading any var</param>
        /// <returns>The state event args</returns>
        private UxrStateSaveEventArgs GetStateSaveEventArgs(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string varName = null, object value = null, object oldValue = null)
        {
            _stateSaveArgs.Set(serializer, level, options, varName, value, oldValue);
            return _stateSaveArgs;
        }

        /// <summary>
        ///     Gets a transform's position in a given space.
        /// </summary>
        /// <param name="transform">Transform to get the position of</param>
        /// <param name="space">Coordinates to get the position in</param>
        /// <returns>Position in the given coordinates</returns>
        private Vector3 GetPosition(Transform transform, UxrTransformSpace space)
        {
            switch (space)
            {
                case UxrTransformSpace.World: return transform.position;
                case UxrTransformSpace.Local: return transform.localPosition;

                case UxrTransformSpace.Avatar:

                    UxrAvatar avatar = GetAvatar();

                    if (avatar)
                    {
                        return avatar.transform.InverseTransformPoint(transform.position);
                    }

                    break;

                default: throw new ArgumentOutOfRangeException(nameof(space), space, "Transform space not implemented");
            }

            return transform.position;
        }

        /// <summary>
        ///     Gets a transform's rotation in a given space.
        /// </summary>
        /// <param name="transform">Transform to get the rotation of</param>
        /// <param name="space">Coordinates to get the rotation in</param>
        /// <returns>Rotation in the given coordinates</returns>
        private Quaternion GetRotation(Transform transform, UxrTransformSpace space)
        {
            switch (space)
            {
                case UxrTransformSpace.World: return transform.rotation;
                case UxrTransformSpace.Local: return transform.localRotation;

                case UxrTransformSpace.Avatar:

                    UxrAvatar avatar = GetAvatar();

                    if (avatar)
                    {
                        return Quaternion.Inverse(avatar.transform.rotation) * transform.rotation;
                    }

                    break;

                default: throw new ArgumentOutOfRangeException(nameof(space), space, "Transform space not implemented");
            }

            return transform.rotation;
        }

        /// <summary>
        ///     Sets a transform's position in a given space.
        /// </summary>
        /// <param name="transform">Transform to set the position of</param>
        /// <param name="space">Coordinates to set the position in</param>
        /// <param name="position">Position value</param>
        private void SetPosition(Transform transform, UxrTransformSpace space, Vector3 position)
        {
            switch (space)
            {
                case UxrTransformSpace.World:
                    transform.position = position;
                    break;

                case UxrTransformSpace.Local:
                    transform.localPosition = position;
                    break;

                case UxrTransformSpace.Avatar:

                    UxrAvatar avatar = GetAvatar();

                    if (avatar)
                    {
                        transform.position = avatar.transform.TransformPoint(position);
                    }

                    break;
            }
        }

        /// <summary>
        ///     Sets a transform's rotation in a given space.
        /// </summary>
        /// <param name="transform">Transform to set the rotation of</param>
        /// <param name="space">Coordinates to set the rotation in</param>
        /// <param name="rotation">Rotation value</param>
        private void SetRotation(Transform transform, UxrTransformSpace space, Quaternion rotation)
        {
            switch (space)
            {
                case UxrTransformSpace.World:
                    transform.rotation = rotation;
                    break;

                case UxrTransformSpace.Local:
                    transform.localRotation = rotation;
                    break;

                case UxrTransformSpace.Avatar:

                    UxrAvatar avatar = GetAvatar();

                    if (avatar)
                    {
                        transform.rotation = avatar.transform.rotation * rotation;
                    }

                    break;
            }
        }

        /// <summary>
        ///     Gets the avatar the target component belongs to.
        /// </summary>
        /// <returns>Avatar component or null if the component doesn't belong to an avatar</returns>
        private UxrAvatar GetAvatar()
        {
            if (_avatar != null)
            {
                return _avatar;
            }

            _avatar = _targetComponent.GetComponentInParent<UxrAvatar>();
            return _avatar;
        }

        /// <summary>
        ///     Gets the name used to identify a serialized transform variable if it's cached, or caches it if it's not yet stored.
        ///     This is used to avoid many string manipulation calls.
        /// </summary>
        /// <param name="transformVarName">The name assigned to the Transform</param>
        /// <param name="subName">The name to identify which part of the Transform is serialized (pos, rot, scale, parent or space)</param>
        /// <returns>The name</returns>
        private string GetTransformVarName(string transformVarName, string subName)
        {
            if (_transformVarNameCache.TryGetValue(transformVarName, out Dictionary<string, string> innerDict))
            {
                if (innerDict.TryGetValue(subName, out string cachedValue))
                {
                    return cachedValue;
                }
            }
            else
            {
                innerDict                                = new Dictionary<string, string>();
                _transformVarNameCache[transformVarName] = innerDict;
            }

            // If not in cache, compute the value and cache it
            string combinedValue = string.Concat(transformVarName, subName);
            innerDict[subName] = combinedValue;

            return combinedValue;
        }

        #endregion

        #region Private Types & Data

        private const string NameIsEnabled       = "__enabled";
        private const string NameIsActive        = "__active";
        private const string NameTransformParent = ".tf.parent";
        private const string NameTransformSpace  = ".tf.space";
        private const string NameTransformPos    = ".tf.pos";
        private const string NameTransformRot    = ".tf.rot";
        private const string NameTransformScale  = ".tf.scl";

        private readonly T _targetComponent;

        private readonly Dictionary<string, object>                     _initialValues         = new Dictionary<string, object>();
        private readonly Dictionary<string, object>                     _lastValues            = new Dictionary<string, object>();
        private readonly UxrStateSaveEventArgs                          _stateSaveArgs         = new UxrStateSaveEventArgs();
        private readonly Dictionary<string, Dictionary<string, string>> _transformVarNameCache = new Dictionary<string, Dictionary<string, string>>();
        private          bool                                           _registered;

        private UxrAvatar _avatar;

        #endregion
    }
}