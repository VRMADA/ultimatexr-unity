// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.Splines;
using UltimateXR.Core;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="Mesh" /> extensions.
    /// </summary>
    public static partial class MeshExt
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

        /// <summary>
        ///     Creates a new mesh from a skinned mesh renderer based on a reference bone and an extract operation.
        /// </summary>
        /// <param name="skin">Skin to process</param>
        /// <param name="bone">Reference bone</param>
        /// <param name="extractOperation">Which part of the skinned mesh to extract</param>
        /// <param name="weightThreshold">Bone weight threshold above which the vertices will be extracted</param>
        /// <returns>New mesh</returns>
        public static Mesh ExtractSubMesh(SkinnedMeshRenderer skin, Transform bone, ExtractSubMeshOperation extractOperation, float weightThreshold = UxrConstants.Geometry.SignificantBoneWeight)
        {
            Mesh newMesh = new Mesh();

            // Create dictionary to check which bones belong to the hierarchy

            Dictionary<int, bool> areHierarchyBones = new Dictionary<int, bool>();

            Vector3[]    vertices    = skin.sharedMesh.vertices;
            Vector3[]    normals     = skin.sharedMesh.normals;
            Vector2[]    uv          = skin.sharedMesh.uv;
            BoneWeight[] boneWeights = skin.sharedMesh.boneWeights;
            Transform[]  bones       = skin.bones;

            for (int i = 0; i < bones.Length; ++i)
            {
                areHierarchyBones.Add(i, bones[i].HasParent(bone));
            }

            // Create filtered mesh

            List<List<int>>      newTriangles   = new List<List<int>>();
            Dictionary<int, int> old2New        = new Dictionary<int, int>();
            List<Vector3>        newVertices    = new List<Vector3>();
            List<Vector3>        newNormals     = new List<Vector3>();
            List<Vector2>        newUV          = new List<Vector2>();
            List<BoneWeight>     newBoneWeights = new List<BoneWeight>();

            bool VertexMeetsRequirement(bool isFromHierarchy)
            {
                switch (extractOperation)
                {
                    case ExtractSubMeshOperation.BoneAndChildren:       return isFromHierarchy;
                    case ExtractSubMeshOperation.NotFromBoneOrChildren: return !isFromHierarchy;
                }

                return false;
            }

            for (int submesh = 0; submesh < skin.sharedMesh.subMeshCount; ++submesh)
            {
                int[]     submeshIndices    = skin.sharedMesh.GetTriangles(submesh);
                List<int> newSubmeshIndices = new List<int>();

                for (int t = 0; t < submeshIndices.Length / 3; t++)
                {
                    float totalWeight = 0.0f;

                    for (int v = 0; v < 3; v++)
                    {
                        BoneWeight boneWeight = boneWeights[submeshIndices[t * 3 + v]];

                        if (areHierarchyBones.TryGetValue(boneWeight.boneIndex0, out bool isFromHierarchy) && VertexMeetsRequirement(isFromHierarchy))
                        {
                            totalWeight += boneWeight.weight0;
                        }

                        if (areHierarchyBones.TryGetValue(boneWeight.boneIndex1, out isFromHierarchy) && VertexMeetsRequirement(isFromHierarchy))
                        {
                            totalWeight += boneWeight.weight1;
                        }

                        if (areHierarchyBones.TryGetValue(boneWeight.boneIndex2, out isFromHierarchy) && VertexMeetsRequirement(isFromHierarchy))
                        {
                            totalWeight += boneWeight.weight2;
                        }

                        if (areHierarchyBones.TryGetValue(boneWeight.boneIndex3, out isFromHierarchy) && VertexMeetsRequirement(isFromHierarchy))
                        {
                            totalWeight += boneWeight.weight3;
                        }
                    }

                    if (totalWeight > weightThreshold)
                    {
                        for (int v = 0; v < 3; v++)
                        {
                            int oldIndex = submeshIndices[t * 3 + v];

                            if (!old2New.ContainsKey(oldIndex))
                            {
                                old2New.Add(oldIndex, old2New.Count);

                                newVertices.Add(vertices[oldIndex]);
                                newNormals.Add(normals[oldIndex]);
                                newUV.Add(uv[oldIndex]);
                                newBoneWeights.Add(boneWeights[oldIndex]);
                            }

                            newSubmeshIndices.Add(old2New[oldIndex]);
                        }
                    }
                }

                newTriangles.Add(newSubmeshIndices);
            }

            // Create new mesh

            newMesh.vertices    = newVertices.ToArray();
            newMesh.normals     = newNormals.ToArray();
            newMesh.uv          = newUV.ToArray();
            newMesh.boneWeights = newBoneWeights.ToArray();

            // Create and assign new triangle list

            newMesh.subMeshCount = newTriangles.Count;

            for (int submesh = 0; submesh < newTriangles.Count; ++submesh)
            {
                newMesh.SetTriangles(newTriangles[submesh].ToArray(), submesh);
            }

            return newMesh;
        }

        /// <summary>
        ///     Computes the number of vertices that a bone influences in a skinned mesh.
        /// </summary>
        /// <param name="skin">Skinned mesh</param>
        /// <param name="bone">Bone to check</param>
        /// <param name="weightThreshold">Weight above which will be considered significant influence</param>
        /// <returns>
        ///     Number of vertices influenced by <paramref name="bone" /> with a weight above
        ///     <paramref name="weightThreshold" />.
        /// </returns>
        public static int GetBoneInfluenceVertexCount(SkinnedMeshRenderer skin, Transform bone, float weightThreshold = UxrConstants.Geometry.SignificantBoneWeight)
        {
            Transform[] skinBones = skin.bones;
            int         boneIndex = skinBones.IndexOf(bone);

            if (boneIndex == -1)
            {
                return 0;
            }

            BoneWeight[] boneWeights = skin.sharedMesh.boneWeights;

            return boneWeights.Count(w => HasBoneInfluence(w, boneIndex, weightThreshold));
        }

        /// <summary>
        ///     Computes the number of vertices that a bone influences in a skinned mesh.
        /// </summary>
        /// <param name="skin">Skinned mesh</param>
        /// <param name="bone">Bone to check</param>
        /// <param name="weightThreshold">Weight above which to consider significant influence</param>
        /// <returns>
        ///     Number of vertices influenced by <paramref name="bone" /> with a weight above
        ///     <paramref name="weightThreshold" />
        /// </returns>
        public static bool HasBoneInfluence(SkinnedMeshRenderer skin, Transform bone, float weightThreshold = UxrConstants.Geometry.SignificantBoneWeight)
        {
            Transform[] skinBones = skin.bones;
            int         boneIndex = skinBones.IndexOf(bone);

            if (boneIndex == -1)
            {
                return false;
            }

            BoneWeight[] boneWeights = skin.sharedMesh.boneWeights;

            return boneWeights.Any(w => HasBoneInfluence(w, boneIndex, weightThreshold));
        }

        /// <summary>
        ///     Checks whether a given bone index has influence on a skinned mesh vertex.
        /// </summary>
        /// <param name="boneWeight">Vertex's bone weight information</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="weightThreshold">Weight above which will be considered significant influence</param>
        /// <returns>Whether the bone influences the vertex in a significant amount</returns>
        public static bool HasBoneInfluence(in BoneWeight boneWeight, int boneIndex, float weightThreshold = UxrConstants.Geometry.SignificantBoneWeight)
        {
            if (boneWeight.boneIndex0 == boneIndex && boneWeight.weight0 > weightThreshold)
            {
                return true;
            }

            if (boneWeight.boneIndex1 == boneIndex && boneWeight.weight1 > weightThreshold)
            {
                return true;
            }

            if (boneWeight.boneIndex2 == boneIndex && boneWeight.weight2 > weightThreshold)
            {
                return true;
            }

            if (boneWeight.boneIndex3 == boneIndex && boneWeight.weight3 > weightThreshold)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Computes the bounding box that contains all the vertices that a bone has influence on in a skinned mesh. The
        ///     bounding box is computed in local bone space.
        /// </summary>
        /// <param name="skin">Skinned mesh</param>
        /// <param name="bone">Bone to check</param>
        /// <param name="weightThreshold">Weight above which to consider significant influence</param>
        /// <returns>
        ///     Bounding box in local <paramref name="bone" /> coordinates.
        /// </returns>
        public static Bounds GetBoneInfluenceBounds(SkinnedMeshRenderer skin, Transform bone, float weightThreshold = UxrConstants.Geometry.SignificantBoneWeight)
        {
            Transform[] skinBones = skin.bones;
            int         boneIndex = skinBones.IndexOf(bone);

            if (boneIndex == -1)
            {
                return new Bounds();
            }

            Vector3[]    vertices      = skin.sharedMesh.vertices;
            BoneWeight[] boneWeights   = skin.sharedMesh.boneWeights;
            Transform[]  bones         = skin.bones;
            Matrix4x4[]  boneBindPoses = skin.sharedMesh.bindposes;
            Vector3      min           = Vector3.zero;
            Vector3      max           = Vector3.zero;
            bool         initialized   = false;

            for (int i = 0; i < boneWeights.Length; ++i)
            {
                if (HasBoneInfluence(boneWeights[i], boneIndex, weightThreshold))
                {
                    Vector3 localVertex = bones[boneIndex].InverseTransformPoint(GetSkinnedWorldVertex(skin, boneWeights[i], vertices[i], bones, boneBindPoses));

                    if (!initialized)
                    {
                        initialized = true;
                        min         = localVertex;
                        max         = localVertex;
                    }
                    else
                    {
                        min = Vector3Ext.Min(localVertex, min);
                        max = Vector3Ext.Max(localVertex, max);
                    }
                }
            }

            return new Bounds((min + max) * 0.5f, max - min);
        }

        /// <summary>
        ///     Gets a skinned vertex in world coordinates.
        /// </summary>
        /// <param name="skin">Skin</param>
        /// <param name="boneWeight">Vertex bone weights info</param>
        /// <param name="vertex">Vertex in local skin coordinates when the skin is in the bind pose</param>
        /// <param name="bones">Bone list</param>
        /// <param name="boneBindPoses">Bone bind poses</param>
        /// <returns>Vertex in world coordinates</returns>
        public static Vector3 GetSkinnedWorldVertex(SkinnedMeshRenderer skin, BoneWeight boneWeight, Vector3 vertex, Transform[] bones, Matrix4x4[] boneBindPoses)
        {
            Vector3 result = Vector3.zero;

            if (boneWeight.weight0 > UxrConstants.Geometry.SmallestBoneWeight)
            {
                result += bones[boneWeight.boneIndex0].localToWorldMatrix.MultiplyPoint(boneBindPoses[boneWeight.boneIndex0].MultiplyPoint(vertex)) * boneWeight.weight0;
            }

            if (boneWeight.weight1 > UxrConstants.Geometry.SmallestBoneWeight)
            {
                result += bones[boneWeight.boneIndex1].localToWorldMatrix.MultiplyPoint(boneBindPoses[boneWeight.boneIndex1].MultiplyPoint(vertex)) * boneWeight.weight1;
            }

            if (boneWeight.weight2 > UxrConstants.Geometry.SmallestBoneWeight)
            {
                result += bones[boneWeight.boneIndex2].localToWorldMatrix.MultiplyPoint(boneBindPoses[boneWeight.boneIndex2].MultiplyPoint(vertex)) * boneWeight.weight2;
            }

            if (boneWeight.weight3 > UxrConstants.Geometry.SmallestBoneWeight)
            {
                result += bones[boneWeight.boneIndex3].localToWorldMatrix.MultiplyPoint(boneBindPoses[boneWeight.boneIndex3].MultiplyPoint(vertex)) * boneWeight.weight3;
            }

            return result;
        }

        #endregion
    }
}