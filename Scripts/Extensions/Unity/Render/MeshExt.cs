// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Splines;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Mesh" /> extensions.
    /// </summary>
    public static class MeshExt
    {
        #region Public Methods

        /// <summary>
        ///     Creates a quad mesh <see cref="Mesh" />.
        /// </summary>
        /// <param name="size">Quad size (total width and height)</param>
        /// <returns>Quad <see cref="Mesh" /></returns>
        public static Mesh CreateQuad(float size)
        {
            Mesh  quadMesh = new Mesh();
            float halfSize = size * 0.5f;

            quadMesh.vertices  = new[] { new Vector3(halfSize, halfSize, 0.0f), new Vector3(halfSize, -halfSize, 0.0f), new Vector3(-halfSize, -halfSize, 0.0f), new Vector3(-halfSize, halfSize, 0.0f) };
            quadMesh.uv        = new[] { new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f) };
            quadMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };

            return quadMesh;
        }

        /// <summary>
        ///     Creates a <see cref="Mesh" /> tessellating a <see cref="UxrSpline" />
        /// </summary>
        /// <param name="spline">Spline to evaluate</param>
        /// <param name="subdivisions">Number of subdivisions along the spline axis</param>
        /// <param name="sides">Number of subdivisions in the section</param>
        /// <param name="radius">Section radius</param>
        /// <returns><see cref="Mesh" /> with the tessellated <see cref="UxrSpline" /></returns>
        public static Mesh CreateSpline(UxrSpline spline, int subdivisions, int sides, float radius)
        {
            subdivisions = Mathf.Max(subdivisions, 2);
            sides        = Mathf.Max(sides,        3);

            Mesh splineMesh = new Mesh();

            // Create mesh

            Vector3[] vertices = new Vector3[subdivisions * (sides + 1)];
            Vector3[] normals  = new Vector3[vertices.Length];
            Vector2[] mapping  = new Vector2[vertices.Length];
            int[]     indices  = new int[(subdivisions - 1) * sides * 2 * 3];

            for (int sub = 0; sub < subdivisions; ++sub)
            {
                float arcLength           = sub / (subdivisions - 1.0f) * spline.ArcLength;
                float normalizedArcLength = arcLength / spline.ArcLength;

                spline.EvaluateUsingArcLength(arcLength, out Vector3 splinePosition, out Vector3 splineDirection);

                Vector3 perpendicular = splineDirection.GetPerpendicularVector().normalized * radius;
                Vector3 vertexStart   = splinePosition + perpendicular;

                for (int side = 0; side < sides + 1; ++side)
                {
                    int vertexIndex = sub * (sides + 1) + side;
                    int faceBase    = sub * sides * 2 * 3 + side * 2 * 3;

                    float rotation = side / (float)sides;
                    float degrees  = 360.0f * rotation;

                    vertices[vertexIndex] = vertexStart.GetRotationAround(splinePosition, splineDirection, degrees);
                    mapping[vertexIndex]  = new Vector2(rotation, normalizedArcLength);
                    normals[vertexIndex]  = (vertices[vertexIndex] - splinePosition).normalized;

                    if (side < sides && sub < subdivisions - 1)
                    {
                        indices[faceBase + 0] = vertexIndex;
                        indices[faceBase + 1] = vertexIndex + 1;
                        indices[faceBase + 2] = vertexIndex + sides + 1;
                        indices[faceBase + 3] = vertexIndex + 1;
                        indices[faceBase + 4] = vertexIndex + sides + 2;
                        indices[faceBase + 5] = vertexIndex + sides + 1;
                    }
                }
            }

            splineMesh.vertices  = vertices;
            splineMesh.uv        = mapping;
            splineMesh.normals   = normals;
            splineMesh.triangles = indices;

            return splineMesh;
        }

        #endregion
    }
}