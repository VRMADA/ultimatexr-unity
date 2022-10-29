// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameObjectExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Manipulation;
using UltimateXR.UI.UnityInputModule;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     <see cref="GameObject" /> extensions.
    /// </summary>
    public static class GameObjectExt
    {
        #region Public Methods

        /// <summary>
        ///     Gets the Component of a given type in the GameObject or any of its parents. It also works on prefabs, where regular
        ///     <see cref="Component.GetComponentInParent" /> will not work:
        ///     https://issuetracker.unity3d.com/issues/getcomponentinparent-is-returning-null-when-the-gameobject-is-a-prefab
        /// </summary>
        /// <typeparam name="T">Component type to get</typeparam>
        /// <returns>Component in same GameObject or any of its parents. Null if it wasn't found</returns>
        public static T SafeGetComponentInParent<T>(this GameObject self)
        {
            return self.GetComponentInParent<T>() ?? self.GetComponentsInParent<T>(true).FirstOrDefault();
        }

        /// <summary>
        ///     Activates/deactivates the object if it isn't active already.
        /// </summary>
        /// <param name="self">GameObject to activate</param>
        /// <param name="activate">Whether to activate or deactivate the object</param>
        public static void CheckSetActive(this GameObject self, bool activate)
        {
            if (self.activeSelf != activate)
            {
                self.SetActive(activate);
            }
        }

        /// <summary>
        ///     Gets a unique path in the scene for the given GameObject. It will include sibling indices to make it unique.
        /// </summary>
        /// <param name="self">GameObject to get the unique path for</param>
        /// <returns>Unique GameObject path string</returns>
        /// <seealso cref="TransformExt.GetUniqueScenePath" />
        public static string GetUniqueScenePath(this GameObject self)
        {
            self.ThrowIfNull(nameof(self));
            return self.transform.GetUniqueScenePath();
        }

        /// <summary>
        ///     Gets the path in the scene of the given GameObject.
        /// </summary>
        /// <param name="self">GameObject to get the scene path for</param>
        /// <returns>Path of the GameObject in the scene</returns>
        /// <seealso cref="TransformExt.GetPathUnderScene" />
        public static string GetPathUnderScene(this GameObject self)
        {
            self.ThrowIfNull(nameof(self));
            return self.transform.GetPathUnderScene();
        }

        /// <summary>
        ///     Checks if the given GameObject is a prefab.
        /// </summary>
        /// <param name="self">GameObject to check</param>
        /// <returns>Whether the GameObject is a prefab or not</returns>
        public static bool IsPrefab(this GameObject self)
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot == self)
            {
                return true;
            }
