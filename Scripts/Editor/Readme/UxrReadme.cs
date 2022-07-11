// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrReadme.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Editor.Readme
{
    /// <summary>
    ///     Readme data drawn using the custom UxrReadmeEditor inspector script.
    /// </summary>
    [CreateAssetMenu(fileName = "Readme", menuName = "UltimateXR/Readme", order = 1)]
    public partial class UxrReadme : ScriptableObject
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Texture2D     _headerImage;
        [SerializeField] private List<Section> _sections = new List<Section>();
        [SerializeField] private List<Credit>  _credits  = new List<Credit>();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the header image.
        /// </summary>
        public Texture2D HeaderImage => _headerImage;

        /// <summary>
        ///     Gets the section information.
        /// </summary>
        public IReadOnlyList<Section> Sections => _sections;

        /// <summary>
        ///     Gets the credits.
        /// </summary>
        public IReadOnlyList<Credit> Credits => _credits;

        #endregion
    }
}