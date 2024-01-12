// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Paths.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Types & Data

        public static readonly string ApplicationProjectPath = Path.GetFullPath(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length));

#if ULTIMATEXR_PACKAGE
        public static readonly string RelativeInstallationPath = $"Packages/{UxrConstants.PackageName}/";
        public static readonly string FullInstallationPath = $"{Path.GetFullPath(RelativeInstallationPath)}";
        public static readonly string InstalledSamplesPath = $"{Application.dataPath}/Samples/{UxrConstants.UltimateXR}/";
#else
        public static string RelativeInstallationPath => GetProjectRelativePath(FullInstallationPath);

        public static string FullInstallationPath
        {
            get
            {
                // If we cached it, return it immediately

                if (!string.IsNullOrEmpty(CachedFullInstallationPath))
                {
                    return CachedFullInstallationPath;
                }

                // Try to infer installation path using a well known source file. This way when UltimateXR is installed
                // in the Assets/ folder it can be moved around and doesn't require to be in a fixed installation path. 

                Assembly assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(a => a.name == UxrConstants.UltimateXR);

                if (assembly == null)
                {
                    return DefaultFullInstallationPath;
                }

                string knownFileName = $"{nameof(UxrComponent)}.cs";
                string knownFilePath = assembly.sourceFiles.FirstOrDefault(f => f.EndsWith(knownFileName));

                if (string.IsNullOrEmpty(knownFilePath))
                {
                    return DefaultFullInstallationPath;
                }

                CachedFullInstallationPath = Path.GetFullPath(PathExt.Combine(ApplicationProjectPath, PathExt.Combine(Path.GetDirectoryName(knownFilePath), @"..\..\..")));

                return CachedFullInstallationPath;
            }
        }
#endif

        public static string HandPosePresetsPath => PathExt.Combine(FullInstallationPath, "Editor/Manipulation/HandPoses/HandPosePresets");

        #endregion

        #region Public Methods

        /// <summary>
        ///     Returns whether an asset can be loaded using <see cref="AssetDatabase.LoadAssetAtPath" />
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
            if (PathExt.IsSubDirectoryOf(path, FullInstallationPath))
            {
                return true;
            }

#endif

            return PathExt.IsSubDirectoryOf(path, Application.dataPath);
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

            return PathExt.IsSubDirectoryOf(path, Application.dataPath);
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
            if (PathExt.IsSubDirectoryOf(path, InstalledSamplesPath))
            {
                return true;
            }
#endif

            return PathExt.IsSubDirectoryOf(path, FullInstallationPath);
        }

        /// <summary>
        ///     Transforms a full path to a path relative to the current project folder.
        /// </summary>
        /// <param name="path">Full path</param>
        /// <returns>Path relative to the current project folder</returns>
        /// <exception cref="DirectoryNotFoundException">The given path does not belong to the current project.</exception>
        public static string GetProjectRelativePath(string path)
        {
            if (!PathExt.IsSubDirectoryOf(path, Application.dataPath))
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
            if (PathExt.IsSubDirectoryOf(path, FullInstallationPath))
            {
                path = $"Packages/{UxrConstants.PackageName}/{path.Substring(FullInstallationPath.Length)}";
            }
            else
#endif
            if (PathExt.IsSubDirectoryOf(path, Application.dataPath))
            {
                path = $"Assets{path.Substring(Application.dataPath.Length)}";
            }

            return path;
        }

        /// <summary>
        ///     Opens a folder selection dialog and returns the selected path or <see cref="string.Empty" /> if cancelled or the
        ///     folder was not in the project.
        ///     If the folder was not in the project it will take care of displaying an error message.
        /// </summary>
        /// <param name="relativeProjectPath">The path relative to the project folder. It will start with Assets</param>
        /// <param name="title">title parameter of <see cref="EditorUtility.OpenFolderPanel" /></param>
        /// <param name="folder">folder parameter of <see cref="EditorUtility.OpenFolderPanel" /></param>
        /// <param name="defaultName">defaultName parameter of <see cref="EditorUtility.OpenFolderPanel" /></param>
        /// <returns>Whether a valid path was returned/</returns>
        public static bool OpenFolderPanel(out string relativeProjectPath, string title = "Select folder", string folder = "", string defaultName = "")
        {
            relativeProjectPath = string.Empty;
            string absolutePath = EditorUtility.OpenFolderPanel("Select folder", folder, defaultName);

            if (!string.IsNullOrEmpty(absolutePath))
            {
                try
                {
                    relativeProjectPath = GetProjectRelativePath(absolutePath);
                    return true;
                }
                catch (DirectoryNotFoundException)
                {
                    ShowFolderNotInProjectError();
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Private Types & Data

#if !ULTIMATEXR_PACKAGE
        private static readonly string DefaultFullInstallationPath = $"{Application.dataPath}/{UxrConstants.UltimateXR}/";
        private static          string CachedFullInstallationPath;
#endif

        #endregion
    }
}