// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.IO;
using UnityEngine;

namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Base event args to synchronize the state of entities for network synchronization.
    /// </summary>
    /// <seealso cref="IUxrStateSync" />
    public abstract class UxrSyncEventArgs : EventArgs
    {
        #region Public Types & Data

        /// <summary>
        ///     Whether this event should be synced through network. Some events like <see cref="UxrAvatarMoveEventArgs" /> might
        ///     not be required to be synced since it's usually a NetworkTransform component that can synchronize it better.
        /// </summary>
        public virtual bool ShouldSyncNetworkEvent => true;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        protected UxrSyncEventArgs()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Deserializes event data serialized using <see cref="SerializeEventBinary" /> and returns the intended target and
        ///     event parameters.
        /// </summary>
        /// <param name="serializedEvent">The byte array with the serialized data</param>
        /// <param name="serializationVersion">The serialization version, to provide backwards compatibility</param>
        /// <param name="stateSync">Event target that should execute the event</param>
        /// <param name="eventArgs">Event parameters</param>
        /// <param name="errorMessage">Will return null if there were no errors or an error message when the method returns false</param>
        /// <returns>Whether the event was deserialized correctly</returns>
        public static bool DeserializeEventBinary(byte[] serializedEvent, int serializationVersion, out IUxrStateSync stateSync, out UxrSyncEventArgs eventArgs, out string errorMessage)
        {
            stateSync = null;
            eventArgs = null;

            if (serializedEvent == null)
            {
                errorMessage = "Serialized event byte array is null";
                return false;
            }

            string logPrefix = $"{nameof(UxrSyncEventArgs)}.{nameof(DeserializeEventBinary)}";

            using (MemoryStream stream = new MemoryStream(serializedEvent))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // Instantiate event object and deserialize it.

                    try
                    {
                        Type         syncEventType   = reader.ReadType(serializationVersion, out string typeName, out string assemblyName);
                        UxrComponent targetComponent = reader.ReadUxrComponent(serializationVersion);
                        object       eventObject     = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(syncEventType); // Creates instance without calling constructor

                        eventArgs = eventObject as UxrSyncEventArgs;
                        stateSync = targetComponent;

                        if (eventArgs == null)
                        {
                            throw new Exception($"Unknown event class ({TypeExt.GetTypeString(typeName, assemblyName)})");
                        }

                        if (stateSync == null)
                        {
                            throw new Exception($"Target component ({TypeExt.GetTypeString(typeName, assemblyName)}) doesn't implement interface {nameof(IUxrStateSync)}");
                        }

                        eventArgs.SerializeEventInternal(new UxrBinarySerializer(reader, serializationVersion));
                    }
                    catch (Exception e)
                    {
                        errorMessage = $"{logPrefix}: Error creating/deserializing event. Exception message: {e}";

                        if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                        {
                            Debug.LogError($"{UxrConstants.CoreModule} {logPrefix}: Error creating/deserializing event. Exception message: {e}");
                        }

                        return false;
                    }
                }
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        ///     Serializes the event to a byte array so that it can be sent through the network or saved to disk.
        /// </summary>
        /// <param name="sourceComponent">Component that generated the event</param>
        /// <returns>Byte array representing the event</returns>
        public byte[] SerializeEventBinary(UxrComponent sourceComponent)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Serialize event type to be able to instantiate event object using reflection
                    writer.Write(GetType());

                    // Serialize the ID of the component that generated the event 
                    BinaryWriterExt.Write(writer, sourceComponent);

                    // Serialize the event data
                    SerializeEventInternal(new UxrBinarySerializer(writer, UxrConstants.Serialization.CurrentBinaryVersion));
                }
                stream.Flush();
                return stream.ToArray();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Serialization/Deserialization of event parameters.
        /// </summary>
        /// <param name="serializer">Serializer that can both serialize and deserialize, so that a single method is used</param>
        protected abstract void SerializeEventInternal(IUxrSerializer serializer);

        #endregion
    }
}