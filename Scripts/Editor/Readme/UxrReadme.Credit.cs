// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Readme.Credit.cs" company="VRMADA">
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
        public class Credit
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private string _name;
            [SerializeField] private string _role;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the name.
            /// </summary>
            public string Name => _name;

            /// <summary>
            ///     Gets the role.
            /// </summary>
            public string Role => _role;

            #endregion
        }
    }
}