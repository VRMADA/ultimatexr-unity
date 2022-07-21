// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentProcessor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Component processor delegate used by <see cref="UxrEditorUtils.ProcessAllProjectComponents{T}" />.
    ///     The component processor is responsible for modifying/processing all the components that the method will send.
    /// </summary>
    /// <typeparam name="T">Type of component to process</typeparam>
    public delegate bool UxrComponentProcessor<T>(UxrComponentInfo<T> componentInfo) where T : Component;
}