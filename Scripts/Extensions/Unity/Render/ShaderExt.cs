// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShaderExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Shader" /> extensions.
    /// </summary>
    public static class ShaderExt
    {
        #region Public Types & Data

        public const string ShaderBase = "UltimateXR/";

        public static Shader UnlitAdditiveColor               => Shader.Find($"{ShaderBase}Basic Unlit/Unlit Additive Color");
        public static Shader UnlitTransparentColor            => Shader.Find($"{ShaderBase}Basic Unlit/Unlit Transparent Color");
        public static Shader UnlitTransparentColorNoDepthTest => Shader.Find($"{ShaderBase}Basic Unlit/Unlit Transparent Color (No Depth Test)");
        public static Shader UnlitOverlayFade                 => Shader.Find($"{ShaderBase}Basic Unlit/Overlay Fade");

        #endregion
    }
}