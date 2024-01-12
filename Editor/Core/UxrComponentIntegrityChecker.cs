// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentIntegrityChecker.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltimateXR.Editor.Core
{
    /// <summary>
    ///     Class that hooks to Unity scene saving to detect inconsistencies in unique IDs of <see cref="UxrComponent" />
    ///     components.
    /// </summary>
    public sealed class UxrComponentIntegrityChecker
    {
        #region Public Methods

        /// <summary>
        ///     Method called when Unity loads. It will hook to the scene saving event.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void HookToSceneSave()
        {
            EditorSceneManager.sceneSaving -= EditorSceneManager_sceneSaving;
            EditorSceneManager.sceneSaving += EditorSceneManager_sceneSaving;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called by Unity when the scene is saved.
        /// </summary>
        /// <param name="scene">Scene to be saved</param>
        /// <param name="path">Path to save to</param>
        private static void EditorSceneManager_sceneSaving(Scene scene, string path)
        {
            // Momentarily don't perform automatic ID generation, since changes might call UxrComponent.OnValidate() which is where automatic ID generation is done.
            EditorPrefs.SetBool(UxrConstants.Editor.AutomaticIdGenerationPrefs, false);

            Dictionary<string, UxrComponent> sceneUxrComponents = new Dictionary<string, UxrComponent>();

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                UxrComponent[] components = rootGameObject.GetComponentsInChildren<UxrComponent>(true);

                foreach (UxrComponent component in components)
                {
                    int attemptCount = 0;

                    while (string.IsNullOrEmpty(component.UniqueId) || sceneUxrComponents.ContainsKey(component.UniqueId))
                    {
                        // ID is either missing (shouldn't happen) or duplicated (by duplicating a GameObject in the scene). Generate new ID.
                        component.ChangeUniqueId(UxrComponent.GetNewUniqueId());

                        attemptCount++;

                        if (attemptCount > MaxAttempts)
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(component.UniqueId) && !sceneUxrComponents.ContainsKey(component.UniqueId))
                    {
                        sceneUxrComponents.Add(component.UniqueId, component);
                    }
                }
            }

            // Re-enable automatic ID generation.
            EditorPrefs.SetBool(UxrConstants.Editor.AutomaticIdGenerationPrefs, true);
        }

        #endregion

        #region Private Types & Data

        private const int MaxAttempts = 100;

        #endregion
    }
}