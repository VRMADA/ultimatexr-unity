// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveImplementer_1.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Helper class simplifying the implementation of the <see cref="IUxrStateSave" /> interface.
    ///     This class includes functionality to serialize the state of components.
    ///     It is utilized by <see cref="UxrComponent" /> to implement <see cref="IUxrStateSave" />.
    ///     In scenarios where custom classes cannot inherit from <see cref="UxrComponent" /> for state saving capabilities,
    ///     this class is designed to implement the interface.
    /// </summary>
    public class UxrStateSaveImplementer<T> where T : Component, IUxrStateSave
    {
        #region Public Types & Data

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
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Serializes the state of a component, handling serialization of different elements if necessary:
        ///     <list type="bullet">
        ///         <item>
        ///             The transform, using <see cref="IUxrStateSave.RequiresTransformStateSave" /> and
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
        public void SerializeState(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, Action<bool, int, UxrStateSaveLevel, UxrStateSaveOptions> customSerializeStateHandler)
        {
            if (_targetComponent.RequiresTransformSerialization(level))
            {
                SerializeStateTransform(serializer, level, options, "this.transform", _targetComponent.TransformStateSaveSpace, _targetComponent.transform);
            }

            customSerializeStateHandler?.Invoke(serializer.IsReading, _targetComponent.StateSerializationVersion, level, options);
        }

        /// <summary>
        ///     Serializes a value only when necessary, depending on <paramref name="level" />, <paramref name="options" /> and if
        ///     the value changed.
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="level">The amount of data to serialize</param>
        /// <param name="options">Options</param>
        /// <param name="name">
        ///     The parameter name. It will be used to track value changes over time. If it is null or empty,
        ///     it will be serialized without checking for value changes. The name must be unique to any other transform or value
        ///     serialized for the target component using <see cref="SerializeStateValue" /> or
        ///     <see cref="SerializeStateTransform" />.
        /// </param>
        /// <param name="value">A reference to the value being loaded/saved</param>
        public void SerializeStateValue<TV>(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string name, ref TV value)
        {
            // Initialize dictionaries if necessary. We store a deep copy to be able to check for changes later by comparing values.

            if (!string.IsNullOrEmpty(name))
            {
                if (!_initialValues.ContainsKey(name))
                {
                    _initialValues.Add(name, value.DeepCopy());
                }
                else if (options.HasFlag(UxrStateSaveOptions.ForceResetChangesCache))
                {
                    _initialValues[name] = value.DeepCopy();
                }

                if (!_lastValues.ContainsKey(name))
                {
                    _lastValues.Add(name, value.DeepCopy());
                }
                else if (options.HasFlag(UxrStateSaveOptions.ForceResetChangesCache))
                {
                    _lastValues[name] = value.DeepCopy();
                }
            }

            // Here we need to write read/write parts separately to implement comparison logic

            bool serialize = false;

            if (!serializer.IsReading)
            {
                // When writing, check if the data changed to export changes only.
                // We use a floating point precision threshold for specific types to help avoiding redundant writes.

                switch (level)
                {
                    case UxrStateSaveLevel.None: return;

                    case UxrStateSaveLevel.ChangesSinceBeginning:

                        serialize = name == null || !value.ValuesEqual(_initialValues[name], UxrConstants.Math.DefaultPrecisionThreshold);
                        break;

                    case UxrStateSaveLevel.ChangesSincePreviousSave:

                        serialize = name == null || !value.ValuesEqual(_lastValues[name], UxrConstants.Math.DefaultPrecisionThreshold);
                        break;

                    case UxrStateSaveLevel.Complete:

                        serialize = true;
                        break;
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
                if (!options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    if (!string.IsNullOrEmpty(name) && !serializer.IsReading)
                    {
                        Debug.Log($"Component {_targetComponent} value {name} changed from {_initialValues[name]} to {value}");
                    }

                    serializer.SerializeAnyVar(ref value);
                }

                if (!options.HasFlag(UxrStateSaveOptions.DontCacheChanges) && !string.IsNullOrEmpty(name))
                {
                    _lastValues[name] = value.DeepCopy();
                }

                SerializeCounter++;
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
        /// <param name="name">
        ///     A name to identify the transform. It will be used to track value changes over time. If it is null or empty,
        ///     it will be serialized without checking for value changes. The name must be unique to any other transform or
        ///     value serialized for the target component using <see cref="SerializeStateValue" /> or
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
        public void SerializeStateTransform(IUxrSerializer serializer, UxrStateSaveLevel level, UxrStateSaveOptions options, string name, UxrTransformSpace space, Transform transform)
        {
            // Can't use ref with Transform property directly, so we need to implement read/write paths separately

            if (serializer.IsReading)
            {
                // Read parent

                IUxrUniqueId newUniqueParent     = transform.parent != null ? transform.parent.GetComponent<IUxrUniqueId>() : null;
                IUxrUniqueId currentUniqueParent = newUniqueParent;
                SerializeStateValue(serializer, level, options, name + TransformParent, ref newUniqueParent);

                if (currentUniqueParent != newUniqueParent)
                {
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
                SerializeStateValue(serializer, level, options, name + TransformSpace, ref boxedSpace);
                space = (UxrTransformSpace)boxedSpace;

                object boxedPos   = GetPosition(transform, space);
                object boxedRot   = GetRotation(transform, space);
                object boxedScale = transform.localScale;

                // Read position

                int counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, name + TransformPos, ref boxedPos);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    switch (space)
                    {
                        case UxrTransformSpace.World:
                            transform.position = (Vector3)boxedPos;
                            break;

                        case UxrTransformSpace.Local:
                            transform.localPosition = (Vector3)boxedPos;
                            break;

                        case UxrTransformSpace.Avatar:

                            UxrAvatar avatar = GetAvatar();

                            if (avatar)
                            {
                                transform.position = avatar.transform.TransformPoint((Vector3)boxedPos);
                            }

                            break;
                    }
                }

                // Read rotation

                counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, name + TransformRot, ref boxedRot);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    switch (space)
                    {
                        case UxrTransformSpace.World:
                            transform.rotation = (Quaternion)boxedRot;
                            break;

                        case UxrTransformSpace.Local:
                            transform.localRotation = (Quaternion)boxedRot;
                            break;

                        case UxrTransformSpace.Avatar:

                            UxrAvatar avatar = GetAvatar();

                            if (avatar)
                            {
                                transform.rotation = avatar.transform.rotation * (Quaternion)boxedRot;
                            }

                            break;
                    }
                }

                // Read scale

                counterBefore = SerializeCounter;
                SerializeStateValue(serializer, level, options, name + TransformScale, ref boxedScale);

                if (counterBefore != SerializeCounter && !options.HasFlag(UxrStateSaveOptions.DontSerialize))
                {
                    transform.localScale = (Vector3)boxedScale;
                }
            }
            else
            {
                // Write parent

                IUxrUniqueId uniqueParent = transform.parent != null ? transform.parent.GetComponent<IUxrUniqueId>() : null;
                SerializeStateValue(serializer, level, options, name + TransformParent, ref uniqueParent);

                // Write space

                object boxedSpace = space;
                SerializeStateValue(serializer, level, options, name + TransformSpace, ref boxedSpace);

                // Compute values

                Vector3    position = GetPosition(transform, space);
                Quaternion rotation = GetRotation(transform, space);

                // Write values

                object boxedPos   = position;
                object boxedRot   = rotation;
                object boxedScale = transform.localScale;

                SerializeStateValue(serializer, level, options, name + TransformPos,   ref boxedPos);
                SerializeStateValue(serializer, level, options, name + TransformRot,   ref boxedRot);
                SerializeStateValue(serializer, level, options, name + TransformScale, ref boxedScale);
            }
        }

        /// <summary>
        ///     Notifies that a component's Start() was called.
        /// </summary>
        public void NotifyEndOfFirstFrame()
        {
            // Cache the initial state after the first frame. We use a dummy serializer in write mode to initialize the changes cache without saving any data.
            _targetComponent.SerializeState(UxrDummySerializer.WriteModeSerializer, _targetComponent.StateSerializationVersion, UxrStateSaveLevel.ChangesSinceBeginning, UxrStateSaveOptions.DontSerialize | UxrStateSaveOptions.ForceResetChangesCache);
        }

        #endregion

        #region Private Methods

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

        #endregion

        #region Private Types & Data

        private const string TransformParent = ".tf.parent";
        private const string TransformSpace  = ".tf.space";
        private const string TransformPos    = ".tf.pos";
        private const string TransformRot    = ".tf.rot";
        private const string TransformScale  = ".tf.scl";

        private readonly T _targetComponent;

        private readonly Dictionary<string, object> _initialValues = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _lastValues    = new Dictionary<string, object>();

        private UxrAvatar _avatar;

        #endregion
    }
}