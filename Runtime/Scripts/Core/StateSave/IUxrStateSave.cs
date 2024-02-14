// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrStateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Interface for classes to load/save their partial or complete state. This can be used to save
    ///     complete sessions to disk to restore the session later. It can also be used to save partial states
    ///     in a timeline to implement replay functionality.<br />
    ///     To leverage the implementation of this interface, consider using <see cref="UxrStateSaveImplementer{T}" />.<br />
    /// </summary>
    public interface IUxrStateSave : IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the current serialization version of the class that implements the interface. It has the same goal as
        ///     <see cref="UxrConstants.Serialization.CurrentBinaryVersion" /> but this version property is specific to each class
        ///     that implements the <see cref="IUxrStateSave" /> interface, which may be used outside the UltimateXR scope,
        ///     in user specific classes that want to benefit from state serialization.<br />
        ///     Each class that implement the <see cref="IUxrStateSave" /> interface may have its own version. It is a number that
        ///     gets incremented by one each time the serialization format of the class that implements this interface changes,
        ///     enabling backwards compatibility.
        /// </summary>
        int StateSerializationVersion { get; }

        /// <summary>
        ///     Gets the space the transform will be serialized in.
        /// </summary>
        UxrTransformSpace TransformStateSaveSpace { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the transform should be serialized when serializing the state.
        /// </summary>
        /// <param name="level">The amount of data to serialize</param>
        /// <returns>Whether the transform should be serialized</returns>
        bool RequiresTransformSerialization(UxrStateSaveLevel level);

        /// <summary>
        ///     Serializes or deserializes the object state.
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="serializationVersion">
        ///     When reading it tells the <see cref="StateSerializationVersion" /> the data was
        ///     serialized with. When writing it uses the latest <see cref="StateSerializationVersion" /> version.
        /// </param>
        /// <param name="level">
        ///     The amount of data to serialize.
        /// </param>
        /// <param name="options">Options</param>
        /// <returns>Whether there were any values in the state that changed</returns>
        bool SerializeState(IUxrSerializer serializer, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options = UxrStateSaveOptions.None);

        #endregion
    }
}