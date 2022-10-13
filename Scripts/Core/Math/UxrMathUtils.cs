// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMathUtils.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Core.Math
{
    /// <summary>
    ///     Contains math computations involving elements that do not belong to a specific class.
    ///     Most math is available through extensions classes in namespaces such as
    ///     <see cref="UltimateXR.Extensions.System.Math">UltimateXR.Extensions.System.Math</see> or
    ///     <see cref="UltimateXR.Extensions.Unity.Math">UltimateXR.Extensions.Unity.Math</see>.
    ///     Math related to animation is also available through classes in namespaces such as
    ///     <see cref="UltimateXR.Animation.IK">UltimateXR.Animation.IK</see>,
    ///     <see cref="UltimateXR.Animation.Interpolation">UltimateXRAnimation.Interpolation</see> or
    ///     <see cref="UltimateXR.Animation.Splines">UltimateXR.Animation.Splines</see>.
    ///     This class will contain math functionality that cannot be assigned to any extensions class.
    /// </summary>
    public static class UxrMathUtils
    {
        #region Public Methods

        /// <summary>
        ///     Tries to find the intersection(s) between a 2D line and a 2D circle
        ///     Code from: http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/
        /// </summary>
        /// <param name="linePoint1">Point A in the line</param>
        /// <param name="linePoint2">Point B in the line</param>
        /// <param name="circlePos">Circle position</param>
        /// <param name="radius">Circle radius</param>
        /// <param name="intersection1">Intersection 1 result, if it exists</param>
        /// <param name="intersection2">Intersection 2 result, if it exists</param>
        /// <returns>Number of intersections found (0, 1 or 2)</returns>
        public static int FindLineCircleIntersections2D(Vector2 linePoint1, Vector2 linePoint2, Vector2 circlePos, float radius, out Vector2 intersection1, out Vector2 intersection2)
        {
            float t;
            float dx  = linePoint2.x - linePoint1.x;
            float dy  = linePoint2.y - linePoint1.y;
            float a   = dx * dx + dy * dy;
            float b   = 2 * (dx * (linePoint1.x - circlePos.x) + dy * (linePoint1.y - circlePos.y));
            float c   = (linePoint1.x - circlePos.x) * (linePoint1.x - circlePos.x) + (linePoint1.y - circlePos.y) * (linePoint1.y - circlePos.y) - radius * radius;
            float det = b * b - 4 * a * c;

            if (a <= 0.0000001 || det < 0)
            {
                // No real solutions.
                intersection1 = new Vector3(float.NaN, float.NaN);
                intersection2 = new Vector3(float.NaN, float.NaN);
                return 0;
            }
            if (det == 0)
            {
                // One solution.
                t             = -b / (2 * a);
                intersection1 = new Vector3(linePoint1.x + t * dx, linePoint1.y + t * dy);
                intersection2 = new Vector3(float.NaN,             float.NaN);
                return 1;
            }
            // Two solutions.
            t             = (-b + Mathf.Sqrt(det)) / (2 * a);
            intersection1 = new Vector3(linePoint1.x + t * dx, linePoint1.y + t * dy);
            t             = (-b - Mathf.Sqrt(det)) / (2 * a);
            intersection2 = new Vector3(linePoint1.x + t * dx, linePoint1.y + t * dy);
            return 2;
        }

        /// <summary>
        ///     Applies to <paramref name="position" /> and <paramref name="rotation" /> the transformation to make a transform
        ///     defined by <paramref name="sourcePosition" /> and <paramref name="sourceRotation" /> move and rotate to
        ///     <paramref name="targetPosition" /> and <paramref name="targetRotation" />.
        /// </summary>
        /// <param name="position">Position to apply the transformation to</param>
        /// <param name="rotation">Rotation to apply the transformation to</param>
        /// <param name="sourcePosition">Source position that will try to match <paramref name="targetPosition" /></param>
        /// <param name="sourceRotation">Source rotation that will try to match <paramref name="targetRotation" /></param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="targetRotation">Target rotation</param>
        /// <param name="rotate">Allows to control whether to rotate or not</param>
        /// <param name="translate">Allows to control whether to translate or not</param>
        /// <param name="t">Optional interpolation value [0.0, 1.0]</param>
        public static void ApplyAlignment(ref Vector3    position,
                                          ref Quaternion rotation,
                                          Vector3        sourcePosition,
                                          Quaternion     sourceRotation,
                                          Vector3        targetPosition,
                                          Quaternion     targetRotation,
                                          bool           rotate,
                                          bool           translate,
                                          float          t = 1.0f)
        {
            if (rotate)
            {
                rotation.ApplyAlignment(sourceRotation, targetRotation, t);
            }

            if (translate)
            {
                position += (targetPosition - sourcePosition) * t;
            }
        }

        /// <summary>
        ///     Checks if a box is completely (all corners) inside a BoxCollider
        /// </summary>
        /// <param name="boxPosition">Position of the box to test if it is inside the BoxCollider</param>
        /// <param name="boxRotation">Rotation of the box to test if it is inside the BoxCollider</param>
        /// <param name="boxScale">Scale of the box to test if it is inside the BoxCollider</param>
        /// <param name="boxCenter">Center of the box (in local coordinates) to test if it is inside the BoxCollider</param>
        /// <param name="boxSize">Size of the box (in local coordinates) to test if it is inside the BoxCollider</param>
        /// <param name="boxVolume">BoxCollider to test against</param>
        /// <param name="margin">Allowed margin for each x, y, z component</param>
        /// <returns>True if all corners are inside the BoxCollider plus margin</returns>
        public static bool IsBoxInsideBox(Vector3 boxPosition, Quaternion boxRotation, Vector3 boxScale, Vector3 boxCenter, Vector3 boxSize, BoxCollider boxVolume, Vector3 margin = default)
        {
            Matrix4x4 boxMatrix = Matrix4x4.TRS(boxPosition, boxRotation, boxScale);
            Vector3[] corners   = new Vector3[8];

            corners[0] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(+boxSize.x, +boxSize.y, +boxSize.z) * 0.5f);
            corners[1] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(+boxSize.x, +boxSize.y, -boxSize.z) * 0.5f);
            corners[2] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(+boxSize.x, -boxSize.y, +boxSize.z) * 0.5f);
            corners[3] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(+boxSize.x, -boxSize.y, -boxSize.z) * 0.5f);
            corners[4] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(-boxSize.x, +boxSize.y, +boxSize.z) * 0.5f);
            corners[5] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(-boxSize.x, +boxSize.y, -boxSize.z) * 0.5f);
            corners[6] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(-boxSize.x, -boxSize.y, +boxSize.z) * 0.5f);
            corners[7] = boxMatrix.MultiplyPoint(boxCenter + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) * 0.5f);

            for (int i = 0; i < 8; ++i)
            {
                if (!corners[i].IsInsideBox(boxVolume, margin))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks if box1 is completely (all corners) inside box2
        /// </summary>
        /// <param name="box1Position">Position of box1</param>
        /// <param name="box1Rotation">Rotation of box1</param>
        /// <param name="box1Scale">Scale of box1</param>
        /// <param name="box1Center">Center of box1 in its own local coordinates</param>
        /// <param name="box1Size">Size of box1 in its own local coordinates</param>
        /// <param name="box2Position">Position of box2</param>
        /// <param name="box2Rotation">Rotation of box2</param>
        /// <param name="box2Scale">Scale of box2</param>
        /// <param name="box2Center">Center of box2 in its own local coordinates</param>
        /// <param name="box2Size">Size of box2 in its own local coordinates</param>
        /// <param name="margin">Allowed margin for each x, y, z component</param>
        /// <returns>True if all corners of box1 are inside box2 plus margin</returns>
        public static bool IsBoxInsideBox(Vector3    box1Position,
                                          Quaternion box1Rotation,
                                          Vector3    box1Scale,
                                          Vector3    box1Center,
                                          Vector3    box1Size,
                                          Vector3    box2Position,
                                          Quaternion box2Rotation,
                                          Vector3    box2Scale,
                                          Vector3    box2Center,
                                          Vector3    box2Size,
                                          Vector3    margin = default)
        {
            Matrix4x4 boxMatrix = Matrix4x4.TRS(box1Position, box1Rotation, box1Scale);
            Vector3[] corners   = new Vector3[8];

            corners[0] = boxMatrix.MultiplyPoint(box1Center + new Vector3(+box1Size.x, +box1Size.y, +box1Size.z) * 0.5f);
            corners[1] = boxMatrix.MultiplyPoint(box1Center + new Vector3(+box1Size.x, +box1Size.y, -box1Size.z) * 0.5f);
            corners[2] = boxMatrix.MultiplyPoint(box1Center + new Vector3(+box1Size.x, -box1Size.y, +box1Size.z) * 0.5f);
            corners[3] = boxMatrix.MultiplyPoint(box1Center + new Vector3(+box1Size.x, -box1Size.y, -box1Size.z) * 0.5f);
            corners[4] = boxMatrix.MultiplyPoint(box1Center + new Vector3(-box1Size.x, +box1Size.y, +box1Size.z) * 0.5f);
            corners[5] = boxMatrix.MultiplyPoint(box1Center + new Vector3(-box1Size.x, +box1Size.y, -box1Size.z) * 0.5f);
            corners[6] = boxMatrix.MultiplyPoint(box1Center + new Vector3(-box1Size.x, -box1Size.y, +box1Size.z) * 0.5f);
            corners[7] = boxMatrix.MultiplyPoint(box1Center + new Vector3(-box1Size.x, -box1Size.y, -box1Size.z) * 0.5f);

            for (int i = 0; i < 8; ++i)
            {
                if (!corners[i].IsInsideBox(box2Position, box2Rotation, box2Scale, box2Center, box2Size, margin))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}