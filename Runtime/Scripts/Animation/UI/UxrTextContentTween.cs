// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTextContentTween.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;
#if ULTIMATEXR_UNITY_TMPRO
using TMPro;
#endif

namespace UltimateXR.Animation.UI
{
    /// <summary>
    ///     Tweening component to animate a UI text component programatically or using the inspector. Supports both Unity's
    ///     Text and TMPro.
    ///     The text interpolation can be used to create a typewriter kind of effect.
    ///     Programatically it also offers the possibility to interpolate parameters in a text string.
    /// </summary>
    [DisallowMultipleComponent]
    public class UxrTextContentTween : UxrTween
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private string _startText;
        [SerializeField] private string _endText;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="UnityEngine.UI.Text" /> component whose string will be interpolated.
        /// </summary>
        public Text TargetText => GetCachedComponent<Text>();

#if ULTIMATEXR_UNITY_TMPRO

        /// <summary>
        ///     Gets the <see cref="TextMeshProUGUI" /> component whose string will be interpolated.
        /// </summary>
        public TextMeshProUGUI TargetTextTMPro => GetCachedComponent<TextMeshProUGUI>();

#endif

        /// <summary>
        ///     Gets or sets the text value.
        /// </summary>
        public string Text
        {
            get
            {
                if (TargetText != null)
                {
                    return TargetText.text;
                }

#if ULTIMATEXR_UNITY_TMPRO

                if (TargetTextTMPro != null)
                {
                    return TargetTextTMPro.text;
                }

#endif
                return null;
            }

            set
            {
                if (TargetText != null)
                {
                    TargetText.text = value;
                }

#if ULTIMATEXR_UNITY_TMPRO

                if (TargetTextTMPro != null)
                {
                    TargetTextTMPro.text = value;
                }

#endif
            }
        }

        /// <summary>
        ///     Gets whether the interpolation uses format string parameters.
        ///     <list type="bullet">
        ///         <item>
        ///             false: Interpolation will be a plain typewriter effect from <see cref="StartText" /> to
        ///             <see cref="EndText" />
        ///         </item>
        ///         <item>
        ///             true: Interpolation will use <see cref="FormatString" /> and <see cref="FormatStringArgs" />. For more
        ///             information on how these are used see
        ///             <see cref="UxrInterpolator.InterpolateText(float,bool,string,object[])">UxrInterpolator.InterpolateText</see>
        ///         </item>
        ///     </list>
        /// </summary>
        public bool UsesFormatString { get; private set; }

        /// <summary>
        ///     Animation start text
        /// </summary>
        public string StartText
        {
            get => _startText;
            set => _startText = value;
        }

        /// <summary>
        ///     Animation end text
        /// </summary>
        public string EndText
        {
            get => _endText;
            set => _endText = value;
        }

        /// <summary>
        ///     Animation format string, when <see cref="UsesFormatString" /> is true.
        /// </summary>
        public string FormatString { get; set; }

        /// <summary>
        ///     Animation format string parameter list, when <see cref="UsesFormatString" /> is true.
        /// </summary>
        public List<object> FormatStringArgs { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates and starts a tweening animation for a Unity UI Text component or TMPro text component.
        /// </summary>
        /// <param name="gameObject">Target GameObject with either a Unity UI Text component or Text Mesh Pro text component</param>
        /// <param name="startText">Start text</param>
        /// <param name="endText">End text</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished</param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrTextContentTween Animate(GameObject gameObject, string startText, string endText, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback = null)
        {
            UxrTextContentTween textContentTween = gameObject.GetOrAddComponent<UxrTextContentTween>();

            textContentTween.UsesFormatString      = false;
            textContentTween.StartText             = startText;
            textContentTween.EndText               = endText;
            textContentTween.InterpolationSettings = settings;
            textContentTween.FinishedCallback      = finishedCallback;
            textContentTween.Restart();

            return textContentTween;
        }

        /// <summary>
        ///     Creates and starts a tweening animation for a Unity UI Text component or TMPro text component. See
        ///     <see cref="UxrInterpolator.InterpolateText(float,bool,string,object[])">UxrInterpolator.InterpolateText</see> for
        ///     information on how <paramref name="formatString" /> and <paramref name="formatStringArgs" /> work.
        /// </summary>
        /// <param name="gameObject">Target GameObject with either a Unity UI Text component or Text Mesh Pro text component</param>
        /// <param name="settings">Interpolation settings that control the animation</param>
        /// <param name="finishedCallback">Optional callback when the animation finished. Use null to ignore.</param>
        /// <param name="formatString">
        ///     Format string. See
        ///     <see cref="UxrInterpolator.InterpolateText(float,bool,string,object[])">UxrInterpolator.InterpolateText</see>
        /// </param>
        /// <param name="formatStringArgs">
        ///     Format string arguments. See
        ///     <see cref="UxrInterpolator.InterpolateText(float,bool,string,object[])">UxrInterpolator.InterpolateText</see>
        /// </param>
        /// <returns>
        ///     Tweening component that will update itself automatically. Can be used to stop the animation prematurely or
        ///     change parameters on the fly.
        /// </returns>
        public static UxrTextContentTween Animate(GameObject gameObject, UxrInterpolationSettings settings, Action<UxrTween> finishedCallback, string formatString, object[] formatStringArgs)
        {
            UxrTextContentTween textContentTween = gameObject.GetOrAddComponent<UxrTextContentTween>();

            textContentTween.UsesFormatString      = true;
            textContentTween.InterpolationSettings = settings;
            textContentTween.FinishedCallback      = finishedCallback;
            textContentTween.FormatString          = formatString;
            textContentTween.FormatStringArgs      = formatStringArgs.ToList();
            textContentTween.Restart();

            return textContentTween;
        }

        #endregion

        #region Protected Overrides UxrTween

        /// <inheritdoc />
        protected override Behaviour TargetBehaviour
        {
            get
            {
                if (TargetText != null)
                {
                    return TargetText;
                }

#if ULTIMATEXR_UNITY_TMPRO

                if (TargetTextTMPro != null)
                {
                    return TargetTextTMPro;
                }

#endif

                return null;
            }
        }

        /// <inheritdoc />
        protected override void StoreOriginalValue()
        {
            _originalText = Text;
        }

        /// <inheritdoc />
        protected override void RestoreOriginalValue()
        {
            if (HasOriginalValueStored)
            {
                Text = _originalText;
            }
        }

        /// <inheritdoc />
        protected override void Interpolate(float t)
        {
            if (StartText == null || EndText == null)
            {
                return;
            }

            if (!UsesFormatString)
            {
                Text = UxrInterpolator.InterpolateText(StartText, EndText, t, true);
            }
            else
            {
                Text = UxrInterpolator.InterpolateText(t, true, FormatString, FormatStringArgs);
            }
        }

        #endregion

        #region Private Types & Data

        private string _originalText;

        #endregion
    }
}