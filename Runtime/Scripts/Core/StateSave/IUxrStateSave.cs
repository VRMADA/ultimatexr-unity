// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrStateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.Unique;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Interface for components to load/save their partial or complete state. This can be used to save
    ///     complete sessions to disk to restore the session later. It can also be used to save partial states
    ///     in a timeline to implement replay functionality.<br />
    ///     To leverage the implementation of this interface, consider using <see cref="UxrStateSaveImplementer{T}" />.<br />
    /// </summary>
    public interface IUxrStateSave : IUxrUniqueId
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the current serialization version of the component type that implements the interface. It has the same goal as
        ///     <see cref="UxrConstants.Serialization.CurrentBinaryVersion" /> but this version property is specific to each class
        ///     that implements the <see cref="IUxrStateSave" /> interface, which may be used outside the UltimateXR scope,
        ///     in user specific classes that want to benefit from state serialization.<br />
        ///     Each class that implement the <see cref="IUxrStateSave" /> interface may have its own version. It is a number that
        ///     gets incremented by one each time the serialization format of the class that implements this interface changes,
        ///     enabling backwards compatibility.
        /// </summary>
        int StateSerializationVersion { get; }

        /// <summary>
        ///     Gets the serialization order of the component. Components are serialized from lower to higher order values.
        /// </summary>
        int SerializationOrder { get; }

        /// <summary>
        ///     Gets whether the component state should be save even when it's disabled.
        ///     This can be useful in components that have state changes even when being disabled. An example is when a
        ///     disabled component is subscribed to an event and the event triggers changes in the component.
        /// </summary>
        bool SaveStateWhenDisabled { get; }

        /// <summary>
        ///     Gets whether to save the enabled state of the component and the active state of the object.
        /// </summary>
        bool SerializeActiveAndEnabledState { get; }

        /// <summary>
        ///     Gets the space the transform will be serialized in.
        /// </summary>
        UxrTransformSpace TransformStateSaveSpace { get; }

        /// <summary>
        ///     Gets the state save monitor.
        /// </summary>
        UxrStateSaveMonitor StateSaveMonitor { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the transform should be serialized when serializing the state.
        /// </summary>
        /// <param name="level">The amount of data to serialize</param>
        /// <returns>Whether the transform should be serialized</returns>
        bool RequiresTransformSerialization(UxrStateSaveLevel level);

        /// <summary>
        ///     Serializes or deserializes the component state.
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="stateSerializationVersion">
        ///     When reading it tells the <see cref="StateSerializationVersion" /> the data was
        ///     serialized with. When writing it uses the latest <see cref="StateSerializationVersion" /> version.
        /// </param>
        /// <param name="level">
        ///     The amount of data to serialize.
        /// </param>
        /// <param name="options">Options</param>
        /// <returns>Whether there were any values in the state that changed</returns>
        bool SerializeState(IUxrSerializer serializer, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options = UxrStateSaveOptions.None);

        /// <summary>
        ///     Interpolates state variables.
        /// </summary>
        /// <param name="vars">Variable names with their old and new values</param>
        /// <param name="t">Interpolation value [0.0, 1.0]</param>
        void InterpolateState(in UxrStateInterpolationVars vars, float t);

        /// <summary>
        ///     Gets the interpolator for a given variable, allowing to use customize interpolation for different variables.
        /// </summary>
        /// <param name="varName">Name of the variable to get the interpolator for</param>
        /// <returns>Interpolator or null to not interpolate</returns>
        UxrVarInterpolator GetInterpolator(string varName);

        #endregion
    }
}