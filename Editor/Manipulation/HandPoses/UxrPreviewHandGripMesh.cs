// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPreviewHandGripMesh.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    /// <summary>
    ///     Class that computes and stores a hand mesh used to preview a grab hand pose in the editor.
    ///     Grab preview meshes are generated so that vertices are in grabber space. By creating a helper object
    ///     and assigning it the preview mesh, it is easy to preview how an avatar would grab an object by placing
    ///     the helper on the grabbable object snap position.
    /// </summary>
    public class UxrPreviewHandGripMesh
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets if the object contains mesh data.
        /// </summary>
        public bool IsValid => _initialVertices != null && _vertices != null && _normals != null && UnityMesh != null && _bones != null && _bonesClosed != null;

        /// <summary>
        ///     Gets the mesh.
        /// </summary>
        public Mesh UnityMesh { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor is private. Use <see cref="Build" /> instead.
        /// </summary>
        private UxrPreviewHandGripMesh()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a preview mesh for the given grabbable object.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to create the preview mesh for</param>
        /// <param name="avatar">The avatar to create the preview mesh for</param>
        /// <param name="grabPoint">The grab point to create the preview mesh for</param>
        /// <param name="handSide">Which hand to create the preview mesh for</param>
        /// <returns>Preview mesh</returns>
        public static UxrPreviewHandGripMesh Build(UxrGrabbableObject grabbableObject, UxrAvatar avatar, int grabPoint, UxrHandSide handSide)
        {
            // Find pose

            UxrHandPoseAsset handPoseAsset = grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(avatar).HandPose;

            if (handPoseAsset == null)
            {
                return null;
            }

            UxrHandPoseAsset avatarHandPose = avatar.GetHandPose(handPoseAsset.name);

            if (avatarHandPose == null)
            {
                Debug.LogWarning($"Avatar {avatar.name}: Could not find {nameof(UxrHandPoseAsset)} file for pose {handPoseAsset.name}.");
                return null;
            }

            // Find grabbers

            UxrGrabber[] grabbers = avatar.GetComponentsInChildren<UxrGrabber>();

            if (grabbers.Length == 0)
            {
                Debug.LogWarning($"Avatar {avatar.name}: No {nameof(UxrGrabber)} components found in hierarchy.");
                return null;
            }

            // Iterate over meshes from grabbers

            UxrPreviewHandGripMesh previewMesh = new UxrPreviewHandGripMesh();

            foreach (UxrGrabber grabber in grabbers)
            {
                // Get mesh snapshot

                Renderer renderer = grabber.HandRenderer;

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer && grabber.Side == handSide)
                {
                    Transform handTransform = avatar.GetHandBone(handSide);
                    previewMesh.UnityMesh = new Mesh();
                    skinnedMeshRenderer.BakeMesh(previewMesh.UnityMesh);
                    previewMesh.CreateMesh(avatar, skinnedMeshRenderer, handSide);
                    previewMesh.CreateBoneList(avatar, skinnedMeshRenderer, avatarHandPose, handSide);
                    previewMesh.UnityMesh.name = UxrGrabPointIndex.GetIndexDisplayName(grabbableObject, grabPoint) + (handSide == UxrHandSide.Left ? UxrGrabbableObject.LeftGrabPoseMeshSuffix : UxrGrabbableObject.RightGrabPoseMeshSuffix);

                    if (avatarHandPose.PoseType == UxrHandPoseType.Fixed)
                    {
                        previewMesh.SetBlendPoseValue(grabber, handTransform, false);
                    }
                    else if (avatarHandPose.PoseType == UxrHandPoseType.Blend)
                    {
                        previewMesh.SetBlendPoseValue(grabber, handTransform, true, grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(grabber.Avatar).PoseBlendValue);
                    }

                    return previewMesh;
                }
            }

            Debug.LogWarning($"Avatar {avatar.name}: No {(handSide == UxrHandSide.Left ? "left" : "right")} {nameof(UxrGrabber)} component found in avatar hierarchy or component has no renderer defined to get the mesh from.");
            return null;
        }

        /// <summary>
        ///     Refreshes the preview mesh.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object to refresh the preview mesh for</param>
        /// <param name="avatar">The avatar to refresh the preview mesh for</param>
        /// <param name="grabPoint">The grab point to refresh the preview mesh for</param>
        /// <param name="handSide">Which hand to refresh the preview mesh for</param>
        public bool Refresh(UxrGrabbableObject grabbableObject, UxrAvatar avatar, int grabPoint, UxrHandSide handSide, bool reloadBoneData = false)
        {
            if (grabbableObject == null || avatar == null)
            {
                return false;
            }

            UxrGrabber[] grabbers = avatar.GetComponentsInChildren<UxrGrabber>();
            UxrGrabber   grabber  = grabbers.FirstOrDefault(g => g.Side == handSide);

            if (grabber == null)
            {
                return false;
            }

            // Find pose

            UxrHandPoseAsset handPoseAsset = grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(avatar).HandPose;

            if (handPoseAsset == null)
            {
                return false;
            }

            UxrHandPoseAsset avatarHandPose = avatar.GetHandPose(handPoseAsset.name);

            if (avatarHandPose == null)
            {
                Debug.LogWarning($"Avatar {avatar.name}: Could not find {nameof(UxrHandPoseAsset)} file for pose {handPoseAsset.name}.");
                return false;
            }

            if (reloadBoneData && grabber.HandRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                CreateBoneList(avatar, skinnedMeshRenderer, avatarHandPose, handSide);
            }

            // Refresh

            if (avatarHandPose.PoseType == UxrHandPoseType.Fixed)
            {
                SetBlendPoseValue(grabber, avatar.GetHandBone(handSide), false);
            }
            else if (avatarHandPose.PoseType == UxrHandPoseType.Blend)
            {
                SetBlendPoseValue(grabber, avatar.GetHandBone(handSide), true, grabbableObject.GetGrabPoint(grabPoint).GetGripPoseInfo(grabber.Avatar).PoseBlendValue);
            }

            return true;
        }

        /// <summary>
        ///     Changes the blend value for a pose that supports blending.
        /// </summary>
        /// <param name="grabber">Grabber component</param>
        /// <param name="handTransform">Hand bone transform</param>
        /// <param name="blend">Blend value</param>
        /// <param name="blendValue">New blend value</param>
        public void SetBlendPoseValue(UxrGrabber grabber, Transform handTransform, bool blend, float blendValue = 0.0f)
        {
            if (!IsValid)
            {
                Debug.LogWarning("Preview mesh is not valid");
                return;
            }

            // Compute hand to grabber transform matrix and remove scaling because grabbable objects may be scaled but we don't want the preview vertices to scale

            Matrix4x4 handToGrabberMatrix = grabber.transform.worldToLocalMatrix * handTransform.localToWorldMatrix;

            // Precompute matrices

            if (blend == false)
            {
                ComputeBoneTransformsFixed(handToGrabberMatrix, _bones);
            }
            else
            {
                ComputeBoneTransformsBlend(handToGrabberMatrix, _bones, _bonesClosed, blendValue);
            }

            // Launch threads

            int threadCount = 8;

            if (_vertices.Length < threadCount)
            {
                ComputeMesh(0, _vertices.Length);
            }
            else
            {
                List<Thread> threads = new List<Thread>();

                int threadVertexCount     = _vertices.Length / threadCount;
                int lastThreadVertexCount = _vertices.Length - threadVertexCount * (threadCount - 1);

                for (int i = 0; i < threadCount; ++i)
                {
                    int    indexStart = i * threadVertexCount;
                    int    indexCount = i < threadCount - 1 ? threadVertexCount : lastThreadVertexCount;
                    Thread thread     = new Thread(() => ComputeMesh(indexStart, indexCount));
                    thread.Start();
                    threads.Add(thread);
                }

                // Wait for threads to finish

                foreach (Thread thread in threads)
                {
                    while (thread.IsAlive)
                    {
                    }
                }
            }

            UnityMesh.vertices = _vertices;
            UnityMesh.normals  = _normals;
            UnityMesh.RecalculateBounds();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes the bone transforms for a fixed pose. The transforms move vertices from local space to grabber space.
        /// </summary>
        /// <param name="handToGrabberMatrix">Matrix that transforms from local hand space to grabber space</param>
        /// <param name="boneList">List of bones</param>
        private static void ComputeBoneTransformsFixed(Matrix4x4 handToGrabberMatrix, List<UxrPreviewHandBoneInfo> boneList)
        {
            foreach (UxrPreviewHandBoneInfo bone in boneList)
            {
                bone.CurrentTransform = handToGrabberMatrix * bone.TransformRelativeToHand * bone.BindPose;
            }
        }

        /// <summary>
        ///     Computes the bone transforms for a blend pose. The transforms move vertices from local space to grabber space.
        /// </summary>
        /// <param name="handToGrabberMatrix">Matrix that transforms from local hand space to grabber space</param>
        /// <param name="boneListOpen">Bones for the open pose (blend 0)</param>
        /// <param name="boneListClosed">Bones for the closed pose blend (1)</param>
        /// <param name="blendValue">Blend value to compute the transforms of</param>
        private static void ComputeBoneTransformsBlend(Matrix4x4 handToGrabberMatrix, List<UxrPreviewHandBoneInfo> boneListOpen, List<UxrPreviewHandBoneInfo> boneListClosed, float blendValue)
        {
            // Compute first pass of interpolated relative matrices. We interpolate relative matrices instead of absolute to have correct blending.

            for (int i = 0; i < boneListOpen.Count; ++i)
            {
                boneListOpen[i].CurrentRelativeTransform = Matrix4x4Ext.Interpolate(boneListOpen[i].TransformRelativeToParent, boneListClosed[i].TransformRelativeToParent, blendValue);
            }

            // Now compute the absolute matrices.

            for (int i = 0; i < boneListOpen.Count; ++i)
            {
                int       currentBoneIndex = i;
                Matrix4x4 matrix           = boneListOpen[i].CurrentRelativeTransform;

                while (boneListOpen[currentBoneIndex].ParentBoneIndex != -1)
                {
                    currentBoneIndex = boneListOpen[currentBoneIndex].ParentBoneIndex;
                    matrix           = boneListOpen[currentBoneIndex].CurrentRelativeTransform * matrix;
                }

                boneListOpen[i].CurrentTransform = matrix;
            }

            // Now pre-compute the composite transform to have vertices from local to the grabber transform. We compute it in the open bone list to use the same multiplication routine as fixed poses later.

            foreach (UxrPreviewHandBoneInfo bone in boneListOpen)
            {
                bone.CurrentTransform = handToGrabberMatrix * bone.CurrentTransform * bone.BindPose;
            }
        }

        /// <summary>
        ///     Gets the bind pose of a given hand.
        /// </summary>
        /// <param name="skinnedMeshRenderer">Skin component</param>
        /// <param name="handTransform">Hand transform</param>
        /// <returns>Bind pose</returns>
        private static Matrix4x4 GetHandBindPose(SkinnedMeshRenderer skinnedMeshRenderer, Transform handTransform)
        {
            for (int i = 0; i < skinnedMeshRenderer.bones.Length; ++i)
            {
                if (skinnedMeshRenderer.bones[i] == handTransform)
                {
                    return skinnedMeshRenderer.sharedMesh.bindposes[i];
                }
            }

            return Matrix4x4.identity;
        }

        /// <summary>
        ///     Creates preview mesh data for a given skinned mesh renderer.
        /// </summary>
        /// <param name="avatar">Avatar to generate the data for</param>
        /// <param name="skinnedMeshRenderer">Skin component</param>
        /// <param name="handSide">Which hand to generate the bone list for</param>
        private void CreateMesh(UxrAvatar avatar, SkinnedMeshRenderer skinnedMeshRenderer, UxrHandSide handSide)
        {
            // Extract only hand portion of the mesh
            
            UnityMesh = MeshExt.ExtractSubMesh(skinnedMeshRenderer, avatar.GetHand(handSide).Wrist, MeshExt.ExtractSubMeshOperation.BoneAndChildren);
            
            // Create dynamic mesh data arrays

            _initialVertices = UnityMesh.vertices;
            _initialNormals  = UnityMesh.normals;
            _boneWeights     = UnityMesh.boneWeights;
            _vertices        = new Vector3[_initialVertices.Length];
            _normals         = new Vector3[_initialVertices.Length];
        }

        /// <summary>
        ///     Creates the internal bone list required for a given hand pose.
        /// </summary>
        /// <param name="avatar">Avatar to generate the bone list for</param>
        /// <param name="skinnedMeshRenderer">Skinned mesh renderer component</param>
        /// <param name="handPoseAsset">Hand pose asset to generate the bone list for</param>
        /// <param name="handSide">Which hand to generate the bone list for</param>
        private void CreateBoneList(UxrAvatar avatar, SkinnedMeshRenderer skinnedMeshRenderer, UxrHandPoseAsset handPoseAsset, UxrHandSide handSide)
        {
            // Create bone information

            Matrix4x4 handBindPose = GetHandBindPose(skinnedMeshRenderer, avatar.GetHandBone(handSide));

            if (handPoseAsset.PoseType == UxrHandPoseType.Fixed)
            {
                _bones = UxrPreviewHandBoneInfo.CreateHandBoneData(skinnedMeshRenderer,
                                                                   skinnedMeshRenderer.sharedMesh.bindposes,
                                                                   handBindPose,
                                                                   handSide == UxrHandSide.Left ? handPoseAsset.HandDescriptorLeft : handPoseAsset.HandDescriptorRight,
                                                                   avatar.GetHand(handSide));

                _bonesClosed = new List<UxrPreviewHandBoneInfo>();
            }
            else if (handPoseAsset.PoseType == UxrHandPoseType.Blend)
            {
                _bones = UxrPreviewHandBoneInfo.CreateHandBoneData(skinnedMeshRenderer,
                                                                   skinnedMeshRenderer.sharedMesh.bindposes,
                                                                   handBindPose,
                                                                   handSide == UxrHandSide.Left ? handPoseAsset.HandDescriptorOpenLeft : handPoseAsset.HandDescriptorOpenRight,
                                                                   avatar.GetHand(handSide));

                _bonesClosed = UxrPreviewHandBoneInfo.CreateHandBoneData(skinnedMeshRenderer,
                                                                         skinnedMeshRenderer.sharedMesh.bindposes,
                                                                         handBindPose,
                                                                         handSide == UxrHandSide.Left ? handPoseAsset.HandDescriptorClosedLeft : handPoseAsset.HandDescriptorClosedRight,
                                                                         avatar.GetHand(handSide));
            }
        }

        /// <summary>
        ///     Called on different threads to compute the mesh.
        /// </summary>
        /// <param name="startIndex">Start vertex index to process</param>
        /// <param name="count">Number of vertices to process</param>
        private void ComputeMesh(int startIndex, int count)
        {
            void AddVertexWeight(ref Vector3 vertex, int vertexIndex, int boneIndex, float boneWeight)
            {
                if (boneWeight > 0.0f)
                {
                    vertex += _bones[boneIndex].CurrentTransform.MultiplyPoint(_initialVertices[vertexIndex]) * boneWeight;
                }
            }

            void AddNormalWeight(ref Vector3 normal, int vertexIndex, int boneIndex, float boneWeight)
            {
                if (boneWeight > 0.0f)
                {
                    normal += _bones[boneIndex].CurrentTransform.MultiplyVector(_initialNormals[vertexIndex]) * boneWeight;
                }
            }

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                Vector3 vertex = Vector3.zero;
                Vector3 normal = Vector3.zero;

                AddVertexWeight(ref vertex, i, _boneWeights[i].boneIndex0, _boneWeights[i].weight0);
                AddVertexWeight(ref vertex, i, _boneWeights[i].boneIndex1, _boneWeights[i].weight1);
                AddVertexWeight(ref vertex, i, _boneWeights[i].boneIndex2, _boneWeights[i].weight2);
                AddVertexWeight(ref vertex, i, _boneWeights[i].boneIndex3, _boneWeights[i].weight3);

                AddNormalWeight(ref normal, i, _boneWeights[i].boneIndex0, _boneWeights[i].weight0);
                AddNormalWeight(ref normal, i, _boneWeights[i].boneIndex1, _boneWeights[i].weight1);
                AddNormalWeight(ref normal, i, _boneWeights[i].boneIndex2, _boneWeights[i].weight2);
                AddNormalWeight(ref normal, i, _boneWeights[i].boneIndex3, _boneWeights[i].weight3);

                _vertices[i] = vertex;
                _normals[i]  = normal;
            }
        }

        #endregion

        #region Private Types & Data

        private Vector3[]                    _initialVertices;
        private Vector3[]                    _initialNormals;
        private Vector3[]                    _vertices;
        private Vector3[]                    _normals;
        private List<UxrPreviewHandBoneInfo> _bones;
        private List<UxrPreviewHandBoneInfo> _bonesClosed;
        private BoneWeight[]                 _boneWeights;
        private Dictionary<int, bool>        _arehandBones;

        #endregion
    }
}