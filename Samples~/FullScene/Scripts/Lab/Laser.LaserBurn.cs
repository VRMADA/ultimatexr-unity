// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Laser.LaserBurn.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Lab
{
    public partial class Laser
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores all information for a burn result of pointing the enabled laser to an object.
        /// </summary>
        private class LaserBurn
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the transform component of the GameObject that is used to represent the burn.
            /// </summary>
            public Transform Transform => GameObject.transform;

            /// <summary>
            ///     Gets the last normal of the laser impact that caused the laser burn.
            /// </summary>
            public Vector3 LastWorldNormal => Transform.TransformVector(LastNormal);

            /// <summary>
            ///     Gets the last world-space position in the burn path.
            /// </summary>
            public Vector3 LastWorldPathPosition => Transform.TransformPoint(PathPositions[PathPositions.Count - 1]);

            /// <summary>
            ///     Gets the dynamically created object to represent the burn.
            /// </summary>
            public GameObject GameObject { get; set; }

            /// <summary>
            ///     Gets the dynamically created object that represents the incandescent part in the burn.
            /// </summary>
            public GameObject GameObjectIncandescent { get; set; }

            /// <summary>
            ///     Gets the collider that was hit.
            /// </summary>
            public Collider Collider { get; set; }

            /// <summary>
            ///     Gets the burn path line renderer.
            /// </summary>
            public LineRenderer LineRenderer { get; set; }

            /// <summary>
            ///     Gets the incandescent  path line renderer.
            /// </summary>
            public LineRenderer IncandescentLineRenderer { get; set; }

            /// <summary>
            ///     Gets the positions in the burn path.
            /// </summary>
            public List<Vector3> PathPositions { get; set; }

            /// <summary>
            ///     Gets the creation times of each path position so that we can fade them out based on age.
            /// </summary>
            public List<float> PathCreationTimes { get; set; }

            /// <summary>
            ///     Last hit normal in local coordinates of the burn object.
            /// </summary>
            public Vector3 LastNormal { get; set; }

            #endregion
        }

        #endregion
    }
}