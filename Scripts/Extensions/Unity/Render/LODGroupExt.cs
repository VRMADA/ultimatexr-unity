// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LODGroupExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Render
{
    /// <summary>
    ///     <see cref="LODGroup" /> extensions.
    ///     Most functionality has been copied from:
    ///     https://github.com/JulienHeijmans/EditorScripts/blob/master/Scripts/Utility/Editor/LODExtendedUtility.cs, which
    ///     in turn copied functionality from:
    ///     https://github.com/Unity-Technologies/AutoLOD/blob/master/Scripts/Extensions/LODGroupExtensions.cs
    /// </summary>
    public static class LODGroupExt
    {
        #region Public Methods

        /// <summary>
        ///     Gets the LOD level index that should be enabled from a specific view.
        /// </summary>
        /// <param name="lodGroup">Component to check</param>
        /// <param name="camera">Camera to use as point of view</param>
        /// <returns>LOD level index that should be enabled</returns>
        public static int GetVisibleLevel(this LODGroup lodGroup, Camera camera)
        {
            var lods           = lodGroup.GetLODs();
            var relativeHeight = GetRelativeHeight(lodGroup, camera);

            int lodIndex = lodGroup.lodCount - 1;

            for (var i = 0; i < lods.Length; i++)
            {
                var lod = lods[i];

                if (relativeHeight >= lod.screenRelativeTransitionHeight)
                {
                    lodIndex = i;
                    break;
                }
            }

            return lodIndex;
        }

        /// <summary>
        ///     Manually enables all renderers belonging to a LOD level.
        /// </summary>
        /// <param name="lodGroup">Component to process</param>
        /// <param name="level">Level whose renderers to enable</param>
        public static void EnableLevelRenderers(this LODGroup lodGroup, int level)
        {
            var lods = lodGroup.GetLODs();

            for (var i = 0; i < lods.Length; i++)
            {
                foreach (Renderer renderer in lods[i].renderers)
                {
                    renderer.enabled = i == level;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes the relative height in the camera view.
        /// </summary>
        /// <param name="lodGroup">Component to check</param>
        /// <param name="camera">Camera to use as point of view</param>
        /// <returns>Relative height</returns>
        private static float GetRelativeHeight(LODGroup lodGroup, Camera camera)
        {
            var distance = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint) - camera.transform.position).magnitude;
            return DistanceToRelativeHeight(camera, distance / QualitySettings.lodBias, GetWorldSpaceSize(lodGroup));
        }

        /// <summary>
        ///     Computes the relative height in the camera view.
        /// </summary>
        /// <param name="camera">Camera to use as point of view</param>
        /// <param name="distance">Distance to the camera</param>
        /// <param name="size">Largest axis in world-space</param>
        /// <returns>Relative height</returns>
        private static float DistanceToRelativeHeight(Camera camera, float distance, float size)
        {
            if (camera.orthographic)
            {
                return size * 0.5F / camera.orthographicSize;
            }

            var halfAngle      = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5F);
            var relativeHeight = size * 0.5F / (distance * halfAngle);
            return relativeHeight;
        }

        /// <summary>
        ///     Computes the largest axis of the <see cref="LODGroup" /> in world-space.
        /// </summary>
        /// <param name="lodGroup">Component to process</param>
        /// <returns>World space size</returns>
        private static float GetWorldSpaceSize(LODGroup lodGroup)
        {
            return GetWorldSpaceScale(lodGroup.transform) * lodGroup.size;
        }

        /// <summary>
        ///     Computes the largest scale axis.
        /// </summary>
        /// <param name="transform">Transform to get the largest scale of</param>
        /// <returns>Largest scale axis</returns>
        private static float GetWorldSpaceScale(Transform transform)
        {
            var   scale       = transform.lossyScale;
            float largestAxis = Mathf.Abs(scale.x);
            largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.y));
            largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.z));
            return largestAxis;
        }

        #endregion
    }
}