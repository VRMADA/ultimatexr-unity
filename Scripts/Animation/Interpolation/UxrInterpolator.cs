// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInterpolator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Animation.Interpolation
{
    /// <summary>
    ///     Provides functionality to interpolate between values using a wide range of interpolation modes.
    ///     This class also provides functionality to interpolate between 2 strings using a typewriter effect.
    /// </summary>
    public static class UxrInterpolator
    {
        #region Public Methods

        /// <summary>
        ///     Smooths a float value using the previous value, new value and a smooth value between [0.0, 1.0].
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <param name="smooth">Smooth value [0.0, 1.0] where 0.0 is no smoothing and 1.0 is maximum smoothing</param>
        /// <returns>Smoothed value</returns>
        public static float SmoothDamp(float oldValue, float newValue, float smooth)
        {
            return Mathf.Lerp(oldValue, newValue, GetSmoothInterpolationValue(smooth, Time.deltaTime));
        }

        /// <summary>
        ///     Smooths a position value using the last position, new position and a smooth value between [0.0, 1.0].
        /// </summary>
        /// <param name="oldPos">Old position</param>
        /// <param name="newPos">New position</param>
        /// <param name="smooth">Smooth value [0.0, 1.0] where 0.0 is no smoothing and 1.0 is maximum smoothing</param>
        /// <returns>Smoothed position value</returns>
        public static Vector3 SmoothDampPosition(Vector3 oldPos, Vector3 newPos, float smooth)
        {
            return Vector3.Lerp(oldPos, newPos, GetSmoothInterpolationValue(smooth, Time.deltaTime));
        }

        /// <summary>
        ///     Smooths a rotation value using the last rotation, new rotation and a smooth value between [0.0, 1.0].
        ///     This tries to do something similar to <see cref="Vector3.SmoothDamp" /> but for rotations.
        /// </summary>
        /// <param name="oldRot">Old rotation</param>
        /// <param name="newRot">New rotation</param>
        /// <param name="smooth">Smooth value [0.0, 1.0] where 0.0 is no smoothing and 1.0 is maximum smoothing</param>
        /// <returns>Smoothed rotation value</returns>
        public static Quaternion SmoothDampRotation(Quaternion oldRot, Quaternion newRot, float smooth)
        {
            return Quaternion.Slerp(oldRot, newRot, GetSmoothInterpolationValue(smooth, Time.deltaTime));
        }

        /// <summary>
        ///     Interpolates between two floating point values using a t between range [0.0, 1.0] and a given easing.
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor</param>
        /// <param name="easing">Easing</param>
        /// <returns>Interpolated value</returns>
        public static float Interpolate(float a, float b, float t, UxrEasing easing)
        {
            return Interpolate(a, b, 1.0f, 0.0f, Mathf.Clamp01(t), easing);
        }

        /// <summary>
        ///     Interpolates between two points using a t between range [0.0, 1.0] and a given easing.
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor</param>
        /// <param name="easing">Easing</param>
        /// <returns>Interpolated value</returns>
        public static Vector3 Interpolate(Vector3 a, Vector3 b, float t, UxrEasing easing)
        {
            return Interpolate(a, b, 1.0f, 0.0f, Mathf.Clamp01(t), easing);
        }

        /// <summary>
        ///     Spherically interpolates (SLERP) between two quaternions using a t between range [0.0, 1.0] and a given easing.
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor</param>
        /// <param name="easing">Easing</param>
        /// <returns>Interpolated value</returns>
        public static Quaternion Interpolate(Quaternion a, Quaternion b, float t, UxrEasing easing)
        {
            return Interpolate(a, b, 1.0f, 0.0f, Mathf.Clamp01(t), easing);
        }

        /// <summary>
        ///     Interpolates between two floating point values.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="duration">
        ///     The duration of the interpolation. If there is looping (loopMode != LoopMode.None) then it will
        ///     specify the duration of a single loop
        /// </param>
        /// <param name="delay">The delay duration before the interpolation starts</param>
        /// <param name="time">
        ///     The time value. This value will be clamped between [delay, delay + duration] or if there is looping
        ///     (loopMode != LoopMode.None) then it will be clamped between [delay, delay + loopedDuration]. In this case
        ///     duration will specify the duration of the loop
        /// </param>
        /// <param name="easing">The interpolation method to use. See @Easing</param>
        /// <param name="loopMode">Which looping mode to use. See @LoopMode</param>
        /// <param name="loopedDuration">
        ///     If loopMode is not LoopMode.None then loopedDuration will specify the duration of the
        ///     interpolation including all the loops. A negative value will make it loop forever.
        /// </param>
        /// <param name="delayUsingEndValue">
        ///     Tells whether to use the interpolation end value during the delay, if there is a <paramref name="delay" />
        ///     specified. By default it's false, which means the interpolation start value is used during the delay.
        /// </param>
        /// <returns>Interpolated floating point value</returns>
        public static float Interpolate(float       startValue,
                                        float       endValue,
                                        float       duration,
                                        float       delay,
                                        float       time,
                                        UxrEasing   easing,
                                        UxrLoopMode loopMode           = UxrLoopMode.None,
                                        float       loopedDuration     = -1.0f,
                                        bool        delayUsingEndValue = false)
        {
            return Interpolate(new Vector4(startValue, 0, 0, 0), new Vector4(endValue, 0, 0, 0), duration, delay, time, easing, loopMode, loopedDuration, delayUsingEndValue).x;
        }

        /// <summary>
        ///     Interpolates between two floating point values.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="time">The time value</param>
        /// <param name="settings">The interpolation settings to use</param>
        /// <returns>Interpolated floating point value</returns>
        /// <exception cref="ArgumentNullException">
        ///     When <paramref name="settings" /> is null.
        /// </exception>
        public static float Interpolate(float startValue, float endValue, float time, UxrInterpolationSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            return Interpolate(new Vector4(startValue, 0, 0, 0),
                               new Vector4(endValue,   0, 0, 0),
                               settings.DurationSeconds,
                               settings.DelaySeconds,
                               time,
                               settings.Easing,
                               settings.LoopMode,
                               settings.LoopedDurationSeconds,
                               settings.DelayUsingEndValue).x;
        }

        /// <summary>
        ///     Gets the T value used for linear interpolations like Vector3.Lerp or Quaternion.Slerp using easing and loop.
        /// </summary>
        /// <param name="t">Value between range [0.0f, 1.0f]</param>
        /// <param name="easing">The interpolation method to use.</param>
        /// <param name="loopMode">Which looping mode to use.</param>
        /// <param name="loopedDuration">
        ///     If loopMode is not LoopMode.None then loopedDuration will specify the duration of the
        ///     interpolation including all the loops. A negative value will make it loop forever.
        /// </param>
        /// <returns>The t value used to linearly interpolate using the specified parameters</returns>
        public static float GetInterpolationFactor(float t, UxrEasing easing, UxrLoopMode loopMode = UxrLoopMode.None, float loopedDuration = -1.0f)
        {
            return Interpolate(0.0f, 1.0f, 1.0f, 0.0f, t, easing, loopMode, loopedDuration);
        }

        /// <summary>
        ///     Interpolates between two <see cref="Vector4" /> values
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="duration">
        ///     The duration of the interpolation. If there is looping (loopMode != LoopMode.None) then it will
        ///     specify the duration of a single loop
        /// </param>
        /// <param name="delay">The delay duration before the interpolation starts</param>
        /// <param name="time">
        ///     The time value. This value will be clamped between [delay, delay + duration] or if there is looping
        ///     (loopMode != LoopMode.None) then it will be clamped between [delay, delay + loopedDuration]. In this case
        ///     duration will specify the duration of the loop
        /// </param>
        /// <param name="easing">The interpolation method to use. See @Easing</param>
        /// <param name="loopMode">Which looping mode to use. See @LoopMode</param>
        /// <param name="loopedDuration">
        ///     If loopMode is not LoopMode.None then loopedDuration will specify the duration of the
        ///     interpolation including all the loops. A negative value will make it loop forever.
        /// </param>
        /// <param name="delayUsingEndValue">
        ///     Tells whether to use the interpolation end value during the delay, if there is a <paramref name="delay" />
        ///     specified. By default it's false, which means the interpolation start value is used during the delay.
        /// </param>
        /// <returns>Interpolated <see cref="Vector4" /> value</returns>
        public static Vector4 Interpolate(Vector4     startValue,
                                          Vector4     endValue,
                                          float       duration,
                                          float       delay,
                                          float       time,
                                          UxrEasing   easing,
                                          UxrLoopMode loopMode           = UxrLoopMode.None,
                                          float       loopedDuration     = -1.0f,
                                          bool        delayUsingEndValue = false)
        {
            if (time < delay)
            {
                return delayUsingEndValue ? endValue : startValue;
            }

            // Compute interpolation t
            time = Mathf.Max(delay, time);

            if (!(loopMode != UxrLoopMode.None && loopedDuration < 0.0f))
            {
                Mathf.Min(time, delay + (loopMode == UxrLoopMode.None ? duration : loopedDuration));
            }

            float t = duration == 0.0f ? 0.0f : (time - delay) / duration;

            if (loopMode == UxrLoopMode.Loop)
            {
                t = t - (int)t;
            }
            else if (loopMode == UxrLoopMode.PingPong)
            {
                int loopCount = (int)t;

                t = t - loopCount;

                if ((loopCount & 1) == 1)
                {
                    // It's going back
                    t = 1.0f - t;
                }
            }

            return InterpolateVector4(startValue, endValue, t, easing);
        }

        /// <summary>
        ///     Interpolates between two <see cref="Vector4" /> values
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="time">The time value</param>
        /// <param name="settings">Interpolation settings to use</param>
        /// <returns>The interpolated <see cref="Vector4" /> value</returns>
        /// <exception cref="ArgumentNullException">
        ///     When <paramref name="settings" /> is null.
        /// </exception>
        public static Vector4 Interpolate(Vector4 startValue, Vector4 endValue, float time, UxrInterpolationSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            return Interpolate(startValue, endValue, settings.DurationSeconds, settings.DelaySeconds, time, settings.Easing, settings.LoopMode, settings.LoopedDurationSeconds, settings.DelayUsingEndValue);
        }

        /// <summary>
        ///     Interpolates between two <see cref="Quaternion" /> values. The interpolation uses SLERP.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="duration">
        ///     The duration of the interpolation. If there is looping (loopMode != LoopMode.None) then it will
        ///     specify the duration of a single loop
        /// </param>
        /// <param name="delay">The delay duration before the interpolation starts</param>
        /// <param name="time">
        ///     The time value. This value will be clamped between [delay, delay + duration] or if there is looping
        ///     (loopMode != LoopMode.None) then it will be clamped between [delay, delay + loopedDuration]. In this case
        ///     duration will specify the duration of the loop
        /// </param>
        /// <param name="easing">The interpolation method to use. See @Easing</param>
        /// <param name="loopMode">Which looping mode to use. See @LoopMode</param>
        /// <param name="loopedDuration">
        ///     If loopMode is not LoopMode.None then loopedDuration will specify the duration of the
        ///     interpolation including all the loops. A negative value will make it loop forever.
        /// </param>
        /// <param name="delayUsingEndValue">
        ///     Tells whether to use the interpolation end value during the delay, if there is a <paramref name="delay" />
        ///     specified. By default it's false, which means the interpolation start value is used during the delay.
        /// </param>
        /// <returns>Interpolated <see cref="Quaternion" /> value</returns>
        public static Quaternion Interpolate(Quaternion  startValue,
                                             Quaternion  endValue,
                                             float       duration,
                                             float       delay,
                                             float       time,
                                             UxrEasing   easing,
                                             UxrLoopMode loopMode           = UxrLoopMode.None,
                                             float       loopedDuration     = -1.0f,
                                             bool        delayUsingEndValue = false)
        {
            float t = Interpolate(0.0f, 1.0f, duration, delay, time, easing, loopMode, loopedDuration);
            return Quaternion.Slerp(startValue, endValue, t);
        }

        /// <summary>
        ///     Interpolates between two <see cref="Quaternion" /> values. The interpolation uses SLERP.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="time">The time value</param>
        /// <param name="settings">Interpolation settings to use</param>
        /// <returns>Interpolated <see cref="Quaternion" /> value</returns>
        /// <exception cref="ArgumentNullException">
        ///     When <paramref name="settings" /> is null.
        /// </exception>
        public static Quaternion Interpolate(Quaternion               startValue,
                                             Quaternion               endValue,
                                             float                    time,
                                             UxrInterpolationSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            return Interpolate(startValue, endValue, settings.DurationSeconds, settings.DelaySeconds, time, settings.Easing, settings.LoopMode, settings.LoopedDurationSeconds, settings.DelayUsingEndValue);
        }

        /// <summary>
        ///     Interpolates text using a typewriter effect.
        /// </summary>
        /// <param name="startText">Start text</param>
        /// <param name="endText">End text</param>
        /// <param name="t">Interpolation t between range [0.0, 1.0]</param>
        /// <param name="isForUnityTextUI">
        ///     If true, uses the rich text properties of the Unity UI text component to add invisible characters during
        ///     interpolation.
        ///     These invisible characters will help keeping the final text layout so that there are no line wraps or line jumps
        ///     during the interpolation.
        /// </param>
        /// <returns>Interpolated text</returns>
        /// <remarks>
        ///     See <see cref="InterpolateText(float,bool,string,object[])" /> to use a format string and arguments for more
        ///     advanced interpolation and numerical string interpolation.
        /// </remarks>
        public static string InterpolateText(string startText, string endText, float t, bool isForUnityTextUI)
        {
            return InterpolateText(t, isForUnityTextUI, "{0}", startText, endText);
        }

        /// <summary>
        ///     Interpolates text using a typewriter effect
        /// </summary>
        /// <param name="t">Interpolation t between range [0.0, 1.0]</param>
        /// <param name="isForUnityTextUI">
        ///     If true, uses the rich text properties of the Unity UI text component to add invisible characters during
        ///     interpolation.
        ///     These invisible characters will help keeping the final text layout so that there are no line wraps or line jumps
        ///     during the interpolation.
        /// </param>
        /// <param name="formatString">
        ///     The format string (what would be the first parameter of <see cref="string.Format(string,object[])" />)
        /// </param>
        /// <param name="formatStringArgs">
        ///     <para>
        ///         Start/end pairs that will be interpolated and fed into <see cref="string.Format(string,object[])" />.
        ///         These should be sequential pairs of values of the same type that represent the start value and the end value.
        ///         For instance format could be "{0}:{1}" and args could be startArg0, endArg0, startArg1, endArg1.
        ///         This will print 2 interpolated values (Arg0 and Arg1) whose start and end values are defined by the other 4
        ///         parameters.
        ///     </para>
        ///     <para>
        ///         The interpolation can detect numerical values (int/float) and use numerical interpolation instead of raw string
        ///         interpolation. This can be useful for effects as seen in the examples.
        ///     </para>
        /// </param>
        /// <example>
        ///     Simple typewriter effect to write a sentence starting from empty: (t goes from 0.0 to 1.0)
        ///     <code>
        ///     InterpolateText(t, true, "{0}", string.Empty, "Welcome to the Matrix!");
        /// </code>
        /// </example>
        /// <example>
        ///     Using format string args to create an increasing score animation. The numerical values are interpolated instead of
        ///     using a typewriter effect. (t goes from 0.0 to 1.0).
        ///     <code>
        ///     int finalScore = 999999;
        ///     InterpolateText(t, true, "Final score: {0:000000}", 0, finalScore);
        /// </code>
        /// </example>
        /// <returns>Interpolated string</returns>
        public static string InterpolateText(float t, bool isForUnityTextUI, string formatString, params object[] formatStringArgs)
        {
#if UNITY_EDITOR
            if (!(formatStringArgs.Length > 0 && formatStringArgs.Length % 2 == 0))
            {
                Debug.LogError("The text has no arguments or the number of arguments is not even");
                return string.Empty;
            }
#endif
            int numArgs = formatStringArgs.Length / 2;

            object[] finalArgs = new object[numArgs];

            for (int i = 0; i < numArgs; i++)
            {
                if (formatStringArgs[i] == null)
                {
                    Debug.LogError("Argument " + i + " is null");
                    return formatStringArgs[i + numArgs] != null ? formatStringArgs[i + numArgs].ToString() : string.Empty;
                }

                if (formatStringArgs[i + numArgs] == null)
                {
                    Debug.LogError("Argument " + (i + numArgs) + " is null");
                    return formatStringArgs[i] != null ? formatStringArgs[i].ToString() : string.Empty;
                }

#if UNITY_EDITOR
                if (!(formatStringArgs[i].GetType() == formatStringArgs[i + numArgs].GetType()))
                {
                    Debug.LogError("Type of argument " + i + " is not the same as argument " + (i + numArgs));
                    return string.Empty;
                }
#endif
                if (formatStringArgs[i] is int)
                {
                    finalArgs[i] = Mathf.RoundToInt(Mathf.Lerp((int)formatStringArgs[i], (int)formatStringArgs[i + numArgs], Mathf.Clamp01(t)));
                }
                else if (formatStringArgs[i] is float)
                {
                    finalArgs[i] = Mathf.Lerp((float)formatStringArgs[i], (float)formatStringArgs[i + numArgs], Mathf.Clamp01(t));
                }
                else if (formatStringArgs[i] is string)
                {
                    string a          = (string)formatStringArgs[i];
                    string b          = (string)formatStringArgs[i + 1];
                    int    startChars = a.Length;
                    int    endChars   = b.Length;

                    int numChars = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(startChars, endChars, t)), 0, b.Length);

                    if (Mathf.Approximately(t, 1.0f))
                    {
                        finalArgs[i] = b;
                    }
                    else if (numChars > 0)
                    {
                        float letterT       = Mathf.Clamp01(Mathf.Repeat(t, 1.0f / endChars) * endChars);
                        int   changingIndex = Mathf.Max(0, numChars - 1);
                        char  startCar      = char.IsUpper(b[changingIndex]) ? 'A' : 'a';
                        char  endCar        = char.IsUpper(b[changingIndex]) ? 'Z' : 'z';

                        finalArgs[i] = b.Substring(0, Mathf.Max(0, numChars - 1)) + (char)(startCar + letterT * (endCar - startCar));

                        if (isForUnityTextUI)
                        {
                            // Add the remaining characters as "invisible" to avoid word wrapping effects during interpolation.
                            finalArgs[i] += @"<color=#00000000>" + (endChars > startChars ? b.Substring(numChars, endChars - numChars) : string.Empty) + @"</color>";
                        }
                    }
                    else
                    {
                        finalArgs[i] = string.Empty;
                    }
                }
            }

            return string.Format(formatString, finalArgs);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets a framerate-independent smoothed interpolation value.
        /// </summary>
        /// <param name="smooth">Smooth value [0.0, 1.0] with 0 meaning no smoothing and 1 maximum smoothing</param>
        /// <param name="deltaTime">Elapsed time in seconds</param>
        /// <returns>Interpolation value [0.0, 1.0]</returns>
        private static float GetSmoothInterpolationValue(float smooth, float deltaTime)
        {
            return smooth > 0.0f ? (1.0f - Mathf.Clamp01(smooth)) * deltaTime * MaxSmoothSpeed : 1.0f;
        }

        /// <summary>
        ///     Evaluates a curve using interpolation. This is the core math code that does the actual interpolation.
        /// </summary>
        /// <param name="start">Initial value</param>
        /// <param name="end">End value</param>
        /// <param name="t">Interpolation t value (range [0.0f, 1.0f])</param>
        /// <param name="easing">Interpolation type</param>
        /// <returns>Interpolated value</returns>
        private static Vector4 InterpolateVector4(Vector4 start, Vector4 end, float t, UxrEasing easing)
        {
            Vector4 change = end - start;

            switch (easing)
            {
                ///////////////////////////////////////// LINEAR ////////////////////////////////////////////////////

                case UxrEasing.Linear: return start + change * t;

                ///////////////////////////////////////// SINE //////////////////////////////////////////////////////

                case UxrEasing.EaseInSine:                  return -change * Mathf.Cos(t * (Mathf.PI / 2.0f)) + change + start;
                case UxrEasing.EaseOutSine:                 return change * Mathf.Sin(t * (Mathf.PI / 2.0f)) + start;
                case UxrEasing.EaseInOutSine:               return -change / 2.0f * (Mathf.Cos(Mathf.PI * t) - 1.0f) + start;
                case UxrEasing.EaseOutInSine when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutSine);
                case UxrEasing.EaseOutInSine:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInSine);

                ///////////////////////////////////////// QUAD //////////////////////////////////////////////////////

                case UxrEasing.EaseInQuad:  return start + t * t * change;
                case UxrEasing.EaseOutQuad: return (t - 2.0f) * t * -change + start;

                case UxrEasing.EaseInOutQuad:
                {
                    t *= 2.0f;

                    if (t < 1)
                    {
                        return change / 2.0f * t * t + start;
                    }

                    t -= 1.0f;
                    return -change / 2.0f * (t * (t - 2.0f) - 1.0f) + start;
                }

                case UxrEasing.EaseOutInQuad when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutQuad);
                case UxrEasing.EaseOutInQuad:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInQuad);

                ///////////////////////////////////////// CUBIC /////////////////////////////////////////////////////

                case UxrEasing.EaseInCubic: return start + t * t * t * change;

                case UxrEasing.EaseOutCubic:
                    t -= 1.0f;
                    return change * (t * t * t + 1.0f) + start;

                case UxrEasing.EaseInOutCubic:
                {
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        return change / 2.0f * t * t * t + start;
                    }

                    t -= 2.0f;
                    return change / 2.0f * (t * t * t + 2.0f) + start;
                }

                case UxrEasing.EaseOutInCubic when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutCubic);
                case UxrEasing.EaseOutInCubic:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInCubic);

                ///////////////////////////////////////// QUART /////////////////////////////////////////////////////

                case UxrEasing.EaseInQuart: return start + t * t * t * t * change;

                case UxrEasing.EaseOutQuart:
                    t -= 1.0f;
                    return -change * (t * t * t * t - 1.0f) + start;

                case UxrEasing.EaseInOutQuart:
                {
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        return change / 2.0f * t * t * t * t + start;
                    }

                    t -= 2.0f;
                    return -change / 2.0f * (t * t * t * t - 2.0f) + start;
                }

                case UxrEasing.EaseOutInQuart when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutQuart);
                case UxrEasing.EaseOutInQuart:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInQuart);

                ///////////////////////////////////////// QUINT /////////////////////////////////////////////////////

                case UxrEasing.EaseInQuint: return start + t * t * t * t * t * change;

                case UxrEasing.EaseOutQuint:
                    t -= 1.0f;
                    return change * (t * t * t * t * t + 1.0f) + start;

                case UxrEasing.EaseInOutQuint:
                {
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        return change / 2.0f * t * t * t * t * t + start;
                    }

                    t -= 2.0f;
                    return change / 2.0f * (t * t * t * t * t + 2.0f) + start;
                }

                case UxrEasing.EaseOutInQuint when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutQuint);
                case UxrEasing.EaseOutInQuint:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInQuint);

                ///////////////////////////////////////// EXPO //////////////////////////////////////////////////////

                case UxrEasing.EaseInExpo:  return change * Mathf.Pow(2.0f, 10.0f * (t - 1.0f)) + start;
                case UxrEasing.EaseOutExpo: return change * (-Mathf.Pow(2.0f, -10.0f * t) + 1.0f) + start;

                case UxrEasing.EaseInOutExpo:
                {
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        return change / 2.0f * Mathf.Pow(2.0f, 10.0f * (t - 1.0f)) + start - change * 0.0005f;
                    }

                    t -= 1.0f;
                    return change / 2.0f * 1.0005f * (-Mathf.Pow(2.0f, -10.0f * t) + 2.0f) + start;
                }

                case UxrEasing.EaseOutInExpo when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutExpo);
                case UxrEasing.EaseOutInExpo:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInExpo);

                ///////////////////////////////////////// CIRC //////////////////////////////////////////////////////

                case UxrEasing.EaseInCirc: return -change * (Mathf.Sqrt(1.0f - t * t) - 1.0f) + start;

                case UxrEasing.EaseOutCirc:
                    t -= 1.0f;
                    return change * Mathf.Sqrt(1.0f - t * t) + start;

                case UxrEasing.EaseInOutCirc:
                {
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        return -change / 2.0f * (Mathf.Sqrt(1.0f - t * t) - 1.0f) + start;
                    }

                    t -= 2.0f;
                    return change / 2.0f * (Mathf.Sqrt(1.0f - t * t) + 1.0f) + start;
                }

                case UxrEasing.EaseOutInCirc when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutCirc);
                case UxrEasing.EaseOutInCirc:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInCirc);

                ///////////////////////////////////////// BACK //////////////////////////////////////////////////////

                case UxrEasing.EaseInBack:
                {
                    float s = 1.70158f;
                    return change * (t * t * (s + 1.0f) * t - s) + start;
                }

                case UxrEasing.EaseOutBack:
                {
                    float s = 1.70158f;
                    return change * ((t - 1.0f) * t * ((s + 1.0f) * t + s) + 1.0f) + start;
                }

                case UxrEasing.EaseInOutBack:
                {
                    float s = 1.70158f;
                    t *= 2.0f;

                    if (t < 1.0f)
                    {
                        s *= 1.525f;
                        return change / 2.0f * (t * t * ((s + 1.0f) * t - s)) + start;
                    }

                    s *= 1.525f;
                    t -= 2.0f;
                    return change / 2.0f * (t * t * ((s + 1.0f) * t + s) + 2.0f) + start;
                }

                case UxrEasing.EaseOutInBack when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutBack);
                case UxrEasing.EaseOutInBack:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInBack);

                ///////////////////////////////////////// ELASTIC ///////////////////////////////////////////////////

                case UxrEasing.EaseInElastic:
                {
                    float   p = 0.3f;
                    Vector4 a = change;
                    float   s = p / 4.0f;

                    t -= 1.0f;

                    return -(Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) * a) + start;
                }

                case UxrEasing.EaseOutElastic:
                {
                    float   p = 0.3f;
                    Vector4 a = change;
                    float   s = p / 4.0f;
                    return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) * a + change + start;
                }

                case UxrEasing.EaseInOutElastic:
                {
                    t *= 2.0f;
                    float   p = 0.3f * 1.5f;
                    Vector4 a = change;
                    float   s = p / 4.0f;

                    if (t < 1.0f)
                    {
                        t -= 1.0f;
                        return -0.5f * (Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) * a) + start;
                    }

                    t -= 1.0f;
                    return 0.5f * Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) * a + change + start;
                }

                case UxrEasing.EaseOutInElastic when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutElastic);
                case UxrEasing.EaseOutInElastic:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInElastic);

                ///////////////////////////////////////// BOUNCE ////////////////////////////////////////////////////

                case UxrEasing.EaseInBounce:                        return change - InterpolateVector4(Vector4.zero, change, 1.0f - t, UxrEasing.EaseOutBounce) + start;
                case UxrEasing.EaseOutBounce when t < 1.0f / 2.75f: return change * (7.5625f * t * t) + start;

                case UxrEasing.EaseOutBounce when t < 2.0f / 2.75f:
                    t -= 1.5f / 2.75f;
                    return change * (7.5625f * t * t + 0.75f) + start;

                case UxrEasing.EaseOutBounce when t < 2.5 / 2.75:
                    t -= 2.25f / 2.75f;
                    return change * (7.5625f * t * t + 0.9375f) + start;

                case UxrEasing.EaseOutBounce:
                    t -= 2.625f / 2.75f;
                    return change * (7.5625f * t * t + 0.984375f) + start;

                case UxrEasing.EaseInOutBounce when t < 0.5f: return InterpolateVector4(Vector4.zero, change, t * 2.0f,          UxrEasing.EaseInBounce) * 0.5f + start;
                case UxrEasing.EaseInOutBounce:               return InterpolateVector4(Vector4.zero, change, (t - 0.5f) * 2.0f, UxrEasing.EaseOutBounce) * 0.5f + change * 0.5f + start;
                case UxrEasing.EaseOutInBounce when t < 0.5f: return InterpolateVector4(start,                 start + change * 0.5f, t * 2.0f,          UxrEasing.EaseOutBounce);
                case UxrEasing.EaseOutInBounce:               return InterpolateVector4(start + change * 0.5f, end,                   (t - 0.5f) * 2.0f, UxrEasing.EaseInBounce);

                default:
#if UNITY_EDITOR
                    Debug.LogError($"{nameof(UxrInterpolator)} Unknown easing mode");
#endif
                    return Vector4.zero;
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Constant used in SmoothDamp methods that controls the speed at which the interpolation will be performed.
        /// </summary>
        private const float MaxSmoothSpeed = 20.0f;

        #endregion
    }
}