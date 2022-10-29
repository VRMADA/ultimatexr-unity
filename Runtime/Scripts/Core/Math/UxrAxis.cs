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
    public class UxrAxis : IEquatable<UxrAxis>
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

        /// <summary>
        ///     Returns the other perpendicular axis.
        /// </summary>
        public UxrAxis OtherPerpendicular => (_axis + 2) % 3;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="axis">Axis as an integer value</param>
        public UxrAxis(int axis)
        {
#if UNITY_EDITOR
            
            if (axis < 0 || axis > 3)
            {
                Debug.LogError($"Assigning invalid value to axis: {axis}");
            }
            
#endif
            _axis = Mathf.Clamp(axis, 0, 2);
        }

        #endregion

        #region Implicit IEquatable<UxrAxis>

        /// <inheritdoc />
        public bool Equals(UxrAxis other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            return _axis == other._axis;
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            return _axis switch
                   {
                               0 => "Right",
                               1 => "Up",
                               _ => "Forward"
                   };
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((UxrAxis)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _axis;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Returns the remaining axis which is not axis1 nor axis2.
        /// </summary>
        /// <param name="axis1">Axis 1</param>
        /// <param name="axis2">Axis 2</param>
        /// <returns>The remaining axis which is not axis1 nor axis2</returns>
        public static UxrAxis OtherThan(UxrAxis axis1, UxrAxis axis2)
        {
            if (axis1 == axis2)
            {
                Debug.LogError($"{nameof(UxrAxis)}: Got same axis for {nameof(OtherThan)} (axis1)");
                return axis1.Perpendicular;
            }

            int smaller = axis1._axis < axis2._axis ? axis1._axis : axis2._axis;
            int bigger  = axis1._axis > axis2._axis ? axis1._axis : axis2._axis;

            if (smaller == 0 && bigger == 1)
            {
                return 2;
            }
            if (smaller == 0 && bigger == 2)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        ///     Gets the color representing
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Color GetColor(float alpha)
        {
            Vector3 axis = this;
            return new Color(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z), alpha);
        }

        #endregion

        #region Operators

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
        ///     Equality operator.
        /// </summary>
        /// <param name="a">Operand A</param>
        /// <param name="b">Operand B</param>
        /// <returns>Whether the two operands are equal</returns>
        public static bool operator ==(UxrAxis a, UxrAxis b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        ///     Inequality operator.
        /// </summary>
        /// <param name="a">Operand A</param>
        /// <param name="b">Operand B</param>
        /// <returns>Whether the two operands are different</returns>
        public static bool operator !=(UxrAxis a, UxrAxis b)
        {
            return !(a == b);
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