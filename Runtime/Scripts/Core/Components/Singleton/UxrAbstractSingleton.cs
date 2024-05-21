// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAbstractSingleton.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.Components.Singleton
{
    /// <summary>
    ///     Singleton base class.
    /// </summary>
    public abstract class UxrAbstractSingleton : UxrComponent
    {
        #region Protected Overrides UxrComponent

        /// <summary>
        ///     Singletons generate unique ID based on full type name. This ensures that the IDs are the same on all devices and
        ///     message exchanges will work correctly.
        /// </summary>
        protected override bool UniqueIdIsTypeName => true;

        /// <inheritdoc />
        protected override int SerializationOrder => UxrConstants.Serialization.SerializationOrderSingleton;

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Whether the singleton requires <see cref="UnityEngine.Object.DontDestroyOnLoad" /> applied to the GameObject so
        ///     that it doesn't get destroyed when a new scene is loaded.
        /// </summary>
        protected virtual bool NeedsDontDestroyOnLoad => true;

        #endregion
    }
}