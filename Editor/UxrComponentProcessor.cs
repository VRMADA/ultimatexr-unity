// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentProcessor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Component processor delegate used by <see cref="UxrEditorUtils.ModifyComponent{T}" /> and
    ///     <see cref="UxrEditorUtils.ProcessAllProjectComponents{T}" />.
    ///     The component processor is responsible for modifying/processing all the components that these methods will send.
    /// </summary>
    /// <typeparam name="T">Type of component to process</typeparam>
    /// <param name="onlyCheck">
    ///     Whether to only check if components should be processed, without making any changes. This
    ///     can be used to get how many elements would be changed without modifying any data
    /// </param>
    public delegate bool UxrComponentProcessor<T>(UxrComponentInfo<T> componentInfo, bool onlyCheck) where T : Component;
}