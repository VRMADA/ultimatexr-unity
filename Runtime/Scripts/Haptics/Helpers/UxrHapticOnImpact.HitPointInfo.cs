// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticOnImpact.HitPointInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Haptics.Helpers
{
    public partial class UxrHapticOnImpact
    {
        #region Private Types & Data

        /// <summary>
        ///     Keeps track of runtime information of all <see cref="Transform" />s that can generate hit events by a
        ///     <see cref="UxrHapticOnImpact" /> component.
        /// </summary>
        private class HitPointInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the <see cref="Transform" /> component whose position will be checked for contacts.
            /// </summary>
            public Transform HitPoint { get; }

            /// <summary>
            ///     Gets the different velocity samples that are used to average the velocity of the last frames.
            /// </summary>
            public List<Vector3> VelocitySamples { get; } = new List<Vector3>(VelocityAverageSamples);

            /// <summary>
            ///     Gets or sets the last frame position.
            /// </summary>
            public Vector3 LastPos { get; set; }

            /// <summary>
            ///     Gets or sets the current velocity.
            /// </summary>
            public Vector3 Velocity { get; set; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="hitPoint">Transform of the hit point</param>
            public HitPointInfo(Transform hitPoint)
            {
                HitPoint = hitPoint;

                for (int i = 0; i < VelocityAverageSamples; ++i)
                {
                    VelocitySamples.Add(Vector3.zero);
                }
            }

            #endregion
        }

        #endregion
    }
}