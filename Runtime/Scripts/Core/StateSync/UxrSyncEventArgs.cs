// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncEventArgs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Runtime.Serialization;
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
        ///     Gets or sets the options. See <see cref="UxrStateSyncOptions" /> flags.
        /// </summary>
        public UxrStateSyncOptions Options { get; set; } = UxrStateSyncOptions.Default;

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
        /// <param name="stateSync">Event target that should execute the event</param>
        /// <param name="eventArgs">Event parameters</param>
        /// <param name="errorMessage">Will return null if there were no errors or an error message when the method returns false</param>
        /// <returns>Whether the event was deserialized correctly</returns>
        public static bool DeserializeEventBinary(byte[] serializedEvent, out IUxrStateSync stateSync, out UxrSyncEventArgs eventArgs, out string errorMessage)
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

                    int       serializationVersion   = 0;
                    Exception componentReadException = null;
                    string    eventTypeName          = null;
                    string    eventTypeAssemblyName  = null;
                    Type      syncEventType          = null;

                    try
                    {
                        serializationVersion = reader.ReadUInt16();
                        syncEventType        = reader.ReadType(serializationVersion, out eventTypeName, out eventTypeAssemblyName);
                        stateSync            = reader.ReadAnyVar(serializationVersion) as IUxrStateSync;
                    }
                    catch (Exception e)
                    {
                        if (syncEventType == null)
                        {
                            errorMessage = $"{logPrefix}: Error creating/deserializing event. Cannot read event type. Exception message: {e}";
                            return false;
                        }

                        componentReadException = e;
                    }
                    
                    object eventObject              = FormatterServices.GetUninitializedObject(syncEventType); // Creates instance without calling constructor
                    bool   eventDeserializeStarted  = false;
                    bool   eventDeserializeFinished = false;

                    try
                    {
                        eventArgs = eventObject as UxrSyncEventArgs;
                        
                        if (eventArgs == null)
                        {
                            throw new Exception($"Unknown event class ({TypeExt.GetTypeString(eventTypeName, eventTypeAssemblyName)})");
                        }

                        if (componentReadException == null && stateSync == null)
                        {
                            throw new Exception($"Target component ({TypeExt.GetTypeString(eventTypeName, eventTypeAssemblyName)}) doesn't implement interface {nameof(IUxrStateSync)}");
                        }

                        eventDeserializeStarted = true;

                        eventArgs.SerializeEventInternal(new UxrBinarySerializer(reader, serializationVersion));

                        eventDeserializeFinished = true;

                        if (componentReadException != null)
                        {
                            throw new Exception($"{logPrefix}: Target component wasn't found for event. Event is {eventArgs}. Exception message: {componentReadException}.");
                        }
                    }
                    catch (Exception e)
                    {
                        if (!eventDeserializeStarted || eventDeserializeFinished)
                        {
                            errorMessage = e.Message;
                        }
                        else
                        {
                            if (componentReadException != null)
                            {
                                errorMessage = $"{logPrefix}: Error creating/deserializing event for unknown target component. Event (may show missing data) is {eventArgs}. Exception message: {e}";                                
                            }
                            else
                            {
                                errorMessage = $"{logPrefix}: Error creating/deserializing event for {stateSync}. Event (may show missing data) is {eventArgs}. Exception message: {e}";
                            }
                        }

                        if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                        {
                            Debug.LogError($"{UxrConstants.CoreModule} {errorMessage}");
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
        /// <param name="sourceComponent">
        ///     Component that generated the event.
        /// </param>
        /// <returns>Byte array representing the event</returns>
        public byte[] SerializeEventBinary(IUxrStateSync sourceComponent)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Serialize the binary serialization version
                    writer.Write((ushort)UxrConstants.Serialization.CurrentBinaryVersion);
                    
                    // Serialize event type to be able to instantiate event object using reflection
                    writer.Write(GetType());
                    
                    // Serialize the component that generated the event 
                    writer.WriteAnyVar(sourceComponent);

                    // Serialize the event data
                    SerializeEventInternal(new UxrBinarySerializer(writer));
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