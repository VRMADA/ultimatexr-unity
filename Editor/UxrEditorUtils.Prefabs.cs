// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Prefabs.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Gets the prefab of an instance in the hierarchy.
        /// </summary>
        /// <param name="gameObject">GameObject in a scene</param>
        /// <param name="prefab">
        ///     The prefab the GameObject comes from. If it's not a prefab, it will get null. 3D models (.fbx files) are not
        ///     considered prefabs by this method.
        /// </param>
        /// <returns>True if a prefab was found, false if not</returns>
        public static bool GetPrefab(GameObject gameObject, out GameObject prefab)
        {
            prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            return prefab && (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant);
        }

        /// <summary>
        ///     Gets the innermost prefab in a hierarchy.
        /// </summary>
        /// <param name="gameObject">GameObject, prefab or instance in a scene</param>
        /// <param name="prefab">
        ///     Will return the prefab the GameObject comes from. If it's a nested prefab, it will return the innermost one. If
        ///     it's not a prefab, it will get null. 3D models (.fbx files) are not considered prefabs by this method.
        /// </param>
        /// <param name="prefabInstance">
        ///     Will return the instance of the prefab in the scene, if <paramref name="gameObject" /> is
        ///     an instance in a scene. If it's a prefab it will return null
        /// </param>
        /// <returns>True if a prefab was found, false if not</returns>
        public static bool GetInnermostNon3DModelPrefabRoot(GameObject gameObject, out GameObject prefab, out GameObject prefabInstance)
        {
            // Find gameObject but in prefab hierarchy. CorrespondingObjectFromOriginalSource() gets the GameObject in the root prefab hierarchy.

            prefab         = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            prefabInstance = gameObject;

            // Now travel up to the prefab root if we aren't there already. We do the same with the prefabInstance to travel to the instance in the scene.

            while (prefab != null && prefab.transform.parent != null && prefabInstance.transform.parent != null)
            {
                prefab         = prefab.transform.parent.gameObject;
                prefabInstance = prefabInstance.transform.parent.gameObject;
            }

            // If gameObject was part of a prefab and not instantiated in the scene, set prefabInstance to null because it references a gameObject in a prefab hierarchy.

            if (gameObject.IsPrefab())
            {
                prefabInstance = null;
            }

            if (prefab != null)
            {
                // Debug.Log($"Root prefab of {prefabInstance.name} is {prefab.name}. Prefab type is {PrefabUtility.GetPrefabAssetType(prefab)}.");
            }

            return prefab && (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant);
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
                    EditorUtility.DisplayDialog("Error", "The file location can't be inside the UltimateXR framework to prevent it from being deleted", "OK");
                }
                else if (!PathIsInCurrentProject(path))
                {
                    EditorUtility.DisplayDialog("Error", "The file location can't be outside the project's Assets folder", "OK");
                }
                else
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, path, out bool success);

                    if (!success || prefab == null)
                    {
                        EditorUtility.DisplayDialog("Error", "There was an error generating the variant", "OK");
                    }
                    else
                    {
                        prefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        prefab.transform.localScale = Vector3.one;

                        if (!avatar.gameObject.IsPrefab())
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