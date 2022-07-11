// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerHand.ObjectVariation.MaterialVariation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    public partial class UxrControllerHand
    {
        #region Public Types & Data

        public partial class ObjectVariation
        {
            #region Public Types & Data

            /// <summary>
            ///     Defines a Material variation in the different materials available for a hand GameObject.
            /// </summary>
            [Serializable]
            public class MaterialVariation
            {
                #region Inspector Properties/Serialized Fields

                [SerializeField] private string   _name;
                [SerializeField] private Material _material;

                #endregion

                #region Public Types & Data

                /// <summary>
                ///     Gets the variation name.
                /// </summary>
                public string Name => _name;

                /// <summary>
                ///     Gets the variation material.
                /// </summary>
                public Material Material => _material;

                #endregion
            }

            #endregion
        }

        #endregion
    }
}