// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEasing.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Lights;
using UltimateXR.Animation.Materials;
using UltimateXR.Animation.Transforms;
using UltimateXR.Animation.UI;
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     <para>
    ///         Type of interpolation curves.
    ///     </para>
    ///     <para>
    ///         References:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>http://easings.net</item>
    ///         <item>http://gillcup.readthedocs.org/en/latest/_images/easings.png</item>
    ///     </list>
    ///     Examples of some classes that use interpolation:
    ///     <list type="bullet">
    ///         <item><see cref="UxrInterpolator" />: Access to interpolation calculations</item>
    ///         <item><see cref="UxrTween" /> and all derived classes (UI tweening)</item>
    ///         <item><see cref="UxrAnimatedTransform" /> (<see cref="Transform" /> animation)</item>
    ///         <item><see cref="UxrAnimatedLightIntensity" /> (<see cref="Light" /> intensity parameter animation)</item>
    ///         <item><see cref="UxrAnimatedMaterial" /> (<see cref="Material" /> parameter animation)</item>
    ///     </list>
    /// </summary>
    public enum UxrEasing
    {
        Linear,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseOutInSine,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseOutInQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutInCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseOutInQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseOutInQuint,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseOutInExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseOutInCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseOutInBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseOutInElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseOutInBounce
    }
}