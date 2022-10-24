// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHand.ObjectVariation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    public partial class UxrControllerHand
    {
        #region Public Types & Data

        /// <summary>
        ///     Defines a GameObject variation in the different hands that are available in the component.
        /// </summary>
        [Serializable]
        public partial class ObjectVariation
        {
            #region Inspector Properties/Serialized Fields

            [SerializeField] private string                  _name;
            [SerializeField] private GameObject              _gameObject;
            [SerializeField] private List<MaterialVariation> _materialVariations;

            #endregion

            #region Public Types & Data

            /// <summary>
            ///     Gets the variation name.
            /// </summary>
            public string Name => _name;

            /// <summary>
            ///     Gets the variation object.
            /// </summary>
            public GameObject GameObject => _gameObject;

            /// <summary>
            ///     Gets the material variations.
            /// </summary>
            public IEnumerable<MaterialVariation> MaterialVariations => _materialVariations;

            #endregion
        }

        #endregion
    }
}