// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonoBehaviourExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using UltimateXR.Animation.Interpolation;
using UnityEngine;

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     <see cref="MonoBehaviour" /> extensions.
    /// </summary>
    public static class MonoBehaviourExt
    {
        #region Public Methods

        /// <summary>
        ///     Enables/disabled the component if it isn't enabled already.
        /// </summary>
        /// <param name="self">Component to enable/disable</param>
        /// <param name="enable">Whether to enable or disable the component</param>
        public static void CheckSetEnabled(this MonoBehaviour self, bool enable)
        {
            if (self.enabled != enable)
            {
                self.enabled = enable;
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Creates a coroutine that simplifies executing a loop during a certain amount of time.
        /// </summary>
        /// <param name="monoBehaviour">Caller</param>
        /// <param name="durationSeconds">Time in seconds of the interpolation</param>
        /// <param name="loopAction">
        ///     The action to perform on each loop step. The action receives
        ///     the interpolation value t [0.0, 1.0] as parameter.
        /// </param>
        /// <param name="easing">Easing to use in the interpolation (linear by default)</param>
        /// <param name="forceLastT1">Forces a last loop step with t = 1.0f exactly.</param>
        /// <returns>Coroutine enumerator</returns>
        public static IEnumerator LoopCoroutine(this MonoBehaviour monoBehaviour,
                                                float              durationSeconds,
                                                Action<float>      loopAction,
                                                UxrEasing          easing      = UxrEasing.Linear,
                                                bool               forceLastT1 = false)
        {
            float startTime = Time.time;

            while (Time.time - startTime < durationSeconds)
            {
                float t = UxrInterpolator.Interpolate(0.0f, 1.0f, Time.time - startTime, new UxrInterpolationSettings(durationSeconds, 0.0f, easing));
                loopAction(t);
                yield return null;
            }

            if (forceLastT1)
            {
                loopAction(1.0f);
            }
        }

        #endregion
    }
}