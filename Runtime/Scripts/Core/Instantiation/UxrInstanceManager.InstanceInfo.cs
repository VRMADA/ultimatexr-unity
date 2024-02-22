// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.InstanceInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;
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
            ///     Gets the Id of the prefab that was instantiated.
            /// </summary>
            public string PrefabId => _prefabId;

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

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="instance">The instance</param>
            /// <param name="prefabId">The id of the prefab that was instantiated</param>
            public InstanceInfo(IUxrUniqueId instance, string prefabId)
            {
                _prefabId = prefabId;
                _parent   = instance.Transform.parent != null ? instance.Transform.parent.GetComponent<IUxrUniqueId>() : null;
                _position = _parent != null ? instance.Transform.localPosition : instance.Transform.position;
                _rotation = _parent != null ? instance.Transform.localRotation : instance.Transform.rotation;
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
                serializer.SerializeUniqueComponent(ref _parent);
                serializer.Serialize(ref _position);
                serializer.Serialize(ref _rotation);
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

                    transform.SetLocalPositionAndRotation(Position, Rotation);
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
            }

            #endregion

            #region Private Types & Data

            private string       _prefabId;
            private IUxrUniqueId _parent;
            private Vector3      _position;
            private Quaternion   _rotation;

            #endregion
        }

        #endregion
    }
}