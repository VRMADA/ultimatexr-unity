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
        public static void ProcessAllScriptableObjects<T>(string basePath, UxrScriptableObjectProcessor<T> scriptableObjectProcessor, UxrProgressUpdater progressUpdater) where T : ScriptableObject
        {
            // Get all asset files and process them

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            for (int i = 0; i < allAssetPaths.Length; ++i)
            {
                string assetPath = allAssetPaths[i];

                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(T) && PathRequiresProcessing(basePath, assetPath))
                {
                    T assetObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                    if (assetObject)
                    {
                        progressUpdater?.Invoke(new UxrProgressInfo($"Processing {typeof(T).Name} assets", $"Asset {assetObject.name}", (float)i / allAssetPaths.Length));
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