#endif

            return self.scene.name == null;
        }

        /// <summary>
        ///     Checks whether the given GameObject is dynamic. Since <see cref="GameObject.isStatic" /> doesn't work at runtime
        ///     due to the static flags being editor-only, a workaround is required to try to find out if an object is dynamic or
        ///     not.
        /// </summary>
        /// <param name="self">GameObject to check</param>
        /// <returns>Whether the object appears to be dynamic</returns>
        public static bool IsDynamic(this GameObject self)
        {
            return self.GetComponentInParent<Rigidbody>() != null ||
                   self.GetComponentInParent<SkinnedMeshRenderer>() != null ||
                   self.GetComponentInParent<UxrAvatar>() != null ||
                   self.GetComponentInParent<UxrGrabbableObject>() != null;
        }

        /// <summary>
        ///     Gets the Component of a given type. If it doesn't exist, it is added to the GameObject.
        /// </summary>
        /// <param name="self">Target GameObject where the component will be looked for and added to if it doesn't exist</param>
        /// <typeparam name="T">Component type to get or add</typeparam>
        /// <returns>Existing component or newly added if it didn't exist before</returns>
        public static T GetOrAddComponent<T>(this GameObject self)
                    where T : Component
        {
            T component = self.GetComponent<T>();

            if (component == null)
            {
                component = self.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        ///     Creates a new GameObject in the exact same position as the given one and parents it.
        /// </summary>
        /// <param name="parent">
        ///     GameObject to parent the new object to and also place it at the same position
        ///     and with the same orientation
        /// </param>
        /// <param name="newGameObjectName">Name for the new GameObject</param>
        /// <returns>New created GameObject</returns>
        public static GameObject CreateGameObjectAndParentSameTransform(GameObject parent, string newGameObjectName)
        {
            GameObject newGameObject = new GameObject(newGameObjectName);
            newGameObject.transform.parent        = parent.transform;
            newGameObject.transform.localPosition = Vector3.zero;
            newGameObject.transform.localRotation = Quaternion.identity;
            return newGameObject;
        }

        /// <summary>
        ///     Computes the geometric center of the given GameObject based on all the MeshRenderers in the hierarchy.
        /// </summary>
        /// <param name="self">GameObject to compute the geometric center of</param>
        /// <returns>Geometric center</returns>
        public static Vector3 GetGeometricCenter(this GameObject self)
        {
            MeshRenderer[] meshRenderers = self.GetComponentsInChildren<MeshRenderer>();
            bool           initialized   = false;
            Vector3        min           = Vector3.zero;
            Vector3        max           = Vector3.zero;

            if (meshRenderers.Length == 0)
            {
                return Vector3.zero;
            }

            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (!initialized)
                {
                    initialized = true;
                    min         = renderer.bounds.min;
                    max         = renderer.bounds.max;
                }
                else
                {
                min = Vector3.Min(min, renderer.bounds.min);
                max = Vector3.Max(max, renderer.bounds.max);
            }
            }

            return (min + max) * 0.5f;
        }

        /// <summary>
        ///     Calculates the <see cref="GameObject" /> <see cref="Bounds" />. The bounds are the <see cref="Renderer" />'s bounds
        ///     if there is one in the GameObject. Otherwise it will encapsulate all renderers found in the children.
        ///     If <paramref name="forceRecurseIntoChildren" /> is true, it will also encapsulate all renderers found in
        ///     the children no matter if the GameObject has a Renderer component or not.
        /// </summary>
        /// <param name="self">The GameObject whose <see cref="Bounds" /> to get</param>
        /// <param name="forceRecurseIntoChildren">
        ///     Whether to also encapsulate all renderers found in the children no matter if the
        ///     GameObject has a Renderer component or not
        /// </param>
        /// <returns>
        ///     <see cref="Bounds" /> in world-space.
        /// </returns>
        public static Bounds GetBounds(this GameObject self, bool forceRecurseIntoChildren)
        {
            Renderer renderer = self.GetComponent<Renderer>();

            if (renderer != null)
            {
                return renderer.bounds;
            }

            IEnumerable<Renderer> renderers = self.GetComponentsInChildren<Renderer>().Where(r => !r.hideFlags.HasFlag(HideFlags.HideInHierarchy));

            if (!renderers.Any())
            {
                return new Bounds(self.transform.position, Vector3.zero);
            }

            Vector3 min = Vector3Ext.Min(renderers.Select(r => r.bounds.min));
            Vector3 max = Vector3Ext.Max(renderers.Select(r => r.bounds.max));

            return new Bounds((max + min) * 0.5f, max - min);
        }

        /// <summary>
        ///     Calculates the <see cref="GameObject" /> <see cref="Bounds" /> in local space. The bounds are the
        ///     <see cref="Renderer" />'s bounds if there is one in the GameObject. Otherwise it will encapsulate all renderers
        ///     found in the children.
        ///     If <paramref name="forceRecurseIntoChildren" /> is true, it will also encapsulate all renderers found in
        ///     the children no matter if the GameObject has a Renderer component or not.
        /// </summary>
        /// <param name="self">The GameObject whose local <see cref="Bounds" /> to get</param>
        /// <param name="forceRecurseIntoChildren">
        ///     Whether to also encapsulate all renderers found in the children no matter if the
        ///     GameObject has a Renderer component or not
        /// </param>
        /// <returns>
        ///     Local <see cref="Bounds" />.
        /// </returns>
        public static Bounds GetLocalBounds(this GameObject self, bool forceRecurseIntoChildren)
        {
            Renderer renderer = self.GetComponent<Renderer>();

            if (renderer != null && !forceRecurseIntoChildren)
            {
                return renderer.localBounds;
            }

            IEnumerable<Renderer> renderers = self.GetComponentsInChildren<Renderer>().Where(r => !r.hideFlags.HasFlag(HideFlags.HideInHierarchy));

            if (!renderers.Any())
            {
                return new Bounds();
            }

            IEnumerable<Vector3> allMinMaxToLocal = renderers.Select(r => self.transform.InverseTransformPoint(r.transform.TransformPoint(r.localBounds.min))).Concat(renderers.Select(r => self.transform.InverseTransformPoint(r.transform.TransformPoint(r.localBounds.max))));
            Vector3     min       = Vector3Ext.Min(allMinMaxToLocal);
            Vector3     max       = Vector3Ext.Max(allMinMaxToLocal);

            return new Bounds((max + min) * 0.5f, max - min);
        }

        /// <summary>
        ///     Sets the layer of a GameObject and all its children.
        /// </summary>
        /// <param name="self">The root GameObject from where to start</param>
        /// <param name="layer">The layer value to assign</param>
        public static void SetLayerRecursively(this GameObject self, int layer)
        {
            if (self != null)
            {
                Transform selfTransform = self.transform;

                self.gameObject.layer = layer;

                for (int i = 0; i < selfTransform.childCount; ++i)
                {
                    SetLayerRecursively(selfTransform.GetChild(i).gameObject, layer);
                }
            }
        }

        /// <summary>
        ///     Checks whether the given GameObject's layer is present in a layer mask.
        /// </summary>
        /// <param name="self">The GameObject whose layer to check</param>
        /// <param name="layerMask">The layer mask to check against</param>
        /// <returns>Whether the GameObject's layer is present in the layer mask</returns>
        public static bool IsInLayerMask(this GameObject self, LayerMask layerMask)
        {
            return (1 << self.layer & layerMask.value) != 0;
        }

        /// <summary>
        ///     Gets the topmost <see cref="UxrCanvas" /> upwards in the hierarchy if it exists.
        /// </summary>
        /// <param name="self">The GameObject whose parents to look for</param>
        /// <returns>The topmost <see cref="UxrCanvas" /> component upwards in the hierarchy or null if it doesn't exists</returns>
        public static UxrCanvas GetTopmostCanvas(this GameObject self)
        {
            UxrCanvas[] canvases = self.GetComponentsInParent<UxrCanvas>();
            return ComponentExt.GetCommonRootComponentFromSet(canvases);
        }

        #endregion
    }
}