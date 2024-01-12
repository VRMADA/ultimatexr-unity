// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRightToLeftSupport.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ULTIMATEXR_UNITY_TMPRO
using TMPro;
#endif

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     <para>
    ///         Component that, added to a UI element, will enable support for left-to-right and right-to-left languages.
    ///         Right-to-left languages not only work by setting text alignment to right, they also require the whole layout to
    ///         be right-to-left.
    ///     </para>
    ///     <para>
    ///         To switch from one to another use the static property <see cref="UseRightToLeft" />.
    ///     </para>
    ///     <para>
    ///         The supported UI components are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item><see cref="Text" /> (Unity UI)</item>
    ///         <item>Text UI (TextMeshPro)</item>
    ///         <item><see cref="HorizontalLayoutGroup" /> (Unity UI)</item>
    ///         <item><see cref="VerticalLayoutGroup" /> (Unity UI)</item>
    ///         <item><see cref="GridLayoutGroup" /> (Unity UI)</item>
    ///         <item><see cref="Image" /> fill origin (Unity UI)</item>
    ///     </list>
    /// </summary>
    public class UxrRightToLeftSupport : UxrComponent<UxrRightToLeftSupport>
    {
        #region Public Types & Data

        /// <summary>
        ///     Sets the global right-to-left setting, changing all <see cref="UxrRightToLeftSupport" /> components.
        ///     Disabled components, or newly instantiated components, will be aligned correctly too.
        /// </summary>
        public static bool UseRightToLeft
        {
            get => s_useRightToLeft;
            set
            {
                s_useRightToLeft = value;
                EnabledComponents.ForEach(c => c.SetRightToLeft(value));
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CheckInitialize();
        }

        /// <summary>
        ///     Sets the RtoL setting when the component is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            SetRightToLeft(s_useRightToLeft);
        }

        #endregion

        #region Private Methods

#if ULTIMATEXR_UNITY_TMPRO
        /// <summary>
        ///     Transforms a TMPro alignment value from LtoR to RtoL.
        /// </summary>
        /// <param name="alignment">Alignment to transform</param>
        /// <returns>RtoL value</returns>
        private static TextAlignmentOptions GetRtoLAlignmentTMPro(TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.TopRight:      return TextAlignmentOptions.TopLeft;
                case TextAlignmentOptions.Right:         return TextAlignmentOptions.Left;
                case TextAlignmentOptions.BottomRight:   return TextAlignmentOptions.BottomLeft;
                case TextAlignmentOptions.BaselineRight: return TextAlignmentOptions.BaselineLeft;
                case TextAlignmentOptions.MidlineRight:  return TextAlignmentOptions.MidlineLeft;
                case TextAlignmentOptions.CaplineRight:  return TextAlignmentOptions.CaplineLeft;
                case TextAlignmentOptions.TopLeft:       return TextAlignmentOptions.TopRight;
                case TextAlignmentOptions.Left:          return TextAlignmentOptions.Right;
                case TextAlignmentOptions.BottomLeft:    return TextAlignmentOptions.BottomRight;
                case TextAlignmentOptions.BaselineLeft:  return TextAlignmentOptions.BaselineRight;
                case TextAlignmentOptions.MidlineLeft:   return TextAlignmentOptions.MidlineRight;
                case TextAlignmentOptions.CaplineLeft:   return TextAlignmentOptions.CaplineRight;
            }

            return alignment;
        }
