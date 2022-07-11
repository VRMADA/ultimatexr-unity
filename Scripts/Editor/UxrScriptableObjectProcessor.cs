// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrScriptableObjectProcessor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     <see cref="ScriptableObject" /> processor delegate used by
    ///     <see cref="UxrEditorUtils.ProcessAllScriptableObjects{T}" />.
    ///     The processor is responsible for modifying/processing all the assets that the method will send.
    /// </summary>
    /// <typeparam name="T">Type of scriptable object to process</typeparam>
    public delegate bool UxrScriptableObjectProcessor<T>(T scriptableObject) where T : ScriptableObject;
}