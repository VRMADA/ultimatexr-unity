// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.InstanceInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.Unity;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    public partial class UxrInstanceManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores instantiation information.
        /// </summary>
        private class InstanceInfo : IUxrSerializable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the Id of the prefab that was instantiated. Null if it's an empty GameObject.
            /// </summary>
            public string PrefabId => _prefabId;

            /// <summary>
            ///     Gets the name of the empty instance, if the instance was created using
            ///     <see cref="UxrInstanceManager.InstantiateEmptyGameObject" />.
            /// </summary>
            public string Name => _name;

            /// <summary>
            ///     Gets the parent if the object was parented to any.
            /// </summary>
            public IUxrUniqueId Parent => _parent;

            /// <summary>
            ///     Gets the position. It will contain relative position to the parent if parented.
            /// </summary>
            public Vector3 Position => _position;

            /// <summary>
            ///     Gets the rotation. It will contain relative rotation to the parent if parented.
            /// </summary>
            public Quaternion Rotation => _rotation;

            /// <summary>
            ///     Gets the local scale.
            /// </summary>
            public Vector3 Scale => _scale;

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="instance">The instance</param>
            /// <param name="prefabId">
            ///     The id of the prefab that was instantiated or null if the instance was created using
            ///     <see cref="UxrInstanceManager.InstantiateEmptyGameObject" />.
            /// </param>
            public InstanceInfo(IUxrUniqueId instance, string prefabId)
            {
                _prefabId = prefabId;
                _name     = string.IsNullOrEmpty(prefabId) ? instance.GameObject.name : null;
                _parent   = instance.Transform.parent != null ? instance.Transform.parent.GetComponent<IUxrUniqueId>() : null;
                _position = _parent != null ? instance.Transform.localPosition : instance.Transform.position;
                _rotation = _parent != null ? instance.Transform.localRotation : instance.Transform.rotation;
                _scale    = instance.Transform.localScale;
            }

            /// <summary>
            ///     Default constructor required for serialization.
            /// </summary>
            private InstanceInfo()
            {
            }

            #endregion

            #region Implicit IUxrSerializable

            /// <inheritdoc />
            public int SerializationVersion => 0;

            /// <inheritdoc />
            public void Serialize(IUxrSerializer serializer, int serializationVersion)
            {
                serializer.Serialize(ref _prefabId);
                serializer.Serialize(ref _name);
                serializer.SerializeUniqueComponent(ref _parent);
                serializer.Serialize(ref _position);
                serializer.Serialize(ref _rotation);
                serializer.Serialize(ref _scale);
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Updates the info using the current object data.
            /// </summary>
            /// <param name="instance">The GameObject to update the information of</param>
            public void UpdateInfoUsingObject(GameObject instance)
            {
                Transform    transform       = instance.transform;
                IUxrUniqueId parent          = transform.parent != null ? transform.parent.GetComponent<IUxrUniqueId>() : null;
                Transform    parentTransform = parent?.Transform;

                if (parentTransform != null)
                {
                    // Use relative parent data
                    _parent   = parent;
                    _position = transform.localPosition;
                    _rotation = transform.localRotation;
                }
                else
                {
                    // Use world data
                    _parent   = null;
                    _position = transform.position;
                    _rotation = transform.rotation;
                }

                _scale = transform.localScale;
            }

            /// <summary>
            ///     Updates the object using the current information.
            /// </summary>
            /// <param name="instance">The GameObject to update</param>
            public void UpdateObjectUsingInfo(GameObject instance)
            {
                Transform transform       = instance.transform;
                Transform parentTransform = Parent?.Transform;

                if (parentTransform != null)
                {
                    // Use relative parent data

                    if (transform.parent != parentTransform)
                    {
                        transform.SetParent(parentTransform);
                    }

                    TransformExt.SetLocalPositionAndRotation(transform, Position, Rotation);
                }
                else
                {
                    // Use world

                    if (transform.parent != null)
                    {
                        transform.SetParent(null);
                    }

                    transform.SetPositionAndRotation(Position, Rotation);
                }

                transform.localScale = Scale;
            }

            #endregion

            #region Private Types & Data

            private string       _prefabId;
            private string       _name;
            private IUxrUniqueId _parent;
            private Vector3      _position;
            private Quaternion   _rotation;
            private Vector3      _scale;

            #endregion
        }

        #endregion
    }
}