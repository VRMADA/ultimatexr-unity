// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Prefabs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Checks if the given GameObject can be destroyed. GameObjects inside prefabs that are inherited from a parent prefab
        ///     cannot be destroyed.
        /// </summary>
        /// <param name="self">GameObject to check</param>
        /// <returns>Whether the GameObject can be destroyed</returns>
        public static bool CanBeDestroyed(this GameObject gameObject)
        {
            if (!gameObject.IsInPrefab())
            {
                return true;
            }

            if (gameObject.IsPrefabRoot())
            {
                return false;
            }

            GameObject source       = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            GameObject parentSource = PrefabUtility.GetCorrespondingObjectFromSource(gameObject.transform.parent.gameObject);

            if (source == null || parentSource == null)
            {
                return true;
            }

            return source.transform.parent != parentSource.transform;
        }

        /// <summary>
        ///     Gets the gameObject in the parent prefab if it exists.
        /// </summary>
        /// <param name="gameObject">GameObject to process</param>
        /// <param name="prefab">
        ///     The GameObject in the prefab it comes from. 3D models (.fbx files) are not considered prefabs by this method.
        /// </param>
        /// <returns>True if a prefab was found, false if not</returns>
        public static bool GetInParentPrefab(GameObject gameObject, out GameObject prefab)
        {
            prefab = null;

            if (gameObject == null)
            {
                return false;
            }

            prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            PrefabAssetType prefabAssetType = prefab != null ? PrefabUtility.GetPrefabAssetType(prefab) : PrefabAssetType.NotAPrefab;

            return prefab && (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant);
        }

        /// <summary>
        ///     Checks if the given GameObject is a prefab instance in a scene. 3D models do not count as prefabs.
        /// </summary>
        /// <param name="self">GameObject to check</param>
        /// <returns>Whether the GameObject is a prefab instance in a scene</returns>
        public static bool IsPrefabInstance(this GameObject gameObject)
        {
            if (gameObject.IsInPrefab())
            {
                return false;
            }

            GameObject      inParentPrefab  = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            PrefabAssetType prefabAssetType = inParentPrefab != null ? PrefabUtility.GetPrefabAssetType(inParentPrefab) : PrefabAssetType.NotAPrefab;

            return prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant;
        }

        /// <summary>
        ///     Gets the innermost prefab in a hierarchy.
        /// </summary>
        /// <param name="component">Component, from a prefab or instance in a scene</param>
        /// <param name="prefab">
        ///     Returns the root GameObject in the prefab the component comes from. If it's a nested prefab, it will return the
        ///     innermost one. If it's not a prefab, it will return null. 3D models (.fbx files) are not considered prefabs by this
        ///     method.
        /// </param>
        /// <param name="prefabInstance">
        ///     Returns the root GameObject of the instantiated <paramref name="prefab" />, in the scene or in a prefab.
        /// </param>
        /// <param name="componentInPrefab">
        ///     Returns the component in the prefab or null if it's not part of a prefab.
        /// </param>
        /// <returns>True if a prefab was found, false if not</returns>
        public static bool GetInnermostNon3DModelPrefabRoot<T>(T              component,
                                                               out GameObject prefab,
                                                               out GameObject prefabInstance,
                                                               out T          componentInPrefab) where T : Component
        {
            // Find component but in prefab hierarchy. CorrespondingObjectFromOriginalSource() gets the component in the root prefab hierarchy.

            prefab         = null;
            prefabInstance = null;

            componentInPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(component);

            if (componentInPrefab != null)
            {
                Transform current = componentInPrefab.transform;
                prefabInstance = component.gameObject;

                // Navigate to parent

                while (current.parent != null)
                {
                    current        = current.parent;
                    prefabInstance = prefabInstance.transform.parent.gameObject;
                }

                prefab = current.gameObject;
            }

            PrefabAssetType prefabAssetType = componentInPrefab != null ? PrefabUtility.GetPrefabAssetType(componentInPrefab) : PrefabAssetType.NotAPrefab;

            if (prefabAssetType == PrefabAssetType.Model)
            {
                // Solve case where prefab is model. We want the immediately superior parent prefab in the hierarchy

                T parentPrefabComponent    = PrefabUtility.GetCorrespondingObjectFromSource(component);
                T prefabComponentCandidate = null;

                if (parentPrefabComponent != null)
                {
                    // Navigate to the correct component in the last prefab parent before it's a model

                    while (PrefabUtility.GetPrefabAssetType(parentPrefabComponent) == PrefabAssetType.Regular ||
                           PrefabUtility.GetPrefabAssetType(parentPrefabComponent) == PrefabAssetType.Variant)
                    {
                        prefabComponentCandidate = parentPrefabComponent;
                        parentPrefabComponent    = PrefabUtility.GetCorrespondingObjectFromSource(parentPrefabComponent);
                    }

                    // Go up in the transform hierarchy to find prefab root

                    prefabInstance = component.gameObject;

                    if (prefabComponentCandidate != null)
                    {
                        componentInPrefab = prefabComponentCandidate;
                        prefab            = prefabComponentCandidate.transform.gameObject;

                        while (prefab.transform.parent != null)
                        {
                            prefab = prefab.transform.parent.gameObject;

                            if (prefabInstance != null && prefabInstance.transform.parent != null)
                            {
                                prefabInstance = prefabInstance.transform.parent.gameObject;
                            }
                        }

                        prefabAssetType = PrefabUtility.GetPrefabAssetType(prefab);
                    }
                }
            }

            return componentInPrefab && (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant);
        }

        /// <summary>
        ///     Gets the innermost prefab in a hierarchy that meets the requirements.
        /// </summary>
        /// <param name="component">Component, from a prefab or instance in a scene</param>
        /// <param name="requiredBasePath">
        ///     Allows to specify a project base path that the prefab needs to be in.
        ///     Even if there are inner prefabs, it will look for the innermost that still is within the base path.
        ///     Use null or empty to specify the whole project.
        /// </param>
        /// <param name="prefab">
        ///     Returns the root GameObject in the prefab the component comes from. If it's a nested prefab, it will return the
        ///     innermost one. If it's not a prefab, it will return null. 3D models (.fbx files) are not considered prefabs by this
        ///     method.
        /// </param>
        /// <param name="prefabInstance">
        ///     Returns the root GameObject of the instantiated <paramref name="prefab" />, in the scene or in a prefab.
        /// </param>
        /// <param name="componentInPrefab">
        ///     Returns the component in the prefab or null if it's not part of a prefab.
        /// </param>
        /// <returns>True if a prefab was found, false if not</returns>
        public static bool GetInnermostNon3DModelPrefabRoot<T>(T              component,
                                                               string         requiredBasePath,
                                                               bool           ignoreUltimateXRAssets,
                                                               out GameObject prefab,
                                                               out GameObject prefabInstance,
                                                               out T          componentInPrefab) where T : Component
        {
            if (string.IsNullOrEmpty(requiredBasePath))
            {
                return GetInnermostNon3DModelPrefabRoot(component, out prefab, out prefabInstance, out componentInPrefab);
            }

            bool IsValidComponent(T c)
            {
                return c != null &&
                       (PrefabUtility.GetPrefabAssetType(c) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(c) == PrefabAssetType.Variant) &&
                       PathRequiresProcessing(requiredBasePath, AssetDatabase.GetAssetPath(c), ignoreUltimateXRAssets);
            }
            
            // Traverse prefab chain looking for parent prefab that is still in valid base path:

            componentInPrefab = PrefabUtility.GetCorrespondingObjectFromSource(component);
            prefab            = null;
            prefabInstance    = component.gameObject;
            
            T lastValidComponentInPrefab = null;

            while (IsValidComponent(componentInPrefab))
            {
                lastValidComponentInPrefab = componentInPrefab;
                componentInPrefab          = PrefabUtility.GetCorrespondingObjectFromSource(componentInPrefab);

                prefab = componentInPrefab.gameObject;

                while (prefab.transform.parent != null)
                {
                    prefab         = prefab.transform.parent.gameObject;
                    prefabInstance = prefabInstance.transform.parent.gameObject;
                }
            }

            if (lastValidComponentInPrefab == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Prompt the user to save a prefab (or prefab variant) of the given avatar.
        /// </summary>
        /// <param name="avatar">Avatar to create prefab of</param>
        /// <param name="title">Text shown on the dialog title</param>
        /// <param name="defaultName">Default prefab file name</param>
        /// <param name="prefab">Returns the prefab GameObject if successful or null if canceled/error</param>
        /// <param name="newInstance">
        ///     If <paramref name="avatar" /> is a prefab instance in a scene, it will return a new instance
        ///     that substitutes the old one. The old instance will be deleted. If <paramref name="avatar" /> is a prefab it will
        ///     return null.
        /// </param>
        /// <returns>Whether the prefab was created successfully</returns>
        public static bool CreateAvatarPrefab(UxrAvatar avatar, string title, string defaultName, out GameObject prefab, out GameObject newInstance)
        {
            prefab      = null;
            newInstance = null;

            string path = EditorUtility.SaveFilePanelInProject(title, defaultName, "prefab", "Please select the file in your project to save the prefab to");

            if (!string.IsNullOrEmpty(path))
            {
                if (PathIsInUltimateXR(path))
                {
                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "The file location can't be inside the UltimateXR framework to prevent it from being deleted", UxrConstants.Editor.Ok);
                }
                else if (!PathIsInCurrentProject(path))
                {
                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "The file location can't be outside the project's Assets folder", UxrConstants.Editor.Ok);
                }
                else
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, path, out bool success);

                    if (!success || prefab == null)
                    {
                        EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "There was an error generating the variant", UxrConstants.Editor.Ok);
                    }
                    else
                    {
                        prefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        prefab.transform.localScale = Vector3.one;

                        if (avatar.gameObject.CanBeDestroyed())
                        {
                            newInstance = PrefabUtility.InstantiatePrefab(prefab, avatar.transform.parent) as GameObject;

                            if (newInstance != null)
                            {
                                newInstance.name = avatar.name;
                                newInstance.transform.SetPositionAndRotation(avatar.transform);
                                newInstance.transform.localScale = avatar.transform.localScale;
                                newInstance.transform.SetSiblingIndex(avatar.transform.GetSiblingIndex());

                                Object.DestroyImmediate(avatar.gameObject);
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}