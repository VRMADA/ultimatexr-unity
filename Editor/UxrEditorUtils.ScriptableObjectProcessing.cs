// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.ScriptableObjectProcessing.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Processes all scriptable objects in a project.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="ScriptableObject" /> to process</typeparam>
        /// <param name="basePath">
        ///     Base path of assets to process. Use null or empty to process the whole project. If using a base path, it should
        ///     start with Assets/
        /// </param>
        /// <param name="scriptableObjectProcessor">
        ///     The component processor. It will receive the component to process as argument and it requires to return a boolean
        ///     telling whether the component was modified or not.
        /// </param>
        /// <param name="progressUpdater">
        ///     Will receive updates of the process so that the information can be fed to a Unity progress bar
        /// </param>
        /// <param name="canceled">Returns whether the user canceled the operation using the progress updater</param>
        /// <param name="ignoreUltimateXRAssets">Whether to ignore assets in UltimateXR folders</param>
        public static void ProcessAllScriptableObjects<T>(string                          basePath,
                                                          UxrScriptableObjectProcessor<T> scriptableObjectProcessor,
                                                          UxrProgressUpdater              progressUpdater,
                                                          out bool                        canceled,
                                                          bool                            ignoreUltimateXRAssets) where T : ScriptableObject
        {
            // Get all asset files and process them

            string[] allAssetPaths = GetAllAssetPathsExceptPackages();

            canceled = false;

            for (int i = 0; i < allAssetPaths.Length; ++i)
            {
                string assetPath = allAssetPaths[i];

                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(T) && PathRequiresProcessing(basePath, assetPath, ignoreUltimateXRAssets))
                {
                    T assetObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                    if (assetObject)
                    {
                        if (progressUpdater != null)
                        {
                            canceled = progressUpdater.Invoke(new UxrProgressInfo($"Processing {typeof(T).Name} assets", $"Asset {assetObject.name}", (float)i / allAssetPaths.Length));

                            if (canceled)
                            {
                                return;
                            }
                        }

                        scriptableObjectProcessor?.Invoke(assetObject);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Resources.UnloadUnusedAssets();
        }

        #endregion
    }
}