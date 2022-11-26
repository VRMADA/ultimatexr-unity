// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Readme.Section.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Editor.Readme
{
    public partial class UxrReadme
    {
        [Serializable]
        public class Section
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private string _heading;
            [SerializeField] private string _text;
            [SerializeField] private string _linkText;
            [SerializeField] private string _url;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the heading.
            /// </summary>
            public string Heading => _heading;

            /// <summary>
            ///     Gets the main text.
            /// </summary>
            public string Text => _text;

            /// <summary>
            ///     Gets the link text.
            /// </summary>
            public string LinkText => _linkText;

            /// <summary>
            ///     Gets the link URL.
            /// </summary>
            public string URL => _url;

            #endregion
        }
    }
}