#endif

        /// <summary>
        ///     Transforms a Unity UI alignment value from LtoR to RtoL.
        /// </summary>
        /// <param name="alignment">Alignment to transform</param>
        /// <returns>RtoL value</returns>
        private static TextAnchor GetRtoLAlignment(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft:   return TextAnchor.UpperRight;
                case TextAnchor.UpperRight:  return TextAnchor.UpperLeft;
                case TextAnchor.MiddleLeft:  return TextAnchor.MiddleRight;
                case TextAnchor.MiddleRight: return TextAnchor.MiddleLeft;
                case TextAnchor.LowerLeft:   return TextAnchor.LowerRight;
                case TextAnchor.LowerRight:  return TextAnchor.LowerLeft;
            }

            return alignment;
        }

        /// <summary>
        ///     Transforms a Unity UI corner value from LtoR to RtoL.
        /// </summary>
        /// <param name="corner">Corner to transform</param>
        /// <returns>RtoL value</returns>
        private static GridLayoutGroup.Corner GetRtoLCorner(GridLayoutGroup.Corner corner)
        {
            switch (corner)
            {
                case GridLayoutGroup.Corner.UpperLeft:  return GridLayoutGroup.Corner.UpperRight;
                case GridLayoutGroup.Corner.UpperRight: return GridLayoutGroup.Corner.UpperLeft;
                case GridLayoutGroup.Corner.LowerLeft:  return GridLayoutGroup.Corner.LowerRight;
                case GridLayoutGroup.Corner.LowerRight: return GridLayoutGroup.Corner.LowerLeft;
            }

            return corner;
        }

        /// <summary>
        ///     Transforms a Unity UI fill origin value from LtoR to RtoL.
        /// </summary>
        /// <param name="fillMethod">Fill method used</param>
        /// <param name="fillOrigin">fill origin to transform</param>
        /// <returns>RtoL value</returns>
        private static int GetRtoLFillOrigin(Image.FillMethod fillMethod, int fillOrigin)
        {
            if (fillMethod == Image.FillMethod.Horizontal)
            {
                switch (fillOrigin)
                {
                    case (int)Image.OriginHorizontal.Left:  return (int)Image.OriginHorizontal.Right;
                    case (int)Image.OriginHorizontal.Right: return (int)Image.OriginHorizontal.Left;
                }
            }
            else if (fillMethod == Image.FillMethod.Radial90)
            {
                switch (fillOrigin)
                {
                    case (int)Image.Origin90.BottomLeft:  return (int)Image.Origin90.BottomRight;
                    case (int)Image.Origin90.BottomRight: return (int)Image.Origin90.BottomLeft;
                    case (int)Image.Origin90.TopLeft:     return (int)Image.Origin90.TopRight;
                    case (int)Image.Origin90.TopRight:    return (int)Image.Origin90.TopLeft;
                }
            }
            else if (fillMethod == Image.FillMethod.Radial180)
            {
                switch (fillOrigin)
                {
                    case (int)Image.Origin180.Left:  return (int)Image.Origin180.Right;
                    case (int)Image.Origin180.Right: return (int)Image.Origin180.Left;
                }
            }
            else if (fillMethod == Image.FillMethod.Radial360)
            {
                switch (fillOrigin)
                {
                    case (int)Image.Origin360.Left:  return (int)Image.Origin360.Right;
                    case (int)Image.Origin360.Right: return (int)Image.Origin360.Left;
                }
            }

            return fillOrigin;
        }

        /// <summary>
        ///     Gets the component references and stores the initial values.
        /// </summary>
        private void CheckInitialize()
        {
            if (_initialized)
            {
                return;
            }
            
#if ULTIMATEXR_UNITY_TMPRO
            _textTMPro = GetComponent<TextMeshProUGUI>();

            if (_textTMPro != null)
            {
                _alignmentTMPro = _textTMPro.alignment;
            }
#endif
            _text = GetComponent<Text>();

            if (_text != null)
            {
                _textAlignment = _text.alignment;
            }

            _horizontalLayout = GetComponent<HorizontalLayoutGroup>();

            if (_horizontalLayout)
            {
                _horLayoutAlignment = _horizontalLayout.childAlignment;
                _horLayoutReversed  = _horizontalLayout.reverseArrangement;
            }

            _verticalLayout = GetComponent<VerticalLayoutGroup>();

            if (_verticalLayout)
            {
                _verLayoutAlignment = _verticalLayout.childAlignment;
            }

            _gridLayout = GetComponent<GridLayoutGroup>();

            if (_gridLayout)
            {
                _gridStartCorner    = _gridLayout.startCorner;
                _gridChildAlignment = _gridLayout.childAlignment;
            }

            _fillImage = GetComponent<Image>();

            if (_fillImage != null && _fillImage.type == Image.Type.Filled)
            {
                _fillOrigin = _fillImage.fillOrigin;
            }
            else
            {
                _fillImage = null;
            }

            _initialized = true;
        }

        /// <summary>
        ///     Switches this component to LtoR or RtoL.
        /// </summary>
        /// <param name="useRightToLeft">Whether to use RtoL (true) or LtoR (false)</param>
        private void SetRightToLeft(bool useRightToLeft)
        {
            CheckInitialize();

#if ULTIMATEXR_UNITY_TMPRO
            if (_textTMPro != null)
            {
                _textTMPro.alignment = useRightToLeft ? GetRtoLAlignmentTMPro(_alignmentTMPro) : _alignmentTMPro;
            }
#endif
            if (_text != null)
            {
                _text.alignment = useRightToLeft ? GetRtoLAlignment(_textAlignment) : _textAlignment;
            }

            if (_horizontalLayout != null)
            {
                _horizontalLayout.childAlignment     = useRightToLeft ? GetRtoLAlignment(_horLayoutAlignment) : _horLayoutAlignment;
                _horizontalLayout.reverseArrangement = useRightToLeft ? !_horLayoutReversed : _horLayoutReversed;
            }

            if (_verticalLayout != null)
            {
                _verticalLayout.childAlignment = useRightToLeft ? GetRtoLAlignment(_verLayoutAlignment) : _verLayoutAlignment;
            }

            if (_gridLayout != null)
            {
                _gridLayout.startCorner    = useRightToLeft ? GetRtoLCorner(_gridStartCorner) : _gridStartCorner;
                _gridLayout.childAlignment = useRightToLeft ? GetRtoLAlignment(_gridChildAlignment) : _gridChildAlignment;
            }

            if (_fillImage != null)
            {
                _fillImage.fillOrigin = useRightToLeft ? GetRtoLFillOrigin(_fillImage.fillMethod, _fillOrigin) : _fillOrigin;
            }
        }

        #endregion

        #region Private Types & Data

        private static bool s_useRightToLeft;

        private bool                   _initialized;
        private Text                   _text;
        private TextAnchor             _textAlignment;
        private HorizontalLayoutGroup  _horizontalLayout;
        private TextAnchor             _horLayoutAlignment;
        private bool                   _horLayoutReversed;
        private VerticalLayoutGroup    _verticalLayout;
        private TextAnchor             _verLayoutAlignment;
        private GridLayoutGroup        _gridLayout;
        private GridLayoutGroup.Corner _gridStartCorner;
        private TextAnchor             _gridChildAlignment;
        private Image                  _fillImage;
        private int                    _fillOrigin;

#if ULTIMATEXR_UNITY_TMPRO
        private TextMeshProUGUI      _textTMPro;
        private TextAlignmentOptions _alignmentTMPro;
#endif

        #endregion
    }
}