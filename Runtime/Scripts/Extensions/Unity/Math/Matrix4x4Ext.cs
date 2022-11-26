// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Matrix4x4Ext.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Math
{
    /// <summary>
    ///     <see cref="Matrix4x4" /> extensions.
    /// </summary>
    public static class Matrix4x4Ext
    {
        #region Public Methods

        /// <summary>
        ///     Interpolates two matrices by decomposing the position, rotation and scale values and interpolating them separately.
        /// </summary>
        /// <param name="matrixA">Source matrix</param>
        /// <param name="matrixB">Destination matrix</param>
        /// <param name="blendValue">Interpolation value [0.0, 1.0]</param>
        /// <returns>Interpolated matrix</returns>
        public static Matrix4x4 Interpolate(Matrix4x4 matrixA, Matrix4x4 matrixB, float blendValue)
        {
            Vector3    position = Vector3.Lerp(matrixA.MultiplyPoint(Vector3.zero), matrixB.MultiplyPoint(Vector3.zero), blendValue);
            Quaternion rotation = Quaternion.Slerp(matrixA.rotation, matrixB.rotation, blendValue);
            Vector3    scale    = Vector3.Lerp(matrixA.lossyScale, matrixB.lossyScale, blendValue);

            return Matrix4x4.TRS(position, rotation, scale);
        }

        /// <summary>
        ///     Computes a projection matrix so that it has an oblique near clip plane.
        /// </summary>
        /// <param name="projection">Projection matrix</param>
        /// <param name="clipPlane">Clipping plane in camera space</param>
        /// <returns>Projection matrix with oblique clip plane</returns>
        public static Matrix4x4 GetObliqueMatrix(this Matrix4x4 projection, Vector4 clipPlane)
        {
            Matrix4x4 oblique = projection;
            Vector4   q       = projection.inverse * new Vector4(Mathf.Sign(clipPlane.x), Mathf.Sign(clipPlane.y), 1.0f, 1.0f);
            Vector4   c       = clipPlane * (2.0F / Vector4.Dot(clipPlane, q));

            //third row = clip plane - fourth row
            oblique[2]  = c.x - projection[3];
            oblique[6]  = c.y - projection[7];
            oblique[10] = c.z - projection[11];
            oblique[14] = c.w - projection[15];

            return oblique;
        }

        /// <summary>
        ///     Computes the reflection matrix around the given plane.
        /// </summary>
        /// <param name="plane">Reflection plane</param>
        /// <returns>Reflected matrix</returns>
        public static Matrix4x4 GetReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 reflectionMat;

            reflectionMat.m00 = 1F - 2F * plane[0] * plane[0];
            reflectionMat.m01 = -2F * plane[0] * plane[1];
            reflectionMat.m02 = -2F * plane[0] * plane[2];
            reflectionMat.m03 = -2F * plane[3] * plane[0];

            reflectionMat.m10 = -2F * plane[1] * plane[0];
            reflectionMat.m11 = 1F - 2F * plane[1] * plane[1];
            reflectionMat.m12 = -2F * plane[1] * plane[2];
            reflectionMat.m13 = -2F * plane[3] * plane[1];

            reflectionMat.m20 = -2F * plane[2] * plane[0];
            reflectionMat.m21 = -2F * plane[2] * plane[1];
            reflectionMat.m22 = 1F - 2F * plane[2] * plane[2];
            reflectionMat.m23 = -2F * plane[3] * plane[2];

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;

            return reflectionMat;
        }

        #endregion
    }
}