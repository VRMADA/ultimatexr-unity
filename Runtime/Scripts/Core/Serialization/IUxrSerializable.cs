// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrSerializable.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Interface to add serialization capabilities to a class.
    /// </summary>
    public interface IUxrSerializable
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the current serialization version of the class that implements the interface. It has the same goal as
        ///     <see cref="UxrConstants.Serialization.CurrentBinaryVersion" /> but this version property is specific to each class
        ///     that implements the <see cref="IUxrSerializable" /> interface, which may be used outside the UltimateXR scope,
        ///     in user specific classes that want to benefit from serialization.<br />
        ///     Each class that implement the <see cref="IUxrSerializable" /> interface may have its own version. It is a number
        ///     that gets incremented by one each time the serialization format of the class that implements this interface
        ///     changes, enabling backwards compatibility.
        /// </summary>
        int SerializationVersion { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Serializes or deserializes the object. The serializer interface uses the same methods to serialize and to
        ///     deserialize, instead of requiring separate serialization and deserialization methods.
        ///     This simplifies implementations and helps eliminating bugs due to inconsistencies between reading and writing.
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="serializationVersion">
        ///     When reading it tells the <see cref="SerializationVersion" /> the data was
        ///     serialized with. When writing it uses the latest <see cref="SerializationVersion" /> version.
        /// </param>
        void Serialize(IUxrSerializer serializer, int serializationVersion);

        #endregion
    }
}