// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSpline.Sample.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Animation.Splines
{
    public abstract partial class UxrSpline
    {
        #region Private Types & Data

        /// <summary>
        ///     Pre-computed curve sample, used for arc-length parametrization.
        /// </summary>
        private class Sample
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the global spline evaluation t value this sample represents.
            /// </summary>
            public float LerpT { get; }

            /// <summary>
            ///     Gets the arc-length distance to the start of the spline.
            /// </summary>
            public float Distance { get; }

            /// <summary>
            ///     Gets the interpolated spline value.
            /// </summary>
            public Vector3 Position { get; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="t">Interpolation value [0.0, 1.0] between the spline start and end position.</param>
            /// <param name="distance">Arc-length distance to the start</param>
            /// <param name="position">Spline position</param>
            public Sample(float t, float distance, Vector3 position)
            {
                LerpT    = t;
                Distance = distance;
                Position = position;
            }

            #endregion
        }

        #endregion
    }
}