// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerNodeDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Manipulation.HandPoses
{
    /// <summary>
    ///     Stores a bone's right, up and forward vectors in local coordinates of its parent. Right, up and forward
    ///     vectors will always point to this directions independently of how the transforms have been set up in
    ///     order to guarantee poses can be reused by other hands that use a different coordinate system.
    /// </summary>
    [Serializable]
    public struct UxrFingerNodeDescriptor
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Matrix4x4 _transformRelativeToHand;
        [SerializeField] private Vector3   _right;
        [SerializeField] private Vector3   _up;
        [SerializeField] private Vector3   _forward;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the original relative transform to the hand bone. We use it mainly to compute
        ///     <see cref="UxrGrabbableObject" /> preview meshes more conveniently.
        /// </summary>
        public Matrix4x4 TransformRelativeToHand => _transformRelativeToHand;

        /// <summary>
        ///     Gets the universal right vector. The vector that points in our well-known right direction, in the coordinate system
        ///     of the finger.
        /// </summary>
        public Vector3 Right => _right;

        /// <summary>
        ///     Gets the universal up vector. The vector that points in our well-known up direction, in the coordinate system of
        ///     the finger.
        /// </summary>
        public Vector3 Up => _up;

        /// <summary>
        ///     Gets the universal forward vector. The vector that points in our well-known forward direction, in the coordinate
        ///     system of the finger.
        /// </summary>
        public Vector3 Forward => _forward;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Creates a well-known axes system for a node, to handle transforms independently of the coordinate system being used
        ///     by a hand rig.
        /// </summary>
        /// <param name="hand">Hand node</param>
        /// <param name="parent">Parent node</param>
        /// <param name="node">Current node being created</param>
        /// <param name="parentLocalAxes">
        ///     In local coordinates, which parent axes point to the well-known right, up and forward directions
        /// </param>
        /// <param name="nodeLocalAxes">
        ///     In local coordinates, which node axes point to the well-known right, up and forward directions
        /// </param>
        public UxrFingerNodeDescriptor(Transform hand, Transform parent, Transform node, UxrUniversalLocalAxes parentLocalAxes, UxrUniversalLocalAxes nodeLocalAxes)
        {
            _right   = Vector3.right;
            _up      = Vector3.up;
            _forward = Vector3.forward;

            _transformRelativeToHand = Matrix4x4.identity;

            if (hand == null || parent == null || node != null)
            {
                return;
            }

            Compute(hand, parent, node, parentLocalAxes, nodeLocalAxes, false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a well-known axes system for a node, to handle transforms independently of the coordinate system being used
        ///     by a hand rig.
        /// </summary>
        /// <param name="hand">Hand node</param>
        /// <param name="parent">Parent node</param>
        /// <param name="node">Current node being created</param>
        /// <param name="parentLocalAxes">
        ///     In local coordinates, which parent axes point to the well-known right, up and forward
        ///     directions
        /// </param>
        /// <param name="nodeLocalAxes">
        ///     In local coordinates, which node axes point to the well-known right, up and forward
        ///     directions
        /// </param>
        /// <param name="computeRelativeMatrixOnly">Whether to compute only the <see cref="TransformRelativeToHand" /> value</param>
        public void Compute(Transform hand, Transform parent, Transform node, UxrUniversalLocalAxes parentLocalAxes, UxrUniversalLocalAxes nodeLocalAxes, bool computeRelativeMatrixOnly)
        {
            _transformRelativeToHand = hand.worldToLocalMatrix * node.localToWorldMatrix;

            if (!computeRelativeMatrixOnly)
            {
                Matrix4x4 matrixParent = new Matrix4x4();
                matrixParent.SetColumn(0, parent.TransformVector(parentLocalAxes.LocalRight));
                matrixParent.SetColumn(1, parent.TransformVector(parentLocalAxes.LocalUp));
                matrixParent.SetColumn(2, parent.TransformVector(parentLocalAxes.LocalForward));
                matrixParent.SetColumn(3, new Vector4(parent.position.x, parent.position.y, parent.position.z, 1));

                _right   = matrixParent.inverse.MultiplyVector(node.TransformVector(nodeLocalAxes.LocalRight));
                _up      = matrixParent.inverse.MultiplyVector(node.TransformVector(nodeLocalAxes.LocalUp));
                _forward = matrixParent.inverse.MultiplyVector(node.TransformVector(nodeLocalAxes.LocalForward));
            }
        }

        /// <summary>
        ///     Mirrors the descriptor. Useful to switch between left and right hand data.
        /// </summary>
        public void Mirror()
        {
            // We do not need to mirror position and rotation because we don't use them for mirroring

            _right.x   = -_right.x;
            _right     = -_right;
            _up.x      = -_up.x;
            _forward.x = -_forward.x;
        }

        /// <summary>
        ///     Interpolates the axes data towards another descriptor.
        /// </summary>
        /// <param name="to">Descriptor to interpolate the data to</param>
        /// <param name="t">Interpolation factor [0.0, 1.0]</param>
        public void InterpolateTo(UxrFingerNodeDescriptor to, float t)
        {
            Quaternion quatSlerp = Quaternion.Slerp(Quaternion.LookRotation(_forward, _up), Quaternion.LookRotation(to._forward, to._up), t);
            _right   = quatSlerp * Vector3.right;
            _up      = quatSlerp * Vector3.up;
            _forward = quatSlerp * Vector3.forward;

            // For performance reasons, _transformRelativeToHand isn't interpolated because it is only used for grab preview poses. Interpolation is used for runtime pose blending.
            // If at any point it becomes necessary, uncomment the line below:

            // _transformRelativeToHand = Matrix4x4Ext.Interpolate(_transformRelativeToHand, to._transformRelativeToHand, t);
        }

        /// <summary>
        ///     Checks if the content of two FingerNodeDescriptors is equal (they describe the same axes).
        /// </summary>
        /// <param name="other">UxrFingerNodeDescriptor to compare it to</param>
        /// <returns>Boolean telling if the two FingerNodeDescriptors describe the same axes</returns>
        public bool Equals(UxrFingerNodeDescriptor other)
        {
            float epsilon = 0.00001f;

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if (Mathf.Abs(_transformRelativeToHand[i, j] - other._transformRelativeToHand[i, j]) > epsilon)
                    {
                        return false;
                    }
                }
            }

            bool equal = _right == other._right && _up == other._up && _forward == other._forward;
/*
            double inequalityThreshold = 9.99999943962493E-11;

            if (Right != other.Right)
            {
                double inequalityValue  = GetInequalityValue(Right, other.Right);
                double inequalityMargin = inequalityValue - inequalityThreshold;
                double inequalityFactor = inequalityValue / inequalityThreshold;
                Debug.Log($"right != other.right Inequality value = {inequalityValue}, margin = {inequalityMargin}, factor {inequalityFactor}");
            }

            if (Up != other.Up)
            {
                double inequalityValue  = GetInequalityValue(Up, other.Up);
                double inequalityMargin = inequalityValue - inequalityThreshold;
                double inequalityFactor = inequalityValue / inequalityThreshold;
                Debug.Log($"up != other.up Inequality value = {inequalityValue}, margin = {inequalityMargin}, factor {inequalityFactor}");
            }

            if (Forward != other.Forward)
            {
                double inequalityValue  = GetInequalityValue(Forward, other.Forward);
                double inequalityMargin = inequalityValue - inequalityThreshold;
                double inequalityFactor = inequalityValue / inequalityThreshold;
                Debug.Log($"forward != other.forward Inequality value = {inequalityValue}, margin = {inequalityMargin}, factor {inequalityFactor}");
            }*/

            return equal;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets an inequality value that measures how different two vectors are. It is used to provide a way to compare
        ///     vectors considering floating point errors.
        /// </summary>
        /// <param name="lhs">Vector A</param>
        /// <param name="rhs">Vector B</param>
        /// <returns>Inequality value</returns>
        private double GetInequalityValue(Vector3 lhs, Vector3 rhs)
        {
            float num1 = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            float num3 = lhs.z - rhs.z;
            return num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3;
        }

        #endregion
    }
}