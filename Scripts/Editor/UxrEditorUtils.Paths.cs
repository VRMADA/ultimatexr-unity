// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Paths.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using UltimateXR.Core;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

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
        ///     Gets the base path where UltimateXR is installed.
        /// </summary>
        /// <returns>Base path</returns>
        public static string GetUltimateXRInstallationPath()
        {
            return Path.Combine(Application.dataPath, UxrConstants.Paths.Base);
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

            return path.IsSubDirectoryOf(GetUltimateXRInstallationPath());
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
                throw new DirectoryNotFoundException($"{nameof(GetProjectRelativePath)}: Path {path} needs to belong to current project");
            }

            return path.Substring(Application.dataPath.Length - "Assets".Length);
        }

        #endregion
    }
}