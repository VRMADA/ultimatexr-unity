// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.ComponentProcessing.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Extensions.Unity;
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
        ///     Modifies a component in a prefab or prefab instance, allowing to apply modifications along the prefab variant chain
        ///     and original prefab if they exist.
        ///     The method is guaranteed to process prefabs in an order where the source prefab is processed first and then all
        ///     child prefabs down to the outermost prefab.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="componentOrGameObject">
        ///     Component or GameObject to process. It can be an instance in the scene or a
        ///     component/GameObject in a prefab
        /// </param>
        /// <param name="options">Component processing options (flags)</param>
        /// <param name="componentProcessor">Processor that modifies the component</param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity
        ///     progress bar
        /// </param>
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns>Whether any modifications were made</returns>
        public static bool ModifyComponent<T>(Object                        componentOrGameObject,
                                              UxrComponentProcessingOptions options,
                                              UxrComponentProcessor<T>      componentProcessor,
                                              UxrProgressUpdater            progressUpdater,
                                              out bool                      canceled,
                                              bool                          onlyCheck = false) where T : Component

        {
            canceled = false;

            if (componentOrGameObject == null)
            {
                return false;
            }

            // Gather component(s) to process

            T[]  components     = null;
            bool sourceIsPrefab = false;

            if (componentOrGameObject is GameObject sourceGameObject)
            {
                components     = options.HasFlag(UxrComponentProcessingOptions.RecurseIntoChildren) ? sourceGameObject.GetComponentsInChildren<T>(true) : sourceGameObject.GetComponents<T>();
                sourceIsPrefab = sourceGameObject.IsInPrefab();
            }
            else if (componentOrGameObject is T sourceComponent)
            {
                components     = new[] { sourceComponent };
                sourceIsPrefab = sourceComponent.IsInPrefab();
            }
            else
            {
                Debug.LogError($"{nameof(ModifyComponent)}: Cannot process type {componentOrGameObject.GetType().Name}, expected {nameof(GameObject)} or component of type {typeof(T)}.");
                return false;
            }

            // Gather all prefabs to process, if any, and for each prefab all its components to process

            Dictionary<string, List<Component>> prefabComponents = new Dictionary<string, List<Component>>();

            for (var i = 0; i < components.Length; i++)
            {
                T c = components[i];

                if (c == null)
                {
                    continue;
                }

                if (progressUpdater != null)
                {
                    canceled = progressUpdater.Invoke(new UxrProgressInfo("Preprocessing components", $"Object {c.name}, Component {c.GetType().Name} ", (float)i / components.Length));

                    if (canceled)
                    {
                        return false;
                    }
                }

                // For each component, iterate over all components in variant prefab chain starting from the original prefab, so that changes get propagated in the correct order

                T componentInChain = c;

                if (!sourceIsPrefab)
                {
                    // Component is instance. Get component in prefab to start.
                    componentInChain = PrefabUtility.GetCorrespondingObjectFromSource(c);
                }

                while (componentInChain != null)
                {
                    if (!options.HasFlag(UxrComponentProcessingOptions.RecurseIntoPrefabs) && !sourceIsPrefab)
                    {
                        break;
                    }

                    T      componentInParentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(componentInChain);
                    bool   isOriginalPrefab        = componentInParentPrefab == null;
                    string assetPath               = AssetDatabase.GetAssetPath(componentInChain.transform.root.gameObject);
                    bool process = (isOriginalPrefab && options.HasFlag(UxrComponentProcessingOptions.ProcessOriginalPrefabComponents)) ||
                                   (!isOriginalPrefab && options.HasFlag(UxrComponentProcessingOptions.ProcessNonOriginalPrefabComponents));

                    if (process)
                    {
                        if (!ShouldIgnoreUltimateXRPath(assetPath, !options.HasFlag(UxrComponentProcessingOptions.ProcessUltimateXRAssetComponents)))
                        {
                            // Initialize

                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                if (!prefabComponents.ContainsKey(assetPath))
                                {
                                    prefabComponents.Add(assetPath, new List<Component>());
                                }

                                // Add if it was not added already through another instance chain

                                if (!prefabComponents[assetPath].Contains(componentInChain))
                                {
                                    prefabComponents[assetPath].Add(componentInChain);
                                }
                            }
                        }
                    }

                    componentInChain = componentInParentPrefab;

                    if (!options.HasFlag(UxrComponentProcessingOptions.RecurseIntoPrefabs) && sourceIsPrefab)
                    {
                        break;
                    }
                }
            }

            // Get sorted prefab list and process prefabs in correct order

            List<string> sortedPrefabPaths = GetSortedPrefabProcessingPaths<T>(prefabComponents.Keys, !options.HasFlag(UxrComponentProcessingOptions.ProcessUltimateXRAssetComponents));

            bool processedAny = false;

            for (var i = 0; i < sortedPrefabPaths.Count; i++)
            {
                string prefabPath = sortedPrefabPaths[i];

                // Filter out paths that are dependencies but are not included in the list that we built because they are not being processed

                if (prefabComponents.ContainsKey(prefabPath))
                {
                    GameObject prefab    = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
                    bool       processed = false;

                    if (progressUpdater != null)
                    {
                        canceled = progressUpdater.Invoke(new UxrProgressInfo("Processing components", $"Prefab {prefab.name}", (float)i / components.Length));

                        if (canceled)
                        {
                            return processedAny;
                        }
                    }

                    foreach (T component in prefabComponents[prefabPath])
                    {
                        bool isOriginalSource = PrefabUtility.GetCorrespondingObjectFromSource(component) == null;

                        if (componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(component, prefab, isOriginalSource, isOriginalSource), onlyCheck))
                        {
                            processed    = true;
                            processedAny = true;
                        }
                    }

                    if (processed && !onlyCheck)
                    {
                        PrefabUtility.SavePrefabAsset(prefab);
                    }
                }
            }

            // End with instances if there are any

            if (!sourceIsPrefab)
            {
                for (var i = 0; i < components.Length; i++)
                {
                    T c = components[i];

                    if (c == null)
                    {
                        continue;
                    }

                    if (progressUpdater != null)
                    {
                        canceled = progressUpdater.Invoke(new UxrProgressInfo("Processing components", $"Object {c.name}, Component {c.GetType().Name} ", (float)i / components.Length));

                        if (canceled)
                        {
                            return processedAny;
                        }
                    }

                    bool process = (c.gameObject.IsPrefabInstance() && options.HasFlag(UxrComponentProcessingOptions.ProcessPrefabSceneComponents)) ||
                                   (!c.gameObject.IsPrefabInstance() && options.HasFlag(UxrComponentProcessingOptions.ProcessOriginalSceneComponents));

                    if (c.gameObject.scene.name != null)
                    {
                        if (ShouldIgnoreUltimateXRPath(c.gameObject.scene.path, !options.HasFlag(UxrComponentProcessingOptions.ProcessUltimateXRAssetComponents)))
                        {
                            process = false;
                        }
                    }

                    bool isOriginalSource = !c.gameObject.IsPrefabInstance();

                    if (process && componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(c, null, isOriginalSource, isOriginalSource), onlyCheck))
                    {
                        processedAny = true;

                        if (!onlyCheck)
                        {
                            EditorUtility.SetDirty(c);
                        }
                    }
                }
            }

            return processedAny;
        }

        /// <summary>
        ///     Processes all components in a project:
        ///     <list type="bullet">
        ///         <item>Components in the currently open scenes</item>
        ///         <item>Components in other scenes in the project</item>
        ///         <item>Components in the project's prefabs</item>
        ///     </list>
        ///     Each component can be guaranteed to be processed only once by checking
        ///     <see cref="UxrComponentInfo{T}.IsOriginalSource" />.
        ///     It is true for:
        ///     <list type="bullet">
        ///         <item>Components in the scene that are not instantiated from a prefab</item>
        ///         <item>Components inside the original prefab</item>
        ///     </list>
        ///     And false for
        ///     <list type="bullet">
        ///         <item>Components in the scene that are instantiated from a prefab</item>
        ///         <item>Components inside prefabs that are inherited from another source prefab</item>
        ///     </list>
        ///     When processing components inside prefabs, <see cref="UxrComponentInfo{T}.TargetPrefab" /> will tell which prefab
        ///     is being processed.
        ///     Also, components are processed in an order that, in prefabs, guarantees that parent prefabs are processed first,
        ///     starting from the source prefab downwards to the outermost prefab.
        ///     This ensures that changes in a prefab that depend on parent prefabs will always get the updated inherited value in
        ///     the current prefab.
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
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns>Whether any modifications were made</returns>
        public static bool ProcessAllProjectComponents<T>(string                   basePath,
                                                          UxrComponentProcessor<T> componentProcessor,
                                                          UxrProgressUpdater       progressUpdater,
                                                          out bool                 canceled,
                                                          bool                     ignoreUltimateXRAssets = false,
                                                          bool                     onlyCheck              = false) where T : Component
        {
            // Process prefabs in project in best order

            bool processedAny = ProcessPrefabs(GetSortedPrefabProcessingPaths<T>(basePath, ignoreUltimateXRAssets), basePath, ignoreUltimateXRAssets, true, true, componentProcessor, progressUpdater, out canceled, onlyCheck);

            if (canceled)
            {
                return processedAny;
            }

            // Open scenes

            List<Scene> openScenes = new List<Scene>();

            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (PathRequiresProcessing(basePath, scene.path, ignoreUltimateXRAssets))
                {
                    if (ProcessAllNonPrefabInstanceGameObjects(scene, componentProcessor, progressUpdater, out canceled, onlyCheck))
                    {
                        processedAny = true;
                    }

                    if (canceled)
                    {
                        return processedAny;
                    }

                    if (ProcessAllInnermostPrefabInstanceGameObjects(scene, basePath, ignoreUltimateXRAssets, componentProcessor, progressUpdater, out canceled, onlyCheck))
                    {
                        processedAny = true;
                    }

                    if (canceled)
                    {
                        return processedAny;
                    }
                }

                openScenes.Add(scene);
            }

            // Process non-open scenes

            foreach (string scenePath in GetAllAssetPathsExceptPackages().Where(path => path.EndsWith(".unity")))
            {
                if (openScenes.All(s => s.path != scenePath) && PathRequiresProcessing(basePath, scenePath, ignoreUltimateXRAssets))
                {
                    Scene scene = default;
                    
                    try
                    {
                        if (progressUpdater != null)
                        {
                            canceled = progressUpdater.Invoke(new UxrProgressInfo("Opening scene", scenePath, 0.0f));

                            if (canceled)
                            {
                                return processedAny;
                            }
                        }

                        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                        if (ProcessAllNonPrefabInstanceGameObjects(scene, componentProcessor, progressUpdater, out canceled, onlyCheck))
                        {
                            processedAny = true;
                        }

                        if (canceled)
                        {
                            return processedAny;
                        }

                        if (ProcessAllInnermostPrefabInstanceGameObjects(scene, basePath, ignoreUltimateXRAssets, componentProcessor, progressUpdater, out canceled, onlyCheck))
                        {
                            processedAny = true;
                        }

                        if (canceled)
                        {
                            return processedAny;
                        }

                        if (progressUpdater != null)
                        {
                            canceled = progressUpdater.Invoke(new UxrProgressInfo("Closing scene", scene.name, 1.0f));

                            if (canceled)
                            {
                                return processedAny;
                            }
                        }

                        if (processedAny && !onlyCheck)
                        {
                            EditorSceneManager.SaveScene(scene);
                        }

                        EditorSceneManager.CloseScene(scene, true);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Error opening scene {scenePath}. Skipping...");

                        if (scene != default)
                        {
                            EditorSceneManager.CloseScene(scene, true);
                        }
                    }
                }
            }

            if (!onlyCheck)
            {
                AssetDatabase.SaveAssets();
                Resources.UnloadUnusedAssets();
            }

            return processedAny;
        }

        /// <summary>
        ///     Same as <see cref="ProcessAllProjectComponents{T}" /> with some key aspects:
        ///     <list type="bullet">
        ///         <item>
        ///             Instead of processing a path looking for scenes, the scene paths will be provided as a parameter.
        ///         </item>
        ///         <item>
        ///             From the scenes, components that do not belong to a prefab instance will be processed.
        ///             Additionally, all prefabs that have instances in the scene will be processed, including all prefabs up in
        ///             the prefab chain. Only prefabs instantiated in the scene that are in  <paramref name="basePath" /> will be
        ///             processed.
        ///         </item>
        ///         <item>
        ///             If <paramref name="processBasePath" /> is true, all prefab assets in <see cref="basePath" /> will be
        ///             processed too. If any is instantiated in a scene processed from <see cref="scenePaths" />,
        ///             it will be processed only once.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="scenePaths">Scene paths to process</param>
        /// <param name="processBasePath">Whether to process all prefab assets in <see cref="basePath" /></param>
        /// <param name="basePath">
        ///     Base path to process looking for prefabs. In addition, prefabs in any scene from
        ///     <paramref name="scenePaths" /> will be processed only if they are located in this base path.
        ///     If the base path is null or empty, the whole project is considered
        /// </param>
        /// <param name="componentProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns>Whether any modifications were made</returns>
        public static bool ProcessScenesAndProjectPathPrefabs<T>(IEnumerable<string>      scenePaths,
                                                                 bool                     processBasePath,
                                                                 string                   basePath,
                                                                 UxrComponentProcessor<T> componentProcessor,
                                                                 UxrProgressUpdater       progressUpdater,
                                                                 out bool                 canceled,
                                                                 bool                     ignoreUltimateXRAssets = false,
                                                                 bool                     onlyCheck              = false) where T : Component
        {
            bool processedAny = false;
            canceled = false;

            List<string> scenePrefabPaths = new List<string>();

            // Process open scenes if they are in the list

            if (scenePaths.Any())
            {
                List<Scene> openScenes = new List<Scene>();

                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    Scene scene = SceneManager.GetSceneAt(i);

                    if (scenePaths.Any(s => s == scene.path))
                    {
                        if (ProcessAllNonPrefabInstanceGameObjects(scene, componentProcessor, progressUpdater, out canceled, onlyCheck))
                        {
                            processedAny = true;
                        }

                        if (canceled)
                        {
                            return processedAny;
                        }

                        if (ProcessAllInnermostPrefabInstanceGameObjects(scene, basePath, ignoreUltimateXRAssets, componentProcessor, progressUpdater, out canceled, onlyCheck))
                        {
                            processedAny = true;
                        }

                        if (canceled)
                        {
                            return processedAny;
                        }

                        GetScenePrefabPaths<T>(scene, scenePrefabPaths);

                        if (canceled)
                        {
                            return processedAny;
                        }
                    }

                    openScenes.Add(scene);
                }

                // Process non-open scenes in list

                foreach (string scenePath in scenePaths)
                {
                    bool isSceneOpen = false;

                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        if (SceneManager.GetSceneAt(i).path == scenePath)
                        {
                            isSceneOpen = true;
                            break;
                        }
                    }

                    if (!isSceneOpen)
                    {
                        try
                        {
                            if (progressUpdater != null)
                            {
                                canceled = progressUpdater.Invoke(new UxrProgressInfo("Opening scene", scenePath, 0.0f));

                                if (canceled)
                                {
                                    return processedAny;
                                }
                            }

                            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                            if (ProcessAllNonPrefabInstanceGameObjects(scene, componentProcessor, progressUpdater, out canceled, onlyCheck))
                            {
                                processedAny = true;
                            }

                            if (canceled)
                            {
                                return processedAny;
                            }

                            if (ProcessAllInnermostPrefabInstanceGameObjects(scene, basePath, ignoreUltimateXRAssets, componentProcessor, progressUpdater, out canceled, onlyCheck))
                            {
                                processedAny = true;
                            }

                            if (canceled)
                            {
                                return processedAny;
                            }

                            GetScenePrefabPaths<T>(scene, scenePrefabPaths);

                            if (canceled)
                            {
                                return processedAny;
                            }

                            if (progressUpdater != null)
                            {
                                canceled = progressUpdater.Invoke(new UxrProgressInfo("Closing scene", scene.name, 1.0f));

                                if (canceled)
                                {
                                    return processedAny;
                                }
                            }

                            if (processedAny && !onlyCheck)
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
            }

            // Process prefabs in best order

            if (processBasePath || scenePrefabPaths.Any())
            {
                if (progressUpdater != null)
                {
                    canceled = progressUpdater.Invoke(new UxrProgressInfo("Processing prefabs", "Sorting prefabs", 0.0f));

                    if (canceled)
                    {
                        return processedAny;
                    }
                }

                processedAny = ProcessPrefabs(GetSortedPrefabProcessingPaths<T>(scenePrefabPaths, basePath, processBasePath, ignoreUltimateXRAssets), basePath, ignoreUltimateXRAssets, true, true, componentProcessor, progressUpdater, out canceled, onlyCheck);

                if (canceled)
                {
                    return processedAny;
                }
            }

            if (!onlyCheck)
            {
                AssetDatabase.SaveAssets();
                Resources.UnloadUnusedAssets();
            }

            return processedAny;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether a given path should be ignored based on the path and the ignore setting.
        /// </summary>
        /// <param name="assetPath">Asset path to check if it should be ignored</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <returns>Whether the given path should be ignored</returns>
        private static bool ShouldIgnoreUltimateXRPath(string assetPath, bool ignoreUltimateXRAssets)
        {
            return ignoreUltimateXRAssets && PathIsInUltimateXR(assetPath);
        }

        /// <summary>
        ///     Checks whether the given asset path requires processing.
        /// </summary>
        /// <param name="basePath">Base path to process. Null or empty to process all paths</param>
        /// <param name="assetPath">Asset path to check if it should be processed</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <returns>Whether <see cref="assetPath" /> should be processed</returns>
        private static bool PathRequiresProcessing(string basePath, string assetPath, bool ignoreUltimateXRAssets)
        {
            if (ShouldIgnoreUltimateXRPath(assetPath, ignoreUltimateXRAssets))
            {
                return false;
            }

            return string.IsNullOrEmpty(basePath) || assetPath.StartsWith(basePath);
        }

        /// <summary>
        ///     Gets a list of prefab paths that should be modified to process all components of a given type. The order ensures
        ///     that parent prefabs will always come first than the child prefabs. This is useful when processing components in
        ///     prefabs that depend on the parent prefab value, because each prefab will always have the updated value from the
        ///     parent when it's its turn to be processed.
        /// </summary>
        /// <param name="paths">Prefab paths to process</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Sorted prefab path list</returns>
        private static List<string> GetSortedPrefabProcessingPaths<T>(IEnumerable<string> paths, bool ignoreUltimateXRAssets) where T : Component
        {
            // Build prefab dependencies

            Dictionary<string, List<string>> assetDependencies = new Dictionary<string, List<string>>();

            foreach (string prefabPath in paths)
            {
                if (!ShouldIgnoreUltimateXRPath(prefabPath, ignoreUltimateXRAssets) && AssetDatabase.GetMainAssetTypeAtPath(prefabPath) == typeof(GameObject) && !assetDependencies.ContainsKey(prefabPath))
                {
                    Dictionary<string, int> dependencies = new Dictionary<string, int>(); // (path, count)
                    GameObject              gameObject   = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;

                    // Get all components of the given type in the prefab

                    T[] components = gameObject.GetComponentsInChildren<T>(true);

                    // Build dictionary where we list all asset paths with inner prefabs that this prefab depends on. 

                    foreach (T component in components)
                    {
                        if (component != null && GetInnermostNon3DModelPrefabRoot(component, out GameObject prefab, out GameObject prefabInstance, out T componentInPrefab))
                        {
                            if (prefab != gameObject)
                            {
                                // Not original source, it's coming from an inner prefab. Save dependency.

                                string assetPath = AssetDatabase.GetAssetPath(prefab);

                                if (!dependencies.ContainsKey(assetPath))
                                {
                                    dependencies.Add(assetPath, 1);
                                }
                                else
                                {
                                    dependencies[assetPath]++;
                                }
                            }
                        }
                    }

                    assetDependencies.Add(prefabPath, dependencies.Keys.ToList());
                }
            }

            // Recursive method to create the list

            void AddDependenciesAndAsset(string assetPath, Dictionary<string, List<string>> assetDependencies, ref List<string> sortedAssetList)
            {
                // Add all asset dependencies first

                if (assetDependencies.ContainsKey(assetPath))
                {
                    foreach (string path in assetDependencies[assetPath])
                    {
                        if (!sortedAssetList.Contains(path))
                        {
                            AddDependenciesAndAsset(path, assetDependencies, ref sortedAssetList);
                        }
                    }
                }

                // Add asset

                if (!sortedAssetList.Contains(assetPath))
                {
                    sortedAssetList.Add(assetPath);
                }
            }

            // Create the list using the recursive method

            List<string> sortedPathList = new List<string>();

            foreach (string prefabPath in assetDependencies.Keys)
            {
                AddDependenciesAndAsset(prefabPath, assetDependencies, ref sortedPathList);
            }

            return sortedPathList;
        }

        /// <summary>
        ///     Same as <see cref="GetSortedPrefabProcessingPaths{T}(System.Collections.Generic.IEnumerable{string})" /> but
        ///     specifying a list of paths and a base path in the project folder. This method will take care of not processing
        ///     duplicates if there are prefabs specified in <paramref name="additionalPaths" /> that are also in
        ///     <see cref="basePath" />.
        /// </summary>
        /// <param name="additionalPaths">Paths to process</param>
        /// <param name="basePath">Project base path to process</param>
        /// <param name="processBasePath">Whether to process all prefabs in <paramref name="basePath" /></param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Sorted prefab path list</returns>
        private static List<string> GetSortedPrefabProcessingPaths<T>(IEnumerable<string> additionalPaths, string basePath, bool processBasePath, bool ignoreUltimateXRAssets) where T : Component
        {
            // Filter required paths from all asset paths in project

            IEnumerable<string> GetPrefabsToProcess()
            {
                foreach (string prefabPath in additionalPaths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(prefabPath) == typeof(GameObject) && PathRequiresProcessing(basePath, prefabPath, ignoreUltimateXRAssets))
                    {
                        yield return prefabPath;
                    }
                }

                if (processBasePath)
                {
                    foreach (string prefabPath in GetAllAssetPathsExceptPackages())
                    {
                        if (AssetDatabase.GetMainAssetTypeAtPath(prefabPath) == typeof(GameObject) && PathRequiresProcessing(basePath, prefabPath, ignoreUltimateXRAssets) && !additionalPaths.Contains(prefabPath))
                        {
                            yield return prefabPath;
                        }
                    }
                }
            }

            return GetSortedPrefabProcessingPaths<T>(GetPrefabsToProcess(), ignoreUltimateXRAssets);
        }

        /// <summary>
        ///     Same as <see cref="GetSortedPrefabProcessingPaths{T}(System.Collections.Generic.IEnumerable{string})" /> but
        ///     specifying a base path in the project folder.
        /// </summary>
        /// <param name="basePath">Project base path to process</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore components in assets in UltimateXR folders</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Sorted prefab path list</returns>
        private static List<string> GetSortedPrefabProcessingPaths<T>(string basePath, bool ignoreUltimateXRAssets) where T : Component
        {
            // Filter required paths from all asset paths in project

            IEnumerable<string> GetPrefabsToProcess()
            {
                foreach (string prefabPath in GetAllAssetPathsExceptPackages())
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(prefabPath) == typeof(GameObject) && PathRequiresProcessing(basePath, prefabPath, ignoreUltimateXRAssets))
                    {
                        yield return prefabPath;
                    }
                }
            }

            return GetSortedPrefabProcessingPaths<T>(GetPrefabsToProcess(), ignoreUltimateXRAssets);
        }

        /// <summary>
        ///     Processes all prefabs in the given paths.
        /// </summary>
        /// <param name="prefabAssetPaths">List of prefab paths to process</param>
        /// <param name="basePath">The base path that was used to get the prefabs from the list</param>
        /// <param name="ignoreUltimateXRAssets">Whether the prefab list was created ignoring UltimateXR assets</param>
        /// <param name="processOriginalValues">
        ///     Whether to process components where they are in the original prefab that they were
        ///     added
        /// </param>
        /// <param name="processNonOriginalValues">
        ///     Whether to process components where they are not in the original prefab that
        ///     they were added, but inherited from another prefab instead
        /// </param>
        /// <param name="componentProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        ///     Here the GameObject that will be given as parameter is the GameObject that has the given component.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="onlyCheckt">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns>True if there was at least one component that was modified</returns>
        private static bool ProcessPrefabs<T>(List<string>             prefabAssetPaths,
                                              string                   basePath,
                                              bool                     ignoreUltimateXRAssets,
                                              bool                     processOriginalValues,
                                              bool                     processNonOriginalValues,
                                              UxrComponentProcessor<T> componentProcessor,
                                              UxrProgressUpdater       progressUpdater,
                                              out bool                 canceled,
                                              bool                     onlyCheck = false) where T : Component
        {
            bool processedAny = false;

            canceled = false;

            for (int i = 0; i < prefabAssetPaths.Count; ++i)
            {
                GameObject prefab    = AssetDatabase.LoadMainAssetAtPath(prefabAssetPaths[i]) as GameObject;
                bool       processed = false;

                if (progressUpdater != null)
                {
                    canceled = progressUpdater.Invoke(new UxrProgressInfo("Processing prefabs", $"Prefab {prefab.name}", (float)i / prefabAssetPaths.Count));

                    if (canceled)
                    {
                        return processedAny;
                    }
                }

                // Get all components of the given type in the prefab

                T[] components = prefab.GetComponentsInChildren<T>(true);

                foreach (T component in components)
                {
                    if (component != null && GetInnermostNon3DModelPrefabRoot(component, out GameObject originalPrefab, out GameObject prefabInstance, out T componentInPrefab))
                    {
                        // Only process those prefabs that are originally in this prefab and don't come from any prefab above in the hierarchy

                        bool isOriginalPrefab              = originalPrefab == prefab;
                        bool process                       = (isOriginalPrefab && processOriginalValues) || (!isOriginalPrefab && processNonOriginalValues);
                        bool isInnermostPrefabInValidChain = isOriginalPrefab;

                        T componentInParentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(component);

                        if (!isInnermostPrefabInValidChain && componentInParentPrefab != null && !PathRequiresProcessing(basePath, AssetDatabase.GetAssetPath(componentInParentPrefab), ignoreUltimateXRAssets))
                        {
                            isInnermostPrefabInValidChain = true;
                        }

                        if (process && componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(component, prefab, isOriginalPrefab, isInnermostPrefabInValidChain), onlyCheck))
                        {
                            processedAny = true;
                            processed    = true;
                        }
                    }
                }

                if (processed && !onlyCheck)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }

            return processedAny;
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
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="onlyCheckt">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <returns>True if there was at least one component that was modified</returns>
        /// <typeparam name="T">The component type</typeparam>
        private static bool ProcessAllNonPrefabInstanceGameObjects<T>(Scene                    scene,
                                                                      UxrComponentProcessor<T> componentProcessor,
                                                                      UxrProgressUpdater       progressUpdater,
                                                                      out bool                 canceled,
                                                                      bool                     onlyCheck = false) where T : Component
        {
            List<T> components   = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>()).ToList();
            bool    processedAny = false;

            canceled = false;

            for (int c = 0; c < components.Count; ++c)
            {
                if (progressUpdater != null)
                {
                    canceled = progressUpdater.Invoke(new UxrProgressInfo($"Processing scene {scene.name}", $"Object {components[c].name}", (float)c / components.Count));

                    if (canceled)
                    {
                        return processedAny;
                    }
                }

                bool isInstantiatedPrefab = GetInnermostNon3DModelPrefabRoot(components[c], out GameObject prefab, out GameObject prefabInstance, out T componentInPrefab);

                // Only process if it's not an instantiated prefab

                if (!isInstantiatedPrefab && componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(components[c], null, true, true), onlyCheck))
                {
                    processedAny = true;
                }
            }

            return processedAny;
        }

        /// <summary>
        ///     Processes all components of a given type in a scene only in prefab instances whose source prefabs lie outside a
        ///     specified project base path.
        ///     In this case, in <see cref="UxrComponentInfo{T}" /> the
        ///     <see cref="UxrComponentInfo{T}.IsInnermostInValidChain" /> will be marked as true.
        ///     This is useful in component processors that target the innermost prefab only, but when the user specifies a base
        ///     path that prevents the real innermost prefab from being processed.
        /// </summary>
        /// <typeparam name="T">Component type to process</typeparam>
        /// <param name="scene">Scene to process</param>
        /// <param name="basePath">
        ///     Project base path to process. This will be used to check whether the prefab instances in the
        ///     scene should be processed. The instances will be processed if their source prefab is not located in the base path
        /// </param>
        /// <param name="ignoreUltimateXRAssets">
        ///     Whether UltimateXR folders are being ignored. In a similar way to
        ///     <paramref name="basePath" />, if UltimateXR folders are being ignored, instances will be processed if their source
        ///     prefab is from UltimateXR directly
        /// </param>
        /// <param name="componentProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        ///     Here the GameObject that will be given as parameter is the GameObject that has the given component.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="onlyCheckt">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <returns>True if there was at least one component that was modified</returns>
        /// <typeparam name="T">The component type</typeparam>
        private static bool ProcessAllInnermostPrefabInstanceGameObjects<T>(Scene                    scene,
                                                                            string                   basePath,
                                                                            bool                     ignoreUltimateXRAssets,
                                                                            UxrComponentProcessor<T> componentProcessor,
                                                                            UxrProgressUpdater       progressUpdater,
                                                                            out bool                 canceled,
                                                                            bool                     onlyCheck = false) where T : Component
        {
            List<T> components   = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>()).ToList();
            bool    processedAny = false;

            canceled = false;

            for (int c = 0; c < components.Count; ++c)
            {
                if (progressUpdater != null)
                {
                    canceled = progressUpdater.Invoke(new UxrProgressInfo($"Processing scene {scene.name}", $"Object {components[c].name}", (float)c / components.Count));

                    if (canceled)
                    {
                        return processedAny;
                    }
                }

                bool isInstantiatedPrefab = GetInnermostNon3DModelPrefabRoot(components[c], out GameObject prefab, out GameObject prefabInstance, out T componentInPrefab);

                if (isInstantiatedPrefab)
                {
                    T componentInParentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(components[c]);

                    // Only process if it's an instantiated prefab whose prefab doesn't meet the path requirements

                    if (componentInParentPrefab != null && !PathRequiresProcessing(basePath, AssetDatabase.GetAssetPath(componentInParentPrefab), ignoreUltimateXRAssets))
                    {
                        if (componentProcessor != null && componentProcessor.Invoke(new UxrComponentInfo<T>(components[c], null, false, true), onlyCheck))
                        {
                            processedAny = true;
                        }
                    }
                }
            }

            return processedAny;
        }

        /// <summary>
        ///     Gets all the prefab paths from a scene that contain components of the given type.
        /// </summary>
        /// <param name="scene">Scene to process</param>
        /// <param name="paths">List where to add the prefab paths found</param>
        /// <typeparam name="T">Component type</typeparam>
        private static void GetScenePrefabPaths<T>(Scene scene, List<string> paths) where T : Component
        {
            List<T> components = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>()).ToList();

            foreach (T c in components)
            {
                T componentInPrefab = PrefabUtility.GetCorrespondingObjectFromSource(c);

                while (componentInPrefab != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(componentInPrefab);

                    if (!string.IsNullOrEmpty(assetPath) &&
                        (PrefabUtility.GetPrefabAssetType(componentInPrefab) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(componentInPrefab) == PrefabAssetType.Variant) &&
                        !paths.Contains(assetPath))
                    {
                        paths.Add(assetPath);
                    }

                    componentInPrefab = PrefabUtility.GetCorrespondingObjectFromSource(componentInPrefab);
                }
            }
        }

        /// <summary>
        ///     Same as AssetDatabase.GetAllAssetPaths() but filtering out assets from packages.
        /// </summary>
        /// <returns>List of asset paths filtering out assets from packages</returns>
        private static string[] GetAllAssetPathsExceptPackages()
        {
            return AssetDatabase.GetAllAssetPaths().Where(p => !p.StartsWith("Packages/")).ToArray();
        }

        #endregion
    }
}