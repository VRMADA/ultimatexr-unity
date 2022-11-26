// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTransformTranslationSpace.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Transforms
{
    /// <summary>
    ///     The space in which we can translate an object. World will translate an object along the global X/Y/Z axes, Local
    ///     will translate it along the object's local X/Y/Z axes and Parent will translate it along its parent's local X/Y/Z
    ///     axes.
    ///     In case there is no parent, Parent mode will be the same as using World mode.
    /// </summary>
    public enum UxrTransformTranslationSpace
    {
        Local,
        World,
        Parent
    }
}