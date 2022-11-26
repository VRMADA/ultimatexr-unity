// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.ComponentProcessing.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Processes all components in a project:
        ///     <list type="bullet">
        ///         <item>Components in the currently open scenes</item>
        ///         <item>Components in other scenes in the project</item>
        ///         <item>Components in the project's prefabs</item>
        ///     </list>
        ///     Each component is guaranteed to be processed only once. If the component is instantiated in a scene through a
        ///     prefab, only the prefab asset will be processed and not the instance.
        ///     If the component is in a nested prefab, only the innermost prefab will be processed.
        /// </summary>
        /// <typeparam name="T">The type of the component to process</typeparam>
        /// <param name="basePath">
        ///     Base path of scenes and prefabs to process. Use null or empty to process the whole project. If
        ///     using a base path, it should start with Assets/
        /// </param>
        /// <param name="componentProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        public static void ProcessAllProjectComponents<T>(string basePath, UxrComponentProcessor<T> componentProcessor, UxrProgressUpdater progressUpdater) where T : Component
        {
            // Open scenes

            List<Scene> openScenes = new List<Scene>();

            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (PathRequiresProcessing(basePath, scene.path))
                {
                    ProcessAllNonPrefabGameObjects(scene, componentProcessor, progressUpdater);
                }

                openScenes.Add(scene);
            }

            // Get all asset files

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            // Process non-open scenes

            foreach (string scenePath in allAssetPaths.Where(path => path.EndsWith(".unity"))) // path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Scene)))
            {
                if (openScenes.All(s => s.path != scenePath) && PathRequiresProcessing(basePath, scenePath))
                {
                    try
                    {
                        progressUpdater?.Invoke(new UxrProgressInfo("Opening scene", scenePath, 0.0f));

                        Scene scene        = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                        bool  processedAny = ProcessAllNonPrefabGameObjects(scene, componentProcessor, progressUpdater);

                        progressUpdater?.Invoke(new UxrProgressInfo("Closing scene", scene.name, 1.0f));

                        if (processedAny)
                        {
                            EditorSceneManager.SaveScene(scene);
                        }

                        EditorSceneManager.CloseScene(scene, true);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Error opening scene {scenePath}. Skipping...");
                    }
                }
            }

            // Process prefabs in project

            for (int i = 0; i < allAssetPaths.Length; ++i)
            {
                string prefabPath = allAssetPaths[i];

                if (AssetDatabase.GetMainAssetTypeAtPath(prefabPath) == typeof(GameObject) && PathRequiresProcessing(basePath, prefabPath))
                {
                    Object     assetObject = AssetDatabase.LoadMainAssetAtPath(prefabPath);
                    GameObject gameObject  = assetObject as GameObject;
                    bool       processed   = false;

                    // Get all components of the given type in the prefab

                    T[] components = gameObject.GetComponentsInChildren<T>();

                    foreach (T component in components)
                    {
                        // Only process those components that belong to the prefab root, ignoring nested prefabs. This ensures two things:
                        // 1) Processing each component only once.
                        // 2) Modifying components in the innermost prefab to store changes correctly. 

                        if (GetInnermostNon3DModelPrefabRoot(component.gameObject, out GameObject prefab, out GameObject prefabInstance) && prefab == gameObject)
                        {
                            progressUpdater?.Invoke(new UxrProgressInfo("Processing prefabs", $"Prefab {prefab.name}", (float)i / allAssetPaths.Length));

                            if (componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(component, prefab)))
                            {
                                processed = true;
                            }
                        }
                    }

                    if (processed)
                    {
                        PrefabUtility.SavePrefabAsset(gameObject);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether the given asset path requires processing.
        /// </summary>
        /// <param name="basePath">Base path to process. Null or empty to process all paths</param>
        /// <param name="assetPath">Asset path to check if it should be processed</param>
        /// <returns>Whether <see cref="assetPath" /> should be processed</returns>
        private static bool PathRequiresProcessing(string basePath, string assetPath)
        {
            return string.IsNullOrEmpty(basePath) || assetPath.StartsWith(basePath);
        }

        /// <summary>
        ///     Processes all components of a given type in a scene only in gameObjects that are not prefab instances.
        /// </summary>
        /// <typeparam name="T">Component type to process</typeparam>
        /// <param name="scene">Scene to process</param>
        /// <param name="componentProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        ///     Here the GameObject that will be given as parameter is the GameObject that has the given component.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        /// <returns>True if there was at least one component that was modified</returns>
        private static bool ProcessAllNonPrefabGameObjects<T>(Scene scene, UxrComponentProcessor<T> componentProcessor, UxrProgressUpdater progressUpdater) where T : Component
        {
            List<T> components   = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>()).ToList();
            bool    processedAny = false;

            for (int c = 0; c < components.Count; ++c)
            {
                if (!GetInnermostNon3DModelPrefabRoot(components[c].gameObject, out GameObject prefab, out GameObject prefabInstance))
                {
                    progressUpdater?.Invoke(new UxrProgressInfo($"Processing scene {scene.name}", $"Object {components[c].name}", (float)c / components.Count));

                    if (componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(components[c], null)))
                    {
                        processedAny = true;
                    }
                }
            }

            return processedAny;
        }

        #endregion
    }
}