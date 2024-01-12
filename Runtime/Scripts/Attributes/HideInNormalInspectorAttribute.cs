// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HideInNormalInspectorAttribute.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Attributes
{
    /// <summary>
    ///     Attribute to hide inspector fields in normal mode while still been shown in debug mode.
    ///     From https://answers.unity.com/questions/157775/hide-from-inspector-interface-but-not-from-the-deb.html
    /// </summary>
    public class HideInNormalInspectorAttribute : PropertyAttribute
    {
    }
}