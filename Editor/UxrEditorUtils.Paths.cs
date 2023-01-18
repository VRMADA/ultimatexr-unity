// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Paths.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using UltimateXR.Core;
using UltimateXR.Extensions.System;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
#if ULTIMATEXR_PACKAGE
        public static readonly string RelativeInstallationPath = $"Packages/{UxrConstants.PackageName}/";
        public static readonly string FullInstallationPath     = $"{Path.GetFullPath(RelativeInstallationPath)}";
        public static readonly string InstalledSamplesPath     = $"{Application.dataPath}/Samples/{UxrConstants.UltimateXR}/";
#else
        public static readonly string RelativeInstallationPath = $"Assets/{UxrConstants.UltimateXR}/";
        public static readonly string FullInstallationPath     = $"{Application.dataPath}/{UxrConstants.UltimateXR}/";
#endif

        public static readonly string HandPosePresetsPath = $"{FullInstallationPath}Editor/Manipulation/HandPoses/HandPosePresets";

        #region Public Methods

        /// <summary>
        ///     Returns whether an asset can be loaded using <see cref="AssetDatabase.LoadAssetAtPath"/>
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Boolean telling whether the given asset can be loaded</returns>
        public static bool CanLoadUsingAssetDatabase(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

#if ULTIMATEXR_PACKAGE

            if (path.IsSubDirectoryOf(FullInstallationPath))
            {
                return true;
            }

#endif

            return path.IsSubDirectoryOf(Application.dataPath);
        }

        /// <summary>
        ///     Returns whether the given path is in the current project.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Boolean telling whether the given path is in the current project</returns>
        public static bool PathIsInCurrentProject(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.IsSubDirectoryOf(Application.dataPath);
        }

        /// <summary>
        ///     Returns whether the given path is inside the UltimateXR framework.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Boolean telling whether the given path is inside the UltimateXR framework</returns>
        public static bool PathIsInUltimateXR(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            
#if ULTIMATEXR_PACKAGE
            if (path.IsSubDirectoryOf(InstalledSamplesPath))
            {
                return true;
            }
#endif

            return path.IsSubDirectoryOf(FullInstallationPath);
        }

        /// <summary>
        ///     Transforms a full path to a path relative to the current project folder.
        /// </summary>
        /// <param name="path">Full path</param>
        /// <returns>Path relative to the current project folder</returns>
        /// <exception cref="DirectoryNotFoundException">The given path does not belong to the current project.</exception>
        public static string GetProjectRelativePath(string path)
        {
            if (!path.IsSubDirectoryOf(Application.dataPath))
            {
                throw new DirectoryNotFoundException($"{nameof(GetProjectRelativePath)}: Path {path} needs to be under current project's Assets folder");
            }

            return path.Substring(Application.dataPath.Length - "Assets".Length);
        }

        /// <summary>
        ///     Gets a list of the files inside the hand pose presets folder
        /// </summary>
        /// <returns>Hand pose preset file names</returns>
        public static string[] GetHandPosePresetFiles()
        {
            string[] files = Directory.GetFiles(HandPosePresetsPath);

#if ULTIMATEXR_PACKAGE
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = $"Packages/{UxrConstants.PackageName}/{files[i].Substring(FullInstallationPath.Length)}";
            }
#else
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = $"Assets{files[i].Substring(Application.dataPath.Length)}";
            }
#endif

            return files;
        }

        /// <summary>
        ///     Transforms a HandPoseAsset file path to a relative path that can be used to load an asset through
        ///     <see cref="AssetDatabase.LoadAssetAtPath{T}" />.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Path compatible with <see cref="AssetDatabase" /></returns>
        public static string ToHandPoseAssetPath(string path)
        {
#if ULTIMATEXR_PACKAGE
            if (path.IsSubDirectoryOf(FullInstallationPath))
            {
                path = $"Packages/{UxrConstants.PackageName}/{path.Substring(FullInstallationPath.Length)}";
            }
            else
#endif
            if (path.IsSubDirectoryOf(Application.dataPath))
            {
                path = $"Assets{path.Substring(Application.dataPath.Length)}";
            }

            return path;
        }

        #endregion
    }
}