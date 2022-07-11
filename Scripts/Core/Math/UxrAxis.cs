// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAxis.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Core.Math
{
    /// <summary>
    ///     Class that allows to have formatted axes (A combo box with X, Y, Z strings) instead of numerical fields.
    ///     It also allows conversion from and to integers and <see cref="Vector3" /> types.
    ///     See the UxrAxisPropertyDrawer editor class for the integration with Unity Editor.
    /// </summary>
    [Serializable]
    public class UxrAxis
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int _axis;

        #endregion

        #region Public Types & Data

        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;

        /// <summary>
        ///     Returns a perpendicular axis.
        /// </summary>
        public UxrAxis Perpendicular => (_axis + 1) % 3;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="axis">Axis as an integer value</param>
        public UxrAxis(int axis)
        {
            _axis = Mathf.Clamp(axis, 0, 2);
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Unary minus operator. Negates the axis value.
        /// </summary>
        /// <param name="axis">Axis to negate</param>
        /// <returns>Negated axis</returns>
        public static Vector3 operator -(UxrAxis axis)
        {
            return -(Vector3)axis;
        }

        /// <summary>
        ///     Implicit conversion from an <see cref="UxrAxis" /> to a <see cref="Vector3" />.
        /// </summary>
        /// <param name="axis">Axis to convert</param>
        /// <returns>Converted <see cref="Vector3" /> value</returns>
        public static implicit operator Vector3(UxrAxis axis)
        {
            return axis._axis switch
                   {
                               0 => Vector3.right,
                               1 => Vector3.up,
                               _ => Vector3.forward
                   };
        }

        /// <summary>
        ///     Implicit conversion from an integer to an <see cref="UxrAxis" />.
        /// </summary>
        /// <param name="axis">Integer to convert</param>
        /// <returns>Converted <see cref="UxrAxis" /> value</returns>
        public static implicit operator UxrAxis(int axis)
        {
            return new UxrAxis(axis);
        }

        /// <summary>
        ///     Implicit conversion from an <see cref="UxrAxis" /> to an integer.
        /// </summary>
        /// <param name="axis">Axis to convert</param>
        /// <returns>Int value (0 = right, 1 = up, 2 = forward)</returns>
        public static implicit operator int(UxrAxis axis)
        {
            return axis._axis;
        }

        #endregion
    }
}